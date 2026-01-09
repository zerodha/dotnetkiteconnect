using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace KiteConnect
{
    /// <summary>
    /// The API client class. In production, you may initialize a single instance of this class per `APIKey`.
    /// </summary>
    public class Kite
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, Converters = { new JsonStringEnumConverter() } };
        // Default root API endpoint. It's possible to
        // override this by passing the `Root` parameter during initialisation.
        private string _root = "https://api.kite.trade";
        private string _login = "https://kite.zerodha.com/connect/login";

        private string _apiKey;
        private string _accessToken;
        private bool _enableLogging;

        private int _timeout;

        private Action _sessionHook;

        private HttpClient httpClient;

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
        public Kite(string APIKey, string AccessToken = null, string Root = null, bool Debug = false, int Timeout = 7000, IWebProxy Proxy = null, int Pool = 2)
        {
            _accessToken = AccessToken;
            _apiKey = APIKey;
            if (!string.IsNullOrEmpty(Root)) this._root = Root;
            _enableLogging = Debug;

            _timeout = Timeout;

            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                Proxy = Proxy,
            };
            httpClient = new(httpClientHandler)
            {
                BaseAddress = new Uri(_root),
                Timeout = TimeSpan.FromMilliseconds(Timeout),
            };

            ServicePointManager.DefaultConnectionLimit = Pool;
        }

        /// <summary>
        /// Enabling logging prints HTTP request and response summaries to console
        /// </summary>
        /// <param name="enableLogging">Set to true to enable logging</param>
        public void EnableLogging(bool enableLogging)
        {
            _enableLogging = enableLogging;
        }

        /// <summary>
        /// Set a callback hook for session (`TokenException` -- timeout, expiry etc.) errors.
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
        public void SetSessionExpiryHook(Action Method)
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
            return string.Format("{0}?api_key={1}&v=3", _login, _apiKey);
        }

        /// <summary>
        /// Do the token exchange with the `RequestToken` obtained after the login flow,
		/// and retrieve the `AccessToken` required for all subsequent requests.The
        /// response contains not just the `AccessToken`, but metadata for
        /// the user who has authenticated.
        /// </summary>
        /// <param name="RequestToken">Token obtained from the GET paramers after a successful login redirect.</param>
        /// <param name="AppSecret">API secret issued with the API key.</param>
        /// <returns>User structure with tokens and profile data</returns>
        public User GenerateSession(string RequestToken, string AppSecret)
        {
            string checksum = Utils.SHA256Hash(_apiKey + RequestToken + AppSecret);

            var param = new Dictionary<string, dynamic>
            {
                {"api_key", _apiKey},
                {"request_token", RequestToken},
                {"checksum", checksum}
            };

            var userData = Post(Routes.API.Token, param);

            return new User(userData);
        }

        /// <summary>
        /// Kill the session by invalidating the access token
        /// </summary>
        /// <param name="AccessToken">Access token to invalidate. Default is the active access token.</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> InvalidateAccessToken(string AccessToken = null)
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "api_key", _apiKey);
            Utils.AddIfNotNull(param, "access_token", AccessToken);

            return Delete(Routes.API.Token, param);
        }

        /// <summary>
        /// Invalidates RefreshToken
        /// </summary>
        /// <param name="RefreshToken">RefreshToken to invalidate</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> InvalidateRefreshToken(string RefreshToken)
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "api_key", _apiKey);
            Utils.AddIfNotNull(param, "refresh_token", RefreshToken);

            return Delete(Routes.API.Token, param);
        }

        /// <summary>
        /// Renew AccessToken using RefreshToken
        /// </summary>
        /// <param name="RefreshToken">RefreshToken to renew the AccessToken.</param>
        /// <param name="AppSecret">API secret issued with the API key.</param>
        /// <returns>TokenRenewResponse that contains new AccessToken and RefreshToken.</returns>
        public TokenSet RenewAccessToken(string RefreshToken, string AppSecret)
        {
            var param = new Dictionary<string, dynamic>();

            string checksum = Utils.SHA256Hash(_apiKey + RefreshToken + AppSecret);

            Utils.AddIfNotNull(param, "api_key", _apiKey);
            Utils.AddIfNotNull(param, "refresh_token", RefreshToken);
            Utils.AddIfNotNull(param, "checksum", checksum);

            return new TokenSet(Post(Routes.API.Refresh, param));
        }

        /// <summary>
        /// Gets currently logged in user details
        /// </summary>
        /// <returns>User profile</returns>
        public Task<Profile> GetProfileAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<Profile>("/user/profile", cancellationToken);
        }

        /// <summary>
        /// A virtual contract provides detailed charges order-wise for brokerage, STT, stamp duty, exchange transaction charges, SEBI turnover charge, and GST.
        /// </summary>
        /// <param name="ContractNoteParams">List of all order params to get contract notes for</param>
        /// <returns>List of contract notes for the params</returns>
        public List<ContractNote> GetVirtualContractNote(List<ContractNoteParams> ContractNoteParams)
        {
            var paramList = new List<Dictionary<string, dynamic>>();

            foreach (var item in ContractNoteParams)
            {
                var param = new Dictionary<string, dynamic>();
                param["order_id"] = item.OrderID;
                param["exchange"] = item.Exchange;
                param["tradingsymbol"] = item.TradingSymbol;
                param["transaction_type"] = item.TransactionType;
                param["quantity"] = item.Quantity;
                param["average_price"] = item.AveragePrice;
                param["product"] = item.Product;
                param["order_type"] = item.OrderType;
                param["variety"] = item.Variety;

                paramList.Add(param);
            }

            var contractNoteData = Post(Routes.Order.ContractNote, paramList, json: true);

            List<ContractNote> contractNotes = new List<ContractNote>();
            foreach (Dictionary<string, dynamic> item in contractNoteData["data"])
                contractNotes.Add(new ContractNote(item));

            return contractNotes;
        }

        /// <summary>
        /// Margin data for a specific order
        /// </summary>
        /// <param name="OrderMarginParams">List of all order params to get margins for</param>
        /// <param name="Mode">Mode of the returned response content. Eg: Constants.Margin.Mode.Compact</param>
        /// <returns>List of margins of order</returns>
        public List<OrderMargin> GetOrderMargins(List<OrderMarginParams> OrderMarginParams, string Mode = null)
        {
            var paramList = new List<Dictionary<string, dynamic>>();

            foreach (var item in OrderMarginParams)
            {
                var param = new Dictionary<string, dynamic>();
                param["exchange"] = item.Exchange;
                param["tradingsymbol"] = item.TradingSymbol;
                param["transaction_type"] = item.TransactionType;
                param["quantity"] = item.Quantity;
                param["price"] = item.Price;
                param["product"] = item.Product;
                param["order_type"] = item.OrderType;
                param["trigger_price"] = item.TriggerPrice;
                param["variety"] = item.Variety;

                paramList.Add(param);
            }

            var queryParams = new Dictionary<string, dynamic>();
            if (Mode != null)
            {
                queryParams["mode"] = Mode;
            }

            var orderMarginsData = Post(Routes.Order.Margins, paramList, QueryParams: queryParams, json: true);

            List<OrderMargin> orderMargins = new List<OrderMargin>();
            foreach (Dictionary<string, dynamic> item in orderMarginsData["data"])
                orderMargins.Add(new OrderMargin(item));

            return orderMargins;
        }

        /// <summary>
        /// Margin data for a basket orders
        /// </summary>
        /// <param name="OrderMarginParams">List of all order params to get margins for</param>
        /// <param name="ConsiderPositions">Consider users positions while calculating margins</param>
        /// <param name="Mode">Mode of the returned response content. Eg: Constants.Margin.Mode.Compact</param>
        /// <returns>List of margins of order</returns>
        public BasketMargin GetBasketMargins(List<OrderMarginParams> OrderMarginParams, bool ConsiderPositions = true, string Mode = null)
        {
            var paramList = new List<Dictionary<string, dynamic>>();

            foreach (var item in OrderMarginParams)
            {
                var param = new Dictionary<string, dynamic>();
                param["exchange"] = item.Exchange;
                param["tradingsymbol"] = item.TradingSymbol;
                param["transaction_type"] = item.TransactionType;
                param["quantity"] = item.Quantity;
                param["price"] = item.Price;
                param["product"] = item.Product;
                param["order_type"] = item.OrderType;
                param["trigger_price"] = item.TriggerPrice;
                param["variety"] = item.Variety;

                paramList.Add(param);
            }

            var queryParams = new Dictionary<string, dynamic>();
            queryParams["consider_positions"] = ConsiderPositions ? "true" : "false";
            if (Mode != null)
            {
                queryParams["mode"] = Mode;
            }

            var basketMarginsData = Post(Routes.Basket.Margins, paramList, QueryParams: queryParams, json: true);

            return new BasketMargin(basketMarginsData["data"]);
        }

        /// <summary>
        /// Get account balance and cash margin details for all segments.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>User margin response with both equity and commodity margins.</returns>
        public Task<UserMarginsResponse> GetMarginsAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<UserMarginsResponse>("/user/margins", cancellationToken);
        }

        /// <summary>
        /// Get account balance and cash margin details for a particular segment.
        /// </summary>
        /// <param name="Segment">Trading segment (eg: equity or commodity)</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Margins for specified segment.</returns>
        public Task<UserMargin> GetMarginsAsync(string Segment, CancellationToken cancellationToken = default)
        {
            return GetAsync<UserMargin>($"/user/margins/{Segment}", cancellationToken);
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
        /// <param name="Validity">Order validity (DAY, IOC and TTL)</param>
        /// <param name="DisclosedQuantity">Quantity to disclose publicly (for equity trades)</param>
        /// <param name="TriggerPrice">For SL, SL-M etc.</param>
        /// <param name="SquareOffValue">Price difference at which the order should be squared off and profit booked (eg: Order price is 100. Profit target is 102. So squareoff = 2)</param>
        /// <param name="StoplossValue">Stoploss difference at which the order should be squared off (eg: Order price is 100. Stoploss target is 98. So stoploss = 2)</param>
        /// <param name="TrailingStoploss">Incremental value by which stoploss price changes when market moves in your favor by the same incremental value from the entry price (optional)</param>
        /// <param name="Variety">You can place orders of varieties; regular orders, after market orders, cover orders, iceberg orders etc. </param>
        /// <param name="Tag">An optional tag to apply to an order to identify it (alphanumeric, max 20 chars)</param>
        /// <param name="ValidityTTL">Order life span in minutes for TTL validity orders</param>
        /// <param name="IcebergLegs">Total number of legs for iceberg order type (number of legs per Iceberg should be between 2 and 10)</param>
        /// <param name="IcebergQuantity">Split quantity for each iceberg leg order (Quantity/IcebergLegs)</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> PlaceOrder(
            string Exchange,
            string TradingSymbol,
            string TransactionType,
            decimal Quantity,
            decimal? Price = null,
            string Product = null,
            string OrderType = null,
            string Validity = null,
            decimal? DisclosedQuantity = null,
            decimal? TriggerPrice = null,
            decimal? SquareOffValue = null,
            decimal? StoplossValue = null,
            decimal? TrailingStoploss = null,
            string Variety = Constants.Variety.Regular,
            string Tag = "",
            int? ValidityTTL = null,
            int? IcebergLegs = null,
            decimal? IcebergQuantity = null,
            string AuctionNumber = null
            )
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "exchange", Exchange);
            Utils.AddIfNotNull(param, "tradingsymbol", TradingSymbol);
            Utils.AddIfNotNull(param, "transaction_type", TransactionType);
            Utils.AddIfNotNull(param, "quantity", Quantity.ToString());
            Utils.AddIfNotNull(param, "price", Price.ToString());
            Utils.AddIfNotNull(param, "product", Product);
            Utils.AddIfNotNull(param, "order_type", OrderType);
            Utils.AddIfNotNull(param, "validity", Validity);
            Utils.AddIfNotNull(param, "disclosed_quantity", DisclosedQuantity.ToString());
            Utils.AddIfNotNull(param, "trigger_price", TriggerPrice.ToString());
            Utils.AddIfNotNull(param, "squareoff", SquareOffValue.ToString());
            Utils.AddIfNotNull(param, "stoploss", StoplossValue.ToString());
            Utils.AddIfNotNull(param, "trailing_stoploss", TrailingStoploss.ToString());
            Utils.AddIfNotNull(param, "variety", Variety);
            Utils.AddIfNotNull(param, "tag", Tag);
            Utils.AddIfNotNull(param, "validity_ttl", ValidityTTL.ToString());
            Utils.AddIfNotNull(param, "iceberg_legs", IcebergLegs.ToString());
            Utils.AddIfNotNull(param, "iceberg_quantity", IcebergQuantity.ToString());
            Utils.AddIfNotNull(param, "auction_number", AuctionNumber);

            return Post(Routes.Order.Place, param);
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
            decimal? Quantity = null,
            decimal? Price = null,
            string Product = null,
            string OrderType = null,
            string Validity = Constants.Validity.Day,
            decimal? DisclosedQuantity = null,
            decimal? TriggerPrice = null,
            string Variety = Constants.Variety.Regular)
        {
            var param = new Dictionary<string, dynamic>();

            string VarietyString = Variety;
            string ProductString = Product;

            if ((ProductString == "bo" || ProductString == "co") && VarietyString != ProductString)
                throw new Exception(string.Format("Invalid variety. It should be: {0}", ProductString));

            Utils.AddIfNotNull(param, "order_id", OrderId);
            Utils.AddIfNotNull(param, "parent_order_id", ParentOrderId);
            Utils.AddIfNotNull(param, "trigger_price", TriggerPrice.ToString());
            Utils.AddIfNotNull(param, "variety", Variety);

            if (VarietyString == "bo" && ProductString == "bo")
            {
                Utils.AddIfNotNull(param, "quantity", Quantity.ToString());
                Utils.AddIfNotNull(param, "price", Price.ToString());
                Utils.AddIfNotNull(param, "disclosed_quantity", DisclosedQuantity.ToString());
            }
            else if (VarietyString != "co" && ProductString != "co")
            {
                Utils.AddIfNotNull(param, "exchange", Exchange);
                Utils.AddIfNotNull(param, "tradingsymbol", TradingSymbol);
                Utils.AddIfNotNull(param, "transaction_type", TransactionType);
                Utils.AddIfNotNull(param, "quantity", Quantity.ToString());
                Utils.AddIfNotNull(param, "price", Price.ToString());
                Utils.AddIfNotNull(param, "product", Product);
                Utils.AddIfNotNull(param, "order_type", OrderType);
                Utils.AddIfNotNull(param, "validity", Validity);
                Utils.AddIfNotNull(param, "disclosed_quantity", DisclosedQuantity.ToString());
            }

            return Put(Routes.Order.Modify, param);
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        /// <param name="OrderId">Id of the order to be cancelled</param>
        /// <param name="Variety">You can place orders of varieties; regular orders, after market orders, cover orders etc. </param>
        /// <param name="ParentOrderId">Id of the parent order (obtained from the /orders call) as BO is a multi-legged order</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> CancelOrder(string OrderId, string Variety = Constants.Variety.Regular, string ParentOrderId = null)
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "order_id", OrderId);
            Utils.AddIfNotNull(param, "parent_order_id", ParentOrderId);
            Utils.AddIfNotNull(param, "variety", Variety);

            return Delete(Routes.Order.Cancel, param);
        }

        /// <summary>
        /// Gets the collection of orders from the orderbook.
        /// </summary>
        /// <returns>List of orders.</returns>
        public List<Order> GetOrders()
        {
            var ordersData = Get(Routes.Order.AllOrders);

            List<Order> orders = new List<Order>();

            foreach (Dictionary<string, dynamic> item in ordersData["data"])
                orders.Add(new Order(item));

            return orders;
        }

        /// <summary>
        /// Gets information about given OrderId.
        /// </summary>
        /// <param name="OrderId">Unique order id</param>
        /// <returns>List of order objects.</returns>
        public List<Order> GetOrderHistory(string OrderId)
        {
            var param = new Dictionary<string, dynamic>();
            param.Add("order_id", OrderId);

            var orderData = Get(Routes.Order.History, param);

            List<Order> orderhistory = new List<Order>();

            foreach (Dictionary<string, dynamic> item in orderData["data"])
                orderhistory.Add(new Order(item));

            return orderhistory;
        }

        /// <summary>
        /// Retreive the list of trades executed (all or ones under a particular order).
        /// An order can be executed in tranches based on market conditions.
        /// These trades are individually recorded under an order.
        /// </summary>
        /// <param name="OrderId">is the ID of the order (optional) whose trades are to be retrieved. If no `OrderId` is specified, all trades for the day are returned.</param>
        /// <returns>List of trades of given order.</returns>
        public List<Trade> GetOrderTrades(string OrderId = null)
        {
            Dictionary<string, dynamic> tradesdata;
            if (!string.IsNullOrEmpty(OrderId))
            {
                var param = new Dictionary<string, dynamic>();
                param.Add("order_id", OrderId);
                tradesdata = Get(Routes.Order.Trades, param);
            }
            else
                tradesdata = Get(Routes.Order.AllTrades);

            List<Trade> trades = new List<Trade>();

            foreach (Dictionary<string, dynamic> item in tradesdata["data"])
                trades.Add(new Trade(item));

            return trades;
        }

        /// <summary>
        /// Retrieve the list of positions.
        /// </summary>
        /// <returns>Day and net positions.</returns>
        public PositionResponse GetPositions()
        {
            var positionsdata = Get(Routes.Portfolio.Positions);
            return new PositionResponse(positionsdata["data"]);
        }

        /// <summary>
        /// Retrieve the list of equity holdings.
        /// </summary>
        /// <returns>List of holdings.</returns>
        public List<Holding> GetHoldings()
        {
            var holdingsData = Get(Routes.Portfolio.Holdings);

            List<Holding> holdings = new List<Holding>();

            foreach (Dictionary<string, dynamic> item in holdingsData["data"])
                holdings.Add(new Holding(item));

            return holdings;
        }

        /// <summary>
        /// Retrieve the list of auction instruments.
        /// </summary>
        /// <returns>List of auction instruments.</returns>
        public List<AuctionInstrument> GetAuctionInstruments()
        {
            var instrumentsData = Get(Routes.Portfolio.AuctionInstruments);

            List<AuctionInstrument> instruments = new List<AuctionInstrument>();

            foreach (Dictionary<string, dynamic> item in instrumentsData["data"])
                instruments.Add(new AuctionInstrument(item));

            return instruments;
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
        public Dictionary<string, dynamic> ConvertPosition(
            string Exchange,
            string TradingSymbol,
            string TransactionType,
            string PositionType,
            decimal? Quantity,
            string OldProduct,
            string NewProduct)
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "exchange", Exchange);
            Utils.AddIfNotNull(param, "tradingsymbol", TradingSymbol);
            Utils.AddIfNotNull(param, "transaction_type", TransactionType);
            Utils.AddIfNotNull(param, "position_type", PositionType);
            Utils.AddIfNotNull(param, "quantity", Quantity.ToString());
            Utils.AddIfNotNull(param, "old_product", OldProduct);
            Utils.AddIfNotNull(param, "new_product", NewProduct);

            return Put(Routes.Portfolio.ModifyPositions, param);
        }

        /// <summary>
        /// Retrieve the list of market instruments available to trade.
        /// Note that the results could be large, several hundred KBs in size,
		/// with tens of thousands of entries in the list.
        /// </summary>
        /// <param name="Exchange">Name of the exchange</param>
        /// <returns>List of instruments.</returns>
        public List<Instrument> GetInstruments(string Exchange = null)
        {
            var param = new Dictionary<string, dynamic>();

            List<Dictionary<string, dynamic>> instrumentsData;

            if (string.IsNullOrEmpty(Exchange))
                instrumentsData = Get(Routes.Market.AllInstruments, param);
            else
            {
                param.Add("exchange", Exchange);
                instrumentsData = Get(Routes.Market.Instruments, param);
            }

            List<Instrument> instruments = new List<Instrument>();

            foreach (Dictionary<string, dynamic> item in instrumentsData)
                instruments.Add(new Instrument(item));

            return instruments;
        }

        /// <summary>
        /// Retrieve quote and market depth of upto 200 instruments
        /// </summary>
        /// <param name="InstrumentId">Indentification of instrument in the form of EXCHANGE:TRADINGSYMBOL (eg: NSE:INFY) or InstrumentToken (eg: 408065)</param>
        /// <returns>Dictionary of all Quote objects with keys as in InstrumentId</returns>
        public Dictionary<string, Quote> GetQuote(string[] InstrumentId)
        {
            var param = new Dictionary<string, dynamic>();
            param.Add("i", InstrumentId);
            Dictionary<string, dynamic> quoteData = Get(Routes.Market.Quote, param)["data"];

            Dictionary<string, Quote> quotes = new Dictionary<string, Quote>();
            foreach (string item in quoteData.Keys)
                quotes.Add(item, new Quote(quoteData[item]));

            return quotes;
        }

        /// <summary>
        /// Retrieve LTP and OHLC of upto 200 instruments
        /// </summary>
        /// <param name="InstrumentId">Indentification of instrument in the form of EXCHANGE:TRADINGSYMBOL (eg: NSE:INFY) or InstrumentToken (eg: 408065)</param>
        /// <returns>Dictionary of all OHLC objects with keys as in InstrumentId</returns>
        public Dictionary<string, OHLC> GetOHLC(string[] InstrumentId)
        {
            var param = new Dictionary<string, dynamic>();
            param.Add("i", InstrumentId);
            Dictionary<string, dynamic> ohlcData = Get(Routes.Market.OHLC, param)["data"];

            Dictionary<string, OHLC> ohlcs = new Dictionary<string, OHLC>();
            foreach (string item in ohlcData.Keys)
                ohlcs.Add(item, new OHLC(ohlcData[item]));

            return ohlcs;
        }

        /// <summary>
        /// Retrieve LTP of upto 200 instruments
        /// </summary>
        /// <param name="InstrumentId">Indentification of instrument in the form of EXCHANGE:TRADINGSYMBOL (eg: NSE:INFY) or InstrumentToken (eg: 408065)</param>
        /// <returns>Dictionary with InstrumentId as key and LTP as value.</returns>
        public Dictionary<string, LTP> GetLTP(string[] InstrumentId)
        {
            var param = new Dictionary<string, dynamic>();
            param.Add("i", InstrumentId);
            Dictionary<string, dynamic> ltpData = Get(Routes.Market.LTP, param)["data"];

            Dictionary<string, LTP> ltps = new Dictionary<string, LTP>();
            foreach (string item in ltpData.Keys)
                ltps.Add(item, new LTP(ltpData[item]));

            return ltps;
        }

        /// <summary>
        /// Retrieve historical data (candles) for an instrument.
        /// </summary>
        /// <param name="InstrumentToken">Identifier for the instrument whose historical records you want to fetch. This is obtained with the instrument list API.</param>
        /// <param name="FromDate">Date in format yyyy-MM-dd for fetching candles between two days. Date in format yyyy-MM-dd hh:mm:ss for fetching candles between two timestamps.</param>
        /// <param name="ToDate">Date in format yyyy-MM-dd for fetching candles between two days. Date in format yyyy-MM-dd hh:mm:ss for fetching candles between two timestamps.</param>
        /// <param name="Interval">The candle record interval. Possible values are: minute, day, 3minute, 5minute, 10minute, 15minute, 30minute, 60minute</param>
        /// <param name="Continuous">Pass true to get continous data of expired instruments.</param>
        /// <param name="OI">Pass true to get open interest data.</param>
        /// <returns>List of Historical objects.</returns>
        public List<Historical> GetHistoricalData(
            string InstrumentToken,
            DateTime FromDate,
            DateTime ToDate,
            string Interval,
            bool Continuous = false,
            bool OI = false)
        {
            var param = new Dictionary<string, dynamic>();

            param.Add("instrument_token", InstrumentToken);
            param.Add("from", FromDate.ToString("yyyy-MM-dd HH:mm:ss"));
            param.Add("to", ToDate.ToString("yyyy-MM-dd HH:mm:ss"));
            param.Add("interval", Interval);
            param.Add("continuous", Continuous ? "1" : "0");
            param.Add("oi", OI ? "1" : "0");

            var historicalData = Get(Routes.Market.Historical, param);

            List<Historical> historicals = new List<Historical>();

            foreach (ArrayList item in historicalData["data"]["candles"])
                historicals.Add(new Historical(item));

            return historicals;
        }

        /// <summary>
        /// Retrieve the buy/sell trigger range for Cover Orders.
        /// </summary>
        /// <param name="InstrumentId">Indentification of instrument in the form of EXCHANGE:TRADINGSYMBOL (eg: NSE:INFY) or InstrumentToken (eg: 408065)</param>
        /// <param name="TrasactionType">BUY or SELL</param>
        /// <returns>List of trigger ranges for given instrument ids for given transaction type.</returns>
        public Dictionary<string, TrigerRange> GetTriggerRange(string[] InstrumentId, string TrasactionType)
        {
            var param = new Dictionary<string, dynamic>();

            param.Add("i", InstrumentId);
            param.Add("transaction_type", TrasactionType.ToLower());

            var triggerdata = Get(Routes.Market.TriggerRange, param)["data"];

            Dictionary<string, TrigerRange> triggerRanges = new Dictionary<string, TrigerRange>();
            foreach (string item in triggerdata.Keys)
                triggerRanges.Add(item, new TrigerRange(triggerdata[item]));

            return triggerRanges;
        }

        #region GTT

        /// <summary>
        /// Retrieve the list of GTTs.
        /// </summary>
        /// <returns>List of GTTs.</returns>
        public List<GTT> GetGTTs()
        {
            var gttsdata = Get(Routes.GTT.AllGTTs);

            List<GTT> gtts = new List<GTT>();

            foreach (Dictionary<string, dynamic> item in gttsdata["data"])
                gtts.Add(new GTT(item));

            return gtts;
        }


        /// <summary>
        /// Retrieve a single GTT
        /// </summary>
        /// <param name="GTTId">Id of the GTT</param>
        /// <returns>GTT info</returns>
        public GTT GetGTT(int GTTId)
        {
            var param = new Dictionary<string, dynamic>();
            param.Add("id", GTTId.ToString());

            var gttdata = Get(Routes.GTT.Info, param);

            return new GTT(gttdata["data"]);
        }

        /// <summary>
        /// Place a GTT order
        /// </summary>
        /// <param name="gttParams">Contains the parameters for the GTT order</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> PlaceGTT(GTTParams gttParams)
        {
            var condition = new Dictionary<string, dynamic>();
            condition.Add("exchange", gttParams.Exchange);
            condition.Add("tradingsymbol", gttParams.TradingSymbol);
            condition.Add("trigger_values", gttParams.TriggerPrices);
            condition.Add("last_price", gttParams.LastPrice);
            condition.Add("instrument_token", gttParams.InstrumentToken);

            var ordersParam = new List<Dictionary<string, dynamic>>();
            foreach (var o in gttParams.Orders)
            {
                var order = new Dictionary<string, dynamic>();
                order["exchange"] = gttParams.Exchange;
                order["tradingsymbol"] = gttParams.TradingSymbol;
                order["transaction_type"] = o.TransactionType;
                order["quantity"] = o.Quantity;
                order["price"] = o.Price;
                order["order_type"] = o.OrderType;
                order["product"] = o.Product;
                ordersParam.Add(order);
            }

            var parms = new Dictionary<string, dynamic>();
            parms.Add("condition", Utils.JsonSerialize(condition));
            parms.Add("orders", Utils.JsonSerialize(ordersParam));
            parms.Add("type", gttParams.TriggerType);

            return Post(Routes.GTT.Place, parms);
        }

        /// <summary>
        /// Modify a GTT order
        /// </summary>
        /// <param name="GTTId">Id of the GTT to be modified</param>
        /// <param name="gttParams">Contains the parameters for the GTT order</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> ModifyGTT(int GTTId, GTTParams gttParams)
        {
            var condition = new Dictionary<string, dynamic>();
            condition.Add("exchange", gttParams.Exchange);
            condition.Add("tradingsymbol", gttParams.TradingSymbol);
            condition.Add("trigger_values", gttParams.TriggerPrices);
            condition.Add("last_price", gttParams.LastPrice);
            condition.Add("instrument_token", gttParams.InstrumentToken);

            var ordersParam = new List<Dictionary<string, dynamic>>();
            foreach (var o in gttParams.Orders)
            {
                var order = new Dictionary<string, dynamic>();
                order["exchange"] = gttParams.Exchange;
                order["tradingsymbol"] = gttParams.TradingSymbol;
                order["transaction_type"] = o.TransactionType;
                order["quantity"] = o.Quantity;
                order["price"] = o.Price;
                order["order_type"] = o.OrderType;
                order["product"] = o.Product;
                ordersParam.Add(order);
            }

            var parms = new Dictionary<string, dynamic>();
            parms.Add("condition", Utils.JsonSerialize(condition));
            parms.Add("orders", Utils.JsonSerialize(ordersParam));
            parms.Add("type", gttParams.TriggerType);
            parms.Add("id", GTTId.ToString());

            return Put(Routes.GTT.Modify, parms);
        }

        /// <summary>
        /// Cancel a GTT order
        /// </summary>
        /// <param name="GTTId">Id of the GTT to be modified</param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Dictionary<string, dynamic> CancelGTT(int GTTId)
        {
            var parms = new Dictionary<string, dynamic>();
            parms.Add("id", GTTId.ToString());

            return Delete(Routes.GTT.Delete, parms);
        }

        #endregion GTT


        #region MF Calls

        /// <summary>
        /// Gets the Mutual funds Instruments.
        /// </summary>
        /// <returns>The Mutual funds Instruments.</returns>
        public List<MFInstrument> GetMFInstruments()
        {
            var param = new Dictionary<string, dynamic>();

            List<Dictionary<string, dynamic>> instrumentsData;

            instrumentsData = Get(Routes.MutualFunds.Instruments, param);

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
            ordersData = Get(Routes.MutualFunds.Orders, param);

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
        public MFOrder GetMFOrders(string OrderId)
        {
            var param = new Dictionary<string, dynamic>();
            param.Add("order_id", OrderId);

            Dictionary<string, dynamic> orderData;
            orderData = Get(Routes.MutualFunds.Order, param);

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
        public Dictionary<string, dynamic> PlaceMFOrder(
            string TradingSymbol,
            string TransactionType,
            decimal? Amount,
            decimal? Quantity = null,
            string Tag = "")
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "tradingsymbol", TradingSymbol);
            Utils.AddIfNotNull(param, "transaction_type", TransactionType);
            Utils.AddIfNotNull(param, "amount", Amount.ToString());
            Utils.AddIfNotNull(param, "quantity", Quantity.ToString());
            Utils.AddIfNotNull(param, "tag", Tag);

            return Post(Routes.MutualFunds.PlaceOrder, param);
        }

        /// <summary>
        /// Cancels the Mutual funds order.
        /// </summary>
        /// <returns>JSON response as nested string dictionary.</returns>
        /// <param name="OrderId">Unique order id.</param>
        public Dictionary<string, dynamic> CancelMFOrder(string OrderId)
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "order_id", OrderId);

            return Delete(Routes.MutualFunds.CancelOrder, param);
        }

        /// <summary>
        /// Gets all Mutual funds SIPs.
        /// </summary>
        /// <returns>The list of all Mutual funds SIPs.</returns>
        public List<MFSIP> GetMFSIPs()
        {
            var param = new Dictionary<string, dynamic>();

            Dictionary<string, dynamic> sipData;
            sipData = Get(Routes.MutualFunds.SIPs, param);

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
        public MFSIP GetMFSIPs(string SIPID)
        {
            var param = new Dictionary<string, dynamic>();
            param.Add("sip_id", SIPID);

            Dictionary<string, dynamic> sipData;
            sipData = Get(Routes.MutualFunds.SIP, param);

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
        public Dictionary<string, dynamic> PlaceMFSIP(
            string TradingSymbol,
            decimal? Amount,
            decimal? InitialAmount,
            string Frequency,
            int? InstalmentDay,
            int? Instalments,
            string Tag = "")
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "tradingsymbol", TradingSymbol);
            Utils.AddIfNotNull(param, "initial_amount", InitialAmount.ToString());
            Utils.AddIfNotNull(param, "amount", Amount.ToString());
            Utils.AddIfNotNull(param, "frequency", Frequency);
            Utils.AddIfNotNull(param, "instalment_day", InstalmentDay.ToString());
            Utils.AddIfNotNull(param, "instalments", Instalments.ToString());

            return Post(Routes.MutualFunds.PlaceSIP, param);
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
        public Dictionary<string, dynamic> ModifyMFSIP(
            string SIPId,
            decimal? Amount,
            string Frequency,
            int? InstalmentDay,
            int? Instalments,
            string Status)
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "status", Status);
            Utils.AddIfNotNull(param, "sip_id", SIPId);
            Utils.AddIfNotNull(param, "amount", Amount.ToString());
            Utils.AddIfNotNull(param, "frequency", Frequency.ToString());
            Utils.AddIfNotNull(param, "instalment_day", InstalmentDay.ToString());
            Utils.AddIfNotNull(param, "instalments", Instalments.ToString());

            return Put(Routes.MutualFunds.ModifySIP, param);
        }

        /// <summary>
        /// Cancels the Mutual funds SIP.
        /// </summary>
        /// <returns>JSON response as nested string dictionary.</returns>
        /// <param name="SIPId">SIP id.</param>
		public Dictionary<string, dynamic> CancelMFSIP(string SIPId)
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "sip_id", SIPId);

            return Delete(Routes.MutualFunds.CancelSIP, param);
        }

        /// <summary>
        /// Gets the Mutual funds holdings.
        /// </summary>
        /// <returns>The list of all Mutual funds holdings.</returns>
        public List<MFHolding> GetMFHoldings()
        {
            var param = new Dictionary<string, dynamic>();

            Dictionary<string, dynamic> holdingsData;
            holdingsData = Get(Routes.MutualFunds.Holdings, param);

            List<MFHolding> holdingslist = new List<MFHolding>();

            foreach (Dictionary<string, dynamic> item in holdingsData["data"])
                holdingslist.Add(new MFHolding(item));

            return holdingslist;
        }

        #endregion

        #region HTTP Functions

        /// <summary>
        /// Alias for sending a GET request.
        /// </summary>
        /// <param name="Route">URL route of API</param>
        /// <param name="Params">Additional paramerters</param>
        /// <returns>Varies according to API endpoint</returns>
        private dynamic Get(string Route, Dictionary<string, dynamic> Params = null, Dictionary<string, dynamic> QueryParams = null)
        {
            return Request(Route, "GET", Params, QueryParams);
        }

        /// <summary>
        /// Alias for sending a POST request.
        /// </summary>
        /// <param name="Route">URL route of API</param>
        /// <param name="Params">Additional paramerters</param>
        /// <returns>Varies according to API endpoint</returns>
        private dynamic Post(string Route, dynamic Params = null, Dictionary<string, dynamic> QueryParams = null, bool json = false)
        {
            return Request(Route, "POST", Params, QueryParams: QueryParams, json: json);
        }

        /// <summary>
        /// Alias for sending a PUT request.
        /// </summary>
        /// <param name="Route">URL route of API</param>
        /// <param name="Params">Additional paramerters</param>
        /// <returns>Varies according to API endpoint</returns>
        private dynamic Put(string Route, dynamic Params = null)
        {
            return Request(Route, "PUT", Params);
        }

        /// <summary>
        /// Alias for sending a DELETE request.
        /// </summary>
        /// <param name="Route">URL route of API</param>
        /// <param name="Params">Additional paramerters</param>
        /// <returns>Varies according to API endpoint</returns>
        private dynamic Delete(string Route, dynamic Params = null)
        {
            return Request(Route, "DELETE", Params);
        }

        /// <summary>
        /// Adds extra headers to request
        /// </summary>
        /// <param name="Req">Request object to add headers</param>
        private void AddExtraHeaders(ref HttpRequestMessage Req)
        {
            var KiteAssembly = Assembly.GetAssembly(typeof(Kite));
            if (KiteAssembly != null)
            {
                Req.Headers.UserAgent.TryParseAdd("KiteConnect.Net/" + KiteAssembly.GetName().Version);
            }

            Req.Headers.Add("X-Kite-Version", "3");
            Req.Headers.Add("Authorization", "token " + _apiKey + ":" + _accessToken);

            if (_enableLogging)
            {
                foreach (var header in Req.Headers)
                {
                    Console.WriteLine("DEBUG: " + header.Key + ": " + string.Join(",", header.Value.ToArray()));
                }
            }
        }

        /// <summary>
        /// Make an HTTP request.
        /// </summary>
        /// <param name="Route">URL route of API</param>
        /// <param name="Method">Method of HTTP request</param>
        /// <param name="Params">Additional paramerters. Can be dictionary, list etc.</param>
        /// <returns>Varies according to API endpoint</returns>
        private dynamic Request(string Route, string Method, dynamic Params = null, Dictionary<string, dynamic> QueryParams = null, bool json = false)
        {
            string route = _root + Route;

            if (Params is null)
                Params = new Dictionary<string, dynamic>();

            if (QueryParams is null)
                QueryParams = new Dictionary<string, dynamic>();

            if (route.Contains("{") && !json)
            {
                var routeParams = (Params as Dictionary<string, dynamic>).ToDictionary(entry => entry.Key, entry => entry.Value);

                foreach (KeyValuePair<string, dynamic> item in routeParams)
                    if (route.Contains("{" + item.Key + "}"))
                    {
                        route = route.Replace("{" + item.Key + "}", (string)item.Value);
                        Params.Remove(item.Key);
                    }
            }

            HttpRequestMessage request = new();

            if (Method == "POST" || Method == "PUT")
            {
                string url = route;
                if (QueryParams.Count > 0)
                {
                    url += "?" + string.Join("&", QueryParams.Select(x => Utils.BuildParam(x.Key, x.Value)));
                }

                string requestBody = "";
                if (json)
                    requestBody = Utils.JsonSerialize(Params);
                else
                    requestBody = string.Join("&", (Params as Dictionary<string, dynamic>).Select(x => Utils.BuildParam(x.Key, x.Value)));

                request.RequestUri = new Uri(url);
                request.Method = new HttpMethod(Method);
                AddExtraHeaders(ref request);

                if (_enableLogging) Console.WriteLine("DEBUG: " + Method + " " + url + "\n" + requestBody);

                request.Content = new StringContent(requestBody, Encoding.UTF8, json ? "application/json" : "application/x-www-form-urlencoded");
            }
            else
            {
                string url = route;
                Dictionary<string, dynamic> allParams = new Dictionary<string, dynamic>();
                // merge both params
                foreach (KeyValuePair<string, dynamic> item in QueryParams)
                {
                    allParams[item.Key] = item.Value;
                }
                foreach (KeyValuePair<string, dynamic> item in Params)
                {
                    allParams[item.Key] = item.Value;
                }
                // build final url
                if (allParams.Count > 0)
                {
                    url += "?" + string.Join("&", allParams.Select(x => Utils.BuildParam(x.Key, x.Value)));
                }

                request.RequestUri = new Uri(url);
                request.Method = new HttpMethod(Method);
                if (_enableLogging) Console.WriteLine("DEBUG: " + Method + " " + url);
                AddExtraHeaders(ref request);
            }

            HttpResponseMessage response = httpClient.Send(request);
            HttpStatusCode status = response.StatusCode;

            string responseBody = response.Content.ReadAsStringAsync().Result;
            if (_enableLogging) Console.WriteLine("DEBUG: " + ((int)status) + " " + responseBody + "\n");

            if (response.Content.Headers.ContentType.MediaType == MediaTypeNames.Application.Json)
            {
                Dictionary<string, dynamic> responseDictionary = Utils.JsonDeserialize(responseBody);

                if (status != HttpStatusCode.OK)
                {
                    string errorType = "GeneralException";
                    string message = "";

                    if (responseDictionary.ContainsKey("error_type"))
                        errorType = responseDictionary["error_type"];

                    if (responseDictionary.ContainsKey("message"))
                        message = responseDictionary["message"];

                    switch (errorType)
                    {
                        case "GeneralException": throw new GeneralException(message, status);
                        case "TokenException":
                            {
                                _sessionHook?.Invoke();
                                throw new TokenException(message, status);
                            }
                        case "PermissionException": throw new PermissionException(message, status);
                        case "OrderException": throw new OrderException(message, status);
                        case "InputException": throw new InputException(message, status);
                        case "DataException": throw new DataException(message, status);
                        case "NetworkException": throw new NetworkException(message, status);
                        default: throw new GeneralException(message, status);
                    }
                }

                return responseDictionary;
            }
            else if (response.Content.Headers.ContentType.MediaType == "text/csv")
                return Utils.ParseCSV(responseBody);
            else
                throw new DataException("Unexpected content type " + response.Content.Headers.ContentType.MediaType + " " + response);

        }

        private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
        {
            string url = _root + path;
            var httpResponse = await httpClient.GetAsync(url, cancellationToken);
            if (httpResponse.IsSuccessStatusCode)
            {
                var response = await httpResponse.Content.ReadFromJsonAsync<SucessResponse<T>>(jsonSerializerOptions, cancellationToken);
                if (response.Status == ResponseStatus.Success)
                    return response.Data;
                else
                    throw new KiteException($"Expected sucess status, got {response.Status}.", httpResponse.StatusCode);
            }
            else
            {
                await ThrowErrorAsync(httpResponse, cancellationToken);
                throw new KiteException("Something went wrong.", httpResponse.StatusCode);
            }
        }

        private async Task ThrowErrorAsync(HttpResponseMessage httpResponse, CancellationToken cancellationToken)
        {
            var response = await httpResponse.Content.ReadFromJsonAsync<ErrorResponse>(jsonSerializerOptions, cancellationToken);
            switch (response.ErrorType)
            {
                case "GeneralException": throw new GeneralException(response.Message, httpResponse.StatusCode);
                case "TokenException":
                    {
                        _sessionHook?.Invoke();
                        throw new TokenException(response.Message, httpResponse.StatusCode);
                    }
                case "PermissionException": throw new PermissionException(response.Message, httpResponse.StatusCode);
                case "OrderException": throw new OrderException(response.Message, httpResponse.StatusCode);
                case "InputException": throw new InputException(response.Message, httpResponse.StatusCode);
                case "DataException": throw new DataException(response.Message, httpResponse.StatusCode);
                case "NetworkException": throw new NetworkException(response.Message, httpResponse.StatusCode);
                default: throw new GeneralException(response.Message, httpResponse.StatusCode);
            }
        }

        #endregion

    }
}
