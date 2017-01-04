using System;

namespace Entitas {

    public static class IPoolsExtension {

        public static IPool<TEntity> CreatePool<TEntity>(this IPools pools, string poolName, int totalComponents,
            string[] componentNames, Type[] componentTypes) where TEntity : class, IEntity, new() {

            var pool = new XXXPool<TEntity>(totalComponents, 0, new EntityInfo(poolName, componentNames, componentTypes));

#if(!ENTITAS_DISABLE_VISUAL_DEBUGGING && UNITY_EDITOR)
            if(UnityEngine.Application.isPlaying) {
                var poolObserver = new Entitas.Unity.VisualDebugging.PoolObserver(pool);
                UnityEngine.Object.DontDestroyOnLoad(poolObserver.gameObject);
            }
#endif

            return pool;
        }
    }
}
