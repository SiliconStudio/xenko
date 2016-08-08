// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Markup;

using GraphX;
using GraphX.Controls;
using GraphX.GraphSharp.Algorithms.Layout.Simple.Tree;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using GraphX.GraphSharp.Algorithms.Layout;
using QuickGraph;

using SiliconStudio.Presentation.Graph.ViewModel;
using SiliconStudio.Presentation.Graph.Controls;

namespace SiliconStudio.Presentation.Graph.Behaviors
{
    /// <summary>
    /// 
    /// </summary>
    public class OutgoingConnectionWrapper : ConnectionWrapper { }

    /// <summary>
    /// 
    /// </summary>
    [ContentProperty("ConnectionWrappers")]
    public class NodeGraphBehavior : Behavior<GraphArea<NodeVertex, NodeEdge, BidirectionalGraph<NodeVertex, NodeEdge>>>
    {
        #region ConnectionWrapperData Struct
        /// <summary>
        /// 
        /// </summary>
        struct ConnectionWrapperData
        {
            public NodeGraphBehavior Owner;
            public ConnectionWrapper Wrapper;
            public NodeVertex Vertex;

            public ConnectionWrapperData(NodeGraphBehavior owner, ConnectionWrapper wrapper, NodeVertex vertex)
            {
                Owner = owner;
                Wrapper = wrapper;
                Vertex = vertex;
            }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty RootNodesProperty = DependencyProperty.Register("RootNodes", typeof(IEnumerable), typeof(NodeGraphBehavior), new PropertyMetadata(null, OnRootNodesChanged));
        private static readonly DependencyProperty OutgoingsProperty = DependencyProperty.RegisterAttached("Outgoings", typeof(IEnumerable), typeof(NodeGraphBehavior), new PropertyMetadata(null, OnOutgoingsChanged));
        #endregion

        // TODO: This dictionary is never cleaned
        private static readonly Dictionary<DependencyObject, ConnectionWrapperData> ConnectionWrapperDats = new Dictionary<DependencyObject, ConnectionWrapperData>();
        
        #region Static Dependency Property Event Handler
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnRootNodesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (NodeGraphBehavior)d;

            var oldList = e.OldValue as INotifyCollectionChanged;
            if (oldList != null) { oldList.CollectionChanged -= behavior.OnRootNodesCollectionChanged; }

            behavior.graph.Clear();

            var newList = e.NewValue as INotifyCollectionChanged;
            if (newList != null) { newList.CollectionChanged += behavior.OnRootNodesCollectionChanged; }

            // Loop through all the root nodes and add them
            var enumerable = (IEnumerable)e.NewValue;
            foreach (var item in enumerable)
            {
                behavior.AddNode(item as NodeVertex);
            }

            behavior.RootNodes = e.NewValue as IEnumerable;            
            behavior.RelayoutGraph();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnOutgoingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // OutgoingsProperty is a property that gets bound to a vertex control dynamically during runtime. 
            
            // TODO When outgoings is changed, we need to redirect that call elsewhere
            // Current approach is to register these to a static dictionary of connection wrappers
            /*ConnectionWrapperData data;
            if (ConnectionWrapperDats.TryGetValue(d, out data))
            {
                //
            }*/
        }
        #endregion

        #region Members
        private BidirectionalGraph<NodeVertex, NodeEdge> graph;
        private readonly List<ConnectionWrapper> connection_wrappers_ = new List<ConnectionWrapper>();        
        #endregion

