using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Testing
{
    [DataContract]
    public class ScreenShotPayload
    {
        public int Size;
        public byte[] Data;
        public string FileName;
    }
}