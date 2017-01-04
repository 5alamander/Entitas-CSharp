namespace Entitas {
    
    public interface IContext {

        int totalComponents { get; }
        ContextInfo contextInfo { get; }

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

    public interface IContext<TEntity> : IContext
        where TEntity : class, IEntity, new() {

        event ContextChangedHandler<TEntity> OnEntityCreated;
        event ContextChangedHandler<TEntity> OnEntityDestroyed;

        event ContextGroupChangedHandler<TEntity> OnGroupCreated;
        event ContextGroupChangedHandler<TEntity> OnGroupCleared;

        TEntity CreateEntity();
        bool HasEntity(TEntity entity);
        TEntity[] GetEntities();
        IGroup<TEntity> GetGroup(IMatcher<TEntity> matcher);
    }
}
