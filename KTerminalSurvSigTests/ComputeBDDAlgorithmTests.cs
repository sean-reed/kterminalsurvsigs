using KTerminalNetworkBDD;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

namespace KTerminalNetworkBDDTests
{
    [TestFixture]
    public class ComputeBDDAlgorithmTests
    {
        Network network1;
        Vertex v0, v1, v2, v3;
        double net1_expected_reliability = 0.97848; // Note: value in Figure 3 of "Comparison of Binary and Multi-Variate Hybrid Decision Diagram Algorithms for K-Terminal Reliability"  paper is incorrect.

        Network network2a, network2b, network2c;
        Vertex va, vb, vc, vd, ve;
        double network2a_expectedReliability = 0.0956;
        double network2b_expectedReliability = 0.3593;
        double network2c_expectedReliability = 0.9673;

        Network net2_8;
        double net2_8_expectedReliability = 0.99555; // Correct value from Table 1 in "K-Terminal Network Reliability Measures With Binary Decision Diagrams" by Hardy et al, 2007.

        private Network grid5by5_2terminal, grid5by5_allterminal, grid6by6net_2terminal, grid2by100net_2terminal, grid2by100net_allterminal;
        private Network grid5by5net_12terminal; // Network (8) from Fig. 9 of "OBDD-Based evaluation of k-terminal network reliability", Yeh et al, 2002.
        private Network grid3by10net_15terminal; // Network (6) from Fig. 9 of "OBDD-Based evaluation of k-terminal network reliability", Yeh et al, 2002.
        private double grid5by5_2terminal_expectedReliability = 0.97555;
        private double grid5by5_allterminal_expectedReliability = 0.93981;
        private double grid5by5net_12terminal_expectedReliability = 0.95743;
        private double grid6by6net_2terminal_expectedReliability = 0.975644995;
        private double grid2by100net_2terminal_expectedReliability = 0.30429;
        private double grid2by100net_allterminal_expectedReliability = 0.25107;
        private double grid3by10net_15terminal_expectedReliability = 0.95493;

        private Network t4Net; // A simple T-shaped network with 4 nodes.
        private List<Edge> t4Net_orderedEdges;
        private double t4Net_expectedReliability = 0.125; // Manually calculated result.

        private Network t4Net_modified; // A modified version of t4Net.
        private List<Edge> t4Net_modified_orderedEdges;
        private double t4Net_modified_expectedReliability = 0.125; // Manually calculated result.

        private Network n4Net; // A simple n-shaped network with 4 nodes.
        private List<Edge> n4Net_orderedEdges;
        private double n4Net_expectedReliability = 0.25; // Manually calculated result.

        private Network n4Net_modified; // A modified version of n4Net.
        private List<Edge> n4Net_modified_orderedEdges;
        private double n4Net_modified_expectedReliability = 0.25; // Manually calculated result.

        private Network p4Net;
        private List<Edge> p4Net_orderedEdges;
        private double p4Net_expectedReliability = 0.625; // Manually calculated result.

        private Network arpanetTriple1973;
        private List<Edge> arpanetTriple1973_orderedEdges;

        // Network(5) (variant of net_19 with |V|/2 terminal nodes) from Fig.9 of "OBDD-Based evaluation of K-terminal network reliability", Yeh et al, 2012.
        private List<Edge> network5_orderedEdges;
        private double network5_expectedReliability = 0.98909; // Value from Hardy 2007 (Yeh et al value appears to be incorrect).

        private Network fully_connected_8; // See Table II in Hardy et al 2007.
        private double fully_connected_8_expected_reliability = 0.999999199977; // Calculated value from version of this code.

        private Network fully_connected_12; // See Table II in Hardy et al 2007.
        private double fully_connected_12_expected_reliability = 0.999999999880007; // Calculated value from version of this code.

        private List<Edge> sigNet1_orderedEdges; // From Fig 1. of "Nonparametric predictive inference for system reliability using the survival signature." by Coolen, Coolen-Maturi 2014.
        private Dictionary<Edge, int> sigNet1_edgeComponentTypes;

        private List<Edge> sigNet2_orderedEdges; // From Fig 2. of "Nonparametric predictive inference for system reliability using the survival signature." by Coolen, Coolen-Maturi 2014.
        private Dictionary<Edge, int> sigNet2_edgeComponentTypes;


