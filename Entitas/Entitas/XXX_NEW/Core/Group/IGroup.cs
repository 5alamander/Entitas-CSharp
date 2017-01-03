namespace Entitas {

    public interface IGroup {

        int count { get; }
        IMatcher matcher { get; }

        void RemoveAllEventHandlers();
    }

    public interface IGroup<TEntity> : IGroup
        where TEntity : class, IEntity, new() {

        event GroupChangedHandler<TEntity> OnEntityAdded;
        event GroupChangedHandler<TEntity> OnEntityRemoved;
        event GroupUpdatedHandler<TEntity> OnEntityUpdated;

        void HandleEntitySilently(TEntity entity);
        void HandleEntity(TEntity entity, int index, IComponent component);
        void UpdateEntity(TEntity entity, int index, IComponent previousComponent, IComponent newComponent);

        GroupChangedHandler<TEntity> HandleEntity(TEntity entity);
        bool ContainsEntity(TEntity entity);
        TEntity[] GetEntities();
        TEntity GetSingleEntity();
    }
}
