// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Quantum.Legacy
{
    [DataContract(Inherited = true)]
    [DataSerializer(typeof(ViewModelReferenceSerializer))]
    public class ViewModelReference
    {
        public IViewModelNode ViewModel { get; protected set; }

        public object Model
        {
            get { return model; }
        }


        public Guid Guid { get; protected set; }
        public bool Recursive { get; protected set; }
        public bool Visible { get; protected set; }

        protected object model;
        protected IChildrenPropertyEnumerator[] additionalEnumerators;

        // Used for multi-selection
        internal ViewModelReference[] AdditionalReferences;

        protected ViewModelReference()
        {
            Visible = true;
        }

        public ViewModelReference(Guid guid, ViewModelContext context)
            : this()
        {
            Guid = guid;
            if (context != null)
                ViewModel = context.GetViewModelNode(Guid);
        }

        public ViewModelReference(object model)
            : this()
        {
            this.model = model;
        }

        public virtual void UpdateGuid(ViewModelContext context)
        {
            if (model != null)
            {
                Guid = context.GetOrCreateGuid(model);
            }
        }

        public bool Equals(ViewModelReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Guid.Equals(Guid) && other.ViewModel == ViewModel;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ViewModelReference) && Equals((ViewModelReference)obj);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}: Model[{1}], ViewModel[{2}]", Guid, Model, ViewModel);
        }

        public static bool operator ==(ViewModelReference left, ViewModelReference right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ViewModelReference left, ViewModelReference right)
        {
            return !Equals(left, right);
        }
    }

    public class ViewModelReferenceGuidComparer : IEqualityComparer<ViewModelReference>
    {
        public bool Equals(ViewModelReference x, ViewModelReference y)
        {
            return ReferenceEquals(x, y) || x.Guid.Equals(y.Guid);
        }

        public int GetHashCode(ViewModelReference obj)
        {
            return obj.Guid.GetHashCode();
        }
    }

    public class CombinedViewModelReference : ViewModelReference
    {
        public CombinedViewModelReference(IEnumerable<object> models)
        {
            model = models.ToArray();
            AdditionalReferences = ((object[])model).Select(x => new ViewModelReference(x)).ToArray();
        }

        public override void UpdateGuid(ViewModelContext context)
        {
            IViewModelNode[] viewModels = ((object[])model).Select(x => context.GetOrCreateModelView(x, "")).ToArray();
            ViewModel = context.GetOrCreateCombinedViewModel(viewModels);
            Guid = ViewModel.Guid;
        }
    }

    public class ViewModelReferenceSerializer : DataSerializer<ViewModelReference>
    {
        public override void Serialize(ref ViewModelReference obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                obj.UpdateGuid(stream.Context.Get(ViewModelController.ContextProperty));
                stream.Write(obj.Guid);
            }
            else
            {
                obj = new ViewModelReference(stream.Read<Guid>(), stream.Context.Get(ViewModelController.ContextProperty));
            }
        }
    }
}