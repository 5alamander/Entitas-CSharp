using System;
using System.Collections.Generic;
using System.Text;

namespace Entitas {

    public class XXXEntity : IEntity {

        public event EntityChangedHandler OnComponentAdded;
        public event EntityChangedHandler OnComponentRemoved;
        public event ComponentReplacedHandler OnComponentReplaced;

        public event EntityDestroyedHandler OnEntityDestroyed;
        public event EntityReleasedHandler OnEntityReleased;

        public int totalComponents { get; private set; }
        public int creationIndex { get; private set; }

        public ContextInfo entityInfo { get; private set; }
        public bool isEnabled { get; private set; }

        Stack<IComponent>[] _componentPools;

        IComponent[] _components;
        IComponent[] _componentsCache;
        int[] _componentIndicesCache;
        string _toStringCache;
        StringBuilder _toStringBuilder;

        public void Initialize(int totalComponents, int creationIndexsss,
                               Stack<IComponent>[] componentPools,
                               ContextInfo entityInfo = null) {
            
            this.totalComponents = totalComponents;
            _componentPools = componentPools;
            this.entityInfo = entityInfo ?? createDefaultEntityInfo();
            _components = new IComponent[totalComponents];
            Reactivate(creationIndex);
        }

        ContextInfo createDefaultEntityInfo() {
            var componentNames = new string[totalComponents];
            for(int i = 0; i < componentNames.Length; i++) {
                componentNames[i] = i.ToString();
            }

            return new ContextInfo("No Context", componentNames, null);
        }

        public void Reactivate(int creationIndex) {
            this.creationIndex = creationIndex;
            isEnabled = true;
        }

        public void Destroy() {
            isEnabled = false;
            RemoveAllComponents();
            OnComponentAdded = null;
            OnComponentReplaced = null;
            OnComponentRemoved = null;
            

            // TODO Test
            if(OnEntityDestroyed != null) {
                OnEntityDestroyed(this);
            }


            // TODO Test
            OnEntityDestroyed = null;
        }

        public void RemoveAllOnEntityReleasedHandlers() {
            OnEntityReleased = null;
        }

        public IEntity AddComponent(int index, IComponent component) {
            if(!isEnabled) {
                throw new EntityIsNotEnabledException(
                    "Cannot add component '" +
                    entityInfo.componentNames[index] + "' to " + this + "!"
                );
            }

            if(HasComponent(index)) {
                throw new EntityAlreadyHasComponentException(
                    index,
                    "Cannot add component '" +
                    entityInfo.componentNames[index] + "' to " + this + "!",
                    "You should check if an entity already has the component " +
                    "before adding it or use entity.ReplaceComponent()."
                );
            }

            _components[index] = component;
            _componentsCache = null;
            _componentIndicesCache = null;
            _toStringCache = null;
            if(OnComponentAdded != null) {
                OnComponentAdded(this, index, component);
            }

            return this;
        }

        public IEntity RemoveComponent(int index) {
            if(!isEnabled) {
                throw new EntityIsNotEnabledException(
                    "Cannot remove component '" +
                    entityInfo.componentNames[index] + "' from " + this + "!"
                );
            }

            if(!HasComponent(index)) {
                throw new EntityDoesNotHaveComponentException(
                    index, "Cannot remove component '" +
                    entityInfo.componentNames[index] + "' from " + this + "!",
                    "You should check if an entity has the component " +
                    "before removing it."
                );
            }

            replaceComponent(index, null);

            return this;
        }

        public IEntity ReplaceComponent(int index, IComponent component) {
            if(!isEnabled) {
                throw new EntityIsNotEnabledException(
                    "Cannot replace component '" +
                    entityInfo.componentNames[index] + "' on " + this + "!"
                );
            }

            if(HasComponent(index)) {
                replaceComponent(index, component);
            } else if(component != null) {
                AddComponent(index, component);
            }

            return this;
        }

        void replaceComponent(int index, IComponent replacement) {
            _toStringCache = null;
            var previousComponent = _components[index];
            if(replacement != previousComponent) {
                _components[index] = replacement;
                _componentsCache = null;
                if(replacement != null) {
                    if(OnComponentReplaced != null) {
                        OnComponentReplaced(
                            this, index, previousComponent, replacement
                        );
                    }
                } else {
                    _componentIndicesCache = null;
                    if(OnComponentRemoved != null) {
                        OnComponentRemoved(this, index, previousComponent);
                    }
                }

                GetComponentPool(index).Push(previousComponent);

            } else {
                if(OnComponentReplaced != null) {
                    OnComponentReplaced(
                        this, index, previousComponent, replacement
                    );
                }
            }
        }

