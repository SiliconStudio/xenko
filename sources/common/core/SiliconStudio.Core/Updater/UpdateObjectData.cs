namespace SiliconStudio.Core.Updater
{
    public struct UpdateObjectData
    {
        public float Condition;
        public object Value;

        public UpdateObjectData(float condition, object value)
        {
            Condition = condition;
            Value = value;
        }

        public UpdateObjectData(object value) : this()
        {
            Condition = 1.0f;
            Value = value;
        }
    }
}