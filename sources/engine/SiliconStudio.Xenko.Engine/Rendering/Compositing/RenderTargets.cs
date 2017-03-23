using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// Type type itself represents a semantic for a render target
    /// </summary>
    /// <remarks>Please implement stateless, so that objects can be recycled.</remarks>
    public interface IRenderTargetSemantic
    {
        string ShaderClass { get; }
    }

    public struct RenderTargetTextureCreationParams
    {
        public PixelFormat PixelFormat { get; set; }
        public TextureFlags TextureFlags { get; set; }
        public MSAALevel MSAALevel { get; set; }
    }

    public struct RenderTargetDesc
    {
        public IRenderTargetSemantic Semantic;
        public RenderTargetTextureCreationParams RenderTargetTextureParams;
    }

    public struct RenderTarget
    {
        public RenderTargetDesc Description;
        public Texture Texture { get; set; }
    }

    public interface IRenderTargetCompositionQueriable
    {
        string GetShaderClass(Type semanticType);
        Texture[] TexturesComposition { get; }
        ShaderSourceCollection MixinCollection { get; }
    }

    public enum SetPolicy
    {
        DefendSilentlyIfSemanticKeyNotFound,
        ThrowOnSemanticKeyNotFound
    }

    /// <summary>
    /// Number of renderings per frame.
    /// </summary>
    /// <remarks>It can represent split viewports, like VR eyes, or multiplayer...</remarks>
    public interface IMultipleRenderViews
    {
        int ViewsCount { get; set; }

        int ViewsIndex { get; set; }
    }

    /// <summary>
    /// Represents how we setup the graphics pipeline output targets.
    /// </summary>
    public sealed class RenderTargetSetup : IRenderTargetCompositionQueriable, IMultipleRenderViews
    {
        private readonly FastList<RenderTarget> list = new FastList<RenderTarget>();
        // composition is a cache to communicate textures to the graphic device interface
        private readonly FastList<Texture> composition = new FastList<Texture>();

        private int FindIndexBySemantic(Type semanticType)
        {
            for (int i = 0; i < list.Count; ++i)
                if (list[i].Description.Semantic.GetType() == semanticType)
                    return i;
            return -1;
        }

        /// <summary>
        /// Resets the composition (unplug all targets).
        /// </summary>
        public void Clear()
        {
            list.Clear();
            composition.Clear();
        }

        public void AddTarget(RenderTarget target)
        {
            list.Add(target);
            if (list.Count - 1 != FindIndexBySemantic(target.Description.Semantic.GetType()))
                throw new InvalidOperationException("Uniquely inserted semantics invariant would be broken");
            // cache invalidation:
            composition.Clear();
        }

        public void Copy(RenderTargetSetup source)
        {
            Clear();
            foreach (var sourceTarget in source.List)
                AddTarget(sourceTarget);
            ViewsCount = source.ViewsCount;
            ViewsIndex = source.ViewsIndex;
        }

        /// <summary>
        /// Copy assigns new content to an existing render target.
        /// </summary>
        /// <param name="target">mutated target information</param>
        /// <remarks>It will locate the index by using the semantic of the parameter target</remarks>
        public void SetTarget(RenderTarget target, SetPolicy policy = SetPolicy.ThrowOnSemanticKeyNotFound)
        {
            if (target.Description.Semantic == null)
                throw new ArgumentNullException("Must fill-in the semantic for slot location");
            int index = FindIndexBySemantic(target.Description.Semantic.GetType());
            if (index == -1)
            {
                if (policy == SetPolicy.ThrowOnSemanticKeyNotFound)
                    throw new KeyNotFoundException("No such semantic found");
                return;
            }
            list[index] = target;
        }

        /// <summary>
        /// Shortcut to avoid copying the full render target struct to mutate just one field.
        /// </summary>
        /// <param name="semanticType">lookup key</param>
        public void SetTextureParams(Type semanticType, RenderTargetTextureCreationParams creationParams, SetPolicy policy = SetPolicy.ThrowOnSemanticKeyNotFound)
        {
            int index = FindIndexBySemantic(semanticType);
            if (index == -1)
            {
                if (policy == SetPolicy.ThrowOnSemanticKeyNotFound)
                    throw new KeyNotFoundException("No such semantic found");
                return;
            }
            list.Items[index].Description.RenderTargetTextureParams = creationParams;
        }

        /// <summary>
        /// Shortcut to avoid copying the full render target struct to mutate just one field.
        /// </summary>
        /// <param name="semanticType">lookup key</param>
        public void SetTexture(Type semanticType, Texture texture, SetPolicy policy = SetPolicy.ThrowOnSemanticKeyNotFound)
        {
            int index = FindIndexBySemantic(semanticType);
            if (index == -1)
            {
                if (policy == SetPolicy.ThrowOnSemanticKeyNotFound)
                    throw new KeyNotFoundException("No such semantic found");
                return;
            }
            list.Items[index].Texture = texture;
        }

        public IReadOnlyList<RenderTarget> List => list;

        /// <summary>
        /// Queries the mapping between the given semantic and the shader source class name.
        /// </summary>
        /// <param name="semanticType">type of the semantic you are looking for</param>
        /// <returns>string containing the mixin name</returns>
        public string GetShaderClass(Type semanticType)
        {
            return GetRenderTarget(semanticType).Description.Semantic?.ShaderClass;
        }

        public bool IsActive(Type semanticType)
        {
            return FindIndexBySemantic(semanticType) != -1;
        }

        /// <param name="semanticType">type of the semantic you are looking for</param>
        /// <returns>A render target with its full description. throws if not found.</returns>
        /// <remarks>Note that the returned type is a value type, so write access will not be reflected.</remarks>
        public RenderTarget GetRenderTarget(Type semanticType)
        {
            var foundAt = FindIndexBySemantic(semanticType);
            if (foundAt == -1)
                throw new KeyNotFoundException($"{semanticType}");
            return list[foundAt];
        }

        public int CompositionCount => list.Count;

        public Texture[] TexturesComposition
        {
            get
            {
                if (composition.Count == 0)
                {
                    foreach (var target in list)
                        composition.Add(target.Texture);
                }
                return composition.Items;
            }
        }

        private readonly Dictionary<int, ShaderSourceCollection> mrtExtensionsSourceCache = new Dictionary<int, ShaderSourceCollection>();

        private static int HashCombine(int hash1, int hash2)
        {
            return hash1 ^ (hash2 + 1327217884 + (hash1 << 6) + (hash1 >> 2));
        }

        public ShaderSourceCollection MixinCollection
        {
            get
            {
                int sourcesCompositionHash = 0;
                // start at slot 1, to mirror what XenkoMRT.xkfx expects.
                for (int rt = 1; rt < CompositionCount; ++rt)
                {
                    if (List[rt].Description.Semantic == null)
                        continue;
                    string classToMixin = List[rt].Description.Semantic.ShaderClass;
                    int nameHash = classToMixin.GetHashCode();
                    sourcesCompositionHash = HashCombine(sourcesCompositionHash, nameHash);
                }

                ShaderSourceCollection ssc;
                mrtExtensionsSourceCache.TryGetValue(sourcesCompositionHash, out ssc);
                if (ssc == null)
                {
                    ssc = new ShaderSourceCollection();
                    for (int rt = 1; rt < CompositionCount; ++rt)
                    {
                        string classToMixin = List[rt].Description.Semantic?.ShaderClass;
                        ssc.Add(classToMixin != null ? new ShaderClassSource(classToMixin) : null);
                    }
                    mrtExtensionsSourceCache.Add(sourcesCompositionHash, ssc);
                }
                return ssc;
            }
        }


        // represents the number of viewport renderings per frame
        public int ViewsCount { get; set; }
        public int ViewsIndex { get; set; }
    }
}
