using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using KTerminalNetworkBDD;

namespace RFIDNetworkExample
{
    class Program
    {
        static void Main(string[] args)
        {

            int[] routerNodes = new[] { 16, 27, 31, 46 };
            int serverNode = 61;

            Func<int, bool> isTerminalTest = (node) =>
            {
                // All terminal case.
                //return 

                // Nodes 47 to 60.
                return (node >= 47 && node <= 60) || node == serverNode;

                // 2-terminal case (node 0 and server).
                //return node == 0 || node == serverNode;
            };

            // Get networks.   
            Network netAllTerminal = GetRFIDNetwork((node) => !routerNodes.Contains(node));

            Network net2Terminal = GetRFIDNetwork((node) => node == 0 ||  node == serverNode);

            Network netKTerminal = GetRFIDNetwork((node) => (node >= 47 && node <= 60) || node == serverNode);

            Console.Write(GetGraphViz(netAllTerminal, serverNode, routerNodes));

            // Get signatures.
            int[] dimToComponentType;
            NDArray allTerminalUnnormalisedSignature;
            NDArray allTerminalSignature = 
                GetSignature(netAllTerminal, routerNodes, serverNode, out dimToComponentType, 
                out allTerminalUnnormalisedSignature);

            int[] twoTerminalSignatureDimToComponentType;
            NDArray twoTerminalUnnormalisedSignature;
            NDArray twoTerminalSignature =
                GetSignature(net2Terminal, routerNodes, serverNode, out dimToComponentType,
                out twoTerminalUnnormalisedSignature);

            int[] kTerminalSignatureDimToComponentType;
            NDArray kTerminalUnnormalisedSignature;
            NDArray kTerminalSignature =
                GetSignature(netKTerminal, routerNodes, serverNode, out dimToComponentType, out kTerminalUnnormalisedSignature);

            Console.WriteLine("DONE");

            // Read in component failure probabilities.
            StreamReader file = new StreamReader("RFID_net_comp_probs.txt");
            string line = file.ReadLine(); // Discard first header line.
            double[,] probs = new double[92, 6];
            while ((line = file.ReadLine()) != null)
            {
                string[] parts = line.Split(new char[] { ',' });
                int type1SurvivedCount = int.Parse(parts[0]);
                int type2SurvivedCount = int.Parse(parts[1]);
                double prob = double.Parse(parts[2]);
                probs[type1SurvivedCount, type2SurvivedCount] = prob;
            }
            NDArray probsArray = NDArray.FromValues(probs);

            // Get k-terminal availability.
            double allTerminalAvailability = NDArray.Multiply(allTerminalSignature, probsArray).GetSum();
            double twoTerminalAvailability = NDArray.Multiply(twoTerminalSignature, probsArray).GetSum();
            double kTerminalAvailability = NDArray.Multiply(kTerminalSignature, probsArray).GetSum();

            var allTerminalSignatureTable = SurvivalSignatureFuns.PrintAsTable(allTerminalSignature, dimToComponentType);
            var twoTerminalSignatureTable = SurvivalSignatureFuns.PrintAsTable(twoTerminalSignature, dimToComponentType);
            var kTerminalSignatureTable = SurvivalSignatureFuns.PrintAsTable(kTerminalSignature, dimToComponentType);

            using (StreamWriter sigFile = new StreamWriter("allTerminalSignature.txt"))
            {
                sigFile.Write(allTerminalSignatureTable);
            }
            using (StreamWriter sigFile = new StreamWriter("twoTerminalSignature.txt"))
            {
                sigFile.Write(twoTerminalSignatureTable);
            }
            using (StreamWriter sigFile = new StreamWriter("kTerminalSignature.txt"))
            {
                sigFile.Write(kTerminalSignatureTable);
            }

            Console.Write(allTerminalAvailability);


            // Get plot values and write to file.
            using (StreamWriter file2 = new StreamWriter("netAvailabilityPlotValues.txt"))
            {
                file2.WriteLine("Normalised Interference, All Terminal Availability, K Terminal Availability, Two Terminal Availability");
                for (int i = 0; i <= 20; i++)
                {
                    double interference = i * (1.0 / 20);
                    double type1Prob = 1.0 - (0.0013 * Math.Exp(-5.5 * Math.Exp(-3.5 * interference)));
                    double type2Prob = 1.0 - (0.0002 * Math.Exp(-7.7 * Math.Exp(-4.1 * interference)));
                    
                    double[] compSurvivalProbabilityByDim = new[] { type1Prob, type2Prob };

                    allTerminalAvailability =
                        SurvivalSignatureFuns.GetProbability(allTerminalUnnormalisedSignature, compSurvivalProbabilityByDim);
                    twoTerminalAvailability =
                        SurvivalSignatureFuns.GetProbability(twoTerminalUnnormalisedSignature, compSurvivalProbabilityByDim);
                    kTerminalAvailability =
                        SurvivalSignatureFuns.GetProbability(kTerminalUnnormalisedSignature, compSurvivalProbabilityByDim);


                    file2.WriteLine($"{interference}, {allTerminalAvailability}, {kTerminalAvailability}, {twoTerminalAvailability}");
                    
                }
            }
             
        }

