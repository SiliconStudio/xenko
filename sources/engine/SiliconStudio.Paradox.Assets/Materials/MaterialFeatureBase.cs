// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using System.Reflection;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Base class for <see cref="IMaterialFeature"/>.
    /// </summary>
    /// <remarks>
    /// This base class automatically iterates on properties to generate the shader
    /// </remarks>
    public abstract class MaterialFeatureBase : IMaterialFeature
    {
        public virtual void GenerateShader(MaterialShaderGeneratorContext context)
        {
            var typeDescriptor = TypeDescriptorFactory.Default.Find(this.GetType());

            foreach (var member in typeDescriptor.Members.OfType<MemberDescriptorBase>())
            {
                var memberValue = member.Get(this);

                // TODO: Should we log an error if a property/field is not supported?
                // TODO: Handle list/collection of IMaterialFeature?

                var memberShaderGen = memberValue as IMaterialFeature;
                if (memberShaderGen != null)
                {
                    memberShaderGen.GenerateShader(context);
                }
                else
                {
                    var materialStreamAttribute = member.MemberInfo.GetCustomAttribute<MaterialStreamAttribute>();
                    if (materialStreamAttribute != null)
                    {
                        if (string.IsNullOrWhiteSpace(materialStreamAttribute.Stream))
                        {
                            context.Log.Error("Material stream cannot be null for member [{0}.{1}]", member.DeclaringType, member.MemberInfo.Name);
                            continue;
                        }

                        var materialNode = memberValue as IMaterialComputeColor;
                        if (materialNode != null)
                        {
                            var classSource = materialNode.GenerateShaderSource(context);
                            switch (materialStreamAttribute.Type)
                            {
                                case MaterialStreamType.Float3:
                                    context.CurrentStack.AddBlendColor3(materialStreamAttribute.Stream, classSource);
                                    break;

                                case MaterialStreamType.Float:
                                    context.CurrentStack.AddBlendColor(materialStreamAttribute.Stream, classSource);
                                    break;
                            }
                        }
                        else
                        {
                            context.Log.Error("Error in [{0}.{1}] support only IMaterialNode instead of [{2}]", member.DeclaringType, member.MemberInfo.Name, member.Type);
                        }
                    }
                }
            }
        }
    }
}