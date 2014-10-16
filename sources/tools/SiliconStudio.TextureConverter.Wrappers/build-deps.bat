call "C:\Program Files (x86)\Microsoft Visual Studio 11.0\vc\vcvarsall.bat" x86
msbuild SiliconStudio.TextureConverter.Wrappers.sln /Property:Configuration=Debug;Platform="Mixed Platforms"
msbuild SiliconStudio.TextureConverter.Wrappers.sln /Property:Configuration=Release;Platform="Mixed Platforms"