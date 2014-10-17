@echo off
REM Parameters
REM %1 server IP
REM %2 server listening port
REM %3 build number
REM %4 device serial
REM %5 test name
REM %6 Paradox sdk dir

REM kill previous instance (does not work on android prior to 3.0 - Honeycomb)
adb -s %4 am shell force-stop Paradox.Graphics.RegressionTests

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe %ParadoxSdkDir%\sources\engine\SiliconStudio.Paradox.Graphics.RegressionTests\SiliconStudio.Paradox.Graphics.RegressionTests.Android.csproj /p:SolutionName=Paradox.Android /p:SolutionDir=%ParadoxSdkDir%\ /p:Configuration=Release /t:Install /p:AdbTarget="-s %4"

REM install the package -> should be done by bamboo too?
REM adb -s %4 -d install -r %6\Bin\Android-AnyCPU-OpenGLES\Paradox.Graphics.RegressionTests-Signed.apk

REM run it
adb -s %4 shell am start -a android.intent.action.MAIN -n Paradox.Graphics.RegressionTests/siliconstudio.paradox.graphics.regressiontests.TestRunner -e PARADOX_SERVER_IP %1 -e PARADOX_SERVER_PORT %2 -e PARADOX_BUILD_NUMBER %3 -e PARADOX_DEVICE_SERIAL %4 -e PARADOX_TEST_NAME %5