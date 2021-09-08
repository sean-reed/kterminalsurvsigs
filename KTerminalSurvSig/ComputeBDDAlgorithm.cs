using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace KTerminalNetworkBDD
{
    public static class ComputeBDDAlgorithm
    {
        private static byte NOT_SET = 255;
        private static int NOT_FOUND = -1;
        private static int NOT_ENCOUNTERED = int.MaxValue;

        private enum PostiveChildGenerationAction
        {
            MarkPartitionVertex1,
            MarkPartitionVertex2,
            MarkedEmptyPartition,
            MergePriorPartitions,
            AddVertex2PartitionVertex1,
            AddVertex1PartitionVertex2,
            AddVerticesNewPartition,
            RemoveVertex,
            RemoveVertexAndMarkPartitionVertex1,
            RemoveVertexAndMarkPartitionVertex2,
            SameAsParent
        };

        public static void ModifyInputForDoesntMatterComponents(NDArray input, IEnumerable<Edge> doesntMatterEdges,
            Dictionary<Edge, int> edgeToComponentType,
            Dictionary<int, int> componentTypeToSigDim)
        {
            foreach (var edge in doesntMatterEdges)
            {
                int dimToShift = componentTypeToSigDim[edgeToComponentType[edge]];
                input.SumWithShiftOne(dimToShift);
            }
        }


        public static NDArray ComputeSignature(List<Edge> orderedEdges, Dictionary<Edge, int> edgeTypes, out int[] dimToComponentType)
        {
            /* Algorithm based on Figure 4 from "Comparison of Binary and Multi-Variate Hybrid Decision Diagram Algorithms for K-Terminal Reliability"
            by Herrmann and Soh, 2011.
            */
            HashSet<Vertex> terminalNodes = new HashSet<Vertex>();
            foreach (var e in orderedEdges)
            {
                if (e.V1.IsTerminal)
                    terminalNodes.Add(e.V1);
                if (e.V2.IsTerminal)
                    terminalNodes.Add(e.V2);
            }

            int K = terminalNodes.Count;

            List<List<Vertex>> boundarySets = GetBoundarySetsAlgorithm(orderedEdges);

            int level = 0;

            HashSet<Vertex> encounteredTerminalNodes = new HashSet<Vertex>();

            // Compute number of edges of each type.
            Dictionary<int, int> componentTypeCounts = edgeTypes.Values.GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());
            int[] sigShape = new int[componentTypeCounts.Count];
            int dimIndex = 0;
            dimToComponentType = new int[sigShape.Length];
            Dictionary<int, int> componentTypeToDim = new Dictionary<int, int>();
            foreach (var componentType in componentTypeCounts.Keys)
            {
                dimToComponentType[dimIndex] = componentType;
                componentTypeToDim[componentType] = dimIndex;
                sigShape[dimIndex] = componentTypeCounts[componentType] + 1;
                dimIndex++;
            }

            // Create root node.
            NDArray rootProbability = new NDArray(sigShape);
            rootProbability.SetValue(new int[componentTypeCounts.Count], 1);
            BoundarySetPartitionSet rootPartitions = new BoundarySetPartitionSet(new byte[] { }, new bool[] { });

            Dictionary<BoundarySetPartitionSet, NDArray> qc = new Dictionary<BoundarySetPartitionSet, NDArray>() { { rootPartitions, rootProbability } };

            Dictionary<BoundarySetPartitionSet, NDArray> qn = new Dictionary<BoundarySetPartitionSet, NDArray>();

            NDArray result = new NDArray(sigShape);
            int allTerminalNodesEncounteredLevel = NOT_ENCOUNTERED;
            bool allTerminalNodesEncountered = false;

            NDArrayPool arrayPool = new NDArrayPool(2000);

            for(int edgeIndex = 0; edgeIndex < orderedEdges.Count; edgeIndex++)
            {
                Edge edge = orderedEdges[edgeIndex];
                int edgeComponentType = edgeTypes[edge];

                int childLevel = level + 1;
                List<Vertex> parentBs = boundarySets[level];
                List<Vertex> childBs = boundarySets[childLevel];

                bool boundarySetSame = parentBs.SequenceEqual(childBs);

                // Create array of indices in parentBs of vertices from resultBs.
                int[] resultBsVertexIndicesInParentBs = new int[childBs.Count];
                for (int vertexIndex = 0; vertexIndex < childBs.Count; vertexIndex++)
                {
                    Vertex v = childBs[vertexIndex];
                    int indexInBs = parentBs.IndexOf(v);
                    resultBsVertexIndicesInParentBs[vertexIndex] = indexInBs;
                }

                int v1ParentBsIndex = parentBs.IndexOf(edge.V1);
                bool v1InParentBs = v1ParentBsIndex != NOT_FOUND;
                int v1ChildBsIndex = childBs.IndexOf(edge.V1);
                int v2ParentBsIndex = parentBs.IndexOf(edge.V2);
                bool v2InParentBs = v2ParentBsIndex != NOT_FOUND;
                int v2ChildBsIndex = childBs.IndexOf(edge.V2);

                if (!allTerminalNodesEncountered)
                {
                    if (edge.V1.IsTerminal)
                        encounteredTerminalNodes.Add(edge.V1);

                    if (edge.V2.IsTerminal)
                        encounteredTerminalNodes.Add(edge.V2);

                    if (encounteredTerminalNodes.Count == K)
                    {
                        allTerminalNodesEncounteredLevel = childLevel;
                        allTerminalNodesEncountered = true;
                    }
                }

                PostiveChildGenerationAction pcga =
                    GetPositiveChildGenerationAction(v1InParentBs, v2InParentBs, v1ChildBsIndex, v2ChildBsIndex, edge.V1.IsTerminal, edge.V2.IsTerminal);
                if (pcga == PostiveChildGenerationAction.SameAsParent && childLevel != allTerminalNodesEncounteredLevel)
                {
                    qn = qc;
                    throw new NotImplementedException("ABOVE LINE NEEDS TO UPDATE FOR MISSING VARIABLE.");
                }
                else
                {
                    Func<byte[], bool[], byte, byte, bool, bool, BoundarySetPartitionSet> getPositivechildFunc;
                    switch (pcga)
                    {
                        case PostiveChildGenerationAction.SameAsParent:
                            getPositivechildFunc =
                                (priorPartitions, partitionMarkings, v1Partition, v2Partition, v1Terminal,
                                        v2Terminal) =>
                                        new BoundarySetPartitionSet(priorPartitions, partitionMarkings);
                            break;
                        case PostiveChildGenerationAction.MergePriorPartitions:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition,
                                    v1Terminal, v2Terminal) =>
                                    MergePartitions(priorPartitions, partitionMarkings, v1Partition, v2Partition);
                            break;
                        case PostiveChildGenerationAction.AddVertex1PartitionVertex2:
                            getPositivechildFunc =
                                (priorPartitions, partitionMarkings, v1Partition, v2Partition, v1Terminal,
                                        v2Terminal) =>
                                        AddUnallocatedVertexToExistingPartition(priorPartitions, partitionMarkings,
                                            v2Partition,
                                            v1Terminal);
                            break;
                        case PostiveChildGenerationAction.AddVertex2PartitionVertex1:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition, v1Terminal,
                                    v2Terminal) =>
                                    AddUnallocatedVertexToExistingPartition(priorPartitions, partitionMarkings,
                                        v1Partition,
                                        v2Terminal);
                            break;
                        case PostiveChildGenerationAction.AddVerticesNewPartition:
                            getPositivechildFunc = AddUnallocatedVerticesToNewPartition;
                            break;
                        case PostiveChildGenerationAction.MarkPartitionVertex1:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition, v1Terminal,
                                    v2Terminal) =>
                                    UpdatePartitionMarking(priorPartitions, partitionMarkings, v1Partition);
                            break;
                        case PostiveChildGenerationAction.MarkPartitionVertex2:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition, v1Terminal,
                                    v2Terminal) =>
                                    UpdatePartitionMarking(priorPartitions, partitionMarkings, v2Partition);
                            break;
                        case PostiveChildGenerationAction.MarkedEmptyPartition:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition, v1Terminal,
                                v2Terminal) => SetEmptyMarkedPartition(priorPartitions, partitionMarkings);
                            break;
                        case PostiveChildGenerationAction.RemoveVertex:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition,
                                v1Terminal, v2Terminal) => RemoveVertex(priorPartitions, partitionMarkings);
                            break;
                        case PostiveChildGenerationAction.RemoveVertexAndMarkPartitionVertex1:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition,
                                v1Terminal, v2Terminal) => RemoveVertexAndMarkPartition(priorPartitions, partitionMarkings, v1Partition);
                            break;
                        case PostiveChildGenerationAction.RemoveVertexAndMarkPartitionVertex2:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition,
                                v1Terminal, v2Terminal) => RemoveVertexAndMarkPartition(priorPartitions, partitionMarkings, v2Partition);
                            break;
                        default:
                            throw new Exception("Unexpected case.");
                    }

                    foreach (var node in qc)
                    {
                        // START: Preliminary calculations of data that may be used in calculation of child nodes. 
                        byte v1ExistingPartition = v1ParentBsIndex == NOT_FOUND
                            ? NOT_SET
                            : node.Key.VertexPartitions[v1ParentBsIndex];
                        byte v2ExistingPartition = v2ParentBsIndex == NOT_FOUND
                            ? NOT_SET
                            : node.Key.VertexPartitions[v2ParentBsIndex];
                        var priorPartitions =
                            GetPriorPartitions(resultBsVertexIndicesInParentBs, node.Key.VertexPartitions);
                        // END: Preliminary calculations of data that may be used in calculation of child nodes.

                        // Calculate boundary set partition set for edge contraction.
                        var positiveChildPartitions = getPositivechildFunc(priorPartitions, node.Key.PartitionMarkings,
                            v1ExistingPartition, v2ExistingPartition, edge.V1.IsTerminal,
                            edge.V2.IsTerminal);

                        // Calculate boundary set partition set for edge deletion.
                        var negativeChildPartitions = GetNegativeChildPartitionSet(priorPartitions, node.Key.PartitionMarkings,
                            v1ChildBsIndex, v2ChildBsIndex, v1ExistingPartition, v2ExistingPartition, edge.V1.IsTerminal,
                            edge.V2.IsTerminal);

                        bool nodeValueRecycled = false; // Flag to set to true if the array is used in the next level of the BDD.

                        if (negativeChildPartitions.Equals(positiveChildPartitions))
                        {
                            if (allTerminalNodesEncountered && !negativeChildPartitions.HasMultipleMarkedPartitions)
                            {
                                ModifyInputForDoesntMatterComponents(node.Value, orderedEdges.GetRange(edgeIndex, orderedEdges.Count - edgeIndex), edgeTypes,
                                    componentTypeToDim);
                                result.SumWith(node.Value);
                            }
                            else if (!negativeChildPartitions.HasEmptyMarkedPartition)
                            {
                                if (qn.ContainsKey(negativeChildPartitions))
                                {
                                    qn[negativeChildPartitions].SumWithAndSumWithShiftOne(node.Value, componentTypeToDim[edgeComponentType]);
                                }
                                else
                                {
                                    node.Value.SumWithShiftOne(componentTypeToDim[edgeComponentType]);
                                    qn[negativeChildPartitions] = node.Value;
                                    nodeValueRecycled = true;
                                }
                            }
                        }
                        else
                        {
                            // Negative (removed / failed) edge.
                            if (allTerminalNodesEncountered && !negativeChildPartitions.HasMultipleMarkedPartitions)
                            {
                                ModifyInputForDoesntMatterComponents(node.Value, orderedEdges.GetRange(edgeIndex + 1, orderedEdges.Count - edgeIndex - 1), edgeTypes,
                                    componentTypeToDim);
                                result.SumWith(node.Value);
                                nodeValueRecycled = true;
                            }
                            else if (!negativeChildPartitions.HasEmptyMarkedPartition)
                            {
                                if (qn.ContainsKey(negativeChildPartitions))
                                {
                                    qn[negativeChildPartitions].SumWith(node.Value);
                                }
                                else
                                {
                                    qn[negativeChildPartitions] = node.Value;
                                    nodeValueRecycled = true;
                                }
                            }

                            // Positive (contracted / surviving) edge.
                            if (allTerminalNodesEncountered && !positiveChildPartitions.HasMultipleMarkedPartitions)
                            {
                                if(nodeValueRecycled)
                                {
                                    NDArray r = arrayPool.Pop(node.Value); 
                                    ModifyInputForDoesntMatterComponents(r, orderedEdges.GetRange(edgeIndex + 1, orderedEdges.Count - edgeIndex - 1), edgeTypes,
                                  componentTypeToDim);
                                    r.OneShifted(componentTypeToDim[edgeComponentType]);
                                    result.SumWith(r);
                                }
                                else
                                {                               
                                    ModifyInputForDoesntMatterComponents(node.Value, orderedEdges.GetRange(edgeIndex + 1, orderedEdges.Count - edgeIndex - 1), edgeTypes,
                              componentTypeToDim);
                                    node.Value.OneShifted(componentTypeToDim[edgeComponentType]);
                                    result.SumWith(node.Value);
                                    nodeValueRecycled = true;
                                }                            
                            }
                            else if (!positiveChildPartitions.HasEmptyMarkedPartition)
                            {
                                if (qn.ContainsKey(positiveChildPartitions))
                                {
                                    qn[positiveChildPartitions].SumWithShiftOne(node.Value, componentTypeToDim[edgeComponentType]);
                                }
                                else
                                {
                                    if(nodeValueRecycled)
                                    {
                                        NDArray temp = arrayPool.Pop(node.Value);
                                        temp.OneShifted(componentTypeToDim[edgeComponentType]);
                                        qn[positiveChildPartitions] = temp;
                                    }
                                    else
                                    {
                                        node.Value.OneShifted(componentTypeToDim[edgeComponentType]);
                                        qn[positiveChildPartitions] = node.Value;
                                        nodeValueRecycled = true;
                                    }
                                }
                            }
                        }

                        if (!nodeValueRecycled)
                        {
                            arrayPool.Push(node.Value);
                        }
                    }
                }

                if (qn.Count == 0)
                    break;

                qc = qn;
                qn = new Dictionary<BoundarySetPartitionSet, NDArray>();
                level++;
            }

            return result;
        }

        public static double ComputeReliability(IList<Edge> orderedEdges)
        {
            /* Algorithm based on Figure 4 from "Comparison of Binary and Multi-Variate Hybrid Decision Diagram Algorithms for K-Terminal Reliability"
            by Herrmann and Soh, 2011.
            */

            HashSet<Vertex> terminalNodes = new HashSet<Vertex>();
            foreach (var e in orderedEdges)
            {
                if (e.V1.IsTerminal)
                    terminalNodes.Add(e.V1);
                if (e.V2.IsTerminal)
                    terminalNodes.Add(e.V2);
            }

            int K = terminalNodes.Count;

            List<List<Vertex>> boundarySets = GetBoundarySetsAlgorithm(orderedEdges);

            int level = 0;

            HashSet<Vertex> encounteredTerminalNodes = new HashSet<Vertex>();

            // Create root node.
            double rootProbability = 1.0;
            BoundarySetPartitionSet rootPartitions = new BoundarySetPartitionSet(new byte[]{}, new bool[]{});

            Dictionary<BoundarySetPartitionSet, double> qc = new Dictionary<BoundarySetPartitionSet, double>() { { rootPartitions, rootProbability } };

            Dictionary<BoundarySetPartitionSet, double> qn = new Dictionary<BoundarySetPartitionSet, double>();

            double summedReliability = 0;
            int allTerminalNodesEncounteredLevel = NOT_ENCOUNTERED;
            bool allTerminalNodesEncountered = false;

            foreach (var edge in orderedEdges)
            {
                int childLevel = level + 1;
                List<Vertex> parentBs = boundarySets[level];
                List<Vertex> childBs = boundarySets[childLevel];

                bool boundarySetSame = parentBs.SequenceEqual(childBs);

                // Create array of indices in parentBs of vertices from resultBs.
                int[] resultBsVertexIndicesInParentBs = new int[childBs.Count];
                for (int vertexIndex = 0; vertexIndex < childBs.Count; vertexIndex++)
                {
                    Vertex v = childBs[vertexIndex];
                    int indexInBs = parentBs.IndexOf(v);
                    resultBsVertexIndicesInParentBs[vertexIndex] = indexInBs;
                }

                int v1ParentBsIndex = parentBs.IndexOf(edge.V1);
                bool v1InParentBs = v1ParentBsIndex != NOT_FOUND;
                int v1ChildBsIndex = childBs.IndexOf(edge.V1);
                int v2ParentBsIndex = parentBs.IndexOf(edge.V2);
                bool v2InParentBs = v2ParentBsIndex != NOT_FOUND;
                int v2ChildBsIndex = childBs.IndexOf(edge.V2);

                if (!allTerminalNodesEncountered)
                {
                    if (edge.V1.IsTerminal)
                        encounteredTerminalNodes.Add(edge.V1);

                    if (edge.V2.IsTerminal)
                        encounteredTerminalNodes.Add(edge.V2);

                    if (encounteredTerminalNodes.Count == K)
                    {
                        allTerminalNodesEncounteredLevel = childLevel;
                        allTerminalNodesEncountered = true;
                    }
                }

                PostiveChildGenerationAction pcga =
                    GetPositiveChildGenerationAction(v1InParentBs, v2InParentBs, v1ChildBsIndex, v2ChildBsIndex, edge.V1.IsTerminal, edge.V2.IsTerminal);
                if (pcga == PostiveChildGenerationAction.SameAsParent && childLevel != allTerminalNodesEncounteredLevel)
                {
                    qn = qc;
                }
                else
                {
                    Func<byte[], bool[], byte, byte, bool, bool, BoundarySetPartitionSet> getPositivechildFunc;
                    switch (pcga)
                    {
                        case PostiveChildGenerationAction.SameAsParent:
                            getPositivechildFunc =
                                (priorPartitions, partitionMarkings, v1Partition, v2Partition, v1Terminal,
                                        v2Terminal) =>
                                        new BoundarySetPartitionSet(priorPartitions, partitionMarkings);
                            break;
                        case PostiveChildGenerationAction.MergePriorPartitions:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition,
                                    v1Terminal, v2Terminal) =>
                                    MergePartitions(priorPartitions, partitionMarkings, v1Partition, v2Partition);
                            break;
                        case PostiveChildGenerationAction.AddVertex1PartitionVertex2:
                            getPositivechildFunc =
                                (priorPartitions, partitionMarkings, v1Partition, v2Partition, v1Terminal,
                                        v2Terminal) =>
                                        AddUnallocatedVertexToExistingPartition(priorPartitions, partitionMarkings,
                                            v2Partition,
                                            v1Terminal);
                            break;
                        case PostiveChildGenerationAction.AddVertex2PartitionVertex1:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition, v1Terminal,
                                    v2Terminal) =>
                                    AddUnallocatedVertexToExistingPartition(priorPartitions, partitionMarkings,
                                        v1Partition,
                                        v2Terminal);
                            break;
                        case PostiveChildGenerationAction.AddVerticesNewPartition:
                            getPositivechildFunc = AddUnallocatedVerticesToNewPartition;
                            break;
                        case PostiveChildGenerationAction.MarkPartitionVertex1:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition, v1Terminal,
                                    v2Terminal) =>
                                    UpdatePartitionMarking(priorPartitions, partitionMarkings, v1Partition);
                            break;
                        case PostiveChildGenerationAction.MarkPartitionVertex2:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition, v1Terminal,
                                    v2Terminal) =>
                                    UpdatePartitionMarking(priorPartitions, partitionMarkings, v2Partition);
                            break;
                        case PostiveChildGenerationAction.MarkedEmptyPartition:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition, v1Terminal,
                                v2Terminal) => SetEmptyMarkedPartition(priorPartitions, partitionMarkings);
                            break;
                        case PostiveChildGenerationAction.RemoveVertex:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition,
                                v1Terminal, v2Terminal) => RemoveVertex(priorPartitions, partitionMarkings);
                            break;
                        case PostiveChildGenerationAction.RemoveVertexAndMarkPartitionVertex1:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition,
                                v1Terminal, v2Terminal) => RemoveVertexAndMarkPartition(priorPartitions, partitionMarkings, v1Partition);
                            break;
                        case PostiveChildGenerationAction.RemoveVertexAndMarkPartitionVertex2:
                            getPositivechildFunc = (priorPartitions, partitionMarkings, v1Partition, v2Partition,
                                v1Terminal, v2Terminal) => RemoveVertexAndMarkPartition(priorPartitions, partitionMarkings, v2Partition);
                            break;
                        default:
                            throw new Exception("Unexpected case.");
                    }

                    double edgeUnreliability = 1.0 - edge.Reliability;

                    foreach (var node in qc)
                    {
                        // START: Preliminary calculations of data that may be used in calculation of child nodes. 
                        byte v1ExistingPartition = v1ParentBsIndex == NOT_FOUND
                            ? NOT_SET
                            : node.Key.VertexPartitions[v1ParentBsIndex];
                        byte v2ExistingPartition = v2ParentBsIndex == NOT_FOUND
                            ? NOT_SET
                            : node.Key.VertexPartitions[v2ParentBsIndex];
                        var priorPartitions =
                            GetPriorPartitions(resultBsVertexIndicesInParentBs, node.Key.VertexPartitions);
                        // END: Preliminary calculations of data that may be used in calculation of child nodes.

                        // Calculate boundary set partition set for edge contraction.
                        var positiveChildPartitions = getPositivechildFunc(priorPartitions, node.Key.PartitionMarkings,
                            v1ExistingPartition, v2ExistingPartition, edge.V1.IsTerminal,
                            edge.V2.IsTerminal);

                        // Calculate boundary set partition set for edge deletion.
                        var negativeChildPartitions = GetNegativeChildPartitionSet(priorPartitions, node.Key.PartitionMarkings,
                            v1ChildBsIndex, v2ChildBsIndex, v1ExistingPartition, v2ExistingPartition, edge.V1.IsTerminal,
                            edge.V2.IsTerminal);

                        if (negativeChildPartitions.Equals(positiveChildPartitions))
                        {
                            if (allTerminalNodesEncountered && !negativeChildPartitions.HasMultipleMarkedPartitions)
                            {
                                summedReliability += node.Value;
                            }
                            else if (!negativeChildPartitions.HasEmptyMarkedPartition)
                            {
                                if (qn.ContainsKey(negativeChildPartitions))
                                {
                                    qn[negativeChildPartitions] += node.Value;
                                }
                                else
                                {
                                    qn[negativeChildPartitions] = node.Value;
                                }
                            }
                        }
                        else
                        {
                            double positiveChildProbability = node.Value * edge.Reliability;
                            if (allTerminalNodesEncountered && !positiveChildPartitions.HasMultipleMarkedPartitions)
                            {
                                summedReliability += positiveChildProbability;
                            }
                            else if (!positiveChildPartitions.HasEmptyMarkedPartition)
                            {
                                if (qn.ContainsKey(positiveChildPartitions))
                                {
                                    qn[positiveChildPartitions] += positiveChildProbability;
                                }
                                else
                                {
                                    qn[positiveChildPartitions] = positiveChildProbability;
                                }
                            }

                            double negativeChildProbability;
                            if (allTerminalNodesEncountered && !negativeChildPartitions.HasMultipleMarkedPartitions)
                            {
                                negativeChildProbability = node.Value * edgeUnreliability;
                                summedReliability += negativeChildProbability;
                            }
                            else if (!negativeChildPartitions.HasEmptyMarkedPartition)
                            {
                                negativeChildProbability = node.Value * edgeUnreliability;
                                if (qn.ContainsKey(negativeChildPartitions))
                                {
                                    qn[negativeChildPartitions] += negativeChildProbability;
                                }
                                else
                                {
                                    qn[negativeChildPartitions] = negativeChildProbability;
                                }
                            }
                        }
                    }
                }

                if (qn.Count == 0)
                    break;

                qc = qn;
                qn = new Dictionary<BoundarySetPartitionSet, double>();
                level++;
            }

            return summedReliability;
        }

        /// <summary>
        /// Get array with prior priorPartitions of each vertex in result boundary set.
        /// </summary>
        /// <param name="resultBsVertexIndicesInParentBs"></param>
        /// <param name="partitions"></param>
        /// <returns></returns>
        private static byte[] GetPriorPartitions(int[] resultBsVertexIndicesInParentBs, byte[] partitions)
        {
            byte[] priorPartitions = new byte[resultBsVertexIndicesInParentBs.Length];
            for (int vertexIndex = 0; vertexIndex < resultBsVertexIndicesInParentBs.Length; vertexIndex++)
            {
                int indexInBs = resultBsVertexIndicesInParentBs[vertexIndex];
                if (indexInBs == NOT_FOUND)
                {
                    priorPartitions[vertexIndex] = NOT_SET;
                }
                else
                {
                    priorPartitions[vertexIndex] = partitions[indexInBs];
                }
            }
            return priorPartitions;
        }

        public static BoundarySetPartitionSet GetNegativeChildPartitionSet(byte[] priorPartitions, bool[] partitionMarkings, 
            int v1ChildBsIndex, int v2ChildBsIndex, byte v1Partition, byte v2Partition, bool v1IsTerminal, bool v2IsTerminal)
        {
            byte[] childVertexPartitions = new byte[priorPartitions.Length];

            List<bool> childMarkedPartitions = new List<bool>();

            byte nextFreePartition = 0;
            byte[] partitionMappings = new byte[partitionMarkings.Length];
            for (int i = 0; i < partitionMappings.Length; i++)
            {
                partitionMappings[i] = NOT_SET;
            }

            for (int vertexIndex = 0; vertexIndex < priorPartitions.Length; vertexIndex++)
            {
                byte priorPartition = priorPartitions[vertexIndex];
                if (priorPartition != NOT_SET)
                {
                    if(partitionMappings[priorPartition] != NOT_SET)
                        childVertexPartitions[vertexIndex] = partitionMappings[priorPartition];
                    else
                    {
                        childVertexPartitions[vertexIndex] = nextFreePartition;
                        partitionMappings[priorPartition] = nextFreePartition;
                        childMarkedPartitions.Add(partitionMarkings[priorPartition]);
                        nextFreePartition++;
                    }
                }
                else
                {
                    childVertexPartitions[vertexIndex] = nextFreePartition;
                    childMarkedPartitions.Add((vertexIndex == v1ChildBsIndex ? v1IsTerminal : v2IsTerminal));
                    nextFreePartition++;
                }
            }

            // Add any empty marked partitions.
            for (int partition = 0; partition < partitionMarkings.Length; partition++)
            {
                if (partitionMarkings[partition] && partitionMappings[partition] == NOT_SET)
                {
                    childMarkedPartitions.Add(true);
                }
            }

            if(v1Partition == NOT_SET && v1ChildBsIndex == NOT_FOUND && v1IsTerminal)
                childMarkedPartitions.Add(true);
            if(v2Partition == NOT_SET && v2ChildBsIndex == NOT_FOUND && v2IsTerminal)
                childMarkedPartitions.Add(true);

            BoundarySetPartitionSet bs =
                new BoundarySetPartitionSet(childVertexPartitions, childMarkedPartitions.ToArray());

            return bs;
        }

        private static BoundarySetPartitionSet SetEmptyMarkedPartition(byte[] priorPartitions, bool[] partitionMarkings)
        {
            bool[] childPartitionMarkings = new bool[partitionMarkings.Length + 1];
            partitionMarkings.CopyTo(childPartitionMarkings, 0);
            childPartitionMarkings[childPartitionMarkings.Length] = true;

            return new BoundarySetPartitionSet(priorPartitions, childPartitionMarkings);
        }

        private static BoundarySetPartitionSet UpdatePartitionMarking(byte[] priorPartitions, bool[] partitionMarkings,
            byte partitionToMark)
        {
            bool[] resultPartitionMarkings = new bool[partitionMarkings.Length];
            for (int i = 0; i < resultPartitionMarkings.Length; i++)
            {
                if (i != partitionToMark)
                {
                    resultPartitionMarkings[i] = partitionMarkings[i];
                }
                else
                {
                    resultPartitionMarkings[i] = true;
                }
            }

           return new BoundarySetPartitionSet(priorPartitions, resultPartitionMarkings);
        }

        private static BoundarySetPartitionSet AddUnallocatedVerticesToNewPartition(byte[] priorPartitions, bool[] partitionMarkings, byte v1Partition, byte v2Partition,
            bool v1Terminal, bool v2Terminal)
        {
            List<bool> resultMarkedPartitions = new List<bool>();

            byte[] partitionMappings = new byte[partitionMarkings.Length];
            for (int i = 0; i < partitionMarkings.Length; i++)
            {
                partitionMappings[i] = NOT_SET;
            }

            byte unallocatedPartition = NOT_SET;

            byte[] resultPartitions = new byte[priorPartitions.Length];
            byte nextFreePartition = 0;
            for (int vertexIndex = 0; vertexIndex < priorPartitions.Length; vertexIndex++)
            {
                int priorPartition = priorPartitions[vertexIndex];

                if (priorPartition == NOT_SET)
                {
                    if (unallocatedPartition == NOT_SET)
                    {
                        resultPartitions[vertexIndex] = nextFreePartition;
                        unallocatedPartition = nextFreePartition;
                        resultMarkedPartitions.Add(v1Terminal || v2Terminal);
                        nextFreePartition++;
                    }
                    else
                    {
                        resultPartitions[vertexIndex] = unallocatedPartition;
                    }

                }
                else if (partitionMappings[priorPartition] == NOT_SET)
                {
                    resultPartitions[vertexIndex] = nextFreePartition;
                    partitionMappings[priorPartition] = nextFreePartition;

                    resultMarkedPartitions.Add(partitionMarkings[priorPartition]);

                    nextFreePartition++;
                }
                else
                {
                    resultPartitions[vertexIndex] = partitionMappings[priorPartition];
                }
            }

            // Add any marked empty partitions.
            for (byte partition = 0; partition < partitionMarkings.Length; partition++)
            {
                if (partitionMappings[partition] == NOT_SET && partitionMarkings[partition])
                {
                    resultMarkedPartitions.Add(true);
                }
            }

            return new BoundarySetPartitionSet(resultPartitions, resultMarkedPartitions.ToArray());
        }

        private static BoundarySetPartitionSet AddUnallocatedVertexToExistingPartition(byte[] partitions, bool[] partitionMarkings, byte partition, bool isTerminal)
        {
            List<bool> resultMarkedPartitions = new List<bool>();

            byte[] partitionMappings = new byte[partitionMarkings.Length];
            for (int i = 0; i < partitionMarkings.Length; i++)
            {
                partitionMappings[i] = NOT_SET;
            }

            byte[] resultPartitions = new byte[partitions.Length];
            byte nextFreePartition = 0;
            for (int vertexIndex = 0; vertexIndex < partitions.Length; vertexIndex++)
            {
                byte priorPartition = partitions[vertexIndex];
                if (priorPartition == NOT_SET)
                {
                    priorPartition = partition;
                }

                if (partitionMappings[priorPartition] == NOT_SET)
                {
                    resultPartitions[vertexIndex] = nextFreePartition;
                    partitionMappings[priorPartition] = nextFreePartition;
                    nextFreePartition++;
                    resultMarkedPartitions.Add(partitionMarkings[priorPartition]);
                }
                else
                {
                    resultPartitions[vertexIndex] = partitionMappings[priorPartition];
                }
            }

            // Add any marked empty partitions.
            for (int partitionIndex = 0; partitionIndex < partitionMarkings.Length; partitionIndex++)
            {
                if (partitionMarkings[partitionIndex] && partitionMappings[partitionIndex] == NOT_SET)
                {
                    resultMarkedPartitions.Add(true);
                }
            }

            bool[] resultMarkedPartitionsArray = resultMarkedPartitions.ToArray();
            resultMarkedPartitionsArray[partitionMappings[partition]] =
                resultMarkedPartitionsArray[partitionMappings[partition]] || isTerminal;

            return new BoundarySetPartitionSet(resultPartitions, resultMarkedPartitionsArray);
        }

        private static BoundarySetPartitionSet RemoveVertex(byte[] priorPartitions, bool[] partitionMarkings)
        {
            List<bool> childPartitionMarkings = new List<bool>();

            byte[] partitionMappings = new byte[partitionMarkings.Length];
            for (int i = 0; i < partitionMarkings.Length; i++)
            {
                partitionMappings[i] = NOT_SET;
            }

            byte[] resultPartitions = new byte[priorPartitions.Length];
            byte nextFreePartition = 0;
            for (int vertexIndex = 0; vertexIndex < priorPartitions.Length; vertexIndex++)
            {
                byte priorPartition = priorPartitions[vertexIndex];
                if (partitionMappings[priorPartition] != NOT_SET) // Prior partition already mapped to a partition.
                    resultPartitions[vertexIndex] = partitionMappings[priorPartition];
                else
                {
                    resultPartitions[vertexIndex] = nextFreePartition;
                    partitionMappings[priorPartition] = nextFreePartition;
                    childPartitionMarkings.Add(partitionMarkings[priorPartition]);
                    nextFreePartition++;
                }
            }

            // Add any empty marked partition.
            for (int partition = 0; partition < partitionMarkings.Length; partition++)
            {
                if (partitionMappings[partition] == NOT_SET && partitionMarkings[partition])
                {
                    childPartitionMarkings.Add(true);
                }
            }

            return new BoundarySetPartitionSet(resultPartitions, childPartitionMarkings.ToArray());
        }

        private static BoundarySetPartitionSet RemoveVertexAndMarkPartition(byte[] priorPartitions, bool[] partitionMarkings, int partitionToMark)
        {
            List<bool> childPartitionMarkings = new List<bool>();

            byte[] partitionMappings = new byte[partitionMarkings.Length];
            for (int i = 0; i < partitionMarkings.Length; i++)
            {
                partitionMappings[i] = NOT_SET;
            }

            byte[] resultPartitions = new byte[priorPartitions.Length];
            byte nextFreePartition = 0;
            for (int vertexIndex = 0; vertexIndex < priorPartitions.Length; vertexIndex++)
            {
                byte priorPartition = priorPartitions[vertexIndex];
                if (partitionMappings[priorPartition] != NOT_SET) // Prior partition already mapped to a partition.
                    resultPartitions[vertexIndex] = partitionMappings[priorPartition];
                else
                {
                    resultPartitions[vertexIndex] = nextFreePartition;
                    partitionMappings[priorPartition] = nextFreePartition;
                    childPartitionMarkings.Add(partitionMarkings[priorPartition] || priorPartition == partitionToMark);
                    nextFreePartition++;
                }
            }

            // Add any empty marked partition.
            for (int partition = 0; partition < partitionMarkings.Length; partition++)
            {
                if (partitionMappings[partition] == NOT_SET && (partitionMarkings[partition] || partition == partitionToMark))
                {
                    childPartitionMarkings.Add(true);
                }
            }

            return new BoundarySetPartitionSet(resultPartitions, childPartitionMarkings.ToArray());
        }

        private static BoundarySetPartitionSet MergePartitions(byte[] priorPartitions,  bool[] partitionMarkings, byte v1Partition, byte v2Partition)
        {
            List<bool> childPartitionMarkings = new List<bool>();

            byte[] partitionMappings = new byte[partitionMarkings.Length];
            for (int i = 0; i < partitionMarkings.Length; i++)
            {
                partitionMappings[i] = NOT_SET;
            }

            // Compute merged priorPartitions.
            byte[] resultPartitions = new byte[priorPartitions.Length];
            byte nextFreePartition = 0;
            for (int vertexIndex = 0; vertexIndex < priorPartitions.Length; vertexIndex++)
            {
                byte priorPartition = priorPartitions[vertexIndex];
                if (partitionMappings[priorPartition] != NOT_SET) // Prior partition already mapped to a partition.
                    resultPartitions[vertexIndex] = partitionMappings[priorPartition];
                else
                {
                    resultPartitions[vertexIndex] = nextFreePartition;

                    if (priorPartition == v1Partition || priorPartition == v2Partition)
                    {
                        partitionMappings[v1Partition] = nextFreePartition;
                        partitionMappings[v2Partition] = nextFreePartition;
                        childPartitionMarkings.Add(partitionMarkings[v1Partition] || partitionMarkings[v2Partition]);
                    }
                    else
                    {
                        partitionMappings[priorPartition] = nextFreePartition;
                        childPartitionMarkings.Add(partitionMarkings[priorPartition]);
                    }
                    nextFreePartition++;
                }
            }

            // Add any empty marked partition.
            for (int partition = 0; partition < partitionMarkings.Length; partition++)
            {
                if (partitionMappings[partition] == NOT_SET && partitionMarkings[partition])
                {
                    childPartitionMarkings.Add(true);
                    break; // Since nodes are merged, they are from only (now empty) partition.
                }
            }

            return new BoundarySetPartitionSet(resultPartitions, childPartitionMarkings.ToArray());
        }

        private static PostiveChildGenerationAction GetPositiveChildGenerationAction(bool v1InParentBs, bool v2InParentBs, int v1ResultBsIndex, 
            int v2ResultBsIndex, bool v1IsTerminal, bool v2IsTerminal)
        {

            if (v1ResultBsIndex != NOT_FOUND)
            {
                if (v2ResultBsIndex != NOT_FOUND)
                {
                    if (v1InParentBs)
                    {
                        if (v2InParentBs)
                        {
                            // Case 1.
                            // Merge priorPartitions.
                            return PostiveChildGenerationAction.MergePriorPartitions;
                        }
                        else
                        {
                            // Case 2.
                            // Add vertex 2 to partition of vertex 1.
                            return PostiveChildGenerationAction.AddVertex2PartitionVertex1;
                        }
                    }
                    else
                    {
                        if (v2InParentBs)
                        {
                            // Case 3.
                            // Add vertex 1 to partition of vertex 2.
                            return PostiveChildGenerationAction.AddVertex1PartitionVertex2;
                        }
                        else
                        {
                            // Case 4.
                            // Add vertices 1 and 2 to new partition.
                            return PostiveChildGenerationAction.AddVerticesNewPartition;
                        }
                    }
                }
                else
                {
                    if (v1InParentBs)
                    {
                        if (v2InParentBs)
                        {
                            // Case 5.
                            // Merge priorPartitions.
                            return PostiveChildGenerationAction.MergePriorPartitions;
                        }
                        else
                        {
                            // Case 6.
                            // Same as parent BS partition set.
                            if (v2IsTerminal)
                            {
                                return PostiveChildGenerationAction.MarkPartitionVertex1;
                            }
                            else
                            {
                                return PostiveChildGenerationAction.SameAsParent;
                            }
                        }
                    }
                    else
                    {
                        if (v2InParentBs)
                        {
                            // Case 7. 
                            // Add vertex 1 to partition of vertex 2.
                            return PostiveChildGenerationAction.AddVertex1PartitionVertex2;
                        }
                        else
                        {
                            // Case 8.
                            // Add vertex 1 to new partition.
                            return PostiveChildGenerationAction.AddVerticesNewPartition;
                        }
                    }
                }
            }
            else
            {
                if (v2ResultBsIndex != NOT_FOUND)
                {
                    if (v1InParentBs)
                    {
                        if (v2InParentBs)
                        {
                            // Case 9.
                            // Merge priorPartitions.
                            return PostiveChildGenerationAction.MergePriorPartitions;
                        }
                        else
                        {
                            // Case 10.
                            // Add vertex 2 to partition of vertex 1.
                            return PostiveChildGenerationAction.AddVertex2PartitionVertex1;
                        }
                    }
                    else
                    {
                        if (v2InParentBs)
                        {
                            // Case 11.
                            // Same as parent BS partition set.
                            if (v1IsTerminal)
                            {
                                return PostiveChildGenerationAction.MarkPartitionVertex2;
                            }
                            else
                            {
                                return PostiveChildGenerationAction.SameAsParent;
                            }
                        }
                        else
                        {
                            // Case 12.
                            // Add vertex 2 to new partition.
                            return PostiveChildGenerationAction.AddVerticesNewPartition;
                        }
                    }
                }
                else
                {
                    if (v1InParentBs)
                    {
                        if (v2InParentBs)
                        {
                            // Case 13.
                            // Merge priorPartitions.
                            return PostiveChildGenerationAction.MergePriorPartitions;
                        }
                        else
                        {
                            // Case 14.
                            if (v2IsTerminal)
                            {
                                return PostiveChildGenerationAction.RemoveVertexAndMarkPartitionVertex1;
                            }
                            else
                            {
                                return PostiveChildGenerationAction.RemoveVertex;
                            }
                        }
                       
                    }
                    else
                    {
                        if (v2InParentBs)
                        {
                            // Case 15.
                            if (v1IsTerminal)
                            {
                                return PostiveChildGenerationAction.RemoveVertexAndMarkPartitionVertex2;
                            }
                            else
                            {
                                return PostiveChildGenerationAction.RemoveVertex;
                            }
                        }
                        else
                        {
                            // Case 16.
                            if (v1IsTerminal || v2IsTerminal)
                            {
                                return PostiveChildGenerationAction.MarkedEmptyPartition;
                            }
                            else
                            {
                                return PostiveChildGenerationAction.SameAsParent;
                            }
                        }
                      
                    }
                }
            }
        }
        
        public static List<List<Vertex>> GetBoundarySetsAlgorithm(IList<Edge> orderedEdges)
        {
            // Store the total number of adjacent edges for each vertex.
            Dictionary<Vertex, int> totalAdjacentEdgeCounts = new Dictionary<Vertex, int>();

            foreach (var edge in orderedEdges)
            {
                if (totalAdjacentEdgeCounts.ContainsKey(edge.V1))
                {
                    totalAdjacentEdgeCounts[edge.V1]++;
                }
                else
                {
                    totalAdjacentEdgeCounts[edge.V1] = 1;
                }

                if (totalAdjacentEdgeCounts.ContainsKey(edge.V2))
                {
                    totalAdjacentEdgeCounts[edge.V2]++;
                }
                else
                {
                    totalAdjacentEdgeCounts[edge.V2] = 1;
                }
            }

            Edge firstEdge = orderedEdges.First();
            List<List<Vertex>> boundarySets = new List<List<Vertex>>() { new List<Vertex>(){} };
            Dictionary<Vertex, int> remainingAdjacentEdgeCounts = new Dictionary<Vertex, int>(totalAdjacentEdgeCounts);
            List<Vertex> boundarySet = new List<Vertex>();
            foreach (var edge in orderedEdges)
            {
                remainingAdjacentEdgeCounts[edge.V1]--;
                remainingAdjacentEdgeCounts[edge.V2]--;

                if (remainingAdjacentEdgeCounts[edge.V1] == 0)
                {
                    boundarySet.Remove(edge.V1);
                }
                else if (remainingAdjacentEdgeCounts[edge.V1] == (totalAdjacentEdgeCounts[edge.V1] - 1))
                {
                    boundarySet.Add(edge.V1);
                }

                if (remainingAdjacentEdgeCounts[edge.V2] == 0)
                {
                    boundarySet.Remove(edge.V2);
                }
                else if (remainingAdjacentEdgeCounts[edge.V2] == (totalAdjacentEdgeCounts[edge.V2] - 1))
                {
                    boundarySet.Add(edge.V2);
                }

                boundarySets.Add(new List<Vertex>(boundarySet));
            }

            return boundarySets;
        }
    }
}