        #region Attach & Detach Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void OnAttached()
        {
            // TODO Move the logic core else where! Or perhaps the components inside logic core.

            // Lets create logic core and filled data graph with edges and vertice
            var LogicCore = new NodeGraphLogicCore(); 

            // This property sets layout algorithm that will be used to calculate vertice positions
            // Different algorithms uses different values and some of them uses edge Weight property.
            LogicCore.DefaultLayoutAlgorithm = GraphX.LayoutAlgorithmTypeEnum.Tree;

            // Now we can set parameters for selected algorithm using AlgorithmFactory property. This property provides methods for
            // creating all available algorithms and algo parameters.
            LogicCore.DefaultLayoutAlgorithmParams = LogicCore.AlgorithmFactory.CreateLayoutParameters(GraphX.LayoutAlgorithmTypeEnum.Tree);
            ((SimpleTreeLayoutParameters)LogicCore.DefaultLayoutAlgorithmParams).Direction = LayoutDirection.LeftToRight;
            ((SimpleTreeLayoutParameters)LogicCore.DefaultLayoutAlgorithmParams).VertexGap = 50;
            ((SimpleTreeLayoutParameters)LogicCore.DefaultLayoutAlgorithmParams).LayerGap = 100;

            // This property sets vertex overlap removal algorithm.
            // Such algorithms help to arrange vertices in the layout so no one overlaps each other.
            LogicCore.DefaultOverlapRemovalAlgorithm = GraphX.OverlapRemovalAlgorithmTypeEnum.FSA;
            LogicCore.DefaultOverlapRemovalAlgorithmParams = LogicCore.AlgorithmFactory.CreateOverlapRemovalParameters(GraphX.OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)LogicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)LogicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;

            // This property sets edge routing algorithm that is used to build route paths according to algorithm logic.
            // For ex., SimpleER algorithm will try to set edge paths around vertices so no edge will intersect any vertex.
            // Bundling algorithm will try to tie different edges that follows same direction to a single channel making complex graphs more appealing.
            LogicCore.DefaultEdgeRoutingAlgorithm = GraphX.EdgeRoutingAlgorithmTypeEnum.SimpleER;

            // This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            // will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            // Area.RelayoutFinished and Area.GenerateGraphFinished.
            LogicCore.AsyncAlgorithmCompute = false;

            // Create the quick graph for the node
            LogicCore.Graph = new BidirectionalGraph<NodeVertex, NodeEdge>();
            graph = LogicCore.Graph;

            // Finally assign logic core to GraphArea object
            AssociatedObject.LogicCore = LogicCore;// as IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>;

            // Create the control factory
            AssociatedObject.ControlFactory = new NodeGraphControlFactory(AssociatedObject);

            //
            base.OnAttached();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnDetaching()
        {
            // TODO
            base.OnDetaching();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRootNodesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var newItem in e.NewItems) { AddNode(newItem as NodeVertex); }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var oldItem in e.OldItems) { RemoveNode(oldItem as NodeVertex); }
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
        private void OnOutgoingsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (NodeEdge newItem in e.NewItems) { AddEdge(newItem); }
                    break;
                case NotifyCollectionChangedAction.Remove:                        
                    foreach (NodeEdge oldItem in e.OldItems) { RemoveEdge(oldItem); }
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
        #endregion

