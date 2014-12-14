// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

using SiliconStudio.Assets;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Effects.Data;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public interface IMaterialAttribute
    {
    }

    public interface IMaterialGeometryAttribute : IMaterialAttribute
    {
        // TODO: Add tesselation...etc.
    }

    public interface IMaterialDiffuseAttribute : IMaterialAttribute
    {
    }

    public interface IMaterialMicroSurfaceAttribute : IMaterialAttribute
    {
    }

    public class MaterialSmoothnessAttribute : IMaterialMicroSurfaceAttribute
    {
        public IMaterialNode SmoothnessMap;
    }

    public class MaterialDiffuseMapAttribute : IMaterialDiffuseAttribute
    {
        public IMaterialNode DiffuseMap;
    }

    public interface IMaterialSpecularAttribute : IMaterialAttribute
    {
    }

    public class MaterialSpecularMapAttribute : IMaterialSpecularAttribute
    {
        public IMaterialNode SpecularMap;

        public IMaterialNode Intensity;

        public IMaterialNode Fresnel;
    }

    public class MaterialMetalnessMapAttribute : IMaterialSpecularAttribute
    {
        public IMaterialNode MetalnessMap;
    }

    public interface IMaterialDiffuseModelAttribute : IMaterialAttribute
    {
    }

    public class MaterialDiffuseLambertianModelAttribute : IMaterialDiffuseModelAttribute
    {
    }

    public interface IMaterialSpecularModelAttribute : IMaterialAttribute
    {
    }

    public interface IMaterialSurfaceAttribute : IMaterialAttribute
    {
    }

    public interface IMaterialOcclusionAttribute : IMaterialAttribute
    {
    }

    public class MaterialNormalMapAttribute : IMaterialSurfaceAttribute
    {
        public IMaterialNode NormalMap;
    }

    public class MaterialBlendLayer
    {
        public MaterialBlendLayer()
        {
            Enabled = true;
        }

        public bool Enabled { get; set; }

        public string Name { get; set; }

        public AssetReference<MaterialAsset2> Material { get; set; }

        public IMaterialNode BlendMap { get; set; }

        public MaterialBlendOverrides Overrides { get; set; }
    }

    public class MaterialBlendOverrides
    {
        public MaterialBlendOverrides()
        {
            SurfaceContribution = 1.0f;
            MicroSurfaceContribution = 1.0f;
            DiffuseContribution = 1.0f;
            SpecularContribution = 1.0f;
            OcclusionContribution = 1.0f;
            OffsetU = 0.0f;
            OffsetV = 0.0f;
            ScaleU = 1.0f;
            ScaleV = 1.0f;
        }

        [DefaultValue(1.0f)]
        public float SurfaceContribution { get; set; }

        [DefaultValue(1.0f)]
        public float MicroSurfaceContribution { get; set; }

        [DefaultValue(1.0f)]
        public float DiffuseContribution { get; set; }

        [DefaultValue(1.0f)]
        public float SpecularContribution { get; set; }

        [DefaultValue(1.0f)]
        public float OcclusionContribution { get; set; }

        [DefaultValue(0.0f)]
        public float OffsetU { get; set; }

        [DefaultValue(0.0f)]
        public float OffsetV { get; set; }

        [DefaultValue(1.0f)]
        public float ScaleU { get; set; }

        [DefaultValue(1.0f)]
        public float ScaleV { get; set; }
    }

    public class MaterialBlendLayerStack : List<MaterialBlendLayer>, IMaterialComposition
    {
    }

    public interface IMaterialDescriptor
    {
    }

    public interface IMaterialComposition : IMaterialDescriptor
    {
    }

    public class MaterialAttributes : IMaterialDescriptor
    {
        public IMaterialGeometryAttribute Geometry { get; set; }

        public IMaterialSurfaceAttribute Surface { get; set; }

        public IMaterialMicroSurfaceAttribute MicroSurface { get; set; }

        public IMaterialDiffuseAttribute Diffuse { get; set; }

        public IMaterialDiffuseModelAttribute DiffuseModel { get; set; }

        public IMaterialSpecularAttribute Specular { get; set; }

        public IMaterialSpecularModelAttribute SpecularModel { get; set; }

        public IMaterialOcclusionAttribute Occlusion { get; set; }

        // TODO: Add Emissive, Transparency attributes...etc.
    }

    public class MaterialAsset2 : Asset
    {
        public IMaterialDescriptor Descriptor { get; set; }

        public ParameterCollectionData Parameters { get; set; }
    }
}