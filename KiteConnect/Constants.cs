using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiteConnect
{
    /// <summary>
    /// 
    /// </summary>
    public class Constants
    {

        // Products
        /// <summary>
        /// 
        /// </summary>
        public const string PRODUCT_MIS = "MIS";
        /// <summary>
        /// 
        /// </summary>
        public const string PRODUCT_CNC = "CNC";
        /// <summary>
        /// 
        /// </summary>
        public const string PRODUCT_NRML = "NRML";

        // Order types
        /// <summary>
        /// 
        /// </summary>
        public const string ORDER_TYPE_MARKET = "MARKET";
        /// <summary>
        /// 
        /// </summary>
        public const string ORDER_TYPE_LIMIT = "LIMIT";
        /// <summary>
        /// 
        /// </summary>
        public const string ORDER_TYPE_SLM = "SL-M";
        /// <summary>
        /// 
        /// </summary>
        public const string ORDER_TYPE_SL = "SL";

        // Order status
        /// <summary>
        /// 
        /// </summary>
        public const string ORDER_STATUS_COMPLETE = "COMPLETE";
        /// <summary>
        /// 
        /// </summary>
        public const string ORDER_STATUS_CANCELLED = "CANCELLED";
        /// <summary>
        /// 
        /// </summary>
        public const string ORDER_STATUS_REJECTED = "REJECTED";
        /// <summary>
        /// 
        /// </summary>

        // Varities
        /// <summary>
        /// 
        /// </summary>
        public const string VARIETY_REGULAR = "regular";
        /// <summary>
        /// 
        /// </summary>
        public const string VARIETY_BO = "bo";
        /// <summary>
        /// 
        /// </summary>
        public const string VARIETY_CO = "co";
        /// <summary>
        /// 
        /// </summary>
        public const string VARIETY_AMO = "amo";

        // Transaction type
        /// <summary>
        /// 
        /// </summary>
        public const string TRANSACTION_TYPE_BUY = "BUY";
        /// <summary>
        /// 
        /// </summary>
        public const string TRANSACTION_TYPE_SELL = "SELL";

        // Validity
        /// <summary>
        /// 
        /// </summary>
        public const string VALIDITY_DAY = "DAY";
        /// <summary>
        /// 
        /// </summary>
        public const string VALIDITY_IOC = "IOC";

        // Exchanges
        /// <summary>
        /// 
        /// </summary>
        public const string EXCHANGE_NSE = "NSE";
        /// <summary>
        /// 
        /// </summary>
        public const string EXCHANGE_BSE = "BSE";
        /// <summary>
        /// 
        /// </summary>
        public const string EXCHANGE_NFO = "NFO";
        /// <summary>
        /// 
        /// </summary>
        public const string EXCHANGE_CDS = "CDS";
        /// <summary>
        /// 
        /// </summary>
        public const string EXCHANGE_BFO = "BFO";
        /// <summary>
        /// 
        /// </summary>
        public const string EXCHANGE_MCX = "MCX";

        // Margins segments
        /// <summary>
        /// 
        /// </summary>
        public const string MARGIN_EQUITY = "equity";
        /// <summary>
        /// 
        /// </summary>
        public const string MARGIN_COMMODITY = "commodity";

        // Ticker modes
        /// <summary>
        /// 
        /// </summary>
        public const string MODE_FULL = "full";
        /// <summary>
        /// 
        /// </summary>
        public const string MODE_QUOTE = "quote";
        /// <summary>
        /// 
        /// </summary>
        public const string MODE_LTP = "ltp";

        // Positions
        /// <summary>
        /// 
        /// </summary>
        public const string POSITION_DAY = "day";
        /// <summary>
        /// 
        /// </summary>
        public const string POSITION_OVERNIGHT = "overnight";

        // Historical intervals
        /// <summary>
        /// 
        /// </summary>
        public const string INTERVAL_MINUTE = "minute";
        /// <summary>
        /// 
        /// </summary>
        public const string INTERVAL_3MINUTE = "3minute";
        /// <summary>
        /// 
        /// </summary>
        public const string INTERVAL_5MINUTE = "5minute";
        /// <summary>
        /// 
        /// </summary>
        public const string INTERVAL_10MINUTE = "10minute";
        /// <summary>
        /// 
        /// </summary>
        public const string INTERVAL_15MINUTE = "15minute";
        /// <summary>
        /// 
        /// </summary>
        public const string INTERVAL_30MINUTE = "30minute";
        /// <summary>
        /// 
        /// </summary>
        public const string INTERVAL_60MINUTE = "60minute";
        /// <summary>
        /// 
        /// </summary>
        public const string INTERVAL_DAY = "day";

        // GTT status
        /// <summary>
        /// 
        /// </summary>
        public const string GTT_ACTIVE = "active";
        /// <summary>
        /// 
        /// </summary>
        public const string GTT_TRIGGERED = "triggered";
        /// <summary>
        /// 
        /// </summary>
        public const string GTT_DISABLED = "disabled";
        /// <summary>
        /// 
        /// </summary>
        public const string GTT_EXPIRED = "expired";
        /// <summary>
        /// 
        /// </summary>
        public const string GTT_CANCELLED = "cancelled";
        /// <summary>
        /// 
        /// </summary>
        public const string GTT_REJECTED = "rejected";
        /// <summary>
        /// 
        /// </summary>
        public const string GTT_DELETED = "deleted";
        /// <summary>
        /// 
        /// </summary>


        // GTT trigger type
        /// <summary>
        /// 
        /// </summary>
        public const string GTT_TRIGGER_OCO = "two-leg";
        /// <summary>
        /// 
        /// </summary>
        public const string GTT_TRIGGER_SINGLE = "single";
    }
}
