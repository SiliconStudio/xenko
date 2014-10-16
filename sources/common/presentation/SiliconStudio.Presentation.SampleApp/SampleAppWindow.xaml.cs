// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Presentation.Controls;
using SiliconStudio.Quantum;

using System.Linq;

//using EditableListBox = SiliconStudio.Presentation.Legacy.EditableListBox;
using TextBox = System.Windows.Controls.TextBox;

namespace SiliconStudio.Presentation.SampleApp
{
    /// <summary>
    /// Interaction logic for SampleAppWindow.xaml
    /// </summary>
    public partial class SampleAppWindow : IDisposable
    {
        private Timer timer;
        
        public SampleAppWindow()
        {
            InitializeComponent();

            Acc.DataContext = new AsyncTestObject { LoadState = ViewModelContentState.NotLoaded };

            //SetupEditableListBoxCommands(EditableListBox);
            //SetupEditableListBoxCommands(EditableListBox2);

            /*
            editableListBox.Items.Clear();
            editableListBox.ItemsSource = new[]
            {
                new Button { Content = "Bound Button 1" },
                new Button { Content = "Bound Button 2" },
                new Button { Content = "Bound Button 3" },
                new Button { Content = "Bound Button 4" },
                new Button { Content = "Bound Button 5" },
            };

            editableListBox.ItemsSource = new ObservableCollection<DependencyObject>()
            {
                new Button { Content = "Bound Button 1" },
                new Button { Content = "Bound Button 2" },
                new Button { Content = "Bound Button 3" },
                new Button { Content = "Bound Button 4" },
                new Button { Content = "Bound Button 5" },
            };*/

            TextBoxViewModel = new TextBoxViewModel();
            TimerTextBoxViewModel = new TimerTextBoxViewModel();
            NumericTextBoxViewModel = new NumericTextBoxViewModel();
            SliderTextBoxViewModel = new SliderTextBoxViewModel();
            VectorEditorViewModel = new VectorEditorViewModel();
            FilteringComboBoxViewModel = new FilteringComboBoxViewModel();
 
            SetupPropertyGrid();

            SetupRadTreeView();

            SetupTilePanel();

            SetupRenamingEditableListBox();

            SetupLogViewer();
        }

        public TextBoxViewModel TextBoxViewModel { get; set; }

        public TimerTextBoxViewModel TimerTextBoxViewModel { get; set; }
        
        public NumericTextBoxViewModel NumericTextBoxViewModel { get; set; }

        public SliderTextBoxViewModel SliderTextBoxViewModel { get; set; }

        public VectorEditorViewModel VectorEditorViewModel { get; set; }

        public FilteringComboBoxViewModel FilteringComboBoxViewModel { get; set; }

        public void Dispose()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void SetupPropertyGrid()
        {
            PropertyGrid.DataContext = new PropertyGridViewModel();
        }

        private void SetupLogViewer()
        {
            var messages = new ObservableCollection<ILogMessage>();
            timer = new Timer(x => Dispatcher.InvokeAsync(() => messages.Add(RandomMessage())), null, 300, 300);
            LogViewer.LogMessages = messages;
        }

        private static LogMessage RandomMessage()
        {
            var rand = new Random();
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < 4 + rand.Next(8); ++i)
            {
                for (int j = 0; j < 2 + rand.Next(18); ++j)
                {
                    stringBuilder.Append((char)('a' + rand.Next(26)));
                }
                stringBuilder.Append(' ');
            }
            stringBuilder.Append('.');

            var messageType = (LogMessageType)rand.Next(6);
            return new LogMessage("SampleApp", messageType, stringBuilder.ToString());
        }

        //public struct Item
        //{
        //    public int ID { get; set; }
        //    public EditionCancellableWrapper<string> Name { get; set; }
        //}

