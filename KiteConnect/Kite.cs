using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace KiteConnect
{
    /// <summary>
    /// The API client class. In production, you may initialize a single instance of this class per `APIKey`.
    /// </summary>
    public class Kite
    {
        internal static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, Converters = { new JsonStringEnumConverter(), new CustomDateTimeJsonConverter(), new CandleJsonConverter() } };
        // Default root API endpoint. It's possible to
        // override this by passing the `Root` parameter during initialisation.
        private readonly string _root = "https://api.kite.trade";
        private readonly string _login = "https://kite.zerodha.com/connect/login";
        private readonly int _timeout;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        private string _accessToken;
        private bool _enableLogging;
        private Action _sessionHook;


        /// <summary>
        /// Initialize a new Kite Connect client instance.
        /// </summary>
        /// <param name="apiKey">API Key issued to you</param>
        /// <param name="accessToken">The token obtained after the login flow in exchange for the `RequestToken` . 
        /// Pre-login, this will default to None,but once you have obtained it, you should persist it in a database or session to pass 
        /// to the Kite Connect class initialisation for subsequent requests.</param>
        /// <param name="root">API end point root. Unless you explicitly want to send API requests to a non-default endpoint, this can be ignored.</param>
        /// <param name="debug">If set to True, will serialise and print requests and responses to stdout.</param>
        /// <param name="timeout">Time in milliseconds for which  the API client will wait for a request to complete before it fails</param>
        /// <param name="proxy">To set proxy for http request. Should be an object of WebProxy.</param>
        /// <param name="pool">Number of connections to server. Client will reuse the connections if they are alive.</param>
        public Kite(string apiKey, string accessToken = null, string root = null, bool debug = false, int timeout = 7000, IWebProxy proxy = null, int pool = 2)
        {
            _accessToken = accessToken;
            _apiKey = apiKey;
            if (!string.IsNullOrEmpty(root))
                _root = root;
            _enableLogging = debug;

            _timeout = timeout;

            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                Proxy = proxy,
            };
            _httpClient = new(httpClientHandler)
            {
                BaseAddress = new Uri(_root),
                Timeout = TimeSpan.FromMilliseconds(timeout),
            };

            ServicePointManager.DefaultConnectionLimit = pool;
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
        /// <param name="method">Action to be invoked when session becomes invalid.</param>
        public void SetSessionExpiryHook(Action method)
        {
            _sessionHook = method;
        }

        /// <summary>
        /// Set the `AccessToken` received after a successful authentication.
        /// </summary>
        /// <param name="accessToken">Access token for the session.</param>
        public void SetAccessToken(string accessToken)
        {
            _accessToken = accessToken;
        }

        /// <summary>
        /// Get the remote login url to which a user should be redirected to initiate the login flow.
        /// </summary>
        /// <returns>Login url to authenticate the user.</returns>
        public string GetLoginUrl()
        {
            return string.Format("{0}?api_key={1}&v=3", _login, _apiKey);
        }

        /// <summary>
        /// Do the token exchange with the `RequestToken` obtained after the login flow,
        /// and retrieve the `AccessToken` required for all subsequent requests.The
        /// response contains not just the `AccessToken`, but metadata for
        /// the user who has authenticated.
        /// </summary>
        /// <param name="requestToken">Token obtained from the GET paramers after a successful login redirect.</param>
        /// <param name="appSecret">API secret issued with the API key.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>User structure with tokens and profile data</returns>
        public Task<User> GenerateSessionAsync(string requestToken, string appSecret, CancellationToken cancellationToken = default)
        {
            string checksum = Utils.SHA256Hash(_apiKey + requestToken + appSecret);

            var formDataBuilder = new ParametersBuilder()
                .Add("api_key", _apiKey)
                .Add("request_token", requestToken)
                .Add("checksum", checksum);

            return PostAsync<User>("/session/token", formData: formDataBuilder.Build(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Kill the session by invalidating the access token
        /// </summary>
        /// <param name="accessToken">Access token to invalidate. Default is the active access token.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Task<bool> InvalidateAccessTokenAsync(string accessToken = null, CancellationToken cancellationToken = default)
        {
            var queryParametersBuilder = new ParametersBuilder()
                .AddIfNotNull("api_key", _apiKey)
                .AddIfNotNull("access_token", accessToken);

            return DeleteAsync<bool>("/session/token", queryParameters: queryParametersBuilder.Build(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Invalidates RefreshToken
        /// </summary>
        /// <param name="refreshToken">RefreshToken to invalidate</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Task<bool> InvalidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var queryParametersBuilder = new ParametersBuilder()
                .AddIfNotNull("api_key", _apiKey)
                .AddIfNotNull("refresh_token", refreshToken);

            return DeleteAsync<bool>("/session/token", queryParameters: queryParametersBuilder.Build(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Renew AccessToken using RefreshToken
        /// </summary>
        /// <param name="refreshToken">RefreshToken to renew the AccessToken.</param>
        /// <param name="appSecret">API secret issued with the API key.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>TokenRenewResponse that contains new AccessToken and RefreshToken.</returns>
        public Task<TokenSet> RenewAccessTokenAsync(string refreshToken, string appSecret, CancellationToken cancellationToken = default)
        {
            string checksum = Utils.SHA256Hash(_apiKey + refreshToken + appSecret);
            var formDataBuilder = new ParametersBuilder()
                .AddIfNotNull("api_key", _apiKey)
                .AddIfNotNull("refresh_token", refreshToken)
                .AddIfNotNull("checksum", checksum);

            return PostAsync<TokenSet>("/session/refresh_token", formData: formDataBuilder.Build(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets currently logged in user details
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>User profile</returns>
        public Task<Profile> GetProfileAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<Profile>("/user/profile", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// A virtual contract provides detailed charges order-wise for brokerage, STT, stamp duty, exchange transaction charges, SEBI turnover charge, and GST.
        /// </summary>
        /// <param name="contractNoteParams">List of all order params to get contract notes for</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of contract notes for the params</returns>
        public Task<List<ContractNote>> GetVirtualContractNoteAsync(List<ContractNoteParams> contractNoteParams, CancellationToken cancellationToken = default)
        {
            return PostJsonAsync<List<ContractNoteParams>, List<ContractNote>>("/charges/orders", jsonData: contractNoteParams, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Margin data for a specific order
        /// </summary>
        /// <param name="orderMarginParams">List of all order params to get margins for</param>
        /// <param name="mode">Mode of the returned response content. Eg: Constants.Margin.Mode.Compact</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of margins of order</returns>
        public Task<List<OrderMargin>> GetOrderMarginsAsync(List<OrderMarginParams> orderMarginParams, string mode = null, CancellationToken cancellationToken = default)
        {
            var queryParameterBuilder = new ParametersBuilder()
                .AddIfNotNull("mode", mode);

            return PostJsonAsync<List<OrderMarginParams>, List<OrderMargin>>("/margins/orders", queryParameters: queryParameterBuilder.Build(), jsonData: orderMarginParams, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Margin data for a basket orders
        /// </summary>
        /// <param name="orderMarginParams">List of all order params to get margins for</param>
        /// <param name="considerPositions">Consider users positions while calculating margins</param>
        /// <param name="mode">Mode of the returned response content. Eg: Constants.Margin.Mode.Compact</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of margins of order</returns>
        public Task<BasketMargin> GetBasketMarginsAsync(List<OrderMarginParams> orderMarginParams, bool considerPositions = true, string mode = null, CancellationToken cancellationToken = default)
        {
            var queryParameterBuilder = new ParametersBuilder()
                .Add("consider_positions", considerPositions)
                .AddIfNotNull("mode", mode);

            return PostJsonAsync<List<OrderMarginParams>, BasketMargin>("/margins/basket", queryParameters: queryParameterBuilder.Build(), jsonData: orderMarginParams, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get account balance and cash margin details for all segments.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>User margin response with both equity and commodity margins.</returns>
        public Task<UserMarginsResponse> GetMarginsAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<UserMarginsResponse>("/user/margins", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get account balance and cash margin details for a particular segment.
        /// </summary>
        /// <param name="segment">Trading segment (eg: equity or commodity)</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Margins for specified segment.</returns>
        public Task<UserMargin> GetMarginsAsync(string segment, CancellationToken cancellationToken = default)
        {
            return GetAsync<UserMargin>($"/user/margins/{segment}", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Place an order
        /// </summary>
        /// <param name="exchange">Name of the exchange</param>
        /// <param name="tradingSymbol">Tradingsymbol of the instrument</param>
        /// <param name="transactionType">BUY or SELL</param>
        /// <param name="quantity">Quantity to transact</param>
        /// <param name="price">For LIMIT orders</param>
        /// <param name="product">Margin product applied to the order (margin is blocked based on this)</param>
        /// <param name="orderType">Order type (MARKET, LIMIT etc.)</param>
        /// <param name="validity">Order validity (DAY, IOC and TTL)</param>
        /// <param name="disclosedQuantity">Quantity to disclose publicly (for equity trades)</param>
        /// <param name="triggerPrice">For SL, SL-M etc.</param>
        /// <param name="squareOffValue">Price difference at which the order should be squared off and profit booked (eg: Order price is 100. Profit target is 102. So squareoff = 2)</param>
        /// <param name="stoplossValue">Stoploss difference at which the order should be squared off (eg: Order price is 100. Stoploss target is 98. So stoploss = 2)</param>
        /// <param name="trailingStoploss">Incremental value by which stoploss price changes when market moves in your favor by the same incremental value from the entry price (optional)</param>
        /// <param name="variety">You can place orders of varieties; regular orders, after market orders, cover orders, iceberg orders etc. </param>
        /// <param name="tag">An optional tag to apply to an order to identify it (alphanumeric, max 20 chars)</param>
        /// <param name="validityTtl">Order life span in minutes for TTL validity orders</param>
        /// <param name="icebergLegs">Total number of legs for iceberg order type (number of legs per Iceberg should be between 2 and 10)</param>
        /// <param name="icebergQuantity">Split quantity for each iceberg leg order (Quantity/IcebergLegs)</param>
        /// <param name="auctionNumber">A unique identifier for a particular auction</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Task<OrderResponse> PlaceOrderAsync(
            string exchange,
            string tradingSymbol,
            string transactionType,
            decimal quantity,
            decimal? price = null,
            string product = null,
            string orderType = null,
            string validity = null,
            decimal? disclosedQuantity = null,
            decimal? triggerPrice = null,
            decimal? squareOffValue = null,
            decimal? stoplossValue = null,
            decimal? trailingStoploss = null,
            string variety = Constants.Variety.Regular,
            string tag = "",
            int? validityTtl = null,
            int? icebergLegs = null,
            decimal? icebergQuantity = null,
            string auctionNumber = null,
            CancellationToken cancellationToken = default
            )
        {
            var formDataBuilder = new ParametersBuilder()
                .AddIfNotNull("exchange", exchange)
                .AddIfNotNull("tradingsymbol", tradingSymbol)
                .AddIfNotNull("transaction_type", transactionType)
                .Add("quantity", quantity)
                .AddIfNotNull("price", price)
                .AddIfNotNull("product", product)
                .AddIfNotNull("order_type", orderType)
                .AddIfNotNull("validity", validity)
                .AddIfNotNull("disclosed_quantity", disclosedQuantity)
                .AddIfNotNull("trigger_price", triggerPrice)
                .AddIfNotNull("squareoff", squareOffValue)
                .AddIfNotNull("stoploss", stoplossValue)
                .AddIfNotNull("trailing_stoploss", trailingStoploss)
                .AddIfNotNull("variety", variety)
                .AddIfNotNull("tag", tag)
                .AddIfNotNull("validity_ttl", validityTtl)
                .AddIfNotNull("iceberg_legs", icebergLegs)
                .AddIfNotNull("iceberg_quantity", icebergQuantity)
                .AddIfNotNull("auction_number", auctionNumber);

            return PostAsync<OrderResponse>($"/orders/{variety}", formData: formDataBuilder.Build(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Modify an open order.
        /// </summary>
        /// <param name="orderId">Id of the order to be modified</param>
        /// <param name="parentOrderId">Id of the parent order (obtained from the /orders call) as BO is a multi-legged order</param>
        /// <param name="exchange">Name of the exchange</param>
        /// <param name="tradingSymbol">Tradingsymbol of the instrument</param>
        /// <param name="transactionType">BUY or SELL</param>
        /// <param name="quantity">Quantity to transact</param>
        /// <param name="price">For LIMIT orders</param>
        /// <param name="product">Margin product applied to the order (margin is blocked based on this)</param>
        /// <param name="orderType">Order type (MARKET, LIMIT etc.)</param>
        /// <param name="validity">Order validity</param>
        /// <param name="disclosedQuantity">Quantity to disclose publicly (for equity trades)</param>
        /// <param name="triggerPrice">For SL, SL-M etc.</param>
        /// <param name="variety">You can place orders of varieties; regular orders, after market orders, cover orders etc. </param>
        /// <param name="cancellationToken"></param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Task<OrderResponse> ModifyOrderAsync(
            string orderId,
            string parentOrderId = null,
            string exchange = null,
            string tradingSymbol = null,
            string transactionType = null,
            decimal? quantity = null,
            decimal? price = null,
            string product = null,
            string orderType = null,
            string validity = Constants.Validity.Day,
            decimal? disclosedQuantity = null,
            decimal? triggerPrice = null,
            string variety = Constants.Variety.Regular,
            CancellationToken cancellationToken = default)
        {
            var formDataBuilder = new ParametersBuilder();

            string VarietyString = variety;
            string ProductString = product;

            if ((ProductString == "bo" || ProductString == "co") && VarietyString != ProductString)
                throw new Exception(string.Format("Invalid variety. It should be: {0}", ProductString));

            formDataBuilder.AddIfNotNull("order_id", orderId)
                .AddIfNotNull("parent_order_id", parentOrderId)
                .AddIfNotNull("trigger_price", triggerPrice)
                .AddIfNotNull("variety", variety);

            if (VarietyString == "bo" && ProductString == "bo")
            {
                formDataBuilder.AddIfNotNull("quantity", quantity)
                    .AddIfNotNull("price", price)
                    .AddIfNotNull("disclosed_quantity", disclosedQuantity);
            }
            else if (VarietyString != "co" && ProductString != "co")
            {
                formDataBuilder.AddIfNotNull("exchange", exchange)
                    .AddIfNotNull("tradingsymbol", tradingSymbol)
                    .AddIfNotNull("transaction_type", transactionType)
                    .AddIfNotNull("quantity", quantity)
                    .AddIfNotNull("price", price)
                    .AddIfNotNull("product", product)
                    .AddIfNotNull("order_type", orderType)
                    .AddIfNotNull("validity", validity)
                    .AddIfNotNull("disclosed_quantity", disclosedQuantity);
            }

            return PutAsync<OrderResponse>($"/orders/{variety}/{orderId}", formData: formDataBuilder.Build(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        /// <param name="orderId">Id of the order to be cancelled</param>
        /// <param name="variety">You can place orders of varieties; regular orders, after market orders, cover orders etc. </param>
        /// <param name="parentOrderId">Id of the parent order (obtained from the /orders call) as BO is a multi-legged order</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Task<OrderResponse> CancelOrderAsync(string orderId, string variety = Constants.Variety.Regular, string parentOrderId = null, CancellationToken cancellationToken = default)
        {
            var queryParametersBuilder = new ParametersBuilder()
                .AddIfNotNull("parent_order_id", parentOrderId);//TODO undocumented parameter

            return DeleteAsync<OrderResponse>($"/orders/{variety}/{orderId}", queryParameters: queryParametersBuilder.Build(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets the collection of orders from the orderbook.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>List of orders.</returns>
        public Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<List<Order>>("/orders", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets information about given OrderId.
        /// </summary>
        /// <param name="orderId">Unique order id</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of order objects.</returns>
        public Task<List<Order>> GetOrderHistoryAsync(string orderId, CancellationToken cancellationToken = default)
        {
            return GetAsync<List<Order>>($"/orders/{orderId}", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retreive the list of trades executed (all or ones under a particular order).
        /// An order can be executed in tranches based on market conditions.
        /// These trades are individually recorded under an order.
        /// </summary>
        /// <param name="orderId">is the ID of the order (optional) whose trades are to be retrieved. If no `OrderId` is specified, all trades for the day are returned.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of trades of given order.</returns>
        public Task<List<Trade>> GetOrderTradesAsync(string orderId = null, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(orderId))
                return GetAsync<List<Trade>>($"/orders/{orderId}/trades", cancellationToken: cancellationToken);
            else
                return GetAsync<List<Trade>>("/trades", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieve the list of positions.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Day and net positions.</returns>
        public Task<PositionResponse> GetPositionsAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<PositionResponse>("/portfolio/positions", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieve the list of equity holdings.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>List of holdings.</returns>
        public Task<List<Holding>> GetHoldingsAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<List<Holding>>("/portfolio/holdings", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieve the list of auction instruments.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>List of auction instruments.</returns>
        public Task<List<AuctionInstrument>> GetAuctionInstrumentsAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<List<AuctionInstrument>>("/portfolio/holdings/auctions", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Modify an open position's product type.
        /// </summary>
        /// <param name="exchange">Name of the exchange</param>
        /// <param name="tradingSymbol">Tradingsymbol of the instrument</param>
        /// <param name="transactionType">BUY or SELL</param>
        /// <param name="positionType">overnight or day</param>
        /// <param name="quantity">Quantity to convert</param>
        /// <param name="oldProduct">Existing margin product of the position</param>
        /// <param name="newProduct">Margin product to convert to</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Task<bool> ConvertPositionAsync(
            string exchange,
            string tradingSymbol,
            string transactionType,
            string positionType,
            decimal? quantity,
            string oldProduct,
            string newProduct,
            CancellationToken cancellationToken = default)
        {
            var formDataBuilder = new ParametersBuilder()
                .AddIfNotNull("exchange", exchange)
                .AddIfNotNull("tradingsymbol", tradingSymbol)
                .AddIfNotNull("transaction_type", transactionType)
                .AddIfNotNull("position_type", positionType)
                .AddIfNotNull("quantity", quantity)
                .AddIfNotNull("old_product", oldProduct)
                .AddIfNotNull("new_product", newProduct);

            return PutAsync<bool>("/portfolio/positions", formData: formDataBuilder.Build(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieve the list of market instruments available to trade.
        /// Note that the results could be large, several hundred KBs in size,
        /// with tens of thousands of entries in the list.
        /// </summary>
        /// <param name="exchange">Name of the exchange</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of instruments.</returns>
        public IAsyncEnumerable<Instrument> GetInstrumentsAsync(string exchange = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(exchange))
                return GetCsvAsync<Instrument>("/instruments", cancellationToken: cancellationToken);
            else
                return GetCsvAsync<Instrument>($"/instruments/{exchange}", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieve quote and market depth of upto 200 instruments
        /// </summary>
        /// <param name="instrumentId">Indentification of instrument in the form of EXCHANGE:TRADINGSYMBOL (eg: NSE:INFY) or InstrumentToken (eg: 408065)</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Dictionary of all Quote objects with keys as in InstrumentId</returns>
        public Task<Dictionary<string, Quote>> GetQuoteAsync(string[] instrumentId, CancellationToken cancellationToken = default)
        {
            var queryParametersBuilder = new ParametersBuilder();
            foreach (var i in instrumentId)
            {
                queryParametersBuilder.Add("i", i);
            }

            return GetAsync<Dictionary<string, Quote>>("/quote", queryParameters: queryParametersBuilder.Build(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieve LTP and OHLC of upto 200 instruments
        /// </summary>
        /// <param name="instrumentId">Indentification of instrument in the form of EXCHANGE:TRADINGSYMBOL (eg: NSE:INFY) or InstrumentToken (eg: 408065)</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Dictionary of all OHLC objects with keys as in InstrumentId</returns>
        public Task<Dictionary<string, OhlcResponse>> GetOhlcAsync(string[] instrumentId, CancellationToken cancellationToken = default)
        {
            var queryParametersBuilder = new ParametersBuilder();
            foreach (var i in instrumentId)
            {
                queryParametersBuilder.Add("i", i);
            }

            return GetAsync<Dictionary<string, OhlcResponse>>("/quote/ohlc", queryParameters: queryParametersBuilder.Build(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieve LTP of upto 200 instruments
        /// </summary>
        /// <param name="instrumentId">Indentification of instrument in the form of EXCHANGE:TRADINGSYMBOL (eg: NSE:INFY) or InstrumentToken (eg: 408065)</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Dictionary with InstrumentId as key and LTP as value.</returns>
        public Task<Dictionary<string, Ltp>> GetLtpAsync(string[] instrumentId, CancellationToken cancellationToken = default)
        {
            var queryParametersBuilder = new ParametersBuilder();
            foreach (var i in instrumentId)
            {
                queryParametersBuilder.Add("i", i);
            }

            return GetAsync<Dictionary<string, Ltp>>("/quote/ltp", queryParameters: queryParametersBuilder.Build(), cancellationToken: cancellationToken);
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
        /// <param name="cancellationToken"></param>
        /// <returns>List of Historical objects.</returns>
        public Task<HistoricalResponse> GetHistoricalDataAsync(
            string InstrumentToken,
            DateTime FromDate,
            DateTime ToDate,
            string Interval,
            bool Continuous = false,
            bool OI = false,
            CancellationToken cancellationToken = default)
        {
            var queryParametersBuilder = new ParametersBuilder();

            queryParametersBuilder.Add("from", FromDate);
            queryParametersBuilder.Add("to", ToDate);
            queryParametersBuilder.AddAsO1("continuous", Continuous);
            queryParametersBuilder.AddAsO1("oi", OI);

            return GetAsync<HistoricalResponse>($"/instruments/historical/{InstrumentToken}/{Interval}", queryParametersBuilder.Build(), cancellationToken: cancellationToken);
        }

        //TODO undocumented API

        /// <summary>
        /// Retrieve the buy/sell trigger range for Cover Orders.
        /// </summary>
        /// <param name="instrumentId">Indentification of instrument in the form of EXCHANGE:TRADINGSYMBOL (eg: NSE:INFY) or InstrumentToken (eg: 408065)</param>
        /// <param name="trasactionType">BUY or SELL</param>
        /// <returns>List of trigger ranges for given instrument ids for given transaction type.</returns>
        public Dictionary<string, TrigerRange> GetTriggerRange(string[] instrumentId, string trasactionType)
        {
            var param = new Dictionary<string, dynamic>();

            param.Add("i", instrumentId);
            param.Add("transaction_type", trasactionType.ToLower());

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
        /// <param name="cancellationToken"></param>
        /// <returns>List of GTTs.</returns>
        public Task<List<Gtt>> GetGttsAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<List<Gtt>>("/gtt/triggers", cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Retrieve a single GTT
        /// </summary>
        /// <param name="GttId">Id of the GTT</param>
        /// <param name="cancellationToken"></param>
        /// <returns>GTT info</returns>
        public Task<Gtt> GetGttAsync(int GttId, CancellationToken cancellationToken = default)
        {
            return GetAsync<Gtt>($"/gtt/triggers/{GttId}", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Place a GTT order
        /// </summary>
        /// <param name="gttParams">Contains the parameters for the GTT order</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Task<GttResponse> PlaceGttAsync(GttParams gttParams, CancellationToken cancellationToken = default)
        {
            var condition = new Dictionary<string, object>
            {
                { "exchange", gttParams.Exchange },
                { "tradingsymbol", gttParams.Tradingsymbol },
                { "trigger_values", gttParams.TriggerPrices },
                { "last_price", gttParams.LastPrice },
                { "instrument_token", gttParams.InstrumentToken } //TODO undocumented
            };

            var ordersParam = new List<Dictionary<string, object>>();
            foreach (var o in gttParams.Orders)
            {
                var order = new Dictionary<string, object>()
                {
                    { "exchange", gttParams.Exchange },
                    { "tradingsymbol", gttParams.Tradingsymbol },
                    { "transaction_type", o.TransactionType },
                    { "quantity", o.Quantity },
                    { "price", o.Price },
                    { "order_type", o.OrderType },
                    { "product", o.Product },
                };
                ordersParam.Add(order);
            }

            var formDataBuilder = new ParametersBuilder()
                .Add("condition", JsonSerializer.Serialize(condition, JsonSerializerOptions))
                .Add("orders", JsonSerializer.Serialize(ordersParam, JsonSerializerOptions))
                .Add("type", gttParams.TriggerType);

            return PostAsync<GttResponse>("/gtt/triggers", formData: formDataBuilder.Build(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Modify a GTT order
        /// </summary>
        /// <param name="gttId">Id of the GTT to be modified</param>
        /// <param name="gttParams">Contains the parameters for the GTT order</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Task<GttResponse> ModifyGttAsync(int gttId, GttParams gttParams, CancellationToken cancellationToken = default)
        {
            var condition = new Dictionary<string, object>
            {
                { "exchange", gttParams.Exchange },
                { "tradingsymbol", gttParams.Tradingsymbol },
                { "trigger_values", gttParams.TriggerPrices },
                { "last_price", gttParams.LastPrice },
                { "instrument_token", gttParams.InstrumentToken } //TODO undocumented
            };

            var ordersParam = new List<Dictionary<string, object>>();
            foreach (var o in gttParams.Orders)
            {
                var order = new Dictionary<string, object>()
                {
                    { "exchange", gttParams.Exchange },
                    { "tradingsymbol", gttParams.Tradingsymbol },
                    { "transaction_type", o.TransactionType },
                    { "quantity", o.Quantity },
                    { "price", o.Price },
                    { "order_type", o.OrderType },
                    { "product", o.Product },
                };
                ordersParam.Add(order);
            }

            var formDataBuilder = new ParametersBuilder()
                .Add("condition", JsonSerializer.Serialize(condition, JsonSerializerOptions))
                .Add("orders", JsonSerializer.Serialize(ordersParam, JsonSerializerOptions))
                .Add("type", gttParams.TriggerType);

            return PutAsync<GttResponse>($"/gtt/triggers/{gttId}", formDataBuilder.Build(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Cancel a GTT order
        /// </summary>
        /// <param name="gttId">Id of the GTT to be modified</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Json response in the form of nested string dictionary.</returns>
        public Task<GttResponse> CancelGttAsync(int gttId, CancellationToken cancellationToken = default)
        {
            return DeleteAsync<GttResponse>($"/gtt/triggers/{gttId}", cancellationToken: cancellationToken);
        }

        #endregion GTT


        #region MF Calls

        /// <summary>
        /// Gets the Mutual funds Instruments.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The Mutual funds Instruments.</returns>
        public IAsyncEnumerable<MFInstrument> GetMFInstrumentsAsync(CancellationToken cancellationToken = default)
        {
            return GetCsvAsync<MFInstrument>("/mf/instruments", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets all Mutual funds orders.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The Mutual funds orders.</returns>
        public Task<List<MFOrder>> GetMFOrdersAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<List<MFOrder>>("/mf/orders", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets the Mutual funds order by OrderId.
        /// </summary>
        /// <returns>The Mutual funds order.</returns>
        /// <param name="orderId">Order id.</param>
        /// <param name="cancellationToken"></param>
        public Task<MFOrder> GetMFOrdersAsync(string orderId, CancellationToken cancellationToken = default)
        {
            return GetAsync<MFOrder>($"/mf/orders/{orderId}", cancellationToken: cancellationToken);
        }

        //TODO undocumented API

        /// <summary>
        /// Places a Mutual funds order.
        /// </summary>
        /// <returns>JSON response as nested string dictionary.</returns>
        /// <param name="tradingSymbol">Tradingsymbol (ISIN) of the fund.</param>
        /// <param name="transactionType">BUY or SELL.</param>
        /// <param name="amount">Amount worth of units to purchase. Not applicable on SELLs.</param>
        /// <param name="quantity">Quantity to SELL. Not applicable on BUYs. If the holding is less than minimum_redemption_quantity, all the units have to be sold.</param>
        /// <param name="tag">An optional tag to apply to an order to identify it (alphanumeric, max 8 chars).</param>
        public Dictionary<string, dynamic> PlaceMFOrder(
            string tradingSymbol,
            string transactionType,
            decimal? amount,
            decimal? quantity = null,
            string tag = "")
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "tradingsymbol", tradingSymbol);
            Utils.AddIfNotNull(param, "transaction_type", transactionType);
            Utils.AddIfNotNull(param, "amount", amount.ToString());
            Utils.AddIfNotNull(param, "quantity", quantity.ToString());
            Utils.AddIfNotNull(param, "tag", tag);

            return Post(Routes.MutualFunds.PlaceOrder, param);
        }

        //TODO undocumented API

        /// <summary>
        /// Cancels the Mutual funds order.
        /// </summary>
        /// <returns>JSON response as nested string dictionary.</returns>
        /// <param name="orderId">Unique order id.</param>
        public Dictionary<string, dynamic> CancelMFOrder(string orderId)
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "order_id", orderId);

            return Delete(Routes.MutualFunds.CancelOrder, param);
        }

        /// <summary>
        /// Gets all Mutual funds SIPs.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The list of all Mutual funds SIPs.</returns>
        public Task<List<MFSip>> GetMFSipsAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<List<MFSip>>("/mf/sips", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets a single Mutual funds SIP by SIP id.
        /// </summary>
        /// <returns>The Mutual funds SIP.</returns>
        /// <param name="SipId">SIP id.</param>
        /// <param name="cancellationToken"></param>
        public Task<MFSip> GetMFSipsAsync(string SipId, CancellationToken cancellationToken = default)
        {
            return GetAsync<MFSip>($"/mf/sips/{SipId}", cancellationToken: cancellationToken);
        }

        //TODO undocumented API

        /// <summary>
        /// Places a Mutual funds SIP order.
        /// </summary>
        /// <returns>JSON response as nested string dictionary.</returns>
        /// <param name="tradingSymbol">ISIN of the fund.</param>
        /// <param name="amount">Amount worth of units to purchase. It should be equal to or greated than minimum_additional_purchase_amount and in multiple of purchase_amount_multiplier in the instrument master.</param>
        /// <param name="initialAmount">Amount worth of units to purchase before the SIP starts. Should be equal to or greater than minimum_purchase_amount and in multiple of purchase_amount_multiplier. This is only considered if there have been no prior investments in the target fund.</param>
        /// <param name="frequency">weekly, monthly, or quarterly.</param>
        /// <param name="instalmentDay">If Frequency is monthly, the day of the month (1, 5, 10, 15, 20, 25) to trigger the order on.</param>
        /// <param name="instalments">Number of instalments to trigger. If set to -1, instalments are triggered at fixed intervals until the SIP is cancelled.</param>
        /// <param name="tag">An optional tag to apply to an order to identify it (alphanumeric, max 8 chars).</param>
        public Dictionary<string, dynamic> PlaceMFSip(
            string tradingSymbol,
            decimal? amount,
            decimal? initialAmount,
            string frequency,
            int? instalmentDay,
            int? instalments,
            string tag = "")
        {
            var param = new Dictionary<string, dynamic>();

            Utils.AddIfNotNull(param, "tradingsymbol", tradingSymbol);
            Utils.AddIfNotNull(param, "initial_amount", initialAmount.ToString());
            Utils.AddIfNotNull(param, "amount", amount.ToString());
            Utils.AddIfNotNull(param, "frequency", frequency);
            Utils.AddIfNotNull(param, "instalment_day", instalmentDay.ToString());
            Utils.AddIfNotNull(param, "instalments", instalments.ToString());

            return Post(Routes.MutualFunds.PlaceSIP, param);
        }

        //TODO undocumented API

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
        public Dictionary<string, dynamic> ModifyMFSip(
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

        //TODO undocumented API

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
        /// <param name="cancellationToken"></param>
        /// <returns>The list of all Mutual funds holdings.</returns>
        public async Task<List<MFHolding>> GetMFHoldingsAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<List<MFHolding>>("/mf/holdings", cancellationToken: cancellationToken);
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
        /// <param name="request">Request object to add headers</param>
        private void AddExtraHeaders(HttpRequestMessage request)
        {
            var KiteAssembly = Assembly.GetAssembly(typeof(Kite));
            if (KiteAssembly != null)
            {
                request.Headers.UserAgent.TryParseAdd("KiteConnect.Net/" + KiteAssembly.GetName().Version);
            }

            request.Headers.Add("X-Kite-Version", "3");
            request.Headers.Add("Authorization", "token " + _apiKey + ":" + _accessToken);

            if (_enableLogging)
            {
                foreach (var header in request.Headers)
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
                AddExtraHeaders(request);

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
                AddExtraHeaders(request);
            }

            HttpResponseMessage response = _httpClient.Send(request);
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

        private Task<TResult> GetAsync<TResult>(string path, IReadOnlyCollection<KeyValuePair<string, string>> queryParameters = null, CancellationToken cancellationToken = default)
        {
            string url = BuildUrl(_root, path, queryParameters);
            return SendRequestAsync<TResult>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        private async IAsyncEnumerable<TResult> GetCsvAsync<TResult>(string path, IReadOnlyCollection<KeyValuePair<string, string>> queryParameters = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string url = BuildUrl(_root, path, queryParameters);
            using var httpResponse = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();
            if (httpResponse.Content.Headers.ContentType.MediaType != "text/csv")
                throw new DataException($"Unexpected content type {httpResponse.Content.Headers.ContentType.MediaType}");
            using var stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                await foreach (var record in csv.GetRecordsAsync<TResult>(cancellationToken))
                {
                    yield return record;
                }
            }
        }

        private Task<TResult> PostAsync<TResult>(string path, IReadOnlyCollection<KeyValuePair<string, string>> queryParameters = null, IReadOnlyCollection<KeyValuePair<string, string>> formData = default, CancellationToken cancellationToken = default)
        {
            string url = BuildUrl(_root, path, queryParameters);
            HttpContent content = null;
            if (formData != null && formData.Count > 0)
                content = new FormUrlEncodedContent(formData);
            return SendRequestAsync<TResult>(HttpMethod.Post, url, content, cancellationToken);
        }

        private Task<TResult> PostJsonAsync<T, TResult>(string path, IReadOnlyCollection<KeyValuePair<string, string>> queryParameters = null, T jsonData = default, CancellationToken cancellationToken = default)
        {
            string url = BuildUrl(_root, path, queryParameters);
            HttpContent content = null;
            if (jsonData != null)
                content = JsonContent.Create(jsonData);
            return SendRequestAsync<TResult>(HttpMethod.Post, url, content, cancellationToken);
        }

        private Task<TResult> PutAsync<TResult>(string path, IReadOnlyCollection<KeyValuePair<string, string>> queryParameters = null, IReadOnlyCollection<KeyValuePair<string, string>> formData = default, CancellationToken cancellationToken = default)
        {
            string url = BuildUrl(_root, path, queryParameters);
            HttpContent content = null;
            if (formData != null && formData.Count > 0)
                content = new FormUrlEncodedContent(formData);
            return SendRequestAsync<TResult>(HttpMethod.Put, url, content, cancellationToken);
        }

        private Task<TResult> DeleteAsync<TResult>(string path, IReadOnlyCollection<KeyValuePair<string, string>> queryParameters = null, CancellationToken cancellationToken = default)
        {
            string url = BuildUrl(_root, path, queryParameters);
            return SendRequestAsync<TResult>(HttpMethod.Delete, url, cancellationToken: cancellationToken);
        }

        private async Task<TResult> SendRequestAsync<TResult>(HttpMethod httpMethod, string url, HttpContent content = null, CancellationToken cancellationToken = default)
        {
            using var httpRequestMessage = new HttpRequestMessage(httpMethod, url);
            AddExtraHeaders(httpRequestMessage);
            if (content != null)
                httpRequestMessage.Content = content;
            using var httpResponse = await _httpClient.SendAsync(httpRequestMessage);
            return await ParseResponseAsync<TResult>(httpResponse, cancellationToken);
        }

        private static string BuildUrl(string baseUrl, string path, IReadOnlyCollection<KeyValuePair<string, string>> queryParameters)
        {
            string url = baseUrl + path;
            if (queryParameters != null && queryParameters.Count > 0)
            {
                var uriBuilder = new UriBuilder(url);
                var existingQueryParamters = HttpUtility.ParseQueryString(url.Contains('?') ? uriBuilder.Query : string.Empty);
                foreach (var item in queryParameters)
                {
                    existingQueryParamters.Add(item.Key, item.Value);
                }
                uriBuilder.Query = existingQueryParamters.ToString();
                url = uriBuilder.Uri.ToString();
            }
            return url;
        }

        private async Task<T> ParseResponseAsync<T>(HttpResponseMessage httpResponse, CancellationToken cancellationToken)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                var strResponse = await httpResponse.Content.ReadAsStringAsync();//TODO remove
                var response = await httpResponse.Content.ReadFromJsonAsync<SucessResponse<T>>(JsonSerializerOptions, cancellationToken);
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
            var response = await httpResponse.Content.ReadFromJsonAsync<ErrorResponse>(JsonSerializerOptions, cancellationToken);
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
