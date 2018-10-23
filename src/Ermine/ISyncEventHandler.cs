namespace Ermine
{
    using Marten;

    public interface ISyncEventHandler<in T>
    {
        void Handle(T @event, IDocumentSession session);
    }
}