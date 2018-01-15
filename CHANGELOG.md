## Migrating from Kite Connect v2 to v3

### New APIs

* GetInstrumentsMargins
* GetQuote
* GetOHLC
* GetLTP
* GetHistoricalData with timestamps
* GetProfile
* InvalidateRefreshToken
* RenewAccessToken

### Changes in Ticker

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

### Changes in function names:

| Verion 2 | Version 3 |
| :---: | :---: |
| RequestAccessToken | GenerateSession |
| SetSessionHook | SetSessionExpiryHook |
| InvalidateToken | InvalidateAccessToken |
| Margins | GetMargins |
| GetOrder | GetOrderHistory |
| GetTrades | GetOrderTrades |
| ModifyProduct | ConvertPosition |
| GetHistorical | GetHistoricalData |
| GetMFOrder | GetMFOrders |
| GetMFSIP | GetMFSIPs |

### Changes in User structure

| Verion 2 | Version 3 |
| :---: | :---: |
| _ | APIKey |
| PasswordReset | _ |
| MemberId | _ |
| OrderType | OrderTypes |
| Exchange | Exchanges |
| Product | Products |

<!-- **Added**

* APIKey

**Removed**

* PasswordReset
* MemberId

**Changed**

* OrderType &rarr; OrderTypes
* Exchange &rarr; Exchanges
* Product &rarr; Products -->

### Changes in Position structure

| Verion 2 | Version 3 |
| :---: | :---: |
| _ | DayBuyQuantity |
| _ | DayBuyPrice |
| _ | DaySellQuantity |
| _ | DaySellPrice |
| NetBuyAmountM2M | _ |
| NetSellAmountM2M | _ |
| BuyM2M | BuyM2MValue |
| SellM2M | SellM2MValue |

<!-- **Added**

* DayBuyQuantity
* DayBuyValue
* DayBuyPrice
* DaySellQuantity
* DaySellValue
* DaySellPrice

**Removed**

* NetBuyAmountM2M
* NetSellAmountM2M

**Changed**

* BuyM2M &rarr; BuyM2MValue
* SellM2M &rarr; SellM2MValue -->

### Changes in Quote structure

| Verion 2 | Version 3 |
| :---: | :---: |
| _ | InstrumentToken |
| _ | Timestamp |
| _ | AveragePrice |
| _ | OIDayHigh |
| _ | OIDayLow |
| ChangePercent | _ |
| LastTime | LastTradeTime |

<!-- **Added**
* InstrumentToken
* Timestamp
* AveragePrice
* OIDayHigh
* OIDayLow
    
**Removed**
* ChangePercent

**Changes**
* LastTime &rarr; LastTradeTime -->

### Changes in Tick structure

| Verion 2 | Version 3 |
| :---: | :---: |
| _ | LastTradeTime |
| _ | OpenInterest |
| _ | OIDayHigh |
| _ | OIDayLow |
| _ | Timestamp |

<!-- **Added**

* LastTradeTime
* OpenInterest
* OIDayHigh
* OIDayLow
* Timestamp -->