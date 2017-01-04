namespace Entitas {

    public delegate void ContextChangedHandler<TEntity>(IContext<TEntity> context, TEntity entity)
        where TEntity : class, IEntity, new();

    public delegate void ContextGroupChangedHandler<TEntity>(IContext<TEntity> context, IGroup<TEntity> group)
        where TEntity : class, IEntity, new();
}
