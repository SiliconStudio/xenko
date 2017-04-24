![Xenko](https://xenko.com/images/external/xenko-logo-side.png)
=======

Welcome to the Xenko source code repository!

Xenko is an open-source C# game engine for realistic rendering and VR. 
The engine is highly modular and aims at giving game makers more flexibility in their development.
Xenko comes with an editor that allows you create and manage the content of your games or applications in a visual and intuitive way.

![Xenko Editor](https://xenko.com/images/external/script-editor.png)

To learn more about Xenko, visit [xenko.com](https://xenko.com/).

## License

Before downloading, using or contributing to Xenko, please carefully read the license agreements below. 

By downloading or using files in this repository, you affirm that you have read, understood and agreed to the terms below.
* [End User License Agreement](LICENSE.md)
* [Contribution License Agreement](doc/ContributorLicenseAgreement.md)

## Documentation

Find explanations and information about Xenko:
* [Xenko Manual](http://doc.xenko.com/latest/manual)
* [API Reference](http://doc.xenko.com/latest/api/SiliconStudio.Assets.html)
* [Release Notes](http://doc.xenko.com/latest/manual)

## Community

Ask for help or report issues:
* [Chat with the community](https://gitter.im/SiliconStudio/xenko?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
* [Ask and answer questions](http://answers.xenko.com/)
* [Discuss topics on our forums](http://forums.xenko.com/)
* [Report engine issues](https://github.com/SiliconStudio/xenko/issues)

## Building from source

### Prerequisites

1. [Git](https://git-scm.com/downloads) with Git LFS option, or install [Git LFS](https://git-lfs.github.com/) separately.
2. [Visual Studio 2017](https://www.visualstudio.com/downloads/) with the following workloads:
  * .NET desktop development
  * Desktop development with C++ (with C++/CLI and VC++ 2015.3 v140 toolset optional components)
3. [FBX SDK 2017.0.1 VS2015](http://usa.autodesk.com/adsk/servlet/pc/item?siteID=123112&id=25408427)

### Build Xenko

1. Clone Xenko with LFS: `git lfs clone git@github.com:SiliconStudio/xenko.git`
2. Set *SiliconStudioXenkoDir* environment variable to point to your `<XenkoDir>`
3. Open `<XenkoDir>\build\Xenko.sln` with Visual Studio 2017, and build.
4. Open `<XenkoDir>\samples\XenkoSamples.sln` and play with the samples.
5. Optionally, open and build `Xenko.Android.sln`, `Xenko.iOS.sln`, etc...
