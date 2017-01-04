namespace Entitas {

    public static class PoolExtension {

        public static TEntity[] GetEntities<TEntity>(this IPool<TEntity> pool, IMatcher<TEntity> matcher)
            where TEntity : class, IEntity, new() {
            return pool.GetGroup(matcher).GetEntities();
        }

        public static Collector<TEntity> CreateCollector<TEntity>(this IPool<TEntity> pool, IMatcher<TEntity> matcher, GroupEvent groupEvent = GroupEvent.OnEntityAdded)
            where TEntity : class, IEntity, new() {
            return new Collector<TEntity>(pool.GetGroup(matcher), groupEvent);
        }

        public static TEntity CloneEntity<TEntity>(this IPool<TEntity> pool,
                                         TEntity entity,
                                         bool replaceExisting = false,
                                         params int[] indices)
            where TEntity : class, IEntity, new() {
            var target = pool.CreateEntity();
            entity.CopyTo(target, replaceExisting, indices);
            return target;
        }
    }
}
