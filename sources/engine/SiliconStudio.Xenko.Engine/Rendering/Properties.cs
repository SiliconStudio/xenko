namespace SiliconStudio.Xenko.Rendering
{
    public enum DataType
    {
        ViewObject,
        Object,
        Render,
        EffectObject,
        View,
        EffectView,
        StaticObject,
        StaticEffectObject,
    }

    public struct ViewObjectPropertyData<T>
    {
        internal T[] Data;

        internal ViewObjectPropertyData(T[] data)
        {
            Data = data;
        }

        internal T this[ViewObjectNodeReference index]
        {
            get { return Data[index.Index]; }
            set { Data[index.Index] = value; }
        }
    }

	public struct ViewObjectPropertyKey<T>
    {
        internal readonly int Index;

        internal ViewObjectPropertyKey(int index)
        {
            Index = index;
        }
    }

	public class ViewObjectPropertyDefinition<T>
    {
    }

	public partial struct ViewObjectNodeReference
    {
        internal readonly int Index;

        /// <summary>
        /// Invalid slot.
        /// </summary>
		public static readonly ViewObjectNodeReference Invalid = new ViewObjectNodeReference(-1);

        internal ViewObjectNodeReference(int index)
        {
            Index = index;
        }

        public static bool operator ==(ViewObjectNodeReference a, ViewObjectNodeReference b)
        {
            return a.Index == b.Index;
        }

		public static bool operator !=(ViewObjectNodeReference a, ViewObjectNodeReference b)
        {
            return a.Index != b.Index;
        }
    }

	partial class RootRenderFeature
	{
        internal ViewObjectPropertyKey<T> CreateViewObjectKey<T>(ViewObjectPropertyDefinition<T> definition = null)
        {
            if (definition != null)
            {
                int existingIndex;
                if (dataArraysByDefinition.TryGetValue(definition, out existingIndex))
                    return new ViewObjectPropertyKey<T>(existingIndex);

                dataArraysByDefinition.Add(definition, dataArrays.Count);
            }

            var dataArraysIndex = dataArrays.Count;
			dataArrays.Add(new DataArray(new DataArrayInfo<T>(DataType.ViewObject)));
            return new ViewObjectPropertyKey<T>(dataArraysIndex);
        }

		internal ViewObjectPropertyData<T> GetData<T>(ViewObjectPropertyKey<T> key)
        {
            return new ViewObjectPropertyData<T>((T[])dataArrays[key.Index].Array);
        }
	}
    public struct ObjectPropertyData<T>
    {
        internal T[] Data;

        internal ObjectPropertyData(T[] data)
        {
            Data = data;
        }

        internal T this[ObjectNodeReference index]
        {
            get { return Data[index.Index]; }
            set { Data[index.Index] = value; }
        }
    }

	public struct ObjectPropertyKey<T>
    {
        internal readonly int Index;

        internal ObjectPropertyKey(int index)
        {
            Index = index;
        }
    }

	public class ObjectPropertyDefinition<T>
    {
    }

	public partial struct ObjectNodeReference
    {
        internal readonly int Index;

        /// <summary>
        /// Invalid slot.
        /// </summary>
		public static readonly ObjectNodeReference Invalid = new ObjectNodeReference(-1);

        internal ObjectNodeReference(int index)
        {
            Index = index;
        }

        public static bool operator ==(ObjectNodeReference a, ObjectNodeReference b)
        {
            return a.Index == b.Index;
        }

		public static bool operator !=(ObjectNodeReference a, ObjectNodeReference b)
        {
            return a.Index != b.Index;
        }
    }

	partial class RootRenderFeature
	{
        internal ObjectPropertyKey<T> CreateObjectKey<T>(ObjectPropertyDefinition<T> definition = null)
        {
            if (definition != null)
            {
                int existingIndex;
                if (dataArraysByDefinition.TryGetValue(definition, out existingIndex))
                    return new ObjectPropertyKey<T>(existingIndex);

                dataArraysByDefinition.Add(definition, dataArrays.Count);
            }

            var dataArraysIndex = dataArrays.Count;
			dataArrays.Add(new DataArray(new DataArrayInfo<T>(DataType.Object)));
            return new ObjectPropertyKey<T>(dataArraysIndex);
        }

		internal ObjectPropertyData<T> GetData<T>(ObjectPropertyKey<T> key)
        {
            return new ObjectPropertyData<T>((T[])dataArrays[key.Index].Array);
        }
	}
    public struct RenderPropertyData<T>
    {
        internal T[] Data;

        internal RenderPropertyData(T[] data)
        {
            Data = data;
        }

        internal T this[RenderNodeReference index]
        {
            get { return Data[index.Index]; }
            set { Data[index.Index] = value; }
        }
    }

	public struct RenderPropertyKey<T>
    {
        internal readonly int Index;

        internal RenderPropertyKey(int index)
        {
            Index = index;
        }
    }

	public class RenderPropertyDefinition<T>
    {
    }

	public partial struct RenderNodeReference
    {
        internal readonly int Index;

        /// <summary>
        /// Invalid slot.
        /// </summary>
		public static readonly RenderNodeReference Invalid = new RenderNodeReference(-1);

        public RenderNodeReference(int index)
        {
            Index = index;
        }

        public static bool operator ==(RenderNodeReference a, RenderNodeReference b)
        {
            return a.Index == b.Index;
        }

		public static bool operator !=(RenderNodeReference a, RenderNodeReference b)
        {
            return a.Index != b.Index;
        }
    }

	partial class RootRenderFeature
	{
        internal RenderPropertyKey<T> CreateRenderKey<T>(RenderPropertyDefinition<T> definition = null)
        {
            if (definition != null)
            {
                int existingIndex;
                if (dataArraysByDefinition.TryGetValue(definition, out existingIndex))
                    return new RenderPropertyKey<T>(existingIndex);

                dataArraysByDefinition.Add(definition, dataArrays.Count);
            }

            var dataArraysIndex = dataArrays.Count;
			dataArrays.Add(new DataArray(new DataArrayInfo<T>(DataType.Render)));
            return new RenderPropertyKey<T>(dataArraysIndex);
        }

		internal RenderPropertyData<T> GetData<T>(RenderPropertyKey<T> key)
        {
            return new RenderPropertyData<T>((T[])dataArrays[key.Index].Array);
        }
	}
    public struct EffectObjectPropertyData<T>
    {
        internal T[] Data;

        internal EffectObjectPropertyData(T[] data)
        {
            Data = data;
        }

        internal T this[EffectObjectNodeReference index]
        {
            get { return Data[index.Index]; }
            set { Data[index.Index] = value; }
        }
    }

	public struct EffectObjectPropertyKey<T>
    {
        internal readonly int Index;

        internal EffectObjectPropertyKey(int index)
        {
            Index = index;
        }
    }

	public class EffectObjectPropertyDefinition<T>
    {
    }

	public partial struct EffectObjectNodeReference
    {
        internal readonly int Index;

        /// <summary>
        /// Invalid slot.
        /// </summary>
		public static readonly EffectObjectNodeReference Invalid = new EffectObjectNodeReference(-1);

        internal EffectObjectNodeReference(int index)
        {
            Index = index;
        }

        public static bool operator ==(EffectObjectNodeReference a, EffectObjectNodeReference b)
        {
            return a.Index == b.Index;
        }

		public static bool operator !=(EffectObjectNodeReference a, EffectObjectNodeReference b)
        {
            return a.Index != b.Index;
        }
    }

	partial class RootRenderFeature
	{
        internal EffectObjectPropertyKey<T> CreateEffectObjectKey<T>(EffectObjectPropertyDefinition<T> definition = null)
        {
            if (definition != null)
            {
                int existingIndex;
                if (dataArraysByDefinition.TryGetValue(definition, out existingIndex))
                    return new EffectObjectPropertyKey<T>(existingIndex);

                dataArraysByDefinition.Add(definition, dataArrays.Count);
            }

            var dataArraysIndex = dataArrays.Count;
			dataArrays.Add(new DataArray(new DataArrayInfo<T>(DataType.EffectObject)));
            return new EffectObjectPropertyKey<T>(dataArraysIndex);
        }

		internal EffectObjectPropertyData<T> GetData<T>(EffectObjectPropertyKey<T> key)
        {
            return new EffectObjectPropertyData<T>((T[])dataArrays[key.Index].Array);
        }
	}
    public struct ViewPropertyData<T>
    {
        internal T[] Data;

        internal ViewPropertyData(T[] data)
        {
            Data = data;
        }

        internal T this[ViewNodeReference index]
        {
            get { return Data[index.Index]; }
            set { Data[index.Index] = value; }
        }
    }

	public struct ViewPropertyKey<T>
    {
        internal readonly int Index;

        internal ViewPropertyKey(int index)
        {
            Index = index;
        }
    }

	public class ViewPropertyDefinition<T>
    {
    }

	public partial struct ViewNodeReference
    {
        internal readonly int Index;

        /// <summary>
        /// Invalid slot.
        /// </summary>
		public static readonly ViewNodeReference Invalid = new ViewNodeReference(-1);

        internal ViewNodeReference(int index)
        {
            Index = index;
        }

        public static bool operator ==(ViewNodeReference a, ViewNodeReference b)
        {
            return a.Index == b.Index;
        }

		public static bool operator !=(ViewNodeReference a, ViewNodeReference b)
        {
            return a.Index != b.Index;
        }
    }

	partial class RootRenderFeature
	{
        internal ViewPropertyKey<T> CreateViewKey<T>(ViewPropertyDefinition<T> definition = null)
        {
            if (definition != null)
            {
                int existingIndex;
                if (dataArraysByDefinition.TryGetValue(definition, out existingIndex))
                    return new ViewPropertyKey<T>(existingIndex);

                dataArraysByDefinition.Add(definition, dataArrays.Count);
            }

            var dataArraysIndex = dataArrays.Count;
			dataArrays.Add(new DataArray(new DataArrayInfo<T>(DataType.View)));
            return new ViewPropertyKey<T>(dataArraysIndex);
        }

		internal ViewPropertyData<T> GetData<T>(ViewPropertyKey<T> key)
        {
            return new ViewPropertyData<T>((T[])dataArrays[key.Index].Array);
        }
	}
    public struct EffectViewPropertyData<T>
    {
        internal T[] Data;

        internal EffectViewPropertyData(T[] data)
        {
            Data = data;
        }

        internal T this[EffectViewNodeReference index]
        {
            get { return Data[index.Index]; }
            set { Data[index.Index] = value; }
        }
    }

	public struct EffectViewPropertyKey<T>
    {
        internal readonly int Index;

        internal EffectViewPropertyKey(int index)
        {
            Index = index;
        }
    }

	public class EffectViewPropertyDefinition<T>
    {
    }

	public partial struct EffectViewNodeReference
    {
        internal readonly int Index;

        /// <summary>
        /// Invalid slot.
        /// </summary>
		public static readonly EffectViewNodeReference Invalid = new EffectViewNodeReference(-1);

        internal EffectViewNodeReference(int index)
        {
            Index = index;
        }

        public static bool operator ==(EffectViewNodeReference a, EffectViewNodeReference b)
        {
            return a.Index == b.Index;
        }

		public static bool operator !=(EffectViewNodeReference a, EffectViewNodeReference b)
        {
            return a.Index != b.Index;
        }
    }

	partial class RootRenderFeature
	{
        internal EffectViewPropertyKey<T> CreateEffectViewKey<T>(EffectViewPropertyDefinition<T> definition = null)
        {
            if (definition != null)
            {
                int existingIndex;
                if (dataArraysByDefinition.TryGetValue(definition, out existingIndex))
                    return new EffectViewPropertyKey<T>(existingIndex);

                dataArraysByDefinition.Add(definition, dataArrays.Count);
            }

            var dataArraysIndex = dataArrays.Count;
			dataArrays.Add(new DataArray(new DataArrayInfo<T>(DataType.EffectView)));
            return new EffectViewPropertyKey<T>(dataArraysIndex);
        }

		internal EffectViewPropertyData<T> GetData<T>(EffectViewPropertyKey<T> key)
        {
            return new EffectViewPropertyData<T>((T[])dataArrays[key.Index].Array);
        }
	}
    public struct StaticObjectPropertyData<T>
    {
        internal T[] Data;

        internal StaticObjectPropertyData(T[] data)
        {
            Data = data;
        }

        internal T this[StaticObjectNodeReference index]
        {
            get { return Data[index.Index]; }
            set { Data[index.Index] = value; }
        }
    }

	public struct StaticObjectPropertyKey<T>
    {
        internal readonly int Index;

        internal StaticObjectPropertyKey(int index)
        {
            Index = index;
        }
    }

	public class StaticObjectPropertyDefinition<T>
    {
    }

	public partial struct StaticObjectNodeReference
    {
        internal readonly int Index;

        /// <summary>
        /// Invalid slot.
        /// </summary>
		public static readonly StaticObjectNodeReference Invalid = new StaticObjectNodeReference(-1);

        internal StaticObjectNodeReference(int index)
        {
            Index = index;
        }

        public static bool operator ==(StaticObjectNodeReference a, StaticObjectNodeReference b)
        {
            return a.Index == b.Index;
        }

		public static bool operator !=(StaticObjectNodeReference a, StaticObjectNodeReference b)
        {
            return a.Index != b.Index;
        }
    }

	partial class RootRenderFeature
	{
        internal StaticObjectPropertyKey<T> CreateStaticObjectKey<T>(StaticObjectPropertyDefinition<T> definition = null)
        {
            if (definition != null)
            {
                int existingIndex;
                if (dataArraysByDefinition.TryGetValue(definition, out existingIndex))
                    return new StaticObjectPropertyKey<T>(existingIndex);

                dataArraysByDefinition.Add(definition, dataArrays.Count);
            }

            var dataArraysIndex = dataArrays.Count;
			dataArrays.Add(new DataArray(new DataArrayInfo<T>(DataType.StaticObject)));
            return new StaticObjectPropertyKey<T>(dataArraysIndex);
        }

		internal StaticObjectPropertyData<T> GetData<T>(StaticObjectPropertyKey<T> key)
        {
            return new StaticObjectPropertyData<T>((T[])dataArrays[key.Index].Array);
        }
	}
    public struct StaticEffectObjectPropertyData<T>
    {
        internal T[] Data;

        internal StaticEffectObjectPropertyData(T[] data)
        {
            Data = data;
        }

        public T this[StaticEffectObjectNodeReference index]
        {
            get { return Data[index.Index]; }
            set { Data[index.Index] = value; }
        }
    }

	public struct StaticEffectObjectPropertyKey<T>
    {
        internal readonly int Index;

        internal StaticEffectObjectPropertyKey(int index)
        {
            Index = index;
        }
    }

	public class StaticEffectObjectPropertyDefinition<T>
    {
    }

	public partial struct StaticEffectObjectNodeReference
    {
        internal readonly int Index;

        /// <summary>
        /// Invalid slot.
        /// </summary>
		public static readonly StaticEffectObjectNodeReference Invalid = new StaticEffectObjectNodeReference(-1);

        internal StaticEffectObjectNodeReference(int index)
        {
            Index = index;
        }

        public static bool operator ==(StaticEffectObjectNodeReference a, StaticEffectObjectNodeReference b)
        {
            return a.Index == b.Index;
        }

		public static bool operator !=(StaticEffectObjectNodeReference a, StaticEffectObjectNodeReference b)
        {
            return a.Index != b.Index;
        }
    }

	partial class RootRenderFeature
	{
        protected internal StaticEffectObjectPropertyKey<T> CreateStaticEffectObjectKey<T>(StaticEffectObjectPropertyDefinition<T> definition = null)
        {
            if (definition != null)
            {
                int existingIndex;
                if (dataArraysByDefinition.TryGetValue(definition, out existingIndex))
                    return new StaticEffectObjectPropertyKey<T>(existingIndex);

                dataArraysByDefinition.Add(definition, dataArrays.Count);
            }

            var dataArraysIndex = dataArrays.Count;
			dataArrays.Add(new DataArray(new DataArrayInfo<T>(DataType.StaticEffectObject)));
            return new StaticEffectObjectPropertyKey<T>(dataArraysIndex);
        }

		protected internal StaticEffectObjectPropertyData<T> GetData<T>(StaticEffectObjectPropertyKey<T> key)
        {
            return new StaticEffectObjectPropertyData<T>((T[])dataArrays[key.Index].Array);
        }
	}
}