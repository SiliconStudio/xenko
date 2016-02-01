using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    [DataContract]
    public struct ParameterKeyInfo
    {
        // Common
        public ParameterKey Key;

        // Values
        public int Offset;
        public int Size;

        // Resources
        public int BindingSlot;

        /// <summary>
        /// Describes a value parameter.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public ParameterKeyInfo(ParameterKey key, int offset, int size)
        {
            Key = key;
            Offset = offset;
            Size = size;
            BindingSlot = -1;
        }

        /// <summary>
        /// Describes a resource parameter.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="bindingSlot"></param>
        public ParameterKeyInfo(ParameterKey key, int bindingSlot)
        {
            Key = key;
            BindingSlot = bindingSlot;
            Offset = -1;
            Size = 1;
        }
    }
}