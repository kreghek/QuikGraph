﻿#if SUPPORTS_SERIALIZATION
using System;
#endif
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

namespace QuikGraph.Algorithms.RandomWalks
{
#if SUPPORTS_SERIALIZATION
    [Serializable]
#endif
    public sealed class RandomWalkAlgorithm<TVertex, TEdge> 
        : ITreeBuilderAlgorithm<TVertex,TEdge>
        where TEdge : IEdge<TVertex>
    {
        private IImplicitGraph<TVertex,TEdge> visitedGraph;
        private EdgePredicate<TVertex,TEdge> endPredicate;
        private IEdgeChain<TVertex,TEdge> edgeChain;

        public RandomWalkAlgorithm(IImplicitGraph<TVertex,TEdge> visitedGraph)
            :this(visitedGraph,new NormalizedMarkovEdgeChain<TVertex,TEdge>())
        {}

        public RandomWalkAlgorithm(
            IImplicitGraph<TVertex,TEdge> visitedGraph,
            IEdgeChain<TVertex,TEdge> edgeChain
            )
        {
#if SUPPORTS_CONTRACTS
            Contract.Requires(visitedGraph != null);
            Contract.Requires(edgeChain != null);
#endif

            this.visitedGraph = visitedGraph;
            this.edgeChain = edgeChain;
        }

        public IImplicitGraph<TVertex,TEdge> VisitedGraph
        {
            get
            {
                return this.visitedGraph;
            }
        }

        public IEdgeChain<TVertex,TEdge> EdgeChain
        {
            get
            {
                return this.edgeChain;
            }
            set
            {
#if SUPPORTS_CONTRACTS
                Contract.Requires(value != null);
#endif

                this.edgeChain = value;
            }
        }

        public EdgePredicate<TVertex,TEdge> EndPredicate
        {
            get
            {
                return this.endPredicate;
            }
            set
            {
                this.endPredicate = value;
            }
        }

        public event VertexAction<TVertex> StartVertex;
        private void OnStartVertex(TVertex v)
        {
            var eh = this.StartVertex;
            if (eh != null)
                eh(v);
        }

        public event VertexAction<TVertex> EndVertex;
        private void OnEndVertex(TVertex v)
        {
            var eh = this.EndVertex;
            if (eh != null)
                eh(v);
        }

        public event EdgeAction<TVertex,TEdge> TreeEdge;
        private void OnTreeEdge(TEdge e)
        {
            var eh = this.TreeEdge;
            if (eh != null)
                eh(e);
        }

        private bool TryGetSuccessor(TVertex u, out TEdge successor)
        {
            return this.EdgeChain.TryGetSuccessor(this.VisitedGraph, u, out successor);
        }

        public void Generate(TVertex root)
        {
#if SUPPORTS_CONTRACTS
            Contract.Requires(root != null);
#endif

            Generate(root, 100);
        }

        public void Generate(TVertex root, int walkCount)
        {
#if SUPPORTS_CONTRACTS
            Contract.Requires(root != null);
#endif

            int count = 0;
            TEdge e = default(TEdge);
            TVertex v = root;

            OnStartVertex(root);
            while (count < walkCount && this.TryGetSuccessor(v, out e))
            {
                // if dead end stop
                if (e==null)
                    break;
                // if end predicate, test
                if (this.endPredicate != null && this.endPredicate(e))
                    break;
                OnTreeEdge(e);
                v = e.Target;
                // upgrade count
                ++count;
            }
            OnEndVertex(v);
        }

    }
}