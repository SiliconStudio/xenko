// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace SiliconStudio.Presentation.Extensions
{
    public static class ClassFieldExtensions
    {
        public static Func<TInstance, TValue> GetFieldAccessor<TInstance, TValue>(string fieldName)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            MemberExpression member = Expression.Field(instanceParam, fieldName);
            LambdaExpression lambda = Expression.Lambda(typeof(Func<TInstance, TValue>), member, instanceParam);

            return (Func<TInstance, TValue>)lambda.Compile();
        }

        public static Func<object, object> GetFieldAccessor(string fieldName, Type instanceType, Type valueType)
        {
            ParameterExpression instanceParam = Expression.Parameter(instanceType, "instance");
            MemberExpression member = Expression.Field(instanceParam, fieldName);
            LambdaExpression lambda = Expression.Lambda(typeof(Func<object, object>), member, instanceParam);

            return (Func<object, object>)lambda.Compile();
        }

        public static Action<TInstance, TValue> SetFieldAccessor<TInstance, TValue>(string fieldName)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");
            MemberExpression member = Expression.Field(instanceParam, fieldName);
            BinaryExpression assign = Expression.Assign(member, valueParam);
            LambdaExpression lambda = Expression.Lambda(typeof(Action<TInstance, TValue>), assign, instanceParam, valueParam);

            return (Action<TInstance, TValue>)lambda.Compile();
        }

        public static Action<object, object> SetFieldAccessor(string fieldName, Type instanceType, Type valueType)
        {
            ParameterExpression instanceParam = Expression.Parameter(instanceType, "instance");
            ParameterExpression valueParam = Expression.Parameter(valueType, "value");
            MemberExpression member = Expression.Field(instanceParam, fieldName);
            BinaryExpression assign = Expression.Assign(member, valueParam);
            LambdaExpression lambda = Expression.Lambda(typeof(Action<object, object>), assign, instanceParam, valueParam);

            return (Action<object, object>)lambda.Compile();
        }
    }
}
