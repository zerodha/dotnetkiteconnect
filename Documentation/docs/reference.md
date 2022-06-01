## ![Class](/assets/class.jpg) &nbsp;&nbsp;Kite Class

The API client class. In production, you may initialize a single instance of this class per `APIKey`.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite Constructor

Initialize a new Kite Connect client instance.

| Argument | Type | Description |
| --- | --- | --- |
| APIKey | String | API Key issued to you |
| AccessToken | String | The token obtained after the login flow in exchange for the `RequestToken` .             Pre-login, this will default to None,but once you have obtained it, you should persist it in a database or session to pass             to the Kite Connect class initialisation for subsequent requests. |
| Root | String | API end point root. Unless you explicitly want to send API requests to a non-default endpoint, this can be ignored. |
| Debug | Boolean | If set to True, will serialise and print requests and responses to stdout. |
| Timeout | Int32 | Time in milliseconds for which  the API client will wait for a request to complete before it fails |
| Proxy | Net.WebProxy | To set proxy for http request. Should be an object of WebProxy. |
| Pool | Int32 | Number of connections to server. Client will reuse the connections if they are alive. |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.EnableLogging

Enabling logging prints HTTP request and response summaries to console

| Argument | Type | Description |
| --- | --- | --- |
| enableLogging | Boolean | Set to true to enable logging |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.SetSessionExpiryHook

Set a callback hook for session (`TokenException` -- timeout, expiry etc.) errors.
            An `AccessToken` (login session) can become invalid for a number of
            reasons, but it doesn't make sense for the client to
            try and catch it during every API call.
            A callback method that handles session errors
            can be set here and when the client encounters
            a token error at any point, it'll be called.
            This callback, for instance, can log the user out of the UI,
            clear session cookies, or initiate a fresh login.

| Argument | Type | Description |
| --- | --- | --- |
| Method | Action | Action to be invoked when session becomes invalid. |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.SetAccessToken

Set the `AccessToken` received after a successful authentication.

| Argument | Type | Description |
| --- | --- | --- |
| AccessToken | String | Access token for the session. |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetLoginURL

Get the remote login url to which a user should be redirected to initiate the login flow.

**Returns:** Login url to authenticate the user.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GenerateSession

Do the token exchange with the `RequestToken` obtained after the login flow,
            and retrieve the `AccessToken` required for all subsequent requests.The
            response contains not just the `AccessToken`, but metadata for
            the user who has authenticated.

| Argument | Type | Description |
| --- | --- | --- |
| RequestToken | String | Token obtained from the GET paramers after a successful login redirect. |
| AppSecret | String | API secret issued with the API key. |

**Returns:** User structure with tokens and profile data

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.InvalidateAccessToken

Kill the session by invalidating the access token

| Argument | Type | Description |
| --- | --- | --- |
| AccessToken | String | Access token to invalidate. Default is the active access token. |

**Returns:** Json response in the form of nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.InvalidateRefreshToken

Invalidates RefreshToken

| Argument | Type | Description |
| --- | --- | --- |
| RefreshToken | String | RefreshToken to invalidate |

**Returns:** Json response in the form of nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.RenewAccessToken

Renew AccessToken using RefreshToken

| Argument | Type | Description |
| --- | --- | --- |
| RefreshToken | String | RefreshToken to renew the AccessToken. |
| AppSecret | String | API secret issued with the API key. |

**Returns:** TokenRenewResponse that contains new AccessToken and RefreshToken.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetProfile

Gets currently logged in user details

**Returns:** User profile

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetOrderMargins

Margin data for a specific order

| Argument | Type | Description |
| --- | --- | --- |
| OrderMarginParams | Collections.Generic.List{OrderMarginParams} | List of all order params to get margins for |
| Mode | String | Mode of the returned response content. Eg: Constants.MARGIN_MODE_COMPACT |

**Returns:** List of margins of order

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetBasketMargins

Margin data for a basket orders

