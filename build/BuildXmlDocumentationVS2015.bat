CALL "%VS140COMNTOOLS%VsDevCmd.bat"
msbuild Xenko.build /p:SiliconStudioGenerateDoc=true /t:BuildWindows > NUL

