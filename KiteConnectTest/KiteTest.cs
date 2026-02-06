using KiteConnect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiteConnectTest
{
    [TestClass]
    public class KiteTest
    {
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
            Assert.ThrowsAsync<TokenException>(async () => await kite.GetPositionsAsync());
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
        public async Task TestPositions()
        {
            string json = File.ReadAllText(@"responses/positions.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            PositionResponse positionResponse = await kite.GetPositionsAsync();
            Assert.AreEqual(positionResponse.Net[0].Tradingsymbol, "LEADMINI17DECFUT");
            Assert.AreEqual(positionResponse.Day[0].Tradingsymbol, "GOLDGUINEA17DECFUT");
        }

        [TestMethod]
        public async Task TestHoldings()
        {
            string json = File.ReadAllText(@"responses/holdings.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Holding> holdings = await kite.GetHoldingsAsync();
            Assert.AreEqual(holdings[0].AveragePrice, 40.67m);
            Assert.AreEqual(holdings[0].MTF.Quantity, 1000m);
        }

        [TestMethod]
        public async Task TestAuctionInstruments()
        {
            string json = File.ReadAllText(@"responses/auction_instruments.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<AuctionInstrument> instruments = await kite.GetAuctionInstrumentsAsync();
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
        public async Task TestOrderMargins()
        {
            string json = File.ReadAllText(@"responses/order_margins.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");

            OrderMarginParams param = new OrderMarginParams();
            param.Exchange = Constants.Exchange.NFO;
            param.Tradingsymbol = "ASHOKLEY20NOVFUT";
            param.TransactionType = Constants.Transaction.Sell;
            param.Quantity = 1;
            param.Price = 64.0000m;
            param.OrderType = Constants.OrderType.Market;
            param.Product = Constants.Product.MIS;

            List<OrderMargin> margins = await kite.GetOrderMarginsAsync(new List<OrderMarginParams>() { param });

            Assert.AreEqual(margins[0].Total, (decimal)8.36025);
            Assert.AreEqual(margins[0].SPAN, (decimal)5.408);
            Assert.AreEqual(margins[0].Leverage, (decimal)5);
            Assert.AreEqual(margins[0].Charges.TransactionTax, (decimal)0.5);
            Assert.AreEqual(margins[0].Charges.GST.IGST, (decimal)0.386496);
        }

        [TestMethod]
        public async Task TestOrderMarginsCompact()
        {
            string json = File.ReadAllText(@"responses/order_margins_compact.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");

            OrderMarginParams param = new OrderMarginParams();
            param.Exchange = Constants.Exchange.NFO;
            param.Tradingsymbol = "ASHOKLEY21JULFUT";
            param.TransactionType = Constants.Transaction.Sell;
            param.Quantity = 1;
            param.Price = 64.0000m;
            param.OrderType = Constants.OrderType.Market;
            param.Product = Constants.Product.MIS;

            OrderMarginParams param2 = new OrderMarginParams();
            param2.Exchange = Constants.Exchange.NFO;
            param2.Tradingsymbol = "NIFTY21JUL15000PE";
            param.TransactionType = Constants.Transaction.Buy;
            param2.Quantity = 75;
            param2.Price = 300;
            param2.Product = Constants.Product.MIS;
            param2.OrderType = Constants.OrderType.Limit;

            List<OrderMargin> margins = await kite.GetOrderMarginsAsync(new List<OrderMarginParams>() { param, param2 }, Mode: Constants.Margin.Mode.Compact);

            Assert.AreEqual(margins[0].Total, (decimal)30.2280825);
            Assert.AreEqual(margins[0].SPAN, (decimal)0);
        }

        [TestMethod]
        public async Task TestBasketMargins()
        {
            string json = File.ReadAllText(@"responses/basket_margins.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");

            OrderMarginParams param = new OrderMarginParams();
            param.Exchange = Constants.Exchange.NFO;
            param.Tradingsymbol = "ASHOKLEY21JULFUT";
            param.TransactionType = Constants.Transaction.Sell;
            param.Quantity = 1;
            param.Price = 64.0000m;
            param.OrderType = Constants.OrderType.Market;
            param.Product = Constants.Product.MIS;

            OrderMarginParams param2 = new OrderMarginParams();
            param2.Exchange = Constants.Exchange.NFO;
            param2.Tradingsymbol = "NIFTY21JUL15000PE";
            param.TransactionType = Constants.Transaction.Buy;
            param2.Quantity = 75;
            param2.Price = 300;
            param2.Product = Constants.Product.MIS;
            param2.OrderType = Constants.OrderType.Limit;

            BasketMargin margins = await kite.GetBasketMarginsAsync(new List<OrderMarginParams>() { param, param2 });

            Assert.AreEqual(margins.Final.Total, (decimal)22530.221345);
            Assert.AreEqual(margins.Final.SPAN, (decimal)26.9577);
        }

        [TestMethod]
        public async Task TestBasketMarginsCompact()
        {
            string json = File.ReadAllText(@"responses/basket_margins_compact.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");

            OrderMarginParams param = new OrderMarginParams();
            param.Exchange = Constants.Exchange.NFO;
            param.Tradingsymbol = "ASHOKLEY21JULFUT";
            param.TransactionType = Constants.Transaction.Sell;
            param.Quantity = 1;
            param.Price = 64.0000m;
            param.OrderType = Constants.OrderType.Market;
            param.Product = Constants.Product.MIS;

            OrderMarginParams param2 = new OrderMarginParams();
            param2.Exchange = Constants.Exchange.NFO;
            param2.Tradingsymbol = "NIFTY21JUL15000PE";
            param.TransactionType = Constants.Transaction.Buy;
            param2.Quantity = 75;
            param2.Price = 300;
            param2.Product = Constants.Product.MIS;
            param2.OrderType = Constants.OrderType.Limit;

            BasketMargin margins = await kite.GetBasketMarginsAsync(new List<OrderMarginParams>() { param, param2 }, Mode: Constants.Margin.Mode.Compact);

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
        public async Task TestOHLC()
        {
            string json = File.ReadAllText(@"responses/ohlc.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            Dictionary<string, OHLCResponse> ohlcs = await kite.GetOHLCAsync(new string[] { "408065", "NSE:INFY" });

            Assert.AreEqual(ohlcs["408065"].LastPrice, 966.8m);
            Assert.AreEqual(ohlcs["408065"].Ohlc.Open, 966.6m);
        }

        [TestMethod]
        public async Task TestLTP()
        {
            string json = File.ReadAllText(@"responses/ltp.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            Dictionary<string, LTP> ltps = await kite.GetLTPAsync(new string[] { "NSE:INFY" });

            Assert.AreEqual(ltps["NSE:INFY"].LastPrice, (decimal)989.2);
        }

        [TestMethod]
        public async Task TestQuote()
        {
            string json = File.ReadAllText(@"responses/quote.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            Dictionary<string, Quote> quotes = await kite.GetQuoteAsync(new string[] { "NSE:ASHOKLEY", "NSE:NIFTY 50" });

            Assert.AreEqual(quotes["NSE:ASHOKLEY"].LastPrice, 76.6m);
            Assert.AreEqual(quotes["NSE:NIFTY 50"].LowerCircuitLimit, 0);
            Assert.AreEqual(quotes["NSE:ASHOKLEY"].Ohlc.Close, 76.45m);
            Assert.AreEqual(quotes["NSE:NIFTY 50"].Ohlc.Low, 11888.85m);
            Assert.AreEqual(quotes["NSE:ASHOKLEY"].Depth.Bids[0].Price, 76.5m);
            Assert.AreEqual(quotes["NSE:ASHOKLEY"].Depth.Offers[0].Price, 76.6m);
        }

        [TestMethod]
        public async Task TestOrders()
        {
            string json = File.ReadAllText(@"responses/orders.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Order> orders = await kite.GetOrdersAsync();

            Assert.AreEqual(orders[0].Price, 72);

            Assert.AreEqual(orders[2].Tag, "connect test order2");
            Assert.AreEqual(orders[2].Tags[1], "XXXXX");

            Assert.AreEqual(orders[3].ValidityTTL, 2);

            Assert.AreEqual(orders[3].Meta["iceberg"]["legs"].GetValue<int>(), 5);

            Assert.AreEqual(orders[0].AuctionNumber, 10);

            Assert.AreEqual(orders[4].Product, Constants.Product.MTF);
        }

        [TestMethod]
        public async Task TestGTTs()
        {
            string json = File.ReadAllText(@"responses/gtt_get_orders.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<GTT> gtts = await kite.GetGTTsAsync();

            Assert.AreEqual(gtts[0].Id, 105099);
            Assert.AreEqual(gtts[0].Condition?.TriggerValues[0], 102m);
            Assert.AreEqual(gtts[0].Condition?.TriggerValues[1], 103.7m);

        }

        [TestMethod]
        public async Task TestGTT()
        {
            string json = File.ReadAllText(@"responses/gtt_get_order.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            GTT gtt = await kite.GetGTTAsync(123);

            Assert.AreEqual(gtt.Id, 123);
        }

        [TestMethod]
        public async Task TestOrderInfo()
        {
            string json = File.ReadAllText(@"responses/orderinfo.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Order> orderhistory = await kite.GetOrderHistoryAsync("171124000819854");

            Assert.AreEqual(orderhistory[0].PendingQuantity, 100);
        }

        [TestMethod]
        public async Task TestInstruments()
        {
            string csv = File.ReadAllText(@"responses/instruments_all.csv", Encoding.UTF8);
            ms.SetResponse("text/csv", csv);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            var instrument = kite.GetInstrumentsAsync().ToBlockingEnumerable().First();

            Assert.AreEqual(instrument.InstrumentToken, (uint)3813889);
            Assert.AreEqual(instrument.LastPrice, 10.01m);
            Assert.AreEqual(instrument.Strike, 13.14m);
        }

        [TestMethod]
        public async Task TestHistoricalData()
        {
            string csv = File.ReadAllText(@"responses/historical.json", Encoding.UTF8);
            ms.SetResponse("application/json", csv);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            var historicalData = await kite.GetHistoricalDataAsync("3813889", DateTime.Now, DateTime.Now, "Minutes");

            Assert.HasCount(6, historicalData.Candles);
            Assert.AreEqual(886.7m, historicalData.Candles[0].Open);
            Assert.AreEqual(886.95m, historicalData.Candles[1].High);
            Assert.AreEqual(886.3m, historicalData.Candles[2].Low);
            Assert.AreEqual(886.75m, historicalData.Candles[3].Close);
            Assert.AreEqual((ulong)9414, historicalData.Candles[4].Volume);
        }

        [TestMethod]
        public async Task TestHistoricalDataWithOI()
        {
            string csv = File.ReadAllText(@"responses/historical-oi.json", Encoding.UTF8);
            ms.SetResponse("application/json", csv);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            var historicalData = await kite.GetHistoricalDataAsync("3813889", DateTime.Now, DateTime.Now, "Minutes");

            Assert.HasCount(6, historicalData.Candles);
            Assert.AreEqual((ulong)13667775, historicalData.Candles[0].OI);
        }

        [TestMethod]
        public async Task TestSegmentInstruments()
        {
            string csv = File.ReadAllText(@"responses/instruments_nse.csv", Encoding.UTF8);
            ms.SetResponse("text/csv", csv);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            var instrument = kite.GetInstrumentsAsync(Constants.Exchange.NSE).ToBlockingEnumerable().First();

            Assert.AreEqual(instrument.InstrumentToken, (uint)3813889);
        }

        [TestMethod]
        public async Task TestTrades()
        {
            string json = File.ReadAllText(@"responses/trades.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Trade> trades = await kite.GetOrderTradesAsync("151220000000000");

            Assert.AreEqual(trades[0].TradeId, "159918");
        }

        [TestMethod]
        public async Task TestMFSIPs()
        {
            string json = File.ReadAllText(@"responses/mf_sips.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<MFSIP> sips = await kite.GetMFSIPsAsync();

            Assert.AreEqual(sips[0].SIPId, "1234");
        }

        [TestMethod]
        public async Task TestMFSIP()
        {
            string json = File.ReadAllText(@"responses/mf_sip.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            MFSIP sip = await kite.GetMFSIPsAsync("1234");

            Assert.AreEqual(sip.SIPId, "1234");
        }

        [TestMethod]
        public async Task TestMFOrders()
        {
            string json = File.ReadAllText(@"responses/mf_orders.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<MFOrder> orders = await kite.GetMFOrdersAsync();

            Assert.AreEqual(orders[0].OrderId, "123123");
        }

        [TestMethod]
        public async Task TestMFOrder()
        {
            string json = File.ReadAllText(@"responses/mf_order.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            MFOrder order = await kite.GetMFOrdersAsync("123123");

            Assert.AreEqual(order.OrderId, "123123");
        }

        [TestMethod]
        public async Task TestMFHoldings()
        {
            string json = File.ReadAllText(@"responses/mf_holdings.json", Encoding.UTF8);
            ms.SetResponse("application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<MFHolding> holdings = await kite.GetMFHoldingsAsync();

            Assert.AreEqual(holdings[0].Folio, "123123/123");
        }

        [TestMethod]
        public void TestMFInstruments()
        {
            string csv = File.ReadAllText(@"responses/mf_instruments.csv", Encoding.UTF8);
            ms.SetResponse("text/csv", csv);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            var instrument = kite.GetMFInstrumentsAsync().ToBlockingEnumerable().First();

            Assert.AreEqual(instrument.Tradingsymbol, "INF209K01157");
            Assert.AreEqual(instrument.MinimumPurchaseAmount, 1234.0m);
            Assert.AreEqual(instrument.PurchaseAmountMultiplier, 13.14m);
        }
    }
}
