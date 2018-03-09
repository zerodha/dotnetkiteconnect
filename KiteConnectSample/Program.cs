using System;
using KiteConnect;
using System.Collections.Generic;

namespace KiteConnectSample
{
    class Program
    {
        // instances of Kite and Ticker
        static Ticker ticker;
        static Kite kite;

        // Initialize key and secret of your app
        static string MyAPIKey = "abcdefghijklmnopqrstuvwxyz";
        static string MySecret = "abcdefghijklmnopqrstuvwxyz";
        static string MyUserId = "ZR0000";

        // persist these data in settings or db or file
        static string MyPublicToken = "abcdefghijklmnopqrstuvwxyz";
        static string MyAccessToken = "abcdefghijklmnopqrstuvwxyz";

        static void Main(string[] args)
        {
            kite = new Kite(MyAPIKey, Debug: true);

            // For handling 403 errors

            kite.SetSessionExpiryHook(OnTokenExpire);

            // Initializes the login flow

            try
            {
                initSession();
            }
            catch (Exception e)
            {
                // Cannot continue without proper authentication
                Console.WriteLine(e.Message);
                Console.ReadKey();
                Environment.Exit(0);
            }

            kite.SetAccessToken(MyAccessToken);

            // Initialize ticker

            initTicker();

            // Positions

            PositionResponse positions = kite.GetPositions();
            Console.WriteLine(Utils.JsonSerialize(positions.Net[0]));

            kite.ConvertPosition(
                Exchange: Constants.EXCHANGE_NSE,
                TradingSymbol: "ASHOKLEY",
                TransactionType: Constants.TRANSACTION_TYPE_BUY,
                PositionType: Constants.POSITION_DAY,
                Quantity: 1,
                OldProduct: Constants.PRODUCT_MIS,
                NewProduct: Constants.PRODUCT_CNC
            );

            // Holdings

            List<Holding> holdings = kite.GetHoldings();
            Console.WriteLine(Utils.JsonSerialize(holdings[0]));

            // Instruments

            List<Instrument> instruments = kite.GetInstruments();
            Console.WriteLine(Utils.JsonSerialize(instruments[0]));

            // Get quotes of upto 200 scrips

            Dictionary<string, Quote> quotes = kite.GetQuote(InstrumentId: new string[] { "NSE:INFY", "NSE:ASHOKLEY" });
            Console.WriteLine(Utils.JsonSerialize(quotes));

            // Get OHLC and LTP of upto 200 scrips

            Dictionary<string, OHLC> ohlcs = kite.GetOHLC(InstrumentId: new string[] { "NSE:INFY", "NSE:ASHOKLEY" });
            Console.WriteLine(Utils.JsonSerialize(ohlcs));

            // Get LTP of upto 200 scrips

            Dictionary<string, LTP> ltps = kite.GetLTP(InstrumentId: new string[] { "NSE:INFY", "NSE:ASHOKLEY" });
            Console.WriteLine(Utils.JsonSerialize(ltps));

            // Trigger Range

            Dictionary <string, TrigerRange> triggerRange = kite.GetTriggerRange(
                InstrumentId: new string[] { "NSE:ASHOKLEY" },
                TrasactionType: Constants.TRANSACTION_TYPE_BUY
            );
            Console.WriteLine(Utils.JsonSerialize(triggerRange));

            // Get all orders

            List<Order> orders = kite.GetOrders();
            Console.WriteLine(Utils.JsonSerialize(orders[0]));

            // Get order by id

            List<Order> orderinfo = kite.GetOrderHistory("1234");
            Console.WriteLine(Utils.JsonSerialize(orderinfo[0]));

            // Place sell order

            Dictionary<string, dynamic> response = kite.PlaceOrder(
                Exchange: Constants.EXCHANGE_CDS,
                TradingSymbol: "USDINR17AUGFUT",
                TransactionType: Constants.TRANSACTION_TYPE_SELL,
                Quantity: 1,
                Price: 64.0000m,
                OrderType: Constants.ORDER_TYPE_MARKET,
                Product: Constants.PRODUCT_MIS
            );
            Console.WriteLine("Order Id: " + response["data"]["order_id"]);

            // Place buy order

            kite.PlaceOrder(
                Exchange: Constants.EXCHANGE_CDS,
                TradingSymbol: "USDINR17AUGFUT",
                TransactionType: Constants.TRANSACTION_TYPE_BUY,
                Quantity: 1,
                Price: 63.9000m,
                OrderType: Constants.ORDER_TYPE_LIMIT,
                Product: Constants.PRODUCT_MIS
            );

            // Cancel order by id

            kite.CancelOrder("1234");

            //BO LIMIT order placing

            kite.PlaceOrder(
                Exchange: Constants.EXCHANGE_NSE,
                TradingSymbol: "ASHOKLEY",
                TransactionType: Constants.TRANSACTION_TYPE_BUY,
                Quantity: 1,
                Price: 115,
                Product: Constants.PRODUCT_MIS,
                OrderType: Constants.ORDER_TYPE_LIMIT,
                Validity: Constants.VALIDITY_DAY,
                SquareOffValue: 2,
                StoplossValue: 2,
                Variety: Constants.VARIETY_BO
            );

            // BO LIMIT exiting

            kite.CancelOrder(
                OrderId: "1234",
                Variety: Constants.VARIETY_BO,
                ParentOrderId: "5678"
            );

            // BO SL order placing

            kite.PlaceOrder(
                Exchange: Constants.EXCHANGE_NSE,
                TradingSymbol: "ASHOKLEY",
                TransactionType: Constants.TRANSACTION_TYPE_BUY,
                Quantity: 1,
                Price: 117,
                Product: Constants.PRODUCT_MIS,
                OrderType: Constants.ORDER_TYPE_SL,
                Validity: Constants.VALIDITY_DAY,
                SquareOffValue: 2,
                StoplossValue: 2,
                TriggerPrice: 117.5m,
                Variety: Constants.VARIETY_BO
            );

            // BO SL exiting

            kite.CancelOrder(
               OrderId: "1234",
               Variety: Constants.VARIETY_BO,
               ParentOrderId: "5678"
           );

            // CO LIMIT order placing

            kite.PlaceOrder(
                Exchange: Constants.EXCHANGE_NSE,
                TradingSymbol: "ASHOKLEY",
                TransactionType: Constants.TRANSACTION_TYPE_BUY,
                Quantity: 1,
                Price: 115.5m,
                Product: Constants.PRODUCT_MIS,
                OrderType: Constants.ORDER_TYPE_LIMIT,
                Validity: Constants.VALIDITY_DAY,
                TriggerPrice: 116.5m,
                Variety: Constants.VARIETY_CO
            );

            // CO LIMIT exiting

            kite.CancelOrder(
               OrderId: "1234",
               Variety: Constants.VARIETY_BO,
               ParentOrderId: "5678"
           );

            // CO MARKET order placing

            kite.PlaceOrder(
                Exchange: Constants.EXCHANGE_NSE,
                TradingSymbol: "ASHOKLEY",
                TransactionType: Constants.TRANSACTION_TYPE_BUY,
                Quantity: 1,
                Product: Constants.PRODUCT_MIS,
                OrderType: Constants.ORDER_TYPE_MARKET,
                Validity: Constants.VALIDITY_DAY,
                TriggerPrice: 116.5m,
                Variety: Constants.VARIETY_CO
            );

            // CO MARKET exiting

            kite.CancelOrder(
                OrderId: "1234",
                Variety: Constants.VARIETY_BO,
                ParentOrderId: "5678"
            );

            // Trades

            List<Trade> trades = kite.GetOrderTrades("1234");
            Console.WriteLine(Utils.JsonSerialize(trades[0]));

            // Margins

            UserMargin commodityMargins = kite.GetMargins(Constants.MARGIN_COMMODITY);
            UserMargin equityMargins = kite.GetMargins(Constants.MARGIN_EQUITY);

            // Historical Data With Dates

            List<Historical> historical = kite.GetHistoricalData(
                InstrumentToken: "5633",
                FromDate: new DateTime(2016, 1, 1, 12, 50, 0),   // 2016-01-01 12:50:00 AM
                ToDate: new DateTime(2016, 1, 1, 13, 10, 0),    // 2016-01-01 01:10:00 PM
                Interval: Constants.INTERVAL_MINUTE,
                Continuous: false
            );
            Console.WriteLine(Utils.JsonSerialize(historical[0]));

            // Mutual Funds Instruments

            List<MFInstrument> mfinstruments = kite.GetMFInstruments();
            Console.WriteLine(Utils.JsonSerialize(mfinstruments[0]));

            // Mutual funds get all orders

            List<MFOrder> mforders = kite.GetMFOrders();
            Console.WriteLine(Utils.JsonSerialize(mforders[0]));

            // Mutual funds get order by id

            MFOrder mforder = kite.GetMFOrders(OrderId: "1234");
            Console.WriteLine(Utils.JsonSerialize(mforder));

            // Mutual funds place order

            kite.PlaceMFOrder(
                TradingSymbol: "INF174K01LS2",
                TransactionType: Constants.TRANSACTION_TYPE_BUY,
                Amount: 20000
            );

            // Mutual funds cancel order by id

            kite.CancelMFOrder(OrderId: "1234");

            // Mutual Funds get all SIPs

            List<MFSIP> mfsips = kite.GetMFSIPs();
            Console.WriteLine(Utils.JsonSerialize(mfsips[0]));

            // Mutual Funds get SIP by id

            MFSIP sip = kite.GetMFSIPs("63429");
            Console.WriteLine(Utils.JsonSerialize(sip));

            // Mutual Funds place SIP order

            kite.PlaceMFSIP(
                TradingSymbol: "INF174K01LS2",
                Amount: 1000,
                InitialAmount: 5000,
                Frequency: "monthly",
                InstalmentDay: 1,
                Instalments: -1 // -1 means infinite
            );

            // Mutual Funds modify SIP order

            kite.ModifyMFSIP(
                SIPId: "1234",
                Amount: 1000,
                Frequency: "monthly",
                InstalmentDay: 1,
                Instalments: 10,
                Status: "paused"
            );

            kite.CancelMFSIP(SIPId: "1234");

            // Mutual Funds Holdings

            List<MFHolding> mfholdings = kite.GetMFHoldings();
            Console.WriteLine(Utils.JsonSerialize(mfholdings[0]));

            Console.ReadKey();

            // Disconnect from ticker

            ticker.Close();
        }

