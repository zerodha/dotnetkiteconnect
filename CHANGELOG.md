## Migrating from Kite Connect v2 to v3

### Changes in Kite

* All timestamps in string are now DateTime objects

New APIs added:

<!-- * GetInstrumentsMargins -->
* GetQuote
* GetOHLC
* GetLTP
* GetTriggerRange
* GetHistoricalData with timestamps
* GetProfile
* InvalidateRefreshToken
* RenewAccessToken

### Changes in Ticker

* Use Access Token to authenticate instead of Public Token. User id is not required.

```csharp
Ticker ticker = new Ticker(MyAPIKey, MyUserId, MyPublicToken);
```

becomes,

```csharp
Ticker ticker = new Ticker(MyAPIKey, MyAccessToken);
```

* Ticker now streams order updates
* New fields in Ticks
* Changed type of instrument token to UInt32

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
| _ | AvatarURL |
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
| OpenInterest | OI |

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

### Changes in TriggerRange structure

| Verion 3.0.0 Beta 1 | Version 3.0.0 Beta 2 |
| :---: | :---: |
| _ | InstrumentToken |
| Start | Lower |
| End | Upper |
| Percent | Percentage |

### Changes in Tick structure

| Verion 2 | Version 3 |
| :---: | :---: |
| _ | LastTradeTime |
| _ | OI |
| _ | OIDayHigh |
| _ | OIDayLow |
| _ | Timestamp |

### Changes in Trade structure

| Verion 2 | Version 3 |
| :---: | :---: |
| OrderTimestamp | - |
| - | FillTimestamp |

<!-- **Added**

* LastTradeTime
* OpenInterest
* OIDayHigh
* OIDayLow
* Timestamp -->