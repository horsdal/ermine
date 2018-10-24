namespace UnitTest
{
    using System.Threading;
    using System.Threading.Tasks;
    using Ermine;
    using Marten;

    public class MyEvent {}
    public class UnusedEvent {}
    public class IgnoredEvent {}
    
    public class MyEventHandler : IAsyncEventHandler<MyEvent>
    {
        public int NoOfCalls = 0;
        
        public Task Handle(MyEvent @event, IDocumentSession session, CancellationToken token)
        {
            this.NoOfCalls++;
            return Task.CompletedTask;
        }
    }
    
    public class MySyncEventHandler : ISyncEventHandler<MyEvent>
    {
        public int NoOfCalls;
        public void Handle(MyEvent @event, IDocumentSession session) => this.NoOfCalls++;
    }

    public class UnusedEventHandler : IAsyncEventHandler<UnusedEvent>
    {
        public int NoOfCalls;
        public Task Handle(UnusedEvent @event, IDocumentSession session, CancellationToken token)
        {
            this.NoOfCalls++;
            return Task.CompletedTask;
        }
    }
    
    public class UnusedSyncEventHandler : ISyncEventHandler<UnusedEvent>
    {
        public int NoOfCalls;
        public void Handle(UnusedEvent @event, IDocumentSession session) => this.NoOfCalls++;
    }

    public class TwiceReturnedEventHandler : IAsyncEventHandler<MyEvent>
    {
        public int NoOfCalls;
        public Task Handle(MyEvent @event, IDocumentSession session, CancellationToken token)
        {
            this.NoOfCalls++;
            return Task.CompletedTask;
        }
    }
    
    public class TwiceReturnedSyncEventHandler : ISyncEventHandler<MyEvent>
    {
        public int NoOfCalls;
        public void Handle(MyEvent @event, IDocumentSession session) => this.NoOfCalls++;
    }
}
