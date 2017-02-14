// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace SiliconStudio.Xenko.Graphics
{
    public class RenderDocManager
    {
        private const string RenderdocClsid = "{5D6BF029-A6BA-417A-8523-120492B1DCE3}";
        private const string LibraryName = "renderdoc.dll";

        private bool isCaptureStarted;

        // Matching https://github.com/baldurk/renderdoc/blob/master/renderdoc/api/app/renderdoc_app.h

        public RenderDocManager(string logFilePath = null)
        {
            var finalLogFilePath = FindAvailablePath((logFilePath ?? Assembly.GetEntryAssembly().Location));

            var reg = Registry.ClassesRoot.OpenSubKey("CLSID\\" + RenderdocClsid + "\\InprocServer32");
            if (reg == null)
            {
                return;
            }
            var path = reg.GetValue(null) != null ? reg.GetValue(null).ToString() : null;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return;
            }

            // Preload the library before using the DLLImport
            var ptr = LoadLibrary(path);
            if (ptr == IntPtr.Zero)
            {
                return;
            }

            // Make sure that the code is compatible with the current installed version.
            // TODO: Newest version of RenderDoc changed their API and we should call RENDERDOC_GetAPI which returns other function pointers
            if (RENDERDOC_API_VERSION != RENDERDOC_GetAPIVersion())
            {
                return;
            }

            RENDERDOC_SetLogFile(finalLogFilePath);

            var focusToggleKey = KeyButton.eKey_F11;
            RENDERDOC_SetFocusToggleKeys(ref focusToggleKey, 1);
            var captureKey = KeyButton.eKey_F12;
            RENDERDOC_SetCaptureKeys(ref captureKey, 1);

            var options = new CaptureOptions();
            RENDERDOC_SetCaptureOptions(ref options);

            int socketPort = 0;
            RENDERDOC_InitRemoteAccess(ref socketPort);
            //RENDERDOC_MaskOverlayBits()
        }

        public void Shutdown()
        {
            RENDERDOC_Shutdown();
        }

        public void StartCapture(IntPtr hwndPtr)
        {
            if (hwndPtr == IntPtr.Zero)
            {
                throw new ArgumentNullException("hwndPtr");
            }

            RENDERDOC_StartFrameCapture(hwndPtr);
            isCaptureStarted = true;
        }

        public void EndFrameCapture(IntPtr hwndPtr)
        {
            if (hwndPtr == IntPtr.Zero)
            {
                throw new ArgumentNullException("hwndPtr");
            }

            if (!isCaptureStarted)
                return;
            if (RENDERDOC_EndFrameCapture(hwndPtr))
            {
                isCaptureStarted = false;
                return;
            }

            ;
            // The capture has failed, calling m_RenderDocEndFrameCapture several time to make sure it won't keep capturing forever.
            while (!RENDERDOC_EndFrameCapture(hwndPtr))
            {
            }
            isCaptureStarted = false;
        }

        private static string FindAvailablePath(string logFilePath)
        {
            var filePath = Path.GetFileNameWithoutExtension(logFilePath);
            for (int i = 0; i < 1000000; i++)
            {
                var path = filePath;
                if (i > 0)
                {
                    path += i;
                }
                path += ".rdc";

                if (!File.Exists(path))
                {
                    return path;
                }
            }
            return filePath + ".rdc";
        }

        [StructLayout(LayoutKind.Sequential)]
        public class CaptureOptions
        {
            /// <summary>
            /// Defaults this instance.
            /// </summary>
            /// <returns>CaptureOptions.</returns>
            public CaptureOptions()
            {
                AllowVSync = true;
                AllowFullscreen = true;
            }

            /// <summary>
            /// Whether or not to allow the application to enable vsync
            ///
            /// Enabled - allows the application to enable or disable vsync at will
            /// Disabled - vsync is force disabled
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool AllowVSync;

            /// <summary>
            /// Whether or not to allow the application to enable fullscreen
            /// Enabled - allows the application to enable or disable fullscreen at will
            /// Disabled - fullscreen is force disabled
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool AllowFullscreen;

            /// <summary>
            /// Enables in-built API debugging features and records the results into the
            /// capture logfile, which is matched up with events on replay
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool DebugDeviceMode;

            /// <summary>
            /// Captures callstacks for every API event during capture
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool CaptureCallstacks;

            /// <summary>
            /// Only captures callstacks for drawcall type API events.
            /// Ignored if CaptureCallstacks is disabled
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool CaptureCallstacksOnlyDraws;

            /// <summary>
            /// Specify a delay in seconds to wait for a debugger to attach after
            /// creating or injecting into a process, before continuing to allow it to run.
            /// </summary>
            public int DelayForDebugger;

            /// <summary>
            /// Verify any writes to mapped buffers, to check that they don't overwrite the
            /// bounds of the pointer returned.
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool VerifyMapWrites;

            /// <summary>
            /// Hooks any system API events that create child processes, and injects
            /// renderdoc into them recursively with the same options.
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool HookIntoChildren;

            /// <summary>
            /// By default renderdoc only includes resources in the final logfile necessary
            /// for that frame, this allows you to override that behaviour
            ///
            /// Enabled - all live resources at the time of capture are included in the log
            /// and available for inspection
            /// Disabled - only the resources referenced by the captured frame are included
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool RefAllResources;

            /// <summary>
            /// By default renderdoc skips saving initial states for
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool SaveAllInitials;

            /// <summary>
            /// In APIs that allow for the recording of command lists to be replayed later,
            /// renderdoc may choose to not capture command lists before a frame capture is
            /// triggered, to reduce overheads. This means any command lists recorded once
            /// and replayed many times will not be available and may cause a failure to
            /// capture.
            ///
            /// Enabled - All command lists are captured from the start of the application
            /// Disabled - Command lists are only captured if their recording begins during
            /// the period when a frame capture is in progress.
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool CaptureAllCmdLists;
        }

        private enum KeyButton : uint
        {
            eKey_0 = 0x30, // '0'
            // ...
            eKey_9 = 0x39, // '9'
            eKey_A = 0x41, // 'A'
            // ...
            eKey_Z = 0x5A, // 'Z'
            eKey_Divide,
            eKey_Multiply,
            eKey_Subtract,
            eKey_Plus,
            eKey_F1,
            eKey_F2,
            eKey_F3,
            eKey_F4,
            eKey_F5,
            eKey_F6,
            eKey_F7,
            eKey_F8,
            eKey_F9,
            eKey_F10,
            eKey_F11,
            eKey_F12,
            eKey_Home,
            eKey_End,
            eKey_Insert,
            eKey_Delete,
            eKey_PageUp,
            eKey_PageDn,
            eKey_Backspace,
            eKey_Tab,
            eKey_PrtScrn,
            eKey_Pause,
            eKey_Max,
        };

        private enum InAppOverlay : uint
        {
            eOverlay_Enabled = 0x1,
            eOverlay_FrameRate = 0x2,
            eOverlay_FrameNumber = 0x4,
            eOverlay_CaptureList = 0x8,
            eOverlay_Default = (eOverlay_Enabled | eOverlay_FrameRate | eOverlay_FrameNumber | eOverlay_CaptureList),
            eOverlay_All = 0xFFFFFFFF,
            eOverlay_None = 0,
        };


        // API breaking change history:
        // Version 1 -> 2 - strings changed from wchar_t* to char* (UTF-8)
        private const int RENDERDOC_API_VERSION = 2;

        //////////////////////////////////////////////////////////////////////////
        // In-program functions
        //////////////////////////////////////////////////////////////////////////
        [DllImport(LibraryName, EntryPoint = "RENDERDOC_GetAPIVersion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int RENDERDOC_GetAPIVersion();

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_Shutdown", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RENDERDOC_Shutdown();

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_SetLogFile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RENDERDOC_SetLogFile([MarshalAs(UnmanagedType.LPStr)] string logfile);

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_GetLogFile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern string RENDERDOC_GetLogFile();

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_GetCapture", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RENDERDOC_GetCapture(int idx, [MarshalAs(UnmanagedType.LPStr)] string logfile, out int pathlength, out long timestamp);

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_SetCaptureOptions", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RENDERDOC_SetCaptureOptions(ref CaptureOptions opts);

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_SetActiveWindow", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RENDERDOC_SetActiveWindow(IntPtr wndHandle);

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_TriggerCapture", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RENDERDOC_TriggerCapture();

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_StartFrameCapture", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RENDERDOC_StartFrameCapture(IntPtr wndHandle);

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_EndFrameCapture", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RENDERDOC_EndFrameCapture(IntPtr wndHandle);

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_GetOverlayBits", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern InAppOverlay RENDERDOC_GetOverlayBits();

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_MaskOverlayBits", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RENDERDOC_MaskOverlayBits(InAppOverlay And, InAppOverlay Or);

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_SetFocusToggleKeys", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RENDERDOC_SetFocusToggleKeys(ref KeyButton keys, int num);

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_SetCaptureKeys", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RENDERDOC_SetCaptureKeys(ref KeyButton keys, int num);

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_InitRemoteAccess", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RENDERDOC_InitRemoteAccess(ref int ident);

        [DllImport(LibraryName, EntryPoint = "RENDERDOC_UnloadCrashHandler", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RENDERDOC_UnloadCrashHandler();

        [DllImport("kernel32", EntryPoint = "LoadLibrary", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);
    }
}