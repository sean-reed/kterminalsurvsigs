using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTerminalNetworkBDD
{
    public class BoundarySetEdge
    {
        public readonly bool AllTerminalNodesVisited;

        public readonly bool V1IsTerminal;

        public bool V2IsTerminal { get; private set; }

        public readonly int V1SourceBoundarySetIndex;

        public readonly int V2SourceBoundarySetIndex;

        public readonly int V1TargetBoundarySetIndex;

        public readonly int V2TargetBoundarySetIndex;

        public BoundarySetEdge(bool allTerminalNodesVisited, bool v1IsTerminal, bool v2IsTerminal, int v1SourceBoundarySetIndex, int v2SourceBoundarySetIndex,
            int v1TargetBoundarySetIndex, int v2TargetBoundarySetIndex)
        {
            AllTerminalNodesVisited = allTerminalNodesVisited;
            V1IsTerminal = v1IsTerminal;
            V2IsTerminal = v2IsTerminal;
            V1SourceBoundarySetIndex = v1SourceBoundarySetIndex;
            V2SourceBoundarySetIndex = v2SourceBoundarySetIndex;
            V1TargetBoundarySetIndex = v1TargetBoundarySetIndex;
            V2TargetBoundarySetIndex = v2TargetBoundarySetIndex;
        }
    }
}