        [SetUp]
        public void SetupFixture()
        {
            /* Construct sample network from Figure 1 of "Comparison of Binary and Multi-Variate Hybrid Decision Diagram Algorithms for K-Terminal Reliability"
            by Herrmann and Soh, 2011. */
            v0 = new Vertex("0", true);
            v1 = new Vertex("1", false);
            v2 = new Vertex("2", false);
            v3 = new Vertex("3", true);
            List<Vertex> n1_vertices = new List<Vertex>() { v0, v1, v2, v3 };

            double n1_edgeReliability = 0.9;
            Edge n1_e0 = new Edge("0", v0, v1, n1_edgeReliability);
            Edge n1_e1 = new Edge("1", v0, v2, n1_edgeReliability);
            Edge n1_e2 = new Edge("2", v1, v2, n1_edgeReliability);
            Edge n1_e3 = new Edge("3", v1, v3, n1_edgeReliability);
            Edge n1_e4 = new Edge("4", v2, v3, n1_edgeReliability);
            List<Edge> n1_edges = new List<Edge>() { n1_e0, n1_e1, n1_e2, n1_e3, n1_e4 };
            network1 = new Network(n1_edges);

               /* Construct sample network from Figure 5 of "K-Terminal Network Reliability Measures With Binary Decision Diagrams" by Hardy et al, 2007. */
            va = new Vertex("a", true);
            vb = new Vertex("b", false);
            vc = new Vertex("c", true);
            vd = new Vertex("d", true);
            ve = new Vertex("e", false);
            List<Vertex> n2_vertices = new List<Vertex>() { va, vb, vc, vd, ve };

            double n2_edgeReliability = 0.3;
            Edge n2_e1 = new Edge("1", va, vb, n2_edgeReliability);
            Edge n2_e2 = new Edge("2", va, vc, n2_edgeReliability);
            Edge n2_e3 = new Edge("3", vc, vb, n2_edgeReliability);
            Edge n2_e4 = new Edge("4", vb, vd, n2_edgeReliability);
            Edge n2_e5 = new Edge("5", vc, ve, n2_edgeReliability);
            Edge n2_e6 = new Edge("6", vd, ve, n2_edgeReliability);
            List<Edge> n2_edges = new List<Edge>() { n2_e1, n2_e2, n2_e3, n2_e4, n2_e5, n2_e6 };
            network2a = new Network(n2_edges);

            double n2b_edgeReliability = 0.5;
            Edge n2b_e1 = new Edge("1", va, vb, n2b_edgeReliability);
            Edge n2b_e2 = new Edge("2", va, vc, n2b_edgeReliability);
            Edge n2b_e3 = new Edge("3", vc, vb, n2b_edgeReliability);
            Edge n2b_e4 = new Edge("4", vb, vd, n2b_edgeReliability);
            Edge n2b_e5 = new Edge("5", vc, ve, n2b_edgeReliability);
            Edge n2b_e6 = new Edge("6", vd, ve, n2b_edgeReliability);
            List<Edge> n2b_edges = new List<Edge>() { n2b_e1, n2b_e2, n2b_e3, n2b_e4, n2b_e5, n2b_e6 };
            network2b = new Network(n2b_edges);

            double n2c_edgeReliability = 0.9;
            Edge n2c_e1 = new Edge("1", va, vb, n2c_edgeReliability);
            Edge n2c_e2 = new Edge("2", va, vc, n2c_edgeReliability);
            Edge n2c_e3 = new Edge("3", vc, vb, n2c_edgeReliability);
            Edge n2c_e4 = new Edge("4", vb, vd, n2c_edgeReliability);
            Edge n2c_e5 = new Edge("5", vc, ve, n2c_edgeReliability);
            Edge n2c_e6 = new Edge("6", vd, ve, n2c_edgeReliability);
            List<Edge> n2c_edges = new List<Edge>() { n2c_e1, n2c_e2, n2c_e3, n2c_e4, n2c_e5, n2c_e6 };
            network2c = new Network(n2c_edges);

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

            net2_8 = new Network(net2_8_edges);

            // Create grid networks.
            Func<int, double> edgeReliabilityFunc = (e) => 0.9;
            grid5by5_2terminal = GridNetworkGenerator.GenerateTwoTerminal(5, 5, edgeReliabilityFunc);
            grid5by5_allterminal = GridNetworkGenerator.GenerateAllTerminal(5, 5, edgeReliabilityFunc);
            Func<int, int, bool> isTerminalPredicate = (rowIndex, columnIndex) =>
            {
                if (rowIndex == 0 || rowIndex == 1 || (rowIndex == 2 && columnIndex == 0) || (rowIndex == 4 && columnIndex == 4))
                    return true;

                return false;
            };
            grid5by5net_12terminal = GridNetworkGenerator.Generate(5, 5, edgeReliabilityFunc, isTerminalPredicate);
            grid6by6net_2terminal = GridNetworkGenerator.GenerateTwoTerminal(6, 6, edgeReliabilityFunc);
            grid2by100net_2terminal = GridNetworkGenerator.GenerateTwoTerminal(2, 100, edgeReliabilityFunc);
            grid2by100net_allterminal = GridNetworkGenerator.GenerateAllTerminal(2, 100, edgeReliabilityFunc);
            isTerminalPredicate = (rowIndex, columnIndex) =>
            {
                if ((rowIndex == 0 && columnIndex <= 7) || (rowIndex == 1 && columnIndex <= 5) ||
                    (rowIndex == 2 && columnIndex == 9))
                    return true;

                return false;
            };
            grid3by10net_15terminal = GridNetworkGenerator.Generate(3, 10, edgeReliabilityFunc, isTerminalPredicate);

            t4Net = Get_t4Net(out t4Net_orderedEdges);

            t4Net_modified = Get_t4Net_modified(out t4Net_modified_orderedEdges);

            n4Net = Get_n4Net(out n4Net_orderedEdges);

            n4Net_modified = Get_n4Net_modified(out n4Net_modified_orderedEdges);

            p4Net = Get_p4Net(out p4Net_orderedEdges);

            arpanetTriple1973 = Get_ThreeReplicaArpanet1973(out arpanetTriple1973_orderedEdges);

            network5_orderedEdges = GetNetwork5OrderedEdges();

            fully_connected_8 = FullyConnectedNetworkGenerator.GenerateFullyConnected(8, (v) => true, (e) => 0.9);

            fully_connected_12 = FullyConnectedNetworkGenerator.GenerateFullyConnected(12, (v) => true, (e) => 0.9);

            GetSigNet1(out sigNet1_orderedEdges, out sigNet1_edgeComponentTypes);

            GetSigNet2(out sigNet2_orderedEdges, out sigNet2_edgeComponentTypes);
        }

