namespace SiliconStudio.Paradox.Physics
{
    /// <summary>
    /// Converter type for <see cref="PhysicsElement"/>.
    /// </summary>
    public class PhysicsElementDataConverter : Core.Serialization.Converters.DataConverter<PhysicsElementData, PhysicsElement>
    {
        /// <inheritdoc/>
        public override void ConvertToData(Core.Serialization.Converters.ConverterContext context, ref PhysicsElementData target, PhysicsElement source)
        {
            target = new PhysicsElementData { Type = source.Type, LinkedBoneName = source.LinkedBoneName };
            context.ConvertToData(ref target.Shape, source.Shape);
            target.CollisionGroup = source.CollisionGroup;
            target.CanCollideWith = source.CanCollideWith;
            target.StepHeight = source.StepHeight;
            target.Sprite = source.Sprite;
        }

        /// <inheritdoc/>
        public override void ConvertFromData(Core.Serialization.Converters.ConverterContext context, PhysicsElementData target, ref PhysicsElement source)
        {
            source = new PhysicsElement { Type = target.Type, LinkedBoneName = target.LinkedBoneName };
            {
                var temp = source.Shape;
                context.ConvertFromData(target.Shape, ref temp);
                source.Shape = temp;
            }
            source.CollisionGroup = target.CollisionGroup;
            source.CanCollideWith = target.CanCollideWith;
            source.StepHeight = target.StepHeight;
            source.Sprite = target.Sprite;
        }
    }
}
