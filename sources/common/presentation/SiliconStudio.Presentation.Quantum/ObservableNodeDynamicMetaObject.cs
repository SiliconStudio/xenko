// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SiliconStudio.Presentation.Quantum
{
    internal class ObservableNodeDynamicMetaObject : DynamicMetaObject
    {
        private readonly ObservableNode node;

        public ObservableNodeDynamicMetaObject(Expression parameter, ObservableNode observableNode)
            : base(parameter, BindingRestrictions.Empty, observableNode)
        {
            node = observableNode;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var self = Expression.Convert(Expression, LimitType);

            Expression expression;
            var propertyName = binder.Name;
            var args = new Expression[1];

            if (binder.Name.StartsWith(ObservableViewModel.HasChildPrefix))
            {
                propertyName = binder.Name.Substring(ObservableViewModel.HasChildPrefix.Length);
                args[0] = Expression.Constant(propertyName);
                expression = Expression.Call(self, typeof(ObservableNode).GetMethod(nameof(ObservableNode.GetChild), BindingFlags.Public | BindingFlags.Instance), args);
                expression = Expression.Convert(Expression.NotEqual(expression, Expression.Constant(null)), binder.ReturnType);
            }
            else if (binder.Name.StartsWith(ObservableViewModel.HasCommandPrefix))
            {
                propertyName = binder.Name.Substring(ObservableViewModel.HasCommandPrefix.Length);
                args[0] = Expression.Constant(propertyName);
                expression = Expression.Call(self, typeof(ObservableNode).GetMethod(nameof(ObservableNode.GetCommand), BindingFlags.Public | BindingFlags.Instance), args);
                expression = Expression.Convert(Expression.NotEqual(expression, Expression.Constant(null)), binder.ReturnType);
            }
            else if (binder.Name.StartsWith(ObservableViewModel.HasAssociatedDataPrefix))
            {
                propertyName = binder.Name.Substring(ObservableViewModel.HasAssociatedDataPrefix.Length);
                args[0] = Expression.Constant(propertyName);
                expression = Expression.Call(self, typeof(ObservableNode).GetMethod(nameof(ObservableNode.GetAssociatedData), BindingFlags.Public | BindingFlags.Instance), args);
                expression = Expression.Convert(Expression.NotEqual(expression, Expression.Constant(null)), binder.ReturnType);
            }
            else
            {
                args[0] = Expression.Constant(propertyName);
                expression = Expression.Call(self, typeof(ObservableNode).GetMethod(nameof(ObservableNode.GetDynamicObject), BindingFlags.Public | BindingFlags.Instance), args);
            }

            var getMember = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            return getMember;
        }

        public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames()
        {
            return node.Children.Select(x => x.Name).Concat(node.Commands.Select(x => x.Name)).Concat(node.AssociatedData.Select(x => x.Key));
        }
    }
}
