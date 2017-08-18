# The Kite Connect API .Net client
The official .Net client for communicating with [Kite Connect API](https://kite.trade).

Minimum required .Net Framework version: `4.5`

Kite Connect is a set of REST-like APIs that expose many capabilities required to build a complete investment and trading platform. Execute orders in real time, manage user portfolio, stream live market data (WebSockets), and more, with the simple HTTP API collection.

[Rainmatter](http://rainmatter.com) (c) 2017. Licensed under the MIT License.

## Documentation

- [Kite Connect HTTP API documentation](https://kite.trade/docs/connect/v1)
- [.Net library documentation](https://kite.trade/docs/kiteconnectdotnet/)

## Install Client Library

### Using NuGet

Execute in **Tools** &raquo; **NuGet Package Manager** &raquo; **Package Manager Console**

```
Install-Package Tech.Zerodha.KiteConnect
```
### Using .Net CLI

```
dotnet add package Tech.Zerodha.KiteConnect
```
### Manual Install

- Download [KiteConnect.dll](https://github.com/rainmattertech/dotnetkiteconnect/blob/master/dist/KiteConnect.dll?raw=true)
- Right click on your project &raquo; **Add** &raquo; **Reference** &raquo; Click **Browse** &raquo; Select **KiteConnect.dll**

## API usage
```csharp
// Import library
using KiteConnect;

// Initialize Kiteconnect using apiKey. Enabling Debug will give logs of requests and responses
Kite kite = new Kite(MyAPIKey, Debug: true);

// Collect login url to authenticate user. Load this URL in browser or WebView. 
// After successful authentication this will redirect to your redirect url with request token.
kite.GetLoginURL();

// Collect tokens and user details using the request token
User user = kite.RequestAccessToken(RequestToken, MySecret);

// Persist these tokens in database or settings
string MyAccessToken = user.AccessToken;
string MyPublicToken = user.PublicToken;

// Initialize Kite APIs with access token
kite.SetAccessToken(MyAccessToken);

// Set session expiry callback. Method can be separate function also.
kite.SetSessionHook(() => Console.WriteLine("Need to login again"));

// Example call for functions like "PlaceOrder" that returns Dictionary
Dictionary<string, dynamic> res;
res = kite.PlaceOrder("CDS", "USDINR17AUGFUT", "BUY", "1", Price: "64.0000", OrderType: "LIMIT", Product: "NRML");
Console.WriteLine(response["data"]["order_id"]);

// Example call for functions like "GetHoldings" that returns a data structure
List<Holding> holdings = kite.GetHoldings();
Console.WriteLine(holdings[0].AveragePrice);

```
For more details and examples, take a look at [Program.cs](https://github.com/rainmattertech/dotnetkiteconnect/blob/master/KiteConnect%20Sample/Program.cs) of `KiteConnect Sample` project in this repository.

## WebSocket live streaming data

This library uses Events to get ticks. These events are non blocking and can be used without additional threads. Create event handlers and attach it to Ticker instance as shown in the example below.

```csharp

/* 
To get live price use KiteTicker websocket connection. 
It is recommended to use only one websocket connection at any point of time and make sure you stop connection, 
once user goes out of app.
*/

// Create a new Ticker instance
Ticker ticker = new Ticker(MyAPIKey, MyUserId, MyPublicToken);

// Add handlers to events
ticker.OnTick += onTick;
ticker.OnReconnect += onReconnect;
ticker.OnNoReconnect += oNoReconnect;
ticker.OnError += onError;
ticker.OnClose += onClose;
ticker.OnConnect += onConnect;

// Engage reconnection mechanism and connect to ticker
ticker.EnableReconnect(Interval: 5,Retries: 50);
ticker.Connect();

// Subscribing to NIFTY50 and setting mode to LTP
ticker.Subscribe(Tokens: new string[] { "256265" });
ticker.SetMode(Tokens: new string[] { "256265" }, Mode: "ltp");

// Example onTick handler
private static void onTick(Tick TickData)
{
    Console.WriteLine("LTP: " + TickData.LastPrice);
}

// Disconnect ticker before closing the application
ticker.Close();
```

For more details about different mode of quotes and subscribing for them, take a look at `KiteConnect Sample` project in this repository and [Kite Connect HTTP API documentation](https://kite.trade/docs/connect/v1).
