// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.UI.Tests.Events;
using SiliconStudio.Xenko.UI.Tests.Layering;

namespace SiliconStudio.Xenko.UI.Tests
{
    public class Program
    {
        public static void Main()
        {
            var uiElementTest = new UIElementLayeringTests();
            uiElementTest.TestAll();

            var panelTest = new PanelTests();
            panelTest.TestAll();

            var controlTest = new ControlTests();
            controlTest.TestAll();

            var stackPanelTest = new StackPanelTests();
            stackPanelTest.TestAll();

            var canvasTest = new CanvasTests();
            canvasTest.TestAll();

            var contentControlTest = new ContentControlTest();
            contentControlTest.TestAll();

            var eventManagerTest = new EventManagerTests();
            eventManagerTest.TestAll();

            var routedEventArgTest = new RoutedEventArgsTest();
            routedEventArgTest.TestAll();

            var uiElementEventTest = new UIElementEventTests();
            uiElementEventTest.TestAll();

            var gridTest = new GridTests();
            gridTest.TestAll();
        }
    }
}