| Argument | Type | Description |
| --- | --- | --- |
| OrderMarginParams | Collections.Generic.List{OrderMarginParams} | List of all order params to get margins for |
| ConsiderPositions | Boolean | Consider users positions while calculating margins |
| Mode | String | Mode of the returned response content. Eg: Constants.MARGIN_MODE_COMPACT |

**Returns:** List of margins of order

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetMargins

Get account balance and cash margin details for all segments.

**Returns:** User margin response with both equity and commodity margins.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetMargins

Get account balance and cash margin details for a particular segment.

| Argument | Type | Description |
| --- | --- | --- |
| Segment | String | Trading segment (eg: equity or commodity) |

**Returns:** Margins for specified segment.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.PlaceOrder

Place an order

| Argument | Type | Description |
| --- | --- | --- |
| Exchange | String | Name of the exchange |
| TradingSymbol | String | Tradingsymbol of the instrument |
| TransactionType | String | BUY or SELL |
| Quantity | Int32 | Quantity to transact |
| Price | Nullable{Decimal} | For LIMIT orders |
| Product | String | Margin product applied to the order (margin is blocked based on this) |
| OrderType | String | Order type (MARKET, LIMIT etc.) |
| Validity | String | Order validity (DAY, IOC and TTL) |
| DisclosedQuantity | Nullable{Int32} | Quantity to disclose publicly (for equity trades) |
| TriggerPrice | Nullable{Decimal} | For SL, SL-M etc. |
| SquareOffValue | Nullable{Decimal} | Price difference at which the order should be squared off and profit booked (eg: Order price is 100. Profit target is 102. So squareoff = 2) |
| StoplossValue | Nullable{Decimal} | Stoploss difference at which the order should be squared off (eg: Order price is 100. Stoploss target is 98. So stoploss = 2) |
| TrailingStoploss | Nullable{Decimal} | Incremental value by which stoploss price changes when market moves in your favor by the same incremental value from the entry price (optional) |
| Variety | String | You can place orders of varieties; regular orders, after market orders, cover orders, iceberg orders etc. |
| Tag | String | An optional tag to apply to an order to identify it (alphanumeric, max 20 chars) |
| ValidityTTL | Nullable{Int32} | Order life span in minutes for TTL validity orders |
| IcebergLegs | Nullable{Int32} | Total number of legs for iceberg order type (number of legs per Iceberg should be between 2 and 10) |
| IcebergQuantity | Nullable{Int32} | Split quantity for each iceberg leg order (Quantity/IcebergLegs) |

**Returns:** Json response in the form of nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.ModifyOrder

Modify an open order.

| Argument | Type | Description |
| --- | --- | --- |
| OrderId | String | Id of the order to be modified |
| ParentOrderId | String | Id of the parent order (obtained from the /orders call) as BO is a multi-legged order |
| Exchange | String | Name of the exchange |
| TradingSymbol | String | Tradingsymbol of the instrument |
| TransactionType | String | BUY or SELL |
| Quantity | String | Quantity to transact |
| Price | Nullable{Decimal} | For LIMIT orders |
| Product | String | Margin product applied to the order (margin is blocked based on this) |
| OrderType | String | Order type (MARKET, LIMIT etc.) |
| Validity | String | Order validity |
| DisclosedQuantity | Nullable{Int32} | Quantity to disclose publicly (for equity trades) |
| TriggerPrice | Nullable{Decimal} | For SL, SL-M etc. |
| Variety | String | You can place orders of varieties; regular orders, after market orders, cover orders etc. |

**Returns:** Json response in the form of nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.CancelOrder

Cancel an order

| Argument | Type | Description |
| --- | --- | --- |
| OrderId | String | Id of the order to be cancelled |
| Variety | String | You can place orders of varieties; regular orders, after market orders, cover orders etc. |
| ParentOrderId | String | Id of the parent order (obtained from the /orders call) as BO is a multi-legged order |

**Returns:** Json response in the form of nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetOrders

Gets the collection of orders from the orderbook.

