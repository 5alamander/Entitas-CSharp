namespace Entitas {

    public class EntityInfoException : EntitasException {

        public EntityInfoException(IContext context, ContextInfo contextInfo) :
            base("Invalid EntityInfo for '" + context + "'!\nExpected " +
                    context.totalComponents + " componentName(s) but got " +
                    contextInfo.componentNames.Length + ":",
                    string.Join("\n", contextInfo.componentNames)) {
        }
    }
}
