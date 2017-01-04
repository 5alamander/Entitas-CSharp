using System;

namespace Entitas {

    public static class IContextsExtension {

        public static IContext<TEntity> CreateContext<TEntity>(this IContexts contexts, string contextName, int totalComponents,
            string[] componentNames, Type[] componentTypes) where TEntity : class, IEntity, new() {

            var context = new Context<TEntity>(totalComponents, 0, new ContextInfo(contextName, componentNames, componentTypes));

#if(!ENTITAS_DISABLE_VISUAL_DEBUGGING && UNITY_EDITOR)
            if(UnityEngine.Application.isPlaying) {
                var contextObserver = new Entitas.Unity.VisualDebugging.ContextObserver(context);
                UnityEngine.Object.DontDestroyOnLoad(contextObserver.gameObject);
            }
#endif

            return context;
        }
    }
}