**Returns:** List of orders.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetOrderHistory

Gets information about given OrderId.

| Argument | Type | Description |
| --- | --- | --- |
| OrderId | String | Unique order id |

**Returns:** List of order objects.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetOrderTrades

Retreive the list of trades executed (all or ones under a particular order).
            An order can be executed in tranches based on market conditions.
            These trades are individually recorded under an order.

| Argument | Type | Description |
| --- | --- | --- |
| OrderId | String | is the ID of the order (optional) whose trades are to be retrieved. If no `OrderId` is specified, all trades for the day are returned. |

**Returns:** List of trades of given order.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetPositions

Retrieve the list of positions.

**Returns:** Day and net positions.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetHoldings

Retrieve the list of equity holdings.

**Returns:** List of holdings.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.ConvertPosition

Modify an open position's product type.

| Argument | Type | Description |
| --- | --- | --- |
| Exchange | String | Name of the exchange |
| TradingSymbol | String | Tradingsymbol of the instrument |
| TransactionType | String | BUY or SELL |
| PositionType | String | overnight or day |
| Quantity | Nullable{Int32} | Quantity to convert |
| OldProduct | String | Existing margin product of the position |
| NewProduct | String | Margin product to convert to |

**Returns:** Json response in the form of nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetInstruments

Retrieve the list of market instruments available to trade.
            Note that the results could be large, several hundred KBs in size,
            with tens of thousands of entries in the list.

| Argument | Type | Description |
| --- | --- | --- |
| Exchange | String | Name of the exchange |

**Returns:** List of instruments.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetQuote

Retrieve quote and market depth of upto 200 instruments

| Argument | Type | Description |
| --- | --- | --- |
| InstrumentId | String[] | Indentification of instrument in the form of EXCHANGE:TRADINGSYMBOL (eg: NSE:INFY) or InstrumentToken (eg: 408065) |

**Returns:** Dictionary of all Quote objects with keys as in InstrumentId

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetOHLC

Retrieve LTP and OHLC of upto 200 instruments

| Argument | Type | Description |
| --- | --- | --- |
| InstrumentId | String[] | Indentification of instrument in the form of EXCHANGE:TRADINGSYMBOL (eg: NSE:INFY) or InstrumentToken (eg: 408065) |

**Returns:** Dictionary of all OHLC objects with keys as in InstrumentId

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetLTP

Retrieve LTP of upto 200 instruments

| Argument | Type | Description |
| --- | --- | --- |
| InstrumentId | String[] | Indentification of instrument in the form of EXCHANGE:TRADINGSYMBOL (eg: NSE:INFY) or InstrumentToken (eg: 408065) |

**Returns:** Dictionary with InstrumentId as key and LTP as value.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetHistoricalData

Retrieve historical data (candles) for an instrument.

| Argument | Type | Description |
| --- | --- | --- |
| InstrumentToken | String | Identifier for the instrument whose historical records you want to fetch. This is obtained with the instrument list API. |
| FromDate | DateTime | Date in format yyyy-MM-dd for fetching candles between two days. Date in format yyyy-MM-dd hh:mm:ss for fetching candles between two timestamps. |
| ToDate | DateTime | Date in format yyyy-MM-dd for fetching candles between two days. Date in format yyyy-MM-dd hh:mm:ss for fetching candles between two timestamps. |
| Interval | String | The candle record interval. Possible values are: minute, day, 3minute, 5minute, 10minute, 15minute, 30minute, 60minute |
| Continuous | Boolean | Pass true to get continous data of expired instruments. |
| OI | Boolean | Pass true to get open interest data. |

**Returns:** List of Historical objects.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetTriggerRange

Retrieve the buy/sell trigger range for Cover Orders.

| Argument | Type | Description |
| --- | --- | --- |
| InstrumentId | String[] | Indentification of instrument in the form of EXCHANGE:TRADINGSYMBOL (eg: NSE:INFY) or InstrumentToken (eg: 408065) |
| TrasactionType | String | BUY or SELL |

