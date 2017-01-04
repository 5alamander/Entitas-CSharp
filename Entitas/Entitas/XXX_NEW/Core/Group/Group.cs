using System.Collections.Generic;

namespace Entitas {
    
    public class XXXGroup<TEntity> : IGroup<TEntity>
        where TEntity : class, IEntity, new() {

        public event GroupChangedHandler<TEntity> OnEntityAdded;
        public event GroupChangedHandler<TEntity> OnEntityRemoved;
        public event GroupUpdatedHandler<TEntity> OnEntityUpdated;

        public int count { get { return _entities.Count; } }
        public IMatcher<TEntity> matcher { get { return _matcher; } }

        readonly IMatcher<TEntity> _matcher;

        readonly HashSet<TEntity> _entities = new HashSet<TEntity>(
            EntityEqualityComparer<TEntity>.comparer
        );
        
        TEntity[] _entitiesCache;
        TEntity _singleEntityCache;
        string _toStringCache;

        public XXXGroup(IMatcher<TEntity> matcher) {
            _matcher = matcher;
        }

        public void HandleEntitySilently(TEntity entity) {
            if(_matcher.Matches(entity)) {
                addEntitySilently(entity);
            } else {
                removeEntitySilently(entity);
            }
        }

        public void HandleEntity(
            TEntity entity, int index, IComponent component) {
            if(_matcher.Matches(entity)) {
                addEntity(entity, index, component);
            } else {
                removeEntity(entity, index, component);
            }
        }

        public void UpdateEntity(
            TEntity entity,
            int index,
            IComponent previousComponent,
            IComponent newComponent) {
            if(_entities.Contains(entity)) {
                if(OnEntityRemoved != null) {
                    OnEntityRemoved(this, entity, index, previousComponent);
                }
                if(OnEntityAdded != null) {
                    OnEntityAdded(this, entity, index, newComponent);
                }
                if(OnEntityUpdated != null) {
                    OnEntityUpdated(
                        this, entity, index, previousComponent, newComponent
                    );
                }
            }
        }

        public void RemoveAllEventHandlers() {
            OnEntityAdded = null;
            OnEntityRemoved = null;
            OnEntityUpdated = null;
        }

        public GroupChangedHandler<TEntity> HandleEntity(TEntity entity) {
            return _matcher.Matches(entity)
                       ? (addEntitySilently(entity) ? OnEntityAdded : null)
                       : (removeEntitySilently(entity) ? OnEntityRemoved : null);
        }

        bool addEntitySilently(TEntity entity) {
            if(entity.isEnabled) {
                var added = _entities.Add(entity);
                if(added) {
                    _entitiesCache = null;
                    _singleEntityCache = null;
                    entity.Retain(this);
                }

                return added;
            }

            return false;
        }

        void addEntity(TEntity entity, int index, IComponent component) {
            if(addEntitySilently(entity) && OnEntityAdded != null) {
                OnEntityAdded(this, entity, index, component);
            }
        }

        bool removeEntitySilently(TEntity entity) {
            var removed = _entities.Remove(entity);
            if(removed) {
                _entitiesCache = null;
                _singleEntityCache = null;
                entity.Release(this);
            }

            return removed;
        }

        void removeEntity(TEntity entity, int index, IComponent component) {
            var removed = _entities.Remove(entity);
            if(removed) {
                _entitiesCache = null;
                _singleEntityCache = null;
                if(OnEntityRemoved != null) {
                    OnEntityRemoved(this, entity, index, component);
                }
                entity.Release(this);
            }
        }

        public bool ContainsEntity(TEntity entity) {
            return _entities.Contains(entity);
        }

        public TEntity[] GetEntities() {
            if(_entitiesCache == null) {
                _entitiesCache = new TEntity[_entities.Count];
                _entities.CopyTo(_entitiesCache);
            }

            return _entitiesCache;
        }

        public TEntity GetSingleEntity() {
            if(_singleEntityCache == null) {
                var c = _entities.Count;
                if(c == 1) {
                    using (var enumerator = _entities.GetEnumerator()) {
                        enumerator.MoveNext();
                        _singleEntityCache = enumerator.Current;
                    }
                } else if(c == 0) {
                    return null;
                } else {
                    throw new GroupSingleEntityException<TEntity>(this);
                }
            }

            return _singleEntityCache;
        }

        public override string ToString() {
            if(_toStringCache == null) {
                _toStringCache = "Group(" + _matcher + ")";
            }
            return _toStringCache;
        }
    }
}
