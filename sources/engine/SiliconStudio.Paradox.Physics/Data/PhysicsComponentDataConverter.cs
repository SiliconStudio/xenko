namespace SiliconStudio.Paradox.Physics
{
    /// <summary>
    /// Converter type for <see cref="PhysicsComponent"/>.
    /// </summary>
    public class PhysicsComponentDataConverter : Core.Serialization.Converters.DataConverter<PhysicsComponentData, PhysicsComponent>
    {
        /// <inheritdoc/>
        public override void ConvertToData(Core.Serialization.Converters.ConverterContext context, ref PhysicsComponentData target, PhysicsComponent source)
        {
            target = new PhysicsComponentData();
            context.ConvertToData(ref target.Elements, source.Elements);
        }

        public override bool CanConstruct
        {
            get { return true; }
        }

        /// <inheritdoc/>
        public override void ConstructFromData(Core.Serialization.Converters.ConverterContext context, PhysicsComponentData target, ref PhysicsComponent source)
        {
            source = new PhysicsComponent();
        }

        /// <inheritdoc/>
        public override void ConvertFromData(Core.Serialization.Converters.ConverterContext context, PhysicsComponentData target, ref PhysicsComponent source)
        {
            {
                var temp = source.Elements;
                context.ConvertFromData(target.Elements, ref temp);
            }
        }
    }
}