**Returns:** List of trigger ranges for given instrument ids for given transaction type.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetGTTs

Retrieve the list of GTTs.

**Returns:** List of GTTs.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetGTT

Retrieve a single GTT

| Argument | Type | Description |
| --- | --- | --- |
| GTTId | Int32 | Id of the GTT |

**Returns:** GTT info

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.PlaceGTT

Place a GTT order

| Argument | Type | Description |
| --- | --- | --- |
| gttParams | GTTParams | Contains the parameters for the GTT order |

**Returns:** Json response in the form of nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.ModifyGTT

Modify a GTT order

| Argument | Type | Description |
| --- | --- | --- |
| GTTId | Int32 | Id of the GTT to be modified |
| gttParams | GTTParams | Contains the parameters for the GTT order |

**Returns:** Json response in the form of nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.CancelGTT

Cancel a GTT order

| Argument | Type | Description |
| --- | --- | --- |
| GTTId | Int32 | Id of the GTT to be modified |

**Returns:** Json response in the form of nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetMFInstruments

Gets the Mutual funds Instruments.

**Returns:** The Mutual funds Instruments.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetMFOrders

Gets all Mutual funds orders.

**Returns:** The Mutual funds orders.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetMFOrders

Gets the Mutual funds order by OrderId.

| Argument | Type | Description |
| --- | --- | --- |
| OrderId | String | Order id. |

**Returns:** The Mutual funds order.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.PlaceMFOrder

Places a Mutual funds order.

| Argument | Type | Description |
| --- | --- | --- |
| TradingSymbol | String | Tradingsymbol (ISIN) of the fund. |
| TransactionType | String | BUY or SELL. |
| Amount | Nullable{Decimal} | Amount worth of units to purchase. Not applicable on SELLs. |
| Quantity | Nullable{Decimal} | Quantity to SELL. Not applicable on BUYs. If the holding is less than minimum_redemption_quantity, all the units have to be sold. |
| Tag | String | An optional tag to apply to an order to identify it (alphanumeric, max 8 chars). |

**Returns:** JSON response as nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.CancelMFOrder

Cancels the Mutual funds order.

| Argument | Type | Description |
| --- | --- | --- |
| OrderId | String | Unique order id. |

**Returns:** JSON response as nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetMFSIPs

Gets all Mutual funds SIPs.

**Returns:** The list of all Mutual funds SIPs.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetMFSIPs

Gets a single Mutual funds SIP by SIP id.

| Argument | Type | Description |
| --- | --- | --- |
| SIPID | String | SIP id. |

**Returns:** The Mutual funds SIP.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.PlaceMFSIP

Places a Mutual funds SIP order.

| Argument | Type | Description |
| --- | --- | --- |
| TradingSymbol | String | ISIN of the fund. |
| Amount | Nullable{Decimal} | Amount worth of units to purchase. It should be equal to or greated than minimum_additional_purchase_amount and in multiple of purchase_amount_multiplier in the instrument master. |
| InitialAmount | Nullable{Decimal} | Amount worth of units to purchase before the SIP starts. Should be equal to or greater than minimum_purchase_amount and in multiple of purchase_amount_multiplier. This is only considered if there have been no prior investments in the target fund. |
| Frequency | String | weekly, monthly, or quarterly. |
| InstalmentDay | Nullable{Int32} | If Frequency is monthly, the day of the month (1, 5, 10, 15, 20, 25) to trigger the order on. |
| Instalments | Nullable{Int32} | Number of instalments to trigger. If set to -1, instalments are triggered at fixed intervals until the SIP is cancelled. |
| Tag | String | An optional tag to apply to an order to identify it (alphanumeric, max 8 chars). |

**Returns:** JSON response as nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.ModifyMFSIP

Modifies the Mutual funds SIP.

