namespace Ermine
{
    using System.Threading;
    using System.Threading.Tasks;
    using Marten;

    public interface IAsyncEventHandler<in T>
    {
        Task Handle(T @event, IDocumentSession session, CancellationToken token);
    }
}