// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;
using System.Reflection;

namespace SiliconStudio.Presentation.Legacy
{
    internal class CheckablePart : INotifyPropertyChanged
    {
        public CheckablePart(SizeI coordinate)
        {
            Coordinate = coordinate;
        }

        public SizeI Coordinate { get; private set; }

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    internal abstract class ValuedCheckablePart : CheckablePart
    {
        protected internal object instance;
        protected readonly string memberName;

        protected ValuedCheckablePart(SizeI coordinate, object instance, string memberName)
            : base(coordinate)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (memberName == null)
                throw new ArgumentNullException("memberName");

            this.instance = instance;
            this.memberName = memberName;
        }

        public virtual string Name { get { return memberName; } }
        public abstract object Value { get; set; }

        public void UpdateInstance(object newInstance)
        {
            instance = newInstance;
            OnPropertyChanged("Value");
        }
    }

    internal class AutomaticValuedCheckablePart : ValuedCheckablePart
    {
        private readonly MemberInfo member;

        public AutomaticValuedCheckablePart(SizeI coordinate, object instance, string memberName)
            : this(coordinate, instance, instance.GetType(), memberName)
        {
        }

        public AutomaticValuedCheckablePart(SizeI coordinate, object instance, Type type, string memberName)
            : base(coordinate, instance, memberName)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            member = type.GetField(memberName);
            if (member == null)
                member = type.GetProperty(memberName);

            if (member == null)
                throw new ArgumentException(string.Format("Member '{0}' not found in type '{1}'.", memberName, type.FullName));
        }

        public override object Value
        {
            get
            {
                if (member.MemberType == MemberTypes.Field)
                    return ((FieldInfo)member).GetValue(instance);
                else
                    return ((PropertyInfo)member).GetValue(instance, null);
            }
            set
            {
                if (object.Equals(Value, value) == false)
                {
                    if (member.MemberType == MemberTypes.Field)
                        ((FieldInfo)member).SetValue(instance, value);
                    else
                        ((PropertyInfo)member).SetValue(instance, value, null);

                    OnPropertyChanged("Value");
                }
            }
        }
    }

    internal class DirectValuedCheckablePart : ValuedCheckablePart
    {
        private readonly IFullVectorElementsMapper mapper;

        public DirectValuedCheckablePart(SizeI coordinate, object instance, string memberName, IFullVectorElementsMapper mapper)
            : base(coordinate, instance, memberName)
        {
            if (mapper == null)
                throw new ArgumentNullException("mapper");

            this.mapper = mapper;
        }

        public override object Value
        {
            get
            {
                return mapper.GetValue(instance, memberName);
            }
            set
            {
                if (mapper.SetValue(instance, memberName, value))
                    OnPropertyChanged("Value");
            }
        }
    }
}
