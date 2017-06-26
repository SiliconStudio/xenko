// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SiliconStudio.Xenko.Assets.BuilderApp")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyProduct("SiliconStudio.Xenko.Assets.BuilderApp")]

// #if !DEBUG
// [assembly: ObfuscateAssembly(false)]
// [assembly: Obfuscation(Feature = "embed Xenko.Effects.Modules.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed Xenko.Engine.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed Xenko.Framework.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed Xenko.Framework.Graphics.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed Xenko.Importer.FBX.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed Xenko.Importer.Assimp.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed Mono.Options.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed SharpDX.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed SharpDX.D3DCompiler.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed SharpDX.DXGI.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed SharpDX.Direct3D11.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed SharpDX.DirectInput.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed SharpDX.RawInput.dll", Exclude = false)]
// [assembly: Obfuscation(Feature = "embed SharpDX.XInput.dll", Exclude = false)]
// #endif

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("bc6f5b23-4e4b-49e6-b9bd-c090076fd732")]

[assembly: InternalsVisibleTo("SiliconStudio.Assets.CompilerApp.Serializers")]
