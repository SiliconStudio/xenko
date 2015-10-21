namespace SiliconStudio.Core.Updater
{
    public struct UpdateObjectData
    {
        public int Condition;
        public object Value;

        public UpdateObjectData(int condition, object value)
        {
            Condition = condition;
            Value = value;
        }

        public UpdateObjectData(int value) : this()
        {
            Condition = 1;
            Value = value;
        }
    }
}