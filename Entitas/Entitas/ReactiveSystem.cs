using System.Collections.Generic;

namespace Entitas {

    public interface IReactiveSystem : IExecuteSystem {
        void Activate();
        void Deactivate();
        void Clear();
    }

    /// A ReactiveSystem calls Execute(entities) if there were changes based on
    /// the specified EntityCollector and will only pass in changed entities.
    /// A common use-case is to react to changes, e.g. a change of the position
    /// of an entity to update the gameObject.transform.position
    /// of the related gameObject.
    public abstract class ReactiveSystem<TEntity> : IReactiveSystem
        where TEntity : class, IEntity, new() {

        readonly EntityCollector<TEntity> _collector;
        readonly List<IEntity> _buffer;
        string _toStringCache;

        protected ReactiveSystem(EntityCollector<TEntity> collector) {
            _collector = collector;
            _buffer = new List<IEntity>();
        }

        /// This will exclude all entities which don't pass the filter.
        protected abstract bool Filter(TEntity entity);

        public abstract void Execute(IReadOnlyList<TEntity> entities);

        /// Activates the ReactiveSystem and starts observing changes
        /// based on the specified EntityCollector.
        /// ReactiveSystem are activated by default.
        public void Activate() {
            _collector.Activate();
        }

        /// Deactivates the ReactiveSystem.
        /// No changes will be tracked while deactivated.
        /// This will also clear the ReactiveSystem.
        /// ReactiveSystem are activated by default.
        public void Deactivate() {
            _collector.Deactivate();
        }

        /// Clears all accumulated changes.
        public void Clear() {
            _collector.ClearCollectedEntities();
        }

        /// Will call Execute(entities) with changed entities
        /// if there are any. Otherwise it will not call Execute(entities).
        public void Execute() {
            if(_collector.collectedEntities.Count != 0) {
                foreach(var e in _collector.collectedEntities) {
                    if(Filter(e)) {
                        _buffer.Add(e.Retain(this));
                    }
                }

                _collector.ClearCollectedEntities();

                if(_buffer.Count != 0) {
                    Execute((IReadOnlyList<TEntity>)_buffer);
                    for (int i = 0; i < _buffer.Count; i++) {
                        _buffer[i].Release(this);
                    }
                    _buffer.Clear();
                }
            }
        }

        public override string ToString() {
            if(_toStringCache == null) {
                _toStringCache = "ReactiveSystem(" + GetType().Name + ")";
            }

            return _toStringCache;
        }

        ~ReactiveSystem () {
            Deactivate();
        }
    }
}
