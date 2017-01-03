namespace Entitas {

    public delegate void PoolChangedHandler<TEntity>(IPool<TEntity> pool, TEntity entity)
        where TEntity : class, IEntity, new();

    public delegate void PoolGroupChangedHandler<TEntity>(IPool<TEntity> pool, IGroup<TEntity> group)
        where TEntity : class, IEntity, new();
}
