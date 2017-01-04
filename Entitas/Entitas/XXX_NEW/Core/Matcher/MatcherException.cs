namespace Entitas {

    // TODO test
    public class MatcherException<TEntity> : EntitasException where TEntity : class, IEntity, new() {

        public MatcherException(string message, IMatcher<TEntity> matcher) : base(
            message + "\nmatcher.indices.Length must be 1 but was " + matcher.indices.Length + "!",
            "Did you use a complex matcher in AllOf, AnyOf or NoneOf? If so, that is not supported."
        ) { }
    }
}
