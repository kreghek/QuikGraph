﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Common;
using GraphX.Controls;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using GraphX.PCL.Logic.Models;
using Mono.Addins;
using QuickGraph;
using QuickGraph.GraphXAdapter;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;

// This plugin demonstrates how to use plugin system.
// Add your plugin reference to MainForm project.
// Read more at Mono.Addins wiki page on GitHub.
// https://github.com/mono/mono-addins/wiki/Architecture-Overview


[assembly: Addin]
[assembly: AddinDependency("GraphTasks", "1.0")]

namespace PluginSample
{
    using Graph = BidirectionalGraph<GraphXVertex, GraphXTaggedEdge<GraphXVertex, int>>;

    [Extension]
    public class PluginSample : IAlgorithm
    {
        private readonly CheckBox _countSymbolsCheckBox;
        private readonly ElementHost _wpfHost;
        private readonly GraphArea<GraphXVertex, GraphXTaggedEdge<GraphXVertex, int>, Graph> _graphArea;
        private readonly GXLogicCore<GraphXVertex, GraphXTaggedEdge<GraphXVertex, int>, Graph> _logic;
        private readonly ZoomControl _zoomControl;
        private Stack<Graph> _steps;
        private Graph _graph;
        private bool _hasStarted;
        private bool _hasFinished;

        public PluginSample()
        {
            _countSymbolsCheckBox = new CheckBox
            {
                Text = "Count symbols",
                Location = new Point(12, 20)
            };

            // GraphX integration
            _logic = new GXLogicCore<GraphXVertex, GraphXTaggedEdge<GraphXVertex, int>, Graph>
            {
                DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.LinLog,
                DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA,
                DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None,
                AsyncAlgorithmCompute = false

            };
            _graphArea = new GraphArea<GraphXVertex, GraphXTaggedEdge<GraphXVertex, int>, Graph>
            {
                EnableWinFormsHostingMode = true,
                LogicCore = _logic,
                EdgeLabelFactory = new DefaultEdgelabelFactory(),
            };

            _zoomControl = new ZoomControl
            {
                Content = _graphArea,
                Visibility = Visibility.Visible,
            };
            ZoomControl.SetViewFinderVisibility(_zoomControl, Visibility.Visible);

            _wpfHost = new ElementHost()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Child = _zoomControl
            };

            _graphArea.ShowAllEdgesLabels();
            _logic.DefaultLayoutAlgorithmParams = _logic.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.LinLog);
            _logic.DefaultOverlapRemovalAlgorithmParams = _logic.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)_logic.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)_logic.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;

            MessageBox.Show(Directory.GetCurrentDirectory());
            MessageBox.Show(new Uri(Directory.GetCurrentDirectory()).ToString());

            var templatePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "GraphXTemplate.xaml";
            _zoomControl.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(templatePath) });

            Options.Controls.Add(_countSymbolsCheckBox);
            Output.Controls.Add(_wpfHost);
        }

        public string Name => "Sample Plugin";
        public string Author => "Eugene Auduchinok";

        public string Description =>
            "This plugin demonstrates how to use plugin system.\n" +
            "Algorithms removes one vertex on each step until there's no vertices.\n";

        public Panel Options { get; } = new Panel();
        public Panel Output { get; } = new Panel();

        public void Run(string dotSource)
        {
            var vertexFun = VertexFactory.Name;
            var edgeFun = EdgeFactory<GraphXVertex>.Weighted(0);

            try
            {
                _graph = Graph.LoadDot(dotSource, vertexFun, edgeFun);
                _steps = new Stack<Graph>();

                _logic.Graph = _graph;
                _graphArea.GenerateGraph();
                _graphArea.SetVerticesDrag(true, true);
                _graphArea.RelayoutGraph(true);
                _zoomControl.ZoomToFill();
                _wpfHost.Refresh();
                _wpfHost.Update();

//                var message = $"{_graph.VertexCount} vertices.";
//                if (_countSymbolsCheckBox.Checked) message = $"{message} {dotSource.Length} symbols read.";
//                MessageBox.Show(message);

                _hasStarted = true;
                _hasFinished = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void NextStep()
        {
            _steps.Push(_graph.Clone());

            var v = _graph.Vertices.First();
            _graph.RemoveVertex(v);
            ShowResults();

            if (!_graph.Vertices.Any()) _hasFinished = true;
        }

        public void PreviousStep()
        {
            _graph = _steps.Pop();
            ShowResults();

            _hasFinished = false;
        }

        private void ShowResults()
        {
            MessageBox.Show($"{_graph.VertexCount} vertices.\nTotal edges weight {_graph.Edges.Sum(edge => edge.Tag)}.");
        }

        public bool CanGoBack => _steps != null && _steps.Count != 0;
        public bool CanGoFurther => _hasStarted && !_hasFinished;
    }
}