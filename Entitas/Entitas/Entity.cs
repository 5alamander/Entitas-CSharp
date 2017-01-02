using System;
using System.Collections.Generic;
using System.Text;

namespace Entitas {

    public class Entity : IEntity {

        public event EntityDestroyedHandler OnEntityDestroyed;
        public event EntityChangedHandler OnComponentAdded;
        public event EntityChangedHandler OnComponentRemoved;
        public event ComponentReplacedHandler OnComponentReplaced;
        public event EntityReleasedHandler OnEntityReleased;

        public int totalComponents { get { return _totalComponents; } }
        public int creationIndex { get { return _creationIndex; } }
        public bool isEnabled { get { return _isEnabled; } }
        public Stack<IComponent>[] componentPools {
            get { return _componentPools; }
        }
        public PoolMetaData poolMetaData { get { return _poolMetaData; } }

        internal int _creationIndex;
        internal bool _isEnabled = true;

        int _totalComponents;
        IComponent[] _components;
        Stack<IComponent>[] _componentPools;
        PoolMetaData _poolMetaData;

        IComponent[] _componentsCache;
        int[] _componentIndicesCache;
        string _toStringCache;
        StringBuilder _toStringBuilder;

        public void Setup(int _creationIndex, int totalComponents,
                          Stack<IComponent>[] componentPools,
                          PoolMetaData poolMetaData = null) {
            
            _totalComponents = totalComponents;
            _components = new IComponent[totalComponents];
            _componentPools = componentPools;

            if(poolMetaData != null) {
                _poolMetaData = poolMetaData;
            } else {

                // If pool.CreateEntity() was used to create the entity,
                // we will never end up here.
                // This is a fallback when entities are created manually.

                var componentNames = new string[totalComponents];
                for(int i = 0; i < componentNames.Length; i++) {
                    componentNames[i] = i.ToString();
                }
                _poolMetaData = new PoolMetaData(
                    "No Pool", componentNames, null
                );
            }

            _isEnabled = true;
        }

        public void Destroy() {
            _isEnabled = false;
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
            if(!_isEnabled) {
                throw new EntityIsNotEnabledException(
                    "Cannot add component '" +
                    _poolMetaData.componentNames[index] +
                    "' to " + this + "!"
                );
            }

            if(HasComponent(index)) {
                throw new EntityAlreadyHasComponentException(
                    index,
                    "Cannot add component '" +
                    _poolMetaData.componentNames[index] +
                    "' to " + this + "!",
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
            if(!_isEnabled) {
                throw new EntityIsNotEnabledException(
                    "Cannot remove component '" +
                    _poolMetaData.componentNames[index] +
                    "' from " + this + "!"
                );
            }

            if(!HasComponent(index)) {
                throw new EntityDoesNotHaveComponentException(
                    index,
                    "Cannot remove component '" +
                    _poolMetaData.componentNames[index] +
                    "' from " + this + "!",
                    "You should check if an entity has the component " +
                    "before removing it."
                );
            }

            replaceComponent(index, null);

            return this;
        }

        public IEntity ReplaceComponent(int index, IComponent component) {
            if(!_isEnabled) {
                throw new EntityIsNotEnabledException(
                    "Cannot replace component '" +
                    _poolMetaData.componentNames[index] +
                    "' on " + this + "!"
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
                    index,
                    "Cannot get component '" +
                    _poolMetaData.componentNames[index] + "' from " +
                    this + "!",
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
        
        public int retainCount { get { return _retainCount; } }
        int _retainCount;

#else

        public int retainCount { get { return owners.Count; } }

        public readonly HashSet<object> owners = new HashSet<object>();

#endif

        public IEntity Retain(object owner) {

#if ENTITAS_FAST_AND_UNSAFE
            
            _retainCount += 1;

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
            
            _retainCount -= 1;
            if(_retainCount == 0) {

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
                    .Append(_creationIndex)
                    .Append("(*")
                    .Append(retainCount)
                    .Append(")")
                    .Append("(");

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
