using System.Collections.Generic;

namespace SiliconStudio.Paradox.Graphics.Data
{
    /// <summary>
    /// Data type for <see cref="SiliconStudio.Paradox.Graphics.SpriteGroup"/>.
    /// </summary>
    [Core.DataContract("ImageGroupData")]
    public class ImageGroupData<T> where T : ImageFragmentData
    {
        public List<T> Images = new List<T>();
    }

    /// <summary>
    /// Converter type for <see cref="SiliconStudio.Paradox.Graphics.SpriteGroup"/>.
    /// </summary>
    public class ImageGroupDataConverter<TImageGroupData, TImageGroup, TImageData, TImage> : Core.Serialization.Converters.DataConverter<TImageGroupData, TImageGroup>
        where TImageGroupData : ImageGroupData<TImageData>, new ()
        where TImageGroup : ImageGroup<TImage>, new ()
        where TImageData : ImageFragmentData
        where TImage : ImageFragment, new()
    {
        /// <inheritdoc/>
        public override void ConvertToData(Core.Serialization.Converters.ConverterContext context, ref TImageGroupData target, TImageGroup source)
        {
            if (target == null)
                target = new TImageGroupData();
        }

        /// <inheritdoc/>
        public override void ConvertFromData(Core.Serialization.Converters.ConverterContext context, TImageGroupData target, ref TImageGroup source)
        {
            if (source == null)
                source = new TImageGroup();

            foreach (var imageData in target.Images)
            {
                var sprite = new TImage();
                context.ConvertFromData(imageData, ref sprite);
                source.Images.Add(sprite);
            }
        }
    }
}