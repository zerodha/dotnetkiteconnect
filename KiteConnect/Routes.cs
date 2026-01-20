namespace KiteConnect
{
    public static class Routes
    {
        public const string Parameters = "/parameters";

        public static class API
        {
            public const string Token = "/session/token";
            public const string Refresh = "/session/refresh_token";
        }

        public static class Instrument
        {
            public const string Margins = "/margins/{segment}";
        }

        public static class Order
        {
            public const string AllOrders = "/orders";
            public const string AllTrades = "/trades";
            public const string Margins = "/margins/orders";
            public const string ContractNote = "/charges/orders";
            public const string History = "/orders/{order_id}";
            public const string Place = "/orders/{variety}";
            public const string Modify = "/orders/{variety}/{order_id}";
            public const string Cancel = "/orders/{variety}/{order_id}";
            public const string Trades = "/orders/{order_id}/trades";
        }

        public static class Basket
        {
            public const string Margins = "/margins/basket";
        }

        public static class User
        {
            public const string Profile = "/user/profile";
            public const string Margins = "/user/margins";
            public const string SegmentMargins = "/user/margins/{segment}";
        }

        public static class GTT
        {
            public const string AllGTTs = "/gtt/triggers";
            public const string Place = "/gtt/triggers";
            public const string Info = "/gtt/triggers/{id}";
            public const string Modify = "/gtt/triggers/{id}";
            public const string Delete = "/gtt/triggers/{id}";
        }

        public static class Portfolio
        {
            public const string Positions = "/portfolio/positions";
            public const string Holdings = "/portfolio/holdings";
            public const string ModifyPositions = "/portfolio/positions";
            public const string AuctionInstruments = "/portfolio/holdings/auctions";
        }

        public static class Market
        {
            public const string AllInstruments = "/instruments";
            public const string Instruments = "/instruments/{exchange}";
            public const string Quote = "/quote";
            public const string OHLC = "/quote/ohlc";
            public const string LTP = "/quote/ltp";
            public const string Historical = "/instruments/historical/{instrument_token}/{interval}";
            public const string TriggerRange = "/instruments/trigger_range/{transaction_type}";
        }

        public static class MutualFunds
        {
            public const string Orders = "/mf/orders";
            public const string Order = "/mf/orders/{order_id}";
            public const string PlaceOrder = "/mf/orders";
            public const string CancelOrder = "/mf/orders/{order_id}";
            public const string SIPs = "/mf/sips";
            public const string PlaceSIP = "/mf/sips";
            public const string CancelSIP = "/mf/sips/{sip_id}";
            public const string ModifySIP = "/mf/sips/{sip_id}";
            public const string SIP = "/mf/sips/{sip_id}";
            public const string Instruments = "/mf/instruments";
            public const string Holdings = "/mf/holdings";
        }
    }
}