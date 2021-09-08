using System;
using System.Collections.Generic;
using System.Linq;

namespace KTerminalNetworkBDD
{
    public static class GridNetworkGenerator
    {
        /// <summary>
        /// Generates a grid network with two terminal nodes.
        /// </summary>
        /// <param name="rows">THe number of rows in the grid.</param>
        /// <param name="columns">The number of columns in the grid.</param>
        /// <param name="edgeReliabilityFunc">A function mapping the index of an edge to its reliability.</param>
        /// <returns>The generated network.</returns>
        public static Network GenerateTwoTerminal(int rows, int columns, Func<int, double> edgeReliabilityFunc)
        {
            bool IsTerminalPredicate(int rowIndex, int columnIndex)
            {
                return (rowIndex == 0 && columnIndex == 0) || ((rowIndex == rows - 1) && (columnIndex == columns - 1));
            }

            return Generate(rows, columns, edgeReliabilityFunc, IsTerminalPredicate);
        }

        /// <summary>
        /// Generates a grid network where all nodes are terminal nodes.
        /// </summary>
        /// <param name="rows">THe number of rows in the grid.</param>
        /// <param name="columns">The number of columns in the grid.</param>
        /// <param name="edgeReliabilityFunc">A function mapping the index of an edge to its reliability.</param>
        /// <returns>The generated network.</returns>
        public static Network GenerateAllTerminal(int rows, int columns, Func<int, double> edgeReliabilityFunc)
        {
            bool IsTerminalPredicate(int rowIndex, int columnIndex)
            {
                return true;
            }

            return Generate(rows, columns, edgeReliabilityFunc, IsTerminalPredicate);
        }

        public static Network Generate(int rows, int columns, Func<int, double> edgeReliabilityFunc,
            Func<int, int, bool> isTerminalPredicate)
        {
            Dictionary<Tuple<int,int>, Vertex> vertices = new Dictionary<Tuple<int, int>, Vertex>();
            List<Edge> edges = new List<Edge>();

            int edgeIndex = 0;
            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    // Create vertex.
                    // Top left and bottom right vertices are the only terminal nodes.
                    bool isTerminal = isTerminalPredicate(rowIndex, columnIndex);
                    Vertex v = new Vertex($"{rowIndex}, {columnIndex}", isTerminal);
                    vertices.Add(new Tuple<int, int>(rowIndex, columnIndex), v);

                    // Create edges.
                    // Edge to vertex to left of this one, if any.
                    if (columnIndex != 0)
                    {
                        Vertex leftV = vertices[new Tuple<int, int>(rowIndex, columnIndex - 1)];
                        Edge edge = new Edge(v + "->" + leftV, v, leftV, edgeReliabilityFunc(edgeIndex));
                        edges.Add(edge);
                        edgeIndex++;
                    }
                    // Edge to vertex above this one, if any.
                    if (rowIndex != 0)
                    {
                        Vertex aboveV = vertices[new Tuple<int, int>(rowIndex - 1, columnIndex)];
                        Edge edge = new Edge(v + "->" + aboveV, v, aboveV, edgeReliabilityFunc(edgeIndex));
                        edges.Add(edge);
                        edgeIndex++;
                    }
                }
            }

            return new Network(edges);
        }
    }
}
