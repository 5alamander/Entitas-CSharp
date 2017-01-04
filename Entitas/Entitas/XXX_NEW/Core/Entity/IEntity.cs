using System;
using System.Collections.Generic;

namespace Entitas {

    public interface IEntity {

        event EntityChangedHandler OnComponentAdded;
        event EntityChangedHandler OnComponentRemoved;
        event ComponentReplacedHandler OnComponentReplaced;

        event EntityDestroyedHandler OnEntityDestroyed;
        event EntityReleasedHandler OnEntityReleased;

        int totalComponents { get; }
        int creationIndex { get; }

        ContextInfo entityInfo { get; }
        bool isEnabled { get; }

        void Initialize(int totalComponents, int creationIndex,
                   Stack<IComponent>[] componentPools,
                   ContextInfo entityInfo = null);

        void Reactivate(int creationIndex);

        void Destroy();
        void RemoveAllOnEntityReleasedHandlers();

        IEntity AddComponent(int index, IComponent component);
        IEntity RemoveComponent(int index);
        IEntity ReplaceComponent(int index, IComponent component);

        IComponent GetComponent(int index);

        IComponent[] GetComponents();
        int[] GetComponentIndices();

        bool HasComponent(int index);
        bool HasComponents(int[] indices);
        bool HasAnyComponent(int[] indices);

        void RemoveAllComponents();

        Stack<IComponent> GetComponentPool(int index);

        IComponent CreateComponent(int index, Type type);
        T CreateComponent<T>(int index) where T : new();

        IEntity Retain(object owner);
        void Release(object owner);
        int retainCount { get; }
    }
}
