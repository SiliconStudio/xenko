// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Presentation.Settings;

namespace SiliconStudio.Presentation.Tests
{
    class ValueSettingsKeys
    {
        public static SettingsValueKey<int> IntValue;
        public static SettingsValueKey<double> DoubleValue;
        public static SettingsValueKey<string> StringValue;

        public static void Initialize()
        {
            IntValue = new SettingsValueKey<int>("Test/Simple/IntValue", 10);
            DoubleValue = new SettingsValueKey<double>("Test/Simple/DoubleValue", 3.14);
            StringValue = new SettingsValueKey<string>("Test/Simple/StringValue", "Test string");
            Console.WriteLine(@"Static settings keys initialized (ValueSettingsKeys)");
        }
    }

    class ListSettingsKeys
    {
        public static SettingsListKey<int> IntList;
        public static SettingsListKey<double> DoubleList;
        public static SettingsListKey<string> StringList;

        public static void Initialize()
        {
            IntList = new SettingsListKey<int>("Test/Lists/IntList", Enumerable.Empty<int>());
            DoubleList = new SettingsListKey<double>("Test/Lists/DoubleList", new[] { 2.0, 6.0, 9.0 });
            StringList = new SettingsListKey<string>("Test/Lists/StringList", new[] { "String 1", "String 2", "String 3" });
            Console.WriteLine(@"Static settings keys initialized (ListSettingsKeys)");
        }
    }

    [TestFixture]
    class TestSettings
    {
        public static Guid SessionGuid = Guid.NewGuid();

        [SetUp]
        public static void InitializeSettings()
        {
            SettingsService.ClearSettings();
        }

        public static string TempPath(string file)
        {
            var dir = Path.Combine(Path.GetTempPath(), SessionGuid.ToString());
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, file);
        }

        [Test]
        public void TestSettingsInitialization()
        {
            ValueSettingsKeys.Initialize();
            Assert.AreEqual(10, ValueSettingsKeys.IntValue.GetValue());
            Assert.AreEqual(3.14, ValueSettingsKeys.DoubleValue.GetValue());
            Assert.AreEqual("Test string", ValueSettingsKeys.StringValue.GetValue());
        }

        [Test]
        public void TestSettingsWrite()
        {
            ValueSettingsKeys.Initialize();
            ValueSettingsKeys.IntValue.SetValue(20);
            ValueSettingsKeys.DoubleValue.SetValue(6.5);
            ValueSettingsKeys.StringValue.SetValue("New string");
            Assert.AreEqual(20, ValueSettingsKeys.IntValue.GetValue());
            Assert.AreEqual(6.5, ValueSettingsKeys.DoubleValue.GetValue());
            Assert.AreEqual("New string", ValueSettingsKeys.StringValue.GetValue());

            ValueSettingsKeys.IntValue.SetValue(30);
            ValueSettingsKeys.DoubleValue.SetValue(9.1);
            ValueSettingsKeys.StringValue.SetValue("Another string");
            Assert.AreEqual(30, ValueSettingsKeys.IntValue.GetValue());
            Assert.AreEqual(9.1, ValueSettingsKeys.DoubleValue.GetValue());
            Assert.AreEqual("Another string", ValueSettingsKeys.StringValue.GetValue());
        }

        [Test]
        public void TestSettingsValueChanged()
        {
            // We use an array to avoid a closure issue (resharper)
            int[] settingsChangedCount = { 0 };

            ValueSettingsKeys.Initialize();
            ListSettingsKeys.Initialize();
            ValueSettingsKeys.IntValue.ChangesValidated += (s, e) => ++settingsChangedCount[0];
            ValueSettingsKeys.DoubleValue.ChangesValidated += (s, e) => ++settingsChangedCount[0];
            ValueSettingsKeys.StringValue.ChangesValidated += (s, e) => ++settingsChangedCount[0];
            ListSettingsKeys.IntList.ChangesValidated += (s, e) => ++settingsChangedCount[0];
            ListSettingsKeys.DoubleList.ChangesValidated += (s, e) => ++settingsChangedCount[0];
            ListSettingsKeys.StringList.ChangesValidated += (s, e) => ++settingsChangedCount[0];

            ValueSettingsKeys.IntValue.SetValue(20);
            ValueSettingsKeys.DoubleValue.SetValue(6.5);
            ValueSettingsKeys.StringValue.SetValue("New string");
            SettingsService.CurrentProfile.ValidateSettingsChanges();
            Assert.AreEqual(3, settingsChangedCount[0]);
            settingsChangedCount[0] = 0;

            var intList = ListSettingsKeys.IntList.GetList();
            var doubleList = ListSettingsKeys.DoubleList.GetList();
            var stringList = ListSettingsKeys.StringList.GetList();

            intList.Add(1);
            doubleList.Remove(2.0);
            stringList.Insert(1, "String 1.5");
            SettingsService.CurrentProfile.ValidateSettingsChanges();
            Assert.AreEqual(3, settingsChangedCount[0]);
            settingsChangedCount[0] = 0;
            
            intList.Add(3);
            doubleList.RemoveAt(0);
            stringList[1] = "String 1.5 Modified";
            SettingsService.CurrentProfile.ValidateSettingsChanges();
            Assert.AreEqual(3, settingsChangedCount[0]);
        }

