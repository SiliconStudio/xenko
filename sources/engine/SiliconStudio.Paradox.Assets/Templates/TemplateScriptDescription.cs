using SiliconStudio.Assets.Templates;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Templates
{
    /// <summary>
    /// A template for using an existing Script code file as a template, expecting a .cs file to be accessible 
    /// from <see cref="TemplateDescription.FullPath"/> with the same name as this template.
    /// Templates must be delcared in the package file, with their respective .pdxtpl file within the Scripts group
    /// </summary>
    [DataContract("TemplateScript")]
    public class TemplateScriptDescription : TemplateAssetDescription
    {
    }
}