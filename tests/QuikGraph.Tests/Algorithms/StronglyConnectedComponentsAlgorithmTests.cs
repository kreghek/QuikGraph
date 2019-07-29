﻿using System;
using NUnit.Framework;
using QuikGraph.Algorithms.ConnectedComponents;
using QuikGraph.Serialization;
using QuikGraph.Tests;

namespace QuikGraph.Algorithms
{
    [TestFixture]
    internal class StronglyConnectedComponentAlgorithmTests : QuikGraphUnitTests
    {
        [Test]
        public void EmptyGraph()
        {
            var g = new AdjacencyGraph<string, Edge<string>>(true);
            var strong = new StronglyConnectedComponentsAlgorithm<string, Edge<String>>(g);
            strong.Compute();
            Assert.AreEqual(0, strong.ComponentCount);
            checkStrong(strong);
        }

        [Test]
        public void OneVertex()
        {
            AdjacencyGraph<string, Edge<string>> g = new AdjacencyGraph<string, Edge<string>>(true);
            g.AddVertex("test");
            var strong = new StronglyConnectedComponentsAlgorithm<string, Edge<String>>(g);
            strong.Compute();
            Assert.AreEqual(1, strong.ComponentCount);

            checkStrong(strong);
        }

        [Test]
        public void TwoVertex()
        {
            AdjacencyGraph<string, Edge<string>> g = new AdjacencyGraph<string, Edge<string>>(true);
            g.AddVertex("v1");
            g.AddVertex("v2");
            var strong = new StronglyConnectedComponentsAlgorithm<string, Edge<String>>(g);
            strong.Compute();
            Assert.AreEqual(2, strong.ComponentCount);

            checkStrong(strong);
        }

        [Test]
        public void TwoVertexOnEdge()
        {
            AdjacencyGraph<string, Edge<string>> g = new AdjacencyGraph<string, Edge<string>>(true);
            g.AddVertex("v1");
            g.AddVertex("v2");
            g.AddEdge(new Edge<string>("v1", "v2"));
            var strong = new StronglyConnectedComponentsAlgorithm<string, Edge<String>>(g);
            strong.Compute();
            Assert.AreEqual(2, strong.ComponentCount);

            checkStrong(strong);
        }


        [Test]
        public void TwoVertexCycle()
        {
            AdjacencyGraph<string, Edge<string>> g = new AdjacencyGraph<string, Edge<string>>(true);
            g.AddVertex("v1");
            g.AddVertex("v2");
            g.AddEdge(new Edge<string>("v1", "v2"));
            g.AddEdge(new Edge<string>("v2", "v1"));
            var strong = new StronglyConnectedComponentsAlgorithm<string, Edge<String>>(g);
            strong.Compute();
            Assert.AreEqual(1, strong.ComponentCount);

            checkStrong(strong);
        }

        [Test]
        public void StronglyConnectedComponentAll()
        {
            foreach (var g in TestGraphFactory.GetAdjacencyGraphs())
                this.Compute(g);
        }

        public void Compute<TVertex,TEdge>(AdjacencyGraph<TVertex, TEdge> g)
            where TEdge : IEdge<TVertex>
        {
            var strong = new StronglyConnectedComponentsAlgorithm<TVertex, TEdge>(g);

            strong.Compute();
            checkStrong(strong);
        }

        private void checkStrong<TVertex,TEdge>(StronglyConnectedComponentsAlgorithm<TVertex, TEdge> strong)
            where TEdge : IEdge<TVertex>
        {
            Assert.AreEqual(strong.VisitedGraph.VertexCount, strong.Components.Count);
            Assert.AreEqual(strong.VisitedGraph.VertexCount, strong.DiscoverTimes.Count);
            Assert.AreEqual(strong.VisitedGraph.VertexCount, strong.Roots.Count);

            foreach (var v in strong.VisitedGraph.Vertices)
            {
                Assert.IsTrue(strong.Components.ContainsKey(v));
                Assert.IsTrue(strong.DiscoverTimes.ContainsKey(v));
            }

            foreach (var de in strong.Components)
            {
                Assert.IsNotNull(de.Key);
                Assert.IsTrue(de.Value <= strong.ComponentCount);
            }

            foreach (var de in strong.DiscoverTimes)
            {
                Assert.IsNotNull(de.Key);
            }
        }
    }
}