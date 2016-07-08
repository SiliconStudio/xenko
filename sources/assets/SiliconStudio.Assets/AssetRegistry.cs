// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SharpYaml.Serialization;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.VisualStudio;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A registry for various content associated with assets.
    /// </summary>
    public static class AssetRegistry
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("Assets.Registry");

        private static readonly Dictionary<Type, string> RegisteredDefaultAssetExtension = new Dictionary<Type, string>();
        private static readonly HashSet<Type> AssetTypes = new HashSet<Type>();
        private static readonly HashSet<Type> RegisteredPackageSessionAnalysisTypes = new HashSet<Type>();
        private static readonly List<IAssetImporter> RegisteredImportersInternal = new List<IAssetImporter>();
        private static readonly Dictionary<Type, SortedList<string, PackageVersion>> RegisteredFormatVersions = new Dictionary<Type, SortedList<string, PackageVersion>>();
        private static readonly HashSet<Type> AlwaysMarkAsRootAssetTypes = new HashSet<Type>();
        private static readonly Dictionary<KeyValuePair<Type, string>, AssetUpgraderCollection> RegisteredAssetUpgraders = new Dictionary<KeyValuePair<Type, string>, AssetUpgraderCollection>();
        private static readonly Dictionary<string, Type> RegisteredAssetFileExtensions = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
        private static readonly Dictionary<string, PackageUpgrader> RegisteredPackageUpgraders = new Dictionary<string, PackageUpgrader>();
        private static readonly HashSet<Assembly> RegisteredAssemblies = new HashSet<Assembly>();
        private static readonly HashSet<IYamlSerializableFactory> RegisteredSerializerFactories = new HashSet<IYamlSerializableFactory>();
        private static readonly List<IDataCustomVisitor> RegisteredDataVisitNodes = new List<IDataCustomVisitor>();
        private static readonly List<IDataCustomVisitor> RegisteredDataVisitNodeBuilders = new List<IDataCustomVisitor>();
        private static readonly Dictionary<string, IAssetFactory<Asset>> RegisteredAssetFactories = new Dictionary<string, IAssetFactory<Asset>>();
        private static readonly Dictionary<Type, HashSet<AssetPartReferenceAttribute>> RegisteredAssetCompositePartTypes = new Dictionary<Type, HashSet<AssetPartReferenceAttribute>>();

        private static Func<object, string, string> stringExpander;

        // Global lock used to secure the registry with threads
        private static readonly object RegistryLock = new object();

        /// <summary>
        /// Gets the list of assemblies currently registered.
        /// </summary>
        public static IEnumerable<Assembly> Assemblies
        {
            get
            {
                lock (RegistryLock)
                {
                    return RegisteredAssemblies.ToList();
                }
            }
        }

        /// <summary>
        /// Gets the supported platforms.
        /// </summary>
        /// <value>The supported platforms.</value>
        public static SolutionPlatformCollection SupportedPlatforms { get; }

        /// <summary>
        /// Gets an enumeration of registered importers.
        /// </summary>
        /// <value>The registered importers.</value>
        public static IEnumerable<IAssetImporter> RegisteredImporters
        {
            get
            {
                lock (RegistryLock)
                {
                    return RegisteredImportersInternal;
                }
            }
        }

        /// <summary>
        /// Registers the supported platforms.
        /// </summary>
        /// <param name="platforms">The platforms.</param>
        /// <exception cref="System.ArgumentNullException">platforms</exception>
        public static void RegisterSupportedPlatforms(List<SolutionPlatform> platforms)
        {
            if (platforms == null) throw new ArgumentNullException(nameof(platforms));
            if (SupportedPlatforms.Count > 0) throw new InvalidOperationException("Cannot register new platforms. RegisterSupportedPlatforms can only be called once");
            SupportedPlatforms.AddRange(platforms);
        }

        /// <summary>
        /// Registers the string expander used by the package references.
        /// </summary>
        /// <param name="expander">The expander.</param>
        public static void RegisterStringExpander(Func<object, string, string> expander)
        {
            stringExpander = expander;
        }

        /// <summary>
        /// Expands a string using the registered string expander (<see cref="RegisterStringExpander"/>)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="stringToExpand">The string to expand.</param>
        /// <returns>System.String.</returns>
        public static string ExpandString(object context, string stringToExpand)
        {
            if (stringExpander != null)
            {
                return stringExpander(context, stringToExpand);
            }
            return stringToExpand;
        }

        /// <summary>
        /// Gets the asset file extensions.
        /// </summary>
        /// <returns>System.String[][].</returns>
        public static string[] GetAssetFileExtensions()
        {
            lock (RegistryLock)
            {
                return RegisteredAssetFileExtensions.Keys.ToArray();
            }
        }

        /// <summary>
        /// Determines whether the file is an asset file type.
        /// </summary>
        /// <param name="extension">The file.</param>
        /// <returns><c>true</c> if [is asset file file] [the specified file]; otherwise, <c>false</c>.</returns>
        public static bool IsAssetFileExtension(string extension)
        {
            if (extension == null) return false;
            lock (RegistryLock)
            {
                return RegisteredAssetFileExtensions.ContainsKey(extension);
            }
        }

        /// <summary>
        /// Gets the asset type from the extension. If no asset type is found, return null.
        /// </summary>
        /// <param name="extension">The extension of the asset file.</param>
        /// <returns>Type of the associated asset or null if not found.</returns>
        public static Type GetAssetTypeFromFileExtension(string extension)
        {
            if (extension == null) throw new ArgumentNullException(nameof(extension));
            lock (RegistryLock)
            {
                Type result;
                RegisteredAssetFileExtensions.TryGetValue(extension, out result);
                return result;
            }
        }

        public static bool IsProjectCodeGeneratorAssetFileExtension(string extension)
        {
            if (extension == null) return false;
            lock (RegisteredAssetFileExtensions)
            {
                var valid = RegisteredAssetFileExtensions.ContainsKey(extension);
                if (valid)
                {
                    var type = RegisteredDefaultAssetExtension.Where(x => x.Value == extension).Select(x => x.Key).FirstOrDefault();
                    if (type != null)
                    {
                        return typeof(ProjectCodeGeneratorAsset).IsAssignableFrom(type);
                    }
                }
                return false;
            }
        }

        public static bool IsProjectSourceCodeAssetFileExtension(string extension)
        {
            if (extension == null) return false;
            lock (RegisteredAssetFileExtensions)
            {
                var valid = RegisteredAssetFileExtensions.ContainsKey(extension);
                if (valid)
                {
                    var type = RegisteredDefaultAssetExtension.Where(x => x.Value == extension).Select(x => x.Key).FirstOrDefault();
                    if (type != null)
                    {
                        return typeof(ProjectSourceCodeAsset).IsAssignableFrom(type);
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the default file associated with an asset.
        /// </summary>
        /// <param name="assetType">The type.</param>
        /// <returns>System.String.</returns>
        public static string GetDefaultExtension(Type assetType)
        {
            IsAssetType(assetType, true);
            lock (RegistryLock)
            {
                string extension;
                RegisteredDefaultAssetExtension.TryGetValue(assetType, out extension);
                return extension;
            }
        }

        /// <summary>
        /// Gets the current format version of an asset.
        /// </summary>
        /// <param name="assetType">The asset type.</param>
        /// <returns>The current format version of this asset.</returns>
        public static SortedList<string, PackageVersion> GetCurrentFormatVersions(Type assetType)
        {
            IsAssetType(assetType, true);
            lock (RegistryLock)
            {
                SortedList<string, PackageVersion> versions;
                RegisteredFormatVersions.TryGetValue(assetType, out versions);
                return versions;
            }
        }

        /// <summary>
        /// Gets the <see cref="AssetUpgraderCollection"/> of an asset type, if available.
        /// </summary>
        /// <param name="assetType">The asset type.</param>
        /// <param name="dependencyName">The dependency name.</param>
        /// <returns>The <see cref="AssetUpgraderCollection"/> of an asset type if available, or <c>null</c> otherwise.</returns>
        public static AssetUpgraderCollection GetAssetUpgraders(Type assetType, string dependencyName)
        {
            IsAssetType(assetType, true);
            lock (RegistryLock)
            {
                AssetUpgraderCollection upgraders;
                RegisteredAssetUpgraders.TryGetValue(new KeyValuePair<Type, string>(assetType, dependencyName), out upgraders);
                return upgraders;
            }
        }

        public static PackageUpgrader GetPackageUpgrader(string packageName)
        {
            lock (RegistryLock)
            {
                PackageUpgrader upgrader;
                RegisteredPackageUpgraders.TryGetValue(packageName, out upgrader);
                return upgrader;
            }
        }

        /// <summary>
        /// Gets the default file associated with an asset.
        /// </summary>
        /// <typeparam name="T">Type of the asset.</typeparam>
        /// <returns>System.String.</returns>
        public static string GetDefaultExtension<T>() where T : Asset
        {
            return GetDefaultExtension(typeof(T));
        }

        public static IEnumerable<Type> GetPackageSessionAnalysisTypes()
        {
            lock (RegistryLock)
            {
                return RegisteredPackageSessionAnalysisTypes;
            }
        }

        public static IAssetFactory<Asset> GetAssetFactory(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));
            lock (RegistryLock)
            {
                IAssetFactory<Asset> factory;
                RegisteredAssetFactories.TryGetValue(typeName, out factory);
                return factory;
            }
        }

        public static IEnumerable<IAssetFactory<Asset>> GetAllAssetFactories()
        {
            lock (RegistryLock)
            {
                return RegisteredAssetFactories.Values.ToList();
            }
        }

        /// <summary>
        /// Returns an array of asset types that can be instanced with <see cref="ObjectFactory.NewInstance"/>.
        /// </summary>
        /// <returns>An array of <see cref="Type"/> elements.</returns>
        public static Type[] GetInstantiableTypes()
        {
            lock (RegistryLock)
            {
                return ObjectFactory.FindRegisteredFactories().Where(type => typeof(Asset).IsAssignableFrom(type) && type.IsPublic).ToArray();
            }
        }

        public static bool IsAssetTypeAlwaysMarkAsRoot(Type type)
        {
            lock (AlwaysMarkAsRootAssetTypes)
            {
                return AlwaysMarkAsRootAssetTypes.Contains(type);
            }
        }

        /// <summary>
        /// Returns an array of asset types that are non-abstract and public.
        /// </summary>
        /// <returns>An array of <see cref="Type"/> elements.</returns>
        public static Type[] GetPublicTypes()
        {
            lock (RegistryLock)
            {
                return AssetTypes.ToArray();
            }
        }

        /// <summary>
        /// Finds the importer associated with an asset by the file of the file to import.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <returns>An instance of the importer of null if not found.</returns>
        public static IEnumerable<IAssetImporter> FindImporterForFile(string file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            lock (RegistryLock)
            {
                foreach (var importer in RegisteredImportersInternal)
                {
                    if (importer.IsSupportingFile(file))
                    {
                        yield return importer;
                    }
                }
            }
        }

        /// <summary>
        /// Finds an importer by its id.
        /// </summary>
        /// <param name="importerId">The importer identifier.</param>
        /// <returns>An instance of the importer of null if not found.</returns>
        public static IAssetImporter FindImporterById(Guid importerId)
        {
            lock (RegistryLock)
            {
                return RegisteredImportersInternal.FirstOrDefault(t => t.Id == importerId);
            }
        }

        /// <summary>
        /// Registers a <see cref="IAssetImporter" /> for the specified asset type.
        /// </summary>
        /// <param name="importer">The importer.</param>
        /// <exception cref="System.ArgumentNullException">importer</exception>
        public static void RegisterImporter(IAssetImporter importer)
        {
            if (importer == null) throw new ArgumentNullException(nameof(importer));

            // Register this importer
            lock (RegistryLock)
            {
                var existingImporter = FindImporterById(importer.Id);
                if (existingImporter != null)
                {
                    RegisteredImportersInternal.Remove(existingImporter);
                }

                RegisteredImportersInternal.Add(importer);
                RegisteredImportersInternal.Sort( (left, right) => left.Order.CompareTo(right.Order));
            }
        }

        public static IEnumerable<IDataCustomVisitor> GetDataVisitNodes()
        {
            lock (RegistryLock)
            {
                return RegisteredDataVisitNodes;
            }
        }

        public static IEnumerable<IDataCustomVisitor> GetDataVisitNodeBuilders()
        {
            lock (RegistryLock)
            {
                return RegisteredDataVisitNodeBuilders;
            }
        }

        public static bool IsAssetPartType(Type type)
        {
            lock (RegistryLock)
            {
                return RegisteredAssetCompositePartTypes.ContainsKey(type);
            }
        }

        public static IEnumerable<AssetPartReferenceAttribute> GetPartReferenceAttributes(Type partType)
        {
            lock (RegistryLock)
            {
                HashSet<AssetPartReferenceAttribute> attributes;
                RegisteredAssetCompositePartTypes.TryGetValue(partType, out attributes);
                return attributes;
            }
        }

        /// <summary>
        /// Registers the asset assembly. This assembly should provide <see cref="Asset"/> objects, associated with
        /// <see cref="IAssetCompiler"/> and optionaly a <see cref="IAssetImporter"/>.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <exception cref="System.ArgumentNullException">assembly</exception>
        /// <exception cref="AssetException">
        /// Invalid compiler type [{0}], must inherit from IAssetImporter.ToFormat(assetCompiler.TypeName)
        /// or
        /// Unable to instantiate compiler [{0}].ToFormat(assetCompiler.TypeName)
        /// or
        /// Invalid importer type [{0}], must inherit from IAssetImporter.ToFormat(assetImporter.ImpoterTypeName)
        /// or
        /// Unable to instantiate importer [{0}].ToFormat(assetImporter.ImpoterTypeName)
        /// </exception>
        public static void RegisterAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            lock (RegistryLock)
            {
                if (RegisteredAssemblies.Contains(assembly))
                {
                    return;
                }
                RegisteredAssemblies.Add(assembly);

                // Process Asset types.
                foreach (var type in assembly.GetTypes())
                {
                    // Register serializer factories
                    if (type.GetCustomAttribute<YamlSerializerFactoryAttribute>() != null)
                    {
                        if (typeof(IYamlSerializableFactory).IsAssignableFrom(type))
                        {
                            try
                            {
                                var yamlFactory = (IYamlSerializableFactory)Activator.CreateInstance(type);
                                RegisteredSerializerFactories.Add(yamlFactory);

                                // TODO: Handle IDataCustomVisitor on its own instead of relying on the coupling with IYamlSerializableFactory
                                var dataCustomVisitor = yamlFactory as IDataCustomVisitor;
                                if (dataCustomVisitor != null)
                                {
                                    RegisteredDataVisitNodes.Add(dataCustomVisitor);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Unable to instantiate serializer factory [{0}]", ex, type);
                            }
                        }
                    }

                    if (type.GetCustomAttribute<DiffNodeBuilderAttribute>() != null)
                    {
                        if (typeof(IDataCustomVisitor).IsAssignableFrom(type))
                        {
                            try
                            {
                                var dataCustomVisitor = (IDataCustomVisitor)Activator.CreateInstance(type);
                                RegisteredDataVisitNodeBuilders.Add(dataCustomVisitor);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Unable to instantiate diff converter [{0}]", ex, type);
                            }
                        }
                    }

                    // Asset importer
                    if (typeof(IAssetImporter).IsAssignableFrom(type) && type.GetConstructor(new Type[0]) != null)
                    {
                        try
                        {
                            var importerInstance = Activator.CreateInstance(type) as IAssetImporter;

                            // Register the importer instance
                            RegisterImporter(importerInstance);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Unable to instantiate importer [{0}]", ex, type.Name);
                        }
                    }

                    // Register asset factory
                    if (typeof(IAssetFactory<Asset>).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericTypeDefinition && type.GetConstructor(Type.EmptyTypes) != null)
                    {
                        var factory = (IAssetFactory<Asset>)Activator.CreateInstance(type);
                        RegisteredAssetFactories.Add(type.Name, factory);
                    }

                    if (typeof(PackageSessionAnalysisBase).IsAssignableFrom(type) && type.GetConstructor(new Type[0]) != null)
                    {
                        RegisteredPackageSessionAnalysisTypes.Add(type);
                    }

                    {
                        var packageUpgraderAttribute = type.GetCustomAttribute<PackageUpgraderAttribute>();
                        if (packageUpgraderAttribute != null)
                        {
                            try
                            {
                                var packageUpgrader = (PackageUpgrader)Activator.CreateInstance(type);
                                packageUpgrader.Attribute = packageUpgraderAttribute;
                                RegisteredPackageUpgraders[packageUpgraderAttribute.PackageName] = packageUpgrader;
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Unable to instantiate package upgrader [{0}]", ex, type.Name);
                            }
                        }
                    }

                    // Only process Asset types
                    var assetType = type;
                    if (!typeof(Asset).IsAssignableFrom(assetType) || !assetType.IsClass)
                    {
                        continue;
                    }

                    // Store in a list all asset types loaded
                    if (assetType.IsPublic && !assetType.IsAbstract)
                    {
                        AssetTypes.Add(assetType);
                    }

                    // Asset FileExtensions
                    var assetDescriptionAttribute = assetType.GetCustomAttribute<AssetDescriptionAttribute>();
                    if (assetDescriptionAttribute != null)
                    {
                        if (assetDescriptionAttribute.FileExtensions != null)
                        {
                            var extensions = FileUtility.GetFileExtensions(assetDescriptionAttribute.FileExtensions);
                            RegisteredDefaultAssetExtension[assetType] = extensions.FirstOrDefault();
                            foreach (var extension in extensions)
                            {
                                if (!RegisteredAssetFileExtensions.ContainsKey(extension))
                                {
                                    RegisteredAssetFileExtensions.Add(extension, assetType);
                                }
                            }
                        }

                        if (assetDescriptionAttribute.AlwaysMarkAsRoot)
                        {
                            lock (AlwaysMarkAsRootAssetTypes)
                            {
                                AlwaysMarkAsRootAssetTypes.Add(assetType);
                            }
                        }
                    }

                    // Asset format version (process name by name)
                    var assetFormatVersions = assetType.GetCustomAttributes<AssetFormatVersionAttribute>();
                    foreach (var assetFormatVersion in assetFormatVersions)
                    {
                        var formatVersion = assetFormatVersion.Version;
                        var minVersion = assetFormatVersion.MinUpgradableVersion;
                        SortedList<string, PackageVersion> formatVersions;
                        if (!RegisteredFormatVersions.TryGetValue(assetType, out formatVersions))
                        {
                            RegisteredFormatVersions.Add(assetType, formatVersions = new SortedList<string, PackageVersion>());
                        }
                        formatVersions.Add(assetFormatVersion.Name, formatVersion);

                        // Asset upgraders (only those matching current name)
                        var assetUpgraders = assetType.GetCustomAttributes<AssetUpgraderAttribute>().Where(x => x.Name == assetFormatVersion.Name);
                        AssetUpgraderCollection upgraderCollection = null;
                        foreach (var upgrader in assetUpgraders)
                        {
                            if (upgraderCollection == null)
                                upgraderCollection = new AssetUpgraderCollection(assetType, formatVersion);

                            upgraderCollection.RegisterUpgrader(upgrader.AssetUpgraderType, upgrader.StartVersion, upgrader.TargetVersion);
                        }
                        if (upgraderCollection != null)
                        {
                            upgraderCollection.Validate(minVersion);
                            RegisteredAssetUpgraders.Add(new KeyValuePair<Type, string>(assetType, assetFormatVersion.Name), upgraderCollection);
                        }
                    }

                    if (typeof(AssetComposite).IsAssignableFrom(assetType))
                    {
                        var attributes = assetType.GetCustomAttributes(typeof(AssetPartReferenceAttribute), true).Cast<AssetPartReferenceAttribute>().ToList();
                        foreach (var attribute in attributes)
                        {
                            HashSet<AssetPartReferenceAttribute> relatedPartTypes;
                            if (!RegisteredAssetCompositePartTypes.TryGetValue(attribute.ReferenceableType, out relatedPartTypes))
                            {
                                relatedPartTypes = new HashSet<AssetPartReferenceAttribute>();
                                RegisteredAssetCompositePartTypes.Add(attribute.ReferenceableType, relatedPartTypes);
                            }
                            attributes.ForEach(x => relatedPartTypes.Add(x));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Unregisters the specified assembly from the asset registry.
        /// </summary>
        /// <param name="assembly">A registered assembly.</param>
        public static void UnregisterAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            lock (RegistryLock)
            {
                if (!RegisteredAssemblies.Contains(assembly))
                {
                    return;
                }
                RegisteredAssemblies.Remove(assembly);


                foreach (var typeToRemove in RegisteredDefaultAssetExtension.Keys.ToList().Where(type => type.Assembly == assembly))
                {
                    RegisteredDefaultAssetExtension.Remove(typeToRemove);
                }

                foreach (var typeToRemove in AssetTypes.ToList().Where(type => type.Assembly == assembly))
                {
                    AssetTypes.Remove(typeToRemove);
                }

                foreach (var typeToRemove in RegisteredPackageSessionAnalysisTypes.ToList().Where(type => type.Assembly == assembly))
                {
                    RegisteredPackageSessionAnalysisTypes.Remove(typeToRemove);
                }

                foreach (var instance in RegisteredImportersInternal.ToList().Where(instance => instance.GetType().Assembly == assembly))
                {
                    RegisteredImportersInternal.Remove(instance);
                }

                foreach (var typeToRemove in RegisteredFormatVersions.Keys.ToList().Where(type => type.Assembly == assembly))
                {
                    RegisteredFormatVersions.Remove(typeToRemove);
                }

                foreach (var typeToRemove in RegisteredAssetUpgraders.Keys.ToList().Where(type => type.Key.Assembly == assembly))
                {
                    RegisteredAssetUpgraders.Remove(typeToRemove);
                }

                foreach (var extensionToRemove in RegisteredAssetFileExtensions.ToList().Where(keyValue => keyValue.Value.Assembly == assembly).Select(keyValue => keyValue.Key))
                {
                    RegisteredAssetFileExtensions.Remove(extensionToRemove);
                }

                foreach (var upgraderToRemove in RegisteredPackageUpgraders.ToList().Where(keyValue => keyValue.Value.GetType().Assembly == assembly).Select(keyValue => keyValue.Key))
                {
                    RegisteredPackageUpgraders.Remove(upgraderToRemove);
                }

                foreach (var instance in RegisteredSerializerFactories.ToList().Where(instance => instance.GetType().Assembly == assembly))
                {
                    RegisteredSerializerFactories.Remove(instance);
                }

                foreach (var instance in RegisteredDataVisitNodes.ToList().Where(instance => instance.GetType().Assembly == assembly))
                {
                    RegisteredDataVisitNodes.Remove(instance);
                }

                foreach (var instance in RegisteredDataVisitNodeBuilders.ToList().Where(instance => instance.GetType().Assembly == assembly))
                {
                    RegisteredDataVisitNodeBuilders.Remove(instance);
                }
            }
        }

        /// <summary>
        /// Check if the specified type is an asset.
        /// </summary>
        /// <param name="assetType">Type of the asset.</param>
        /// <param name="throwException">A boolean indicating whether this method should throw an exception if the type is not an asset type.</param>
        /// <returns><c>true</c> if the asset is an asset type, false otherwise.</returns>
        public static bool IsAssetType(Type assetType, bool throwException = false)
        {
            if (assetType == null)
                throw new ArgumentNullException(nameof(assetType));

            if (!typeof(Asset).IsAssignableFrom(assetType))
            {
                if (throwException)
                    throw new ArgumentException("Type [{0}] must be assignable to Asset".ToFormat(assetType), nameof(assetType));
                return false;
            }
            return true;
        }

        static AssetRegistry()
        {
            SupportedPlatforms = new SolutionPlatformCollection();
            // Statically find all assemblies related to assets and register them
            var assemblies = AssemblyRegistry.Find(AssemblyCommonCategories.Assets);
            foreach (var assembly in assemblies)
            {
                RegisterAssembly(assembly);
            }
            AssemblyRegistry.AssemblyRegistered += AssemblyRegistryAssemblyRegistered;
            AssemblyRegistry.AssemblyUnregistered += AssemblyRegistryOnAssemblyUnregistered;
        }

        private static void AssemblyRegistryOnAssemblyUnregistered(object sender, AssemblyRegisteredEventArgs e)
        {
            if (e.Categories.Contains(AssemblyCommonCategories.Assets))
            {
                UnregisterAssembly(e.Assembly);
            }
        }

        static void AssemblyRegistryAssemblyRegistered(object sender, AssemblyRegisteredEventArgs e)
        {
            // Handle delay-loading assemblies
            if (e.Categories.Contains(AssemblyCommonCategories.Assets))
            {
                RegisterAssembly(e.Assembly);
            }
        }
    }
}
