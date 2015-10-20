namespace SiliconStudio.Core.Updater
{
    struct UpdateOperation
    {
        internal UpdateOperationType Type;
        internal UpdatableMember Member;

        // TODO: Should we switch to short + short? (note: could be a problem with big arrays)

        // Apply an offset to current object pointer.
        public int AdjustOffset;

        // Note: It is either an offset (blittable struct) or an index into object array (reference types and non blittable struct)
        public int DataOffset;
    
        public override string ToString()
        {
            return Type.ToString();
        }
    }
}