        private Network GetHerrmannNetFig1(out List<Edge> orderedEdges)
        {
            // Construct network from Figure 1 of "A memory efficient algorithm for network reliability" by Herrmann et al, 2009.
            Vertex v0 = new Vertex("0", true);
            Vertex v1 = new Vertex("1", false);
            Vertex v2 = new Vertex("2", false);
            Vertex v3 = new Vertex("3", true);

            double edgeReliability = 0.9;

            Edge e0 = new Edge("e0", v0, v1, edgeReliability);
            Edge e1 = new Edge("e1", v0, v2, edgeReliability);
            Edge e2 = new Edge("e2", v1, v2, edgeReliability);
            Edge e3 = new Edge("e3", v1, v3, edgeReliability);
            Edge e4 = new Edge("e4", v2, v3, edgeReliability);
            orderedEdges = new List<Edge>() { e0, e1, e2, e3, e4 };

            return new Network(orderedEdges);
        }

        private List<Edge> GetNetwork5OrderedEdges()
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

            Network network5 = new Network(edges);

            return BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(network5, 0).ToList();
        }

        private void GetSigNet1(out List<Edge> orderedEdges, out Dictionary<Edge, int> edgeComponentTypes)
        {
            Vertex va = new Vertex("a", true);
            Vertex vb = new Vertex("b", false);
            Vertex vc = new Vertex("c", false);
            Vertex vd = new Vertex("d", false);
            Vertex ve = new Vertex("e", true);

            Edge e1 = new Edge("1", va, vb, 0.9);
            Edge e2 = new Edge("2", vb, vc, 0.9);
            Edge e3 = new Edge("3", vb, vd, 0.8);
            Edge e4 = new Edge("4", vc, vd, 0.8);
            Edge e5 = new Edge("5", vc, ve, 0.9);
            Edge e6 = new Edge("6", vd, ve, 0.8);

            orderedEdges = new List<Edge>() {e1, e2, e3, e4, e5, e6};

            edgeComponentTypes = new Dictionary<Edge, int>(){{e1,1}, {e2,1}, {e3, 2}, {e4, 2}, {e5, 1}, {e6, 2}};

        }

        private void GetSigNet2(out List<Edge> orderedEdges, out Dictionary<Edge, int> edgeComponentTypes)
        {
            Vertex vs = new Vertex("s", true);
            Vertex va = new Vertex("a", false);
            Vertex vb = new Vertex("b", false);
            Vertex vc = new Vertex("c", false);
            Vertex vt = new Vertex("t", true);

            double type1Reliability = 0.9;
            double type2Reliability = 0.45;
            double type3Reliability = 0.78;

            Edge e1 = new Edge("1", vs, va, type1Reliability);
            Edge e2 = new Edge("2", vs, va, type2Reliability);
            Edge e3 = new Edge("3", va, vb, type2Reliability);
            Edge e4 = new Edge("4", va, vb, type3Reliability);
            Edge e5 = new Edge("5", vb, vt, type2Reliability);
            Edge e6 = new Edge("6", vb, vt, type3Reliability);
            Edge e7 = new Edge("7", vs, vt, type2Reliability);
            Edge e8 = new Edge("8", vs, vc, type3Reliability);
            Edge e9 = new Edge("9", vc, vt, type3Reliability);

            orderedEdges = new List<Edge>() { e1, e2, e7, e8, e3, e4, e9, e5, e6 };

            edgeComponentTypes = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
        }

        private Network Get_t4Net(out List<Edge> orderedEdges)
        {
            // A simple t-shaped network.
            var vA = new Vertex("A", true);
            var vB = new Vertex("B", false);
            var vC = new Vertex("C", true);
            var vD = new Vertex("D", true);
            var e1 = new Edge("1", vA, vB, 0.5);
            var e2 = new Edge("2", vB, vC, 0.5);
            var e3 = new Edge("3", vB, vD, 0.5);
            orderedEdges = new List<Edge>(){e1, e2, e3};

            var t4Net = new Network(orderedEdges);

            return t4Net;
        }

        private Network Get_t4Net_modified(out List<Edge> orderedEdges)
        {
            // A simple t-shaped network. Same as t4Net but with edge 2 reversed.
            var vA = new Vertex("A", true);
            var vB = new Vertex("B", false);
            var vC = new Vertex("C", true);
            var vD = new Vertex("D", true);
            var e1 = new Edge("1", vA, vB, 0.5);
            var e2 = new Edge("2", vC, vB, 0.5);
            var e3 = new Edge("3", vB, vD, 0.5);
            orderedEdges = new List<Edge>() { e1, e2, e3 };

            var t4Net = new Network(orderedEdges);

            return t4Net;
        }

        private Network Get_n4Net(out List<Edge> orderedEdges)
        {
            // A simple n-shaped network.
            var vA = new Vertex("A", true);
            var vB = new Vertex("B", false);
            var vC = new Vertex("C", true);
            var vD = new Vertex("D", false);
            var e1 = new Edge("1", vA, vB, 0.5);
            var e2 = new Edge("2", vB, vC, 0.5);
            var e3 = new Edge("3", vA, vD, 0.5);
            orderedEdges = new List<Edge>() { e1, e2, e3 };
            
            var n4Net = new Network(orderedEdges);

            return n4Net;
        }

