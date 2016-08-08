// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


using GraphX;
using GraphX.Controls.Models;
using SiliconStudio.Presentation.Graph.Helper;
using SiliconStudio.Presentation.Extensions;
using System.Windows.Input;

namespace SiliconStudio.Presentation.Graph.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [TemplatePart(Name = "PART_linkItemsControl", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_linkPath", Type = typeof(Path))]    
    public class NodeEdgeControl : EdgeControl, INotifyPropertyChanged
    {
        #region Dependency Properties
        public static DependencyProperty LinksProperty = DependencyProperty.Register( "Links", typeof(IEnumerable), typeof(NodeEdgeControl), new PropertyMetadata(OnLinksChanged));

        public static readonly DependencyProperty LinkStrokeProperty = DependencyProperty.Register("LinkStroke", typeof(Brush), typeof(NodeEdgeControl), new PropertyMetadata(Brushes.LightGray));
        public static readonly DependencyProperty LinkStrokeThicknessProperty = DependencyProperty.Register("LinkStrokeThickness", typeof(double), typeof(NodeEdgeControl), new PropertyMetadata(5.0));
        public static readonly DependencyProperty MouseOverLinkStrokeProperty = DependencyProperty.Register("MouseOverLinkStroke", typeof(Brush), typeof(NodeEdgeControl), new PropertyMetadata(Brushes.Green));
        public static readonly DependencyProperty SelectedLinkStrokeProperty = DependencyProperty.Register("SelectedLinkStroke", typeof(Brush), typeof(NodeEdgeControl), new PropertyMetadata(Brushes.LightGreen));
        #endregion

        #region Static Dependency Property Event Handler
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static void OnLinksChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = (NodeEdgeControl)obj;

            // \remark Letting the items control handle the collection changes

            var oldList = e.OldValue as INotifyCollectionChanged;
            if (oldList != null) { oldList.CollectionChanged -= control.OnLinksCollectionChanged; }

            var newList = e.NewValue as INotifyCollectionChanged;
            if (newList != null) { newList.CollectionChanged += control.OnLinksCollectionChanged; }

            control.LinksSource = e.NewValue as IEnumerable;

            control.link_paths_.Clear();
            foreach (Tuple<object, object> item in control.LinksSource)
            {
                control.link_paths_.Add(item, null);
            } 
        }
        #endregion

        #region Members
        public event PropertyChangedEventHandler PropertyChanged;
        private ItemsControl link_items_control_;
        private Dictionary<Tuple<object, object>, Path> link_paths_ = new Dictionary<Tuple<object, object>, Path>();
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="edge"></param>
        /// <param name="showLabels"></param>
        /// <param name="showArrows"></param>
        public NodeEdgeControl(VertexControl source, VertexControl target, object edge, bool showLabels = false, bool showArrows = true)
            : base(source, target, edge, showLabels, showArrows)
        {
            this.Loaded += OnLoaded;
        }

        /// <summary>
        /// 
        /// </summary>
        public NodeEdgeControl()
            : base()
        {
            this.Loaded += OnLoaded;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Template != null)
            {
                link_items_control_ = Template.FindName("PART_linkItemsControl", this) as ItemsControl;
                if (link_items_control_ == null) { Debug.WriteLine("EdgeControl Template -> Edge template have no 'PART_linkGrid' Grid object!"); }

                var itemsSource = link_items_control_.ItemsSource as INotifyCollectionChanged;
                if (itemsSource == null) { Debug.WriteLine("Bad bad"); }

                // Better to check for the collection change in the items control since it is used generate the actual slot and connector controls
                itemsSource.CollectionChanged += OnItemsSourceCollectionChanged;

                //
                RetrieveLinkPaths();

                //
                UpdateEdge();                               
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Just update source and grab the link paths from the items control
            RetrieveLinkPaths();

            // Update the edges
            UpdateEdge();       
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLinksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO should I clear the link_paths_ in this case?
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Tuple<object, object> newItem in e.NewItems) 
                    { 
                        link_paths_.Add(newItem, null); 
                    }                    
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Tuple<object, object> oldItem in e.OldItems) 
                    {
                        if (link_paths_[oldItem] != null)
                        {
                            link_paths_[oldItem].MouseDown -= OnLinkMouseDown;
                        }
                        link_paths_.Remove(oldItem);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }              
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLinkMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                (RootArea as NodeGraphArea).OnLinkSelected(sender as FrameworkElement);
            }
            e.Handled = true;
        }
        #endregion

        #region Links & Path Methods
        /// <summary>
        /// 
        /// </summary>
        public void RetrieveLinkPaths()
        {
            if (link_items_control_ == null) { return;  }
            
            // Loop though all the items and find the paths
            for (int i = 0; i < link_items_control_.Items.Count; ++i)
            {                
                var item = link_items_control_.ItemContainerGenerator.ContainerFromIndex(i);
                if (item != null)
                {
                    Path path = UIHelper.FindChild<Path>(item, "PART_linkPath");
                    if (path == null)
                    {
                        (item as FrameworkElement).ApplyTemplate();
                        path = UIHelper.FindChild<Path>(item, "PART_linkPath");
                    }

                    Tuple<object, object> link = path.DataContext as Tuple<object, object>;
                    if (!link_paths_.ContainsKey(link))
                    {
                        Debug.WriteLine("baaad");
                    }

                    if (link_paths_[link] == null)
                    {
                        //
                        link_paths_[link] = path;
                        path.MouseDown += OnLinkMouseDown;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateLabel"></param>
        public override void UpdateEdge(bool updateLabel = true)
        {
            // TODO Need to let the styling come through for the paths!

            base.UpdateEdge(updateLabel);

            // Short-circuit out if the items control is null
            if (link_items_control_ == null) { return; }
            
            // Loop through all link paths and update the path positions
            foreach (KeyValuePair<Tuple<object, object>, Path> entry in link_paths_)
            {
                if (entry.Value == null) { continue; }  // TODO Bad Bad

                Tuple<object, object> link = entry.Key;                    
                BezierSegment bezier = new BezierSegment();
                PathGeometry geometry = new PathGeometry();
                PathFigure figure = new PathFigure();

                figure.Segments.Add(bezier);
                geometry.Figures.Add(figure);                

                // Find the output slot 
                DependencyObject slot = null;
                if ((Source as NodeVertexControl).OutputConnectors.TryGetValue(link.Item1, out slot))
                {
                    UIElement container = VisualTreeHelper.GetChild(Source, 0) as UIElement;
                    Point offset = (slot as UIElement).TransformToAncestor(container).Transform(new Point(0, 0));
                    Point location = Source.GetPosition() + (Vector)offset;                      
                    Vector halfsize = new Vector((double)slot.GetValue(FrameworkElement.WidthProperty) * 0.8,
                                                    (double)slot.GetValue(FrameworkElement.HeightProperty) / 2.0);

                    figure.SetCurrentValue(PathFigure.StartPointProperty, location + halfsize);
                    //figure.StartPoint = location + halfsize;                        
                }

                // Find input slot
                if ((Target as NodeVertexControl).InputConnectors.TryGetValue(link.Item2, out slot))
                {
                    UIElement container = VisualTreeHelper.GetChild(Target, 0) as UIElement;
                    Point offset = (slot as UIElement).TransformToAncestor(container).Transform(new Point(0, 0));
                    Point location = Target.GetPosition() + (Vector)offset;

                    //
                    Vector halfsize = new Vector((double)slot.GetValue(FrameworkElement.WidthProperty) * 0.2,
                                                    (double)slot.GetValue(FrameworkElement.HeightProperty) / 2.0);

                    bezier.SetCurrentValue(BezierSegment.Point3Property, location + halfsize);
                    //bezier.Point3 = location + halfsize;   
                }

                double length = bezier.Point3.X - figure.StartPoint.X;
                double curvature = length * 0.4;

                bezier.SetCurrentValue(BezierSegment.Point1Property, new Point(figure.StartPoint.X + curvature, figure.StartPoint.Y));
                //bezier.Point1 = new Point(figure.StartPoint.X + curvage, figure.StartPoint.Y);

                bezier.SetCurrentValue(BezierSegment.Point2Property, new Point(bezier.Point3.X - curvature, bezier.Point3.Y));
                //bezier.Point2 = new Point(bezier.Point3.X - curvage, bezier.Point3.Y);

                //
                entry.Value.Data = geometry;
            }

            // TODO Should I be doing this here??? should I be uing setcurrentvalue??
            Visibility = Visibility.Visible;
        }
        #endregion

        #region Properties
        public IEnumerable LinksSource { get { return (IEnumerable)GetValue(LinksProperty); } set { SetValue(LinksProperty, value); } }
        public Brush LinkStroke { get { return (Brush)GetValue(LinkStrokeProperty); } set { SetValue(LinkStrokeProperty, value); } }
        public double LinkStrokeThickness { get { return (double)GetValue(LinkStrokeThicknessProperty); } set { SetValue(LinkStrokeThicknessProperty, value); } }
        public Brush MouseOverLinkStroke { get { return (Brush)GetValue(MouseOverLinkStrokeProperty); } set { SetValue(MouseOverLinkStrokeProperty, value); } }
        public Brush SelectedLinkStroke { get { return (Brush)GetValue(SelectedLinkStrokeProperty); } set { SetValue(SelectedLinkStrokeProperty, value); } }        

        public Dictionary<Tuple<object, object>, Path> LinkPaths { get { return link_paths_; } }
        #endregion

        #region Notify Property Change Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
