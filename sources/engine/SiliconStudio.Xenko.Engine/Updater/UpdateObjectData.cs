namespace SiliconStudio.Xenko.Updater
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

        public UpdateObjectData(object value) : this()
        {
            Condition = 1;
            Value = value;
        }
    }
}