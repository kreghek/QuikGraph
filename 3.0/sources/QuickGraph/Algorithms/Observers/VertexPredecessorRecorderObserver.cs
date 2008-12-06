﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace QuickGraph.Algorithms.Observers
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Vertex"></typeparam>
    /// <typeparam name="Edge"></typeparam>
    /// <reference-ref
    ///     idref="boost"
    ///     />
    [Serializable]
    public sealed class VertexPredecessorRecorderObserver<TVertex, TEdge> :
        IObserver<ITreeBuilderAlgorithm<TVertex, TEdge>>
        where TEdge : IEdge<TVertex>
    {
        private readonly IDictionary<TVertex, TEdge> vertexPredecessors;

        public VertexPredecessorRecorderObserver()
            :this(new Dictionary<TVertex,TEdge>())
        {}

        public VertexPredecessorRecorderObserver(
            IDictionary<TVertex, TEdge> vertexPredecessors)
        {
            Contract.Requires(vertexPredecessors != null);

            this.vertexPredecessors = vertexPredecessors;
        }

        public IDictionary<TVertex, TEdge> VertexPredecessors
        {
            get { return this.vertexPredecessors; }
        }

        public void Attach(ITreeBuilderAlgorithm<TVertex, TEdge> algorithm)
        {
            Contract.Requires(algorithm != null);
            algorithm.TreeEdge+=new EdgeEventHandler<TVertex,TEdge>(TreeEdge);
        }

        public void Detach(ITreeBuilderAlgorithm<TVertex, TEdge> algorithm)
        {
            Contract.Requires(algorithm != null);
            algorithm.TreeEdge -= new EdgeEventHandler<TVertex, TEdge>(TreeEdge);
        }

        void TreeEdge(Object sender, EdgeEventArgs<TVertex, TEdge> e)
        {
            this.vertexPredecessors[e.Edge.Target] = e.Edge;
        }

        public bool TryGetPath(TVertex vertex, out IEnumerable<TEdge> path)
        {
            return AlgorithmExtensions.TryGetPath(this.VertexPredecessors, vertex, out path);
        }
    }
}