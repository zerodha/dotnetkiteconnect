namespace KiteConnect
{
    public static class Constants
    {
        public static class Product
        {
            public const string MIS = "MIS";
            public const string CNC = "CNC";
            public const string NRML = "NRML";
            public const string MTF = "MTF";
        }

        public static class OrderType
        {
            public const string Market = "MARKET";
            public const string Limit = "LIMIT";
            public const string SLM = "SL-M";
            public const string SL = "SL";
        }

        public static class OrderStatus
        {
            public const string Complete = "COMPLETE";
            public const string Cancelled = "CANCELLED";
            public const string Rejected = "REJECTED";
        }

        public static class Variety
        {
            public const string Regular = "regular";
            public const string BO = "bo";
            public const string CO = "co";
            public const string AMO = "amo";
            public const string Iceberg = "iceberg";
            public const string Auction = "auction";
        }

        public static class Transaction
        {
            public const string Buy = "BUY";
            public const string Sell = "SELL";
        }

        public static class Validity
        {
            public const string Day = "DAY";
            public const string IOC = "IOC";
            public const string TTL = "TTL";
        }

        public static class Exchange
        {
            public const string NSE = "NSE";
            public const string BSE = "BSE";
            public const string NFO = "NFO";
            public const string CDS = "CDS";
            public const string BFO = "BFO";
            public const string MCX = "MCX";
        }

        public static class Margin
        {
            public const string Equity = "equity";
            public const string Commodity = "commodity";

            public static class Mode
            {
                public const string Compact = "compact";
            }
        }

        public static class TickerMode
        {
            public const string Full = "full";
            public const string Quote = "quote";
            public const string LTP = "ltp";
        }

        public static class Position
        {
            public const string Day = "day";
            public const string Overnight = "overnight";
        }

        public static class Interval
        {
            public const string Minute = "minute";
            public const string Minute3 = "3minute";
            public const string Minute5 = "5minute";
            public const string Minute10 = "10minute";
            public const string Minute15 = "15minute";
            public const string Minute30 = "30minute";
            public const string Minute60 = "60minute";
            public const string Day = "day";
        }

        public static class GTT
        {
            public static class Status
            {
                public const string Active = "active";
                public const string Triggered = "triggered";
                public const string Disabled = "disabled";
                public const string Expired = "expired";
                public const string Cancelled = "cancelled";
                public const string Rejected = "rejected";
                public const string Deleted = "deleted";
            }

            public static class Trigger
            {
                public const string OCO = "two-leg";
                public const string Single = "single";
            }
        }
    }
}
