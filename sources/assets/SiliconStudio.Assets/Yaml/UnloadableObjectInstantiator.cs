// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Yaml.Events;

namespace SiliconStudio.Core.Yaml
{
    public static class UnloadableObjectInstantiator
    {
        private static Dictionary<Type, Type> proxyTypes = new Dictionary<Type, Type>();

        public delegate void ProcessProxyTypeDelegate(Type baseType, TypeBuilder typeBuilder);

        /// <summary>
        /// Callback to perform additional changes to the generated proxy object.
        /// </summary>
        public static ProcessProxyTypeDelegate ProcessProxyType;

        /// <summary>
        /// Creates an object that implements the given <paramref name="baseType"/> and <see cref="IUnloadable"/>.
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="typeName"></param>
        /// <param name="parsingEvents"></param>
        /// <returns></returns>
        public static IUnloadable CreateUnloadableObject(Type baseType, string typeName, string assemblyName, string error, List<ParsingEvent> parsingEvents)
        {
            Type proxyType;
            lock (proxyTypes)
            {
                if (!proxyTypes.TryGetValue(baseType, out proxyType))
                {
                    var asmName = new AssemblyName($"YamlProxy_{Guid.NewGuid():N}");

                    // Create assembly (in memory)
                    var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
                    var moduleBuilder = asmBuilder.DefineDynamicModule("DynamicModule");

                    // Create type
                    TypeBuilder typeBuilder = moduleBuilder.DefineType($"{baseType}YamlProxy");

                    // Add DisplayAttribute
                    var displayAttributeCtor = typeof(DisplayAttribute).GetConstructor(new Type[] { typeof(string), typeof(string) });
                    var displayAttribute = new CustomAttributeBuilder(displayAttributeCtor, new object[] { "Error: unable to load this object", null });
                    typeBuilder.SetCustomAttribute(displayAttribute);

                    // Add NonInstantiableAttribute
                    var nonInstantiableAttributeCtor = typeof(NonInstantiableAttribute).GetConstructor(Type.EmptyTypes);
                    var nonInstantiableAttribute = new CustomAttributeBuilder(nonInstantiableAttributeCtor, new object[0]);
                    typeBuilder.SetCustomAttribute(nonInstantiableAttribute);

                    // Inherit expected base type
                    if (baseType.IsInterface)
                        typeBuilder.AddInterfaceImplementation(baseType);
                    else
                        typeBuilder.SetParent(baseType);

                    // Implement IUnloadable
                    typeBuilder.AddInterfaceImplementation(typeof(IUnloadable));

                    var backingFields = new List<FieldBuilder>();
                    foreach (var property in new[] { new { Name = nameof(IUnloadable.TypeName), Type = typeof(string) }, new { Name = nameof(IUnloadable.AssemblyName), Type = typeof(string) }, new { Name = nameof(IUnloadable.Error), Type = typeof(string) }, new { Name = nameof(IUnloadable.ParsingEvents), Type = typeof(List<ParsingEvent>) } })
                    {
                        // Add backing field
                        var backingField = typeBuilder.DefineField($"{property.Name.ToLowerInvariant()}", property.Type, FieldAttributes.Private);
                        backingFields.Add(backingField);

                        // Create property
                        var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.Type, Type.EmptyTypes);

                        // Create getter method
                        var propertyGetter = typeBuilder.DefineMethod($"get_{property.Name}",
                            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, property.Type, Type.EmptyTypes);
                        var propertyGetterIL = propertyGetter.GetILGenerator();
                        propertyGetterIL.Emit(OpCodes.Ldarg_0);
                        propertyGetterIL.Emit(OpCodes.Ldfld, backingField);
                        propertyGetterIL.Emit(OpCodes.Ret);
                        propertyBuilder.SetGetMethod(propertyGetter);

                        // Add DataMemberIgnoreAttribute
                        var dataMemberIgnoreAttributeCtor = typeof(DataMemberIgnoreAttribute).GetConstructor(Type.EmptyTypes);
                        var dataMemberIgnoreAttribute = new CustomAttributeBuilder(dataMemberIgnoreAttributeCtor, new object[0]);
                        propertyBuilder.SetCustomAttribute(dataMemberIgnoreAttribute);
                    }

                    // .ctor (initialize backing fields too)
                    var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, backingFields.Select(x => x.FieldType).ToArray());
                    var ctorIL = ctor.GetILGenerator();
                    // Call parent ctor (if one without parameters exist)
                    var defaultCtor = baseType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                    if (defaultCtor != null)
                    {
                        ctorIL.Emit(OpCodes.Ldarg_0);
                        ctorIL.Emit(OpCodes.Call, defaultCtor);
                    }
                    // Initialize fields
                    for (int index = 0; index < backingFields.Count; index++)
                    {
                        var backingField = backingFields[index];
                        ctorIL.Emit(OpCodes.Ldarg_0);
                        ctorIL.Emit(OpCodes.Ldarg, index + 1);
                        ctorIL.Emit(OpCodes.Stfld, backingField);
                    }
                    ctorIL.Emit(OpCodes.Ret);

                    // Build list of class hierarchy (from deeper to closer)
                    var currentType = baseType;
                    var abstractBaseTypes = new List<Type>();
                    while (currentType != null)
                    {
                        abstractBaseTypes.Add(currentType);
                        currentType = currentType.BaseType;
                    }
                    abstractBaseTypes.Reverse();

                    // Check that all interfaces are implemented
                    var interfaceMethods = new List<MethodInfo>();
                    foreach (var @interface in baseType.GetInterfaces())
                    {
                        interfaceMethods.AddRange(@interface.GetMethods(BindingFlags.Public | BindingFlags.Instance));
                    }

                    // Build list of abstract methods
                    var abstractMethods = new List<MethodInfo>();
                    foreach (var currentBaseType in abstractBaseTypes)
                    {
                        foreach (var method in currentBaseType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                        {
                            if ((method.Attributes & MethodAttributes.Abstract) != 0)
                            {
                                // abstract: add it
                                abstractMethods.Add(method);
                            }
                            else if ((method.Attributes & MethodAttributes.Virtual) != 0 && (method.Attributes & MethodAttributes.NewSlot) == 0)
                            {
                                // override: check if it overrides a previously described abstract method
                                for (int index = 0; index < abstractMethods.Count; index++)
                                {
                                    var abstractMethod = abstractMethods[index];
                                    if (abstractMethod.Name == method.Name
                                        && CompareMethodSignature(abstractMethod, method))
                                    {
                                        // Found a match, let's remove it from list of method to reimplement
                                        abstractMethods.RemoveAt(index);
                                        break;
                                    }
                                }
                            }

                            // Remove interface methods already implemented
                            // override: check if it overrides a previously described abstract method
                            for (int index = 0; index < interfaceMethods.Count; index++)
                            {
                                var interfaceMethod = interfaceMethods[index];
                                if (interfaceMethod.Name == method.Name
                                    && CompareMethodSignature(interfaceMethod, method))
                                {
                                    // Found a match, let's remove it from list of method to reimplement
                                    interfaceMethods.RemoveAt(index--);
                                }
                            }
                        }
                    }

                    // Note: It seems that C# also creates a Property/Event for each override; but it doesn't seem to fail when creating the type with only non-abstract getter/setter -- so we don't recreate the property/event
                    // Implement all abstract methods
                    foreach (var method in abstractMethods.Concat(interfaceMethods))
                    {
                        // Updates MethodAttributes for override method
                        var attributes = method.Attributes;
                        attributes &= ~MethodAttributes.Abstract;
                        attributes &= ~MethodAttributes.NewSlot;
                        attributes |= MethodAttributes.HideBySig;

                        var overrideMethod = typeBuilder.DefineMethod(method.Name, attributes, method.CallingConvention, method.ReturnType, method.GetParameters().Select(x => x.ParameterType).ToArray());
                        var overrideMethodIL = overrideMethod.GetILGenerator();

                        // TODO: For properties, do we want get { return default(T); } set { } instead?
                        //       And for events, add { } remove { } too?
                        overrideMethodIL.ThrowException(typeof(NotImplementedException));
                    }

                    // User-registered callbacks
                    ProcessProxyType?.Invoke(baseType, typeBuilder);

                    proxyType = typeBuilder.CreateType();
                    proxyTypes.Add(baseType, proxyType);
                }
            }

            return (IUnloadable)Activator.CreateInstance(proxyType, typeName, assemblyName, error, parsingEvents);
        }

        private static bool CompareMethodSignature(MethodInfo method1, MethodInfo method2)
        {
            var parameters1 = method1.GetParameters();
            var parameters2 = method2.GetParameters();

            if (parameters1.Length != parameters2.Length)
                return false;

            for (int i = 0; i < parameters1.Length; ++i)
            {
                if (parameters1[i].ParameterType != parameters2[i].ParameterType)
                    return false;
            }

            return true;
        }
    }
}
