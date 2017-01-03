namespace Entitas {

    public delegate void GroupChangedHandler<TEntity>(
        IGroup<TEntity> group, TEntity entity, int index, IComponent component
    ) where TEntity : class, IEntity, new();

    public delegate void GroupUpdatedHandler<TEntity>(
        IGroup<TEntity> group, TEntity entity, int index,
        IComponent previousComponent, IComponent newComponent
    ) where TEntity : class, IEntity, new();
}
