using System.Collections.Generic;
using System.Text;

namespace Entitas {

    /// An EntityCollector can observe one or more groups and collects
    /// changed entities based on the specified eventType.
    public class EntityCollector<TEntity>
        where TEntity : class, IEntity, new() {

        /// Returns all collected entities.
        /// Call collector.ClearCollectedEntities()
        /// once you processed all entities.
        public HashSet<TEntity> collectedEntities {
            get { return _collectedEntities; }
        }

        readonly HashSet<TEntity> _collectedEntities;
        readonly Group<TEntity>[] _groups;
        readonly GroupEventType[] _eventTypes;
        Group<TEntity>.GroupChanged _addEntityCache;
        string _toStringCache;
        StringBuilder _toStringBuilder;

        /// Creates an EntityCollector and will collect changed entities
        /// based on the specified eventType.
        public EntityCollector(Group<TEntity> group, GroupEventType eventType)
            : this(new [] { group }, new [] { eventType }) {
        }

        /// Creates an EntityCollector and will collect changed entities
        /// based on the specified eventTypes.
        public EntityCollector(Group<TEntity>[] groups, GroupEventType[] eventTypes) {
            _groups = groups;
            _collectedEntities = new HashSet<TEntity>(
                EntityEqualityComparer.comparer
            );
            _eventTypes = eventTypes;

            if(groups.Length != eventTypes.Length) {
                throw new EntityCollectorException(
                    "Unbalanced count with groups (" + groups.Length +
                    ") and event types (" + eventTypes.Length + ").",
                    "Group and event type count must be equal."
                );
            }

            _addEntityCache = addEntity;
            Activate();
        }

        /// Activates the EntityCollector and will start collecting
        /// changed entities. EntityCollectors are activated by default.
        public void Activate() {
            for (int i = 0; i < _groups.Length; i++) {
                var group = _groups[i];
                var eventType = _eventTypes[i];
                if(eventType == GroupEventType.OnEntityAdded) {
                    group.OnEntityAdded -= _addEntityCache;
                    group.OnEntityAdded += _addEntityCache;
                } else if(eventType == GroupEventType.OnEntityRemoved) {
                    group.OnEntityRemoved -= _addEntityCache;
                    group.OnEntityRemoved += _addEntityCache;
                } else if(eventType == GroupEventType.OnEntityAddedOrRemoved) {
                    group.OnEntityAdded -= _addEntityCache;
                    group.OnEntityAdded += _addEntityCache;
                    group.OnEntityRemoved -= _addEntityCache;
                    group.OnEntityRemoved += _addEntityCache;
                }
            }
        }

        /// Deactivates the EntityCollector.
        /// This will also clear all collected entities.
        /// EntityCollectors are activated by default.
        public void Deactivate() {
            for (int i = 0; i < _groups.Length; i++) {
                var group = _groups[i];
                group.OnEntityAdded -= _addEntityCache;
                group.OnEntityRemoved -= _addEntityCache;
            }
            ClearCollectedEntities();
        }

        /// Clears all collected entities.
        public void ClearCollectedEntities() {
            foreach(var entity in _collectedEntities) {
                entity.Release(this);
            }
            _collectedEntities.Clear();
        }

        void addEntity(Group<TEntity> group,
                       TEntity entity,
                       int index,
                       IComponent component) {
            var added = _collectedEntities.Add(entity);
            if(added) {
                entity.Retain(this);
            }
        }

        public override string ToString() {
            if(_toStringCache == null) {
                if(_toStringBuilder == null) {
                    _toStringBuilder = new StringBuilder();
                }
                _toStringBuilder.Length = 0;
                _toStringBuilder.Append("Collector(");

                const string separator = ", ";
                var lastSeparator = _groups.Length - 1;
                for (int i = 0; i < _groups.Length; i++) {
                    _toStringBuilder.Append(_groups[i]);
                    if(i < lastSeparator) {
                        _toStringBuilder.Append(separator);
                    }
                }

                _toStringBuilder.Append(")");
                _toStringCache = _toStringBuilder.ToString();
            }

            return _toStringCache;
        }

        ~EntityCollector () {
            Deactivate();
        }
    }

    public class EntityCollectorException : EntitasException {
        public EntityCollectorException(string message, string hint) :
            base(message, hint) {
        }
    }
}
