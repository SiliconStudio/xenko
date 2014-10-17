namespace SiliconStudio.Paradox.Graphics.Regression
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class RegressionTestAttribute : System.Attribute
    {
        private int frameIndex;

        public RegressionTestAttribute(int frame)
        {
            frameIndex = frame;
        }
    }
}
