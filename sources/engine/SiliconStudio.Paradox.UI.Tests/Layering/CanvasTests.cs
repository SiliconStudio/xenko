// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using NUnit.Framework;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SiliconStudio.Paradox.UI.Tests.Layering
{
    /// <summary>
    /// Series of tests for <see cref="Canvas"/>
    /// </summary>
    public class CanvasTests : Canvas
    {
        private Random rand;

        /// <summary>
        /// launch all the tests of <see cref="CanvasTests"/>
        /// </summary>
        public void TestAll()
        {
            Initialize();
            TestProperties();
            TestCollapseOverride();
            TestBasicInvalidations();
            TestMeasureOverrideRelative();
            TestArrangeOverrideRelative();
            TestMeasureOverrideAbsolute();
            TestArrangeOverrideAbsolute();
        }

        [TestFixtureSetUp]
        public void Initialize()
        {
            // create a rand variable changing from a test to the other
            rand = new Random(DateTime.Now.Millisecond);
        }

        private void ResetState()
        {
            DependencyProperties = new PropertyContainer(this);
            Children.Clear();
            InvalidateArrange();
            InvalidateMeasure();
        }

        /// <summary>
        /// Test the <see cref="Canvas"/> properties
        /// </summary>
        [Test]
        public void TestProperties()
        {
            var newElement = new Canvas();

            // test default values
            Assert.AreEqual(float.NaN, newElement.DependencyProperties.Get(RelativeSizePropertyKey).X);
            Assert.AreEqual(float.NaN, newElement.DependencyProperties.Get(RelativeSizePropertyKey).Y);
            Assert.AreEqual(float.NaN, newElement.DependencyProperties.Get(RelativeSizePropertyKey).Z);
            Assert.AreEqual(Vector3.Zero, newElement.DependencyProperties.Get(RelativePositionPropertyKey));
            Assert.AreEqual(Vector3.Zero, newElement.DependencyProperties.Get(AbsolutePositionPropertyKey));
            Assert.AreEqual(Vector3.Zero, newElement.DependencyProperties.Get(PinOriginPropertyKey));

            // test pin origin validator
            newElement.DependencyProperties.Set(PinOriginPropertyKey, new Vector3(-1, -1, -1));
            Assert.AreEqual(Vector3.Zero, newElement.DependencyProperties.Get(PinOriginPropertyKey));
            newElement.DependencyProperties.Set(PinOriginPropertyKey, new Vector3(2, 2, 2));
            Assert.AreEqual(Vector3.One, newElement.DependencyProperties.Get(PinOriginPropertyKey));
            newElement.DependencyProperties.Set(PinOriginPropertyKey, new Vector3(0.5f, 0.5f, 0.5f));
            Assert.AreEqual(new Vector3(0.5f, 0.5f, 0.5f), newElement.DependencyProperties.Get(PinOriginPropertyKey));

            // test relative size validator
            newElement.DependencyProperties.Set(RelativeSizePropertyKey, new Vector3(0.5f, 0.5f, 0.5f));
            Assert.AreEqual(new Vector3(0.5f, 0.5f, 0.5f), newElement.DependencyProperties.Get(RelativeSizePropertyKey));
            newElement.DependencyProperties.Set(RelativeSizePropertyKey, new Vector3(2.5f, 3.5f, 4.5f));
            Assert.AreEqual(new Vector3(2.5f, 3.5f, 4.5f), newElement.DependencyProperties.Get(RelativeSizePropertyKey));
            Assert.Throws<InvalidOperationException>(() => newElement.DependencyProperties.Set(RelativeSizePropertyKey, new Vector3(-2.5f, 3.5f, 4.5f)));
            Assert.Throws<InvalidOperationException>(() => newElement.DependencyProperties.Set(RelativeSizePropertyKey, new Vector3(2.5f, -3.5f, 4.5f)));
            Assert.Throws<InvalidOperationException>(() => newElement.DependencyProperties.Set(RelativeSizePropertyKey, new Vector3(2.5f, 3.5f, -0.1f)));
        }

        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            var canvas = new Canvas();
            var child = new Canvas();
            canvas.Children.Add(child);

            // - test the properties that are supposed to invalidate the object measurement
            UIElementLayeringTests.TestMeasureInvalidation(canvas, () => child.DependencyProperties.Set(PinOriginPropertyKey, new Vector3(0.1f, 0.2f, 0.3f)));
            UIElementLayeringTests.TestMeasureInvalidation(canvas, () => child.DependencyProperties.Set(RelativePositionPropertyKey, new Vector3(1f, 2f, 3f)));
            UIElementLayeringTests.TestMeasureInvalidation(canvas, () => child.DependencyProperties.Set(AbsolutePositionPropertyKey, new Vector3(1f, 2f, 3f)));
            UIElementLayeringTests.TestMeasureInvalidation(canvas, () => child.DependencyProperties.Set(RelativeSizePropertyKey, new Vector3(1f, 2f, 3f)));
        }
        
        /// <summary>
        /// Test the function <see cref="Canvas.MeasureOverride"/>
        /// </summary>
        [Test]
        public void TestMeasureOverrideRelative()
        {
            ResetState();

            // check that desired size is null if no children
            Measure(1000 * rand.NextVector3());
            Assert.AreEqual(Vector3.Zero, DesiredSize);

            var child = new MeasureValidator();
            child.DependencyProperties.Set(RelativeSizePropertyKey, new Vector3(0.2f,0.3f,0.4f));
            Children.Add(child);
            
            child.ExpectedMeasureValue = new Vector3(2,3,4);
            child.ReturnedMeasuredValue = new Vector3(4,3,2);
            Measure(10 * Vector3.One);
            Assert.AreEqual(new Vector3(20f, 10f, 5f), DesiredSize);
        }

        /// <summary>
        /// Test for the function <see cref="Canvas.ArrangeOverride"/>
        /// </summary>
        [Test]
        public void TestArrangeOverrideRelative()
        {
            ResetState();

            DepthAlignment = DepthAlignment.Stretch;

            // test that arrange set render size to provided size when there is no children
            var providedSize = 1000 * rand.NextVector3();
            var providedSizeWithoutMargins = CalculateSizeWithoutThickness(ref providedSize, ref MarginInternal);
            Measure(providedSize);
            Arrange(providedSize, false);
            Assert.AreEqual(providedSizeWithoutMargins, RenderSize);

            ResetState();

            DepthAlignment = DepthAlignment.Stretch;
            
            var child = new ArrangeValidator();
            child.DependencyProperties.Set(RelativeSizePropertyKey, new Vector3(0.2f, 0.3f, 0.4f));
            child.DependencyProperties.Set(PinOriginPropertyKey, new Vector3(0f, 0.5f, 1f));
            child.DependencyProperties.Set(RelativePositionPropertyKey, new Vector3(0.2f, 0.4f, 0.6f));
            Children.Add(child);

            child.ReturnedMeasuredValue = 2 * new Vector3(2, 6, 12);
            child.ExpectedArrangeValue = new Vector3(4, 12, 18);
            providedSize = new Vector3(10, 20, 30);
            Measure(providedSize);
            Arrange(providedSize, false);
            Assert.AreEqual(Matrix.Translation(2f-5f,8f-6f-10f,18f-18f-15f), child.DependencyProperties.Get(PanelArrangeMatrixPropertyKey));
        }
        
        /// <summary>
        /// Test <see cref="Canvas.CollapseOverride"/>
        /// </summary>
        [Test]
        public void TestCollapseOverride()
        {
            ResetState();

            // create two children
            var childOne = new StackPanelTests();
            var childTwo = new StackPanelTests();

            // set fixed size to the children
            childOne.Width = rand.NextFloat();
            childOne.Height = rand.NextFloat();
            childOne.Depth = rand.NextFloat();
            childTwo.Width = 10 * rand.NextFloat();
            childTwo.Height = 20 * rand.NextFloat();
            childTwo.Depth = 30 * rand.NextFloat();

            // add the children to the stack panel 
            Children.Add(childOne);
            Children.Add(childTwo);

            // arrange the stack panel and check children size
            Arrange(1000 * rand.NextVector3(), true);
            Assert.AreEqual(Vector3.Zero, childOne.RenderSize);
            Assert.AreEqual(Vector3.Zero, childTwo.RenderSize);
        }

        /// <summary>
        /// Test the function <see cref="Canvas.MeasureOverride"/> with absolute position
        /// </summary>
        [Test]
        public void TestMeasureOverrideAbsolute()
        {
            ResetState();

            // check that desired size is null if no children
            Measure(1000 * rand.NextVector3());
            Assert.AreEqual(Vector3.Zero, DesiredSize);

            var child = new MeasureValidator();
            Children.Add(child);
            child.Margin = Thickness.UniformCuboid(10);

            // check canvas desired size and child provided size with one child out of the available zone
            var availableSize = new Vector3(100, 200, 300);
            var childDesiredSize = new Vector3(30, 80, 130);

            var pinOrigin = Vector3.Zero;
            TestOutOfBounds(child, childDesiredSize, new Vector3(0, 80, 130), new Vector3(-1, 100, 150), pinOrigin, availableSize, new Vector3(49, 200, 300));
            TestOutOfBounds(child, childDesiredSize, new Vector3(0, 80, 130), new Vector3(101, 100, 150), pinOrigin, availableSize, new Vector3(151, 200, 300));
            TestOutOfBounds(child, childDesiredSize, new Vector3(30, 0, 130), new Vector3(50, -1, 150), pinOrigin, availableSize, new Vector3(100, 99, 300));
            TestOutOfBounds(child, childDesiredSize, new Vector3(30, 0, 130), new Vector3(50, 201, 150), pinOrigin, availableSize, new Vector3(100, 301, 300));
            TestOutOfBounds(child, childDesiredSize, new Vector3(30, 80,  0), new Vector3(50, 100, -1), pinOrigin, availableSize, new Vector3(100, 200, 149));
            TestOutOfBounds(child, childDesiredSize, new Vector3(30, 80,  0), new Vector3(50, 100, 301), pinOrigin, availableSize, new Vector3(100, 200, 451));

            pinOrigin = Vector3.One;
            TestOutOfBounds(child, childDesiredSize, new Vector3(0, 80, 130), new Vector3(-1, 100, 150), pinOrigin, availableSize, new Vector3(0, 100, 150));
            TestOutOfBounds(child, childDesiredSize, new Vector3(0, 80, 130), new Vector3(101, 100, 150), pinOrigin, availableSize, new Vector3(101, 100, 150));
            TestOutOfBounds(child, childDesiredSize, new Vector3(30, 0, 130), new Vector3(50, -1, 150), pinOrigin, availableSize, new Vector3(50, 0, 150));
            TestOutOfBounds(child, childDesiredSize, new Vector3(30, 0, 130), new Vector3(50, 201, 150), pinOrigin, availableSize, new Vector3(50, 201, 150));
            TestOutOfBounds(child, childDesiredSize, new Vector3(30, 80,  0), new Vector3(50, 100, -1), pinOrigin, availableSize, new Vector3(50, 100, 0));
            TestOutOfBounds(child, childDesiredSize, new Vector3(30, 80,  0), new Vector3(50, 100, 301), pinOrigin, availableSize, new Vector3(50, 100, 301));

            pinOrigin = 0.5f * Vector3.One;
            TestOutOfBounds(child, childDesiredSize, new Vector3(0, 180, 280), new Vector3(-1, 100, 150), pinOrigin, availableSize, new Vector3(24, 150, 225));
            TestOutOfBounds(child, childDesiredSize, new Vector3(0, 180, 280), new Vector3(101, 100, 150), pinOrigin, availableSize, new Vector3(126, 150, 225));
            TestOutOfBounds(child, childDesiredSize, new Vector3(80, 0, 280), new Vector3(50, -1, 150), pinOrigin, availableSize, new Vector3(75, 49, 225));
            TestOutOfBounds(child, childDesiredSize, new Vector3(80, 0, 280), new Vector3(50, 201, 150), pinOrigin, availableSize, new Vector3(75, 251, 225));
            TestOutOfBounds(child, childDesiredSize, new Vector3(80, 180,  0), new Vector3(50, 100, -1), pinOrigin, availableSize, new Vector3(75, 150, 74));
            TestOutOfBounds(child, childDesiredSize, new Vector3(80, 180,  0), new Vector3(50, 100, 301), pinOrigin, availableSize, new Vector3(75, 150, 376));

            // check canvas desired size and child provided size with one child in the available zone
            var position = availableSize / 2;
            TestOutOfBounds(child, childDesiredSize, new Vector3(30, 80, 130), position, Vector3.Zero, availableSize, new Vector3(100, 200, 300));
            TestOutOfBounds(child, childDesiredSize, new Vector3(30, 80, 130), position, Vector3.One, availableSize, new Vector3(50, 100, 150));
            TestOutOfBounds(child, childDesiredSize, new Vector3(80, 180, 280), position, 0.5f * Vector3.One, availableSize, new Vector3(75, 150, 225));

            // check that canvas desired size with several children
            ResetState();
            var child1 = new CanvasTests();
            var child2 = new CanvasTests();
            var child3 = new CanvasTests();
            Children.Add(child1);
            Children.Add(child2);
            Children.Add(child3);
            child1.Margin = new Thickness(10, 20, 30, 40, 50, 60);
            child2.Margin = new Thickness(60, 50, 40, 30, 20, 10);
            child3.Margin = new Thickness(1, 2, 3, 4, 5, 6);
            child1.Width = 100;
            child1.Height = 200;
            child1.Depth = 300;
            child2.Width = 10;
            child2.Height = 20;
            child2.Depth = 30;
            child3.Width = 300;
            child3.Height = 200;
            child3.Depth = 100;
            child1.DependencyProperties.Set(AbsolutePositionPropertyKey, new Vector3(1000, 1100, 1200));
            child1.DependencyProperties.Set(PinOriginPropertyKey, new Vector3(0, 0, 1));
            child2.DependencyProperties.Set(AbsolutePositionPropertyKey, new Vector3(1050, 1150, 1200));
            child2.DependencyProperties.Set(PinOriginPropertyKey, new Vector3(1, 1, 0));
            child3.DependencyProperties.Set(AbsolutePositionPropertyKey, new Vector3(500, 600, 700));
            child3.DependencyProperties.Set(PinOriginPropertyKey, new Vector3(0.5f, 0.5f, 0.5f));
            Measure(Vector3.Zero);
            Assert.AreEqual(new Vector3(1150, 1370, 1280), DesiredSize);
        }

        private void TestOutOfBounds(MeasureValidator child, Vector3 childDesiredSize, Vector3 childExpectedValue, Vector3 pinPosition, Vector3 pinOrigin, Vector3 availableSize, Vector3 expectedSize)
        {
            child.ExpectedMeasureValue = childExpectedValue;
            child.ReturnedMeasuredValue = childDesiredSize;
            child.DependencyProperties.Set(AbsolutePositionPropertyKey, pinPosition);
            child.DependencyProperties.Set(PinOriginPropertyKey, pinOrigin);
            Measure(availableSize);
            Assert.AreEqual(expectedSize, DesiredSize);
        }

        /// <summary>
        /// Test for the function <see cref="Canvas.ArrangeOverride"/> with absolute position
        /// </summary>
        [Test]
        public void TestArrangeOverrideAbsolute()
        {
            ResetState();

            DepthAlignment = DepthAlignment.Stretch;

            // test that arrange set render size to provided size when there is no children
            var providedSize = 1000 * rand.NextVector3();
            var providedSizeWithoutMargins = CalculateSizeWithoutThickness(ref providedSize, ref MarginInternal);
            Measure(providedSize);
            Arrange(providedSize, false);
            Assert.AreEqual(providedSizeWithoutMargins, RenderSize);

            ResetState();

            // Create and add children
            for (var i = 0; i < 6; i++)
                Children.Add(new ArrangeValidator());

            // set an available size
            var availablesizeWithMargins = 1000 * rand.NextVector3();
            var availableSizeWithoutMargins = CalculateSizeWithoutThickness(ref availablesizeWithMargins, ref MarginInternal);

            for (var i = 0; i < Children.Count; ++i)
            {
                var child = (ArrangeValidator)Children[i];

                // set children margins
                child.Margin = rand.NextThickness(10, 11, 12, 13, 14, 15);

                // set pin positions and origins
                child.DependencyProperties.Set(AbsolutePositionPropertyKey, 100 * rand.NextVector3());
                child.DependencyProperties.Set(PinOriginPropertyKey, rand.NextVector3());

                // set last last child pin at 0
                if (i == Children.Count - 2)
                {
                    child.DependencyProperties.Set(AbsolutePositionPropertyKey, Vector3.Zero);
                    child.DependencyProperties.Set(PinOriginPropertyKey, Vector3.Zero);
                }

                // set last child pin max
                if (i == Children.Count - 1)
                {
                    child.DependencyProperties.Set(AbsolutePositionPropertyKey, availableSizeWithoutMargins);
                    child.DependencyProperties.Set(PinOriginPropertyKey, Vector3.One);
                }

                // set the arrange validator values
                child.ReturnedMeasuredValue = 1000 * rand.NextVector3();
                var returnedWithMargin = CalculateSizeWithThickness(ref child.ReturnedMeasuredValue, ref child.MarginInternal);

                // calculate the arrange supposed size
                var pinOrigin = child.DependencyProperties.Get(PinOriginPropertyKey);
                var pinPosition = child.DependencyProperties.Get(AbsolutePositionPropertyKey);
                var childAvailableSize = Vector3.Zero;

                if (pinPosition.X >= 0 && pinPosition.X <= availableSizeWithoutMargins.X
                    && pinPosition.Y >= 0 && pinPosition.Y <= availableSizeWithoutMargins.Y
                    && pinPosition.Z >= 0 && pinPosition.Z <= availableSizeWithoutMargins.Z)
                {
                    var availableBeforeElement = Vector3.Demodulate(pinPosition, pinOrigin);
                    var availableAfterElement = Vector3.Demodulate(availableSizeWithoutMargins - pinPosition, Vector3.One - pinOrigin);

                    for (var dim = 0; dim < 3; dim++)
                    {
                        if (pinOrigin[dim] == 0.0f)
                            childAvailableSize[dim] = availableAfterElement[dim];
                        else if (pinOrigin[dim] == 1.0f)
                            childAvailableSize[dim] = availableBeforeElement[dim];
                        else
                            childAvailableSize[dim] = Math.Min(availableBeforeElement[dim], availableAfterElement[dim]);
                    }
                }
                var expectedVectorSizeWithMargin = new Vector3(
                    Math.Min(childAvailableSize.X, returnedWithMargin.X),
                    Math.Min(childAvailableSize.Y, returnedWithMargin.Y),
                    Math.Min(childAvailableSize.Z, returnedWithMargin.Z));
                var expectedVectorSizeWithoutMargin = CalculateSizeWithoutThickness(ref expectedVectorSizeWithMargin, ref child.MarginInternal);

                child.ExpectedArrangeValue = expectedVectorSizeWithoutMargin;
            }

            // Measure the stack
            Measure(availablesizeWithMargins);
            Arrange(availablesizeWithMargins, false);

            // checks the stack arranged size
            Assert.AreEqual(availableSizeWithoutMargins, RenderSize);

            // Checks the children arrange matrix
            for (int i = 0; i < Children.Count; i++)
            {
                var pinPosition = Children[i].DependencyProperties.Get(AbsolutePositionPropertyKey);
                var pinOrigin = Children[i].DependencyProperties.Get(PinOriginPropertyKey);
                var childOffsets = (pinPosition - Vector3.Modulate(pinOrigin, Children[i].RenderSize)) - RenderSize / 2;
                Assert.AreEqual(Matrix.Translation(childOffsets), Children[i].DependencyProperties.Get(PanelArrangeMatrixPropertyKey));
            }
        }

        /// <summary>
        /// Test the function <see cref="Canvas.ComputeAbsolutePinPosition"/>
        /// </summary>
        [Test]
        public void TestComputeAbsolutePinPosition()
        {
            var child = new Button();

            // directly set the values
            var parentSize = new Vector3(2);
            child.SetCanvasRelativePosition(new Vector3(float.NaN));
            child.SetCanvasAbsolutePosition(new Vector3(-1.5f, 0, 1.5f));
            Assert.AreEqual(child.GetCanvasAbsolutePosition(), ComputeAbsolutePinPosition(child, ref parentSize));
            child.SetCanvasAbsolutePosition(new Vector3(float.NaN));
            child.SetCanvasRelativePosition(new Vector3(-1.5f, 0, 1.5f));
            Assert.AreEqual(2*child.GetCanvasRelativePosition(), ComputeAbsolutePinPosition(child, ref parentSize));

            // indirectly set the value
            child.SetCanvasAbsolutePosition(new Vector3(-1.5f, 0, 1.5f));
            child.SetCanvasRelativePosition(new Vector3(float.NaN));
            Assert.AreEqual(child.GetCanvasAbsolutePosition(), ComputeAbsolutePinPosition(child, ref parentSize));
            child.SetCanvasRelativePosition(new Vector3(-1.5f, 0, 1.5f));
            child.SetCanvasAbsolutePosition(new Vector3(float.NaN));
            Assert.AreEqual(2*child.GetCanvasRelativePosition(), ComputeAbsolutePinPosition(child, ref parentSize));

            // indirect/direct mix
            child.SetCanvasAbsolutePosition(new Vector3(-1.5f, float.NaN, 1.5f));
            child.SetCanvasRelativePosition(new Vector3(float.NaN, 1, float.NaN));
            Assert.AreEqual(new Vector3(-1.5f, 2, 1.5f), ComputeAbsolutePinPosition(child, ref parentSize));
            child.SetCanvasRelativePosition(new Vector3(-1.5f, float.NaN, 1.5f));
            child.SetCanvasAbsolutePosition(new Vector3(float.NaN, 1, float.NaN));
            Assert.AreEqual(new Vector3(-3f, 1, 3f), ComputeAbsolutePinPosition(child, ref parentSize));

            // infinite values
            parentSize = new Vector3(float.PositiveInfinity);
            child.SetCanvasRelativePosition(new Vector3(-1.5f, 0, 1.5f));
            Utilities.AreExactlyEqual(new Vector3(float.NegativeInfinity, 0f, float.PositiveInfinity), ComputeAbsolutePinPosition(child, ref parentSize));
        }

        /// <summary>
        /// Test the function <see cref="Canvas.ComputeAvailableSize"/>.
        /// </summary>
        [Test]
        public void TestComputeAvailableSize()
        {
            var child = new Button();
            child.SetCanvasPinOrigin(new Vector3(0, 0.5f, 1));

            // tests in the cases position is absolute
            var availableSize = new Vector3(100, 150, 200);
            child.SetCanvasAbsolutePosition(new Vector3(-1, -2, -3));
            Utilities.AreExactlyEqual(new Vector3(0), ComputeAvailableSize(child, availableSize, false));
            child.SetCanvasAbsolutePosition(new Vector3(0, 0, 0));
            Utilities.AreExactlyEqual(new Vector3(100, 0, 0), ComputeAvailableSize(child, availableSize, false));
            child.SetCanvasAbsolutePosition(new Vector3(1, 2, 3));
            Utilities.AreExactlyEqual(new Vector3(99, 4, 3), ComputeAvailableSize(child, availableSize, false));
            child.SetCanvasAbsolutePosition(availableSize);
            Utilities.AreExactlyEqual(new Vector3(0, 0, 200), ComputeAvailableSize(child, availableSize, false));
            child.SetCanvasAbsolutePosition(availableSize + new Vector3(1, 2, 3));
            Utilities.AreExactlyEqual(new Vector3(0), ComputeAvailableSize(child, availableSize, false));

            // tests in the cases position is relative
            child.SetCanvasRelativePosition(new Vector3(-1, -2, -3));
            Utilities.AreExactlyEqual(new Vector3(0), ComputeAvailableSize(child, availableSize, false));
            child.SetCanvasRelativePosition(new Vector3(0, 0, 0));
            Utilities.AreExactlyEqual(new Vector3(100, 0, 0), ComputeAvailableSize(child, availableSize, false));
            child.SetCanvasRelativePosition(new Vector3(0.1f, 0.2f, 0.4f));
            Utilities.AreExactlyEqual(new Vector3(90, 60, 80), ComputeAvailableSize(child, availableSize, false));
            child.SetCanvasRelativePosition(new Vector3(1f));
            Utilities.AreExactlyEqual(new Vector3(0, 0, 200), ComputeAvailableSize(child, availableSize, false));
            child.SetCanvasRelativePosition(new Vector3(1.1f, 2f, 3f));
            Utilities.AreExactlyEqual(new Vector3(0), ComputeAvailableSize(child, availableSize, false));

            // tests in the case available size are infinite
            availableSize = new Vector3(float.PositiveInfinity);
            child.SetCanvasAbsolutePosition(new Vector3(-1, -2, -3));
            Utilities.AreExactlyEqual(new Vector3(0), ComputeAvailableSize(child, availableSize, false));
            child.SetCanvasAbsolutePosition(new Vector3(1, 2, 3));
            Utilities.AreExactlyEqual(new Vector3(float.PositiveInfinity, 4, 3), ComputeAvailableSize(child, availableSize, false));
            child.SetCanvasRelativePosition(new Vector3(-1f, -2f, -3f));
            Utilities.AreExactlyEqual(new Vector3(0), ComputeAvailableSize(child, availableSize, false));
            child.SetCanvasRelativePosition(new Vector3(1f, 2f, 3f));
            Utilities.AreExactlyEqual(new Vector3(float.PositiveInfinity), ComputeAvailableSize(child, availableSize, false));
            child.SetCanvasRelativeSize(new Vector3(0, 0.5f, 1.5f));
            Utilities.AreExactlyEqual(new Vector3(0, float.PositiveInfinity, float.PositiveInfinity), ComputeAvailableSize(child, availableSize, false));
        }

        /// <summary>
        /// Test the function <see cref="Canvas.MeasureOverride"/> when provided size is infinite
        /// </summary>
        [Test]
        public void TestMeasureOverrideInfinite()
        {
            var child1 = new MeasureValidator();
            var canvas = new Canvas { Children = { child1 } };

            // check that relative 0 x inf available = 0 
            child1.SetCanvasRelativeSize(Vector3.Zero);
            child1.ExpectedMeasureValue = Vector3.Zero;
            canvas.Measure(new Vector3(float.PositiveInfinity));
            child1.SetCanvasRelativeSize(new Vector3(float.NaN));

            // check sizes with infinite measure values and absolute position
            child1.SetCanvasAbsolutePosition(new Vector3(1, -1, -3));
            child1.ExpectedMeasureValue = new Vector3(float.PositiveInfinity, 0, 0);
            child1.ReturnedMeasuredValue = new Vector3(2);
            canvas.Measure(new Vector3(float.PositiveInfinity));
            Assert.AreEqual(new Vector3(3, 1, 0), canvas.DesiredSizeWithMargins);

            // check sizes with infinite measure values and relative position
            child1.SetCanvasPinOrigin(new Vector3(0, .5f, 1));
            child1.SetCanvasRelativePosition(new Vector3(-1));
            child1.ExpectedMeasureValue = new Vector3(0);
            child1.ReturnedMeasuredValue = new Vector3(1);
            canvas.Measure(new Vector3(float.PositiveInfinity));
            Assert.AreEqual(new Vector3(0.5f, 0.25f, 0), canvas.DesiredSizeWithMargins);
            child1.SetCanvasRelativePosition(new Vector3(0));
            child1.ExpectedMeasureValue = new Vector3(float.PositiveInfinity, 0, 0);
            child1.ReturnedMeasuredValue = new Vector3(1);
            canvas.Measure(new Vector3(float.PositiveInfinity));
            Assert.AreEqual(new Vector3(1, 0.5f, 0), canvas.DesiredSizeWithMargins);
            child1.SetCanvasRelativePosition(new Vector3(0.5f));
            child1.ExpectedMeasureValue = new Vector3(float.PositiveInfinity);
            child1.ReturnedMeasuredValue = new Vector3(1);
            canvas.Measure(new Vector3(float.PositiveInfinity));
            Assert.AreEqual(new Vector3(2, 1, 2), canvas.DesiredSizeWithMargins);
            child1.SetCanvasRelativePosition(new Vector3(1f));
            child1.ExpectedMeasureValue = new Vector3(float.PositiveInfinity);
            child1.ReturnedMeasuredValue = new Vector3(1);
            canvas.Measure(new Vector3(float.PositiveInfinity));
            Assert.AreEqual(new Vector3(0, 0.5f, 1), canvas.DesiredSizeWithMargins);
            child1.SetCanvasRelativePosition(new Vector3(2f));
            child1.ExpectedMeasureValue = new Vector3(float.PositiveInfinity);
            child1.ReturnedMeasuredValue = new Vector3(1);
            canvas.Measure(new Vector3(float.PositiveInfinity));
            Assert.AreEqual(new Vector3(0, 0.25f, 0.5f), canvas.DesiredSizeWithMargins);

            // check that the maximum is correctly taken
            var child2 = new MeasureValidator();
            var child3 = new MeasureValidator();
            canvas.Children.Add(child2);
            canvas.Children.Add(child3);
            child1.InvalidateMeasure();
            child1.SetCanvasPinOrigin(new Vector3(0.5f));
            child1.SetCanvasRelativePosition(new Vector3(0.5f));
            child1.ExpectedMeasureValue = new Vector3(float.PositiveInfinity);
            child1.ReturnedMeasuredValue = new Vector3(10);
            child2.SetCanvasPinOrigin(new Vector3(0.5f));
            child2.SetCanvasRelativePosition(new Vector3(-.1f, .5f, 1.2f));
            child2.ExpectedMeasureValue = new Vector3(0, float.PositiveInfinity, float.PositiveInfinity);
            child2.ReturnedMeasuredValue = new Vector3(30.8f, 5, 48);
            child3.SetCanvasRelativeSize(new Vector3(0f, 1f, 2f));
            child3.ExpectedMeasureValue = new Vector3(0, float.PositiveInfinity, float.PositiveInfinity);
            child3.ReturnedMeasuredValue = new Vector3(0, 5, 50);
            canvas.Measure(new Vector3(float.PositiveInfinity));
            Assert.AreEqual(new Vector3(14f, 10f, 25f), canvas.DesiredSizeWithMargins);
        }
    }
}
