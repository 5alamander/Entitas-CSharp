namespace Entitas {

    public class EntityInfoException : EntitasException {

        public EntityInfoException(IPool pool, EntityInfo poolMetaData) :
            base("Invalid EntityInfo for '" + pool + "'!\nExpected " +
                    pool.totalComponents + " componentName(s) but got " +
                    poolMetaData.componentNames.Length + ":",
                    string.Join("\n", poolMetaData.componentNames)) {
        }
    }
}
