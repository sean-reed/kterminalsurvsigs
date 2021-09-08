using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTerminalNetworkBDD
{
    public class Vertex
    {
        public readonly string Label;

        public readonly bool IsTerminal;

        public Vertex(string label, bool isTerminal)
        {
            this.Label = label;
            this.IsTerminal = isTerminal;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Vertex v = obj as Vertex;
            if ((System.Object)v == null)
            {
                return false;
            }

            return v.Label.Equals(this.Label) && v.IsTerminal.Equals(this.IsTerminal);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashcode = 17;
                hashcode = hashcode * 31 + Label.GetHashCode();
                hashcode = hashcode * 31 + IsTerminal.GetHashCode();
                return hashcode;
            }
        }

        public override string ToString()
        {
            string result;
            if(IsTerminal)
            {
                result = Label.ToString() + "*";
            }
            else
            {
                result = Label.ToString();
            }

            return result;
        }
    }
}
