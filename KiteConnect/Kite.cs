using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Collections;
using Microsoft.VisualBasic.FileIO;


namespace KiteConnect
{
    /// <summary>
    /// The API client class. In production, you may initialize a single instance of this class per `APIKey`.
    /// </summary>
    public class Kite
    {
        // Default root API endpoint. It's possible to
        // override this by passing the `Root` parameter during initialisation.
        private string _root = "https://api.kite.trade";
        private string _login = "https://kite.trade/connect/login";

        private string _apiKey;
        private string _accessToken;
        private bool _debug;
        private WebProxy _proxy;
        private int _timeout;

        private Action _sessionHook;

        //private Cache cache = new Cache();

        private readonly Dictionary<string, string> _routes = new Dictionary<string, string>
        {
            ["parameters"] = "/parameters",
            ["api.validate"] = "/session/token",
            ["api.invalidate"] = "/session/token",
            ["user.margins"] = "/user/margins/{segment}",

            ["orders"] = "/orders",
            ["trades"] = "/trades",
            ["orders.info"] = "/orders/{order_id}",

            ["orders.place"] = "/orders/{variety}",
            ["orders.modify"] = "/orders/{variety}/{order_id}",
            ["orders.cancel"] = "/orders/{variety}/{order_id}",
            ["orders.trades"] = "/orders/{order_id}/trades",

            ["portfolio.positions"] = "/portfolio/positions",
            ["portfolio.holdings"] = "/portfolio/holdings",
            ["portfolio.positions.modify"] = "/portfolio/positions",

            ["market.instruments.all"] = "/instruments",
            ["market.instruments"] = "/instruments/{exchange}",
            ["market.quote"] = "/instruments/{exchange}/{tradingsymbol}",
            ["market.ohlc"] = "/quote/ohlc",
            ["market.ltp"] = "/quote/ltp",
            ["market.historical"] = "/instruments/historical/{instrument_token}/{interval}",
            ["market.trigger_range"] = "/instruments/{exchange}/{tradingsymbol}/trigger_range",

            ["mutualfunds.orders"] = "/mf/orders",
            ["mutualfunds.order"] = "/mf/orders/{order_id}",
            ["mutualfunds.orders.place"] = "/mf/orders",
            ["mutualfunds.cancel_order"] = "/mf/orders/{order_id}",

            ["mutualfunds.sips"] = "/mf/sips",
            ["mutualfunds.sips.place"] = "/mf/sips",
            ["mutualfunds.cancel_sips"] = "/mf/sips/{sip_id}",
            ["mutualfunds.sips.modify"] = "/mf/sips/{sip_id}",
            ["mutualfunds.sip"] = "/mf/sips/{sip_id}",

            ["mutualfunds.instruments"] = "/mf/instruments",
            ["mutualfunds.holdings"] = "/mf/holdings"
        };

        /// <summary>
        /// Initialize a new Kite Connect client instance.
        /// </summary>
        /// <param name="APIKey">API Key issued to you</param>
        /// <param name="AccessToken">The token obtained after the login flow in exchange for the `RequestToken` . 
        /// Pre-login, this will default to None,but once you have obtained it, you should persist it in a database or session to pass 
        /// to the Kite Connect class initialisation for subsequent requests.</param>
        /// <param name="Root">API end point root. Unless you explicitly want to send API requests to a non-default endpoint, this can be ignored.</param>
        /// <param name="Debug">If set to True, will serialise and print requests and responses to stdout.</param>
        /// <param name="Timeout">Time in milliseconds for which  the API client will wait for a request to complete before it fails</param>
        /// <param name="Proxy">To set proxy for http request. Should be an object of WebProxy.</param>
        /// <param name="Pool">Number of connections to server. Client will reuse the connections if they are alive.</param>
        public Kite(string APIKey, string AccessToken = null, string Root = null, bool Debug = false, int Timeout = 7000, WebProxy Proxy = null, int Pool = 2)
        {
            _accessToken = AccessToken;
            _apiKey = APIKey;
            if (!String.IsNullOrEmpty(Root)) this._root = Root;
            _debug = Debug;

            _timeout = Timeout;
            _proxy = Proxy;

            ServicePointManager.DefaultConnectionLimit = Pool;
        }

        /// <summary>
        /// Set a callback hook for session (`TokenError` -- timeout, expiry etc.) errors.
		/// An `AccessToken` (login session) can become invalid for a number of
        /// reasons, but it doesn't make sense for the client to
		/// try and catch it during every API call.
        /// A callback method that handles session errors
        /// can be set here and when the client encounters
        /// a token error at any point, it'll be called.
        /// This callback, for instance, can log the user out of the UI,
		/// clear session cookies, or initiate a fresh login.
        /// </summary>
        /// <param name="Method">Action to be invoked when session becomes invalid.</param>
        public void SetSessionHook(Action Method)
        {
            _sessionHook = Method;
        }

