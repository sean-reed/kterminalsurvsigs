using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTerminalNetworkBDD
{
    public class Edge
    {
        public readonly string Label;
        
        public readonly Vertex V1;

        public readonly Vertex V2;

        public readonly double Reliability;

        public Edge(string label, Vertex v1, Vertex v2, double reliability)
        {
            this.Label = label;
            this.V1 = v1;
            this.V2 = v2;
            this.Reliability = reliability;
        }

        public override string ToString()
        { 
            return V1.Label + " -> " + V2.Label;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Label.GetHashCode();
                hash = hash * 31 + V1.GetHashCode();
                hash = hash * 31 + V2.GetHashCode();
                hash = hash * 31 + Reliability.GetHashCode();

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as Edge;

            if (other == null) return false;

            return this.Label.Equals(other.Label) && this.V1.Equals(other.V1) && this.V2.Equals(other.V2) &&
                   this.Reliability.Equals(other.Reliability);
        }
    }
}
