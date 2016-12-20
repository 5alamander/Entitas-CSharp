using System;
using System.Collections.Generic;

namespace Entitas {

    public delegate void EntityDestroyed(IEntity entity);

    public delegate void EntityChanged(
        IEntity entity, int index, IComponent component
    );

    public delegate void ComponentReplaced(
        IEntity entity, int index,
        IComponent previousComponent, IComponent newComponent
    );

    public delegate void EntityReleased(IEntity entity);

    /// Use pool.CreateEntity() to create a new entity and
    /// pool.DestroyEntity() to destroy it.
    /// You can add, replace and remove IComponent to an entity.
    public interface IEntity {

        /// Occurs when the entity gets destroyed.
        /// All event handlers will be removed when
        /// the entity gets destroyed.
        event EntityDestroyed OnEntityDestroyed;

        /// Occurs when a component gets added.
        /// All event handlers will be removed when
        /// the entity gets destroyed.
        event EntityChanged OnComponentAdded;

        /// Occurs when a component gets removed.
        /// All event handlers will be removed when
        /// the entity gets destroyed.
        event EntityChanged OnComponentRemoved;

        /// Occurs when a component gets replaced.
        /// All event handlers will be removed when
        /// the entity gets destroyed.
        event ComponentReplaced OnComponentReplaced;

        /// Occurs when an entity gets released and is not retained anymore.
        /// All event handlers will be removed when
        /// the entity gets destroyed.
        event EntityReleased OnEntityReleased;

        /// The total amount of components an entity can possibly have.
        int totalComponents { get; }

        /// Each entity has its own unique creationIndex which will be set by
        /// the pool when you create the entity.
        int creationIndex { get; }

        /// The pool manages the state of an entity.
        /// Active entities are enabled, destroyed entities are not.
        bool isEnabled { get; }

        /// componentPools is set by the pool which created the entity and
        /// is used to reuse removed components.
        /// Removed components will be pushed to the componentPool.
        /// Use entity.CreateComponent(index, type) to get a new or
        /// reusable component from the componentPool.
        /// Use entity.GetComponentPool(index) to get a componentPool for
        /// a specific component index.
        Stack<IComponent>[] componentPools { get; }

        /// The poolMetaData is set by the pool which created the entity and
        /// contains information about the pool.
        /// It's used to provide better error messages.
        PoolMetaData poolMetaData { get; }

        // TODO Docs
        void Setup(int creationIndex, int totalComponents,
                   Stack<IComponent>[] componentPools,
                   PoolMetaData poolMetaData = null);

        /// Destroys the entity and removes all its components.
        void Destroy();

        void RemoveAllOnEntityReleasedHandlers();

        /// Adds a component at the specified index.
        /// You can only have one component at an index.
        /// Each component type must have its own constant index.
        /// The prefered way is to use the
        /// generated methods from the code generator.
        IEntity AddComponent(int index, IComponent component);

        /// Removes a component at the specified index.
        /// You can only remove a component at an index if it exists.
        /// The prefered way is to use the
        /// generated methods from the code generator.
        IEntity RemoveComponent(int index);

        /// Replaces an existing component at the specified index
        /// or adds it if it doesn't exist yet.
        /// The prefered way is to use the
        /// generated methods from the code generator.
        IEntity ReplaceComponent(int index, IComponent component);

        /// Returns a component at the specified index.
        /// You can only get a component at an index if it exists.
        /// The prefered way is to use the
        /// generated methods from the code generator.
        IComponent GetComponent(int index);

        /// Returns all added components.
        IComponent[] GetComponents();

        /// Returns all indices of added components.
        int[] GetComponentIndices();

        /// Determines whether this entity has a component
        /// at the specified index.
        bool HasComponent(int index);

        /// Determines whether this entity has components
        /// at all the specified indices.
        bool HasComponents(int[] indices);

        /// Determines whether this entity has a component
        /// at any of the specified indices.
        bool HasAnyComponent(int[] indices);

        /// Removes all components.
        void RemoveAllComponents();

        /// Returns the componentPool for the specified component index.
        /// componentPools is set by the pool which created the entity and
        /// is used to reuse removed components.
        /// Removed components will be pushed to the componentPool.
        /// Use entity.CreateComponent(index, type) to get a new or
        /// reusable component from the componentPool.
        Stack<IComponent> GetComponentPool(int index);

        /// Returns a new or reusable component from the componentPool
        /// for the specified component index.
        IComponent CreateComponent(int index, Type type);

        /// Returns a new or reusable component from the componentPool
        /// for the specified component index.
        T CreateComponent<T>(int index) where T : new();

#if ENTITAS_FAST_AND_UNSAFE

        /// Returns the number of objects that retain this entity.
        int retainCount { get; }

#else

        /// Returns the number of objects that retain this entity.
        int retainCount { get; }

#endif

        /// Retains the entity. An owner can only retain the same entity once.
        /// Retain/Release is part of AERC (Automatic Entity Reference Counting)
        /// and is used internally to prevent pooling retained entities.
        /// If you use retain manually you also have to
        /// release it manually at some point.
        IEntity Retain(object owner);

        /// Releases the entity. An owner can only release an entity
        /// if it retains it.
        /// Retain/Release is part of AERC (Automatic Entity Reference Counting)
        /// and is used internally to prevent pooling retained entities.
        /// If you use retain manually you also have to
        /// release it manually at some point.
        void Release(object owner);
    }

    public class EntityAlreadyHasComponentException : EntitasException {

        public EntityAlreadyHasComponentException(
            int index, string message, string hint) : base(
                message +
                "\nEntity already has a component at index "
                + index + "!",
                hint
            ) {
        }
    }

    public class EntityDoesNotHaveComponentException : EntitasException {

        public EntityDoesNotHaveComponentException(
            int index, string message, string hint) : base(
                message +
                "\nEntity does not have a component at index "
                + index + "!",
                hint
            ) {
        }
    }

    public class EntityIsNotEnabledException : EntitasException {

        public EntityIsNotEnabledException(string message) :
            base(
                message + "\nEntity is not enabled!",
                "The entity has already been destroyed. " +
                "You cannot modify destroyed entities."
            ) {
        }
    }

    public class EntityEqualityComparer : IEqualityComparer<IEntity> {

        public static readonly EntityEqualityComparer comparer =
            new EntityEqualityComparer();

        public bool Equals(IEntity x, IEntity y) {
            return x == y;
        }

        public int GetHashCode(IEntity obj) {
            return obj.creationIndex;
        }
    }

    public class EntityIsAlreadyRetainedByOwnerException : EntitasException {

        public EntityIsAlreadyRetainedByOwnerException(
            Entity entity, object owner) : base(
                "'" + owner + "' cannot retain " + entity + "!\n" +
                "Entity is already retained by this object!",
                "The entity must be released by this object first."
            ) {
        }
    }

    public class EntityIsNotRetainedByOwnerException : EntitasException {

        public EntityIsNotRetainedByOwnerException(Entity entity, object owner) :
            base(
                "'" + owner + "' cannot release " + entity + "!\n" +
                "Entity is not retained by this object!",
                "An entity can only be released from objects that retain it."
            ) {
        }
    }
}
