﻿using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace QuickGraph
{
    /// <summary>
    /// The default <see cref="IEdge&lt;TVertex&gt;"/> implementation.
    /// </summary>
    /// <typeparam name="TVertex">The type of the vertex.</typeparam>
    [Serializable]
    [DebuggerDisplay("{Source}->{Target}")]
    public class Edge<TVertex> 
        : IEdge<TVertex>
    {
        private readonly TVertex source;
        private readonly TVertex target;

        /// <summary>
        /// Initializes a new instance of the <see cref="Edge&lt;TVertex&gt;"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public Edge(TVertex source, TVertex target)
        {
            Contract.Requires(source != null);
            Contract.Requires(target != null);

            this.source = source;
            this.target = target;
        }

        /// <summary>
        /// Gets the type of the vertex.
        /// </summary>
        /// <value>The type of the vertex.</value>
        public static Type VertexType
        {
            [Pure]
            get { return typeof(TVertex); }
        }

        /// <summary>
        /// Gets the source vertex
        /// </summary>
        /// <value></value>
        public TVertex Source
        {
            [Pure]
            get { return this.source; }
        }

        /// <summary>
        /// Gets the target vertex
        /// </summary>
        /// <value></value>
        public TVertex Target
        {
            [Pure]
            get { return this.target; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return this.Source + "->" + this.Target;
        }
    }
}