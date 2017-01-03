using System;

namespace Entitas {

    public class EntityInfo {

        public string poolName { get; private set; }
        public string[] componentNames { get; private set; }
        public Type[] componentTypes { get; private set; }

        public EntityInfo(string poolName, string[] componentNames, Type[] componentTypes) {
            this.poolName = poolName;
            this.componentNames = componentNames;
            this.componentTypes = componentTypes;
        }
    }
}