| Argument | Type | Description |
| --- | --- | --- |
| SIPId | String | SIP id. |
| Amount | Nullable{Decimal} | Amount worth of units to purchase. It should be equal to or greated than minimum_additional_purchase_amount and in multiple of purchase_amount_multiplier in the instrument master. |
| Frequency | String | weekly, monthly, or quarterly. |
| InstalmentDay | Nullable{Int32} | If Frequency is monthly, the day of the month (1, 5, 10, 15, 20, 25) to trigger the order on. |
| Instalments | Nullable{Int32} | Number of instalments to trigger. If set to -1, instalments are triggered idefinitely until the SIP is cancelled. |
| Status | String | Pause or unpause an SIP (active or paused). |

**Returns:** JSON response as nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.CancelMFSIP

Cancels the Mutual funds SIP.

| Argument | Type | Description |
| --- | --- | --- |
| SIPId | String | SIP id. |

**Returns:** JSON response as nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.GetMFHoldings

Gets the Mutual funds holdings.

**Returns:** The list of all Mutual funds holdings.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.Get

Alias for sending a GET request.

| Argument | Type | Description |
| --- | --- | --- |
| Route | String | URL route of API |
| Params | Collections.Generic.Dictionary{String:Object} | Additional paramerters |

**Returns:** Varies according to API endpoint

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.Post

Alias for sending a POST request.

| Argument | Type | Description |
| --- | --- | --- |
| Route | String | URL route of API |
| Params | Object | Additional paramerters |

**Returns:** Varies according to API endpoint

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.Put

Alias for sending a PUT request.

| Argument | Type | Description |
| --- | --- | --- |
| Route | String | URL route of API |
| Params | Object | Additional paramerters |

**Returns:** Varies according to API endpoint

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.Delete

Alias for sending a DELETE request.

| Argument | Type | Description |
| --- | --- | --- |
| Route | String | URL route of API |
| Params | Object | Additional paramerters |

**Returns:** Varies according to API endpoint

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.AddExtraHeaders

Adds extra headers to request

| Argument | Type | Description |
| --- | --- | --- |
| Req | Net.HttpWebRequest@ | Request object to add headers |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Kite.Request

Make an HTTP request.

| Argument | Type | Description |
| --- | --- | --- |
| Route | String | URL route of API |
| Method | String | Method of HTTP request |
| Params | Object | Additional paramerters. Can be dictionary, list etc. |

**Returns:** Varies according to API endpoint

## ![Class](/assets/class.jpg) &nbsp;&nbsp;Tick Class

Tick data structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;DepthItem Class

Market depth item structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;Historical Class

Historical structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;Holding Class

Holding structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;AvailableMargin Class

Available margin structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;UtilisedMargin Class

Utilised margin structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;UserMargin Class

UserMargin structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;UserMarginsResponse Class

User margins response structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;OrderMarginParams Class

OrderMarginParams structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;OrderMargin Class

OrderMargin structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;BasketMargin Class

BasketMargin structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;OrderMarginPNL Class

OrderMarginPNL structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;Position Class

Position structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;PositionResponse Class

Position response structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;Order Class

Order structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;GTT Class

GTTOrder structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;GTTMeta Class

GTTMeta structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;GTTCondition Class

GTTCondition structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;GTTOrder Class

GTTOrder structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;GTTResult Class

GTTResult structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;GTTOrderResult Class

GTTOrderResult structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;GTTParams Class

GTTParams structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;GTTOrderParams Class

GTTOrderParams structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;Instrument Class

Instrument structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;Trade Class

Trade structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;TrigerRange Class

Trigger range structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;User Class

User structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;Profile Class

User structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;Quote Class

Quote structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;OHLC Class

OHLC Quote structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;LTP Class

LTP Quote structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;MFHolding Class

Mutual funds holdings structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;MFInstrument Class

Mutual funds instrument structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;MFOrder Class

Mutual funds order structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;MFSIP Class

Mutual funds SIP structure

## ![Class](/assets/class.jpg) &nbsp;&nbsp;Ticker Class