        #region Graph Operation Methods
        /// <summary>
        /// 
        /// </summary>
        public void RelayoutGraph()
        {
            AssociatedObject.RelayoutGraph();

            // TODO We might not need this
            /*foreach (var edge in AssociatedObject.EdgesList)
            {
                edge.Value.Visibility = Visibility.Visible;
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        protected void AddNode(NodeVertex node)
        {
            // Skip if it it already been added
            if (graph.ContainsVertex(node)) { return; }

            // Add the vertex to the logic graph 
            graph.AddVertex(node);

            // Create the vertex control
            var control = AssociatedObject.ControlFactory.CreateVertexControl(node);
            control.DataContext = node;
            control.Visibility = Visibility.Hidden; // make them invisible (there is no layout positions yet calculated)
            
            // Create data binding for input slots and output slots
            var binding = new Binding();
            binding.Path = new PropertyPath("InputSlots");
            binding.Mode = BindingMode.TwoWay;
            binding.Source = node;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;            
            BindingOperations.SetBinding(control, NodeVertexControl.InputSlotsProperty, binding);

            binding = new Binding();
            binding.Path = new PropertyPath("OutputSlots");
            binding.Mode = BindingMode.TwoWay;
            binding.Source = node;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(control, NodeVertexControl.OutputSlotsProperty, binding);
            
            // Add vertex and control to the graph area
            AssociatedObject.AddVertex(node, control);

            // Loop through all the connection wrappers and process them
            foreach (var wrapper in connection_wrappers_)
            {
                // TODO: throw proper exceptions when the wrapper binding cannot be exploited (has RelativeSource, has ElementName, etc.)
                if (wrapper is OutgoingConnectionWrapper)
                {
                    // Create a new binding between the node and the connection wrapper
                    // This will allow the node to access the outgoing property 
                    binding = new Binding();
                    binding.Path = (wrapper as OutgoingConnectionWrapper).Binding.Path;
                    binding.Mode = (wrapper as OutgoingConnectionWrapper).Binding.Mode;
                    binding.Source = node;

                    var dummyObj = new DependencyObject();
                    BindingOperations.SetBinding(dummyObj, OutgoingsProperty, binding);
                    var outgoings = dummyObj.GetValue(OutgoingsProperty) as IEnumerable;

                    var collection = outgoings as INotifyCollectionChanged;
                    if (collection != null)
                    {
                        collection.CollectionChanged += OnOutgoingsCollectionChanged;
                    }

                    // Loop through all outgoing connections
                    foreach (NodeEdge entry in outgoings) {                        
                        NodeEdge edge = entry;
                        NodeVertex target = edge.Target as NodeVertex;

                        if (!graph.ContainsVertex(target)) { AddNode(target); }
                        if (!graph.ContainsEdge(edge)) { AddEdge(edge); }
                    }
                    
                    // TODO
                    //ConnectionWrapperData.Add(dummyObj, new ConnectionWrapperData(this, wrapper, node));
                }
            }

            AssociatedObject.RelayoutGraph();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        protected void RemoveNode(NodeVertex node)
        {
            if (graph.ContainsVertex(node))
            {
                // Remove outgoing edges first
                foreach (NodeEdge outgoing in node.Outgoings)
                {
                    graph.RemoveEdge(outgoing);
                    AssociatedObject.RemoveEdge(outgoing);
                }

                // TODO Need a better way to removing incoming edges
                IEnumerable<EdgeControl> controls = AssociatedObject.GetAllEdgeControls().Where(x => (x.Edge as NodeEdge).Target == node);
                foreach (var control in controls)
                {
                    NodeEdge edge = control.Edge as NodeEdge;
                    graph.RemoveEdge(edge);
                    AssociatedObject.RemoveEdge(edge);
                }
                
                // Then remove the vertex
                graph.RemoveVertex(node);
                AssociatedObject.RemoveVertex(node);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        protected void AddEdge(NodeEdge edge)
        {
            // Skip if it it already been added
            if (graph.ContainsEdge(edge)) { return; }

            // Add the vertex to the logic graph 
            graph.AddEdge(edge);            
            
            // Create the vertex control
            var control = AssociatedObject.ControlFactory.CreateEdgeControl(AssociatedObject.VertexList[edge.Source], AssociatedObject.VertexList[edge.Target], edge);
            control.DataContext = edge;
            control.Visibility = Visibility.Hidden; // make them invisible (there is no layout positions yet calculated)

            // Create data binding for input slots and output slots
            var binding = new Binding();
            binding.Path = new PropertyPath("Links");
            binding.Mode = BindingMode.TwoWay;
            binding.Source = edge;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(control, NodeEdgeControl.LinksProperty, binding);

            // Add vertex and control to the graph area
            AssociatedObject.AddEdge(edge, control);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        protected void RemoveEdge(NodeEdge edge)
        {
            if (graph.ContainsEdge(edge))
            {
                graph.RemoveEdge(edge);
                AssociatedObject.RemoveEdge(edge);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        protected void RemoveEdge(NodeVertex source, NodeVertex target)
        {
            // TODO: Make an efficient remove
            var edgeToRemove = graph.Edges.Where(x => x.Source == source && x.Target == target).ToList();
            foreach (var edge in edgeToRemove)
            {
                graph.RemoveEdge(edge);
            }
        }
        #endregion
        
        #region Properties
        public IEnumerable RootNodes { get { return (IEnumerable)GetValue(RootNodesProperty); } set { SetValue(RootNodesProperty, value); } }
        public IList ConnectionWrappers { get { return connection_wrappers_; } }
        #endregion
    }
}
