using System.Collections.Generic;

namespace Entitas {

    public abstract class ReactiveSystem<TEntity> : IReactiveSystem where TEntity : class, IEntity, new() {

        readonly Collector<TEntity> _collector;
        readonly List<TEntity> _buffer;

        string _toStringCache;

        protected ReactiveSystem(XXXPools pools) {
            _collector = GetTrigger(pools);
            _buffer = new List<TEntity>();
        }

        protected abstract Collector<TEntity> GetTrigger(XXXPools pools);
        protected abstract bool Filter(TEntity entity);

        public abstract void Execute(IList<TEntity> entities);

        public void Activate() {
            _collector.Activate();
        }

        public void Deactivate() {
            _collector.Deactivate();
        }

        public void Clear() {
            _collector.ClearCollectedEntities();
        }

        public void Execute() {
            if(_collector.collectedEntities.Count != 0) {
                foreach(var e in _collector.collectedEntities) {
                    if(Filter(e)) {
                        e.Retain(this);
                        _buffer.Add(e);
                    }
                }

                _collector.ClearCollectedEntities();

                if(_buffer.Count != 0) {
                    Execute(_buffer);
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
