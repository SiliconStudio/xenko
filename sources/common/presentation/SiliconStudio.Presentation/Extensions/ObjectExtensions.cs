// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace SiliconStudio.Presentation.Extensions
{
    public static class ObjectExtensions
    {
        private static readonly Dictionary<Type, Delegate> CachedMemberwiseCloneMethods = new Dictionary<Type, Delegate>();

        public static object MemberwiseClone(this object instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            Delegate method;

            Type instanceType = instance.GetType();

            if (CachedMemberwiseCloneMethods.TryGetValue(instanceType, out method) == false)
            {
                DynamicMethod dynamicMethod = GenerateDynamicMethod(instanceType);

                Type methodType = typeof(Func<,>).MakeGenericType(instanceType, instanceType);
                method = dynamicMethod.CreateDelegate(methodType);

                CachedMemberwiseCloneMethods.Add(instanceType, method);
            }

            return method.DynamicInvoke(instance);
        }

        public static T MemberwiseClone<T>(this T instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            Delegate method = null;

            Type instanceType = typeof(T);

            if (CachedMemberwiseCloneMethods.TryGetValue(instanceType, out method) == false)
            {
                DynamicMethod dynamicMethod = GenerateDynamicMethod(instanceType);

                method = dynamicMethod.CreateDelegate(typeof(Func<T, T>));

                CachedMemberwiseCloneMethods.Add(typeof(T), method);
            }

            return ((Func<T, T>)method)(instance);
        }

        private static DynamicMethod GenerateDynamicMethod(Type instanceType)
        {
            DynamicMethod dymMethod = new DynamicMethod("DynamicCloneMethod", instanceType, new Type[] { instanceType }, true);

            ILGenerator generator = dymMethod.GetILGenerator();

            generator.DeclareLocal(instanceType);

            bool isValueType = instanceType.IsValueType;

            if (isValueType)
            {
                generator.Emit(OpCodes.Ldloca, 0);
                generator.Emit(OpCodes.Initobj, instanceType);
            }
            else
            {
                ConstructorInfo constructorInfo = instanceType.GetConstructor(new Type[0]);
                generator.Emit(OpCodes.Newobj, constructorInfo);
                generator.Emit(OpCodes.Stloc_0);
            }

            foreach (FieldInfo field in instanceType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // Load the new object on the eval stack... (currently 1 item on eval stack)
                if (isValueType)
                    generator.Emit(OpCodes.Ldloca, 0);
                else
                    generator.Emit(OpCodes.Ldloc_0);
                // Load initial object (parameter)          (currently 2 items on eval stack)
                generator.Emit(OpCodes.Ldarg_0);
                // Replace value by field value             (still currently 2 items on eval stack)
                generator.Emit(OpCodes.Ldfld, field);
                // Store the value of the top on the eval stack into the object underneath that value on the value stack.
                //  (0 items on eval stack)
                generator.Emit(OpCodes.Stfld, field);
            }

            // Load new constructed obj on eval stack -> 1 item on stack
            generator.Emit(OpCodes.Ldloc_0);
            // Return constructed object.   --> 0 items on stack
            generator.Emit(OpCodes.Ret);

            return dymMethod;
        }
    }
}