        private Network Get_n4Net_modified(out List<Edge> orderedEdges)
        {
            // A modified version of n4Net with edge 2 reversed.
            var vA = new Vertex("A", true);
            var vB = new Vertex("B", false);
            var vC = new Vertex("C", true);
            var vD = new Vertex("D", false);
            var e1 = new Edge("1", vA, vB, 0.5);
            var e2 = new Edge("2", vC, vB, 0.5);
            var e3 = new Edge("3", vA, vD, 0.5);
            orderedEdges = new List<Edge>() { e1, e2, e3 };

            var net = new Network(orderedEdges);

            return net;
        }

        private Network Get_p4Net(out List<Edge> orderedEdges)
        {
            var vA = new Vertex("A", false);
            var vB = new Vertex("B", true);
            var vC = new Vertex("C", true);
            var vD = new Vertex("D", false);
            var e1 = new Edge("1", vA, vB, 0.5);
            var e2 = new Edge("2", vA, vC, 0.5);
            var e3 = new Edge("3", vA, vD, 0.5);
            var e4 = new Edge("4", vB,vC, 0.5);
            orderedEdges = new List<Edge>() { e1, e2, e4 , e3};

            var net = new Network(orderedEdges);

            return net;
        }

        private Network Get_ThreeReplicaArpanet1973(out List<Edge> orderedEdges)
        {
            // This network is from Fig. 13 of "Fast computation of bounds for two-terminal network reliability" by Sebastio et al 2014, EJOR Vol. 238.

            double edgeReliability = 0.99;

            var source = new Vertex("source", true);
            var target = new Vertex("target", true);

            var v1 = new Vertex("1", false);
            var v2 = new Vertex("2", false);
            var v3 = new Vertex("3", false);
            var v4 = new Vertex("4", false);
            var v5 = new Vertex("5", false);
            var v6 = new Vertex("6", false);
            var v7 = new Vertex("7", false);
            var v8 = new Vertex("8", false);
            var v9 = new Vertex("9", false);
            var v10 = new Vertex("10", false);
            var v11 = new Vertex("11", false);
            var v12 = new Vertex("12", false);
            var v13 = new Vertex("13", false);
            var v14 = new Vertex("14", false);
            var v15 = new Vertex("15", false);
            var v16 = new Vertex("16", false);
            var v17 = new Vertex("17", false);
            var v18 = new Vertex("18", false);
            var v19 = new Vertex("19", false);

            var e1 = new Edge("1", source, v1, edgeReliability);
            var e2 = new Edge("2", source, v2, edgeReliability);
            var e3 = new Edge("3", v1, v2, edgeReliability);
            var e4 = new Edge("4", v2, v4, edgeReliability);
            var e5 = new Edge("5", v1, v3, edgeReliability);
            var e6 = new Edge("6", v3, v4, edgeReliability);
            var e7 = new Edge("7", v1, v6, edgeReliability);
            var e8 = new Edge("8", v4, v5, edgeReliability);
            var e9 = new Edge("9", v4, v9, edgeReliability);
            var e10 = new Edge("10", v5, v6, edgeReliability);
            var e11 = new Edge("11", v9, v10, edgeReliability);
            var e12 = new Edge("12", v6, v7, edgeReliability);
            var e13 = new Edge("13", v6, v12, edgeReliability);
            var e14 = new Edge("14", v7, v8, edgeReliability);
            var e15 = new Edge("15", v8, v10, edgeReliability);
            var e16 = new Edge("16", v12, v13, edgeReliability);
            var e17 = new Edge("17", v8, v11, edgeReliability);
            var e18 = new Edge("18", v10, v16, edgeReliability);
            var e19 = new Edge("19", v13, v14, edgeReliability);
            var e20 = new Edge("20", v11, v15, edgeReliability);
            var e21 = new Edge("21", v16, v17, edgeReliability);
            var e22 = new Edge("22", v14, v15, edgeReliability);
            var e23 = new Edge("23", v14, target, edgeReliability);
            var e24 = new Edge("24", v17, v18, edgeReliability);
            var e25 = new Edge("25", v18, v19, edgeReliability);
            var e26 = new Edge("26", v19, target, edgeReliability);

            // Note: vertex 20 missing in figure.
            var v21 = new Vertex("21", false);
            var v22 = new Vertex("22", false);
            var v23 = new Vertex("23", false);
            var v24 = new Vertex("24", false);
            var v25 = new Vertex("25", false);
            var v26 = new Vertex("26", false);
            var v27 = new Vertex("27", false);
            var v28 = new Vertex("28", false);
            var v29 = new Vertex("29", false);
            var v30 = new Vertex("30", false);
            var v31 = new Vertex("31", false);
            var v32 = new Vertex("32", false);
            var v33 = new Vertex("33", false);
            var v34 = new Vertex("34", false);
            var v35 = new Vertex("35", false);
            var v36 = new Vertex("36", false);
            var v37 = new Vertex("37", false);
            var v38 = new Vertex("38", false);
            var v39 = new Vertex("39", false);

            var e27 = new Edge("27", source, v21, edgeReliability);
            var e28 = new Edge("28", source, v22, edgeReliability);
            var e29 = new Edge("29", v21, v22, edgeReliability);
            var e30 = new Edge("30", v22, v24, edgeReliability);
            var e31 = new Edge("31", v21, v23, edgeReliability);
            var e32 = new Edge("32", v23, v24, edgeReliability);
            var e33 = new Edge("33", v21, v26, edgeReliability);
            var e34 = new Edge("34", v24, v25, edgeReliability);
            var e35 = new Edge("35", v24, v29, edgeReliability);
            var e36 = new Edge("36", v25, v26, edgeReliability);
            var e37 = new Edge("37", v29, v30, edgeReliability);
            var e38 = new Edge("38", v26, v27, edgeReliability);
            var e39 = new Edge("39", v26, v32, edgeReliability);
            var e40 = new Edge("40", v27, v28, edgeReliability);
            var e41 = new Edge("41", v28, v30, edgeReliability);
            var e42 = new Edge("42", v32, v33, edgeReliability);
            var e43 = new Edge("43", v28, v31, edgeReliability);
            var e44 = new Edge("44", v30, v36, edgeReliability);
            var e45 = new Edge("45", v33, v34, edgeReliability);
            var e46 = new Edge("46", v31, v35, edgeReliability);
            var e47 = new Edge("47", v36, v37, edgeReliability);
            var e48 = new Edge("48", v34, v35, edgeReliability);
            var e49 = new Edge("49", v34, target, edgeReliability);
            var e50 = new Edge("50", v37, v38, edgeReliability);
            var e51 = new Edge("51", v38, v39, edgeReliability);
            var e52 = new Edge("52", v39, target, edgeReliability);

            // Note: re-numbered vertices 40 to 58 from figure to 41 to 59 (i.e. incremented number of each vertex by 1).
            var v41 = new Vertex("41", false);
            var v42 = new Vertex("42", false);
            var v43 = new Vertex("43", false);
            var v44 = new Vertex("44", false);
            var v45 = new Vertex("45", false);
            var v46 = new Vertex("46", false);
            var v47 = new Vertex("47", false);
            var v48 = new Vertex("48", false);
            var v49 = new Vertex("49", false);
            var v50 = new Vertex("50", false);
            var v51 = new Vertex("51", false);
            var v52 = new Vertex("52", false);
            var v53 = new Vertex("53", false);
            var v54 = new Vertex("54", false);
            var v55 = new Vertex("55", false);
            var v56 = new Vertex("56", false);
            var v57 = new Vertex("57", false);
            var v58 = new Vertex("58", false);
            var v59 = new Vertex("59", false);

            var e53 = new Edge("53", source, v41, edgeReliability);
            var e54 = new Edge("54", source, v42, edgeReliability);
            var e55 = new Edge("55", v41, v42, edgeReliability);
            var e56 = new Edge("56", v42, v44, edgeReliability);
            var e57 = new Edge("57", v41, v43, edgeReliability);
            var e58 = new Edge("58", v43, v44, edgeReliability);
            var e59 = new Edge("59", v41, v46, edgeReliability);
            var e60 = new Edge("60", v44, v45, edgeReliability);
            var e61 = new Edge("61", v44, v49, edgeReliability);
            var e62 = new Edge("62", v45, v46, edgeReliability);
            var e63 = new Edge("63", v49, v50, edgeReliability);
            var e64 = new Edge("64", v46, v47, edgeReliability);
            var e65 = new Edge("65", v46, v42, edgeReliability);
            var e66 = new Edge("66", v47, v48, edgeReliability);
            var e67 = new Edge("67", v48, v50, edgeReliability);
            var e68 = new Edge("68", v52, v53, edgeReliability);
            var e69 = new Edge("69", v48, v51, edgeReliability);
            var e70 = new Edge("70", v50, v56, edgeReliability);
            var e71 = new Edge("71", v53, v54, edgeReliability);
            var e72 = new Edge("72", v51, v55, edgeReliability);
            var e73 = new Edge("73", v56, v57, edgeReliability);
            var e74 = new Edge("74", v54, v55, edgeReliability);
            var e75 = new Edge("75", v54, target, edgeReliability);
            var e76 = new Edge("76", v57, v58, edgeReliability);
            var e77 = new Edge("77", v58, v59, edgeReliability);
            var e78 = new Edge("78", v59, target, edgeReliability);

            List<Edge> edges = new List<Edge>()
            {
                e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11, e12, e13, e14, e15, e16, e17, e18, e19,
                e20, e21, e22, e23, e24, e25, e26, e27, e28, e29, e30, e31, e32, e33, e34, e35, e36, 
                e37, e38, e39, e40, e41, e42, e43, e44, e45, e46, e47, e48, e49, e50, e51, e52, e53, e54, 
                e55, e56, e57, e58, e59, e60, e61, e62, e63, e64, e65, e67, e68, e69, e70, e71, e72, e73, e74,
                e75, e76, e77, e78
            };

            Network arpanetTriple1974 = new Network(edges);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(arpanetTriple1974, 0).ToList();

            return arpanetTriple1974;
        }