        public IComponent GetComponent(int index) {
            if(!HasComponent(index)) {
                throw new EntityDoesNotHaveComponentException(
                    index, "Cannot get component '" +
                    entityInfo.componentNames[index] + "' from " + this + "!",
                    "You should check if an entity has the component " +
                    "before getting it."
                );
            }

            return _components[index];
        }

        public IComponent[] GetComponents() {
            if(_componentsCache == null) {
                var components = EntitasCache.GetIComponentList();

                    for(int i = 0; i < _components.Length; i++) {
                        var component = _components[i];
                        if(component != null) {
                            components.Add(component);
                        }
                    }

                    _componentsCache = components.ToArray();

                EntitasCache.PushIComponentList(components);
            }

            return _componentsCache;
        }

        public int[] GetComponentIndices() {
            if(_componentIndicesCache == null) {
                var indices = EntitasCache.GetIntList();

                    for(int i = 0; i < _components.Length; i++) {
                        if(_components[i] != null) {
                            indices.Add(i);
                        }
                    }

                    _componentIndicesCache = indices.ToArray();

                EntitasCache.PushIntList(indices);
            }

            return _componentIndicesCache;
        }

        public bool HasComponent(int index) {
            return _components[index] != null;
        }

        public bool HasComponents(int[] indices) {
            for(int i = 0; i < indices.Length; i++) {
                if(_components[indices[i]] == null) {
                    return false;
                }
            }

            return true;
        }

        public bool HasAnyComponent(int[] indices) {
            for(int i = 0; i < indices.Length; i++) {
                if(_components[indices[i]] != null) {
                    return true;
                }
            }

            return false;
        }

        public void RemoveAllComponents() {
            _toStringCache = null;
            for(int i = 0; i < _components.Length; i++) {
                if(_components[i] != null) {
                    replaceComponent(i, null);
                }
            }
        }

        public Stack<IComponent> GetComponentPool(int index) {
            var componentPool = _componentPools[index];
            if(componentPool == null) {
                componentPool = new Stack<IComponent>();
                _componentPools[index] = componentPool;
            }

            return componentPool;
        }

        public IComponent CreateComponent(int index, Type type) {
            var componentPool = GetComponentPool(index);
            return componentPool.Count > 0
                    ? componentPool.Pop()
                    : (IComponent)Activator.CreateInstance(type);
        }

        public T CreateComponent<T>(int index) where T : new() {
            var componentPool = GetComponentPool(index);
            return componentPool.Count > 0 ? (T)componentPool.Pop() : new T();
        }

#if ENTITAS_FAST_AND_UNSAFE
        public int retainCount { get; private set; }
#else
        public readonly HashSet<object> owners = new HashSet<object>();
        public int retainCount { get { return owners.Count; } }
#endif

        public IEntity Retain(object owner) {

#if ENTITAS_FAST_AND_UNSAFE
            retainCount += 1;
#else
            if(!owners.Add(owner)) {
                throw new EntityIsAlreadyRetainedByOwnerException(this, owner);
            }
#endif

            _toStringCache = null;

            return this;
        }

        public void Release(object owner) {

#if ENTITAS_FAST_AND_UNSAFE
            retainCount -= 1;
            if(retainCount == 0) {
#else
            if(!owners.Remove(owner)) {
                throw new EntityIsNotRetainedByOwnerException(this, owner);
            }

            if(owners.Count == 0) {
#endif

                _toStringCache = null;

                if(OnEntityReleased != null) {
                    OnEntityReleased(this);
                }
            }
        }

        public override string ToString() {
            if(_toStringCache == null) {
                if(_toStringBuilder == null) {
                    _toStringBuilder = new StringBuilder();
                }
                _toStringBuilder.Length = 0;
                _toStringBuilder
                    .Append("Entity_")
                    .Append(creationIndex)
                    .Append("(*")
                    .Append(retainCount)
                    .Append(")(");

                const string separator = ", ";
                var components = GetComponents();
                var lastSeparator = components.Length - 1;
                for(int i = 0; i < components.Length; i++) {
                    var component = components[i];
                    var type = component.GetType();
                    var implementsToString = type.GetMethod("ToString")
                                                 .DeclaringType == type;
                    _toStringBuilder.Append(
                        implementsToString
                            ? component.ToString()
                            : type.Name.RemoveComponentSuffix()
                    );

                    if(i < lastSeparator) {
                        _toStringBuilder.Append(separator);
                    }
                }

                _toStringBuilder.Append(")");
                _toStringCache = _toStringBuilder.ToString();
            }

            return _toStringCache;
        }
    }
}
