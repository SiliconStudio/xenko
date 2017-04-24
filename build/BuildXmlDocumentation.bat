CALL "%VS120COMNTOOLS%VsDevCmd.bat"
msbuild Xenko.build /p:GenerateDoc=true /t:BuildWindows > NUL