        [Test]
        public void BoundarySetsAreCorrect()
        {
            // For network 1.
            List<List<Vertex>> boundarySets = ComputeBDDAlgorithm.GetBoundarySetsAlgorithm(network1.Edges);

            List<Vertex> n1_expectedBS0 = new List<Vertex>() {};
            List<Vertex> n1_expectedBS1 = new List<Vertex>() { v0, v1 };
            List<Vertex> n1_expectedBS2 = new List<Vertex>() { v1, v2 };
            List<Vertex> n1_expectedBS3 = new List<Vertex>() { v1, v2 };
            List<Vertex> n1_expectedBS4 = new List<Vertex>() { v2, v3 };
            List<Vertex> n1_expectedBS5 = new List<Vertex>() {};

            Assert.AreEqual(6, boundarySets.Count);

            Assert.IsTrue(n1_expectedBS0.SequenceEqual(boundarySets[0]));
            Assert.IsTrue(n1_expectedBS1.SequenceEqual(boundarySets[1]));
            Assert.IsTrue(n1_expectedBS2.SequenceEqual(boundarySets[2]));
            Assert.IsTrue(n1_expectedBS3.SequenceEqual(boundarySets[3]));
            Assert.IsTrue(n1_expectedBS4.SequenceEqual(boundarySets[4]));

            // For network 2.
            boundarySets = ComputeBDDAlgorithm.GetBoundarySetsAlgorithm(network2a.Edges);

            List<Vertex> n2_expectedBS0 = new List<Vertex>() { };
            List<Vertex> n2_expectedBS1 = new List<Vertex>() { va, vb };
            List<Vertex> n2_expectedBS2 = new List<Vertex>() { vb, vc };
            List<Vertex> n2_expectedBS3 = new List<Vertex>() { vb, vc };
            List<Vertex> n2_expectedBS4 = new List<Vertex>() { vc, vd };
            List<Vertex> n2_expectedBS5 = new List<Vertex>() { vd, ve };
            List<Vertex> n2_expectedBS6 = new List<Vertex>() { };

            Assert.AreEqual(7, boundarySets.Count);

            Assert.IsTrue(n2_expectedBS0.SequenceEqual(boundarySets[0]));
            Assert.IsTrue(n2_expectedBS1.SequenceEqual(boundarySets[1]));
            Assert.IsTrue(n2_expectedBS2.SequenceEqual(boundarySets[2]));
            Assert.IsTrue(n2_expectedBS3.SequenceEqual(boundarySets[3]));
            Assert.IsTrue(n2_expectedBS4.SequenceEqual(boundarySets[4]));
            Assert.IsTrue(n2_expectedBS5.SequenceEqual(boundarySets[5]));
            Assert.IsTrue(n2_expectedBS6.SequenceEqual(boundarySets[6]));

            // For network p4.
            boundarySets = ComputeBDDAlgorithm.GetBoundarySetsAlgorithm(p4Net_orderedEdges);
 
            List<Vertex> p4_expectedBS0 = new List<Vertex>(){};
            List<Vertex> p4_expectedBS1 = new List<Vertex>(){p4Net.Vertices.Where(v => v.Label == "A").First(), p4Net.Vertices.Where(v => v.Label == "B").First() };
            List<Vertex> p4_expectedBS2 = new List<Vertex>(){ p4Net.Vertices.Where(v => v.Label == "A").First() , p4Net.Vertices.Where(v => v.Label == "B").First() , p4Net.Vertices.Where(v => v.Label == "C").First() };
            List<Vertex> p4_expectedBS3 = new List<Vertex>(){ p4Net.Vertices.Where(v => v.Label == "A").First() };
            List<Vertex> p4_expectedBS4 = new List<Vertex>(){  };

            Assert.AreEqual(5, boundarySets.Count);
            Assert.IsTrue(p4_expectedBS0.SequenceEqual(boundarySets[0]));
            Assert.IsTrue(p4_expectedBS1.SequenceEqual(boundarySets[1]));
            Assert.IsTrue(p4_expectedBS2.SequenceEqual(boundarySets[2]));
            Assert.IsTrue(p4_expectedBS3.SequenceEqual(boundarySets[3]));
            Assert.IsTrue(p4_expectedBS4.SequenceEqual(boundarySets[4]));

            boundarySets = ComputeBDDAlgorithm.GetBoundarySetsAlgorithm(arpanetTriple1973_orderedEdges);

            double maxNodesAtAnyLevel = 0;
            double maxNodesAtPreviousLevel = 1;
            int levelWithMax = -1;
            for (int i = 0; i < arpanetTriple1973_orderedEdges.Count; i++)
            {
                double maxPartitions = Math.Pow(2, Math.Pow(boundarySets[i].Count, 2)) * Math.Pow(3, boundarySets[i].Count);
                double maxNodesAtLevel = Math.Min(maxNodesAtPreviousLevel * 2, maxPartitions);
                if(maxNodesAtLevel > maxNodesAtAnyLevel)
                {
                    maxNodesAtAnyLevel = maxNodesAtLevel;
                    levelWithMax = i;
                }
                maxNodesAtPreviousLevel = maxNodesAtLevel;
            }

            double proportion = maxNodesAtAnyLevel / (Math.Pow(2, arpanetTriple1973_orderedEdges.Count));

            double rel = ComputeBDDAlgorithm.ComputeReliability(arpanetTriple1973_orderedEdges);
        }

