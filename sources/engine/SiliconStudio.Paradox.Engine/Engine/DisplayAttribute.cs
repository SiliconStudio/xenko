// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Reflection;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Paradox.Engine
{
    public class DisplayAttribute : Attribute
    {
        private readonly int order;

        private readonly string name;

        private readonly string category;

        private readonly string description;

        public DisplayAttribute(int order , string name = null, string category = null, string description = null)
        {
            this.order = order;
            this.name = name;
            this.category = category;
            this.description = description;
        }

        public DisplayAttribute(string name = null, string category = null, string description = null) : this(0, name, category, description)
        {
        }

        public int Order
        {
            get
            {
                return order;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Category
        {
            get
            {
                return category;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        public static bool IsDisplayable(object obj)
        {
            return IsDisplayable(obj.GetType());
        }

        public static bool IsDisplayable(Type type)
        {
            return type.GetTypeInfo().GetCustomAttributes(typeof(DisplayAttribute), true).GetEnumerator().MoveNext();
        }

        public static DisplayAttribute GetDisplay(Type type)
        {
            var attributes = type.GetTypeInfo().GetCustomAttributes(typeof(DisplayAttribute), true);
            return attributes.FirstOrDefault() as DisplayAttribute;
        }

        public static DisplayAttribute GetDisplay(MemberInfo memberInfo)
        {
            var attributes = memberInfo.GetCustomAttributes(typeof(DisplayAttribute), true);
            return attributes.FirstOrDefault() as DisplayAttribute;
        }
    }
}