using System.Collections.Generic;

namespace Entitas {

    public class Systems : IInitializeSystem, IExecuteSystem,
                           ICleanupSystem, ITearDownSystem {

        protected readonly List<IInitializeSystem> _initializeSystems;
        protected readonly List<IExecuteSystem> _executeSystems;
        protected readonly List<ICleanupSystem> _cleanupSystems;
        protected readonly List<ITearDownSystem> _tearDownSystems;

        public Systems() {
            _initializeSystems = new List<IInitializeSystem>();
            _executeSystems = new List<IExecuteSystem>();
            _cleanupSystems = new List<ICleanupSystem>();
            _tearDownSystems = new List<ITearDownSystem>();
        }

        public Systems Add(ISystem system) {
            var initializeSystem = system as IInitializeSystem;
            if(initializeSystem != null) {
                _initializeSystems.Add(initializeSystem);
            }

            var executeSystem = system as IExecuteSystem;
            if(executeSystem != null) {
                _executeSystems.Add(executeSystem);
            }

            var cleanupSystem = system as ICleanupSystem;
            if(cleanupSystem != null) {
                _cleanupSystems.Add(cleanupSystem);
            }

            var tearDownSystem = system as ITearDownSystem;
            if(tearDownSystem != null) {
                _tearDownSystems.Add(tearDownSystem);
            }

            return this;
        }

        public void Initialize() {
            for (int i = 0; i < _initializeSystems.Count; i++) {
                _initializeSystems[i].Initialize();
            }
        }

        public void Execute() {
            for (int i = 0; i < _executeSystems.Count; i++) {
                _executeSystems[i].Execute();
            }
        }

        public void Cleanup() {
            for (int i = 0; i < _cleanupSystems.Count; i++) {
                _cleanupSystems[i].Cleanup();
            }
        }

        public void TearDown() {
            for (int i = 0; i < _tearDownSystems.Count; i++) {
                _tearDownSystems[i].TearDown();
            }
        }

        public void ActivateReactiveSystems() {
            for (int i = 0; i < _executeSystems.Count; i++) {
                var system = _executeSystems[i];
                var reactiveSystem = system as IReactiveSystem;
                if(reactiveSystem != null) {
                    reactiveSystem.Activate();
                }

                var nestedSystems = system as Systems;
                if(nestedSystems != null) {
                    nestedSystems.ActivateReactiveSystems();
                }
            }
        }

        public void DeactivateReactiveSystems() {
            for (int i = 0; i < _executeSystems.Count; i++) {
                var system = _executeSystems[i];
                var reactiveSystem = system as IReactiveSystem;
                if(reactiveSystem != null) {
                    reactiveSystem.Deactivate();
                }

                var nestedSystems = system as Systems;
                if(nestedSystems != null) {
                    nestedSystems.DeactivateReactiveSystems();
                }
            }
        }

        public void ClearReactiveSystems() {
            for (int i = 0; i < _executeSystems.Count; i++) {
                var system = _executeSystems[i];
                var reactiveSystem = system as IReactiveSystem;
                if(reactiveSystem != null) {
                    reactiveSystem.Clear();
                }
                
                var nestedSystems = system as Systems;
                if(nestedSystems != null) {
                    nestedSystems.ClearReactiveSystems();
                }
            }
        }
    }
}
