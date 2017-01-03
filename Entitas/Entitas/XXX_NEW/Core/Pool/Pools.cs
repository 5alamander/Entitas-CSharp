using System;
using System.Collections.Generic;
using System.Linq;

namespace Entitas {

    public class XXXPools {

        public static XXXPools sharedInstance { get; set; }

        public static IPool<TEntity> CreatePool<TEntity>(
            string poolName, int totalComponents,
            string[] componentNames, Type[] componentTypes)
            where TEntity : class, IEntity, new() {
            var pool = new XXXPool<TEntity>(totalComponents, 0, new EntityInfo(
                poolName, componentNames, componentTypes)
            );

#if(!ENTITAS_DISABLE_VISUAL_DEBUGGING && UNITY_EDITOR)
            if(UnityEngine.Application.isPlaying) {
                var poolObserver =
                    new Entitas.Unity.VisualDebugging.PoolObserver(pool);
                UnityEngine.Object.DontDestroyOnLoad(poolObserver.gameObject);
            }
#endif

            return pool;
        }

        Dictionary<Type, IPool> _pools = new Dictionary<Type, IPool>();

        public void AddPool<TEntity>(IPool<TEntity> pool) where TEntity : class, IEntity, new() {
            _pools.Add(typeof(TEntity), pool);
        }

        public IPool<TEntity> Get<TEntity>() where TEntity : class, IEntity, new() {
            return (IPool<TEntity>)_pools[typeof(TEntity)];
        }

        public IPool[] allPools {
            get { return _pools.Values.OrderBy(p => p.entityInfo.poolName).ToArray(); }
        }
    }
}
