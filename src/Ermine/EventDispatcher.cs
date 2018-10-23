namespace Ermine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Marten;
    using Marten.Events.Projections;
    using Marten.Events.Projections.Async;
    using Marten.Storage;

    public class EventDispatcher : IProjection
    {
        private readonly Func<Type, IEnumerable<object>> handlerFactory;

        public EventDispatcher(Func<Type,IEnumerable<object>> handlerFactory)
        {
            this.handlerFactory = handlerFactory;
        }

        public void Apply(IDocumentSession session, EventPage page) 
        {
            var events = page.Events.Select(e => e.Data).GroupBy(e => e.GetType());
            events.SelectMany(group =>
            {
                var handlerType = typeof(ISyncEventHandler<>).MakeGenericType(group.Key);
                var handlers = this.handlerFactory(handlerType) ?? Enumerable.Empty<object>();
                return handlers.SelectMany(h => group.Select(ev => handlerType.InvokeMember("Handle", BindingFlags.InvokeMethod, null, h, new[] {ev, session})));
            }).ToArray();
        }

        public Task ApplyAsync(IDocumentSession session, EventPage page, CancellationToken token)
        {
            var events = page.Events.Select(e => e.Data).GroupBy(e => e.GetType());
            return Task.WhenAll(
                events.SelectMany(group =>
                {
                    var handlerType = typeof(IAsyncEventHandler<>).MakeGenericType(group.Key);
                    var handlers = this.handlerFactory(handlerType) ?? Enumerable.Empty<object>();
                    return handlers.SelectMany(h => group.Select(ev => (Task) handlerType.InvokeMember("Handle", BindingFlags.InvokeMethod,
                        null, h, new[] {ev, session, token})));
                }));
        }

        public void EnsureStorageExists(ITenant tenant) { }

        public Type[] Consumes { get; } = new Type[0];
        public AsyncOptions AsyncOptions { get; } = new AsyncOptions();
    }
}