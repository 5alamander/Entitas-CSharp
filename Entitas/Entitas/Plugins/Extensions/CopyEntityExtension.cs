namespace Entitas {

    public static class CopyEntityExtension {

        public static void CopyTo<TEntity>(this TEntity entity, TEntity target, bool replaceExisting = false, params int[] indices)
            where TEntity : class, IEntity, new() {
            var componentIndices = indices.Length == 0
                                          ? entity.GetComponentIndices()
                                          : indices;

            for(int i = 0; i < componentIndices.Length; i++) {
                var index = componentIndices[i];
                var component = entity.GetComponent(index);
                var clonedComponent = target.CreateComponent(
                    index, component.GetType()
                );
                component.CopyPublicMemberValues(clonedComponent);

                if(replaceExisting) {
                    target.ReplaceComponent(index, clonedComponent);
                } else {
                    target.AddComponent(index, clonedComponent);
                }
            }
        }
    }
}
