using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.FixProjectReferences;

namespace SiliconStudio.Xenko.Code.Tests
{
    /// <summary>
    /// Test class that check if there is some copy-local references between Xenko projects.
    /// </summary>
    [TestFixture]
    public class FixProjectReferenceTests
    {
        [Test, Category("Code")]
        public void TestCopyLocals()
        {
            var log = new LoggerResult();
            log.ActivateLog(LogMessageType.Error);
            if (!FixProjectReference.ProcessCopyLocals(log, @"..\..\build\Xenko.sln", false))
                Assert.Fail($"Found some dependencies between Xenko projects that are not set to CopyLocal=false; please run SiliconStudio.Xenko.FixProjectReferences:\r\n{log.ToText()}");
        }
    }
}
