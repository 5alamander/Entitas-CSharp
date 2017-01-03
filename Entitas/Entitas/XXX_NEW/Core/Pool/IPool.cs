namespace Entitas {
    
    public interface IPool {

        int totalComponents { get; }
        EntityInfo entityInfo { get; }

        int count { get; }
        int reusableEntitiesCount { get; }
        int retainedEntitiesCount { get; }

        void DestroyAllEntities();
        void ClearGroups();

        void AddEntityIndex(string name, IEntityIndex entityIndex);
        IEntityIndex GetEntityIndex(string name);
        void DeactivateAndRemoveEntityIndices();

        void ResetCreationIndex();
        void ClearComponentPool(int index);
        void ClearComponentPools();
        void Reset();
    }

    public interface IPool<TEntity> : IPool
        where TEntity : class, IEntity, new() {

        event PoolChangedHandler<TEntity> OnEntityCreated;
        event PoolChangedHandler<TEntity> OnEntityDestroyed;

        event PoolGroupChangedHandler<TEntity> OnGroupCreated;
        event PoolGroupChangedHandler<TEntity> OnGroupCleared;

        TEntity CreateEntity();
        bool HasEntity(TEntity entity);
        TEntity[] GetEntities();
        IGroup<TEntity> GetGroup(IMatcher matcher);
    }
}