        /// <summary>
        /// Set the `AccessToken` received after a successful authentication.
        /// </summary>
        /// <param name="AccessToken">Access token for the session.</param>
        public void SetAccessToken(string AccessToken)
        {
            this._accessToken = AccessToken;
        }

        /// <summary>
        /// Get the remote login url to which a user should be redirected to initiate the login flow.
        /// </summary>
        /// <returns>Login url to authenticate the user.</returns>
        public string GetLoginURL()
        {
            return String.Format("{0}?api_key={1}", this._login, this._apiKey);
        }

        /// <summary>
        /// Do the token exchange with the `RequestToken` obtained after the login flow,
		/// and retrieve the `AccessToken` required for all subsequent requests.The
        /// response contains not just the `AccessToken`, but metadata for
        /// the user who has authenticated.
        /// </summary>
        /// <param name="RequestToken">Token obtained from the GET paramers after a successful login redirect.</param>
        /// <param name="AppSecret">API secret issued with the API key.</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public User RequestAccessToken(string RequestToken, string AppSecret)
        {
            string checksum = SHA256(this._apiKey + RequestToken + AppSecret);

            var param = new Dictionary<string, dynamic>
            {
                {"request_token", RequestToken},
                {"checksum", checksum}
            };

            var userdata = Post("api.validate", param);

            return new User(userdata);
        }

        /// <summary>
        /// Kill the session by invalidating the access token
        /// </summary>
        /// <param name="AccessToken">Access token to invalidate. Default is the active access token.</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> InvalidateToken(string AccessToken = null)
        {
            var param = new Dictionary<string, dynamic>();

            AddIfNotNull(param, "access_token", AccessToken);

            return Delete("api.invalidate", param);
        }

        /// <summary>
        /// Get account balance and cash margin details for a particular segment.
        /// </summary>
        /// <param name="Segment">Trading segment (eg: equity or commodity)</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> Margins(string Segment)
        {
            return Get("user.margins", new Dictionary<string, dynamic> { { "segment", Segment } });
        }

        /// <summary>
        /// Place an order
        /// </summary>
        /// <param name="Exchange">Name of the exchange</param>
        /// <param name="TradingSymbol">Tradingsymbol of the instrument</param>
        /// <param name="TransactionType">BUY or SELL</param>
        /// <param name="Quantity">Quantity to transact</param>
        /// <param name="Price">For LIMIT orders</param>
        /// <param name="Product">Margin product applied to the order (margin is blocked based on this)</param>
        /// <param name="OrderType">Order type (MARKET, LIMIT etc.)</param>
        /// <param name="Validity">Order validity</param>
        /// <param name="DisclosedQuantity">Quantity to disclose publicly (for equity trades)</param>
        /// <param name="TriggerPrice">For SL, SL-M etc.</param>
        /// <param name="SquareOffValue">Price difference at which the order should be squared off and profit booked (eg: Order price is 100. Profit target is 102. So squareoff_value = 2)</param>
        /// <param name="StoplossValue">Stoploss difference at which the order should be squared off (eg: Order price is 100. Stoploss target is 98. So stoploss_value = 2)</param>
        /// <param name="TrailingStoploss">Incremental value by which stoploss price changes when market moves in your favor by the same incremental value from the entry price (optional)</param>
        /// <param name="Variety">You can place orders of varieties; regular orders, after market orders, cover orders etc. </param>
        /// <param name="Tag">An optional tag to apply to an order to identify it (alphanumeric, max 8 chars)</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> PlaceOrder(
            string Exchange, 
            string TradingSymbol, 
            string TransactionType,
            string Quantity,
            string Price = null, 
            string Product = null,
            string OrderType = null, 
            string Validity = null,            
            string DisclosedQuantity = null, 
            string TriggerPrice = null,
            string SquareOffValue = null,
            string StoplossValue = null,
            string TrailingStoploss = null,
            string Variety = "regular",
            string Tag = "")
        {
            var param = new Dictionary<string, dynamic>();

            AddIfNotNull(param, "exchange", Exchange);
            AddIfNotNull(param, "tradingsymbol", TradingSymbol);
            AddIfNotNull(param, "transaction_type", TransactionType);
            AddIfNotNull(param, "quantity", Quantity);
            AddIfNotNull(param, "price", Price);
            AddIfNotNull(param, "product", Product);
            AddIfNotNull(param, "order_type", OrderType);
            AddIfNotNull(param, "validity", Validity);
            AddIfNotNull(param, "disclosed_quantity", DisclosedQuantity);
            AddIfNotNull(param, "trigger_price", TriggerPrice);
            AddIfNotNull(param, "squareoff_value", SquareOffValue);
            AddIfNotNull(param, "stoploss_value", StoplossValue);
            AddIfNotNull(param, "trailing_stoploss", TrailingStoploss);
            AddIfNotNull(param, "variety", Variety);
            AddIfNotNull(param, "tag", Tag);

            return Post("orders.place", param);
        }

