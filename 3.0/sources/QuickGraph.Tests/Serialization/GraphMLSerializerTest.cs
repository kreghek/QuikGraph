using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using QuickGraph.Unit;
using Microsoft.Pex.Framework;
using System.Xml.XPath;

namespace QuickGraph.Serialization
{
    public static class GraphMLFilesHelper
    {
        public static string[] GetFileNames()
        {
            return Directory.GetFiles("GraphML", "*.graphml");
        }

        public static IEnumerable<AdjacencyGraph<IdentifiableVertex, IdentifiableEdge<IdentifiableVertex>>> GetGraphs()
        {
            foreach (var graphmlFile in GraphMLFilesHelper.GetFileNames())
            {
                var g = LoadGraph(graphmlFile);
                yield return g;
            }
        }

        public static AdjacencyGraph<IdentifiableVertex, IdentifiableEdge<IdentifiableVertex>> LoadGraph(string graphmlFile)
        {
            Console.WriteLine(graphmlFile);
            var g = new AdjacencyGraph<IdentifiableVertex, IdentifiableEdge<IdentifiableVertex>>();
            using (var reader = new StreamReader(graphmlFile))
            {
                g.DeserializeFromGraphML(
                    reader,
                    id => new IdentifiableVertex(id),
                    (source, target, id) => new IdentifiableEdge<IdentifiableVertex>(source, target, id)
                    );
            }
            return g;
        }
    }

    [TestFixture, PexClass]
    public partial class GraphMLSerializerIntegrationTest
    {
        [Test]
        public void DeserializeFromGraphMLNorth()
        {
            foreach (var graphmlFile in GraphMLFilesHelper.GetFileNames())
            {
                Console.Write(graphmlFile);
                var g = new AdjacencyGraph<IdentifiableVertex, IdentifiableEdge<IdentifiableVertex>>();
                using (var reader = new StreamReader(graphmlFile))
                {
                    g.DeserializeFromGraphML(
                        reader,
                        id => new IdentifiableVertex(id),
                        (source, target, id) => new IdentifiableEdge<IdentifiableVertex>(source, target, id)
                        );
                }
                Console.Write(": {0} vertices, {1} edges", g.VertexCount, g.EdgeCount);

                var vertices = new Dictionary<string, IdentifiableVertex>();
                foreach(var v in g.Vertices)
                    vertices.Add(v.ID, v);

                // check all nodes are loaded
                var settings = new XmlReaderSettings();
                settings.XmlResolver = new GraphMLXmlResolver();
                settings.ProhibitDtd = false;
                settings.ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags.None;
                using(var xreader = XmlReader.Create(graphmlFile, settings))
                {
                    var doc = new XPathDocument(xreader);
                    foreach (XPathNavigator node in doc.CreateNavigator().Select("/graphml/graph/node"))
                    {
                        string id = node.GetAttribute("id", "");
                        Assert.IsTrue(vertices.ContainsKey(id));
                    }
                    Console.Write(", vertices ok");

                    // check all edges are loaded
                    foreach (XPathNavigator node in doc.CreateNavigator().Select("/graphml/graph/edge"))
                    {
                        string source = node.GetAttribute("source", "");
                        string target = node.GetAttribute("target", "");
                        Assert.IsTrue(g.ContainsEdge(vertices[source], vertices[target]));
                    }
                    Console.Write(", edges ok");
                }
                Console.WriteLine();
            }
        }
    }

    [TestFixture, PexClass]
    public partial class GraphMLSerializerTest
    {
        [Test, PexMethod]
        public void RoundTrip()
        {
            RoundTripGraph(new AdjacencyGraphFactory().SimpleIdentifiable());
        }

        [PexMethod]
        public void RoundTripGraph([PexAssumeNotNull]IMutableVertexAndEdgeListGraph<NamedVertex, NamedEdge> g)
        {
            AdjacencyGraph<NamedVertex, NamedEdge> gd = new AdjacencyGraph<NamedVertex, NamedEdge>();
            string baseLine;
            string output;

            using (var writer = new StringWriter())
            {
                using(var xwriter = XmlWriter.Create(writer))
                    g.SerializeToGraphML(xwriter);
                baseLine = writer.ToString();
                TestConsole.WriteLineBold("Original graph:");
                Console.WriteLine(writer.ToString());
                TestConsole.WriteLineBold("---");

                using (XmlTextReader reader = new XmlTextReader(new StringReader(writer.ToString())))
                {
                    gd.DeserializeFromGraphML(reader,
                        id => new NamedVertex(id),
                        (source, target, id) => new NamedEdge(source, target, id)
                    );
                }
            }

            TestConsole.WriteLineBold("Roundtripped graph:");
            using (var sw = new StringWriter())
            {
                using (var xwriter = XmlWriter.Create(sw))
                    gd.SerializeToGraphML(xwriter);
                output = sw.ToString();
                Console.WriteLine(sw.ToString());
            }

            Assert.AreEqual(g.VertexCount, gd.VertexCount);
            Assert.AreEqual(g.EdgeCount, gd.EdgeCount);
            StringAssert.AreEqual(
                baseLine,
                output,
                StringComparison.InvariantCulture
                );
        }
    }
}