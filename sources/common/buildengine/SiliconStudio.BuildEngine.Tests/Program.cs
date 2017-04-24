// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
