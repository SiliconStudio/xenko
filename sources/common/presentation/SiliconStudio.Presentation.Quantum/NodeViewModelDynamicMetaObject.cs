// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SiliconStudio.Presentation.Quantum
{
    internal class NodeViewModelDynamicMetaObject : DynamicMetaObject
    {
        private readonly NodeViewModelBase node;

        public NodeViewModelDynamicMetaObject(Expression parameter, NodeViewModelBase node)
            : base(parameter, BindingRestrictions.Empty, node)
        {
            this.node = node;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var self = Expression.Convert(Expression, LimitType);

            Expression expression;
            var propertyName = binder.Name;
            var args = new Expression[1];

            if (binder.Name.StartsWith(GraphViewModel.HasChildPrefix))
            {
                propertyName = binder.Name.Substring(GraphViewModel.HasChildPrefix.Length);
                args[0] = Expression.Constant(propertyName);
                expression = Expression.Call(self, typeof(NodeViewModelBase).GetMethod(nameof(NodeViewModelBase.GetChild), BindingFlags.Public | BindingFlags.Instance), args);
                expression = Expression.Convert(Expression.NotEqual(expression, Expression.Constant(null)), binder.ReturnType);
            }
            else if (binder.Name.StartsWith(GraphViewModel.HasCommandPrefix))
            {
                propertyName = binder.Name.Substring(GraphViewModel.HasCommandPrefix.Length);
                args[0] = Expression.Constant(propertyName);
                expression = Expression.Call(self, typeof(NodeViewModelBase).GetMethod(nameof(NodeViewModelBase.GetCommand), BindingFlags.Public | BindingFlags.Instance), args);
                expression = Expression.Convert(Expression.NotEqual(expression, Expression.Constant(null)), binder.ReturnType);
            }
            else if (binder.Name.StartsWith(GraphViewModel.HasAssociatedDataPrefix))
            {
                propertyName = binder.Name.Substring(GraphViewModel.HasAssociatedDataPrefix.Length);
                args[0] = Expression.Constant(propertyName);
                expression = Expression.Call(self, typeof(NodeViewModelBase).GetMethod(nameof(NodeViewModelBase.GetAssociatedData), BindingFlags.Public | BindingFlags.Instance), args);
                expression = Expression.Convert(Expression.NotEqual(expression, Expression.Constant(null)), binder.ReturnType);
            }
            else
            {
                args[0] = Expression.Constant(propertyName);
                expression = Expression.Call(self, typeof(NodeViewModelBase).GetMethod(nameof(NodeViewModelBase.GetDynamicObject), BindingFlags.Public | BindingFlags.Instance), args);
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