        private static NDArray GetSignature(Network net, int[] routerNodes, int serverNode, out int[] dimToComponentType,
            out NDArray unnormalisedSignature)
        {
            // Define dictionary of edge types.
            var edgeToType = new Dictionary<Edge, int>();
            foreach (var edge in net.Edges)
            {
                if ((routerNodes.Contains(int.Parse(edge.V1.Label)) && routerNodes.Contains(int.Parse(edge.V2.Label)))
                    || int.Parse(edge.V1.Label) == serverNode || int.Parse(edge.V2.Label) == serverNode)
                {
                    edgeToType[edge] = 2;
                }
                else
                {
                    edgeToType[edge] = 1;
                }
            }

            // Compute the signature.
            var oe = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(net, 0).ToList();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            unnormalisedSignature = ComputeBDDAlgorithm.ComputeSignature(oe, edgeToType, out dimToComponentType);
            watch.Stop();
            Console.WriteLine($"Total seconds: {watch.Elapsed.TotalSeconds}.");
            var normalisationArray = SurvivalSignatureFuns.GetNormalisationArray(unnormalisedSignature.Shape);
            var signature = NDArray.Divide(unnormalisedSignature, normalisationArray);

            return signature;
        }

        private static string GetGraphViz(Network net, int serverNode, IList<int> routerNodes)
        {
            Func<Edge, string> edgeOptionsFunc = e =>
            {
                string options = $"label=\"\"";

                if ((routerNodes.Contains(int.Parse(e.V1.Label)) || int.Parse(e.V1.Label) == serverNode)
                    && (routerNodes.Contains(int.Parse(e.V2.Label)) || int.Parse(e.V2.Label) == serverNode))
                {
                    options += ", style =\"dashed\"";
                }

                return options;
            };

            Func<Vertex, string> vertexOptionsFunc = (v) =>
            {
                string vLabel = (int.Parse(v.Label) + 1).ToString();
                string options = $"label=\"{vLabel}\"";

                if (routerNodes.Contains(int.Parse(v.Label)))
                {
                    options += ", shape=\"circle\", style=\"filled\"";
                }
                else if (int.Parse(v.Label) == serverNode)
                {
                    options += ", shape=\"triangle\", style=\"filled\"";
                }
                else
                {
                    options += ", shape=\"square\"";
                }

                return options;
            };
            string viz = net.GraphVizString(edgeOptionsFunc, vertexOptionsFunc);

            return viz;
        }

