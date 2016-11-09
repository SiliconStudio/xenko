using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Core.Reflection
{
    // This class exists only for backward compatibility with previous ~Id. It can be removed once we drop backward support
    internal class ShadowId
    {
        /// <summary>
        /// Special member id used to serialize attached id to an object.
        /// </summary>
        public const string YamlSpecialId = "~Id";

        // TODO: Should we reinitialize this when assemblies are reloaded?
        private static readonly Dictionary<Type, bool> IdentifiableTypes = new Dictionary<Type, bool>();
        private static readonly ConditionalWeakTable<object, ShadowId> Shadows = new ConditionalWeakTable<object, ShadowId>();

        private readonly bool isIdentifiable;
        private Guid? id;

        // Do not rename any string from this array, it is used for migration purpose only!
        private static readonly string[] PreviouslyNonIdentifiableTypes =
        {
            "AssetBase",
            "AssetBaseMutable",
            "AssetFolder",
            "ComputeNode",
            "EntityComponentReference",
            "EntityDesign",
            "IdentifiableAssetPartReference",
            "MaterialFeature",
            "MaterialOverrides",
            "MostRecentlyUsedFile",
            "MRUAdditionalData",
            "PackageDependency",
            "PackageMeta",
            "PackageProfile",
            "PackageVersion",
            "PrimitiveProceduralModelBase",
            "RenderFrameProviderBase",
            "SceneSettingsData",
            "SettingsFile",
            "SettingsProfile",
            "UIDesign",
            "UIElementDesign",
        };

        internal ShadowId()
        {
        }

        private ShadowId(Type type)
        {
            isIdentifiable = IsTypeIdentifiable(type);
        }


        public static bool IsTypeIdentifiable(Type type)
        {
            bool result;
            lock (IdentifiableTypes)
            {
                if (IdentifiableTypes.TryGetValue(type, out result))
                    return result;

                var currentType = type;
                var nonIdentifiable = false;
                while (currentType != null)
                {
                    if (PreviouslyNonIdentifiableTypes.Contains(currentType.Name))
                    {
                        nonIdentifiable = true;
                        break;
                    }

                    currentType = currentType.BaseType;
                }


                // Early exit if we don't need to add a unique identifier to a type
                result = !(type == typeof(string)
                           || type.GetTypeInfo().IsValueType
                           || type.GetTypeInfo().IsArray
                           || type.IsCollection()
                           || type.IsDictionary()
                           || nonIdentifiable);

                IdentifiableTypes.Add(type, result);
            }
            return result;
        }

        public static ShadowId Get(object instance)
        {
            if (instance == null) return null;
            ShadowId shadow;
            Shadows.TryGetValue(instance, out shadow);
            return shadow;
        }

        public static ShadowId GetOrCreate(object instance)
        {
            if (instance == null) return null;
            var shadow = Shadows.GetValue(instance, callback => new ShadowId(instance.GetType()));
            return shadow;
        }

        public Guid GetId(object instance)
        {
            // If the object is not identifiable, early exit
            if (!isIdentifiable)
            {
                return Guid.Empty;
            }

            // Don't use  local id if the object is already identifiable

            // If an object has an attached reference, we cannot use the id of the instance
            // So we need to use an auto-generated Id
            var attachedReference = AttachedReferenceManager.GetAttachedReference(instance);
            if (attachedReference == null)
            {
                var component = instance as IIdentifiable;
                if (component != null)
                {
                    return component.Id;
                }
            }

            // If we don't have yet an id, create one.
            if (!id.HasValue)
            {
                id = Guid.NewGuid();
            }

            return id.Value;
        }

        public void SetId(object instance, Guid newId)
        {
            // If the object is not identifiable, early exit
            if (!isIdentifiable)
            {
                return;
            }

            // If the object instance is already identifiable, store id into it directly
            var attachedReference = AttachedReferenceManager.GetAttachedReference(instance);
            var component = instance as IIdentifiable;
            if (attachedReference == null && component != null)
            {
                component.Id = newId;
            }
            else
            {
                id = newId;
            }
        }
    }
}