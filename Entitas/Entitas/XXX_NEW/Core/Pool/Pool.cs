using System.Collections.Generic;

namespace Entitas {

    public class XXXPool<TEntity> : IPool<TEntity>
        where TEntity : class, IEntity, new() {

        public event PoolChangedHandler<TEntity> OnEntityCreated;
        public event PoolChangedHandler<TEntity> OnEntityDestroyed;
        public event PoolGroupChangedHandler<TEntity> OnGroupCreated;
        public event PoolGroupChangedHandler<TEntity> OnGroupCleared;

        public int totalComponents { get { return _totalComponents; } }
        public EntityInfo entityInfo { get { return _entityInfo; } }

        public int count { get { return _entities.Count; } }
        public int reusableEntitiesCount {
            get { return _reusableEntities.Count; }
        }
        public int retainedEntitiesCount {
            get { return _retainedEntities.Count; }
        }

        readonly int _totalComponents;
        readonly EntityInfo _entityInfo;

        readonly HashSet<TEntity> _entities = new HashSet<TEntity>(
            EntityEqualityComparer<TEntity>.comparer
        );

        readonly Stack<TEntity> _reusableEntities = new Stack<TEntity>();
        readonly HashSet<TEntity> _retainedEntities = new HashSet<TEntity>(
            EntityEqualityComparer<TEntity>.comparer
        );

        readonly Dictionary<IMatcher<TEntity>, IGroup<TEntity>> _groups =
            new Dictionary<IMatcher<TEntity>, IGroup<TEntity>>();

        readonly List<IGroup<TEntity>>[] _groupsForIndex;

        readonly Stack<IComponent>[] _componentPools;
        readonly Dictionary<string, IEntityIndex> _entityIndices;

        int _creationIndex;
        TEntity[] _entitiesCache;

        // Cache delegates to avoid gc allocations
        EntityDestroyedHandler _cachedEntityDestroyed;
        EntityChangedHandler _cachedEntityChanged;
        ComponentReplacedHandler _cachedComponentReplaced;
        EntityReleasedHandler _cachedEntityReleased;

        public XXXPool(int totalComponents) : this(totalComponents, 0, null) {
        }

        public XXXPool(int totalComponents,
                    int startCreationIndex,
                    EntityInfo entityInfo) {
            _totalComponents = totalComponents;
            _creationIndex = startCreationIndex;
            _entityInfo = entityInfo ?? createDefaultEntityInfo();

            if(_entityInfo.componentNames.Length != totalComponents) {
                throw new EntityInfoException(this, entityInfo);
            }

            _groupsForIndex = new List<IGroup<TEntity>>[totalComponents];
            _componentPools = new Stack<IComponent>[totalComponents];
            _entityIndices = new Dictionary<string, IEntityIndex>();

            // Cache delegates to avoid gc allocations
            _cachedEntityDestroyed = onEntityDestroyed;
            _cachedEntityChanged = updateGroupsComponentAddedOrRemoved;
            _cachedComponentReplaced = updateGroupsComponentReplaced;
            _cachedEntityReleased = onEntityReleased;
        }

        EntityInfo createDefaultEntityInfo() {
            var componentNames = new string[totalComponents];
            const string prefix = "Index ";
            for (int i = 0; i < componentNames.Length; i++) {
                componentNames[i] = prefix + i;
            }

            return new EntityInfo("Unnamed Pool", componentNames, null);
        }

        public TEntity CreateEntity() {
            TEntity entity;
            if(_reusableEntities.Count > 0) {
                entity = _reusableEntities.Pop();
                entity.Reactivate(_creationIndex++);
            } else {
                entity = new TEntity();
                entity.Initialize(_totalComponents, _creationIndex++,
                                  _componentPools, _entityInfo);
            }
            entity.Retain(this);
            _entities.Add(entity);
            _entitiesCache = null;

            entity.OnEntityDestroyed +=_cachedEntityDestroyed;
            entity.OnComponentAdded +=_cachedEntityChanged;
            entity.OnComponentRemoved += _cachedEntityChanged;
            entity.OnComponentReplaced += _cachedComponentReplaced;
            entity.OnEntityReleased += _cachedEntityReleased;

            if(OnEntityCreated != null) {
                OnEntityCreated(this, entity);
            }

            return entity;
        }

        public void DestroyAllEntities() {
            var entities = GetEntities();
            for (int i = 0; i < entities.Length; i++) {
                entities[i].Destroy();
            }

            _entities.Clear();

            if(_retainedEntities.Count != 0) {
                throw new PoolStillHasRetainedEntitiesException(this);
            }
        }

        public bool HasEntity(TEntity entity) {
            return _entities.Contains(entity);
        }

        public TEntity[] GetEntities() {
            if(_entitiesCache == null) {
                _entitiesCache = new TEntity[_entities.Count];
                _entities.CopyTo(_entitiesCache);
            }

            return _entitiesCache;
        }

        public IGroup<TEntity> GetGroup(IMatcher<TEntity> matcher) {
            IGroup<TEntity> group;
            if(!_groups.TryGetValue(matcher, out group)) {
                group = new XXXGroup<TEntity>(matcher);
                var entities = GetEntities();
                for (int i = 0; i < entities.Length; i++) {
                    group.HandleEntitySilently(entities[i]);
                }
                _groups.Add(matcher, group);

                for (int i = 0; i < matcher.indices.Length; i++) {
                    var index = matcher.indices[i];
                    if(_groupsForIndex[index] == null) {
                        _groupsForIndex[index] = new List<IGroup<TEntity>>();
                    }
                    _groupsForIndex[index].Add(group);
                }

                if(OnGroupCreated != null) {
                    OnGroupCreated(this, group);
                }
            }

            return group;
        }

        public void ClearGroups() {
            foreach(var group in _groups.Values) {
                group.RemoveAllEventHandlers();
                var entities = group.GetEntities();
                // TODO Move logic to group, e.g. Deactivate
                for (int i = 0; i < entities.Length; i++) {
                    entities[i].Release(group);
                }

                if(OnGroupCleared != null) {
                    OnGroupCleared(this, group);
                }
            }
            _groups.Clear();

            for (int i = 0; i < _groupsForIndex.Length; i++) {
                _groupsForIndex[i] = null;
            }
        }

        public void AddEntityIndex(string name, IEntityIndex entityIndex) {
            if(_entityIndices.ContainsKey(name)) {
                throw new PoolEntityIndexDoesAlreadyExistException(this, name);
            }

            _entityIndices.Add(name, entityIndex);
        }

        public IEntityIndex GetEntityIndex(string name) {
            IEntityIndex entityIndex;
            if(!_entityIndices.TryGetValue(name, out entityIndex)) {
                throw new PoolEntityIndexDoesNotExistException(this, name);
            }

            return entityIndex;
        }

        public void DeactivateAndRemoveEntityIndices() {
            foreach(var entityIndex in _entityIndices.Values) {
                entityIndex.Deactivate();
            }

            _entityIndices.Clear();
        }

        public void ResetCreationIndex() {
            _creationIndex = 0;
        }

        public void ClearComponentPool(int index) {
            var componentPool = _componentPools[index];
            if(componentPool != null) {
                componentPool.Clear();
            }
        }

        public void ClearComponentPools() {
            for (int i = 0; i < _componentPools.Length; i++) {
                ClearComponentPool(i);
            }
        }

        public void Reset() {
            ClearGroups();
            DestroyAllEntities();
            ResetCreationIndex();

            OnEntityCreated = null;
            OnEntityDestroyed = null;
            OnGroupCreated = null;
            OnGroupCleared = null;
        }

        public override string ToString() {
            return _entityInfo.poolName;
        }

        void onEntityDestroyed(IEntity entity) {
            var tEntity = (TEntity)entity;
            var removed = _entities.Remove(tEntity);
            if(!removed) {
                throw new PoolDoesNotContainEntityException(
                    "'" + this + "' cannot destroy " + entity + "!",
                    "Did you call pool.DestroyEntity() on a wrong pool?"
                );
            }
            _entitiesCache = null;

            if(OnEntityDestroyed != null) {
                OnEntityDestroyed(this, tEntity);
            }

            if(entity.retainCount == 1) {
                // Can be released immediately without
                // adding to _retainedEntities
                entity.OnEntityReleased -= _cachedEntityReleased;
                _reusableEntities.Push(tEntity);
                entity.Release(this);
                entity.RemoveAllOnEntityReleasedHandlers();
            } else {
                _retainedEntities.Add(tEntity);
                entity.Release(this);
            }
        }

        void updateGroupsComponentAddedOrRemoved(
            IEntity entity, int index, IComponent component) {
            var groups = _groupsForIndex[index];
            if(groups != null) {
                var tEntity = (TEntity)entity;
                var events = EntitasCache.GetGroupChangedList<TEntity>();

                    for(int i = 0; i < groups.Count; i++) {
                        events.Add(groups[i].HandleEntity(tEntity));
                    }

                    for(int i = 0; i < events.Count; i++) {
                        var groupChangedEvent = events[i];
                        if(groupChangedEvent != null) {
                            groupChangedEvent(
                                groups[i], tEntity, index, component
                            );
                        }
                    }

                EntitasCache.PushGroupChangedList(events);
            }
        }

        void updateGroupsComponentReplaced(IEntity entity, int index,
            IComponent previousComponent, IComponent newComponent) {
            var groups = _groupsForIndex[index];
            if(groups != null) {
                var tEntity = (TEntity)entity;
                for (int i = 0; i < groups.Count; i++) {
                    groups[i].UpdateEntity(
                        tEntity, index, previousComponent, newComponent
                    );
                }
            }
        }

        void onEntityReleased(IEntity entity) {
            if(entity.isEnabled) {
                throw new EntityIsNotDestroyedException(
                    "Cannot release " + entity + "!"
                );
            }
            var tEntity = (TEntity)entity;
            entity.RemoveAllOnEntityReleasedHandlers();
            _retainedEntities.Remove(tEntity);
            _reusableEntities.Push(tEntity);
        }
    }
}
