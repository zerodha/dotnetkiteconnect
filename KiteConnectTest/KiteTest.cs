using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KiteConnect;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KiteConnectTest
{
    [TestClass]
    public class KiteTest
    {
        [TestMethod]
        public void TestSetAccessToken()
        {
            Kite kite = new Kite("apikey");
            kite.SetAccessToken("access_token");
            Assert.ThrowsException<TokenException>(() => kite.GetPositions());
        }


        [TestMethod]
        public void TestProfile()
        {
            string json = File.ReadAllText(@"responses\profile.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            Profile profile = kite.GetProfile();
            Assert.AreEqual(profile.Email, "xxxyyy@gmail.com");
            ms.Stop();
        }

        [TestMethod]
        public void TestPositions()
        {
            string json = File.ReadAllText(@"responses\positions.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            PositionResponse positionResponse = kite.GetPositions();
            Assert.AreEqual(positionResponse.Net[0].TradingSymbol, "LEADMINI17DECFUT");
            Assert.AreEqual(positionResponse.Day[0].TradingSymbol, "GOLDGUINEA17DECFUT");
            ms.Stop();
        }

        [TestMethod]
        public void TestHoldings()
        {
            string json = File.ReadAllText(@"responses\holdings.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Holding> holdings = kite.GetHoldings();
            Assert.AreEqual(holdings.Count, 1);
            ms.Stop();
        }

        [TestMethod]
        public void TestMargins()
        {
            string json = File.ReadAllText(@"responses\margins.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            UserMarginsResponse margins = kite.GetMargins();

            Assert.AreEqual(margins.Equity.Net, (decimal)1697.7);
            Assert.AreEqual(margins.Commodity.Net, (decimal)-8676.296);
            ms.Stop();
        }

        [TestMethod]
        public void TestEquityMargins()
        {
            string json = File.ReadAllText(@"responses\equity_margins.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            UserMargin margin = kite.GetMargins("equity");

            Assert.AreEqual(margin.Net, (decimal)1812.3535);
            ms.Stop();
        }

        [TestMethod]
        public void TestCommodityMargins()
        {
            string json = File.ReadAllText(@"responses\equity_margins.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            UserMargin margin = kite.GetMargins("commodity");

            Assert.AreEqual(margin.Net, (decimal)1812.3535);
            ms.Stop();
        }

        [TestMethod]
        public void TestOHLC()
        {
            string json = File.ReadAllText(@"responses\ohlc.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            Dictionary<string, OHLC> ohlcs = kite.GetOHLC(new string[] { "408065", "NSE:INFY" });

            Assert.AreEqual(ohlcs["408065"].LastPrice, (decimal)966.8);
            ms.Stop();
        }

        [TestMethod]
        public void TestLTP()
        {
            string json = File.ReadAllText(@"responses\ltp.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            Dictionary<string, LTP> ltps = kite.GetLTP(new string[] { "NSE:INFY" });

            Assert.AreEqual(ltps["NSE:INFY"].LastPrice, (decimal)989.2);
            ms.Stop();
        }

        [TestMethod]
        public void TestQuote()
        {
            string json = File.ReadAllText(@"responses\quote.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            Dictionary<string, Quote> quotes = kite.GetQuote(new string[] { "NSE:ASHOKLEY", "NSE:NIFTY 50" });

            Assert.AreEqual(quotes["NSE:ASHOKLEY"].LastPrice, (decimal)76.6);
            Assert.AreEqual(quotes["NSE:NIFTY 50"].LowerCircuitLimit, 0);
            ms.Stop();
        }

        [TestMethod]
        public void TestOrders()
        {
            string json = File.ReadAllText(@"responses\orders.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Order> orders = kite.GetOrders();

            Assert.AreEqual(orders[0].Price, 90);
            ms.Stop();
        }

        [TestMethod]
        public void TestGTTs()
        {
            string json = File.ReadAllText(@"responses\gtt_get_orders.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<GTT> gtts = kite.GetGTTs();

            Assert.AreEqual(gtts[0].Id, 105099);
            ms.Stop();
        }

        [TestMethod]
        public void TestGTT()
        {
            string json = File.ReadAllText(@"responses\gtt_get_order.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            GTT gtt = kite.GetGTT(123);

            Assert.AreEqual(gtt.Id, 123);
            ms.Stop();
        }

        [TestMethod]
        public void TestOrderInfo()
        {
            string json = File.ReadAllText(@"responses\orderinfo.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Order> orderhistory = kite.GetOrderHistory("171124000819854");

            Assert.AreEqual(orderhistory[0].PendingQuantity, 100);
            ms.Stop();
        }

        [TestMethod]
        public void TestInstruments()
        {
            string csv = File.ReadAllText(@"responses\instruments_all.csv", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "text/csv", csv);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Instrument> instruments = kite.GetInstruments();

            Assert.AreEqual(instruments[0].InstrumentToken, (uint)3813889);
            ms.Stop();
        }

        [TestMethod]
        public void TestSegmentInstruments()
        {
            string csv = File.ReadAllText(@"responses\instruments_nse.csv", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "text/csv", csv);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Instrument> instruments = kite.GetInstruments(Constants.EXCHANGE_NSE);

            Assert.AreEqual(instruments[0].InstrumentToken, (uint)3813889);
            ms.Stop();
        }

        [TestMethod]
        public void TestTrades()
        {
            string json = File.ReadAllText(@"responses\trades.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<Trade> trades = kite.GetOrderTrades("151220000000000");

            Assert.AreEqual(trades[0].TradeId, "159918");
            ms.Stop();
        }

        [TestMethod]
        public void TestMFSIPs()
        {
            string json = File.ReadAllText(@"responses\mf_sips.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<MFSIP> sips = kite.GetMFSIPs();

            Assert.AreEqual(sips[0].SIPId, "1234");
            ms.Stop();
        }

        [TestMethod]
        public void TestMFSIP()
        {
            string json = File.ReadAllText(@"responses\mf_sip.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            MFSIP sip = kite.GetMFSIPs("1234");

            Assert.AreEqual(sip.SIPId, "1234");
            ms.Stop();
        }

        [TestMethod]
        public void TestMFOrders()
        {
            string json = File.ReadAllText(@"responses\mf_orders.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<MFOrder> orders = kite.GetMFOrders();

            Assert.AreEqual(orders[0].OrderId, "123123");
            ms.Stop();
        }

        [TestMethod]
        public void TestMFOrder()
        {
            string json = File.ReadAllText(@"responses\mf_order.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            MFOrder order = kite.GetMFOrders("123123");

            Assert.AreEqual(order.OrderId, "123123");
            ms.Stop();
        }

        [TestMethod]
        public void TestMFHoldings()
        {
            string json = File.ReadAllText(@"responses\mf_holdings.json", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "application/json", json);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<MFHolding> holdings = kite.GetMFHoldings();

            Assert.AreEqual(holdings[0].Folio, "123123/123");
            ms.Stop();
        }

        [TestMethod]
        public void TestMFInstruments()
        {
            string csv = File.ReadAllText(@"responses\mf_instruments.csv", Encoding.UTF8);
            MockServer ms = new MockServer("http://localhost:8080/", "text/csv", csv);
            Kite kite = new Kite("apikey", Root: "http://localhost:8080");
            List<MFInstrument> instruments = kite.GetMFInstruments();

            Assert.AreEqual(instruments[0].TradingSymbol, "INF209K01157");
            ms.Stop();
        }
    }
}
