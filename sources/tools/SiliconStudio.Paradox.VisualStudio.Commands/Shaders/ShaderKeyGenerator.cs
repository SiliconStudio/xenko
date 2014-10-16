// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Paradox.Shaders.Parser.Mixins;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Paradox.VisualStudio.Commands.Shaders
{
    public class ShaderKeyGenerator : ShaderKeyGeneratorBase
    {
        private ShaderClassType shaderClassType;
        
        public ShaderKeyGenerator(ShaderClassType shader)
        {
            // Register SiliconStudio.Paradox.Assets and SiliconStudio.Engine assemblies
            RuntimeHelpers.RunModuleConstructor(typeof(SiliconStudio.Paradox.Effects.Modules.MaterialKeys).Module.ModuleHandle);
            RuntimeHelpers.RunModuleConstructor(typeof(SiliconStudio.Paradox.Assets.SpriteFont.SpriteFontAsset).Module.ModuleHandle);
            RuntimeHelpers.RunModuleConstructor(typeof(SiliconStudio.Paradox.Engine.Data.ModelComponentData).Module.ModuleHandle);
            RuntimeHelpers.RunModuleConstructor(typeof(SiliconStudio.Paradox.Assets.Materials.MaterialDescription).Module.ModuleHandle);
            RuntimeHelpers.RunModuleConstructor(typeof(SiliconStudio.Core.Mathematics.Color4).Module.ModuleHandle);

            shaderClassType = shader;
        }

        public override bool Run()
        {
            WriteLine("// AUTO-GENERATED, DO NOT MODIFY!");
            WriteLine("using System;");
            WriteLine("using SiliconStudio.Paradox.Effects;");
            WriteLine("using SiliconStudio.Paradox.Graphics;");
            WriteLine("using SiliconStudio.Core.Mathematics;");
            WriteLine("using Buffer = SiliconStudio.Paradox.Graphics.Buffer;");
            WriteLine();
            Write("namespace SiliconStudio.Paradox.Effects.Modules");
            OpenBrace();

            Visit(shaderClassType);
            
            CloseBrace();

            return true;
        }

        [Visit]
        protected virtual void Visit(ShaderClassType shader)
        {
            Write("public static partial class ");
            Write(shader.Name);
            Write("Keys");
            OpenBrace();
            foreach (var decl in shader.Members.OfType<Variable>())
            {
                VisitDynamic(decl);
            }
            CloseBrace();
        }

        [Visit]
        protected virtual void Visit(GenericType<ObjectType> type)
        {
            if (IsStringInList(type.Name, "StructuredBuffer", "RWStructuredBuffer", "ConsumeStructuredBuffer", "AppendStructuredBuffer"))
            {
                Write("Buffer");
            }
            ProcessInitialValueStatus = false;
        }

        protected override bool IsKeyType(TypeBase type)
        {
            return base.IsKeyType(type)
                   || (type is GenericType<ObjectType> && IsStringInList(type.Name, "StructuredBuffer", "RWStructuredBuffer", "ConsumeStructuredBuffer", "AppendStructuredBuffer"));
        }
    }
}
