using KTerminalNetworkBDD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a 4x4 grid network where all nodes are terminal nodes and
            // where edges are one of two types with reliabilities 0.9 and 0.1.
            double type1Reliability = 0.9;
            double type2Reliability = 0.1;
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {

                if (eIndex % 2 == 0)
                    return type1Reliability;
                else
                    return type2Reliability;
            };
            Network network = GridNetworkGenerator.GenerateAllTerminal(4, 4, edgeReliabilityFunc);

            // Create a dictionary mapping edges to its type, based on reliability.
            Dictionary<Edge, int> edgeToTypeDict = new Dictionary<Edge, int>();
            foreach (Edge edge in network.Edges)
            {
                if (edge.Reliability == type1Reliability)
                    edgeToTypeDict[edge] = 1;
                else
                    edgeToTypeDict[edge] = 2;
            }

            // Compute the survival signature.
            int[] dimToComponentType;
            NDArray signature = ComputeBDDAlgorithm.ComputeSignature(network.Edges, edgeToTypeDict, out dimToComponentType);

            // Print the survival signature and the reliability of the network to the console window.
            Console.WriteLine("Survival signature table:");
            Console.Write(SurvivalSignatureFuns.PrintAsTable(signature, dimToComponentType));
            double probability = SurvivalSignatureFuns.GetProbability(signature, new double[] { 0.9, 0.1 });
            Console.WriteLine("Probability: {0}", probability);
        }
    }
}
