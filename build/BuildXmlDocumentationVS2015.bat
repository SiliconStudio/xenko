CALL "%VS140COMNTOOLS%VsDevCmd.bat"
msbuild Xenko.build /p:GenerateDoc=true /t:BuildWindows > NUL

