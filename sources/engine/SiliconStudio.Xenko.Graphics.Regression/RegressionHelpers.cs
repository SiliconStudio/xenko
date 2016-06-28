// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using SiliconStudio.Core.LZ4;

#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.System.Profile;
using Windows.Security.ExchangeActiveSyncProvisioning;
#endif

namespace SiliconStudio.Xenko.Graphics.Regression
{
    public partial class TestRunner
    {
        public const string XenkoVersion = "XENKO_VERSION";

        public const string XenkoBuildNumber = "XENKO_BUILD_NUMBER";

        public const string XenkoTestName = "XENKO_TEST_NAME";

        public const string XenkoBranchName = "XENKO_BRANCH_NAME";
    }

    enum ImageServerMessageType
    {
        ConnectionFinished = 0,
        SendImage = 1,
        RequestImageComparisonStatus = 2,
    }
    
    public class PlatformPermutator
    {
        public static ImageTestResultConnection GetDefaultImageTestResultConnection()
        {
            var result = new ImageTestResultConnection();

            // TODO: Check build number in environment variables
            result.BuildNumber = -1;

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            result.Platform = "Windows";
            result.Serial = Environment.MachineName;
    #if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12
            result.DeviceName = "Direct3D12";
    #elif SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
            result.DeviceName = "Direct3D";
    #elif SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            result.DeviceName = "OpenGLES";
    #elif SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
            result.DeviceName = "OpenGL";
    #elif SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
            result.DeviceName = "Vulkan";
    #endif
#elif SILICONSTUDIO_PLATFORM_ANDROID
            result.Platform = "Android";
            result.DeviceName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            result.Serial = Android.OS.Build.Serial ?? "Unknown";
#elif SILICONSTUDIO_PLATFORM_IOS
            result.Platform = "iOS";
            result.DeviceName = iOSDeviceType.Version.ToString();
            result.Serial = UIKit.UIDevice.CurrentDevice.Name;
#elif SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
    #if SILICONSTUDIO_PLATFORM_WINDOWS_PHONE
            result.Platform = "WindowsPhone";
    #elif SILICONSTUDIO_PLATFORM_WINDOWS_STORE
            result.Platform = "WindowsStore";
    #else
            result.Platform = "Windows10";
    #endif
            var deviceInfo = new EasClientDeviceInformation();
            result.DeviceName = deviceInfo.SystemManufacturer + " " + deviceInfo.SystemProductName;
            try
            {
                result.Serial = deviceInfo.Id.ToString();
            }
            catch (Exception)
            {
    #if SILICONSTUDIO_PLATFORM_WINDOWS_PHONE || SILICONSTUDIO_PLATFORM_WINDOWS_STORE
                var token = HardwareIdentification.GetPackageSpecificToken(null);
                var hardwareId = token.Id;

                var hasher = HashAlgorithmProvider.OpenAlgorithm("MD5");
                var hashed = hasher.HashData(hardwareId);

                result.Serial = CryptographicBuffer.EncodeToHexString(hashed);
    #else
                // Ignored on Windows 10
    #endif
            }
#endif

            return result;
        }

        public static string GetCurrentPlatformName()
        {
            return GetPlatformName(GetPlatform());
        }

        public static string GetPlatformName(TestPlatform platform)
        {
            switch (platform)
            {
                case TestPlatform.WindowsDx:
                    return "Windows_Direct3D11";
                case TestPlatform.WindowsOgl:
                    return "Windows_OpenGL";
                case TestPlatform.WindowsOgles:
                    return "Windows_OpenGLES";
                case TestPlatform.Android:
                    return "Android";
                case TestPlatform.Ios:
                    return "IOS";
                case TestPlatform.WindowsPhone:
                    return "Windows_Phone";
                case TestPlatform.WindowsStore:
                    return "Windows_Store";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static TestPlatform GetPlatform()
        {
#if SILICONSTUDIO_PLATFORM_ANDROID
            return TestPlatform.Android;
#elif SILICONSTUDIO_PLATFORM_IOS
            return TestPlatform.Ios;
#elif SILICONSTUDIO_PLATFORM_WINDOWS_PHONE
            return TestPlatform.WindowsPhone;
#elif SILICONSTUDIO_PLATFORM_WINDOWS_STORE
            return TestPlatform.WindowsStore;
#elif SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D
            return TestPlatform.WindowsDx;
#elif SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            return TestPlatform.WindowsOgles;
#elif SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
            return TestPlatform.WindowsOgl;
#elif SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
            return TestPlatform.WindowsVulkan;
#endif

        }
    }

    [Flags]
    public enum ImageComparisonFlags
    {
        CopyOnShare = 1,
    }

    public class ImageTestResultConnection
    {
        public int BuildNumber;
        public string Platform;
        public string Serial;
        public string DeviceName;
        public string BranchName = "";
        public ImageComparisonFlags Flags;

        public void Read(BinaryReader reader)
        {
            Platform = reader.ReadString();
            BuildNumber = reader.ReadInt32();
            Serial = reader.ReadString();
            DeviceName = reader.ReadString();
            BranchName = reader.ReadString();
            Flags = (ImageComparisonFlags)reader.ReadInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Platform);
            writer.Write(BuildNumber);
            writer.Write(Serial);
            writer.Write(DeviceName);
            writer.Write(BranchName);
            writer.Write((int)Flags);
        }
    }

    public struct ImageInformation
    {
        public int Width;
        public int Height;
        public int TextureSize;
        public int BaseVersion;
        public int CurrentVersion;
        public int FrameIndex;
        public TestPlatform Platform;
        public PixelFormat Format;
    }

    public enum TestPlatform
    {
        WindowsDx,
        WindowsOgl,
        WindowsOgles,
        WindowsVulkan,
        WindowsStore,
        WindowsPhone,
        Android,
        Ios
    }
}
