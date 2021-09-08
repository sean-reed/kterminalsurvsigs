using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTerminalNetworkBDD
{
    public class Network
    {
        public List<Vertex> Vertices => _vertexEdges.Keys.ToList();

        public List<Edge> Edges { get; private set; }

        private readonly Dictionary<Vertex, List<Edge>> _vertexEdges;

        /// <summary>
        /// Total number of terminal nodes in network.
        /// </summary>
        public int K { get; private set; }

        public Network(List<Edge> edges)
        {
            // Check for edge consistency.
            HashSet<string> edgeLabels = new HashSet<string>(edges.Select(e => e.Label));
            if (edgeLabels.Count != edges.Count)
            {
                throw new ArgumentException("Label of each edge in edges must be unique.");
            }


            this.Edges = edges;

            // Build dictionary of edges connected to each vertex.
            _vertexEdges = new Dictionary<Vertex, List<Edge>>();
            foreach (var edge in Edges)
            {
                if (_vertexEdges.ContainsKey(edge.V1))
                {
                    _vertexEdges[edge.V1].Add(edge);
                }
                else
                {
                    _vertexEdges[edge.V1] = new List<Edge>(){edge};
                }

                if (_vertexEdges.ContainsKey(edge.V2))
                {
                    _vertexEdges[edge.V2].Add(edge);
                }
                else
                {
                    _vertexEdges[edge.V2] = new List<Edge>() { edge };
                }
            }

            this.K = _vertexEdges.Keys.Count(v => v.IsTerminal);

            // Check for vertex consistency.
            HashSet<string> vertexLabels = new HashSet<string>(Vertices.Select(v => v.Label));
            if (vertexLabels.Count != Vertices.Count)
            {
                throw new ArgumentException("Label of each vertex in vertices must be unique.");
            }
        }

        public IEnumerable<Edge> GetEdges(Vertex vertex)
        {
            return _vertexEdges[vertex];
        }

        public string GraphVizString()
        {
            return GraphVizString(e => $"label=\"{e.Label}\"", v => $"label=\"{v.Label}\"");
        }

        public string GraphVizString(Func<Edge, string> edgeOptionsFunc, Func<Vertex, string> vertexOptionsFunc)
        {
 
            StringBuilder s = new StringBuilder("graph G {");
            foreach (var v in Vertices)
            {
                s.Append($"{v.Label} [{vertexOptionsFunc(v)}];");
            }
            foreach (var edge in Edges)
            {
                s.Append($"{edge.V1.Label} -- {edge.V2.Label} [{edgeOptionsFunc(edge)}];");
            }

            s.Append("}"); // End of graph.

            return s.ToString();
        }
    }
}
