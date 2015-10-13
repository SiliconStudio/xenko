// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.BuildEngine.Tests
{
    class Program
    {
        static void Main()
        {
            //var testCancellation = new TestCancellation();
            //testCancellation.TestCancellationToken();
            //testCancellation.TestCancelCallback();
            //testCancellation.TestCancelPrerequisites();
            TestIO test = new TestIO();
            test.TestInputFromPreviousOutputWithCache();
        }
    }
}
