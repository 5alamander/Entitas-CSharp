using System.Collections.Generic;
using System.Text;

namespace Entitas {

    public class Collector<TEntity> where TEntity : class, IEntity, new() {

        public HashSet<TEntity> collectedEntities { get { return _collectedEntities; } }

        readonly HashSet<TEntity> _collectedEntities;
        readonly IGroup<TEntity>[] _groups;
        readonly GroupEvent[] _groupEvents;

        GroupChangedHandler<TEntity> _addEntityCache;
        string _toStringCache;
        StringBuilder _toStringBuilder;

        public Collector(IGroup<TEntity> group, GroupEvent groupEvent)
            : this(new [] { group }, new [] { groupEvent }) {
        }

        public Collector(IGroup<TEntity>[] groups, GroupEvent[] groupEvents) {
            _groups = groups;
            _groupEvents = groupEvents;
            _collectedEntities = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.comparer);

            if(groups.Length != groupEvents.Length) {
                throw new CollectorException(
                    "Unbalanced count with groups (" + groups.Length +
                    ") and groupEvents (" + groupEvents.Length + ").",
                    "Group and groupEvent count must be equal."
                );
            }

            _addEntityCache = addEntity;
            Activate();
        }

        public void Activate() {
            for (int i = 0; i < _groups.Length; i++) {
                var group = _groups[i];
                var groupEvent = _groupEvents[i];
                switch(groupEvent) {
                    case GroupEvent.OnEntityAdded:
                        group.OnEntityAdded -= _addEntityCache;
                        group.OnEntityAdded += _addEntityCache;
                        break;
                    case GroupEvent.OnEntityRemoved:
                        group.OnEntityRemoved -= _addEntityCache;
                        group.OnEntityRemoved += _addEntityCache;
                        break;
                    case GroupEvent.OnEntityAddedOrRemoved:
                        group.OnEntityAdded -= _addEntityCache;
                        group.OnEntityAdded += _addEntityCache;
                        group.OnEntityRemoved -= _addEntityCache;
                        group.OnEntityRemoved += _addEntityCache;
                        break;
                }
            }
        }

        public void Deactivate() {
            for (int i = 0; i < _groups.Length; i++) {
                var group = _groups[i];
                group.OnEntityAdded -= _addEntityCache;
                group.OnEntityRemoved -= _addEntityCache;
            }
            ClearCollectedEntities();
        }

        public void ClearCollectedEntities() {
            foreach(var entity in _collectedEntities) {
                entity.Release(this);
            }
            _collectedEntities.Clear();
        }

        void addEntity(IGroup<TEntity> group, TEntity entity, int index, IComponent component) {
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

        ~Collector () {
            Deactivate();
        }
    }
}
