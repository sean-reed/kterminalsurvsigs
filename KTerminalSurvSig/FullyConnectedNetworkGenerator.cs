using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KTerminalNetworkBDD
{
    public static class FullyConnectedNetworkGenerator
    {
        public static Network GenerateFullyConnected(int numberVertices, Func<int, bool> isTerminalPredicate, 
            Func<int, double> edgeReliabilityFunc)
        {
            Vertex[] vertices = new Vertex[numberVertices];
            List<Edge> edges = new List<Edge>();

            int edgeIndex = 0;

            for (int i = 0; i < numberVertices; i++)
            {
                Vertex v = new Vertex(i.ToString(), isTerminalPredicate(i));

                vertices[i] = v;

                for (int j = 0; j < i; j++)
                {
                    string edgeLabel = i.ToString() + " -- " + j.ToString();
                    Edge e = new Edge(edgeLabel, v, vertices[j], edgeReliabilityFunc(edgeIndex));

                    edges.Add(e);
                    edgeIndex++;
                }
            }

            return new Network(edges);
         }

        /// <summary>
        /// Network that has each of its nodes connected to the following n-1 vertices. 
        /// Since edges are undirected, each vertex is also connected to the preceding n-1 vertices. 
        /// Note that if n equals the number of nodes we have the fully connected graph of n vertices.
        /// </summary>
        /// <param name="numberVertices">Total n</param>
        /// <param name="n">Each vertex is connected by an edge to the following n-1 vertices.</param>
        /// <param name="isTerminalPredicate">Function that returns whether an edge is terminal from its number. </param>
        /// <param name="edgeReliabilityFunc">Function that returns the edge reliability from its number.</param>
        /// <returns>The closest n fully connected network.</returns>
        public static Network GenerateClosestNFullyConnected(int numberVertices, int n,
            Func<int, bool> isTerminalPredicate, Func<int, double> edgeReliabilityFunc)
        {
            Vertex[] vertices = new Vertex[numberVertices];
            List<Edge> edges = new List<Edge>();

            for (int i = 0; i < numberVertices; i++)
            {
                Vertex v = new Vertex(i.ToString(), isTerminalPredicate(i));

                vertices[i] = v;
            }

            // Create edges.
            for (int i = 0; i < numberVertices; i++)
            {
                for (int j = i+1; j < numberVertices && j < i+n; j++)
                {
                    string edgeLabel = i.ToString() + " -- " + j.ToString();
                    Edge e = new Edge(edgeLabel, vertices[i], vertices[j], edgeReliabilityFunc(j));

                    edges.Add(e);
                }
            }

            return new Network(edges);
        }
    }
}
