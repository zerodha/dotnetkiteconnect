using KiteConnect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KiteConnectTest
{
    [TestClass]
    public class KiteTest
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, Converters = { new JsonStringEnumConverter() } };

        MockServer ms;

        [TestInitialize]
        public void TestInitialize()
        {
            ms = new MockServer("http://localhost:8080/");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ms.Stop();
        }

        [TestMethod]
        public void TestSetAccessToken()
        {
            Kite kite = new Kite("apikey");
            kite.SetAccessToken("access_token");
            Assert.Throws<TokenException>(() => kite.GetPositions());
        }


        [TestMethod]
        public void TestError()
        {
            string json = File.ReadAllText(@"responses/error.json", Encoding.UTF8);
            ms.SetStatusCode(403);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080", Debug: true);
            Assert.ThrowsAsync<GeneralException>(async () => await kite.GetProfileAsync());
        }

        [TestMethod]
        public async Task TestProfile()
        {
            string json = File.ReadAllText(@"responses/profile.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            Profile profile = await kite.GetProfileAsync();
            Console.WriteLine(profile.Email);
            Assert.AreEqual(profile.Email, "xxxyyy@gmail.com");
        }

        [TestMethod]
        public void TestPositions()
        {
            string json = File.ReadAllText(@"responses/positions.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            PositionResponse positionResponse = kite.GetPositions();
            Assert.AreEqual(positionResponse.Net[0].TradingSymbol, "LEADMINI17DECFUT");
            Assert.AreEqual(positionResponse.Day[0].TradingSymbol, "GOLDGUINEA17DECFUT");
        }

        [TestMethod]
        public void TestHoldings()
        {
            string json = File.ReadAllText(@"responses/holdings.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Holding> holdings = kite.GetHoldings();
            Assert.AreEqual(holdings[0].AveragePrice, 40.67m);
            Assert.AreEqual(holdings[0].MTF.Quantity, 1000m);
        }

        [TestMethod]
        public void TestAuctionInstruments()
        {
            string json = File.ReadAllText(@"responses/auction_instruments.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<AuctionInstrument> instruments = kite.GetAuctionInstruments();
            Assert.AreEqual(instruments[0].PNL, 564.8000000000002m);
        }

        [TestMethod]
        public async Task TestMargins()
        {
            string json = File.ReadAllText(@"responses/margins.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            UserMarginsResponse margins = await kite.GetMarginsAsync();

            Assert.AreEqual(margins.Equity.Net, (decimal)1697.7);
            Assert.AreEqual(margins.Commodity.Net, (decimal)-8676.296);
        }

        [TestMethod]
        public async Task TestMarginsNoTurnover()
        {
            string json = File.ReadAllText(@"responses/margins_noturnover.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            UserMarginsResponse margins = await kite.GetMarginsAsync();

            Assert.AreEqual(margins.Equity.Utilised.Turnover, (decimal)0);
            Assert.AreEqual(margins.Commodity.Utilised.Turnover, (decimal)0);
        }

        [TestMethod]
        public void TestOrderMargins()
        {
            string json = File.ReadAllText(@"responses/order_margins.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");

            OrderMarginParams param = new OrderMarginParams();
            param.Exchange = Constants.Exchange.NFO;
            param.TradingSymbol = "ASHOKLEY20NOVFUT";
            param.TransactionType = Constants.Transaction.Sell;
            param.Quantity = 1;
            param.Price = 64.0000m;
            param.OrderType = Constants.OrderType.Market;
            param.Product = Constants.Product.MIS;

            List<OrderMargin> margins = kite.GetOrderMargins(new List<OrderMarginParams>() { param });

            Assert.AreEqual(margins[0].Total, (decimal)8.36025);
            Assert.AreEqual(margins[0].SPAN, (decimal)5.408);
            Assert.AreEqual(margins[0].Leverage, (decimal)5);
            Assert.AreEqual(margins[0].Charges.TransactionTax, (decimal)0.5);
            Assert.AreEqual(margins[0].Charges.GST.IGST, (decimal)0.386496);
        }

        [TestMethod]
        public void TestOrderMarginsCompact()
        {
            string json = File.ReadAllText(@"responses/order_margins_compact.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");

            OrderMarginParams param = new OrderMarginParams();
            param.Exchange = Constants.Exchange.NFO;
            param.TradingSymbol = "ASHOKLEY21JULFUT";
            param.TransactionType = Constants.Transaction.Sell;
            param.Quantity = 1;
            param.Price = 64.0000m;
            param.OrderType = Constants.OrderType.Market;
            param.Product = Constants.Product.MIS;

            OrderMarginParams param2 = new OrderMarginParams();
            param2.Exchange = Constants.Exchange.NFO;
            param2.TradingSymbol = "NIFTY21JUL15000PE";
            param.TransactionType = Constants.Transaction.Buy;
            param2.Quantity = 75;
            param2.Price = 300;
            param2.Product = Constants.Product.MIS;
            param2.OrderType = Constants.OrderType.Limit;

            List<OrderMargin> margins = kite.GetOrderMargins(new List<OrderMarginParams>() { param, param2 }, Mode: Constants.Margin.Mode.Compact);

            Assert.AreEqual(margins[0].Total, (decimal)30.2280825);
            Assert.AreEqual(margins[0].SPAN, (decimal)0);
        }

        [TestMethod]
        public void TestBasketMargins()
        {
            string json = File.ReadAllText(@"responses/basket_margins.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");

            OrderMarginParams param = new OrderMarginParams();
            param.Exchange = Constants.Exchange.NFO;
            param.TradingSymbol = "ASHOKLEY21JULFUT";
            param.TransactionType = Constants.Transaction.Sell;
            param.Quantity = 1;
            param.Price = 64.0000m;
            param.OrderType = Constants.OrderType.Market;
            param.Product = Constants.Product.MIS;

            OrderMarginParams param2 = new OrderMarginParams();
            param2.Exchange = Constants.Exchange.NFO;
            param2.TradingSymbol = "NIFTY21JUL15000PE";
            param.TransactionType = Constants.Transaction.Buy;
            param2.Quantity = 75;
            param2.Price = 300;
            param2.Product = Constants.Product.MIS;
            param2.OrderType = Constants.OrderType.Limit;

            BasketMargin margins = kite.GetBasketMargins(new List<OrderMarginParams>() { param, param2 });

            Assert.AreEqual(margins.Final.Total, (decimal)22530.221345);
            Assert.AreEqual(margins.Final.SPAN, (decimal)26.9577);
        }

        [TestMethod]
        public void TestBasketMarginsCompact()
        {
            string json = File.ReadAllText(@"responses/basket_margins_compact.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");

            OrderMarginParams param = new OrderMarginParams();
            param.Exchange = Constants.Exchange.NFO;
            param.TradingSymbol = "ASHOKLEY21JULFUT";
            param.TransactionType = Constants.Transaction.Sell;
            param.Quantity = 1;
            param.Price = 64.0000m;
            param.OrderType = Constants.OrderType.Market;
            param.Product = Constants.Product.MIS;

            OrderMarginParams param2 = new OrderMarginParams();
            param2.Exchange = Constants.Exchange.NFO;
            param2.TradingSymbol = "NIFTY21JUL15000PE";
            param.TransactionType = Constants.Transaction.Buy;
            param2.Quantity = 75;
            param2.Price = 300;
            param2.Product = Constants.Product.MIS;
            param2.OrderType = Constants.OrderType.Limit;

            BasketMargin margins = kite.GetBasketMargins(new List<OrderMarginParams>() { param, param2 }, Mode: Constants.Margin.Mode.Compact);

            Assert.AreEqual(margins.Final.Total, (decimal)22530.2280825);
            Assert.AreEqual(margins.Final.SPAN, (decimal)0);
        }

        [TestMethod]
        public async Task TestEquityMargins()
        {
            string json = File.ReadAllText(@"responses/equity_margins.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            UserMargin margin = await kite.GetMarginsAsync("equity");

            Assert.AreEqual(margin.Net, (decimal)1812.3535);
        }

        [TestMethod]
        public async Task TestCommodityMargins()
        {
            string json = File.ReadAllText(@"responses/equity_margins.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            UserMargin margin = await kite.GetMarginsAsync("commodity");

            Assert.AreEqual(margin.Net, (decimal)1812.3535);
        }

        [TestMethod]
        public void TestOHLC()
        {
            string json = File.ReadAllText(@"responses/ohlc.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            Dictionary<string, OHLC> ohlcs = kite.GetOHLC(new string[] { "408065", "NSE:INFY" });

            Assert.AreEqual(ohlcs["408065"].LastPrice, (decimal)966.8);
        }

        [TestMethod]
        public void TestLTP()
        {
            string json = File.ReadAllText(@"responses/ltp.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            Dictionary<string, LTP> ltps = kite.GetLTP(new string[] { "NSE:INFY" });

            Assert.AreEqual(ltps["NSE:INFY"].LastPrice, (decimal)989.2);
        }

        [TestMethod]
        public void TestQuote()
        {
            string json = File.ReadAllText(@"responses/quote.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            Dictionary<string, Quote> quotes = kite.GetQuote(new string[] { "NSE:ASHOKLEY", "NSE:NIFTY 50" });

            Assert.AreEqual(quotes["NSE:ASHOKLEY"].LastPrice, (decimal)76.6);
            Assert.AreEqual(quotes["NSE:NIFTY 50"].LowerCircuitLimit, 0);
        }

        [TestMethod]
        public void TestOrders()
        {
            string json = File.ReadAllText(@"responses/orders.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Order> orders = kite.GetOrders();

            Assert.AreEqual(orders[0].Price, 72);

            Assert.AreEqual(orders[2].Tag, "connect test order2");
            Assert.AreEqual(orders[2].Tags[1], "XXXXX");

            Assert.AreEqual(orders[3].ValidityTTL, 2);

            Assert.AreEqual(orders[3].Meta["iceberg"]["legs"], 5);

            Assert.AreEqual(orders[0].AuctionNumber, 10);

            Assert.AreEqual(orders[4].Product, Constants.Product.MTF);
        }

        [TestMethod]
        public void TestGTTs()
        {
            string json = File.ReadAllText(@"responses/gtt_get_orders.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<GTT> gtts = kite.GetGTTs();

            Assert.AreEqual(gtts[0].Id, 105099);
            Assert.AreEqual(gtts[0].Condition?.TriggerValues[0], 102m);
            Assert.AreEqual(gtts[0].Condition?.TriggerValues[1], 103.7m);

        }

        [TestMethod]
        public void TestGTT()
        {
            string json = File.ReadAllText(@"responses/gtt_get_order.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            GTT gtt = kite.GetGTT(123);

            Assert.AreEqual(gtt.Id, 123);
        }

        [TestMethod]
        public void TestOrderInfo()
        {
            string json = File.ReadAllText(@"responses/orderinfo.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Order> orderhistory = kite.GetOrderHistory("171124000819854");

            Assert.AreEqual(orderhistory[0].PendingQuantity, 100);
        }

        [TestMethod]
        public void TestInstruments()
        {
            string csv = File.ReadAllText(@"responses/instruments_all.csv", Encoding.UTF8);
            ms.SetResponse("text/csv", csv);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Instrument> instruments = kite.GetInstruments();

            Assert.AreEqual(instruments[0].InstrumentToken, (uint)3813889);
            Assert.AreEqual(instruments[0].LastPrice, 10.01m);
            Assert.AreEqual(instruments[0].Strike, 13.14m);
        }

        [TestMethod]
        public void TestSegmentInstruments()
        {
            string csv = File.ReadAllText(@"responses/instruments_nse.csv", Encoding.UTF8);
            ms.SetResponse("text/csv", csv);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Instrument> instruments = kite.GetInstruments(Constants.Exchange.NSE);

            Assert.AreEqual(instruments[0].InstrumentToken, (uint)3813889);
        }

        [TestMethod]
        public void TestTrades()
        {
            string json = File.ReadAllText(@"responses/trades.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Trade> trades = kite.GetOrderTrades("151220000000000");

            Assert.AreEqual(trades[0].TradeId, "159918");
        }

        [TestMethod]
        public void TestMFSIPs()
        {
            string json = File.ReadAllText(@"responses/mf_sips.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<MFSIP> sips = kite.GetMFSIPs();

            Assert.AreEqual(sips[0].SIPId, "1234");
        }

        [TestMethod]
        public void TestMFSIP()
        {
            string json = File.ReadAllText(@"responses/mf_sip.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            MFSIP sip = kite.GetMFSIPs("1234");

            Assert.AreEqual(sip.SIPId, "1234");
        }

        [TestMethod]
        public void TestMFOrders()
        {
            string json = File.ReadAllText(@"responses/mf_orders.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<MFOrder> orders = kite.GetMFOrders();

            Assert.AreEqual(orders[0].OrderId, "123123");
        }

        [TestMethod]
        public void TestMFOrder()
        {
            string json = File.ReadAllText(@"responses/mf_order.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            MFOrder order = kite.GetMFOrders("123123");

            Assert.AreEqual(order.OrderId, "123123");
        }

        [TestMethod]
        public void TestMFHoldings()
        {
            string json = File.ReadAllText(@"responses/mf_holdings.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<MFHolding> holdings = kite.GetMFHoldings();

            Assert.AreEqual(holdings[0].Folio, "123123/123");
        }

        [TestMethod]
        public void TestMFInstruments()
        {
            string csv = File.ReadAllText(@"responses/mf_instruments.csv", Encoding.UTF8);
            ms.SetResponse("text/csv", csv);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<MFInstrument> instruments = kite.GetMFInstruments();

            Assert.AreEqual(instruments[0].TradingSymbol, "INF209K01157");
            Assert.AreEqual(instruments[0].MinimumPurchaseAmount, 1234.0m);
            Assert.AreEqual(instruments[0].PurchaseAmountMultiplier, 13.14m);
        }
    }
}
