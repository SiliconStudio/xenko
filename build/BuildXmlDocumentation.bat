CALL "%VS120COMNTOOLS%VsDevCmd.bat"
msbuild Paradox.build /p:GenerateDoc=true /t:BuildWindows > NUL