        private void SetupRenamingEditableListBox()
        {
            ////renamingEditableListBox.DataContext = Enumerable.Range(1, 10)
            ////    .Select(i => new Item { ID = i, Name = new EditableValueWrapper<string> { Value = (i * 10.0).ToString() } })
            ////    .ToArray();

            // TODO: FIXME
            //var array = Enumerable.Range(1, 10)
            //    .Select(i => new Item { ID = i, Name = (i * 10.0).ToString() })
            //    .ToArray();

            //renamingEditableListBox.DataContext = array;

            //string str = array[0].Name;
            //Console.WriteLine(str);
        }

        private void SetupRadTreeView()
        {
            //var root = new Node
            //{
            //    Content = "Root",
            //    Children = new[]
            //    {
            //        new Node
            //        {
            //            Content = "Child 1",
            //            Children = new[]
            //            {
            //                new Node { Content = "Sub Child 11" },
            //                new Node { Content = "Sub Child 12" },
            //                new Node { Content = "Sub Child 13" }
            //            },
            //        },
            //        new Node
            //        {
            //            Content = "Child 2",
            //            Children = new[]
            //            {
            //                new Node { Content = "Sub Child 21" },
            //                new Node { Content = "Sub Child 22" },
            //                new Node { Content = "Sub Child 23" }
            //            },
            //        }
            //    },
            //};

            //RadTreeView.ItemsSource = new[] { root };
        }

        //private void SetupEditableListBoxCommands(EditableListBox editableListBox)
        //{
        //    int count = 300;

        //    System.Windows.Interactivity.Interaction.GetBehaviors(editableListBox).Add(
        //        new DropBehavior
        //        {
        //            Command = new AnonymousCommand<DropCommandParameters>(p => editableListBox.Items.Insert(p.TargetIndex, new Border
        //                {
        //                    BorderBrush = Brushes.Orange,
        //                    Background = Brushes.CornflowerBlue,
        //                    BorderThickness = new Thickness(2.0),
        //                    CornerRadius = new CornerRadius(3.0),
        //                    Margin = new Thickness(1.0),
        //                    Padding = new Thickness(4.0),
        //                    Child = new TextBlock { Text = ((ContentControl)p.Data).Content.ToString(), },
        //                })),
        //            DataType = "EditableListBoxExternalDrop",
        //        });

        //    editableListBox.AddNewItemCommand = new AnonymousCommand(p =>
        //    {
        //        var pp = p as DropCommandParameters;

        //        if (pp != null)
        //        {
        //            var data = pp.Data as ContentControl;
        //            if (data != null) // this is for testing purpose
        //            {
        //                editableListBox.Items.Insert(pp.TargetIndex, new Border
        //                {
        //                    Child = new TextBlock { Text = data.Content.ToString() },
        //                    CornerRadius = new CornerRadius(3.0),
        //                    BorderThickness = new Thickness(2.0),
        //                    BorderBrush = Brushes.Orange,
        //                    Background = Brushes.CornflowerBlue,
        //                    Margin = new Thickness(3.0),
        //                    Padding = new Thickness(15.0, 5.0, 150.0, 5.0),
        //                });
        //            }
        //        }
        //        else
        //        {
        //            editableListBox.Items.Add(new Button
        //            {
        //                Content = "Button " + count++,
        //                Margin = new Thickness(3.0),
        //                Padding = new Thickness(15.0, 5.0, 150.0, 5.0),
        //            });
        //        }
        //    });

        //    editableListBox.RemoveSelectedItemsCommand = new AnonymousCommand<IEnumerable<object>>(x => x.ForEach(y => editableListBox.Items.Remove(y)));

        //    editableListBox.ReorderItemCommand = new AnonymousCommand(p =>
        //    {
        //        var pp = p as DropCommandParameters;
        //        if (pp == null)
        //            throw new ArgumentException("parameter must be of type DropCommandParameters");

