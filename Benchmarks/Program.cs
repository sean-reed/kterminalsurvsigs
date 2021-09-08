using KTerminalNetworkBDD;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Tuple<string, Network>> benchmarkNetworks = new List<Tuple<string, Network>>();
            benchmarkNetworks.Add(new Tuple<string, Network>("net2_8", Get_net_2_8()));
            benchmarkNetworks.Add(new Tuple<string, Network>("net2_8_2T", Get_net_2_8_2T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("net2_8_3T", Get_net_2_8_3T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("net.19", Get_net19()));
            benchmarkNetworks.Add(new Tuple<string, Network>("net.19", Get_net19_2T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("net.19_3T", Get_net19_3T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("network(5)", Get_network5()));
            benchmarkNetworks.Add(new Tuple<string, Network>("network(5)_2T", Get_network5_2T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("network(5)_3T", Get_network5_3T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("8x8", Get_8x8()));
            benchmarkNetworks.Add(new Tuple<string, Network>("8x8_2T", Get_8x8_2T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("8x8_3T", Get_8x8_3T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("12x12", Get_12x12()));
            benchmarkNetworks.Add(new Tuple<string, Network>("12x12_2T", Get_12x12_2T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("12x12_3T", Get_12x12_3T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("3x12", Get_3x12()));
            benchmarkNetworks.Add(new Tuple<string, Network>("3x12_2T", Get_3x12_2T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("3x12_3T", Get_3x12_3T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("3x100", Get_3x100()));
            benchmarkNetworks.Add(new Tuple<string, Network>("3x100_2T", Get_3x100_2T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("3x100_3T", Get_3x100_3T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("K10", Get_K10()));
            benchmarkNetworks.Add(new Tuple<string, Network>("K10", Get_K10_2T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("K10_3T", Get_K10_3T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("K12", Get_K12()));
            benchmarkNetworks.Add(new Tuple<string, Network>("K12_2T", Get_K12_2T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("K12_3T", Get_K12_3T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("K7,15", Get_K7_15()));
            benchmarkNetworks.Add(new Tuple<string, Network>("K7,15_2T", Get_K7_15_2T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("K7,15_3T", Get_K7_15_3T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("K7,50", Get_K7_50()));
            benchmarkNetworks.Add(new Tuple<string, Network>("K7,50_2T", Get_K7_50_2T()));
            benchmarkNetworks.Add(new Tuple<string, Network>("K7,50_3T", Get_K7_50_3T()));

            RunBenchmarks(benchmarkNetworks);
        }

        private static void RunBenchmarks(List<Tuple<string, Network>> networks)
        {
            foreach (var bm in networks)
            {
                string name = bm.Item1;
                Network net = bm.Item2;

                Console.WriteLine("Network: " + name);
                WriteNetworkStats(net);
                WriteBenchmarkForSignature(net);
            }
        }

        private static Network Get_8x8()
        {
            Func<int, double> edgeReliabilityFunc = (eIndex) => 0.9;
            return GridNetworkGenerator.GenerateTwoTerminal(8, 8, edgeReliabilityFunc);
        }

        private static Network Get_8x8_2T()
        {
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
               if (eIndex % 2 == 0)
                    return 0.9;
                else
                    return 0.8;
            };
            return GridNetworkGenerator.GenerateTwoTerminal(8, 8, edgeReliabilityFunc);
        }

        private static Network Get_8x8_3T()
        {
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 3 == 0)
                    return 0.9;
                else if (eIndex % 2 == 0)
                    return 0.8;
                else
                    return 0.7;
            };
            return GridNetworkGenerator.GenerateTwoTerminal(8, 8, edgeReliabilityFunc);
        }

        private static Network Get_12x12()
        {
            Func<int, double> edgeReliabilityFunc = (eIndex) => 0.9;
            return GridNetworkGenerator.GenerateTwoTerminal(12, 12, edgeReliabilityFunc);
        }

        private static Network Get_12x12_2T()
        {
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
   
                if (eIndex % 2 == 0)
                    return 0.9;
                else
                    return 0.8;
            };
            return GridNetworkGenerator.GenerateTwoTerminal(12, 12, edgeReliabilityFunc);
        }

        private static Network Get_12x12_3T()
        {
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 3 == 0)
                    return 0.9;
                else if (eIndex % 2 == 0)
                    return 0.8;
                else
                    return 0.7;
            };
            return GridNetworkGenerator.GenerateTwoTerminal(12, 12, edgeReliabilityFunc);
        }

        private static Network Get_3x12()
        {
            Func<int, double> edgeReliabilityFunc = (eIndex) => 0.9;
            return GridNetworkGenerator.GenerateTwoTerminal(12, 3, edgeReliabilityFunc);
        }

        private static Network Get_3x12_2T()
        {
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 2 == 0)
                    return 0.9;
                else
                    return 0.8;
            };
            return GridNetworkGenerator.GenerateTwoTerminal(12, 3, edgeReliabilityFunc);
        }

        private static Network Get_3x12_3T()
        {
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 3 == 0)
                    return 0.9;
                else if (eIndex % 2 == 0)
                    return 0.8;
                else
                    return 0.7;
            };
            return GridNetworkGenerator.GenerateTwoTerminal(12, 3, edgeReliabilityFunc);
        }

        private static Network Get_3x100()
        {
            Func<int, double> edgeReliabilityFunc = (eIndex) => 0.9;
            return GridNetworkGenerator.GenerateTwoTerminal(100, 3, edgeReliabilityFunc);
        }

        private static Network Get_3x100_2T()
        {
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 2 == 0)
                    return 0.9;
                else
                    return 0.8;
            };
            return GridNetworkGenerator.GenerateTwoTerminal(100, 3, edgeReliabilityFunc);
        }

        private static Network Get_3x100_3T()
        {
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 3 == 0)
                    return 0.9;
                else if (eIndex % 2 == 0)
                    return 0.8;
                else
                    return 0.7;
            };
            return GridNetworkGenerator.GenerateTwoTerminal(100, 3, edgeReliabilityFunc);
        }


        private static Network Get_K10_3T()
        {
            Func<int, bool> isTerminalPredicate = (vIndex) => true;
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 3 == 0)
                    return 0.9;
                else if (eIndex % 2 == 0)
                    return 0.8;
                else
                    return 0.7;
            };

            var k10 =
                FullyConnectedNetworkGenerator.GenerateFullyConnected(10, isTerminalPredicate,
                    edgeReliabilityFunc);

            return k10;
        }


        private static Network Get_K10_2T()
        {
            Func<int, bool> isTerminalPredicate = (vIndex) => true;
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 2 == 0)
                    return 0.9;
                else
                    return 0.8;
            };

            var k10 =
                FullyConnectedNetworkGenerator.GenerateFullyConnected(10, isTerminalPredicate,
                    edgeReliabilityFunc);

            return k10;
        }

        private static Network Get_K10()
        {
            Func<int, bool> isTerminalPredicate = (vIndex) => true;
            Func<int, double> edgeReliabilityFunc = (eIndex) => 0.9;
            var k10 =
                FullyConnectedNetworkGenerator.GenerateFullyConnected(10, isTerminalPredicate,
                    edgeReliabilityFunc);

            return k10;
        }

        private static Network Get_K12_2T()
        {
            Func<int, bool> isTerminalPredicate = (vIndex) => true;
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 2 == 0)
                    return 0.9;
                else
                    return 0.8;
            };

            var k12 =
                FullyConnectedNetworkGenerator.GenerateFullyConnected(12, isTerminalPredicate,
                    edgeReliabilityFunc);

            return k12;
        }

        private static Network Get_K12_3T()
        {
            Func<int, bool> isTerminalPredicate = (vIndex) => true;
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 3 == 0)
                    return 0.9;
                else if (eIndex % 2 == 0)
                    return 0.8;
                else
                    return 0.7;
            };

            var k12 =
                FullyConnectedNetworkGenerator.GenerateFullyConnected(12, isTerminalPredicate,
                    edgeReliabilityFunc);

            return k12;
        }

        private static Network Get_K12()
        {
            Func<int, bool> isTerminalPredicate = (vIndex) => true;
            Func<int, double> edgeReliabilityFunc = (eIndex) => 0.9;
            var k12 =
                FullyConnectedNetworkGenerator.GenerateFullyConnected(12, isTerminalPredicate,
                    edgeReliabilityFunc);

            return k12;
        }

        private static Network Get_K7_15()
        {
            Func<int, bool> isTerminalPredicate = (vIndex) => true;
            Func<int, double> edgeReliabilityFunc = (eIndex) => 0.9;
            var netK7_15 =
                FullyConnectedNetworkGenerator.GenerateClosestNFullyConnected(15, 7, isTerminalPredicate,
                    edgeReliabilityFunc);

            return netK7_15;
        }

        private static Network Get_K7_15_2T()
        {
            Func<int, bool> isTerminalPredicate = (vIndex) => true;
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 2 == 0)
                    return 0.9;
                else
                    return 0.8;
            };
            var netK7_15_2T =
                FullyConnectedNetworkGenerator.GenerateClosestNFullyConnected(15, 7, isTerminalPredicate,
                    edgeReliabilityFunc);

            return netK7_15_2T;
        }


        private static Network Get_K7_15_3T()
        {
            Func<int, bool> isTerminalPredicate = (vIndex) => true;
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 3 == 0)
                    return 0.9;
                else if (eIndex % 2 == 0)
                    return 0.8;
                else
                    return 0.7;
            };
            var netK7_15_3T =
                FullyConnectedNetworkGenerator.GenerateClosestNFullyConnected(15, 7, isTerminalPredicate,
                    edgeReliabilityFunc);

            return netK7_15_3T;
        }

        private static Network Get_K7_50()
        {
            Func<int, bool> isTerminalPredicate = (vIndex) => true;
            Func<int, double> edgeReliabilityFunc = (eIndex) => 0.9;
            var netK7_50 =
                FullyConnectedNetworkGenerator.GenerateClosestNFullyConnected(50, 7, isTerminalPredicate,
                    edgeReliabilityFunc);

            return netK7_50;
        }

        private static Network Get_K7_50_2T()
        {
            Func<int, bool> isTerminalPredicate = (vIndex) => true;
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 2 == 0)
                    return 0.9;
                else
                    return 0.8;
            };
            var netK7_50_2T =
                FullyConnectedNetworkGenerator.GenerateClosestNFullyConnected(50, 7, isTerminalPredicate,
                    edgeReliabilityFunc);

            return netK7_50_2T;
        }

        private static Network Get_K7_50_3T()
        {
            Func<int, bool> isTerminalPredicate = (vIndex) => true;
            Func<int, double> edgeReliabilityFunc = (eIndex) =>
            {
                if (eIndex % 3 == 0)
                    return 0.9;
                else if (eIndex % 2 == 0)
                    return 0.8;
                else
                    return 0.7;
            };
            var netK7_50_3T =
                FullyConnectedNetworkGenerator.GenerateClosestNFullyConnected(50, 7, isTerminalPredicate,
                    edgeReliabilityFunc);

            return netK7_50_3T;
        }

        private static Network Get_net_2_8_2T()
        {
            // Construct networks from Figure 9 of "OBDD-Based Evaluation of k-Terminal Network Reliability", Yeh et al, 2002.
            // net2_8
            Vertex s = new Vertex("s", true);
            Vertex v1 = new Vertex("1", false);
            Vertex v2 = new Vertex("2", false);
            Vertex v3 = new Vertex("3", false);
            Vertex t = new Vertex("t", true);
            Vertex v4 = new Vertex("4", false);
            Vertex v5 = new Vertex("5", false);
            Vertex v6 = new Vertex("6", false);
            Vertex v7 = new Vertex("7", false);
            Vertex v8 = new Vertex("8", false);
            Vertex v9 = new Vertex("9", false);
            Vertex v10 = new Vertex("10", false);
            Vertex v11 = new Vertex("11", false);
            Vertex v12 = new Vertex("12", false);
            Vertex v13 = new Vertex("13", false);
            Vertex v14 = new Vertex("14", false);
            List<Vertex> net2_8_vertices = new List<Vertex>()
            {
                s,
                v1,
                v2,
                v3,
                t,
                v4,
                v5,
                v6,
                v7,
                v8,
                v9,
                v10,
                v11,
                v12,
                v13,
                v14
            };

            double t1EdgeReliability = 0.9;
            double t2EdgeReliability = 0.8;
            Edge e1 = new Edge("1", s, v1, t1EdgeReliability);
            Edge e2 = new Edge("2", v1, v2, t1EdgeReliability);
            Edge e3 = new Edge("3", v2, v3, t2EdgeReliability);
            Edge e4 = new Edge("4", v3, t, t2EdgeReliability);
            Edge e5 = new Edge("5", t, v4, t1EdgeReliability);
            Edge e6 = new Edge("6", v4, v5, t2EdgeReliability);
            Edge e7 = new Edge("7", v5, v6, t2EdgeReliability);
            Edge e8 = new Edge("8", v6, s, t1EdgeReliability);
            Edge e9 = new Edge("9", s, v7, t1EdgeReliability);
            Edge e10 = new Edge("10", v1, v8, t1EdgeReliability);
            Edge e11 = new Edge("11", v2, v9, t2EdgeReliability);
            Edge e12 = new Edge("12", v3, v10, t1EdgeReliability);
            Edge e13 = new Edge("13", t, v11, t2EdgeReliability);
            Edge e14 = new Edge("14", v4, v12, t1EdgeReliability);
            Edge e15 = new Edge("15", v5, v13, t2EdgeReliability);
            Edge e16 = new Edge("16", v6, v14, t2EdgeReliability);
            Edge e17 = new Edge("17", v7, v8, t2EdgeReliability);
            Edge e18 = new Edge("18", v8, v9, t2EdgeReliability);
            Edge e19 = new Edge("19", v9, v10, t2EdgeReliability);
            Edge e20 = new Edge("20", v10, v11, t1EdgeReliability);
            Edge e21 = new Edge("21", v11, v12, t1EdgeReliability);
            Edge e22 = new Edge("22", v12, v13, t2EdgeReliability);
            Edge e23 = new Edge("23", v13, v14, t1EdgeReliability);
            Edge e24 = new Edge("24", v14, v7, t1EdgeReliability);

            List<Edge> net2_8_edges = new List<Edge>()
            {
                e1,
                e2,
                e3,
                e4,
                e5,
                e6,
                e7,
                e8,
                e9,
                e10,
                e11,
                e12,
                e13,
                e14,
                e15,
                e16,
                e17,
                e18,
                e19,
                e20,
                e21,
                e22,
                e23,
                e24
            };

            Network net2_8 = new Network(net2_8_edges);

            return net2_8;
        }

        private static Network Get_net_2_8_3T()
        {
            // Construct networks from Figure 9 of "OBDD-Based Evaluation of k-Terminal Network Reliability", Yeh et al, 2002.
            // net2_8
            Vertex s = new Vertex("s", true);
            Vertex v1 = new Vertex("1", false);
            Vertex v2 = new Vertex("2", false);
            Vertex v3 = new Vertex("3", false);
            Vertex t = new Vertex("t", true);
            Vertex v4 = new Vertex("4", false);
            Vertex v5 = new Vertex("5", false);
            Vertex v6 = new Vertex("6", false);
            Vertex v7 = new Vertex("7", false);
            Vertex v8 = new Vertex("8", false);
            Vertex v9 = new Vertex("9", false);
            Vertex v10 = new Vertex("10", false);
            Vertex v11 = new Vertex("11", false);
            Vertex v12 = new Vertex("12", false);
            Vertex v13 = new Vertex("13", false);
            Vertex v14 = new Vertex("14", false);
            List<Vertex> net2_8_vertices = new List<Vertex>()
            {
                s,
                v1,
                v2,
                v3,
                t,
                v4,
                v5,
                v6,
                v7,
                v8,
                v9,
                v10,
                v11,
                v12,
                v13,
                v14
            };

            double t1EdgeReliability = 0.9;
            double t2EdgeReliability = 0.8;
            double t3EdgeReliability = 0.7;
            Edge e1 = new Edge("1", s, v1, t1EdgeReliability);
            Edge e2 = new Edge("2", v1, v2, t1EdgeReliability);
            Edge e3 = new Edge("3", v2, v3, t3EdgeReliability);
            Edge e4 = new Edge("4", v3, t, t3EdgeReliability);
            Edge e5 = new Edge("5", t, v4, t1EdgeReliability);
            Edge e6 = new Edge("6", v4, v5, t2EdgeReliability);
            Edge e7 = new Edge("7", v5, v6, t2EdgeReliability);
            Edge e8 = new Edge("8", v6, s, t1EdgeReliability);
            Edge e9 = new Edge("9", s, v7, t1EdgeReliability);
            Edge e10 = new Edge("10", v1, v8, t1EdgeReliability);
            Edge e11 = new Edge("11", v2, v9, t2EdgeReliability);
            Edge e12 = new Edge("12", v3, v10, t3EdgeReliability);
            Edge e13 = new Edge("13", t, v11, t2EdgeReliability);
            Edge e14 = new Edge("14", v4, v12, t3EdgeReliability);
            Edge e15 = new Edge("15", v5, v13, t2EdgeReliability);
            Edge e16 = new Edge("16", v6, v14, t3EdgeReliability);
            Edge e17 = new Edge("17", v7, v8, t2EdgeReliability);
            Edge e18 = new Edge("18", v8, v9, t3EdgeReliability);
            Edge e19 = new Edge("19", v9, v10, t2EdgeReliability);
            Edge e20 = new Edge("20", v10, v11, t3EdgeReliability);
            Edge e21 = new Edge("21", v11, v12, t3EdgeReliability);
            Edge e22 = new Edge("22", v12, v13, t2EdgeReliability);
            Edge e23 = new Edge("23", v13, v14, t1EdgeReliability);
            Edge e24 = new Edge("24", v14, v7, t1EdgeReliability);

            List<Edge> net2_8_edges = new List<Edge>()
            {
                e1,
                e2,
                e3,
                e4,
                e5,
                e6,
                e7,
                e8,
                e9,
                e10,
                e11,
                e12,
                e13,
                e14,
                e15,
                e16,
                e17,
                e18,
                e19,
                e20,
                e21,
                e22,
                e23,
                e24
            };

            Network net2_8 = new Network(net2_8_edges);

            return net2_8;
        }

        private static Network Get_net19_3T()
        {
            double t1EdgeReliability = 0.9;
            double t2EdgeReliability = 0.8;
            double t3EdgeReliability = 0.7;

            // net_19 from Fig. 9 in "OBDD-Based evaluation of k-terminal network reliability" Yeh et al, 2002.
            Vertex va = new Vertex("a", true);
            Vertex vb = new Vertex("b", false);
            Vertex vc = new Vertex("c", false);
            Vertex vd = new Vertex("d", false);
            Vertex ve = new Vertex("e", false);
            Vertex vf = new Vertex("f", false);
            Vertex vg = new Vertex("g", false);
            Vertex vh = new Vertex("h", false);
            Vertex vi = new Vertex("i", false);
            Vertex vj = new Vertex("j", true);
            Vertex vk = new Vertex("k", false);
            Vertex vl = new Vertex("l", false);
            Vertex vm = new Vertex("m", false);
            Vertex vn = new Vertex("n", false);
            Vertex vo = new Vertex("o", false);
            Vertex vp = new Vertex("p", false);
            Vertex vq = new Vertex("q", false);
            Vertex vr = new Vertex("r", false);
            Vertex vs = new Vertex("s", false);
            Vertex vt = new Vertex("t", false);

            Edge e1 = new Edge("1", va, vr, t1EdgeReliability);
            Edge e2 = new Edge("2", va, vb, t2EdgeReliability);
            Edge e3 = new Edge("3", va, vs, t3EdgeReliability);
            Edge e4 = new Edge("4", vb, vc, t1EdgeReliability);
            Edge e5 = new Edge("5", vb, vd, t1EdgeReliability);
            Edge e6 = new Edge("6", vc, ve, t3EdgeReliability);
            Edge e7 = new Edge("7", ve, vf, t2EdgeReliability);
            Edge e8 = new Edge("8", vd, vf, t1EdgeReliability);
            Edge e9 = new Edge("9", vd, vm, t3EdgeReliability);
            Edge e10 = new Edge("10", vc, vl, t2EdgeReliability);
            Edge e11 = new Edge("11", ve, vg, t2EdgeReliability);
            Edge e12 = new Edge("12", vf, vh, t3EdgeReliability);
            Edge e13 = new Edge("13", vl, vi, t2EdgeReliability);
            Edge e14 = new Edge("14", vl, vn, t3EdgeReliability);
            Edge e15 = new Edge("15", vg, vi, t1EdgeReliability);
            Edge e16 = new Edge("16", vg, vj, t1EdgeReliability);
            Edge e17 = new Edge("17", vh, vj, t3EdgeReliability);
            Edge e18 = new Edge("18", vh, vk, t2EdgeReliability);
            Edge e19 = new Edge("19", vk, vm, t2EdgeReliability);
            Edge e20 = new Edge("20", vi, vp, t3EdgeReliability);
            Edge e21 = new Edge("21", vj, vo, t3EdgeReliability);
            Edge e22 = new Edge("22", vk, vq, t1EdgeReliability);
            Edge e23 = new Edge("23", vo, vp, t3EdgeReliability);
            Edge e24 = new Edge("24", vo, vq, t2EdgeReliability);
            Edge e25 = new Edge("25", vm, vt, t3EdgeReliability);
            Edge e26 = new Edge("26", vn, vr, t2EdgeReliability);
            Edge e27 = new Edge("27", vp, vr, t2EdgeReliability);
            Edge e28 = new Edge("28", vq, vs, t1EdgeReliability);
            Edge e29 = new Edge("29", vt, vs, t1EdgeReliability);
            Edge e30 = new Edge("30", vr, vs, t1EdgeReliability);

            List<Edge> edges = new List<Edge>()
            {
                e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11, e12, e13, e14, e15, e16, e17, e18, e19,
                e20, e21, e22, e23, e24, e25, e26, e27, e28, e29, e30
            };

            return new Network(edges);
        }

        private static Network Get_net19_2T()
        {
            double t1EdgeReliability = 0.9;
            double t2EdgeReliability = 0.8;

            // net_19 from Fig. 9 in "OBDD-Based evaluation of k-terminal network reliability" Yeh et al, 2002.
            Vertex va = new Vertex("a", true);
            Vertex vb = new Vertex("b", false);
            Vertex vc = new Vertex("c", false);
            Vertex vd = new Vertex("d", false);
            Vertex ve = new Vertex("e", false);
            Vertex vf = new Vertex("f", false);
            Vertex vg = new Vertex("g", false);
            Vertex vh = new Vertex("h", false);
            Vertex vi = new Vertex("i", false);
            Vertex vj = new Vertex("j", true);
            Vertex vk = new Vertex("k", false);
            Vertex vl = new Vertex("l", false);
            Vertex vm = new Vertex("m", false);
            Vertex vn = new Vertex("n", false);
            Vertex vo = new Vertex("o", false);
            Vertex vp = new Vertex("p", false);
            Vertex vq = new Vertex("q", false);
            Vertex vr = new Vertex("r", false);
            Vertex vs = new Vertex("s", false);
            Vertex vt = new Vertex("t", false);

            Edge e1 = new Edge("1", va, vr, t1EdgeReliability);
            Edge e2 = new Edge("2", va, vb, t2EdgeReliability);
            Edge e3 = new Edge("3", va, vs, t1EdgeReliability);
            Edge e4 = new Edge("4", vb, vc, t1EdgeReliability);
            Edge e5 = new Edge("5", vb, vd, t1EdgeReliability);
            Edge e6 = new Edge("6", vc, ve, t2EdgeReliability);
            Edge e7 = new Edge("7", ve, vf, t2EdgeReliability);
            Edge e8 = new Edge("8", vd, vf, t1EdgeReliability);
            Edge e9 = new Edge("9", vd, vm, t2EdgeReliability);
            Edge e10 = new Edge("10", vc, vl, t2EdgeReliability);
            Edge e11 = new Edge("11", ve, vg, t2EdgeReliability);
            Edge e12 = new Edge("12", vf, vh, t1EdgeReliability);
            Edge e13 = new Edge("13", vl, vi, t2EdgeReliability);
            Edge e14 = new Edge("14", vl, vn, t1EdgeReliability);
            Edge e15 = new Edge("15", vg, vi, t1EdgeReliability);
            Edge e16 = new Edge("16", vg, vj, t1EdgeReliability);
            Edge e17 = new Edge("17", vh, vj, t2EdgeReliability);
            Edge e18 = new Edge("18", vh, vk, t2EdgeReliability);
            Edge e19 = new Edge("19", vk, vm, t2EdgeReliability);
            Edge e20 = new Edge("20", vi, vp, t2EdgeReliability);
            Edge e21 = new Edge("21", vj, vo, t1EdgeReliability);
            Edge e22 = new Edge("22", vk, vq, t1EdgeReliability);
            Edge e23 = new Edge("23", vo, vp, t2EdgeReliability);
            Edge e24 = new Edge("24", vo, vq, t2EdgeReliability);
            Edge e25 = new Edge("25", vm, vt, t1EdgeReliability);
            Edge e26 = new Edge("26", vn, vr, t2EdgeReliability);
            Edge e27 = new Edge("27", vp, vr, t2EdgeReliability);
            Edge e28 = new Edge("28", vq, vs, t1EdgeReliability);
            Edge e29 = new Edge("29", vt, vs, t1EdgeReliability);
            Edge e30 = new Edge("30", vr, vs, t1EdgeReliability);

            List<Edge> edges = new List<Edge>()
            {
                e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11, e12, e13, e14, e15, e16, e17, e18, e19,
                e20, e21, e22, e23, e24, e25, e26, e27, e28, e29, e30
            };

            return new Network(edges);
        }

        private static Network Get_net19()
        {
            // net.19 from Fig. 9 in "OBDD-Based evaluation of k-terminal network reliability" Yeh et al, 2002.
            Vertex va = new Vertex("a", true);
            Vertex vb = new Vertex("b", false);
            Vertex vc = new Vertex("c", false);
            Vertex vd = new Vertex("d", false);
            Vertex ve = new Vertex("e", false);
            Vertex vf = new Vertex("f", false);
            Vertex vg = new Vertex("g", false);
            Vertex vh = new Vertex("h", false);
            Vertex vi = new Vertex("i", false);
            Vertex vj = new Vertex("j", true);
            Vertex vk = new Vertex("k", false);
            Vertex vl = new Vertex("l", false);
            Vertex vm = new Vertex("m", false);
            Vertex vn = new Vertex("n", false);
            Vertex vo = new Vertex("o", false);
            Vertex vp = new Vertex("p", false);
            Vertex vq = new Vertex("q", false);
            Vertex vr = new Vertex("r", false);
            Vertex vs = new Vertex("s", false);
            Vertex vt = new Vertex("t", false);

            Edge e1 = new Edge("1", va, vr, 0.9);
            Edge e2 = new Edge("2", va, vb, 0.9);
            Edge e3 = new Edge("3", va, vs, 0.9);
            Edge e4 = new Edge("4", vb, vc, 0.9);
            Edge e5 = new Edge("5", vb, vd, 0.9);
            Edge e6 = new Edge("6", vc, ve, 0.9);
            Edge e7 = new Edge("7", ve, vf, 0.9);
            Edge e8 = new Edge("8", vd, vf, 0.9);
            Edge e9 = new Edge("9", vd, vm, 0.9);
            Edge e10 = new Edge("10", vc, vl, 0.9);
            Edge e11 = new Edge("11", ve, vg, 0.9);
            Edge e12 = new Edge("12", vf, vh, 0.9);
            Edge e13 = new Edge("13", vl, vi, 0.9);
            Edge e14 = new Edge("14", vl, vn, 0.9);
            Edge e15 = new Edge("15", vg, vi, 0.9);
            Edge e16 = new Edge("16", vg, vj, 0.9);
            Edge e17 = new Edge("17", vh, vj, 0.9);
            Edge e18 = new Edge("18", vh, vk, 0.9);
            Edge e19 = new Edge("19", vk, vm, 0.9);
            Edge e20 = new Edge("20", vi, vp, 0.9);
            Edge e21 = new Edge("21", vj, vo, 0.9);
            Edge e22 = new Edge("22", vk, vq, 0.9);
            Edge e23 = new Edge("23", vo, vp, 0.9);
            Edge e24 = new Edge("24", vo, vq, 0.9);
            Edge e25 = new Edge("25", vm, vt, 0.9);
            Edge e26 = new Edge("26", vn, vr, 0.9);
            Edge e27 = new Edge("27", vp, vr, 0.9);
            Edge e28 = new Edge("28", vq, vs, 0.9);
            Edge e29 = new Edge("29", vt, vs, 0.9);
            Edge e30 = new Edge("30", vr, vs, 0.9);

            List<Edge> edges = new List<Edge>()
            {
                e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11, e12, e13, e14, e15, e16, e17, e18, e19,
                e20, e21, e22, e23, e24, e25, e26, e27, e28, e29, e30
            };

            return new Network(edges);
        }

        private static Network Get_network5_3T()
        {
            double t1EdgeReliability = 0.9;
            double t2EdgeReliability = 0.8;
            double t3EdgeReliability = 0.7;

            // Network(5) from Fig. 9 in "OBDD-Based evaluation of k-terminal network reliability" Yeh et al, 2002.
            Vertex va = new Vertex("a", true);
            Vertex vb = new Vertex("b", false);
            Vertex vc = new Vertex("c", false);
            Vertex vd = new Vertex("d", false);
            Vertex ve = new Vertex("e", false);
            Vertex vf = new Vertex("f", false);
            Vertex vg = new Vertex("g", true);
            Vertex vh = new Vertex("h", true);
            Vertex vi = new Vertex("i", true);
            Vertex vj = new Vertex("j", true);
            Vertex vk = new Vertex("k", true);
            Vertex vl = new Vertex("l", false);
            Vertex vm = new Vertex("m", false);
            Vertex vn = new Vertex("n", false);
            Vertex vo = new Vertex("o", true);
            Vertex vp = new Vertex("p", true);
            Vertex vq = new Vertex("q", true);
            Vertex vr = new Vertex("r", false);
            Vertex vs = new Vertex("s", false);
            Vertex vt = new Vertex("t", false);

            Edge e1 = new Edge("1", va, vr, t1EdgeReliability);
            Edge e2 = new Edge("2", va, vb, t2EdgeReliability);
            Edge e3 = new Edge("3", va, vs, t3EdgeReliability);
            Edge e4 = new Edge("4", vb, vc, t1EdgeReliability);
            Edge e5 = new Edge("5", vb, vd, t1EdgeReliability);
            Edge e6 = new Edge("6", vc, ve, t3EdgeReliability);
            Edge e7 = new Edge("7", ve, vf, t2EdgeReliability);
            Edge e8 = new Edge("8", vd, vf, t1EdgeReliability);
            Edge e9 = new Edge("9", vd, vm, t3EdgeReliability);
            Edge e10 = new Edge("10", vc, vl, t2EdgeReliability);
            Edge e11 = new Edge("11", ve, vg, t2EdgeReliability);
            Edge e12 = new Edge("12", vf, vh, t3EdgeReliability);
            Edge e13 = new Edge("13", vl, vi, t2EdgeReliability);
            Edge e14 = new Edge("14", vl, vn, t3EdgeReliability);
            Edge e15 = new Edge("15", vg, vi, t1EdgeReliability);
            Edge e16 = new Edge("16", vg, vj, t1EdgeReliability);
            Edge e17 = new Edge("17", vh, vj, t3EdgeReliability);
            Edge e18 = new Edge("18", vh, vk, t2EdgeReliability);
            Edge e19 = new Edge("19", vk, vm, t2EdgeReliability);
            Edge e20 = new Edge("20", vi, vp, t3EdgeReliability);
            Edge e21 = new Edge("21", vj, vo, t3EdgeReliability);
            Edge e22 = new Edge("22", vk, vq, t1EdgeReliability);
            Edge e23 = new Edge("23", vo, vp, t3EdgeReliability);
            Edge e24 = new Edge("24", vo, vq, t2EdgeReliability);
            Edge e25 = new Edge("25", vm, vt, t3EdgeReliability);
            Edge e26 = new Edge("26", vn, vr, t2EdgeReliability);
            Edge e27 = new Edge("27", vp, vr, t2EdgeReliability);
            Edge e28 = new Edge("28", vq, vs, t1EdgeReliability);
            Edge e29 = new Edge("29", vt, vs, t1EdgeReliability);
            Edge e30 = new Edge("30", vr, vs, t1EdgeReliability);

            List<Edge> edges = new List<Edge>()
            {
                e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11, e12, e13, e14, e15, e16, e17, e18, e19,
                e20, e21, e22, e23, e24, e25, e26, e27, e28, e29, e30
            };

            return new Network(edges);
        }

        private static Network Get_network5_2T()
        {
            double t1EdgeReliability = 0.9;
            double t2EdgeReliability = 0.8;

            // Network(5) from Fig. 9 in "OBDD-Based evaluation of k-terminal network reliability" Yeh et al, 2002.
            Vertex va = new Vertex("a", true);
            Vertex vb = new Vertex("b", false);
            Vertex vc = new Vertex("c", false);
            Vertex vd = new Vertex("d", false);
            Vertex ve = new Vertex("e", false);
            Vertex vf = new Vertex("f", false);
            Vertex vg = new Vertex("g", true);
            Vertex vh = new Vertex("h", true);
            Vertex vi = new Vertex("i", true);
            Vertex vj = new Vertex("j", true);
            Vertex vk = new Vertex("k", true);
            Vertex vl = new Vertex("l", false);
            Vertex vm = new Vertex("m", false);
            Vertex vn = new Vertex("n", false);
            Vertex vo = new Vertex("o", true);
            Vertex vp = new Vertex("p", true);
            Vertex vq = new Vertex("q", true);
            Vertex vr = new Vertex("r", false);
            Vertex vs = new Vertex("s", false);
            Vertex vt = new Vertex("t", false);

            Edge e1 = new Edge("1", va, vr, t1EdgeReliability);
            Edge e2 = new Edge("2", va, vb, t2EdgeReliability);
            Edge e3 = new Edge("3", va, vs, t2EdgeReliability);
            Edge e4 = new Edge("4", vb, vc, t1EdgeReliability);
            Edge e5 = new Edge("5", vb, vd, t1EdgeReliability);
            Edge e6 = new Edge("6", vc, ve, t1EdgeReliability);
            Edge e7 = new Edge("7", ve, vf, t2EdgeReliability);
            Edge e8 = new Edge("8", vd, vf, t1EdgeReliability);
            Edge e9 = new Edge("9", vd, vm, t1EdgeReliability);
            Edge e10 = new Edge("10", vc, vl, t2EdgeReliability);
            Edge e11 = new Edge("11", ve, vg, t2EdgeReliability);
            Edge e12 = new Edge("12", vf, vh, t2EdgeReliability);
            Edge e13 = new Edge("13", vl, vi, t2EdgeReliability);
            Edge e14 = new Edge("14", vl, vn, t1EdgeReliability);
            Edge e15 = new Edge("15", vg, vi, t1EdgeReliability);
            Edge e16 = new Edge("16", vg, vj, t1EdgeReliability);
            Edge e17 = new Edge("17", vh, vj, t2EdgeReliability);
            Edge e18 = new Edge("18", vh, vk, t2EdgeReliability);
            Edge e19 = new Edge("19", vk, vm, t2EdgeReliability);
            Edge e20 = new Edge("20", vi, vp, t1EdgeReliability);
            Edge e21 = new Edge("21", vj, vo, t2EdgeReliability);
            Edge e22 = new Edge("22", vk, vq, t1EdgeReliability);
            Edge e23 = new Edge("23", vo, vp, t1EdgeReliability);
            Edge e24 = new Edge("24", vo, vq, t2EdgeReliability);
            Edge e25 = new Edge("25", vm, vt, t2EdgeReliability);
            Edge e26 = new Edge("26", vn, vr, t2EdgeReliability);
            Edge e27 = new Edge("27", vp, vr, t2EdgeReliability);
            Edge e28 = new Edge("28", vq, vs, t1EdgeReliability);
            Edge e29 = new Edge("29", vt, vs, t1EdgeReliability);
            Edge e30 = new Edge("30", vr, vs, t1EdgeReliability);

            List<Edge> edges = new List<Edge>()
            {
                e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11, e12, e13, e14, e15, e16, e17, e18, e19,
                e20, e21, e22, e23, e24, e25, e26, e27, e28, e29, e30
            };

            return new Network(edges);
        }

        private static Network Get_network5()
        {
            // Network(5) from Fig. 9 in "OBDD-Based evaluation of k-terminal network reliability" Yeh et al, 2002.
            Vertex va = new Vertex("a", true);
            Vertex vb = new Vertex("b", false);
            Vertex vc = new Vertex("c", false);
            Vertex vd = new Vertex("d", false);
            Vertex ve = new Vertex("e", false);
            Vertex vf = new Vertex("f", false);
            Vertex vg = new Vertex("g", true);
            Vertex vh = new Vertex("h", true);
            Vertex vi = new Vertex("i", true);
            Vertex vj = new Vertex("j", true);
            Vertex vk = new Vertex("k", true);
            Vertex vl = new Vertex("l", false);
            Vertex vm = new Vertex("m", false);
            Vertex vn = new Vertex("n", false);
            Vertex vo = new Vertex("o", true);
            Vertex vp = new Vertex("p", true);
            Vertex vq = new Vertex("q", true);
            Vertex vr = new Vertex("r", false);
            Vertex vs = new Vertex("s", false);
            Vertex vt = new Vertex("t", false);

            Edge e1 = new Edge("1", va, vr, 0.9);
            Edge e2 = new Edge("2", va, vb, 0.9);
            Edge e3 = new Edge("3", va, vs, 0.9);
            Edge e4 = new Edge("4", vb, vc, 0.9);
            Edge e5 = new Edge("5", vb, vd, 0.9);
            Edge e6 = new Edge("6", vc, ve, 0.9);
            Edge e7 = new Edge("7", ve, vf, 0.9);
            Edge e8 = new Edge("8", vd, vf, 0.9);
            Edge e9 = new Edge("9", vd, vm, 0.9);
            Edge e10 = new Edge("10", vc, vl, 0.9);
            Edge e11 = new Edge("11", ve, vg, 0.9);
            Edge e12 = new Edge("12", vf, vh, 0.9);
            Edge e13 = new Edge("13", vl, vi, 0.9);
            Edge e14 = new Edge("14", vl, vn, 0.9);
            Edge e15 = new Edge("15", vg, vi, 0.9);
            Edge e16 = new Edge("16", vg, vj, 0.9);
            Edge e17 = new Edge("17", vh, vj, 0.9);
            Edge e18 = new Edge("18", vh, vk, 0.9);
            Edge e19 = new Edge("19", vk, vm, 0.9);
            Edge e20 = new Edge("20", vi, vp, 0.9);
            Edge e21 = new Edge("21", vj, vo, 0.9);
            Edge e22 = new Edge("22", vk, vq, 0.9);
            Edge e23 = new Edge("23", vo, vp, 0.9);
            Edge e24 = new Edge("24", vo, vq, 0.9);
            Edge e25 = new Edge("25", vm, vt, 0.9);
            Edge e26 = new Edge("26", vn, vr, 0.9);
            Edge e27 = new Edge("27", vp, vr, 0.9);
            Edge e28 = new Edge("28", vq, vs, 0.9);
            Edge e29 = new Edge("29", vt, vs, 0.9);
            Edge e30 = new Edge("30", vr, vs, 0.9);

            List<Edge> edges = new List<Edge>()
            {
                e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11, e12, e13, e14, e15, e16, e17, e18, e19,
                e20, e21, e22, e23, e24, e25, e26, e27, e28, e29, e30
            };

            return new Network(edges);
        }

        private static string GetNet_2_8GraphViz(out string signatureTable)
        {
            var net = Get_net_2_8_3T();

            var edgeToType = GetEdgeToTypeMapping(net);

            var vertexCoords = new Dictionary<Vertex, Point>();
            vertexCoords[net.Vertices.First(v => v.Label == "s")] = new Point(1, 5);
            vertexCoords[net.Vertices.First(v => v.Label == "t")] = new Point(4, 0);
            vertexCoords[net.Vertices.First(v => v.Label == "1")] = new Point(4, 5);
            vertexCoords[net.Vertices.First(v => v.Label == "2")] = new Point(5, 4);
            vertexCoords[net.Vertices.First(v => v.Label == "3")] = new Point(5, 1);
            vertexCoords[net.Vertices.First(v => v.Label == "4")] = new Point(1, 0);
            vertexCoords[net.Vertices.First(v => v.Label == "5")] = new Point(0, 1);
            vertexCoords[net.Vertices.First(v => v.Label == "6")] = new Point(0, 4);
            vertexCoords[net.Vertices.First(v => v.Label == "7")] = new Point(2, 4);
            vertexCoords[net.Vertices.First(v => v.Label == "8")] = new Point(3, 4);
            vertexCoords[net.Vertices.First(v => v.Label == "9")] = new Point(4, 3);
            vertexCoords[net.Vertices.First(v => v.Label == "10")] = new Point(4, 2);
            vertexCoords[net.Vertices.First(v => v.Label == "11")] = new Point(3, 1);
            vertexCoords[net.Vertices.First(v => v.Label == "12")] = new Point(2, 1);
            vertexCoords[net.Vertices.First(v => v.Label == "13")] = new Point(1, 2);
            vertexCoords[net.Vertices.First(v => v.Label == "14")] = new Point(1, 3);

            Func<Vertex, string> vertexOptionsFunc = (v) =>
            {
                List<string> options = new List<string>();

                if (v.IsTerminal)
                {
                    options.Add("style=filled");
                }

                options.Add("label=\"\"");
                var coords = vertexCoords[v];
                options.Add("pos=\"" + coords.X + "," + coords.Y + "!\"");

                return string.Join(",", options);
            };

            Func<Edge, string> edgeOptionsFunc = (e) => $"label=\"{edgeToType[e].ToString()}\"";

            var oe = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(net, 0).ToList();
            int[] dimToComponentType;
            var unnormalisedSignature = ComputeBDDAlgorithm.ComputeSignature(oe, edgeToType, out dimToComponentType);
            var signature = NDArray.Divide(unnormalisedSignature,
                SurvivalSignatureFuns.GetNormalisationArray(unnormalisedSignature.Shape));

            signatureTable = SurvivalSignatureFuns.PrintAsTable(signature, dimToComponentType);

            return net.GraphVizString(edgeOptionsFunc, vertexOptionsFunc);
        }

        private static string GetNetwork5GraphViz(out string signatureTable)
        {
            Network net5 = Get_network5_3T();

            var vertexCoords = new Dictionary<Vertex, Point>();
            vertexCoords[net5.Vertices.First(v => v.Label == "a")] = new Point(3,7);
            vertexCoords[net5.Vertices.First(v => v.Label == "b")] = new Point(3, 6);
            vertexCoords[net5.Vertices.First(v => v.Label == "c")] = new Point(2, 6);
            vertexCoords[net5.Vertices.First(v => v.Label == "d")] = new Point(4, 6);
            vertexCoords[net5.Vertices.First(v => v.Label == "e")] = new Point(2, 5);
            vertexCoords[net5.Vertices.First(v => v.Label == "f")] = new Point(4, 5);
            vertexCoords[net5.Vertices.First(v => v.Label == "g")] = new Point(2, 4);
            vertexCoords[net5.Vertices.First(v => v.Label == "h")] = new Point(4, 4);
            vertexCoords[net5.Vertices.First(v => v.Label == "i")] = new Point(2, 3);
            vertexCoords[net5.Vertices.First(v => v.Label == "j")] = new Point(3, 3);
            vertexCoords[net5.Vertices.First(v => v.Label == "k")] = new Point(4,3);
            vertexCoords[net5.Vertices.First(v => v.Label == "l")] = new Point(1,4);
            vertexCoords[net5.Vertices.First(v => v.Label == "m")] = new Point(5,4);
            vertexCoords[net5.Vertices.First(v => v.Label == "n")] = new Point(1,3);
            vertexCoords[net5.Vertices.First(v => v.Label == "o")] = new Point(3, 2);
            vertexCoords[net5.Vertices.First(v => v.Label == "p")] = new Point(2, 2);
            vertexCoords[net5.Vertices.First(v => v.Label == "q")] = new Point(4,2);
            vertexCoords[net5.Vertices.First(v => v.Label == "r")] = new Point(2, 1);
            vertexCoords[net5.Vertices.First(v => v.Label == "s")] = new Point(4, 1);
            vertexCoords[net5.Vertices.First(v => v.Label == "t")] = new Point(5, 3);

            Func<Vertex, string> vertexOptionsFunc = (v) =>
            {
                List<string> options = new List<string>();

                if (v.IsTerminal)
                {
                    options.Add("style=filled");
                }

                options.Add("label=\"\"");
                var coords = vertexCoords[v];
                options.Add("pos=\"" + coords.X + "," + coords.Y + "!\"");

                return string.Join(",", options);
            };

            var edgeToType = GetEdgeToTypeMapping(net5);

            Func<Edge, string> edgeOptionsFunc = (e) => $"label=\"{edgeToType[e].ToString()}\"";

            var oe = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(net5, 0).ToList();
            int[] dimToComponentType;
            var unnormalisedSignature = ComputeBDDAlgorithm.ComputeSignature(oe, edgeToType, out dimToComponentType);
            var signature = NDArray.Divide(unnormalisedSignature, SurvivalSignatureFuns.GetNormalisationArray(unnormalisedSignature.Shape));
            signatureTable = SurvivalSignatureFuns.PrintAsTable(signature, dimToComponentType);

            return net5.GraphVizString(edgeOptionsFunc, vertexOptionsFunc);
        }

        private static Network Get_net_2_8()
        {
            // Construct networks from Figure 9 of "OBDD-Based Evaluation of k-Terminal Network Reliability", Yeh et al, 2002.
            // net2_8
            Vertex s = new Vertex("s", true);
            Vertex v_v1 = new Vertex("1", false);
            Vertex v_v2 = new Vertex("2", false);
            Vertex v_v3 = new Vertex("3", false);
            Vertex t = new Vertex("t", true);
            Vertex v_v4 = new Vertex("4", false);
            Vertex v_v5 = new Vertex("5", false);
            Vertex v_v6 = new Vertex("6", false);
            Vertex v_v7 = new Vertex("7", false);
            Vertex v_v8 = new Vertex("8", false);
            Vertex v_v9 = new Vertex("9", false);
            Vertex v_v10 = new Vertex("10", false);
            Vertex v_v11 = new Vertex("11", false);
            Vertex v_v12 = new Vertex("12", false);
            Vertex v_v13 = new Vertex("13", false);
            Vertex v_v14 = new Vertex("14", false);
            List<Vertex> net2_8_vertices = new List<Vertex>() { s, v_v1, v_v2, v_v3, t, v_v4, v_v5, v_v6, v_v7, v_v8,
                v_v9, v_v10, v_v11, v_v12, v_v13, v_v14};


            double edgeReliability = 0.9;
            Edge e1 = new Edge("1", s, v_v1, edgeReliability);
            Edge e2 = new Edge("2", v_v1, v_v2, edgeReliability);
            Edge e3 = new Edge("3", v_v2, v_v3, edgeReliability);
            Edge e4 = new Edge("4", v_v3, t, edgeReliability);
            Edge e5 = new Edge("5", t, v_v4, edgeReliability);
            Edge e6 = new Edge("6", v_v4, v_v5, edgeReliability);
            Edge e7 = new Edge("7", v_v5, v_v6, edgeReliability);
            Edge e8 = new Edge("8", v_v6, s, edgeReliability);
            Edge e9 = new Edge("9", s, v_v7, edgeReliability);
            Edge e10 = new Edge("10", v_v1, v_v8, edgeReliability);
            Edge e11 = new Edge("11", v_v2, v_v9, edgeReliability);
            Edge e12 = new Edge("12", v_v3, v_v10, edgeReliability);
            Edge e13 = new Edge("13", t, v_v11, edgeReliability);
            Edge e14 = new Edge("14", v_v4, v_v12, edgeReliability);
            Edge e15 = new Edge("15", v_v5, v_v13, edgeReliability);
            Edge e16 = new Edge("16", v_v6, v_v14, edgeReliability);
            Edge e17 = new Edge("17", v_v7, v_v8, edgeReliability);
            Edge e18 = new Edge("18", v_v8, v_v9, edgeReliability);
            Edge e19 = new Edge("19", v_v9, v_v10, edgeReliability);
            Edge e20 = new Edge("20", v_v10, v_v11, edgeReliability);
            Edge e21 = new Edge("21", v_v11, v_v12, edgeReliability);
            Edge e22 = new Edge("22", v_v12, v_v13, edgeReliability);
            Edge e23 = new Edge("23", v_v13, v_v14, edgeReliability);
            Edge e24 = new Edge("24", v_v14, v_v7, edgeReliability);

            List<Edge> net2_8_edges = new List<Edge>()
            {
                e1,
                e2,
                e3,
                e4,
                e5,
                e6,
                e7,
                e8,
                e9,
                e10,
                e11,
                e12,
                e13,
                e14,
                e15,
                e16,
                e17,
                e18,
                e19,
                e20,
                e21,
                e22,
                e23,
                e24
            };

            Network net2_8 = new Network(net2_8_edges);

            return net2_8;
        }

        private static Dictionary<Edge, int> GetEdgeToTypeMapping(Network net)
        {
            Dictionary<double, int> reliabilityToType = new Dictionary<double, int>();
            Dictionary<Edge, int> edgeToType = new Dictionary<Edge, int>();
            foreach (var edge in net.Edges)
            {
                if (!reliabilityToType.ContainsKey(edge.Reliability))
                {
                    reliabilityToType[edge.Reliability] = reliabilityToType.Count + 1;
                }

                edgeToType[edge] = reliabilityToType[edge.Reliability];
            }

            return edgeToType;
        }

        private static void WriteBenchmarkForSignature(Network net, bool printSignature=false)
        {
            var edgeToType = GetEdgeToTypeMapping(net);

            int[] dimToComponentType;

            var orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(net, 0).ToList();

            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
                watch.Stop();


                Console.WriteLine("Signature computed in {0} seconds.", watch.Elapsed.TotalSeconds);
                Console.WriteLine();

                if (printSignature)
                {
                    var sigNorm = SurvivalSignatureFuns.GetNormalisationArray(signature.Shape);
                    var normSig = NDArray.Divide(signature, sigNorm);
                    Console.Write(SurvivalSignatureFuns.PrintAsTable(normSig, dimToComponentType));
                }
            }
            catch (OutOfMemoryException e)
            {
                Console.WriteLine("Not enough memory to compute signature.");
            }
        

        }

        private static void WriteNetworkStats(Network net)
        {
            Console.WriteLine("Vertices: {0}, Edges: {1},  Terminal Nodes: {2}, Number of Types: {3}", 
                net.Vertices.Count, net.Edges.Count, net.Vertices.Count((v) => v.IsTerminal),
                net.Edges.GroupBy((e) =>e.Reliability).Count());
        }
    }
}
