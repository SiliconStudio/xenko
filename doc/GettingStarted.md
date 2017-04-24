# Building instructions

## Prerequisites

1. [Git LFS](https://git-lfs.github.com/)
2. Visual Studio 2013 with Update 4 (only needed for C++ projects, will be updated later)
3. Visual Studio 2015 (with SDK if you want Visual Studio package to compile (installable from VS2015 itself))
4. [FBX SDK 2016.1.1 VS2015](http://usa.autodesk.com/adsk/servlet/pc/item?id=24735038&siteID=123112)
5. [.NET Framework 4.5.2 Developer Pack](https://www.microsoft.com/en-us/download/details.aspx?id=42637)

## Building

1. Clone Xenko with LFS: `git lfs clone git@git.xenko.com:xenko/Xenko-Runtime.git`
2. Set the *SiliconStudioXenkoDir* environment variable to point to your `<XenkoDir>`
3. Open `<XenkoDir>\build\Xenko.sln` with Visual Studio 2017 and build.
4. Open `<XenkoDir>\samples\XenkoSamples.sln` and play with the samples.
5. Optionally, open and build `Xenko.Android.sln`, `Xenko.iOS.sln`, etc.

## Using the editor

The runtime repository does not contain the editor source code. You can still use the editor from the Binary Release to work on you modified runtime.

If you are subscribed to a Xenko version that includes editor sources, use `git lfs clone git@git.xenko.com:xenko/Xenko-Full.git` instead.