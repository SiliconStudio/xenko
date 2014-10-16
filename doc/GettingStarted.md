# Getting Started

## Prerequisites

1. Visual Studio 2013 with Update 3
2. Visual Studio 2013 SDK (if you want to build the Visual Studio Package)
3. FBX SDK 2015.1 VS2013, available from http://autodesk.com/fbx

## Build Instructions

1. Clone Paradox with submodules: `git clone --recursive git@github.com:SiliconStudio/paradox.git <ParadoxDir>`
2. Set *SiliconStudioParadoxDir* environment variable to point to your `<ParadoxDir>`
3. Open `<ParadoxDir>\build\Paradox.sln` with Visual Studio 2013, and build.
4. Open `<ParadoxDir>\samples\ParadoxSamples.sln` and play with the samples.
5. Optionally, open and build `Paradox.Android.sln`, `Paradox.iOS.sln`, etc...

## Using the editor

Since the Editor depends on Telerik and is not currently included in the source release, you can still use the binaries from the Binary Release version.

You can still use the editor from the Binary Release.