        private static Network GetRFIDNetwork(Func<int, bool> isTerminalTest)
        {
            // Create vertices.
            int vertexCount = 62;
            List<Vertex> vertices = new List<Vertex>();
            for (int i = 0; i < vertexCount; i++)
            {
                vertices.Add(new Vertex(i.ToString(), isTerminalTest(i)));
            }

            // Create edges.
            List<Tuple<int, int>> connectedVertices = new List<Tuple<int, int>>();
            connectedVertices.Add(new Tuple<int, int>(0, 1));
            connectedVertices.Add(new Tuple<int, int>(0, 2));
            connectedVertices.Add(new Tuple<int, int>(1, 2));
            connectedVertices.Add(new Tuple<int, int>(1, 3));
            connectedVertices.Add(new Tuple<int, int>(2, 3));
            connectedVertices.Add(new Tuple<int, int>(2, 4));
            connectedVertices.Add(new Tuple<int, int>(3, 5));
            connectedVertices.Add(new Tuple<int, int>(4, 5));
            connectedVertices.Add(new Tuple<int, int>(5, 6));
            connectedVertices.Add(new Tuple<int, int>(6, 10));
            connectedVertices.Add(new Tuple<int, int>(6, 16));
            connectedVertices.Add(new Tuple<int, int>(7, 8));
            connectedVertices.Add(new Tuple<int, int>(7, 9));
            connectedVertices.Add(new Tuple<int, int>(7, 10));
            connectedVertices.Add(new Tuple<int, int>(8, 9));
            connectedVertices.Add(new Tuple<int, int>(9, 10));
            connectedVertices.Add(new Tuple<int, int>(10, 16));
            connectedVertices.Add(new Tuple<int, int>(11, 12));
            connectedVertices.Add(new Tuple<int, int>(11, 13));
            connectedVertices.Add(new Tuple<int, int>(12, 13));
            connectedVertices.Add(new Tuple<int, int>(12, 14));
            connectedVertices.Add(new Tuple<int, int>(13, 14));
            connectedVertices.Add(new Tuple<int, int>(13, 15));
            connectedVertices.Add(new Tuple<int, int>(14, 15));
            connectedVertices.Add(new Tuple<int, int>(14, 17));
            connectedVertices.Add(new Tuple<int, int>(15, 16));
            connectedVertices.Add(new Tuple<int, int>(16, 17));
            connectedVertices.Add(new Tuple<int, int>(16, 27));
            connectedVertices.Add(new Tuple<int, int>(16, 46));
            connectedVertices.Add(new Tuple<int, int>(17, 18));
            connectedVertices.Add(new Tuple<int, int>(17, 19));
            connectedVertices.Add(new Tuple<int, int>(18, 19));
            connectedVertices.Add(new Tuple<int, int>(20, 21));
            connectedVertices.Add(new Tuple<int, int>(20, 22));
            connectedVertices.Add(new Tuple<int, int>(21, 22));
            connectedVertices.Add(new Tuple<int, int>(21, 27));
            connectedVertices.Add(new Tuple<int, int>(21, 28));
            connectedVertices.Add(new Tuple<int, int>(23, 24));
            connectedVertices.Add(new Tuple<int, int>(23, 26));
            connectedVertices.Add(new Tuple<int, int>(23, 27));
            connectedVertices.Add(new Tuple<int, int>(24, 25));
            connectedVertices.Add(new Tuple<int, int>(24, 27));
            connectedVertices.Add(new Tuple<int, int>(25, 26));
            connectedVertices.Add(new Tuple<int, int>(27, 28));
            connectedVertices.Add(new Tuple<int, int>(27, 30));
            connectedVertices.Add(new Tuple<int, int>(27, 31));
            connectedVertices.Add(new Tuple<int, int>(28, 29));
            connectedVertices.Add(new Tuple<int, int>(28, 30));
            connectedVertices.Add(new Tuple<int, int>(29, 30));
            connectedVertices.Add(new Tuple<int, int>(31, 33));
            connectedVertices.Add(new Tuple<int, int>(31, 35));
            connectedVertices.Add(new Tuple<int, int>(31, 41));
            connectedVertices.Add(new Tuple<int, int>(31, 46));
            connectedVertices.Add(new Tuple<int, int>(31, 61));
            connectedVertices.Add(new Tuple<int, int>(32, 33));
            connectedVertices.Add(new Tuple<int, int>(32, 36));
            connectedVertices.Add(new Tuple<int, int>(32, 37));
            connectedVertices.Add(new Tuple<int, int>(33, 36));
            connectedVertices.Add(new Tuple<int, int>(34, 35));
            connectedVertices.Add(new Tuple<int, int>(34, 36));
            connectedVertices.Add(new Tuple<int, int>(34, 37));
            connectedVertices.Add(new Tuple<int, int>(34, 38));
            connectedVertices.Add(new Tuple<int, int>(35, 36));
            connectedVertices.Add(new Tuple<int, int>(35, 40));
            connectedVertices.Add(new Tuple<int, int>(35, 41));
            connectedVertices.Add(new Tuple<int, int>(37, 39));
            connectedVertices.Add(new Tuple<int, int>(38, 39));
            connectedVertices.Add(new Tuple<int, int>(40, 41));
            connectedVertices.Add(new Tuple<int, int>(40, 42));
            connectedVertices.Add(new Tuple<int, int>(41, 43));
            connectedVertices.Add(new Tuple<int, int>(42, 43));
            connectedVertices.Add(new Tuple<int, int>(42, 44));
            connectedVertices.Add(new Tuple<int, int>(43, 45));
            connectedVertices.Add(new Tuple<int, int>(44, 45));
            connectedVertices.Add(new Tuple<int, int>(46, 47));
            connectedVertices.Add(new Tuple<int, int>(46, 48));
            connectedVertices.Add(new Tuple<int, int>(46, 55));
            connectedVertices.Add(new Tuple<int, int>(46, 57));
            connectedVertices.Add(new Tuple<int, int>(47, 48));
            connectedVertices.Add(new Tuple<int, int>(47, 50));
            connectedVertices.Add(new Tuple<int, int>(48, 49));
            connectedVertices.Add(new Tuple<int, int>(49, 50));
            connectedVertices.Add(new Tuple<int, int>(49, 53));
            connectedVertices.Add(new Tuple<int, int>(50, 51));
            connectedVertices.Add(new Tuple<int, int>(51, 52));
            connectedVertices.Add(new Tuple<int, int>(51, 54));
            connectedVertices.Add(new Tuple<int, int>(52, 54));
            connectedVertices.Add(new Tuple<int, int>(52, 53));
            connectedVertices.Add(new Tuple<int, int>(53, 54));
            connectedVertices.Add(new Tuple<int, int>(55, 56));
            connectedVertices.Add(new Tuple<int, int>(55, 57));
            connectedVertices.Add(new Tuple<int, int>(56, 58));
            connectedVertices.Add(new Tuple<int, int>(56, 60));
            connectedVertices.Add(new Tuple<int, int>(57, 58));
            connectedVertices.Add(new Tuple<int, int>(58, 59));
            connectedVertices.Add(new Tuple<int, int>(59, 60));

            List<Edge> edges = new List<Edge>();
            foreach (var e in connectedVertices)
            {
                edges.Add(new Edge(edges.Count.ToString(), vertices[e.Item1], vertices[e.Item2], double.NaN));
            }

            Network net = new Network(edges);

           

            return net;
        }
    }
}
