using Entitas.Serialization.Blueprints;

namespace Entitas {

    public static class BlueprintEntityExtension {

        public static IEntity ApplyBlueprint(this IEntity entity, Blueprint blueprint, bool replaceComponents = false) {
            var componentsLength = blueprint.components.Length;
            for (int i = 0; i < componentsLength; i++) {
                var componentBlueprint = blueprint.components[i];
                if(replaceComponents) {
                    entity.ReplaceComponent(componentBlueprint.index, componentBlueprint.CreateComponent(entity));
                } else {
                    entity.AddComponent(componentBlueprint.index, componentBlueprint.CreateComponent(entity));
                }
            }

            return entity;
        }
    }
}
