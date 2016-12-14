using System;

namespace SiliconStudio.Xenko.VirtualReality
{
    public class NoHmdDeviceException : Exception
    {
        public NoHmdDeviceException() : base("Failed to initialize a VR HMD device.")
        {          
        }
    }
}