        private static void initSession()
        {
            Console.WriteLine("Goto " + kite.GetLoginURL());
            Console.WriteLine("Enter request token: ");
            string requestToken = Console.ReadLine();
            User user = kite.GenerateSession(requestToken, MySecret);

            Console.WriteLine(Utils.JsonSerialize(user));

            MyAccessToken = user.AccessToken;
            MyPublicToken = user.PublicToken;
        }

        private static void initTicker()
        {
            ticker = new Ticker(MyAPIKey, MyAccessToken, CustomWebSocket: new Win7WebSocket());

            ticker.OnTick += OnTick;
            ticker.OnReconnect += OnReconnect;
            ticker.OnNoReconnect += OnNoReconnect;
            ticker.OnError += OnError;
            ticker.OnClose += OnClose;
            ticker.OnConnect += OnConnect;
            ticker.OnOrderUpdate += OnOrderUpdate;

            ticker.EnableReconnect(Interval: 5, Retries: 50);
            ticker.Connect();

            // Subscribing to NIFTY50 and setting mode to LTP
            ticker.Subscribe(Tokens: new UInt32[] { 256265 });
            ticker.SetMode(Tokens: new UInt32[] { 256265 }, Mode: Constants.MODE_LTP);
        }

        private static void OnTokenExpire()
        {
            Console.WriteLine("Need to login again");
        }

        private static void OnConnect()
        {
            Console.WriteLine("Connected ticker");
        }

        private static void OnClose()
        {
            Console.WriteLine("Closed ticker");
        }

        private static void OnError(string Message)
        {
            Console.WriteLine("Error: " + Message);
        }

        private static void OnNoReconnect()
        {
            Console.WriteLine("Not reconnecting");
        }

        private static void OnReconnect()
        {
            Console.WriteLine("Reconnecting");
        }

        private static void OnTick(Tick TickData)
        {
            Console.WriteLine("Tick " + Utils.JsonSerialize(TickData));
        }

        private static void OnOrderUpdate(Order OrderData)
        {
            Console.WriteLine("OrderUpdate " + Utils.JsonSerialize(OrderData));
        }
    }
}
