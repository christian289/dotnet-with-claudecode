# Pub-Sub Quick Reference

## 1. Channel Basics

```csharp
using System.Threading.Channels;

var channel = Channel.CreateUnbounded<Message>();

// Producer
await channel.Writer.WriteAsync(message);

// Consumer
await foreach (var msg in channel.Reader.ReadAllAsync(ct))
{
    await HandleMessage(msg);
}

// Completion signal
channel.Writer.Complete();
```

## 2. Bounded Channel

```csharp
// Backpressure control
var channel = Channel.CreateBounded<Message>(new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait
});
```

## 3. DI Registration

```csharp
services.AddSingleton(Channel.CreateUnbounded<Message>());
services.AddSingleton(sp => sp.GetRequiredService<Channel<Message>>().Reader);
services.AddSingleton(sp => sp.GetRequiredService<Channel<Message>>().Writer);
```

## 4. Channels vs Rx.NET

| Feature | Channels | Rx.NET |
|---------|----------|--------|
| Purpose | Producer-Consumer | Event streams |
| Backpressure | Built-in | Separate implementation |
| Dependency | BCL | NuGet |
