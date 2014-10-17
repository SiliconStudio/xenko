// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Paradox.Shaders.Parser.Utility;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Utility;

namespace SiliconStudio.Paradox.Shaders.Parser.Mixins
{
    internal class ShaderVirtualTable
    {
        #region Public member

        public Dictionary<string, MethodDeclaration[]> VirtualTableGroup = new Dictionary<string, MethodDeclaration[]>();
        
        #endregion

        #region Constructor

        public ShaderVirtualTable() {}

        #endregion

        #region Public methods

        /// <summary>
        /// Adds the virtual table of the mixin
        /// </summary>
        /// <param name="shaderVirtualTable"></param>
        /// <param name="className"></param>
        /// <param name="errorLogger"></param>
        public void AddVirtualTable(ShaderVirtualTable shaderVirtualTable, string className, LoggerResult errorLogger)
        {
            var newVT = shaderVirtualTable.VirtualTableGroup[className].ToArray();
            VirtualTableGroup.Add(className, newVT);

            foreach (var methodDecl in newVT)
                ReplaceVirtualMethod(methodDecl, errorLogger);
        }

        /// <summary>
        /// Replace the method occurence with its last definition
        /// </summary>
        /// <param name="methodDeclaration">the overriding method</param>
        /// <param name="errorLogger"></param>
        public void ReplaceVirtualMethod(MethodDeclaration methodDeclaration, LoggerResult errorLogger)
        {
            var baseDeclarationMixin = (string)methodDeclaration.GetTag(ParadoxTags.BaseDeclarationMixin);
            foreach (var dict in VirtualTableGroup.Select(x => x.Value))
            {
                for (int i = 0; i < dict.Length; ++i)
                {
                    var method = dict[i];
                    var originalDecl = (string)method.GetTag(ParadoxTags.BaseDeclarationMixin);

                    // TODO: take typedefs into account...
                    if (originalDecl == baseDeclarationMixin && method.IsSameSignature(methodDeclaration))
                    {
                        if (method.Qualifiers.Contains(ParadoxStorageQualifier.Stage) && !methodDeclaration.Qualifiers.Contains(ParadoxStorageQualifier.Stage))
                        {
                            errorLogger.Warning(ParadoxMessageCode.WarningMissingStageKeyword, methodDeclaration.Span, methodDeclaration, (methodDeclaration.GetTag(ParadoxTags.ShaderScope) as ModuleMixin).MixinName);
                            methodDeclaration.Qualifiers |= ParadoxStorageQualifier.Stage;
                        }
                        else if (!method.Qualifiers.Contains(ParadoxStorageQualifier.Stage) && methodDeclaration.Qualifiers.Contains(ParadoxStorageQualifier.Stage))
                        {
                            errorLogger.Error(ParadoxMessageCode.ErrorExtraStageKeyword, methodDeclaration.Span, methodDeclaration, method, (methodDeclaration.GetTag(ParadoxTags.ShaderScope) as ModuleMixin).MixinName);
                            methodDeclaration.Qualifiers.Values.Remove(ParadoxStorageQualifier.Stage);
                        }

                        dict[i] = methodDeclaration;
                    }
                }
            }
        }

        /// <summary>
        /// Adds the methods defined in the final mixin
        /// </summary>
        /// <param name="methodDeclarations">a list of MethodDeclaration</param>
        /// <param name="className">the name of the class</param>
        /// <param name="errorLogger">the logger for errors and warnings</param>
        public void AddFinalDeclarations(List<MethodDeclaration> methodDeclarations, string className, LoggerResult errorLogger)
        {
            var finalDict = new MethodDeclaration[methodDeclarations.Count];
            foreach (var methodDecl in methodDeclarations)
            {
                var vtableReference = (VTableReference)methodDecl.GetTag(ParadoxTags.VirtualTableReference);
                finalDict[vtableReference.Slot] = methodDecl;

                // TODO: override/abstract behavior
                //if (methodDecl.Qualifiers.Contains(ParadoxStorageQualifier.Override))
                    LookForBaseDeclarationMixin(methodDecl, errorLogger);
            }

            VirtualTableGroup.Add(className, finalDict);
        }

