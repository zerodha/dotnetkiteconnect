using System;
using KiteConnect;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        static async Task Main(string[] args)
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

            // Get all GTTs

            List<GTT> gtts = kite.GetGTTs();
            Console.WriteLine(Utils.JsonSerialize(gtts[0]));

            // Get GTT by Id

            GTT gtt = kite.GetGTT(99691);
            Console.WriteLine(Utils.JsonSerialize(gtt));

            // Cacncel GTT by Id

            var gttCancelResponse = kite.CancelGTT(1582);
            Console.WriteLine(Utils.JsonSerialize(gttCancelResponse));

            // Place GTT

            GTTParams gttParams = new GTTParams();
            gttParams.TriggerType = Constants.GTT.Trigger.OCO;
            gttParams.Exchange = "NSE";
            gttParams.TradingSymbol = "SBIN";
            gttParams.LastPrice = 288.9m;

            List<decimal> triggerPrices = new List<decimal>();
            triggerPrices.Add(260m);
            triggerPrices.Add(320m);
            gttParams.TriggerPrices = triggerPrices;

            // Only sell is allowed for OCO or two-leg orders.
            // Single leg orders can be buy or sell order.
            // Passing a last price is mandatory.
            // A stop-loss order must have trigger and price below last price and target order must have trigger and price above last price.
            // Only limit order type  and CNC product type is allowed for now.

            GTTOrderParams order1Params = new GTTOrderParams();
            order1Params.OrderType = Constants.OrderType.Limit;
            order1Params.Price = 250m;
            order1Params.Product = Constants.Product.CNC;
            order1Params.TransactionType = Constants.Transaction.Sell;
            order1Params.Quantity = 0;

            GTTOrderParams order2Params = new GTTOrderParams();
            order2Params.OrderType = Constants.OrderType.Limit;
            order2Params.Price = 320m;
            order2Params.Product = Constants.Product.CNC;
            order2Params.TransactionType = Constants.Transaction.Sell;
            order2Params.Quantity = 1;

            // Target or upper trigger
            List<GTTOrderParams> ordersList = new List<GTTOrderParams>();
            ordersList.Add(order1Params);
            ordersList.Add(order2Params);
            gttParams.Orders = ordersList;

            var placeGTTResponse = kite.PlaceGTT(gttParams);
            Console.WriteLine(Utils.JsonSerialize(placeGTTResponse));

            var modifyGTTResponse = kite.ModifyGTT(407301, gttParams);
            Console.WriteLine(Utils.JsonSerialize(modifyGTTResponse));

            // Positions

            PositionResponse positions = kite.GetPositions();
            Console.WriteLine(Utils.JsonSerialize(positions.Net[0]));

            kite.ConvertPosition(
                Exchange: Constants.Exchange.NSE,
                TradingSymbol: "ASHOKLEY",
                TransactionType: Constants.Transaction.Buy,
                PositionType: Constants.Position.Day,
                Quantity: 1,
                OldProduct: Constants.Product.MIS,
                NewProduct: Constants.Product.CNC
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

            Dictionary<string, TrigerRange> triggerRange = kite.GetTriggerRange(
                InstrumentId: new string[] { "NSE:ASHOKLEY" },
                TrasactionType: Constants.Transaction.Buy
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
                Exchange: Constants.Exchange.CDS,
                TradingSymbol: "USDINR17AUGFUT",
                TransactionType: Constants.Transaction.Sell,
                Quantity: 1,
                Price: 64.0000m,
                OrderType: Constants.OrderType.Market,
                Product: Constants.Product.MIS
            );
            Console.WriteLine("Order Id: " + response["data"]["order_id"]);

            // Place buy order

            kite.PlaceOrder(
                Exchange: Constants.Exchange.CDS,
                TradingSymbol: "USDINR17AUGFUT",
                TransactionType: Constants.Transaction.Buy,
                Quantity: 1,
                Price: 63.9000m,
                OrderType: Constants.OrderType.Limit,
                Product: Constants.Product.MIS
            );

            // Cancel order by id

            kite.CancelOrder("1234");

            //BO LIMIT order placing

            kite.PlaceOrder(
                Exchange: Constants.Exchange.NSE,
                TradingSymbol: "ASHOKLEY",
                TransactionType: Constants.Transaction.Buy,
                Quantity: 1,
                Price: 115,
                Product: Constants.Product.MIS,
                OrderType: Constants.OrderType.Limit,
                Validity: Constants.Validity.Day,
                SquareOffValue: 2,
                StoplossValue: 2,
                Variety: Constants.Variety.BO
            );

            // BO LIMIT exiting

            kite.CancelOrder(
                OrderId: "1234",
                Variety: Constants.Variety.BO,
                ParentOrderId: "5678"
            );

            // BO SL order placing

            kite.PlaceOrder(
                Exchange: Constants.Exchange.NSE,
                TradingSymbol: "ASHOKLEY",
                TransactionType: Constants.Transaction.Buy,
                Quantity: 1,
                Price: 117,
                Product: Constants.Product.MIS,
                OrderType: Constants.OrderType.SL,
                Validity: Constants.Validity.Day,
                SquareOffValue: 2,
                StoplossValue: 2,
                TriggerPrice: 117.5m,
                Variety: Constants.Variety.BO
            );

            // BO SL exiting

            kite.CancelOrder(
               OrderId: "1234",
               Variety: Constants.Variety.BO,
               ParentOrderId: "5678"
           );

            // CO LIMIT order placing

            kite.PlaceOrder(
                Exchange: Constants.Exchange.NSE,
                TradingSymbol: "ASHOKLEY",
                TransactionType: Constants.Transaction.Buy,
                Quantity: 1,
                Price: 115.5m,
                Product: Constants.Product.MIS,
                OrderType: Constants.OrderType.Limit,
                Validity: Constants.Validity.Day,
                TriggerPrice: 116.5m,
                Variety: Constants.Variety.CO
            );

            // CO LIMIT exiting

            kite.CancelOrder(
               OrderId: "1234",
               Variety: Constants.Variety.BO,
               ParentOrderId: "5678"
           );

            // CO MARKET order placing

            kite.PlaceOrder(
                Exchange: Constants.Exchange.NSE,
                TradingSymbol: "ASHOKLEY",
                TransactionType: Constants.Transaction.Buy,
                Quantity: 1,
                Product: Constants.Product.MIS,
                OrderType: Constants.OrderType.Market,
                Validity: Constants.Validity.Day,
                TriggerPrice: 116.5m,
                Variety: Constants.Variety.CO
            );

            // CO MARKET exiting

            kite.CancelOrder(
                OrderId: "1234",
                Variety: Constants.Variety.BO,
                ParentOrderId: "5678"
            );

            // Place order with TTL validity
            kite.PlaceOrder(
                Exchange: Constants.Exchange.NSE,
                TradingSymbol: "INFY",
                TransactionType: Constants.Transaction.Buy,
                Quantity: 1,
                Price: 1500.0m,
                OrderType: Constants.OrderType.Limit,
                Product: Constants.Product.MIS,
                Validity: Constants.Validity.TTL,
                ValidityTTL: 5
            );

            // Place an Iceberg order
            kite.PlaceOrder(
                Exchange: Constants.Exchange.NSE,
                TradingSymbol: "INFY",
                TransactionType: Constants.Transaction.Buy,
                Quantity: 10,
                Price: 1500.0m,
                OrderType: Constants.OrderType.Limit,
                Product: Constants.Product.MIS,
                Variety: Constants.Variety.Iceberg,
                IcebergLegs: 2,
                IcebergQuantity: 5
            );

            // Trades

            List<Trade> trades = kite.GetOrderTrades("1234");
            Console.WriteLine(Utils.JsonSerialize(trades[0]));

            // Margins

            UserMargin commodityMargins = await kite.GetMarginsAsync(Constants.Margin.Commodity);
            UserMargin equityMargins = await kite.GetMarginsAsync(Constants.Margin.Equity);

            // Order margins

            OrderMarginParams orderParam = new OrderMarginParams();
            orderParam.Exchange = Constants.Exchange.NFO;
            orderParam.TradingSymbol = "ASHOKLEY21JULFUT";
            orderParam.TransactionType = Constants.Transaction.Sell;
            orderParam.Quantity = 1;
            orderParam.Price = 64.0000m;
            orderParam.OrderType = Constants.OrderType.Market;
            orderParam.Product = Constants.Product.MIS;

            List<OrderMargin> margins = kite.GetOrderMargins(new List<OrderMarginParams>() { orderParam });

            // Basket margins

            OrderMarginParams basketParam = new OrderMarginParams();
            basketParam.Exchange = Constants.Exchange.NFO;
            basketParam.TradingSymbol = "NIFTY21JUL15000PE";
            basketParam.TransactionType = Constants.Transaction.Buy;
            basketParam.Quantity = 75;
            basketParam.Price = 300;
            basketParam.Product = Constants.Product.MIS;
            basketParam.OrderType = Constants.OrderType.Limit;

            BasketMargin basketMargins = kite.GetBasketMargins(new List<OrderMarginParams>() { basketParam }, ConsiderPositions: true);

            // Virtual contract notes

            ContractNoteParams contractNoteParam = new ContractNoteParams();
            contractNoteParam.OrderID = "230821101633675";
            contractNoteParam.Quantity = 1;
            contractNoteParam.AveragePrice = 99.7m;
            contractNoteParam.Exchange = "NSE";
            contractNoteParam.TradingSymbol = "BHEL";
            contractNoteParam.TransactionType = Constants.Transaction.Buy;
            contractNoteParam.Variety = Constants.Variety.Regular;
            contractNoteParam.OrderType = Constants.OrderType.Limit;
            contractNoteParam.Product = Constants.Product.MIS;
            List<ContractNote> contractNotes = kite.GetVirtualContractNote(new List<ContractNoteParams>() { contractNoteParam });
            Console.WriteLine(Utils.JsonSerialize(contractNotes));

            // Historical Data With Dates

            List<Historical> historical = kite.GetHistoricalData(
                InstrumentToken: "5633",
                FromDate: new DateTime(2016, 1, 1, 12, 50, 0),   // 2016-01-01 12:50:00 AM
                ToDate: new DateTime(2016, 1, 1, 13, 10, 0),    // 2016-01-01 01:10:00 PM
                Interval: Constants.Interval.Minute,
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
                TransactionType: Constants.Transaction.Buy,
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
            ticker = new Ticker(MyAPIKey, MyAccessToken);

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
            ticker.Subscribe(Tokens: new uint[] { 256265 });
            ticker.SetMode(Tokens: new uint[] { 256265 }, Mode: Constants.TickerMode.LTP);
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
