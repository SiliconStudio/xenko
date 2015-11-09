namespace SiliconStudio.Xenko.Updater
{
    public struct UpdateMemberInfo
    {
        public string Name;
        public int DataOffset;

        public UpdateMemberInfo(string name, int dataOffset)
        {
            Name = name;
            DataOffset = dataOffset;
        }
    }
}