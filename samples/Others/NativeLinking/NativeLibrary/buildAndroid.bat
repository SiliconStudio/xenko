@echo OFF
setlocal
set NDK_PATH=%ANDROID_NDK_PATH%
IF [%NDK_PATH%]==[] echo "Please set the variable 'ANDROID_NDK_PATH' to the installation folder of the Android NDK."

call ndk-build clean
call ndk-build
xcopy "libs\*" "..\NativeLibs\Android\" /s /i /y /q
echo "libs files have been copied to ../NativeLibs/Android"