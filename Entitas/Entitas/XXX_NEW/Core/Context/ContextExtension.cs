namespace Entitas {

    public static class ContextExtension {

        public static TEntity[] GetEntities<TEntity>(this IContext<TEntity> context, IMatcher<TEntity> matcher)
            where TEntity : class, IEntity, new() {
            return context.GetGroup(matcher).GetEntities();
        }

        public static Collector<TEntity> CreateCollector<TEntity>(this IContext<TEntity> context, IMatcher<TEntity> matcher, GroupEvent groupEvent = GroupEvent.OnEntityAdded)
            where TEntity : class, IEntity, new() {
            return new Collector<TEntity>(context.GetGroup(matcher), groupEvent);
        }

        public static TEntity CloneEntity<TEntity>(this IContext<TEntity> context,
                                         TEntity entity,
                                         bool replaceExisting = false,
                                         params int[] indices)
            where TEntity : class, IEntity, new() {
            var target = context.CreateEntity();
            entity.CopyTo(target, replaceExisting, indices);
            return target;
        }
    }
}
