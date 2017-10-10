using System;
using System.Web.Script.Serialization;
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

            kite.SetSessionHook(onTokenExpire);

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
            Console.WriteLine(JsonSerialize(positions.Net[0]));

            kite.ModifyProduct("NSE", "ASHOKLEY", "BUY", "day", "1", "MIS", "CNC");

            // Holdings

            List<Holding> holdings = kite.GetHoldings();
            Console.WriteLine(JsonSerialize(holdings[0]));

            // Instruments

            List<Instrument> instruments = kite.GetInstruments();
            Console.WriteLine(JsonSerialize(instruments[0]));

            // Quote

            Quote quote = kite.GetQuote("NSE", "INFY");
            Console.WriteLine(JsonSerialize(quote));

            // Get OHLC and LTP of upto 200 scrips

            Dictionary<string, OHLC> ohlcs = kite.GetOHLC(new string[] { "NSE:INFY", "NSE:ASHOKLEY" });
            Console.WriteLine(JsonSerialize(ohlcs));

            // Get LTP of upto 200 scrips

            Dictionary<string, LTP> ltps = kite.GetLTP(new string[] { "NSE:INFY", "NSE:ASHOKLEY" });
            Console.WriteLine(JsonSerialize(ltps));

            // Trigger Range

            TrigerRange triggerRange = kite.GetTriggerRange("NSE", "INFY", "BUY");
            Console.WriteLine(JsonSerialize(triggerRange));

            // Orders

            List<Order> orders = kite.GetOrders();
            Console.WriteLine(JsonSerialize(orders[0]));

            Dictionary<string, dynamic> response = kite.PlaceOrder("CDS", "USDINR17AUGFUT", "SELL", "1", Price: "64.0000", OrderType: "MARKET", Product: "NRML");
            Console.WriteLine("Order Id: " + response["data"]["order_id"]);

            kite.PlaceOrder("CDS", "USDINR17AUGFUT", "BUY", "1", Price: "63.9000", OrderType: "LIMIT", Product: "NRML");
            kite.CancelOrder("1234");

            List<OrderInfo> orderinfo = kite.GetOrder("1234");
            Console.WriteLine(JsonSerialize(orderinfo[0]));

            // Trades

            List<Trade> trades = kite.GetTrades("1234");
            Console.WriteLine(JsonSerialize(trades[0]));

            // Margins

            kite.Margins("commodity");
            kite.Margins("equity");

            // Historical Data With Dates

            List<Historical> historical = kite.GetHistorical("5633", "2015-12-28", "2016-01-01", "minute");
            Console.WriteLine(JsonSerialize(historical[0]));

            // Historical Data With Timestamps

            List<Historical> historical_timestamp = kite.GetHistorical("5633", "2016-01-01 11:00:00", "2016-01-01 11:10:00", "minute");
            Console.WriteLine(JsonSerialize(historical_timestamp[0]));

            // Continuous Historical Data

            List<Historical> historical_continuous = kite.GetHistorical("5633", "2015-12-28", "2016-01-01", "minute", Continuous: true);
            Console.WriteLine(JsonSerialize(historical_continuous[0]));

            // Mutual Funds Instruments

            List<MFInstrument> mfinstruments = kite.GetMFInstruments();
            Console.WriteLine(JsonSerialize(mfinstruments[0]));

            // Mutual Funds Orders

            List<MFOrder> mforders = kite.GetMFOrders();
            Console.WriteLine(JsonSerialize(mforders[0]));

            MFOrder mforder = kite.GetMFOrder("1234");
            Console.WriteLine(JsonSerialize(mforder));

            kite.PlaceMFOrder("INF174K01LS2", "BUY", "20000");

            kite.CancelMFOrder("1234");

            // Mutual Funds SIPs

            List<MFSIP> mfsips = kite.GetMFSIPs();
            Console.WriteLine(JsonSerialize(mfsips[0]));

            MFSIP sip = kite.GetMFSIP("63429");
            Console.WriteLine(JsonSerialize(sip));

            kite.PlaceMFSIP("INF174K01LS2", "1000", "5000", "monthly", "1", "-1");

            kite.ModifyMFSIP("1234", "1000", "monthly", "1", "10", "paused");

            kite.CancelMFSIP("1234");

            // Mutual Funds Holdings

            List<MFHolding> mfholdings = kite.GetMFHoldings();
            Console.WriteLine(JsonSerialize(mfholdings[0]));

            Console.ReadKey();

            // Disconnect from ticker

            ticker.Close();
		}

        private static void initSession()
        {
            Console.WriteLine("Goto " + kite.GetLoginURL());
            Console.WriteLine("Enter request token: ");
            string requestToken = Console.ReadLine();
            User user = kite.RequestAccessToken(requestToken, MySecret);

            Console.WriteLine(JsonSerialize(user));

            MyAccessToken = user.AccessToken;
            MyPublicToken = user.PublicToken;
        }

        private static void initTicker()
        {
            ticker = new Ticker(MyAPIKey, MyUserId, MyPublicToken);

            ticker.OnTick += onTick;
            ticker.OnReconnect += onReconnect;
            ticker.OnNoReconnect += oNoReconnect;
            ticker.OnError += onError;
            ticker.OnClose += onClose;
            ticker.OnConnect += onConnect;

            ticker.EnableReconnect(Interval: 5,Retries: 50);
            ticker.Connect();

            // Subscribing to NIFTY50 and setting mode to LTP
            ticker.Subscribe(Tokens: new string[] { "256265" });
            ticker.SetMode(Tokens: new string[] { "256265" }, Mode: "ltp");
        }

        private static void onTokenExpire()
        {
            Console.WriteLine("Need to login again");
        }

        private static void onConnect()
        {
            Console.WriteLine("Connected ticker");
        }

        private static void onClose()
        {
            Console.WriteLine("Closed ticker");
        }

        private static void onError(string Message)
        {
            Console.WriteLine("Error: " + Message);
        }

        private static void oNoReconnect()
        {
            Console.WriteLine("Not reconnecting");
        }

        private static void onReconnect()
        {
            Console.WriteLine("Reconnecting");
        }

        private static void onTick(Tick TickData)
        {
            Console.WriteLine("Tick " + JsonSerialize(TickData));
        }

        // helper funtion to get json from nested string dictionaries
        static string JsonSerialize(object x)
        {
            var jss = new JavaScriptSerializer();
            return jss.Serialize(x);
        }
    }
}
