# Steamworks Toolkit

Steamworks Toolkit is a set of utilities built on top of **Steamworks.NET** that extends the standard callback-based API with **UniTask** support.  
It allows awaiting and observing **CallResult** and **Callback** events in an async-friendly manner, and provides convenient facade types such as **UserData**, **LobbyData**, **InventoryItem**, **GameData**, and more.

---

## Requirements

### Required
- Steamworks.NET
- UniTask
- Newtonsoft.Json

### Optional
- HyTeKGames.AnimatedImages  
  https://assetstore.unity.com/packages/tools/utilities/animatedimages-easily-play-gifs-apng-webp-more-282390  
  *(used for animated avatar / frame if present)*

---

## Features

### Async-friendly Steam callbacks
- Await the next `Callback<T>` event with `UniTask`
- Observe `Callback<T>` events through a global stream

### Async-friendly Steam API calls (CallResult)
- Convert `SteamAPICall_t` to `UniTask<(T result, bool ioError)>`
- Observe any `CallResult<T>` completion through a global stream

### Handle-based awaiting
A generic `HandleAwaiter<TSelf, THandle, TResponse, TResult>` abstraction for cases where a single callback stream contains multiple handles and you want to await a specific one.

### Facade types
Convenient wrappers that make Steamworks data easier to consume:
- UserData
- LobbyData
- InventoryItem
- GameData
- and more

---

## Usage

### SteamCallbackObserver<T>

Observe or await Steam `Callback<T>` events.

#### Subscribe
```csharp
SteamCallbackObserver<LobbyInvite_t>.OnTrigger += OnLobbyInvite;

static void OnLobbyInvite( LobbyInvite_t data )
{
    // handle invite
}
```

#### Unsubscribe
```csharp
SteamCallbackObserver<LobbyInvite_t>.OnTrigger -= OnLobbyInvite;
```

#### Await the next callback
```csharp
using var cts = new CancellationTokenSource( 10_000 );

var invite = await SteamCallbackObserver<LobbyInvite_t>.WaitAny( cts.Token );
```

---

### SteamCallResultObserver<T>

Convert Steam API calls based on `CallResult<T>` into async-friendly workflows.

#### Await a Steam API call
```csharp
var apiCall = SteamInventory.RequestPrices( );

var (result, ioError) = await apiCall.ToUniTask<SteamInventoryRequestPricesResult_t>( );

if( ioError )
{
    // handle error
}
```

#### Observe CallResult completions globally
```csharp
SteamCallResultObserver<SteamInventoryRequestPricesResult_t>.OnTrigger += OnPricesReady;

static void OnPricesReady( SteamInventoryRequestPricesResult_t result, bool ioError )
{
    // react to price update
}
```

Trigger observation without awaiting:
```csharp
SteamInventory.RequestPrices( ).Observe<SteamInventoryRequestPricesResult_t>( );
```

---

### HandleAwaiter<TSelf, THandle, TResponse, TResult>

Used when a single callback stream represents multiple entities and you want to await results for a specific handle.

#### Await a handle-specific callback
```csharp
var user = UserData.Get( steamId );

var stateChange = await PersonaStateChangeAwaiter.Instance.WaitAsync( user );
```

#### Built-in HandleAwaiters
Steamworks Toolkit already provides the following handle-based awaiters:

- AuthTicketForWebAwaiter
- AvatarImageLoadedAwaiter
- InventoryResultAwaiter
- LobbyDataUpdateAwaiter
- MicroTxnAuthorizationAwaiter
- PersonaStateChangeAwaiter

To create a new handle-based awaiter, derive from `HandleAwaiter<TSelf, THandle, TResponse, TResult>` and implement the required methods.

---

## License
MIT License
