using System.Linq;

namespace SiliconStudio.Paradox.EntityModel.Data
{
    internal class EntityDataConverter : DataConverter<EntityData, Entity>
    {
        public override bool CanConstruct
        {
            get { return true; }
        }

        public override void ConstructFromData(ConverterContext converterContext, EntityData entityData, ref Entity entity)
        {
            entity = new Entity(entityData.Name);
            entity.Id = entityData.Id;
            foreach (var component in entityData.Components)
            {
                entity.Components.SetObject(component.Key, converterContext.ConvertFromData<EntityComponent>(component.Value, ConvertFromDataFlags.Construct));
            }
        }

        public override void ConvertFromData(ConverterContext converterContext, EntityData entityData, ref Entity entity)
        {
            foreach (var component in entityData.Components)
            {
                var entityComponent = (EntityComponent)entity.Components.Get(component.Key);
                converterContext.ConvertFromData(component.Value, ref entityComponent, ConvertFromDataFlags.Convert);
                entity.Components.SetObject(component.Key, entityComponent);
            }
        }

        public override void ConvertToData(ConverterContext converterContext, ref EntityData entityData, Entity entity)
        {
            entityData = new EntityData { Name = entity.Name };

            foreach (var component in entity.Components.Where(x => x.Value is EntityComponent))
            {
                entityData.Components.Add(component.Key, converterContext.ConvertToData<EntityComponentData>(component.Value));
            }
        }
    }
}