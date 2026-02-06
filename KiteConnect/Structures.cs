using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Nodes;
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

    internal class WebsocketMessage
    {
        public string Type { get; set; }
    }

    internal class WebsocketMessage<T> : WebsocketMessage
    {
        public T Data { get; set; }
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
        public uint Quantity { get; set; }
        public decimal Price { get; set; }
        public uint Orders { get; set; }
    }

    /// <summary>
    /// Historical Response structure
    /// </summary>
    public class HistoricalResponse
    {
        public List<Candle> Candles { get; set; }
    }

    /// <summary>
    /// Candle structure
    /// </summary>
    public class Candle
    {
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
        public string Product { get; set; }
        public string Exchange { get; set; }
        public decimal Price { get; set; }
        public decimal LastPrice { get; set; }
        public decimal CollateralQuantity { get; set; }
        public decimal PNL { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal AveragePrice { get; set; }
        public string Tradingsymbol { get; set; }
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
        public string Tradingsymbol { get; set; }
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
        public string Tradingsymbol { get; set; }

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
        public string Tradingsymbol { get; set; }

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
        /// <summary>
        /// Exchange in which instrument is listed (Constants.Exchange.NSE, Constants.Exchange.BSE, etc.)
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// Tradingsymbol of the instrument  (ex. RELIANCE, INFY)
        /// </summary>
        public string Tradingsymbol { get; set; }

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
        public decimal Price { get; set; }

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
        public OrderMargin Initial { get; set; }
        public OrderMargin Final { get; set; }
        public List<OrderMargin> Orders { get; set; }

    }


    /// <summary>
    /// OrderMarginPNL structure
    /// </summary>
    public class OrderMarginPNL
    {
        public decimal Realised { get; set; }
        public decimal Unrealised { get; set; }
    }

    /// <summary>
    /// Position structure
    /// </summary>
    public class Position
    {
        public string Product { get; set; }
        public decimal OvernightQuantity { get; set; }
        public string Exchange { get; set; }
        public decimal SellValue { get; set; }

        [JsonPropertyName("buy_m2m")]
        public decimal BuyM2M { get; set; }
        public decimal LastPrice { get; set; }
        public string Tradingsymbol { get; set; }
        public decimal Realised { get; set; }
        public decimal PNL { get; set; }
        public decimal Multiplier { get; set; }
        public decimal SellQuantity { get; set; }

        [JsonPropertyName("sell_m2m")]
        public decimal SellM2M { get; set; }
        public decimal BuyValue { get; set; }
        public decimal BuyQuantity { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal Unrealised { get; set; }
        public decimal Value { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }

        [JsonPropertyName("m2m")]
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
        public List<Position> Day { get; set; }
        public List<Position> Net { get; set; }
    }

    /// <summary>
    /// Order structure
    /// </summary>
    public class Order
    {
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
        public JsonNode Meta { get; set; }
    }

    /// <summary>
    /// GTTOrder structure
    /// </summary>
    public class GTT
    {
        public int Id { get; set; }
        public GTTCondition? Condition { get; set; }

        [JsonPropertyName("type")]
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
        public string RejectionReason { get; set; }
    }

    /// <summary>
    /// GTTCondition structure
    /// </summary>
    public class GTTCondition
    {
        public uint InstrumentToken { get; set; }
        public string Exchange { get; set; }
        public string Tradingsymbol { get; set; }
        public List<decimal> TriggerValues { get; set; }
        public decimal LastPrice { get; set; }
    }

    /// <summary>
    /// GTTOrder structure
    /// </summary>
    public class GTTOrder
    {
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
        //TODO missing some documented fields
        public GTTOrderResult? OrderResult { get; set; }
        public string Timestamp { get; set; }

        [JsonPropertyName("triggered_at")]
        public decimal TriggeredAtPrice { get; set; }
    }

    /// <summary>
    /// GTTOrderResult structure
    /// </summary>
    public class GTTOrderResult
    {
        public string OrderId { get; set; }
        public string RejectionReason { get; set; }
    }

    /// <summary>
    /// GTTParams structure
    /// </summary>
    public class GTTParams
    {
        public string Tradingsymbol { get; set; }
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
        [Name("instrument_token")]
        public uint InstrumentToken { get; set; }

        [Name("exchange_token")]
        public uint ExchangeToken { get; set; }

        [Name("tradingsymbol")]
        public string Tradingsymbol { get; set; }

        [Name("name")]
        public string Name { get; set; }

        [Name("last_price")]
        public decimal LastPrice { get; set; }

        [Name("tick_size")]
        public decimal TickSize { get; set; }

        [Name("expiry")]
        public DateTime? Expiry { get; set; }

        [Name("instrument_type")]
        public string InstrumentType { get; set; }

        [Name("segment")]
        public string Segment { get; set; }

        [Name("exchange")]
        public string Exchange { get; set; }

        [Name("strike")]
        public decimal Strike { get; set; }

        [Name("lot_size")]
        public uint LotSize { get; set; }
    }

    /// <summary>
    /// Trade structure
    /// </summary>
    public class Trade
    {
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
        public string APIKey { get; set; }
        public string[] Products { get; set; }
        public string UserName { get; set; }

        [JsonPropertyName("user_shortname")]
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
        public uint InstrumentToken { get; set; }
        public decimal LastPrice { get; set; }
        public uint LastQuantity { get; set; }
        public decimal AveragePrice { get; set; }
        public uint Volume { get; set; }
        public uint BuyQuantity { get; set; }
        public uint SellQuantity { get; set; }
        public OHLC Ohlc { get; set; }

        [JsonPropertyName("net_change")]
        public decimal Change { get; set; }
        public decimal LowerCircuitLimit { get; set; }
        public decimal UpperCircuitLimit { get; set; }
        public QuoteDepth Depth { get; set; }

        // KiteConnect 3 Fields

        public DateTime? LastTradeTime { get; set; }
        public uint OI { get; set; }
        public uint OIDayHigh { get; set; }
        public uint OIDayLow { get; set; }
        public DateTime? Timestamp { get; set; }
    }

    public class QuoteDepth
    {
        [JsonPropertyName("buy")]
        public List<DepthItem> Bids { get; set; }
        [JsonPropertyName("sell")]
        public List<DepthItem> Offers { get; set; }
    }

    public class OHLC
    {
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }

    /// <summary>
    /// OHLC Quote structure
    /// </summary>
    public class OHLCResponse
    {
        public uint InstrumentToken { get; set; }
        public decimal LastPrice { get; set; }
        public OHLC Ohlc { get; set; }
    }

    /// <summary>
    /// LTP Quote structure
    /// </summary>
    public class LTP
    {
        public uint InstrumentToken { get; set; }
        public decimal LastPrice { get; set; }
    }

    /// <summary>
    /// Mutual funds holdings structure
    /// </summary>
    public class MFHolding
    {
        public decimal Quantity { get; set; }
        public string Fund { get; set; }
        public string Folio { get; set; }
        public decimal AveragePrice { get; set; }
        public string Tradingsymbol { get; set; }
        public decimal LastPrice { get; set; }
        public decimal PNL { get; set; }
    }

    /// <summary>
    /// Mutual funds instrument structure
    /// </summary>
    public class MFInstrument
    {
        [Name("tradingsymbol")]
        public string Tradingsymbol { get; set; }

        [Name("amc")]
        public string AMC { get; set; }

        [Name("name")]
        public string Name { get; set; }

        [Name("purchase_allowed")]
        public string PurchaseAllowedString { get; set; }
        public bool PurchaseAllowed => PurchaseAllowedString == "1";

        [Name("redemption_allowed")]
        public string RedemtpionAllowedString { get; set; }
        public bool RedemtpionAllowed => RedemtpionAllowedString == "1";

        [Name("minimum_purchase_amount")]
        public decimal MinimumPurchaseAmount { get; set; }

        [Name("purchase_amount_multiplier")]
        public decimal PurchaseAmountMultiplier { get; set; }

        [Name("minimum_additional_purchase_amount")]
        public decimal MinimumAdditionalPurchaseAmount { get; set; }

        [Name("minimum_redemption_quantity")]
        public decimal MinimumRedemptionQuantity { get; set; }

        [Name("redemption_quantity_multiplier")]
        public decimal RedemptionQuantityMultiplier { get; set; }

        [Name("last_price")]
        public decimal LastPrice { get; set; }

        [Name("dividend_type")]
        public string DividendType { get; set; }

        [Name("scheme_type")]
        public string SchemeType { get; set; }

        [Name("plan")]
        public string Plan { get; set; }

        [Name("settlement_type")]
        public string SettlementType { get; set; }

        [Name("last_price_date")]
        public DateTime? LastPriceDate { get; set; }
    }

    /// <summary>
    /// Mutual funds order structure
    /// </summary>
    public class MFOrder
    {
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

    public class OrderResponse
    {
        public string OrderId { get; set; }
    }

    public class GTTResponse
    {
        public int TriggerId { get; set; }
    }
}