The WebSocket client for connecting to Kite Connect's streaming quotes service.

### ![Event](/assets/event.jpg) &nbsp;&nbsp;Ticker.OnConnect

Event triggered when ticker is connected

### ![Event](/assets/event.jpg) &nbsp;&nbsp;Ticker.OnClose

Event triggered when ticker is disconnected

### ![Event](/assets/event.jpg) &nbsp;&nbsp;Ticker.OnTick

Event triggered when ticker receives a tick

### ![Event](/assets/event.jpg) &nbsp;&nbsp;Ticker.OnOrderUpdate

Event triggered when ticker receives an order update

### ![Event](/assets/event.jpg) &nbsp;&nbsp;Ticker.OnError

Event triggered when ticker encounters an error

### ![Event](/assets/event.jpg) &nbsp;&nbsp;Ticker.OnReconnect

Event triggered when ticker is reconnected

### ![Event](/assets/event.jpg) &nbsp;&nbsp;Ticker.OnNoReconnect

Event triggered when ticker is not reconnecting after failure

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker Constructor

Initialize websocket client instance.

| Argument | Type | Description |
| --- | --- | --- |
| APIKey | String | API key issued to you |
| UserID | String | Zerodha client id of the authenticated user |
| AccessToken | String | Token obtained after the login flow in             exchange for the `request_token`.Pre-login, this will default to None,            but once you have obtained it, you should            persist it in a database or session to pass            to the Kite Connect class initialisation for subsequent requests. |
| Root | Boolean | Websocket API end point root. Unless you explicitly             want to send API requests to a non-default endpoint, this can be ignored. |
| Reconnect | Int32 | Enables WebSocket autreconnect in case of network failure/disconnection. |
| ReconnectInterval | Int32 | Interval (in seconds) between auto reconnection attemptes. Defaults to 5 seconds. |
| ReconnectTries | Boolean | Maximum number reconnection attempts. Defaults to 50 attempts. |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.ReadShort

Reads 2 byte short int from byte stream

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.ReadInt

Reads 4 byte int32 from byte stream

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.GetDivisor

Get the divisor to convert price values

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.ReadLTP

Reads an ltp mode tick from raw binary data

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.ReadIndexQuote

Reads a index's quote mode tick from raw binary data

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.ReadQuote

Reads a quote mode tick from raw binary data

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.ReadFull

Reads a full mode tick from raw binary data

### ![Field](/assets/pubfield.jpg) &nbsp;&nbsp;Ticker.IsConnected

Tells whether ticker is connected to server not.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.Connect

Start a WebSocket connection

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.Close

Close a WebSocket connection

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.Reconnect

Reconnect WebSocket connection in case of failures

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.Subscribe

Subscribe to a list of instrument_tokens.

| Argument | Type | Description |
| --- | --- | --- |
| Tokens | UInt32[] | List of instrument instrument_tokens to subscribe |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.UnSubscribe

Unsubscribe the given list of instrument_tokens.

| Argument | Type | Description |
| --- | --- | --- |
| Tokens | UInt32[] | List of instrument instrument_tokens to unsubscribe |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.SetMode

Set streaming mode for the given list of tokens.

| Argument | Type | Description |
| --- | --- | --- |
| Tokens | UInt32[] | List of instrument tokens on which the mode should be applied |
| Mode | String | Mode to set. It can be one of the following: ltp, quote, full. |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.ReSubscribe

Resubscribe to all currently subscribed tokens. Used to restore all the subscribed tokens after successful reconnection.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.EnableReconnect

Enable WebSocket autreconnect in case of network failure/disconnection.

| Argument | Type | Description |
| --- | --- | --- |
| Interval | Int32 | Interval between auto reconnection attemptes. `onReconnect` callback is triggered when reconnection is attempted. |
| Retries | Int32 | Maximum number reconnection attempts. Defaults to 50 attempts. `onNoReconnect` callback is triggered when number of retries exceeds this value. |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Ticker.DisableReconnect

