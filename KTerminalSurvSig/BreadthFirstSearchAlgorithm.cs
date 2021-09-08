using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTerminalNetworkBDD
{
    public static class BreadthFirstSearchAlgorithm
    {
        public static IEnumerable<Edge> GetEdgeVisitedOrder(Network network, int startVertexIndex)
        {
            if(startVertexIndex >= network.Vertices.Count) throw new ArgumentOutOfRangeException(nameof(startVertexIndex));
         
            HashSet<Vertex> visited = new HashSet<Vertex>();
            Queue<Vertex> verticesToProcess = new Queue<Vertex>();
           
            Vertex startVertex = network.Vertices[startVertexIndex];
            visited.Add(startVertex);
            verticesToProcess.Enqueue(startVertex);
           
            HashSet<Edge> visitedEdges = new HashSet<Edge>();

            while (verticesToProcess.Count != 0)
            {
                Vertex nextVertex = verticesToProcess.Dequeue();

                IEnumerable<Edge> adjacentEdges = network.GetEdges(nextVertex);
                foreach (var outEdge in adjacentEdges)
                {
                    if (!visitedEdges.Contains(outEdge))
                    {
                        yield return outEdge;

                        visitedEdges.Add(outEdge);
                    }

                    if (!visited.Contains(outEdge.V1))
                    {
                        verticesToProcess.Enqueue(outEdge.V1);
                        visited.Add(outEdge.V1);
                    }
                    else if (!visited.Contains(outEdge.V2))
                    {
                        verticesToProcess.Enqueue(outEdge.V2);
                        visited.Add(outEdge.V2);
                    }
                }              
            }
        }
    }
}
