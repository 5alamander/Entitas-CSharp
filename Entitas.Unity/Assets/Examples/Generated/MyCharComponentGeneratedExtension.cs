//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGenerator.ComponentExtensionsGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Entitas;

namespace Entitas {

    public partial class Entity {

        public MyCharComponent myChar { get { return (MyCharComponent)GetComponent(VisualDebuggingComponentIds.MyChar); } }
        public bool hasMyChar { get { return HasComponent(VisualDebuggingComponentIds.MyChar); } }

        public void AddMyChar(char newMyChar) {
            var component = CreateComponent<MyCharComponent>(VisualDebuggingComponentIds.MyChar);
            component.myChar = newMyChar;
            AddComponent(VisualDebuggingComponentIds.MyChar, component);
        }

        public void ReplaceMyChar(char newMyChar) {
            var component = CreateComponent<MyCharComponent>(VisualDebuggingComponentIds.MyChar);
            component.myChar = newMyChar;
            ReplaceComponent(VisualDebuggingComponentIds.MyChar, component);
        }

        public void RemoveMyChar() {
            RemoveComponent(VisualDebuggingComponentIds.MyChar);
        }
    }
}

    public partial class VisualDebuggingMatcher {

        static IMatcher _matcherMyChar;

        public static IMatcher MyChar {
            get {
                if(_matcherMyChar == null) {
                    var matcher = (Matcher)Matcher.AllOf(VisualDebuggingComponentIds.MyChar);
                    matcher.componentNames = VisualDebuggingComponentIds.componentNames;
                    _matcherMyChar = matcher;
                }

                return _matcherMyChar;
            }
        }
    }