        //        editableListBox.Items.Remove(pp.Data);
        //        int index = pp.TargetIndex;
        //        if (index > pp.SourceIndex)
        //            index--;
        //        editableListBox.Items.Insert(index, pp.Data);
        //    });
        //}

        private void SetupTilePanel()
        {
            VirtTilePanel.DataContext = Enumerable.Range(0, 10000)
                .Select(i => string.Format("Item {0}", i))
                .ToArray();
        }

        //private void EffectEditorProduceOutputDataButtonClick(object sender, RoutedEventArgs e)
        //{
        //    EffectDefinition output = effectEditor.ProduceEffectDefinition();
        //    // breakpoint here to manually check data validity
        //}
        private void OnAsyncContentControlSetLoadingButtonClick(object sender, RoutedEventArgs e)
        {
            //((AsyncTestObject)acc.DataContext).LoadContentCommand.Execute(null);
            ((AsyncTestObject)Acc.DataContext).LoadState = ViewModelContentState.Loading;
        }

        private void TextBoxClearTextButtonClick(object sender, RoutedEventArgs e)
        {
            SskkTextBox.Clear();
        }

        private void TextBoxValidateButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tag = button.Tag as string;
            if (tag == null) throw new ArgumentException();
            var target = (Controls.TextBox)FindName(tag);
            if (target == null) throw new ArgumentException();
            target.Validate();
        }

        private void TextBoxCancelButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tag = button.Tag as string;
            if (tag == null) throw new ArgumentException();
            var target = (Controls.TextBox)FindName(tag);
            if (target == null) throw new ArgumentException();
            target.Cancel();
        }

        private void SliderTextBoxRangeIndicatorBrushButton(object sender, RoutedEventArgs e)
        {
            var brushes = new List<Brush> { Brushes.Lime, Brushes.DarkTurquoise, Brushes.Orange };
            int index = (brushes.IndexOf(SliderTextBox.RangeIndicatorBrush) + 1) % brushes.Count;
            SliderTextBox.RangeIndicatorBrush = brushes[index];
        }

        private void SliderTextBoxMouseValidationTriggerButton(object sender, RoutedEventArgs e)
        {
            SliderTextBox.MouseValidationTrigger = SliderTextBox.MouseValidationTrigger == MouseValidationTrigger.OnMouseMove ? MouseValidationTrigger.OnMouseUp : MouseValidationTrigger.OnMouseMove;
        }

        private void SetTextButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tag = button.Tag as string;
            if (tag != null)
            {
                var parms = tag.Split('|');
                var target = (TextBox)FindName(parms[0]);
                if (target == null) throw new ArgumentException();
                if (parms[1] == "Text")
                    target.Text = parms[2];
                else if (parms[1] == "Value")
                    ((NumericTextBox)target).Value = double.Parse(parms[2]);
            }
        }
    }

    //public class TestLineElementsMapper : IVectorElementsMapper
    //{
    //    public IEnumerable<string> GetVectorElementNames()
    //    {
    //        yield return "X1";
    //        yield return "Y1";
    //        yield return "X2";
    //        yield return "Y2";
    //    }
    //}

    //public class TestLine : INotifyPropertyChanged
    //{
    //    static TestLine()
    //    {
    //        VectorEditor.RegisterVectorLayoutDescriptor(typeof(TestLine), new SizeI(2, 2), new TestLineElementsMapper());
    //    }

    //    private float x1;
    //    public float X1 { get { return x1; } set { x1 = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("X1")); } }
    //    private float y1;
    //    public float Y1 { get { return y1; } set { y1 = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Y1")); } }
    //    private float x2;
    //    public float X2 { get { return x2; } set { x2 = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("X2")); } }
    //    private float y2;
    //    public float Y2 { get { return y2; } set { y2 = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Y2")); } }

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    public override string ToString()
    //    {
    //        return string.Format("{0:f2};{1:f2} | {2:f2};{3:f2}", X1, Y1, X2, Y2);
    //    }
    //}
}
