using System;

namespace Entitas {

    public class ContextInfo {

        public string contextName { get; private set; }
        public string[] componentNames { get; private set; }
        public Type[] componentTypes { get; private set; }

        public ContextInfo(string contextName, string[] componentNames, Type[] componentTypes) {
            this.contextName = contextName;
            this.componentNames = componentNames;
            this.componentTypes = componentTypes;
        }
    }
}
