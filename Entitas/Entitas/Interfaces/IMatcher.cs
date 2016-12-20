namespace Entitas {

    public interface IMatcher {

        int[] indices { get; }
    }

    public interface IMatcher<TEntity> : IMatcher
        where TEntity : class, IEntity, new() {

        bool Matches(TEntity entity);
    }
}
