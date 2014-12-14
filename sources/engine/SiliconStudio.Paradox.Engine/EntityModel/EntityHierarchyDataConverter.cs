namespace SiliconStudio.Paradox.EntityModel.Data
{
    internal class EntityHierarchyDataConverter : DataConverter<EntityHierarchyData, Entity>
    {
        [SiliconStudio.Core.ModuleInitializer]
        internal static void Initialize()
        {
            ConverterContext.RegisterConverter(new EntityHierarchyDataConverter());
        }

        public void SaveEntityData(ConverterContext converterContext, EntityHierarchyData entityHierarchyData, Entity entity)
        {
            EntityData entityData = null;
            converterContext.ConvertToData(ref entityData, entity);
            entityHierarchyData.Entities.Add(entityData);

            foreach (var child in entity.Transformation.Children)
            {
                SaveEntityData(converterContext, entityHierarchyData, child.Entity);
            }
        }

        public override void ConvertToData(ConverterContext converterContext, ref EntityHierarchyData entityHierarchyData, Entity entity)
        {
            entityHierarchyData = new EntityHierarchyData();

            SaveEntityData(converterContext, entityHierarchyData, entity);
        }

        public override void ConvertFromData(ConverterContext converterContext, EntityHierarchyData entityHierarchyData, ref Entity rootEntity)
        {
            // Work in two steps: first construct entities and components, and then convert actual data (to avoid problems with circular references)
            // Note: We could do it in one step (by using ConvertFromDataFlags.Default in first step), but it should help avoid uncontrollable recursion.

            // Keep list of entities. Probably not necessary since converterContext cache should prevent them from being GC anyway.
            var entities = new Entity[entityHierarchyData.Entities.Count];

            // Build entities first
            for (int index = 0; index < entityHierarchyData.Entities.Count; index++)
            {
                var entityData = entityHierarchyData.Entities[index];
                entities[index] = converterContext.ConvertFromData<Entity>(entityData, ConvertFromDataFlags.Construct);
            }

            // Convert entities
            for (int index = 0; index < entityHierarchyData.Entities.Count; index++)
            {
                var entityData = entityHierarchyData.Entities[index];
                var entity = entities[index];
                converterContext.ConvertFromData(entityData, ref entity, ConvertFromDataFlags.Convert);
            }

            var rootIndex = entityHierarchyData.Entities.BinarySearch(entityHierarchyData.RootEntity);
            rootEntity = entities[rootIndex];
        }
    }
}