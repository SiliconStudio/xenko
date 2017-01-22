using NUnit.Framework;

namespace SiliconStudio.AssemblyProcessor.Tests
{
    public class TestRoslynCodeMerger
    {
        [Test]
        public void TestGenerateRoslynAssemblyLocation()
        {
            string actual = RoslynCodeMerger.GenerateRoslynAssemblyLocation("test.exe.dll.exe");
            string expected = "test.exe.dll.Serializers.dll";
            Assert.That(actual, Is.EqualTo(expected));

            actual = RoslynCodeMerger.GenerateRoslynAssemblyLocation("exe.exe");
            expected = "exe.Serializers.dll";
            Assert.That(actual, Is.EqualTo(expected));

            actual = RoslynCodeMerger.GenerateRoslynAssemblyLocation("dll.dll");
            expected = "dll.Serializers.dll";
            Assert.That(actual, Is.EqualTo(expected));

            actual = RoslynCodeMerger.GenerateRoslynAssemblyLocation("exe.dll");
            expected = "exe.Serializers.dll";
            Assert.That(actual, Is.EqualTo(expected));

            actual = RoslynCodeMerger.GenerateRoslynAssemblyLocation("dll.exe");
            expected = "dll.Serializers.dll";
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}