        /// <summary>
        /// Modify an open order.
        /// </summary>
        /// <param name="OrderId">Id of the order to be modified</param>
        /// <param name="ParentOrderId">Id of the parent order (obtained from the /orders call) as BO is a multi-legged order</param>
        /// <param name="Exchange">Name of the exchange</param>
        /// <param name="TradingSymbol">Tradingsymbol of the instrument</param>
        /// <param name="TransactionType">BUY or SELL</param>
        /// <param name="Quantity">Quantity to transact</param>
        /// <param name="Price">For LIMIT orders</param>
        /// <param name="Product">Margin product applied to the order (margin is blocked based on this)</param>
        /// <param name="OrderType">Order type (MARKET, LIMIT etc.)</param>
        /// <param name="Validity">Order validity</param>
        /// <param name="DisclosedQuantity">Quantity to disclose publicly (for equity trades)</param>
        /// <param name="TriggerPrice">For SL, SL-M etc.</param>
        /// <param name="Variety">You can place orders of varieties; regular orders, after market orders, cover orders etc. </param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> ModifyOrder(
            string OrderId,
            string ParentOrderId = null,
            string Exchange = null,
            string TradingSymbol = null,
            string TransactionType = null,
            string Quantity = null,
            string Price = null,
            string Product = null,
            string OrderType = null,
            string Validity = "DAY",
            string DisclosedQuantity = "0",
            string TriggerPrice = "0",
            string Variety = "regular")
        {
            var param = new Dictionary<string, dynamic>();

            if ((Product.ToLower() == "bo" || Product.ToLower() == "co") && Variety.ToLower() != Product.ToLower())           
                throw new Exception(String.Format("Invalid variety. It should be: {}", Product.ToLower()));

            AddIfNotNull(param, "order_id", OrderId);
            AddIfNotNull(param, "parent_order_id", ParentOrderId);
            AddIfNotNull(param, "trigger_price", TriggerPrice);
            AddIfNotNull(param, "variety", Variety);

            if (Variety.ToLower() == "bo" && Product.ToLower() == "bo")
            {
                AddIfNotNull(param, "quantity", Quantity);
                AddIfNotNull(param, "price", Price);
                AddIfNotNull(param, "disclosed_quantity", DisclosedQuantity);
            }
            else if(Variety.ToLower() != "co" && Product.ToLower() != "co")
            {
                AddIfNotNull(param, "exchange", Exchange);
                AddIfNotNull(param, "tradingsymbol", TradingSymbol);
                AddIfNotNull(param, "transaction_type", TransactionType);
                AddIfNotNull(param, "quantity", Quantity);
                AddIfNotNull(param, "price", Price);
                AddIfNotNull(param, "product", Product);
                AddIfNotNull(param, "order_type", OrderType);
                AddIfNotNull(param, "validity", Validity);
                AddIfNotNull(param, "disclosed_quantity", DisclosedQuantity);
            }

            return Put("orders.modify", param);
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        /// <param name="OrderId">Id of the order to be cancelled</param>
        /// <param name="Variety">You can place orders of varieties; regular orders, after market orders, cover orders etc. </param>
        /// <param name="ParentOrderId">Id of the parent order (obtained from the /orders call) as BO is a multi-legged order</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> CancelOrder(string OrderId, string Variety = "regular", string ParentOrderId = null)
        {
            var param = new Dictionary<string, dynamic>();

            AddIfNotNull(param, "order_id", OrderId);
            AddIfNotNull(param, "parent_order_id", ParentOrderId);
            AddIfNotNull(param, "variety", Variety);

            return Delete("orders.cancel", param);
        }

        /// <summary>
        /// Gets the collection of orders from the orderbook.
        /// </summary>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public List<Order> GetOrders()
        {
            var ordersdata = Get("orders");

            List<Order> orders = new List<Order>();

            foreach (Dictionary<string, dynamic> item in ordersdata["data"])
                orders.Add(new Order(item));

            return orders;
        }

        /// <summary>
        /// Gets information about given OrderId.
        /// </summary>
        /// <param name="OrderId">Unique order id</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public List<OrderInfo> GetOrder(string OrderId)
        {
            var param = new Dictionary<string, dynamic>();
            param.Add("order_id", OrderId);

            var orderdata = Get("orders.info", param);

            List<OrderInfo> orderinfo = new List<OrderInfo>();

            foreach (Dictionary<string, dynamic> item in orderdata["data"])
                orderinfo.Add(new OrderInfo(item));

            return orderinfo;
        }

        /// <summary>
        /// Retreive the list of trades executed (all or ones under a particular order).
        /// An order can be executed in tranches based on market conditions.
        /// These trades are individually recorded under an order.
        /// </summary>
        /// <param name="OrderId">is the ID of the order (optional) whose trades are to be retrieved. If no `order_id` is specified, all trades for the day are returned.</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public List<Trade> GetTrades(string OrderId = null)
        {
            Dictionary<string, dynamic> tradesdata;
            if (!String.IsNullOrEmpty(OrderId))
            {
                var param = new Dictionary<string, dynamic>();
                param.Add("order_id", OrderId);
                tradesdata = Get("orders.trades", param);
            }
            else
                tradesdata = Get("trades");

            List<Trade> trades = new List<Trade>();

            foreach (Dictionary<string, dynamic> item in tradesdata["data"])
                trades.Add(new Trade(item));

            return trades;
        }

        /// <summary>
        /// Retrieve the list of positions.
        /// </summary>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public PositionResponse GetPositions()
        {
            var positionsdata = Get("portfolio.positions");

            List<Position> daypositions = new List<Position>();
            List<Position> netpositions = new List<Position>();

            foreach (Dictionary<string, dynamic> item in positionsdata["data"]["day"])
                daypositions.Add(new Position(item));
            foreach (Dictionary<string, dynamic> item in positionsdata["data"]["net"])
                netpositions.Add(new Position(item));

            PositionResponse response = new PositionResponse(daypositions, netpositions);

            return response;
        }

        /// <summary>
        /// Retrieve the list of equity holdings.
        /// </summary>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public List<Holding> GetHoldings()
        {
            var holdingsdata = Get("portfolio.holdings");

            List<Holding> holdings = new List<Holding>();

            foreach (Dictionary<string, dynamic> item in holdingsdata["data"])
                holdings.Add(new Holding(item));

            return holdings;
        }

        /// <summary>
        /// Modify an open position's product type.
        /// </summary>
        /// <param name="Exchange">Name of the exchange</param>
        /// <param name="TradingSymbol">Tradingsymbol of the instrument</param>
        /// <param name="TransactionType">BUY or SELL</param>
        /// <param name="PositionType">overnight or day</param>
        /// <param name="Quantity">Quantity to convert</param>
        /// <param name="OldProduct">Existing margin product of the position</param>
        /// <param name="NewProduct">Margin product to convert to</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> ModifyProduct(
            string Exchange,
            string TradingSymbol,
            string TransactionType,
            string PositionType,
            string Quantity,
            string OldProduct,
            string NewProduct)
        {
            var param = new Dictionary<string, dynamic>();

            AddIfNotNull(param, "exchange", Exchange);
            AddIfNotNull(param, "tradingsymbol", TradingSymbol);
            AddIfNotNull(param, "transaction_type", TransactionType);
            AddIfNotNull(param, "position_type", PositionType);
            AddIfNotNull(param, "quantity", Quantity);
            AddIfNotNull(param, "old_product", OldProduct);
            AddIfNotNull(param, "new_product", NewProduct);

            return Put("portfolio.positions.modify", param);
        }

        /// <summary>
        /// Retrieve the list of market instruments available to trade.
        /// Note that the results could be large, several hundred KBs in size,
		/// with tens of thousands of entries in the list.
        /// </summary>
        /// <param name="Exchange">Name of the exchange</param>
        /// <returns>Json response in the form of array of nested string dictionary.</returns>
        public List<Instrument> GetInstruments(string Exchange = null)
        {
            var param = new Dictionary<string, dynamic>();

            List<Dictionary<string, dynamic>> instrumentsData;

            if (String.IsNullOrEmpty(Exchange))
               instrumentsData = Get("market.instruments.all", param);
            else
            {
                param.Add("exchange", Exchange);
                instrumentsData = Get("market.instruments", param);
            }

            List<Instrument> instruments = new List<Instrument>();

            foreach (Dictionary<string, dynamic> item in instrumentsData)
                instruments.Add(new Instrument(item));

            return instruments;
        }

        /// <summary>
        /// Retrieve quote and market depth for an instrument.
        /// </summary>
        /// <param name="Exchange">Name of the exchange</param>
        /// <param name="TradingSymbol">Tradingsymbol of the instrument</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Quote GetQuote(string Exchange, string TradingSymbol)
        {
            var param = new Dictionary<string, dynamic>();

            param.Add("exchange", Exchange);
            param.Add("tradingsymbol", TradingSymbol);

            var quotedata = Get("market.quote", param);

            return new Quote(quotedata["data"]);
        }

        /// <summary>
        /// Retrieve LTP and OHLC of upto 200 instruments
        /// </summary>
        /// <param name="Exchange">Name of the exchange</param>
        /// <param name="TradingSymbol">Tradingsymbol of the instrument</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, OHLC> GetOHLC(string[] ExchangeTradingSymbol)
        {
            var param = new Dictionary<string, dynamic>();
            param.Add("i", ExchangeTradingSymbol);
            var ohlcdata = Get("market.ohlc", param);

            Dictionary<string, OHLC> ohlcs = new Dictionary<string, OHLC>();
            foreach (string item in ExchangeTradingSymbol)
                ohlcs.Add(item, new OHLC(ohlcdata["data"][item]));

            return ohlcs;
        }

        /// <summary>
        /// Retrieve LTP of upto 200 instruments
        /// </summary>
        /// <param name="Exchange">Name of the exchange</param>
        /// <param name="TradingSymbol">Tradingsymbol of the instrument</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, LTP> GetLTP(string[] ExchangeTradingSymbol)
        {
            var param = new Dictionary<string, dynamic>();
            param.Add("i", ExchangeTradingSymbol);
            var ltpdata = Get("market.ltp", param);

            Dictionary<string, LTP> ltps = new Dictionary<string, LTP>();
            foreach (string item in ExchangeTradingSymbol)
                ltps.Add(item, new LTP(ltpdata["data"][item]));

            return ltps;
        }

        /// <summary>
        /// Retrieve historical data (candles) for an instrument.
        /// </summary>
        /// <param name="InstrumentToken">Identifier for the instrument whose historical records you want to fetch. This is obtained with the instrument list API.</param>
        /// <param name="FromDate">Date in format yyyy-mm-dd for fetching candles between two days. Date in format yyyy-mm-dd hh:mm:ss for fetching candles between two timestamps.</param>
        /// <param name="ToDate">Date in format yyyy-mm-dd for fetching candles between two days. Date in format yyyy-mm-dd hh:mm:ss for fetching candles between two timestamps.</param>
        /// <param name="Interval">The candle record interval. Possible values are: minute, day, 3minute, 5minute, 10minute, 15minute, 30minute, 60minute</param>
        /// <param name="Continuous">Pass true to get continous data of expired instruments.</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public List<Historical> GetHistorical(
            string InstrumentToken,
            string FromDate,
            string ToDate,
            string Interval,
            bool Continuous = false)
        {
            var param = new Dictionary<string, dynamic>();

            param.Add("instrument_token", InstrumentToken);
            param.Add("from", FromDate);
            param.Add("to", ToDate);
            param.Add("interval", Interval);
            param.Add("continuous", Continuous ? "1" : "0");

            var historicaldata = Get("market.historical", param);

            List<Historical> historicals = new List<Historical>();

            foreach (ArrayList item in historicaldata["data"]["candles"])
                historicals.Add(new Historical(item));

            return historicals;
        }

        /// <summary>
        /// Retrieve the buy/sell trigger range for Cover Orders.
        /// </summary>
        /// <param name="Exchange">Name of the exchange</param>
        /// <param name="TradingSymbol">Tradingsymbol of the instrument</param>
        /// <param name="TrasactionType">BUY or SELL</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public TrigerRange GetTriggerRange(string Exchange, string TradingSymbol, string TrasactionType)
        {
            var param = new Dictionary<string, dynamic>();

            param.Add("exchange", Exchange);
            param.Add("tradingsymbol", TradingSymbol);
            param.Add("transaction_type", TrasactionType);

            var triggerdata = Get("market.trigger_range", param);

            return new TrigerRange(triggerdata["data"]);
        }

        /// <summary>
        /// Gets the Mutual funds Instruments.
        /// </summary>
        /// <returns>The Mutual funds Instruments.</returns>
        public List<MFInstrument> GetMFInstruments(){
			var param = new Dictionary<string, dynamic>();

			List<Dictionary<string, dynamic>> instrumentsData;
			
			instrumentsData = Get("mutualfunds.instruments", param);

            List<MFInstrument> instruments = new List<MFInstrument>();

			foreach (Dictionary<string, dynamic> item in instrumentsData)
				instruments.Add(new MFInstrument(item));

			return instruments;
        }

        /// <summary>
        /// Gets all Mutual funds orders.
        /// </summary>
        /// <returns>The Mutual funds orders.</returns>
        public List<MFOrder> GetMFOrders()
		{
			var param = new Dictionary<string, dynamic>();

			Dictionary<string, dynamic> ordersData;
			ordersData = Get("mutualfunds.orders", param);

            List<MFOrder> orderlist = new List<MFOrder>();

			foreach (Dictionary<string, dynamic> item in ordersData["data"])
                orderlist.Add(new MFOrder(item));

			return orderlist;
		}

        /// <summary>
        /// Gets the Mutual funds order by OrderId.
        /// </summary>
        /// <returns>The Mutual funds order.</returns>
        /// <param name="OrderId">Order id.</param>
        public MFOrder GetMFOrder(String OrderId)
		{
			var param = new Dictionary<string, dynamic>();
            param.Add("order_id", OrderId);

			Dictionary<string, dynamic> orderData;
			orderData = Get("mutualfunds.order", param);

            return new MFOrder(orderData["data"]);
		}

		/// <summary>
		/// Places a Mutual funds order.
		/// </summary>
		/// <returns>JSON response as nested string dictionary.</returns>
		/// <param name="TradingSymbol">Tradingsymbol (ISIN) of the fund.</param>
		/// <param name="TransactionType">BUY or SELL.</param>
		/// <param name="Amount">Amount worth of units to purchase. Not applicable on SELLs.</param>
		/// <param name="Quantity">Quantity to SELL. Not applicable on BUYs. If the holding is less than minimum_redemption_quantity, all the units have to be sold.</param>
		/// <param name="Tag">An optional tag to apply to an order to identify it (alphanumeric, max 8 chars).</param>
		public Dictionary<string, dynamic> PlaceMFOrder(string TradingSymbol, string TransactionType, string Amount, string Quantity = "", string Tag = ""){
			var param = new Dictionary<string, dynamic>();
			
            AddIfNotNull(param, "tradingsymbol", TradingSymbol);
            AddIfNotNull(param, "transaction_type", TransactionType);
            AddIfNotNull(param, "amount", Amount);
            AddIfNotNull(param, "quantity", Quantity);
            AddIfNotNull(param, "tag", Tag);

			return Post("mutualfunds.orders.place", param);
        }

		/// <summary>
		/// Cancels the Mutual funds order.
		/// </summary>
		/// <returns>JSON response as nested string dictionary.</returns>
		/// <param name="OrderId">Unique order id.</param>
		public Dictionary<string, dynamic> CancelMFOrder(String OrderId)
		{
			var param = new Dictionary<string, dynamic>();

			AddIfNotNull(param, "order_id", OrderId);

			return Delete("mutualfunds.cancel_order", param);
		}

		/// <summary>
		/// Gets all Mutual funds SIPs.
		/// </summary>
		/// <returns>The list of all Mutual funds SIPs.</returns>
		public List<MFSIP> GetMFSIPs()
		{
			var param = new Dictionary<string, dynamic>();

			Dictionary<string, dynamic> sipData;
			sipData = Get("mutualfunds.sips", param);

			List<MFSIP> siplist = new List<MFSIP>();

			foreach (Dictionary<string, dynamic> item in sipData["data"])
				siplist.Add(new MFSIP(item));

			return siplist;
		}

		/// <summary>
		/// Gets a single Mutual funds SIP by SIP id.
		/// </summary>
		/// <returns>The Mutual funds SIP.</returns>
		/// <param name="SIPID">SIP id.</param>
		public MFSIP GetMFSIP(String SIPID)
		{
			var param = new Dictionary<string, dynamic>();
			param.Add("sip_id", SIPID);

			Dictionary<string, dynamic> sipData;
			sipData = Get("mutualfunds.sip", param);

			return new MFSIP(sipData["data"]);
		}

		/// <summary>
		/// Places a Mutual funds SIP order.
		/// </summary>
		/// <returns>JSON response as nested string dictionary.</returns>
		/// <param name="TradingSymbol">ISIN of the fund.</param>
		/// <param name="Amount">Amount worth of units to purchase. It should be equal to or greated than minimum_additional_purchase_amount and in multiple of purchase_amount_multiplier in the instrument master.</param>
		/// <param name="InitialAmount">Amount worth of units to purchase before the SIP starts. Should be equal to or greater than minimum_purchase_amount and in multiple of purchase_amount_multiplier. This is only considered if there have been no prior investments in the target fund.</param>
		/// <param name="Frequency">weekly, monthly, or quarterly.</param>
		/// <param name="InstalmentDay">If Frequency is monthly, the day of the month (1, 5, 10, 15, 20, 25) to trigger the order on.</param>
		/// <param name="Instalments">Number of instalments to trigger. If set to -1, instalments are triggered at fixed intervals until the SIP is cancelled.</param>
		/// <param name="Tag">An optional tag to apply to an order to identify it (alphanumeric, max 8 chars).</param>
		public Dictionary<string, dynamic> PlaceMFSIP(string TradingSymbol, string Amount, string InitialAmount, string Frequency, string InstalmentDay, string Instalments, string Tag = "")
		{
			var param = new Dictionary<string, dynamic>();

			AddIfNotNull(param, "tradingsymbol", TradingSymbol);
			AddIfNotNull(param, "initial_amount", InitialAmount);
			AddIfNotNull(param, "amount", Amount);
            AddIfNotNull(param, "frequency", Frequency);
			AddIfNotNull(param, "instalment_day", InstalmentDay);
            AddIfNotNull(param, "instalments", Instalments);

			return Post("mutualfunds.sips.place", param);
		}

		/// <summary>
		/// Modifies the Mutual funds SIP.
		/// </summary>
		/// <returns>JSON response as nested string dictionary.</returns>
		/// <param name="SIPId">SIP id.</param>
		/// <param name="Amount">Amount worth of units to purchase. It should be equal to or greated than minimum_additional_purchase_amount and in multiple of purchase_amount_multiplier in the instrument master.</param>
		/// <param name="Frequency">weekly, monthly, or quarterly.</param>
		/// <param name="InstalmentDay">If Frequency is monthly, the day of the month (1, 5, 10, 15, 20, 25) to trigger the order on.</param>
		/// <param name="Instalments">Number of instalments to trigger. If set to -1, instalments are triggered idefinitely until the SIP is cancelled.</param>
		/// <param name="Status">Pause or unpause an SIP (active or paused).</param>
		public Dictionary<string, dynamic> ModifyMFSIP(string SIPId, string Amount, string Frequency, string InstalmentDay, string Instalments, string Status)
		{
			var param = new Dictionary<string, dynamic>();

			AddIfNotNull(param, "status", Status);
			AddIfNotNull(param, "sip_id", SIPId);
			AddIfNotNull(param, "amount", Amount);
			AddIfNotNull(param, "frequency", Frequency);
			AddIfNotNull(param, "instalment_day", InstalmentDay);
			AddIfNotNull(param, "instalments", Instalments);

			return Put("mutualfunds.sips.modify", param);
		}

        /// <summary>
        /// Cancels the Mutual funds SIP.
        /// </summary>
        /// <returns>JSON response as nested string dictionary.</returns>
        /// <param name="SIPId">SIP id.</param>
		public Dictionary<string, dynamic> CancelMFSIP(String SIPId)
		{
			var param = new Dictionary<string, dynamic>();

			AddIfNotNull(param, "sip_id", SIPId);

			return Delete("mutualfunds.cancel_sips", param);
		}

        /// <summary>
        /// Gets the Mutual funds holdings.
        /// </summary>
        /// <returns>The list of all Mutual funds holdings.</returns>
        public List<MFHolding> GetMFHoldings()
		{
			var param = new Dictionary<string, dynamic>();

			Dictionary<string, dynamic> holdingsData;
			holdingsData = Get("mutualfunds.holdings", param);

			List<MFHolding> holdingslist = new List<MFHolding>();

			foreach (Dictionary<string, dynamic> item in holdingsData["data"])
				holdingslist.Add(new MFHolding(item));

			return holdingslist;
		}

        /// <summary>
        /// Alias for sending a GET request.
        /// </summary>
        /// <param name="Route">URL route of API</param>
        /// <param name="Params">Additional paramerters</param>
        /// <returns>Varies according to API endpoint</returns>
        private dynamic Get(string Route, Dictionary<string, dynamic> Params = null)
        {
            return Request(Route, "GET", Params);
        }

        /// <summary>
        /// Alias for sending a POST request.
        /// </summary>
        /// <param name="Route">URL route of API</param>
        /// <param name="Params">Additional paramerters</param>
        /// <returns>Varies according to API endpoint</returns>
        private dynamic Post(string Route, Dictionary<string, dynamic> Params = null)
        {
            return Request(Route, "POST", Params);
        }

        /// <summary>
        /// Alias for sending a PUT request.
        /// </summary>
        /// <param name="Route">URL route of API</param>
        /// <param name="Params">Additional paramerters</param>
        /// <returns>Varies according to API endpoint</returns>
        private dynamic Put(string Route, Dictionary<string, dynamic> Params = null)
        {
            return Request(Route, "PUT", Params);
        }

        /// <summary>
        /// Alias for sending a DELETE request.
        /// </summary>
        /// <param name="Route">URL route of API</param>
        /// <param name="Params">Additional paramerters</param>
        /// <returns>Varies according to API endpoint</returns>
        private dynamic Delete(string Route, Dictionary<string, dynamic> Params = null)
        {
            return Request(Route, "DELETE", Params);
        }

        private string BuildParam(string Key, dynamic Value)
        {
            if(Value is string)
            {
                return HttpUtility.UrlEncode(Key) + "=" + HttpUtility.UrlEncode((string)Value);
            }
            else
            {
                string[] values = (string[])Value;
                return String.Join("&", values.Select(x => HttpUtility.UrlEncode(Key) + "=" + HttpUtility.UrlEncode(x)));
            }
        }
        /// <summary>
        /// Make an HTTP request.
        /// </summary>
        /// <param name="Route">URL route of API</param>
        /// <param name="Method">Method of HTTP request</param>
        /// <param name="Params">Additional paramerters</param>
        /// <returns>Varies according to API endpoint</returns>
        private dynamic Request(string Route, string Method, Dictionary<string, dynamic> Params = null)
        {
            string url = _root + _routes[Route];

            if (Params is null)
                Params = new Dictionary<string, dynamic>();

            if (url.Contains("{"))
            {
                var urlparams = Params.ToDictionary(entry => entry.Key, entry => entry.Value);

                foreach (KeyValuePair<string, dynamic> item in urlparams)
                    if (url.Contains("{" + item.Key + "}"))
                    {
                        url = url.Replace("{" + item.Key + "}", (string)item.Value);
                        Params.Remove(item.Key);
                    }
            }

            if (!Params.ContainsKey("api_key"))
                Params.Add("api_key", _apiKey);

            if (!Params.ContainsKey("access_token") && !String.IsNullOrEmpty(_accessToken))
                Params.Add("access_token", _accessToken);

            HttpWebRequest request;
            string paramString = String.Join("&", Params.Select(x => BuildParam(x.Key, x.Value)));

            
            if (Method == "POST" || Method == "PUT")
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = Method;
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = paramString.Length;
                if (_debug) Console.WriteLine("DEBUG: " + Method + " " + url + "\n" + paramString + "\n");

                using (Stream webStream = request.GetRequestStream())
                    using (StreamWriter requestWriter = new StreamWriter(webStream))
                        requestWriter.Write(paramString);
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create(url + "?" + paramString);
                request.Method = Method;
                if (_debug) Console.WriteLine("DEBUG: " + Method + " " + url + "?" + paramString + "\n");
            }

            //request.Headers.Add("X-Kite-Version: 3");
            //if(request.Method == "GET" && cache.IsCached(request.RequestUri.AbsoluteUri))
            //{
            //    request.Headers.Add("If-None-Match: " + cache.GetETag(request.RequestUri.AbsoluteUri));
            //}
            request.Timeout = _timeout;
            if (_proxy != null) request.Proxy = _proxy;

            WebResponse webResponse;
            try
            {
                webResponse = request.GetResponse();
            }
            catch(WebException e)
            {
                if (e.Response is null)
                    throw e;
                
                webResponse = e.Response;
            }
            
            using (Stream webStream = webResponse.GetResponseStream())
            {
                using (StreamReader responseReader = new StreamReader(webStream))
                {
                    string response = responseReader.ReadToEnd();                    
                    if (_debug) Console.WriteLine("DEBUG: " + (int)((HttpWebResponse)webResponse).StatusCode + " " + response + "\n");

                    if (((HttpWebResponse)webResponse).StatusCode == HttpStatusCode.Forbidden)
                        _sessionHook?.Invoke();

                    //if (request.Method == "GET" && webResponse.Headers[HttpResponseHeader.ETag] != null)
                    //{

                    //}
                    if (webResponse.ContentType == "application/json")
                    {
                        Dictionary<string, dynamic> responseDictionary = JsonDeserialize(response);
                        return responseDictionary;
                    }
                    else if (webResponse.ContentType == "text/csv")
                        return ParseCSV(response);
                    else
                        throw new Exception("Unexpected content type");
                }
            }
        }

        /// <summary>
        /// Deserialize Json string to nested string dictionary.
        /// </summary>
        /// <param name="Json">Json string to deserialize.</param>
        /// <returns>Json in the form of nested string dictionary.</returns>
        private Dictionary<string, dynamic> JsonDeserialize(string Json)
        {
            var jss = new JavaScriptSerializer();
            Dictionary<string, dynamic> dict = jss.Deserialize<Dictionary<string, dynamic>>(Json);
            return dict;
        }

        /// <summary>
        /// Parse instruments API's CSV response.
        /// </summary>
        /// <param name="Data">Response of instruments API.</param>
        /// <returns>CSV data as array of nested string dictionary.</returns>
        private List<Dictionary<string, dynamic>> ParseCSV(string Data)
        {
            string[] lines = Data.Split('\n');

            List<Dictionary<string, dynamic>> instruments = new List<Dictionary<string, dynamic>>();

            using (TextFieldParser parser = new TextFieldParser(StreamFromString(Data)))
            {
                // parser.CommentTokens = new string[] { "#" };
                parser.SetDelimiters(new string[] { "," });
                parser.HasFieldsEnclosedInQuotes = true;

                // Skip over header line.
                string[] headers = parser.ReadLine().Split(',');

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    Dictionary<string, dynamic> item = new Dictionary<string, dynamic>();

                    for (var i = 0; i < headers.Length; i++)
                        item.Add(headers[i], fields[i]);

                    //item.Add("instrument_token", fields[0]);
                    //item.Add("exchange_token", fields[1]);
                    //item.Add("tradingsymbol", fields[2]);
                    //item.Add("name", fields[3]);
                    //item.Add("last_price", fields[4]);
                    //item.Add("expiry", fields[5]);
                    //item.Add("strike", fields[6]);
                    //item.Add("tick_size", fields[7]);
                    //item.Add("lot_size", fields[8]);
                    //item.Add("instrument_type", fields[9]);
                    //item.Add("segment", fields[10]);
                    //item.Add("exchange", fields[11]);

                    instruments.Add(item);
                }
            }

            return instruments;
        }

        /// <summary>
        /// Wraps a string inside a stream
        /// </summary>
        /// <param name="value">string data</param>
        /// <returns>Stream that reads input string</returns>
        private MemoryStream StreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        /// <summary>
        /// Helper function to add parameter to the request only if it is not null or empty
        /// </summary>
        /// <param name="Params">Dictionary to add the key-value pair</param>
        /// <param name="Key">Key of the parameter</param>
        /// <param name="Value">Value of the parameter</param>
        private void AddIfNotNull(Dictionary<string, dynamic> Params, string Key, string Value)
        {
            if (!String.IsNullOrEmpty(Value))
                Params.Add(Key, Value);
        }

        /// <summary>
        /// Generates SHA256 checksum for login.
        /// </summary>
        /// <param name="Data">Input data to generate checksum for.</param>
        /// <returns>SHA256 checksum in hex format.</returns>
        private string SHA256(string Data)
        {
            Console.WriteLine(Data);
            SHA256Managed sha256 = new SHA256Managed();
            StringBuilder hexhash = new StringBuilder();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(Data), 0, Encoding.UTF8.GetByteCount(Data));
            foreach (byte b in hash)
            {
                hexhash.Append(b.ToString("x2"));
            }
            return hexhash.ToString();
        }
    }
}
