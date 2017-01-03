namespace Entitas {

    public static class GroupExtension {

        public static Collector<TEntity> CreateCollector<TEntity>(
            this IGroup<TEntity> group, GroupEvent groupEvent = GroupEvent.OnEntityAdded)
            where TEntity : class, IEntity, new() {
            return new Collector<TEntity>(group, groupEvent);
        }
    }
}
