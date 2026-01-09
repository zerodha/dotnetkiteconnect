using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;

namespace KiteConnect
{
    internal enum ResponseStatus
    {
        Success,
        Error,
    }

    internal class SucessResponse<T>
    {
        public ResponseStatus Status { get; set; }
        public T Data { get; set; }
    }

    internal class ErrorResponse
    {
        public ResponseStatus Status { get; set; }
        public string Message { get; set; }
        public string ErrorType { get; set; }
    }

    /// <summary>
    /// Tick data structure
    /// </summary>
    public class Tick
    {
        public string Mode { get; set; }
        public uint InstrumentToken { get; set; }
        public bool Tradable { get; set; }
        public decimal LastPrice { get; set; }
        public uint LastQuantity { get; set; }
        public decimal AveragePrice { get; set; }
        public uint Volume { get; set; }
        public uint BuyQuantity { get; set; }
        public uint SellQuantity { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Change { get; set; }
        public DepthItem[] Bids { get; set; }
        public DepthItem[] Offers { get; set; }

        // KiteConnect 3 Fields

        public DateTime? LastTradeTime { get; set; }
        public uint OI { get; set; }
        public uint OIDayHigh { get; set; }
        public uint OIDayLow { get; set; }
        public DateTime? Timestamp { get; set; }
    }

    /// <summary>
    /// Market depth item structure
    /// </summary>
    public class DepthItem
    {
        public DepthItem(Dictionary<string, dynamic> data)
        {
            Quantity = Convert.ToUInt32(data["quantity"]);
            Price = data["price"];
            Orders = Convert.ToUInt32(data["orders"]);
        }

        public uint Quantity { get; set; }
        public decimal Price { get; set; }
        public uint Orders { get; set; }
    }

    /// <summary>
    /// Historical structure
    /// </summary>
    public class Historical
    {
        public Historical(ArrayList data)
        {
            TimeStamp = Convert.ToDateTime(data[0]);
            Open = Convert.ToDecimal(data[1]);
            High = Convert.ToDecimal(data[2]);
            Low = Convert.ToDecimal(data[3]);
            Close = Convert.ToDecimal(data[4]);
            Volume = Convert.ToUInt64(data[5]);
            OI = data.Count > 6 ? Convert.ToUInt64(data[6]) : 0;
        }

        public DateTime TimeStamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public ulong Volume { get; set; }
        public ulong OI { get; set; }
    }

