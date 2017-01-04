namespace Entitas {

    public delegate void EntityChangedHandler(
        IEntity entity, int index, IComponent component
    );

    public delegate void ComponentReplacedHandler(
        IEntity entity, int index,
        IComponent previousComponent, IComponent newComponent
    );

    public delegate void EntityDestroyedHandler(IEntity entity);
    public delegate void EntityReleasedHandler(IEntity entity);
}
