## 3.0.2

* Added GTT support
* Fixed issue in parsing quote data of index instruments

**New APIs:**

* GetGTTs
* GetGTT
* PlaceGTT
* ModifyGTT
* CancelGTT

## 3.0.1

* Disabled sending mode updates if the given list of tokens is empty

## 3.0.0

**New APIs:**

* GetInstrumentsMargins
* GetQuote
* GetOHLC
* GetLTP
* GetHistoricalData with timestamps
* GetProfile

**Changes in Ticker:**

* Use Access Token to authenticate instead of Public Token

```csharp
Ticker ticker = new Ticker(MyAPIKey, MyUserId, MyPublicToken);
```

becomes,

```csharp
Ticker ticker = new Ticker(MyAPIKey, MyUserId, MyAccessToken, Root: "wss://websocket.kite.trade/v3");
```

* Ticker now streams order updates
* New fields in Ticks

**Changes in function names:**

|    Verion 2     |       Version 3       |
| :-------------: | :-------------------: |
| SetSessionHook  | SetSessionExpiryHook  |
| InvalidateToken | InvalidateAccessToken |
|     Margins     |      GetMargins       |
|    GetOrder     |    GetOrderHistory    |
|    GetTrades    |    GetOrderTrades     |
|  ModifyProduct  |    ConvertPosition    |
|  GetHistorical  |   GetHistoricalData   |
|   GetMFOrder    |      GetMFOrders      |
|    GetMFSIP     |       GetMFSIPs       |

**Changes in User structure:**

|   Verion 2    | Version 3  |
| :-----------: | :--------: |
|       _       |   APIKey   |
| PasswordReset |     _      |
|   MemberId    |     _      |
|   OrderType   | OrderTypes |
|   Exchange    | Exchanges  |
|    Product    |  Products  |

**Changes in Position structure:**

|     Verion 2     |    Version 3    |
| :--------------: | :-------------: |
|        _         | DayBuyQuantity  |
|        _         |   DayBuyPrice   |
|        _         | DaySellQuantity |
|        _         |  DaySellPrice   |
| NetBuyAmountM2M  |        _        |
| NetSellAmountM2M |        _        |
|      BuyM2M      |   BuyM2MValue   |
|     SellM2M      |  SellM2MValue   |

**Changes in Quote structure:**

|   Verion 2    |      Version 3      |
| :-----------: | :-----------------: |
|       _       |   InstrumentToken   |
|       _       |      Timestamp      |
|       _       |    AveragePrice     |
|       _       | DayHighOpenInterest |
|       _       | DayLowOpenInterest  |
| ChangePercent |          _          |
|   LastTime    |    LastTradeTime    |


**Changes in Tick structure:**

| Verion 2 |      Version 3      |
| :------: | :-----------------: |
|    _     |    LastTradeTime    |
|    _     |    OpenInterest     |
|    _     | DayHighOpenInterest |
|    _     | DayLowOpenInterest  |
|    _     |      Timestamp      |
