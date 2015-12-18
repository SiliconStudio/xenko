// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SiliconStudio.Shaders.Utility;
using LinqExpression = System.Linq.Expressions.Expression;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// Provides a dictionary of cloned values, where the [key] is the original object 
    /// and [value] the new object cloned associated to the original object.
    /// </summary>
    public class CloneContext : Dictionary<object, object>
    {
        /// <summary>
        /// Internal cache of delegates used on calling DeepClone.
        /// It avoids to use directly the ThreadStatic field in <see cref="DeepCloner"/>
        /// </summary>
        internal Dictionary<DeepCloner.CacheKey, Delegate> Cache;

        internal bool IsSelfClone;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloneContext"/> class.
        /// </summary>
        public CloneContext()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloneContext"/> class.
        /// </summary>
        /// <param name="parent">The parent context use for chaining several clone context.</param>
        public CloneContext(CloneContext parent) : base(new ReferenceEqualityComparer<object>())
        {
            Parent = parent;
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public CloneContext Parent { get; private set; }

        public new bool ContainsKey(object key)
        {
            return base.ContainsKey(key) || (Parent != null && Parent.ContainsKey(key));
        }

        public new bool TryGetValue(object key, out object value)
        {
            return base.TryGetValue(key, out value) || (Parent != null && Parent.TryGetValue(key, out value));
        }

        public new object this[object key]
        {
            get
            {
                return base[key] ?? (Parent != null ? Parent[key] : null);
            }

            set
            {
                base[key] = value;
            }
        }
    }
    
    /// <summary>
    /// DeepClone extension.
    /// </summary>
    public static class DeepCloner
    {
        #region Constants and Fields

        [ThreadStatic]
        private static Dictionary<CacheKey, Delegate> deepcache;

        [ThreadStatic]
        private static Dictionary<CacheKey, Delegate> deepCacheSelf;

        private static readonly MethodInfo DeepcloneArray = MethodReflector(() => DeepClone<object>(new object[0], out tempObjects, null));

        private static readonly MethodInfo DeepcloneObject = MethodReflector(() => DeepClone(ref tempObject, out tempObject, null));

        private static readonly MethodInfo MethodAdd = typeof(Dictionary<object, object>).GetTypeInfo().GetDeclaredMethod("Add");

        private static readonly MethodInfo MethodTryGetValue = typeof(CloneContext).GetTypeInfo().GetDeclaredMethod("TryGetValue");

        private static object tempObject = new object();

        private static object[] tempObjects = new object[0];

        #endregion

        #region Delegates

        private delegate void CloneDelegate<T>(ref T toClone, out T output, CloneContext context);

        #endregion

        #region Public Methods

        private static Dictionary<CacheKey, Delegate> GetCache(CloneContext context)
        {
            if (context.IsSelfClone)
            {
                return deepCacheSelf ?? (deepCacheSelf = new Dictionary<CacheKey, Delegate>());
            }

            return deepcache ?? (deepcache = new Dictionary<CacheKey, Delegate>());
        }

        /// <summary>
        /// Clones deeply the specified object. See remarks.
        /// </summary>
        /// <typeparam name="T">Type of the object to clone</typeparam>
        /// <param name="obj">The object instance to clone recursively.</param>
        /// <param name="context">The context dictionary to use while cloning this object.</param>
        /// <returns>A cloned instance of the object</returns>
        /// <remarks>
        /// In order to be cloneable, all objects and sub-objects are required to provide a public parameter-less constructor.
        /// </remarks>
        public static T DeepClone<T>(this T obj, CloneContext context = null)
        {
            if (ReferenceEquals(obj, null))
                return default(T);

            // Create a new context if no context was given
            if (context == null)
                context = new CloneContext();

            context.Cache = GetCache(context);
            T result;
            DeepClone(ref obj, out result, context);
            return result;
        }

        /// <summary>
        /// Recursively collects all object references and fill the context with [key] object => [value] object (self referencing).
        /// </summary>
        /// <typeparam name="T">Type of the object to collect all object references</typeparam>
        /// <param name="obj">The object instance to collect object references recursively.</param>
        /// <param name="context">The context dictionary to fill with collected object references while cloning this object.</param>
        /// <exception cref="System.ArgumentNullException">context</exception>
        public static void DeepCollect<T>(T obj, CloneContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            // Just run the DeepClone in IsSelfClone mode.
            context.IsSelfClone = true;
            DeepClone(obj, context);
            context.IsSelfClone = false;
        }

        #endregion

        #region Methods

        private static void DeepClone<T>(ref T obj, out T dest, CloneContext context)
        {
            if (ReferenceEquals(obj, null))
                dest = default(T);
            else
            {
                var t = obj.GetType();
                if (IsPrimitive(t) || obj is Type)
                {
                    dest = obj;
                }
                else if (typeof(T) == t && t.GetTypeInfo().IsValueType)
                {
                    GetObjectCloner<T>(typeof(T), context)(ref obj, out dest, context);
                }
                else
                {
                    object localDest = null;
                    if (!context.TryGetValue(obj, out localDest))
                    {
                        GetObjectCloner<T>(t, context)(ref obj, out dest, context);
                    }
                    else
                    {
                        dest = (T)localDest;
                    }
                }
            }
        }

        private static void DeepClone<T>(T[] obj, out T[] dest, CloneContext context)
        {
            if (ReferenceEquals(obj, null))
                dest = null;
            else
            {
                object arrayObj;
                if (context.TryGetValue(obj, out arrayObj))
                    dest = (T[])arrayObj;
                else
                {
                    var t = typeof(T);
                    if (IsPrimitive(t) || t == typeof(Type))
                    {
                        dest = context.IsSelfClone ? obj : (T[])obj.Clone();
                    }
                    else
                    {
                        dest = context.IsSelfClone ? obj : new T[obj.Length];
                        context.Add(obj, dest);
                        for (var i = 0; i < dest.Length; i++)
                            DeepClone(ref obj[i], out dest[i], context);
                    }
                }
            }
        }

        private static List<FieldInfo> GetFields(Type type)
        {
            var fields = new List<FieldInfo>();
            var t = type;
            while (t != null)
            {
                fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                t = t.GetTypeInfo().BaseType;
            }

            return fields;
        }

        private static CloneDelegate<T> GetObjectCloner<T>(Type type, CloneContext context)
        {
            Delegate result;
            var cache = context.Cache;

            var key = new CacheKey(typeof(T), type);
            if (!cache.TryGetValue(key, out result))
            {
                var statements = new List<LinqExpression>();

                var param = LinqExpression.Parameter(typeof(T).MakeByRefType(), "input");
                var output = LinqExpression.Parameter(typeof(T).MakeByRefType(), "output");
                var contextParam = LinqExpression.Parameter(typeof(CloneContext), "context");

                ParameterExpression localCast;
                ParameterExpression localOutput;
                ParameterExpression localObj;
                bool isStruct = type.GetTypeInfo().IsValueType && typeof(T) == type;
                bool isArray = type.IsArray;

                if (isStruct || isArray)
                {
                    localCast = param;
                    localOutput = output;
                    localObj = null;
                }
                else
                {
                    localObj = LinqExpression.Variable(typeof(object), "localObj");
                    localCast = LinqExpression.Variable(type, "localCast");
                    localOutput = LinqExpression.Variable(type, "localOut");

                    statements.Add(LinqExpression.Assign(localCast, LinqExpression.Convert(param, type)));

                    if (context.IsSelfClone)
                    {
                        statements.Add(LinqExpression.Call(contextParam, MethodAdd, param, param));
                        statements.Add(LinqExpression.Assign(output, localCast));
                    }
                    else
                    {
                        // NOTE: this can fail if there is no default constructor for type.
                        // for example Dictionary<K,T>.ValueCollection, which is created when a access to members Keys or Values is performed on the dicitonary.
                        statements.Add(LinqExpression.Assign(localOutput, LinqExpression.New(type)));


                        statements.Add(LinqExpression.Assign(output, localOutput));
                        statements.Add(LinqExpression.Call(contextParam, MethodAdd, param, localOutput));
                    }
                }

                var fields = GetFields(type);

                // If we have a struct with primitive only
                if (isArray)
                {
                    var genericCloneMethod = DeepcloneArray.GetGenericMethodDefinition();
                    genericCloneMethod = genericCloneMethod.MakeGenericMethod(new[] { type.GetElementType() });

                    statements.Add(LinqExpression.Call(genericCloneMethod, localCast, localOutput, contextParam));
                }
                else if (isStruct && fields.All(field => IsPrimitive(field.FieldType)))
                {
                    statements.Add(LinqExpression.Assign(localOutput, param));
                }
                else
                {
                    foreach (var field in fields)
                    {
                        if (field.IsInitOnly)
                            throw new InvalidOperationException(String.Format("Field [{0}] in [{1}] is readonly, which is not supported in DeepClone", field.Name, type));

                        var t = field.FieldType;
                        if (t.GetTypeInfo().IsSubclassOf(typeof(Delegate)))
                            continue;

                        var value = LinqExpression.Field(localCast, field);

                        LinqExpression cloneField = null;
                        if (IsPrimitive(t))
                        {
                            if (!context.IsSelfClone)
                            {
                                cloneField = LinqExpression.Assign(LinqExpression.Field(localOutput, field), value);
                            }
                        }
                        else
                        {
                            MethodInfo genericCloneMethod;
                            if (field.FieldType.IsArray)
                            {
                                genericCloneMethod = DeepcloneArray.GetGenericMethodDefinition();
                                genericCloneMethod = genericCloneMethod.MakeGenericMethod(new[] {field.FieldType.GetElementType()});
                            }
                            else
                            {
                                genericCloneMethod = DeepcloneObject.GetGenericMethodDefinition();
                                genericCloneMethod = genericCloneMethod.MakeGenericMethod(new[] {field.FieldType});
                            }

                            cloneField = LinqExpression.Call(genericCloneMethod, value, context.IsSelfClone ? value : LinqExpression.Field(localOutput, field), contextParam);
                        }

                        if (cloneField != null)
                        {
                            statements.Add(cloneField);
                        }
                    }
                }

                LinqExpression body;
                if (isStruct || isArray)
                {
                    if (statements.Count == 0)
                    {
                        body = LinqExpression.Empty();
                    }
                    else
                    {
                        body = LinqExpression.Block(statements);
                    }
                }
                else
                {
                    var innerBody = LinqExpression.Block(new[] { localCast, localOutput }, statements);

                    body = LinqExpression.Block(
                        new[] { localObj },
                        LinqExpression.IfThenElse(
                            LinqExpression.Call(contextParam, MethodTryGetValue, param, localObj), LinqExpression.Assign(output, LinqExpression.Convert(localObj, typeof(T))), innerBody));
                }

                var tempLambda = LinqExpression.Lambda<CloneDelegate<T>>(body, param, output, contextParam);
                result = tempLambda.Compile();
                cache.Add(key, result);
            }

            return (CloneDelegate<T>)result;
        }

        private static bool IsPrimitive(Type type)
        {
            return type.GetTypeInfo().IsPrimitive || type.GetTypeInfo().IsEnum || type == typeof(string);
        }

        private static MethodInfo MethodReflector(Expression<Action> access)
        {
            return ((MethodCallExpression)access.Body).Method;
        }

        internal struct CacheKey : IEquatable<CacheKey>
        {
            private readonly Type type;
            private readonly Type realType;

            public CacheKey(Type type, Type realType)
                : this()
            {
                this.type = type;
                this.realType = realType;
            }

            public bool Equals(CacheKey other)
            {
                return type == other.type && realType == other.realType;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is CacheKey && Equals((CacheKey)obj);
            }

            public override int GetHashCode()
            {
                return (type.GetHashCode() * 397) ^ realType.GetHashCode();
            }
        }


        #endregion
    }
}