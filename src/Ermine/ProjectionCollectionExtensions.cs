namespace Ermine
{
    using System;
    using System.Collections.Generic;
    using Marten.Events.Projections;

    public static class ProjectionCollectionExtensions
    {
        public static void AddAsyncEventHandlers(this ProjectionCollection projectionCollection, Func<Type, IEnumerable<object>> handlerFactory)
        {
            projectionCollection.Add(new EventDispatcher(handlerFactory));
        }
    }
}