        [Test]
        public void TestSettingsList()
        {
            ListSettingsKeys.Initialize();
            var intList = ListSettingsKeys.IntList.GetList();
            intList.Add(1);
            intList.Add(3);
            var doubleList = ListSettingsKeys.DoubleList.GetList();
            doubleList.Remove(2.0);
            doubleList.RemoveAt(0);
            var stringList = ListSettingsKeys.StringList.GetList();
            stringList.Insert(1, "String 1.5");
            stringList[2] = "String 2.0";

            intList = ListSettingsKeys.IntList.GetList();
            Assert.That(intList, Is.EquivalentTo(new[] { 1, 3 }));
            doubleList = ListSettingsKeys.DoubleList.GetList();
            Assert.That(doubleList, Is.EquivalentTo(new[] { 9.0 }));
            stringList = ListSettingsKeys.StringList.GetList();
            Assert.That(stringList, Is.EquivalentTo(new[] { "String 1", "String 1.5", "String 2.0", "String 3" }));
        }

        [Test]
        public void TestSettingsSaveAndLoad()
        {
            TestSettingsWrite();
            TestSettingsList();
            SettingsService.SaveSettingsProfile(SettingsService.CurrentProfile, TempPath("TestSettingsSaveAndLoad.txt"));
            SettingsService.LoadSettingsProfile(TempPath("TestSettingsSaveAndLoad.txt"), true);

            Assert.AreEqual(30, ValueSettingsKeys.IntValue.GetValue());
            Assert.AreEqual(9.1, ValueSettingsKeys.DoubleValue.GetValue());
            Assert.AreEqual("Another string", ValueSettingsKeys.StringValue.GetValue());
           
            var intList = ListSettingsKeys.IntList.GetList();
            Assert.That(intList, Is.EquivalentTo(new[] { 1, 3 }));
            var doubleList = ListSettingsKeys.DoubleList.GetList();
            Assert.That(doubleList, Is.EquivalentTo(new[] { 9.0 }));
            var stringList = ListSettingsKeys.StringList.GetList();
            Assert.That(stringList, Is.EquivalentTo(new[] { "String 1", "String 1.5", "String 2.0", "String 3" }));
        }

        const string TestSettingsLoadFileText =
@"!SettingsFile
Settings:
    Test/Lists/DoubleList:
        - 9
    Test/Lists/IntList:
        - 1
        - 3
    Test/Lists/StringList:
        - String 1
        - String 1.5
        - String 2
        - String 3
    Test/Simple/DoubleValue: 25.0
    Test/Simple/IntValue: 45
    Test/Simple/StringValue: 07/25/2004 18:18:00";

        [Test]
        public void TestSettingsLoad()
        {
            using (var writer = new StreamWriter(TempPath("TestSettingsLoad.txt")))
            {
                writer.Write(TestSettingsLoadFileText);
            }
            SettingsService.LoadSettingsProfile(TempPath("TestSettingsLoad.txt"), true);

            ValueSettingsKeys.Initialize();
            ListSettingsKeys.Initialize();
            Assert.AreEqual(45, ValueSettingsKeys.IntValue.GetValue());
            Assert.AreEqual(25.0, ValueSettingsKeys.DoubleValue.GetValue());
            Assert.AreEqual(new DateTime(2004, 7, 25, 18, 18, 00).ToString(CultureInfo.InvariantCulture), ValueSettingsKeys.StringValue.GetValue());
            var intList = ListSettingsKeys.IntList.GetList();
            Assert.That(intList, Is.EquivalentTo(new[] { 1, 3 }));
            var doubleList = ListSettingsKeys.DoubleList.GetList();
            Assert.That(doubleList, Is.EquivalentTo(new[] { 9.0 }));
            var stringList = ListSettingsKeys.StringList.GetList();
            Assert.That(stringList, Is.EquivalentTo(new[] { "String 1", "String 1.5", "String 2", "String 3" }));
        }

        const string TestSettingsValueChangedOnLoadText =
@"!SettingsFile
Settings:
    Test/Lists/DoubleList: # Same as default
        - 2.0
        - 6.0
        - 9.0
    Test/Lists/IntList:
        - 1
    Test/Simple/DoubleValue: 3.14 # Same as default
    Test/Simple/IntValue: 45 # Different from default
    # String value unset";
        
