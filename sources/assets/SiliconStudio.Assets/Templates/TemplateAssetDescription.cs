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
    }
}