        [Test]
        public void NetworkReliabilitiesAreCorrect()
        {
            var orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(network1, 0).ToList();
            double net1Reliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(net1_expected_reliability, net1Reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(network2a, 0).ToList();
            double network2aReliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(network2a_expectedReliability, network2aReliability, 0.0001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(network2b, 0).ToList();
            double network2bReliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(network2b_expectedReliability, network2bReliability, 0.0001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(network2c, 0).ToList();
            double network2cReliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(network2c_expectedReliability, network2cReliability, 0.0001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(net2_8, 9).ToList();
            double net2_8_reliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(net2_8_expectedReliability, net2_8_reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid5by5_2terminal, 0).ToList();
            double grid5by5net_2terminal_reliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(grid5by5_2terminal_expectedReliability, grid5by5net_2terminal_reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid5by5net_12terminal, 0).ToList();
            double grid5by5net_12terminal_reliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(grid5by5net_12terminal_expectedReliability, grid5by5net_12terminal_reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid5by5_allterminal, 0).ToList();
            double grid5by5net_allterminal_reliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(grid5by5_allterminal_expectedReliability, grid5by5net_allterminal_reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid6by6net_2terminal, 0).ToList();
            double grid6by6net_2terminal_reliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(grid6by6net_2terminal_expectedReliability, grid6by6net_2terminal_reliability, 0.000000001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid2by100net_2terminal, 0).ToList();
            double grid2by100net_2terminal_reliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(grid2by100net_2terminal_expectedReliability, grid2by100net_2terminal_reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid2by100net_allterminal, 0).ToList();
            double grid2by100net_allterminal_reliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(grid2by100net_allterminal_expectedReliability, grid2by100net_allterminal_reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid3by10net_15terminal, 0).ToList();
            double grid3by10net_15terminal_reliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(grid3by10net_15terminal_expectedReliability, grid3by10net_15terminal_reliability, 0.00001);

            double t4Net_reliability = ComputeBDDAlgorithm.ComputeReliability(t4Net_orderedEdges);
            Assert.AreEqual(t4Net_expectedReliability, t4Net_reliability);

            double t4Net_modified_reliability = ComputeBDDAlgorithm.ComputeReliability(t4Net_modified_orderedEdges);
            Assert.AreEqual(t4Net_modified_expectedReliability, t4Net_modified_reliability);

            double n4Net_reliability = ComputeBDDAlgorithm.ComputeReliability(n4Net_orderedEdges);
            Assert.AreEqual(n4Net_expectedReliability, n4Net_reliability);

            double n4Net_modified_reliability = ComputeBDDAlgorithm.ComputeReliability(n4Net_modified_orderedEdges);
            Assert.AreEqual(n4Net_modified_expectedReliability, n4Net_reliability);

            double p4Net_reliability = ComputeBDDAlgorithm.ComputeReliability(p4Net_orderedEdges);
            Assert.AreEqual(p4Net_expectedReliability, p4Net_reliability);

            double network5_reliability = ComputeBDDAlgorithm.ComputeReliability(network5_orderedEdges);
            Assert.AreEqual(network5_expectedReliability, network5_reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(fully_connected_8, 0).ToList();
            double fully_connected_8_reliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(fully_connected_8_expected_reliability, fully_connected_8_reliability, 0.000000000001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(fully_connected_12, 0).ToList();
            double fully_connected_12_reliability = ComputeBDDAlgorithm.ComputeReliability(orderedEdges);
            Assert.AreEqual(fully_connected_12_expected_reliability, fully_connected_12_reliability, 0.000000000000001);

            //double disconnected5Net_reliability = ComputeBDDAlgorithm.ComputeReliability(disconnected5Net_orderedEdges);
            //Assert.AreEqual(disconnected5Net_expectedReliability, disconnected5Net_reliability);

            //double disconnected5Net_modified_reliability =
            //    ComputeBDDAlgorithm.ComputeReliability(disconnected5Net_modified_orderedEdges);
            //Assert.AreEqual(disconnected5Net_modified_expectedReliability, disconnected5Net_modified_reliability);

            //double disconnected4Net_reliability = ComputeBDDAlgorithm.ComputeReliability(disconnected4Net_orderedEdges);
            //Assert.AreEqual(disconnected4Net_expectedReliability, disconnected4Net_reliability);
        }

        [Test]
        public void SignaturesAreCorrect()
        {
            // For sigNet1, set expected signature based on result in Table 1 of "Nonparametric predictive inference for system reliability using the survival signature",
            // Coolen et al, 2014.
            var sigNet1_expected_signature = new NDArray(new int[] { 4, 4 });
            sigNet1_expected_signature.SetValue(new int[] { 0, 0 }, 0);
            sigNet1_expected_signature.SetValue(new int[] { 0, 1 }, 0);
            sigNet1_expected_signature.SetValue(new int[] { 0, 2 }, 0);
            sigNet1_expected_signature.SetValue(new int[] { 0, 3 }, 0);
            sigNet1_expected_signature.SetValue(new int[] { 1, 0 }, 0);
            sigNet1_expected_signature.SetValue(new int[] { 1, 1 }, 0);
            sigNet1_expected_signature.SetValue(new int[] { 1, 2 }, 1);
            sigNet1_expected_signature.SetValue(new int[] { 1, 3 }, 1);
            sigNet1_expected_signature.SetValue(new int[] { 2, 0 }, 0);
            sigNet1_expected_signature.SetValue(new int[] { 2, 1 }, 0);
            sigNet1_expected_signature.SetValue(new int[] { 2, 2 }, 4);
            sigNet1_expected_signature.SetValue(new int[] { 2, 3 }, 2);
            sigNet1_expected_signature.SetValue(new int[] { 3, 0 }, 1);
            sigNet1_expected_signature.SetValue(new int[] { 3, 1 }, 3);
            sigNet1_expected_signature.SetValue(new int[] { 3, 2 }, 3);
            sigNet1_expected_signature.SetValue(new int[] { 3, 3 }, 1);

            int[] dimToComponentType;
            double[] dimToProbability = new double[] { 0.9, 0.8 };
            NDArray result = ComputeBDDAlgorithm.ComputeSignature(sigNet1_orderedEdges, sigNet1_edgeComponentTypes, out dimToComponentType);
            Assert.IsTrue(NDArray.ArrayEqual(sigNet1_expected_signature, result));

            result = ComputeBDDAlgorithm.ComputeSignature(sigNet2_orderedEdges, sigNet2_edgeComponentTypes, out dimToComponentType);
            // For sigNet2, check result against expected signature (for given entries) from Table 4 of "Nonparametric predictive inference for system reliability using the survival signature",
            // Coolen et al, 2014.
            Assert.AreEqual(0, result.GetValue(new int[] { 0, 0, 1 }));
            Assert.AreEqual(1, result.GetValue(new int[] { 0, 0, 2}));
            Assert.AreEqual(2, result.GetValue(new int[] { 0,0,3}));
            Assert.AreEqual(1, result.GetValue(new int[] { 0,1,0}));
            Assert.AreEqual(4, result.GetValue(new int[] { 0,1,1}));
            Assert.AreEqual(10, result.GetValue(new int[] { 0,1,2}));
            Assert.AreEqual(12, result.GetValue(new int[] { 0,1,3}));
            Assert.AreEqual(3, result.GetValue(new int[] { 0,2,0}));
            Assert.AreEqual(14, result.GetValue(new int[] { 0,2,1}));
            Assert.AreEqual(27, result.GetValue(new int[] { 0,2,2}));
            Assert.AreEqual(22, result.GetValue(new int[] { 0,2,3}));
            Assert.AreEqual(0, result.GetValue(new int[] { 1,0,0}));
            Assert.AreEqual(0, result.GetValue(new int[] { 1,0,1}));
            Assert.AreEqual(2, result.GetValue(new int[] { 1,0,2}));
            Assert.AreEqual(1, result.GetValue(new int[] { 1,1,0}));
            Assert.AreEqual(6, result.GetValue(new int[] { 1,1,1}));
            Assert.AreEqual(16, result.GetValue(new int[] { 1,1,2}));
            Assert.AreEqual(4, result.GetValue(new int[] { 1,2,0}));
            Assert.AreEqual(18, result.GetValue(new int[] { 1,2,1}));
            Assert.AreEqual(32, result.GetValue(new int[] { 1,2,2}));

            // Check reliability calculated from signatures against known values.

            List<Edge> orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(network1, 0).ToList();
            var edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            var signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            var reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(net1_expected_reliability, reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(network2a, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(network2a_expectedReliability, reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(network2b, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(network2b_expectedReliability, reliability, 0.0001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(network2c, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(network2c_expectedReliability, reliability, 0.0001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(net2_8, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(net2_8_expectedReliability, reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid5by5_2terminal, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(grid5by5_2terminal_expectedReliability, reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid5by5net_12terminal, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(grid5by5net_12terminal_expectedReliability, reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(network2a, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(network2a_expectedReliability, reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid5by5_allterminal, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(grid5by5_allterminal_expectedReliability, reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid6by6net_2terminal, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(grid6by6net_2terminal_expectedReliability, reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid2by100net_2terminal, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(grid2by100net_2terminal_expectedReliability, reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid2by100net_allterminal, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(grid2by100net_allterminal_expectedReliability, reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(grid3by10net_15terminal, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(grid3by10net_15terminal_expectedReliability, reliability, 0.00001);

            orderedEdges = t4Net_orderedEdges;
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(t4Net_expectedReliability, reliability, 0.00001);

            orderedEdges = t4Net_modified_orderedEdges;
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(t4Net_modified_expectedReliability, reliability, 0.00001);

            orderedEdges = n4Net_orderedEdges;
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(n4Net_expectedReliability, reliability, 0.00001);

            orderedEdges = n4Net_modified_orderedEdges;
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(n4Net_modified_expectedReliability, reliability, 0.00001);

            orderedEdges = p4Net_orderedEdges;
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(p4Net_expectedReliability, reliability, 0.00001);

            orderedEdges = network5_orderedEdges;
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(network5_expectedReliability, reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(fully_connected_8, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(fully_connected_8_expected_reliability, reliability, 0.00001);

            orderedEdges = BreadthFirstSearchAlgorithm.GetEdgeVisitedOrder(fully_connected_12, 0).ToList();
            edgeToType = SurvivalSignatureFuns.GetEdgeTypesFromReliability(orderedEdges);
            signature = ComputeBDDAlgorithm.ComputeSignature(orderedEdges, edgeToType, out dimToComponentType);
            dimToProbability = new double[signature.Rank];
            for (int i = 0; i < dimToProbability.Length; i++)
            {
                int componentType = dimToComponentType[i];
                dimToProbability[i] = edgeToType.Where(kvPair => kvPair.Value == componentType).First().Key.Reliability;
            }
            reliability = SurvivalSignatureFuns.GetProbability(signature, dimToProbability);
            Assert.AreEqual(fully_connected_12_expected_reliability, reliability, 0.00001);
        }
    }
}