Disable WebSocket autreconnect.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Utils.StringToDate

Convert string to Date object

| Argument | Type | Description |
| --- | --- | --- |
| obj | String | Date string. |

**Returns:** Date object/

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Utils.JsonSerialize

Serialize C# object to JSON string.

| Argument | Type | Description |
| --- | --- | --- |
| obj | Object | C# object to serialize. |

**Returns:** JSON string/

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Utils.JsonDeserialize

Deserialize Json string to nested string dictionary.

| Argument | Type | Description |
| --- | --- | --- |
| Json | String | Json string to deserialize. |

**Returns:** Json in the form of nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Utils.ElementToDict

Recursively traverses an object and converts JsonElement objects to corresponding primitives.

| Argument | Type | Description |
| --- | --- | --- |
| obj | Text.Json.JsonElement | Input JsonElement object. |

**Returns:** Object with primitives

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Utils.StringToDecimal

Converts string to decimal. Handles culture and scientific notations.

| Argument | Type | Description |
| --- | --- | --- |
| value | String | Input string. |

**Returns:** Decimal value

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Utils.ParseCSV

Parse instruments API's CSV response.

| Argument | Type | Description |
| --- | --- | --- |
| Data | String | Response of instruments API. |

**Returns:** CSV data as array of nested string dictionary.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Utils.StreamFromString

Wraps a string inside a stream

| Argument | Type | Description |
| --- | --- | --- |
| value | String | string data |

**Returns:** Stream that reads input string

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Utils.AddIfNotNull

Helper function to add parameter to the request only if it is not null or empty

| Argument | Type | Description |
| --- | --- | --- |
| Params | Collections.Generic.Dictionary{String:Object} | Dictionary to add the key-value pair |
| Key | String | Key of the parameter |
| Value | String | Value of the parameter |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Utils.SHA256

Generates SHA256 checksum for login.

| Argument | Type | Description |
| --- | --- | --- |
| Data | String | Input data to generate checksum for. |

**Returns:** SHA256 checksum in hex format.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Utils.BuildParam

Creates key=value with url encoded value

| Argument | Type | Description |
| --- | --- | --- |
| Key | String | Key |
| Value | Object | Value |

**Returns:** Combined string

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Utils.UnixToDateTime

Converts Unix timestamp to DateTime

| Argument | Type | Description |
| --- | --- | --- |
| unixTimeStamp | UInt64 | Unix timestamp in seconds. |

**Returns:** DateTime object.

### ![Method](/assets/method.jpg) &nbsp;&nbsp;Utils.GetValueOrDefault``2

Returns a default value if key doesn't exist in dictionary

## ![Class](/assets/class.jpg) &nbsp;&nbsp;WebSocket Class

A wrapper for .Net's ClientWebSocket with callbacks

### ![Method](/assets/method.jpg) &nbsp;&nbsp;WebSocket Constructor

Initialize WebSocket class

| Argument | Type | Description |
| --- | --- | --- |
| BufferLength | Int32 | Size of buffer to keep byte stream chunk. |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;WebSocket.IsConnected

Check if WebSocket is connected or not

**Returns:** True if connection is live

### ![Method](/assets/method.jpg) &nbsp;&nbsp;WebSocket.Connect

Connect to WebSocket

### ![Method](/assets/method.jpg) &nbsp;&nbsp;WebSocket.Send

Send message to socket connection

| Argument | Type | Description |
| --- | --- | --- |
| Message | String | Message to send |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;WebSocket.Close

Close the WebSocket connection

| Argument | Type | Description |
| --- | --- | --- |
| Abort | Boolean | If true WebSocket will not send 'Close' signal to server. Used when connection is disconnected due to netork issues. |

### ![Method](/assets/method.jpg) &nbsp;&nbsp;System.ExceptionExtensions.Messages

Returns a list of all the exception messages from the top-level
            exception down through all the inner exceptions. Useful for making
            logs and error pages easier to read when dealing with exceptions.
            Usage: Exception.Messages()

