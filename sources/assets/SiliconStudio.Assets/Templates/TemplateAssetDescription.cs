using System;
using System.Linq;
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Templates
{
    /// <summary>
    /// A template for using an existing Asset as a template, expecting a <see cref="Assets.Asset"/> to be accessible 
    /// from <see cref="TemplateDescription.FullPath"/> with the same name as this template.
    /// </summary>
    [DataContract("TemplateAsset")]
    public class TemplateAssetDescription : TemplateDescription
    {
        public string AssetTypeName { get; set; }

        public bool RequireName { get; set; } = true;

        public Type GetAssetType()
        {
            return AssetRegistry.GetPublicTypes().FirstOrDefault(x => x.Name == AssetTypeName);
        }
    }
    [DataContract("TemplateAssetFactory")]
    public class TemplateAssetFactoryDescription : TemplateAssetDescription
    {
        private IAssetFactory<Asset> factory;

        public string FactoryTypeName { get; set; }

        public IAssetFactory<Asset> GetFactory()
        {
            if (factory != null)
                return factory;

            if (FactoryTypeName != null)
            {
                factory = AssetRegistry.GetAssetFactory(FactoryTypeName);
            }
            else
            {
                var assetType = GetAssetType();
                var factoryType = typeof(DefaultAssetFactory<>).MakeGenericType(assetType);
                factory = (IAssetFactory<Asset>)Activator.CreateInstance(factoryType);
            }
            return factory;
        }
    }
}
