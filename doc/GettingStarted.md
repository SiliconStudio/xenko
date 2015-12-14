# Getting Started

## Prerequisites

1. Visual Studio 2013 with Update 4 (only needed for C++ projects, will be updated later)
2. Visual Studio 2015 (with SDK if you want Visual Studio package to compile (installable from VS2015 itself))
3. [FBX SDK 2016.1.1 VS2015](http://usa.autodesk.com/adsk/servlet/pc/item?id=24735038&siteID=123112)
4. [.NET Framework 4.5.2 Developer Pack](https://www.microsoft.com/en-us/download/details.aspx?id=42637)

## Build Instructions

1. Clone Xenko with submodules: `git clone --recursive git@github.com:SiliconStudio/xenko.git <XenkoDir>`
2. Set *SiliconStudioXenkoDir* environment variable to point to your `<XenkoDir>`
3. Open `<XenkoDir>\build\Xenko.sln` with Visual Studio 2013, and build.
4. Open `<XenkoDir>\samples\XenkoSamples.sln` and play with the samples.
5. Optionally, open and build `Xenko.Android.sln`, `Xenko.iOS.sln`, etc...

## Using the editor

Since the Editor depends on Telerik and is not currently included in the source release, you can still use the binaries from the Binary Release version.

You can still use the editor from the Binary Release.