        [Test]
        public void TestSettingsValueChangedOnLoad()
        {
            ValueSettingsKeys.Initialize();
            ListSettingsKeys.Initialize();
            using (var writer = new StreamWriter(TempPath("TestSettingsValueChangedOnLoadText.txt")))
            {
                writer.Write(TestSettingsValueChangedOnLoadText);
            }

            int intValueChangeCount = 0;
            int doubleValueChangeCount = 0;
            int stringValueChangeCount = 0;
            int intListChangeCount = 0;
            int doubleListChangeCount = 0;
            int stringListChangeCount = 0;
            ValueSettingsKeys.IntValue.ChangesValidated += (s, e) => ++intValueChangeCount;
            ValueSettingsKeys.DoubleValue.ChangesValidated += (s, e) => ++doubleValueChangeCount;
            ValueSettingsKeys.StringValue.ChangesValidated += (s, e) => ++stringValueChangeCount;
            ListSettingsKeys.IntList.ChangesValidated += (s, e) => ++intListChangeCount;
            ListSettingsKeys.DoubleList.ChangesValidated += (s, e) => ++doubleListChangeCount;
            ListSettingsKeys.StringList.ChangesValidated += (s, e) => ++stringListChangeCount;

            SettingsService.LoadSettingsProfile(TempPath("TestSettingsValueChangedOnLoadText.txt"), true);
            SettingsService.CurrentProfile.ValidateSettingsChanges();

            Assert.AreEqual(1, intValueChangeCount);
            Assert.AreEqual(0, doubleValueChangeCount);
            Assert.AreEqual(0, stringValueChangeCount);
            Assert.AreEqual(1, intListChangeCount);
            Assert.AreEqual(0, doubleListChangeCount);
            Assert.AreEqual(0, stringListChangeCount);
        }

        const string TestSettingsLoadWrongTypeFileText =
@"!SettingsFile
Settings:
    Test/Lists/DoubleList:
        - String 1
        - String 2    
    Test/Lists/IntList: This is a string
    Test/Simple/DoubleValue: This is a string
    Test/Simple/IntValue:
        - String 1
        - String 2";

        [Test]
        public void TestSettingsLoadWrongType()
        {
            using (var writer = new StreamWriter(TempPath("TestSettingsLoadWrongType.txt")))
            {
                writer.Write(TestSettingsLoadWrongTypeFileText);
            }
            SettingsService.LoadSettingsProfile(TempPath("TestSettingsLoadWrongType.txt"), true);

            ValueSettingsKeys.Initialize();
            ListSettingsKeys.Initialize();
            Assert.AreEqual(ValueSettingsKeys.IntValue.DefaultValue, ValueSettingsKeys.IntValue.GetValue());
            Assert.AreEqual(ValueSettingsKeys.DoubleValue.DefaultValue, ValueSettingsKeys.DoubleValue.GetValue());
            Assert.AreEqual(ValueSettingsKeys.StringValue.DefaultValue, ValueSettingsKeys.StringValue.GetValue());
            var intList = ListSettingsKeys.IntList.GetList();
            Assert.That(intList, Is.EquivalentTo(ListSettingsKeys.IntList.DefaultValue));
            var doubleList = ListSettingsKeys.DoubleList.GetList();
            Assert.That(doubleList, Is.EquivalentTo(ListSettingsKeys.DoubleList.DefaultValue));
        }

        const string TestSettingsFileModifiedText1 =
@"!SettingsFile
Settings:
    Test/Simple/IntValue: 55";

        const string TestSettingsFileModifiedText2 =
@"!SettingsFile
Settings:
    Test/Simple/IntValue: 75";

        [Test]
        public void TestSettingsFileModified()
        {
            // NUnit does not support async tests so lets wrap this task into a synchronous operation
            var task = Task.Run(async () =>
                {
                    var tcs = new TaskCompletionSource<int>();
                    EventHandler<FileModifiedEventArgs> settingsModified = (s, e) => e.ReloadFile = true;
                    EventHandler<SettingsFileLoadedEventArgs> settingsLoaded = (s, e) => tcs.SetResult(0);
                    try
                    {
                        using (var writer = new StreamWriter(TempPath("TestSettingsFileModified.txt")))
                        {
                            writer.Write(TestSettingsFileModifiedText1);
                        }
                        SettingsService.LoadSettingsProfile(TempPath("TestSettingsFileModified.txt"), true);
                        SettingsService.CurrentProfile.MonitorFileModification = true;
                        SettingsService.CurrentProfile.FileModified += settingsModified;
                        ValueSettingsKeys.Initialize();
                        ListSettingsKeys.Initialize();
                        Assert.AreEqual(55, ValueSettingsKeys.IntValue.GetValue());

                        SettingsService.SettingsFileLoaded += settingsLoaded;

                        using (var writer = new StreamWriter(TempPath("TestSettingsFileModified.txt")))
                        {
                            writer.Write(TestSettingsFileModifiedText2);
                        }

                        // Gives some time to the file watcher to awake.
                        await tcs.Task;

                        Assert.AreEqual(75, ValueSettingsKeys.IntValue.GetValue());
                        SettingsService.SettingsFileLoaded -= settingsLoaded;
                    }
                    catch
                    {
                        SettingsService.SettingsFileLoaded -= settingsLoaded;
                    }
                });

            // Block while the task has not ended to ensure that no other test will start before this one ends.
            task.Wait();
        }
    }
}
