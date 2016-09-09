// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME || SILICONSTUDIO_RUNTIME_CORECLR
namespace System.Security
{
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = true, Inherited = false)]
    public class SuppressUnmanagedCodeSecurityAttribute : Attribute
    {
    }
}
#endif