        /// <summary>
        /// Finds the location of the method in the virtual table of its definition mixin
        /// </summary>
        /// <param name="methodDeclaration"></param>
        /// <returns></returns>
        public VTableReference GetBaseDeclaration(MethodDeclaration methodDeclaration)
        {
            var baseMethodDeclMixin = methodDeclaration.GetTag(ParadoxTags.BaseDeclarationMixin) as string;
            var slot = -1;
            var vt = VirtualTableGroup[baseMethodDeclMixin];
            for (int i = 0; i < vt.Length; ++i)
            {
                if (methodDeclaration.IsSameSignature(vt[i]))
                {
                    slot = i;
                    break;
                }
            }
            return new VTableReference { Shader = baseMethodDeclMixin, Slot = slot };
        }

        /// <summary>
        /// Returns the method at the specified location
        /// </summary>
        /// <param name="mixinName">the sub virtual table</param>
        /// <param name="slot">the slot index</param>
        /// <returns>the method in the specified location</returns>
        public MethodDeclaration GetMethod(string mixinName, int slot)
        {
            MethodDeclaration[] decls;
            if (VirtualTableGroup.TryGetValue(mixinName, out decls))
            {
                if (decls.Length > slot)
                    return VirtualTableGroup[mixinName][slot];
            }
            return null;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Find the base definition of the method and override its occurence
        /// </summary>
        /// <param name="methodDeclaration"></param>
        /// <param name="errorLogger"></param>
        private void LookForBaseDeclarationMixin(MethodDeclaration methodDeclaration, LoggerResult errorLogger)
        {
            foreach (var dict in VirtualTableGroup.Select(x => x.Value))
            {
                for (int i = 0; i < dict.Length; ++i)
                {
                    var method = dict[i];
                    var baseDeclarationMixin = (string)method.GetTag(ParadoxTags.BaseDeclarationMixin);

                    // TODO: take typedefs into account...
                    if (method.IsSameSignature(methodDeclaration))
                    {
                        var sourceShader = ((ModuleMixin)methodDeclaration.GetTag(ParadoxTags.ShaderScope)).MixinName;

                        // test override
                        if (methodDeclaration is MethodDefinition && method is MethodDefinition && !methodDeclaration.Qualifiers.Contains(ParadoxStorageQualifier.Override))
                            errorLogger.Error(ParadoxMessageCode.ErrorMissingOverride, method.Span, methodDeclaration, sourceShader);
                        if (!(methodDeclaration is MethodDefinition))
                            errorLogger.Error(ParadoxMessageCode.ErrorOverrindingDeclaration, method.Span, methodDeclaration, sourceShader);

                        if (method.Qualifiers.Contains(ParadoxStorageQualifier.Stage) && !methodDeclaration.Qualifiers.Contains(ParadoxStorageQualifier.Stage))
                        {
                            errorLogger.Warning(ParadoxMessageCode.WarningMissingStageKeyword, methodDeclaration.Span, methodDeclaration, (methodDeclaration.GetTag(ParadoxTags.ShaderScope) as ModuleMixin).MixinName);
                            methodDeclaration.Qualifiers |= ParadoxStorageQualifier.Stage;
                        }
                        else if (!method.Qualifiers.Contains(ParadoxStorageQualifier.Stage) && methodDeclaration.Qualifiers.Contains(ParadoxStorageQualifier.Stage))
                        {
                            errorLogger.Error(ParadoxMessageCode.ErrorExtraStageKeyword, methodDeclaration.Span, methodDeclaration, method, (methodDeclaration.GetTag(ParadoxTags.ShaderScope) as ModuleMixin).MixinName);
                            methodDeclaration.Qualifiers.Values.Remove(ParadoxStorageQualifier.Stage);
                        }

                        dict[i] = methodDeclaration;
                        methodDeclaration.SetTag(ParadoxTags.BaseDeclarationMixin, baseDeclarationMixin);
                    }
                }
            }
        }

        #endregion
    }
}
