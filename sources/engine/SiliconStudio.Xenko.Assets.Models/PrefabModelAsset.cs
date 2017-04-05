using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Models
{
    /// <summary>
    /// A model asset that is generated from a prefab, combining and merging meshes by materials and layout.
    /// </summary>
    [DataContract("PrefabModelAsset")]
    [AssetDescription(FileExtension)]
    [AssetContentType(typeof(Model))]
    [Display(1855, "Prefab Model")]
    public sealed class PrefabModelAsset : Asset, IModelAsset
    {
        /// <summary>
        /// The default file extension used by the <see cref="ProceduralModelAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkprefabmodel";

        [DataMember]
        public List<ModelMaterial> Materials { get; } = new List<ModelMaterial>();

        [DataMember]
        public AssetReference Prefab { get; set; }

//        public IEnumerable<IReference> EnumerateCompileTimeDependencies(PackageSession session)
//        {
//            if (Prefab != null)
//            {
//                // Return the prefab itself
//                yield return Prefab;
//
//                // Then we need to return used models and materials because they affects how the meshes are generated
//                var prefab = session.FindAsset(Prefab.Location)?.Asset as EntityHierarchyAssetBase;
//                if (prefab != null)
//                {
//                    // Use a dictionary to ensure each reference is yielded only once
//                    var references = new Dictionary<AssetId, IReference>();
//                    foreach (var entity in prefab.Hierarchy.Parts)
//                    {
//                        // Gather all entities with a model component and a valid model
//                        var modelComponent = entity.Entity.Get<ModelComponent>();
//                        if (modelComponent?.Model != null)
//                        {
//                            var modelReference = AttachedReferenceManager.GetAttachedReference(modelComponent.Model);
//                            var model = session.FindAsset(modelReference.Url)?.Asset as IModelAsset;
//                            if (model != null)
//                            {
//                                references[modelReference.Id] = modelReference;
//
//                                var enumerableModel = model as IAssetCompileTimeDependencies;
//                                if (enumerableModel != null)
//                                {
//                                    foreach (var enumerateCompileTimeDependency in enumerableModel.EnumerateCompileTimeDependencies(session))
//                                    {
//                                        references[enumerateCompileTimeDependency.Id] = enumerateCompileTimeDependency;
//                                    }
//                                }
//
//                                // Build the list of material for this model
//                                var materialList = model.Materials.Select(x => x.MaterialInstance.Material).ToList();
//                                for (var i = 0; i < modelComponent.Materials.Count && i < materialList.Count; i++)
//                                foreach (var material in modelComponent.Materials)
//                                {
//                                    // Apply any material override from the model component
//                                    materialList[material.Key] = material.Value;
//                                }
//
//                                // Add the model and the related materials to the list of reference
//                                references[modelReference.Id] = modelReference;
//                                foreach (var material in materialList)
//                                {
//                                    var materialReference = AttachedReferenceManager.GetAttachedReference(material);
//                                    if (materialReference != null)
//                                    {
//                                        references[materialReference.Id] = materialReference;
//                                    }
//                                }
//                            }
//                        }
//                    }
//
//                    // Finally return all the referenced models and materials
//                    foreach (var reference in references.Values)
//                    {
//                        yield return reference;
//                    }
//                }
//            }
//        }
    }
}