    /// <summary>
    /// Holding structure
    /// </summary>
    public class Holding
    {
        public Holding(Dictionary<string, dynamic> data)
        {
            try
            {
                Product = data["product"];
                Exchange = data["exchange"];
                Price = data["price"];
                LastPrice = data["last_price"];
                CollateralQuantity = Convert.ToInt32(data["collateral_quantity"]);
                PNL = data["pnl"];
                ClosePrice = data["close_price"];
                AveragePrice = data["average_price"];
                TradingSymbol = data["tradingsymbol"];
                CollateralType = data["collateral_type"];
                T1Quantity = Convert.ToInt32(data["t1_quantity"]);
                InstrumentToken = Convert.ToUInt32(data["instrument_token"]);
                ISIN = data["isin"];
                RealisedQuantity = Convert.ToInt32(data["realised_quantity"]);
                Quantity = Convert.ToInt32(data["quantity"]);
                UsedQuantity = Convert.ToInt32(data["used_quantity"]);
                AuthorisedQuantity = Convert.ToInt32(data["authorised_quantity"]);
                AuthorisedDate = Utils.StringToDate(data["authorised_date"]);
                Discrepancy = data["discrepancy"];
                MTF = new MTFHolding(data["mtf"]);
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public string Product { get; set; }
        public string Exchange { get; set; }
        public decimal Price { get; set; }
        public decimal LastPrice { get; set; }
        public decimal CollateralQuantity { get; set; }
        public decimal PNL { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal AveragePrice { get; set; }
        public string TradingSymbol { get; set; }
        public string CollateralType { get; set; }
        public decimal T1Quantity { get; set; }
        public uint InstrumentToken { get; set; }
        public string ISIN { get; set; }
        public decimal RealisedQuantity { get; set; }
        public decimal Quantity { get; set; }
        public decimal UsedQuantity { get; set; }
        public decimal AuthorisedQuantity { get; set; }
        public DateTime? AuthorisedDate { get; set; }
        public bool Discrepancy { get; set; }
        public MTFHolding MTF { get; set; }
    }

    /// <summary>
    /// MTF Holding structure
    /// </summary>
    public class MTFHolding
    {
        public MTFHolding(Dictionary<string, dynamic> data)
        {
            try
            {
                Quantity = data["quantity"];
                UsedQuantity = data["used_quantity"];
                AveragePrice = data["average_price"];
                Value = data["value"];
                InitialMargin = data["initial_margin"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public decimal Quantity { get; set; }
        public decimal UsedQuantity { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal Value { get; set; }
        public decimal InitialMargin { get; set; }
    }

    /// <summary>
    /// AuctionInstrument structure
    /// </summary>
    public class AuctionInstrument
    {
        public AuctionInstrument(Dictionary<string, dynamic> data)
        {
            try
            {
                TradingSymbol = data["tradingsymbol"];
                Exchange = data["exchange"];
                InstrumentToken = Convert.ToUInt32(data["instrument_token"]);
                ISIN = data["isin"];
                Product = data["product"];
                Price = data["price"];
                Quantity = Convert.ToInt32(data["quantity"]);
                T1Quantity = Convert.ToInt32(data["t1_quantity"]);
                RealisedQuantity = Convert.ToInt32(data["realised_quantity"]);
                AuthorisedQuantity = Convert.ToInt32(data["authorised_quantity"]);
                AuthorisedDate = Utils.StringToDate(data["authorised_date"]);
                OpeningQuantity = Convert.ToInt32(data["opening_quantity"]);
                CollateralQuantity = Convert.ToInt32(data["collateral_quantity"]);
                CollateralType = data["collateral_type"];
                Discrepancy = data["discrepancy"];
                AveragePrice = data["average_price"];
                LastPrice = data["last_price"];
                ClosePrice = data["close_price"];
                PNL = data["pnl"];
                DayChange = data["day_change"];
                DayChangePercentage = data["day_change_percentage"];
                AuctionNumber = data["auction_number"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public string TradingSymbol { get; set; }
        public string Exchange { get; set; }
        public uint InstrumentToken { get; set; }
        public string ISIN { get; set; }
        public string Product { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal T1Quantity { get; set; }
        public decimal RealisedQuantity { get; set; }
        public decimal AuthorisedQuantity { get; set; }
        public DateTime? AuthorisedDate { get; set; }
        public decimal OpeningQuantity { get; set; }
        public decimal CollateralQuantity { get; set; }
        public string CollateralType { get; set; }
        public bool Discrepancy { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal LastPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal PNL { get; set; }
        public decimal DayChange { get; set; }
        public decimal DayChangePercentage { get; set; }
        public string AuctionNumber { get; set; }
    }

    /// <summary>
    /// Available margin structure
    /// </summary>
    public class AvailableMargin
    {
        [JsonPropertyName("adhoc_margin")]
        public decimal AdHocMargin { get; set; }
        public decimal Cash { get; set; }
        public decimal Collateral { get; set; }
        public decimal IntradayPayin { get; set; }
    }

    /// <summary>
    /// Utilised margin structure
    /// </summary>
    public class UtilisedMargin
    {
        public decimal Debits { get; set; }
        public decimal Exposure { get; set; }

        [JsonPropertyName("m2m_realised")]
        public decimal M2MRealised { get; set; }

        [JsonPropertyName("m2m_unrealised")]
        public decimal M2MUnrealised { get; set; }
        public decimal OptionPremium { get; set; }
        public decimal Payout { get; set; }
        public decimal Span { get; set; }
        public decimal HoldingSales { get; set; }
        public decimal Turnover { get; set; }

    }

    /// <summary>
    /// UserMargin structure
    /// </summary>
    public class UserMargin
    {
        public bool Enabled { get; set; }
        public decimal Net { get; set; }
        public AvailableMargin Available { get; set; }
        public UtilisedMargin Utilised { get; set; }
    }

    /// <summary>
    /// User margins response structure
    /// </summary>
    public class UserMarginsResponse
    {
        public UserMargin Equity { get; set; }
        public UserMargin Commodity { get; set; }
    }

    /// <summary>
    /// OrderMarginParams structure
    /// </summary>
    public class OrderMarginParams
    {
        /// <summary>
        /// Exchange in which instrument is listed (Constants.Exchange.NSE, Constants.Exchange.BSE, etc.)
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// Tradingsymbol of the instrument  (ex. RELIANCE, INFY)
        /// </summary>
        public string TradingSymbol { get; set; }

        /// <summary>
        /// Transaction type (Constants.Transaction.Buy or Constants.Transaction.Sell)
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Order quantity
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Order Price
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Trigger price
        /// </summary>
        public decimal? TriggerPrice { get; set; }

        /// <summary>
        /// Product code (Constants.Product.CNC, Constants.Product.MIS, Constants.Product.NRML)
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// Order type (Constants.OrderType.Market, Constants.OrderType.SL, etc.)
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// Variety (Constants.Variety.Regular, Constants.Variety.AMO, etc.)
        /// </summary>
        public string Variety { get; set; }
    }

    /// <summary>
    /// OrderMargin structure
    /// </summary>
    public class OrderMargin
    {
        public OrderMargin(Dictionary<string, dynamic> data)
        {
            try
            {
                Type = data["type"];
                Exchange = data["exchange"];
                Tradingsymbol = data["tradingsymbol"];
                Total = data["total"];

                // available only in non compact mode
                OptionPremium = Utils.GetValueOrDefault(data, "option_premium", 0m);
                SPAN = Utils.GetValueOrDefault(data, "span", 0m);
                Exposure = Utils.GetValueOrDefault(data, "exposure", 0m);
                Additional = Utils.GetValueOrDefault(data, "additional", 0m);
                BO = Utils.GetValueOrDefault(data, "bo", 0m);
                Cash = Utils.GetValueOrDefault(data, "cash", 0m);
                VAR = Utils.GetValueOrDefault(data, "var", 0m);
                Leverage = Utils.GetValueOrDefault(data, "leverage", 0m);
                Charges = new OrderCharges(Utils.GetValueOrDefault(data, "charges", new Dictionary<string, dynamic>()));
                PNL = new OrderMarginPNL(Utils.GetValueOrDefault(data, "pnl", new Dictionary<string, dynamic>()));
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public string Type { get; set; }
        public string Exchange { get; set; }
        public string Tradingsymbol { get; set; }
        public decimal OptionPremium { get; set; }
        public decimal SPAN { get; set; }
        public decimal Exposure { get; set; }
        public decimal Additional { get; set; }
        public decimal BO { get; set; }
        public decimal Cash { get; set; }
        public decimal VAR { get; set; }
        public OrderMarginPNL PNL { get; set; }
        public OrderCharges Charges { get; set; }
        public decimal Leverage { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>
    /// ContractNoteParams structure
    /// </summary>
    public class ContractNoteParams
    {
        /// <summary>
        /// Order ID that is received in the orderbook
        /// </summary>
        public string OrderID { get; set; }

        /// <summary>
        /// Exchange in which instrument is listed (Constants.Exchange.NSE, Constants.Exchange.BSE, etc.)
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// Tradingsymbol of the instrument  (ex. RELIANCE, INFY)
        /// </summary>
        public string TradingSymbol { get; set; }

        /// <summary>
        /// Transaction type (Constants.Transaction.Buy or Constants.Transaction.Sell)
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Order quantity
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Average price
        /// </summary>
        public decimal? AveragePrice { get; set; }

        /// <summary>
        /// Product code (Constants.Product.CNC, Constants.Product.MIS, Constants.Product.NRML)
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// Order type (Constants.OrderType.Market, Constants.OrderType.SL, etc.)
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// Variety (Constants.Variety.Regular, Constants.Variety.AMO, etc.)
        /// </summary>
        public string Variety { get; set; }
    }

    /// <summary>
    /// ContractNote structure
    /// </summary>
    public class ContractNote
    {
        public ContractNote(Dictionary<string, dynamic> data)
        {
            try
            {
                Exchange = data["exchange"];
                TradingSymbol = data["tradingsymbol"];
                TransactionType = data["transaction_type"];
                Quantity = Convert.ToInt32(data["quantity"]);
                Price = Utils.GetValueOrDefault(data, "price", 0m);
                Product = data["product"];
                OrderType = data["order_type"];
                Variety = data["variety"];
                Charges = new OrderCharges(Utils.GetValueOrDefault(data, "charges", new Dictionary<string, dynamic>()));
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        /// <summary>
        /// Exchange in which instrument is listed (Constants.Exchange.NSE, Constants.Exchange.BSE, etc.)
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// Tradingsymbol of the instrument  (ex. RELIANCE, INFY)
        /// </summary>
        public string TradingSymbol { get; set; }

        /// <summary>
        /// Transaction type (Constants.Transaction.Buy or Constants.Transaction.Sell)
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Order quantity
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Order price
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Product code (Constants.Product.CNC, Constants.Product.MIS, Constants.Product.NRML)
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// Order type (Constants.OrderType.Market, Constants.OrderType.SL, etc.)
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// Variety (Constants.Variety.Regular, Constants.Variety.AMO, etc.)
        /// </summary>
        public string Variety { get; set; }

        /// <summary>
        /// Order charges
        /// </summary>
        public OrderCharges Charges { get; set; }
    }

    /// <summary>
    /// OrderCharges structure
    /// </summary>
    public class OrderCharges
    {
        public OrderCharges(Dictionary<string, dynamic> data)
        {
            try
            {
                TransactionTax = Utils.GetValueOrDefault(data, "transaction_tax", 0m);
                TransactionTaxType = Utils.GetValueOrDefault(data, "transaction_tax_type", "");
                ExchangeTurnoverCharge = Utils.GetValueOrDefault(data, "exchange_turnover_charge", 0m);
                SEBITurnoverCharge = Utils.GetValueOrDefault(data, "sebi_turnover_charge", 0m);
                Brokerage = Utils.GetValueOrDefault(data, "brokerage", 0m);
                StampDuty = Utils.GetValueOrDefault(data, "stamp_duty", 0m);
                Total = Utils.GetValueOrDefault(data, "total", 0m);
                GST = new OrderChargesGST(Utils.GetValueOrDefault(data, "gst", new Dictionary<string, dynamic>()));
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public decimal TransactionTax { get; set; }
        public string TransactionTaxType { get; set; }
        public decimal ExchangeTurnoverCharge { get; set; }
        public decimal SEBITurnoverCharge { get; set; }
        public decimal Brokerage { get; set; }
        public decimal StampDuty { get; set; }
        public decimal Total { get; set; }
        public OrderChargesGST GST { get; set; }
    }

    /// <summary>
    /// OrderChargesGST structure
    /// </summary>
    public class OrderChargesGST
    {
        public OrderChargesGST(Dictionary<string, dynamic> data)
        {
            try
            {
                IGST = Utils.GetValueOrDefault(data, "igst", 0m);
                CGST = Utils.GetValueOrDefault(data, "cgst", 0m);
                SGST = Utils.GetValueOrDefault(data, "sgst", 0m);
                Total = Utils.GetValueOrDefault(data, "total", 0m);
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public decimal IGST { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>
    /// BasketMargin structure
    /// </summary>
    public class BasketMargin
    {
        public BasketMargin(Dictionary<string, dynamic> data)
        {
            try
            {
                Initial = new OrderMargin(data["initial"]);
                Final = new OrderMargin(data["final"]);
                Orders = new List<OrderMargin>();

                foreach (Dictionary<string, dynamic> item in data["orders"])
                    Orders.Add(new OrderMargin(item));
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public OrderMargin Initial { get; set; }
        public OrderMargin Final { get; set; }
        public List<OrderMargin> Orders { get; set; }

    }


    /// <summary>
    /// OrderMarginPNL structure
    /// </summary>
    public class OrderMarginPNL
    {
        public OrderMarginPNL(Dictionary<string, dynamic> data)
        {
            try
            {
                Realised = Utils.GetValueOrDefault(data, "realised", 0m);
                Unrealised = Utils.GetValueOrDefault(data, "unrealised", 0m);
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public decimal Realised { get; set; }
        public decimal Unrealised { get; set; }
    }

    /// <summary>
    /// Position structure
    /// </summary>
    public class Position
    {
        public Position(Dictionary<string, dynamic> data)
        {
            try
            {
                Product = data["product"];
                OvernightQuantity = Convert.ToInt32(data["overnight_quantity"]);
                Exchange = data["exchange"];
                SellValue = data["sell_value"];
                BuyM2M = data["buy_m2m"];
                LastPrice = data["last_price"];
                TradingSymbol = data["tradingsymbol"];
                Realised = data["realised"];
                PNL = data["pnl"];
                Multiplier = data["multiplier"];
                SellQuantity = Convert.ToInt32(data["sell_quantity"]);
                SellM2M = data["sell_m2m"];
                BuyValue = data["buy_value"];
                BuyQuantity = Convert.ToInt32(data["buy_quantity"]);
                AveragePrice = data["average_price"];
                Unrealised = data["unrealised"];
                Value = data["value"];
                BuyPrice = data["buy_price"];
                SellPrice = data["sell_price"];
                M2M = data["m2m"];
                InstrumentToken = Convert.ToUInt32(data["instrument_token"]);
                ClosePrice = data["close_price"];
                Quantity = Convert.ToInt32(data["quantity"]);
                DayBuyQuantity = Convert.ToInt32(data["day_buy_quantity"]);
                DayBuyValue = data["day_buy_value"];
                DayBuyPrice = data["day_buy_price"];
                DaySellQuantity = Convert.ToInt32(data["day_sell_quantity"]);
                DaySellValue = data["day_sell_value"];
                DaySellPrice = data["day_sell_price"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }

        }

        public string Product { get; set; }
        public decimal OvernightQuantity { get; set; }
        public string Exchange { get; set; }
        public decimal SellValue { get; set; }
        public decimal BuyM2M { get; set; }
        public decimal LastPrice { get; set; }
        public string TradingSymbol { get; set; }
        public decimal Realised { get; set; }
        public decimal PNL { get; set; }
        public decimal Multiplier { get; set; }
        public decimal SellQuantity { get; set; }
        public decimal SellM2M { get; set; }
        public decimal BuyValue { get; set; }
        public decimal BuyQuantity { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal Unrealised { get; set; }
        public decimal Value { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal M2M { get; set; }
        public uint InstrumentToken { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal DayBuyQuantity { get; set; }
        public decimal DayBuyPrice { get; set; }
        public decimal DayBuyValue { get; set; }
        public decimal DaySellQuantity { get; set; }
        public decimal DaySellPrice { get; set; }
        public decimal DaySellValue { get; set; }
    }

    /// <summary>
    /// Position response structure
    /// </summary>
    public class PositionResponse
    {
        public PositionResponse(Dictionary<string, dynamic> data)
        {
            Day = new List<Position>();
            Net = new List<Position>();

            foreach (Dictionary<string, dynamic> item in data["day"])
                Day.Add(new Position(item));
            foreach (Dictionary<string, dynamic> item in data["net"])
                Net.Add(new Position(item));
        }

        public List<Position> Day { get; set; }
        public List<Position> Net { get; set; }
    }

    /// <summary>
    /// Order structure
    /// </summary>
    public class Order
    {
        public Order(Dictionary<string, dynamic> data)
        {
            try
            {
                AveragePrice = data["average_price"];
                CancelledQuantity = Convert.ToInt32(data["cancelled_quantity"]);
                DisclosedQuantity = Convert.ToInt32(data["disclosed_quantity"]);
                Exchange = data["exchange"];
                ExchangeOrderId = data["exchange_order_id"];
                ExchangeTimestamp = Utils.StringToDate(data["exchange_timestamp"]);
                FilledQuantity = Convert.ToInt32(data["filled_quantity"]);
                InstrumentToken = Convert.ToUInt32(data["instrument_token"]);
                OrderId = data["order_id"];
                OrderTimestamp = Utils.StringToDate(data["order_timestamp"]);
                OrderType = data["order_type"];
                ParentOrderId = data["parent_order_id"];
                PendingQuantity = Convert.ToInt32(data["pending_quantity"]);
                PlacedBy = data["placed_by"];
                Price = data["price"];
                Product = data["product"];
                Quantity = Convert.ToInt32(data["quantity"]);
                Status = data["status"];
                StatusMessage = data["status_message"];
                Tag = data["tag"];
                Tags = new List<string>();
                if (data.ContainsKey("tags"))
                {
                    Tags = ((data["tags"] ?? Tags) as ArrayList).Cast<string>().ToList();
                }
                Tradingsymbol = data["tradingsymbol"];
                TransactionType = data["transaction_type"];
                TriggerPrice = data["trigger_price"];
                Validity = data["validity"];
                ValidityTTL = 0;
                if (data.ContainsKey("validity_ttl"))
                {
                    ValidityTTL = Convert.ToInt32(data["validity_ttl"]);
                }
                Variety = data["variety"];

                AuctionNumber = 0;
                if (data.ContainsKey("auction_number"))
                {
                    AuctionNumber = Convert.ToInt32(data["auction_number"]);
                }

                Meta = new Dictionary<string, dynamic>();
                if (data.ContainsKey("meta"))
                {
                    Meta = data["meta"];
                }
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }

        }

        public decimal AveragePrice { get; set; }
        public decimal CancelledQuantity { get; set; }
        public decimal DisclosedQuantity { get; set; }
        public string Exchange { get; set; }
        public string ExchangeOrderId { get; set; }
        public DateTime? ExchangeTimestamp { get; set; }
        public decimal FilledQuantity { get; set; }
        public uint InstrumentToken { get; set; }
        public string OrderId { get; set; }
        public DateTime? OrderTimestamp { get; set; }
        public string OrderType { get; set; }
        public string ParentOrderId { get; set; }
        public decimal PendingQuantity { get; set; }
        public string PlacedBy { get; set; }
        public decimal Price { get; set; }
        public string Product { get; set; }
        public decimal Quantity { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public string Tag { get; set; }
        public List<string> Tags { get; set; }
        public string Tradingsymbol { get; set; }
        public string TransactionType { get; set; }
        public decimal TriggerPrice { get; set; }
        public string Validity { get; set; }
        public int ValidityTTL { get; set; }
        public int AuctionNumber { get; set; }
        public string Variety { get; set; }
        public Dictionary<string, dynamic> Meta { get; set; }
    }

    /// <summary>
    /// GTTOrder structure
    /// </summary>
    public class GTT
    {
        public GTT(Dictionary<string, dynamic> data)
        {
            try
            {
                Id = Convert.ToInt32(data["id"]);
                Condition = new GTTCondition(data["condition"]);
                TriggerType = data["type"];

                Orders = new List<GTTOrder>();
                foreach (Dictionary<string, dynamic> item in data["orders"])
                    Orders.Add(new GTTOrder(item));

                Status = data["status"];
                CreatedAt = Utils.StringToDate(data["created_at"]);
                UpdatedAt = Utils.StringToDate(data["updated_at"]);
                ExpiresAt = Utils.StringToDate(data["expires_at"]);
                Meta = new GTTMeta(data["meta"]);
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public int Id { get; set; }
        public GTTCondition? Condition { get; set; }
        public string TriggerType { get; set; }
        public List<GTTOrder> Orders { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public GTTMeta? Meta { get; set; }
    }

    /// <summary>
    /// GTTMeta structure
    /// </summary>
    public class GTTMeta
    {
        public GTTMeta(Dictionary<string, dynamic> data)
        {
            try
            {
                RejectionReason = data != null && data.ContainsKey("rejection_reason") ? data["rejection_reason"] : "";
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public string RejectionReason { get; set; }
    }

    /// <summary>
    /// GTTCondition structure
    /// </summary>
    public class GTTCondition
    {
        public GTTCondition(Dictionary<string, dynamic> data)
        {
            try
            {
                InstrumentToken = 0;
                if (data.ContainsKey("instrument_token"))
                {
                    InstrumentToken = Convert.ToUInt32(data["instrument_token"]);
                }
                Exchange = data["exchange"];
                TradingSymbol = data["tradingsymbol"];
                TriggerValues = (data["trigger_values"] as ArrayList).Cast<decimal>().ToList();
                LastPrice = data["last_price"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public uint InstrumentToken { get; set; }
        public string Exchange { get; set; }
        public string TradingSymbol { get; set; }
        public List<decimal> TriggerValues { get; set; }
        public decimal LastPrice { get; set; }
    }

    /// <summary>
    /// GTTOrder structure
    /// </summary>
    public class GTTOrder
    {
        public GTTOrder(Dictionary<string, dynamic> data)
        {
            try
            {
                TransactionType = data["transaction_type"];
                Product = data["product"];
                OrderType = data["order_type"];
                Quantity = Convert.ToInt32(data["quantity"]);
                Price = data["price"];
                Result = data["result"] == null ? null : new GTTResult(data["result"]);
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public string TransactionType { get; set; }
        public string Product { get; set; }
        public string OrderType { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public GTTResult? Result { get; set; }
    }

    /// <summary>
    /// GTTResult structure
    /// </summary>
    public class GTTResult
    {
        public GTTResult(Dictionary<string, dynamic> data)
        {
            try
            {
                OrderResult = data["order_result"] == null ? null : new GTTOrderResult(data["order_result"]);
                Timestamp = data["timestamp"];
                TriggeredAtPrice = data["triggered_at"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public GTTOrderResult? OrderResult { get; set; }
        public string Timestamp { get; set; }
        public decimal TriggeredAtPrice { get; set; }
    }

    /// <summary>
    /// GTTOrderResult structure
    /// </summary>
    public class GTTOrderResult
    {
        public GTTOrderResult(Dictionary<string, dynamic> data)
        {
            try
            {
                OrderId = data["order_id"];
                RejectionReason = data["rejection_reason"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public string OrderId { get; set; }
        public string RejectionReason { get; set; }
    }

    /// <summary>
    /// GTTParams structure
    /// </summary>
    public class GTTParams
    {
        public string TradingSymbol { get; set; }
        public string Exchange { get; set; }
        public uint InstrumentToken { get; set; }
        public string TriggerType { get; set; }
        public decimal LastPrice { get; set; }
        public List<GTTOrderParams> Orders { get; set; }
        public List<decimal> TriggerPrices { get; set; }
    }

    /// <summary>
    /// GTTOrderParams structure
    /// </summary>
    public class GTTOrderParams
    {
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        // Order type (LIMIT, SL, SL-M, MARKET)
        public string OrderType { get; set; }
        // Product code (NRML, MIS, CNC)
        public string Product { get; set; }
        // Transaction type (BUY, SELL)
        public string TransactionType { get; set; }
    }

    /// <summary>
    /// Instrument structure
    /// </summary>
    public class Instrument
    {
        public Instrument(Dictionary<string, dynamic> data)
        {
            try
            {
                InstrumentToken = Convert.ToUInt32(data["instrument_token"]);
                ExchangeToken = Convert.ToUInt32(data["exchange_token"]);
                TradingSymbol = data["tradingsymbol"];
                Name = data["name"];
                LastPrice = Utils.StringToDecimal(data["last_price"]);
                TickSize = Utils.StringToDecimal(data["tick_size"]);
                Expiry = Utils.StringToDate(data["expiry"]);
                InstrumentType = data["instrument_type"];
                Segment = data["segment"];
                Exchange = data["exchange"];
                Strike = Utils.StringToDecimal(data["strike"]);

                LotSize = Convert.ToUInt32(data["lot_size"]);
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }

        }

        public uint InstrumentToken { get; set; }
        public uint ExchangeToken { get; set; }
        public string TradingSymbol { get; set; }
        public string Name { get; set; }
        public decimal LastPrice { get; set; }
        public decimal TickSize { get; set; }
        public DateTime? Expiry { get; set; }
        public string InstrumentType { get; set; }
        public string Segment { get; set; }
        public string Exchange { get; set; }
        public decimal Strike { get; set; }
        public uint LotSize { get; set; }
    }

    /// <summary>
    /// Trade structure
    /// </summary>
    public class Trade
    {
        public Trade(Dictionary<string, dynamic> data)
        {
            try
            {
                TradeId = data["trade_id"];
                OrderId = data["order_id"];
                ExchangeOrderId = data["exchange_order_id"];
                Tradingsymbol = data["tradingsymbol"];
                Exchange = data["exchange"];
                InstrumentToken = Convert.ToUInt32(data["instrument_token"]);
                TransactionType = data["transaction_type"];
                Product = data["product"];
                AveragePrice = data["average_price"];
                Quantity = Convert.ToInt32(data["quantity"]);
                FillTimestamp = Utils.StringToDate(data["fill_timestamp"]);
                ExchangeTimestamp = Utils.StringToDate(data["exchange_timestamp"]);
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }

        }

        public string TradeId { get; set; }
        public string OrderId { get; set; }
        public string ExchangeOrderId { get; set; }
        public string Tradingsymbol { get; set; }
        public string Exchange { get; set; }
        public uint InstrumentToken { get; set; }
        public string TransactionType { get; set; }
        public string Product { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal Quantity { get; set; }
        public DateTime? FillTimestamp { get; set; }
        public DateTime? ExchangeTimestamp { get; set; }
    }

    /// <summary>
    /// Trigger range structure
    /// </summary>
    public class TrigerRange
    {
        public TrigerRange(Dictionary<string, dynamic> data)
        {
            try
            {
                InstrumentToken = Convert.ToUInt32(data["instrument_token"]);
                Lower = data["lower"];
                Upper = data["upper"];
                Percentage = data["percentage"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }

        }
        public uint InstrumentToken { get; set; }
        public decimal Lower { get; set; }
        public decimal Upper { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// User structure
    /// </summary>
    public class User
    {
        public User(Dictionary<string, dynamic> data)
        {
            try
            {
                APIKey = data["data"]["api_key"];
                Products = (string[])data["data"]["products"].ToArray(typeof(string));
                UserName = data["data"]["user_name"];
                UserShortName = data["data"]["user_shortname"];
                AvatarURL = data["data"]["avatar_url"];
                Broker = data["data"]["broker"];
                AccessToken = data["data"]["access_token"];
                PublicToken = data["data"]["public_token"];
                RefreshToken = data["data"]["refresh_token"];
                UserType = data["data"]["user_type"];
                UserId = data["data"]["user_id"];
                LoginTime = Utils.StringToDate(data["data"]["login_time"]);
                Exchanges = (string[])data["data"]["exchanges"].ToArray(typeof(string));
                OrderTypes = (string[])data["data"]["order_types"].ToArray(typeof(string));
                Email = data["data"]["email"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }

        }

        public string APIKey { get; set; }
        public string[] Products { get; set; }
        public string UserName { get; set; }
        public string UserShortName { get; set; }
        public string AvatarURL { get; set; }
        public string Broker { get; set; }
        public string AccessToken { get; set; }
        public string PublicToken { get; set; }
        public string RefreshToken { get; set; }
        public string UserType { get; set; }
        public string UserId { get; set; }
        public DateTime? LoginTime { get; set; }
        public string[] Exchanges { get; set; }
        public string[] OrderTypes { get; set; }
        public string Email { get; set; }
    }

    public class TokenSet
    {
        public TokenSet(Dictionary<string, dynamic> data)
        {
            try
            {
                UserId = data["data"]["user_id"];
                AccessToken = data["data"]["access_token"];
                RefreshToken = data["data"]["refresh_token"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// User structure
    /// </summary>
    public class Profile
    {
        public string[] Products { get; set; }
        public string UserName { get; set; }

        [JsonPropertyName("user_shortname")]
        public string UserShortName { get; set; }
        public string AvatarURL { get; set; }
        public string Broker { get; set; }
        public string UserType { get; set; }
        public string[] Exchanges { get; set; }
        public string[] OrderTypes { get; set; }
        public string Email { get; set; }
    }

    /// <summary>
    /// Quote structure
    /// </summary>
    public class Quote
    {
        public Quote(Dictionary<string, dynamic> data)
        {
            try
            {
                InstrumentToken = Convert.ToUInt32(data["instrument_token"]);
                Timestamp = Utils.StringToDate(data["timestamp"]);
                LastPrice = data["last_price"];

                Change = data["net_change"];

                Open = data["ohlc"]["open"];
                Close = data["ohlc"]["close"];
                Low = data["ohlc"]["low"];
                High = data["ohlc"]["high"];

                if (data.ContainsKey("last_quantity"))
                {
                    // Non index quote
                    LastQuantity = Convert.ToUInt32(data["last_quantity"]);
                    LastTradeTime = Utils.StringToDate(data["last_trade_time"]);
                    AveragePrice = data["average_price"];
                    Volume = Convert.ToUInt32(data["volume"]);

                    BuyQuantity = Convert.ToUInt32(data["buy_quantity"]);
                    SellQuantity = Convert.ToUInt32(data["sell_quantity"]);

                    OI = Convert.ToUInt32(data["oi"]);

                    OIDayHigh = Convert.ToUInt32(data["oi_day_high"]);
                    OIDayLow = Convert.ToUInt32(data["oi_day_low"]);

                    LowerCircuitLimit = data["lower_circuit_limit"];
                    UpperCircuitLimit = data["upper_circuit_limit"];

                    Bids = new List<DepthItem>();
                    Offers = new List<DepthItem>();

                    if (data["depth"]["buy"] != null)
                    {
                        foreach (Dictionary<string, dynamic> bid in data["depth"]["buy"])
                            Bids.Add(new DepthItem(bid));
                    }

                    if (data["depth"]["sell"] != null)
                    {
                        foreach (Dictionary<string, dynamic> offer in data["depth"]["sell"])
                            Offers.Add(new DepthItem(offer));
                    }
                }
                else
                {
                    // Index quote
                    LastQuantity = 0;
                    LastTradeTime = null;
                    AveragePrice = 0;
                    Volume = 0;

                    BuyQuantity = 0;
                    SellQuantity = 0;

                    OI = 0;

                    OIDayHigh = 0;
                    OIDayLow = 0;

                    LowerCircuitLimit = 0;
                    UpperCircuitLimit = 0;

                    Bids = new List<DepthItem>();
                    Offers = new List<DepthItem>();
                }
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }

        }

        public uint InstrumentToken { get; set; }
        public decimal LastPrice { get; set; }
        public uint LastQuantity { get; set; }
        public decimal AveragePrice { get; set; }
        public uint Volume { get; set; }
        public uint BuyQuantity { get; set; }
        public uint SellQuantity { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Change { get; set; }
        public decimal LowerCircuitLimit { get; set; }
        public decimal UpperCircuitLimit { get; set; }
        public List<DepthItem> Bids { get; set; }
        public List<DepthItem> Offers { get; set; }

        // KiteConnect 3 Fields

        public DateTime? LastTradeTime { get; set; }
        public uint OI { get; set; }
        public uint OIDayHigh { get; set; }
        public uint OIDayLow { get; set; }
        public DateTime? Timestamp { get; set; }
    }

    /// <summary>
    /// OHLC Quote structure
    /// </summary>
    public class OHLC
    {
        public OHLC(Dictionary<string, dynamic> data)
        {
            try
            {
                InstrumentToken = Convert.ToUInt32(data["instrument_token"]);
                LastPrice = data["last_price"];

                Open = data["ohlc"]["open"];
                Close = data["ohlc"]["close"];
                Low = data["ohlc"]["low"];
                High = data["ohlc"]["high"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }

        }
        public uint InstrumentToken { get; set; }
        public decimal LastPrice { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
    }

    /// <summary>
    /// LTP Quote structure
    /// </summary>
    public class LTP
    {
        public LTP(Dictionary<string, dynamic> data)
        {
            try
            {
                InstrumentToken = Convert.ToUInt32(data["instrument_token"]);
                LastPrice = data["last_price"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }

        }
        public uint InstrumentToken { get; set; }
        public decimal LastPrice { get; set; }
    }

    /// <summary>
    /// Mutual funds holdings structure
    /// </summary>
    public class MFHolding
    {
        public MFHolding(Dictionary<string, dynamic> data)
        {
            try
            {
                Quantity = data["quantity"];
                Fund = data["fund"];
                Folio = data["folio"];
                AveragePrice = data["average_price"];
                TradingSymbol = data["tradingsymbol"];
                LastPrice = data["last_price"];
                PNL = data["pnl"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }

        }

        public decimal Quantity { get; set; }
        public string Fund { get; set; }
        public string Folio { get; set; }
        public decimal AveragePrice { get; set; }
        public string TradingSymbol { get; set; }
        public decimal LastPrice { get; set; }
        public decimal PNL { get; set; }
    }

    /// <summary>
    /// Mutual funds instrument structure
    /// </summary>
    public class MFInstrument
    {
        public MFInstrument(Dictionary<string, dynamic> data)
        {
            try
            {
                TradingSymbol = data["tradingsymbol"];
                AMC = data["amc"];
                Name = data["name"];

                PurchaseAllowed = data["purchase_allowed"] == "1";
                RedemtpionAllowed = data["redemption_allowed"] == "1";

                MinimumPurchaseAmount = Utils.StringToDecimal(data["minimum_purchase_amount"]);
                PurchaseAmountMultiplier = Utils.StringToDecimal(data["purchase_amount_multiplier"]);
                MinimumAdditionalPurchaseAmount = Utils.StringToDecimal(data["minimum_additional_purchase_amount"]);
                MinimumRedemptionQuantity = Utils.StringToDecimal(data["minimum_redemption_quantity"]);
                RedemptionQuantityMultiplier = Utils.StringToDecimal(data["redemption_quantity_multiplier"]);
                LastPrice = Utils.StringToDecimal(data["last_price"]);

                DividendType = data["dividend_type"];
                SchemeType = data["scheme_type"];
                Plan = data["plan"];
                SettlementType = data["settlement_type"];
                LastPriceDate = Utils.StringToDate(data["last_price_date"]);
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }

        }

        public string TradingSymbol { get; set; }
        public string AMC { get; set; }
        public string Name { get; set; }

        public bool PurchaseAllowed { get; set; }
        public bool RedemtpionAllowed { get; set; }

        public decimal MinimumPurchaseAmount { get; set; }
        public decimal PurchaseAmountMultiplier { get; set; }
        public decimal MinimumAdditionalPurchaseAmount { get; set; }
        public decimal MinimumRedemptionQuantity { get; set; }
        public decimal RedemptionQuantityMultiplier { get; set; }
        public decimal LastPrice { get; set; }

        public string DividendType { get; set; }
        public string SchemeType { get; set; }
        public string Plan { get; set; }
        public string SettlementType { get; set; }
        public DateTime? LastPriceDate { get; set; }
    }

    /// <summary>
    /// Mutual funds order structure
    /// </summary>
    public class MFOrder
    {
        public MFOrder(Dictionary<string, dynamic> data)
        {
            try
            {
                StatusMessage = data["status_message"];
                PurchaseType = data["purchase_type"];
                PlacedBy = data["placed_by"];
                Amount = data["amount"];
                Quantity = data["quantity"];
                SettlementId = data["settlement_id"];
                OrderTimestamp = Utils.StringToDate(data["order_timestamp"]);
                AveragePrice = data["average_price"];
                TransactionType = data["transaction_type"];
                ExchangeOrderId = data["exchange_order_id"];
                ExchangeTimestamp = Utils.StringToDate(data["exchange_timestamp"]);
                Fund = data["fund"];
                Variety = data["variety"];
                Folio = data["folio"];
                Tradingsymbol = data["tradingsymbol"];
                Tag = data["tag"];
                OrderId = data["order_id"];
                Status = data["status"];
                LastPrice = data["last_price"];
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }

        }

        public string StatusMessage { get; set; }
        public string PurchaseType { get; set; }
        public string PlacedBy { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
        public string SettlementId { get; set; }
        public DateTime? OrderTimestamp { get; set; }
        public decimal AveragePrice { get; set; }
        public string TransactionType { get; set; }
        public string ExchangeOrderId { get; set; }
        public DateTime? ExchangeTimestamp { get; set; }
        public string Fund { get; set; }
        public string Variety { get; set; }
        public string Folio { get; set; }
        public string Tradingsymbol { get; set; }
        public string Tag { get; set; }
        public string OrderId { get; set; }
        public string Status { get; set; }
        public decimal LastPrice { get; set; }
    }

    /// <summary>
    /// Mutual funds SIP structure
    /// </summary>
    public class MFSIP
    {
        public MFSIP(Dictionary<string, dynamic> data)
        {
            try
            {
                DividendType = data["dividend_type"];
                PendingInstalments = Convert.ToInt32(data["pending_instalments"]);
                Created = Utils.StringToDate(data["created"]);
                LastInstalment = Utils.StringToDate(data["last_instalment"]);
                TransactionType = data["transaction_type"];
                Frequency = data["frequency"];
                InstalmentDate = Convert.ToInt32(data["instalment_date"]);
                Fund = data["fund"];
                SIPId = data["sip_id"];
                Tradingsymbol = data["tradingsymbol"];
                Tag = data["tag"];
                InstalmentAmount = Convert.ToInt32(data["instalment_amount"]);
                Instalments = Convert.ToInt32(data["instalments"]);
                Status = data["status"];
                OrderId = data.ContainsKey(("order_id")) ? data["order_id"] : "";
            }
            catch (Exception e)
            {
                throw new DataException(e.Message + " " + Utils.JsonSerialize(data), HttpStatusCode.OK, e);
            }
        }

        public string DividendType { get; set; }
        public int PendingInstalments { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastInstalment { get; set; }
        public string TransactionType { get; set; }
        public string Frequency { get; set; }
        public int InstalmentDate { get; set; }
        public string Fund { get; set; }
        public string SIPId { get; set; }
        public string Tradingsymbol { get; set; }
        public string Tag { get; set; }
        public decimal InstalmentAmount { get; set; }
        public int Instalments { get; set; }
        public string Status { get; set; }
        public string OrderId { get; set; }
    }

}
