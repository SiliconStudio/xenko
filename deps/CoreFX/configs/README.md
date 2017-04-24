# Description

The goal of this configuration file is to make MSBuild think you are compiling against the PCL but actually you are compiling against the set of reference assemblies supplied in this project.

To do that we supply our own version of the Portable C# targets. It is a copy of the Microsoft supplied one at **C:\Program Files (x86)\MSBuild\Microsoft\Portable\v5.0\Microsoft.Portable.CSharp.targets** where we added in addition to the existing imports some definitions for identifying our .NET target, and we override **\_CheckForInvalidTargetFrameworkProfile** to do nothing since CoreCLR has no profile.

# Implementation notes

Before including Microsoft.Portable.Common.targets we define the Framework version to 4.6, as the 5.0 folder from Microsoft is empty and would not include our assemblies. After inclusion, we update the version to 5.0 as expected.

If you find a better way to set this up without having to copy the Microsoft supplied MS Build targets, feel free to send a pull request.
