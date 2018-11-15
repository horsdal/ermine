[![Build Status](https://chr-horsdal.visualstudio.com/ermine/_apis/build/status/horsdal.ermine)](https://chr-horsdal.visualstudio.com/ermine/_build/latest?definitionId=2)
[![NuGet version](https://img.shields.io/nuget/v/Ermines.svg?style=flat)](https://www.nuget.org/packages/Ermines)

# Ermine
Easy to use generic event handler add-on for [Marten](https://github.com/JasperFx/marten).

## Getting started

### Install the NuGet

Install the [Ermines Nuget package](https://www.nuget.org/packages/Ermines):

```
> dotnet add package Ermines
```

### Create an event handler

Let's create an event handler for `MyEvent`:

```
using Ermine;

public class MyEventHandler : IAsyncEventHandler<MyEvent>
{
    public Task Handle(MyEvent @event, IDocumentSession session, CancellationtToken ct)
    {
        // store a read model, call another service, do the fandago
        return Task.CompletedTask;
    }
}
```

if you prefer a synchronous event handler use the interface `ISyncEventHandler<TEvent>`

### Add Ermines to your Marten document store

Configure your Marten document store to call Ermines event handlers when events are saved to the docuemnt store:

```
    var docStore = DocumentStore.For(x =>
    {
        x.Events.InlineProjections.Add(new EventDispatcher(t => 
        {
            if (t == typeof(MyEvent))
                return new [] { new MyEventHandler() };
            return null;
        }));
        // other document store config
    });
```

This registers the Ermines `EventDispatcher` with Marten and sets up a factory for creating event handlers based on event types. If you are using a dependency injection container you can use that to resolve event handlers:

```
    var container = ... // create DI container
    var docStore = DocumentStore.For(x =>
    {
        x.Events.InlineProjections.Add(new EventDispatcher(t => container.GetServices(t)));
    });
```

### Use the event store

Now whenever an event of type `MyEvent` is saved the event handler `MyEventHanlder` is called. That is when:

```
    using (var sesion = docStore.LightweightSession())
    {
        session.Events.Append(streamId, new MyEvent());
        await sessoin.SaveChangAsync();
    }
```

the `Handle` metthod on an instance of `MyEventHandler` is called with the new event and the `session` as arguments.

Beware that the event handlers are called during the `SaveChangesAsync` call.
