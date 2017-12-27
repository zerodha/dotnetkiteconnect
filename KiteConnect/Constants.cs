using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiteConnect
{
    public class Constants
    {

        // Products
        public const string PRODUCT_MIS = "MIS";
        public const string PRODUCT_CNC = "CNC";
        public const string PRODUCT_NRML = "NRML";

        // Order types
        public const string ORDER_TYPE_MARKET = "MARKET";
        public const string ORDER_TYPE_LIMIT = "LIMIT";
        public const string ORDER_TYPE_SLM = "SL-M";
        public const string ORDER_TYPE_SL = "SL";

        // Varities
        public const string VARIETY_REGULAR = "regular";
        public const string VARIETY_BO = "bo";
        public const string VARIETY_CO = "co";
        public const string VARIETY_AMO = "amo";

        // Transaction type
        public const string TRANSACTION_TYPE_BUY = "BUY";
        public const string TRANSACTION_TYPE_SELL = "SELL";

        // Validity
        public const string VALIDITY_DAY = "DAY";
        public const string VALIDITY_IOC = "IOC";

        // Exchanges
        public const string EXCHANGE_NSE = "NSE";
        public const string EXCHANGE_BSE = "BSE";
        public const string EXCHANGE_NFO = "NFO";
        public const string EXCHANGE_CDS = "CDS";
        public const string EXCHANGE_BFO = "BFO";
        public const string EXCHANGE_MCX = "MCX";

        // Margins segments
        public const string MARGIN_EQUITY = "equity";
        public const string MARGIN_COMMODITY = "commodity";

        // Ticker modes
        public const string MODE_FULL = "full";
        public const string MODE_QUOTE = "quote";
        public const string MODE_LTP = "ltp";

        //Positions
        public const string POSITION_DAY = "day";
        public const string POSITION_OVERNIGHT = "overnight";

        //Historical intervals
        public const string INTERVAL_MINUTE = "minute";
        public const string INTERVAL_3MINUTE = "3minute";
        public const string INTERVAL_5MINUTE = "5minute";
        public const string INTERVAL_10MINUTE = "10minute";
        public const string INTERVAL_15MINUTE = "15minute";
        public const string INTERVAL_30MINUTE = "30minute";
        public const string INTERVAL_60MINUTE = "60minute";
        public const string INTERVAL_DAY = "day";

        //public const string Equity = "equity";
        //public const string Commodity = "commodity";
        //public const string Futures = "futures";
        //public const string Currency = "currency";

        //public const string Buy = "BUY";
        //public const string Sell = "SELL";

        //public const string Limit = "LIMIT";
        //public const string SL = "SL";
        //public const string SLM = "SL-M";

        //public const string MIS = "MIS";
        //public const string CNC = "CNC";
        //public const string NRML = "NRML";

        //public const string Regular = "regular";
        //public const string BO = "bo";
        //public const string CO = "co";
        //public const string AMO = "amo";

        //public const string Full = "full";
        //public const string Quote = "quote";
        //public const string LTP = "ltp";

        //public const string Day = "day";
        //public const string Overnight = "overnight";

        //public const string DAY = "DAY";
        //public const string IOC = "IOC";

        //public const string Minute = "minute";
        //public const string ThreeMinute = "3minute";
        //public const string Minute = "5minute";
        //public const string Minute = "10minute";
        //public const string Minute = "15minute";
        //public const string Minute = "30minute";
        //public const string Minute = "60minute";
        //public const string Equity = "day";

        //public const string Equity = "weekly";
        //public const string Equity = "monthly";
        //public const string Equity = "quarterly";

        //public const string Equity = "active";
        //public const string Equity = "paused";


        //static Dictionary<string, List<string>> values = new Dictionary<string, List<string>>
        //{
        //    [typeof(Segments).Name] = new List<string> { "equity", "commodity", "futures", "currency" },
        //    [typeof(TransactionTypes).Name] = new List<string> { "BUY", "SELL" },
        //    [typeof(OrderTypes).Name] = new List<string> { "MARKET", "LIMIT", "SL", "SL-M" },
        //    [typeof(ProductTypes).Name] = new List<string> { "MIS", "CNC", "NRML" },
        //    [typeof(VarietyTypes).Name] = new List<string> { "regular", "bo", "co", "amo" },
        //    [typeof(TickerModes).Name] = new List<string> { "full", "quote", "ltp" },
        //    [typeof(PositionTypes).Name] = new List<string> { "day", "overnight" },
        //    [typeof(ValidityTypes).Name] = new List<string> { "DAY", "IOC", "AMO" },
        //    [typeof(Exchanges).Name] = new List<string> { "NSE", "BSE", "NFO", "CDS", "MCX" },
        //    [typeof(CandleIntervals).Name] = new List<string> { "minute", "3minute", "5minute", "10minute", "15minute", "30minute", "60minute", "day" },
        //    [typeof(SIPFrequency).Name] = new List<string> { "weekly", "monthly", "quarterly" },
        //    [typeof(SIPStatus).Name] = new List<string> { "active", "paused" },
        //};

        //public static T ToEnum<T>(string value)
        //{
        //    return (T)(object)values[typeof(T).Name].IndexOf(value);
        //}

        //public static string ToValue<T>(T enumValue)
        //{
        //    if (enumValue == null)
        //        return "";
        //    var index = Enum.GetNames(typeof(T)).ToList().IndexOf(enumValue.ToString());
        //    return values[typeof(T).Name][index];
        //}
    }
}
