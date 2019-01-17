namespace UnitTest
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Ermine;
    using Marten.Events;
    using Marten.Events.Projections.Async;
    using Xunit;

    public class EventDispatcher_should
    {
        private readonly MyEventHandler eventHandler = new MyEventHandler();
        private readonly UnusedEventHandler unusedEventHandler = new UnusedEventHandler();
        private readonly MySyncEventHandler syncEventHandler = new MySyncEventHandler();
        private readonly UnusedSyncEventHandler unusedSyncEventHandler = new UnusedSyncEventHandler();
        private readonly TwiceReturnedEventHandler twiceReturnedEventHandler = new TwiceReturnedEventHandler();
        private readonly TwiceReturnedSyncEventHandler twiceReturnedSyncEventHandler = new TwiceReturnedSyncEventHandler();

        private readonly EventDispatcher sut;

        public EventDispatcher_should()
        {
            this.sut = new EventDispatcher(t =>
            {
                switch (t)
                {
                    case Type x when x ==typeof(IAsyncEventHandler<MyEvent>):
                        return new IAsyncEventHandler<MyEvent>[] { this.eventHandler, this.twiceReturnedEventHandler, this.twiceReturnedEventHandler };
                    case Type x when x ==typeof(ISyncEventHandler<MyEvent>):
                        return new ISyncEventHandler<MyEvent>[] { this.syncEventHandler, this.twiceReturnedSyncEventHandler, this.twiceReturnedSyncEventHandler };
                    default:
                        return null;
                }
            });            
        }
        
        private static EventPage CreateEventPage(int eventCount)
        {
            var events = Enumerable
                .Repeat((IEvent) new Event<MyEvent>(new MyEvent()), eventCount)
                .Append(new Event<IgnoredEvent>(new IgnoredEvent()))
                .ToList().AsReadOnly();
            return new EventPage(0, eventCount, events);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        public async Task Call_async_event_handler(int eventCount)
        {
            var eventPage = CreateEventPage(eventCount);
            await this.sut.ApplyAsync(null, eventPage, CancellationToken.None);
            Assert.Equal(eventCount*2, this.eventHandler.NoOfCalls);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        public void Call_sync_event_handler(int eventCount)
        {
            var eventPage = CreateEventPage(eventCount);
            this.sut.Apply(null, eventPage);
            Assert.Equal(eventCount, this.syncEventHandler.NoOfCalls);
        }
        
        [Fact]
        public async Task Not_call_handler_when_no_event()
        {
            var eventPage = CreateEventPage(1);
            await this.sut.ApplyAsync(null, eventPage, CancellationToken.None);
            Assert.Equal(0, this.unusedEventHandler.NoOfCalls);            
        }

        [Fact]
        public void Not_call_sync_handler_when_no_event()
        {
            var eventPage = CreateEventPage(1);
            this.sut.Apply(null, eventPage);
            Assert.Equal(0, this.unusedSyncEventHandler.NoOfCalls);            
        }
        
        [Fact]
        public async Task call_twice_returned_handler_twice()
        {
            var eventPage = CreateEventPage(1);
            await this.sut.ApplyAsync(null, eventPage, CancellationToken.None);
            Assert.Equal(2, this.twiceReturnedEventHandler.NoOfCalls);            
        }

        [Fact]
        public void call_twice_returned_sync_handler_twice()
        {
            var eventPage = CreateEventPage(1);
            this.sut.Apply(null, eventPage);
            Assert.Equal(2, this.twiceReturnedSyncEventHandler.NoOfCalls);            
        }
    }
}