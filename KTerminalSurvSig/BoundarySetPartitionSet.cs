using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace KTerminalNetworkBDD
{
    public class BoundarySetPartitionSet
    {
        /// <summary>
        /// Returns the Stirling number of the second kind (also known as Stirling partition number), which
        /// gives the number of ways to partition a set of n elements into k non-empty subsets.
        /// </summary>
        /// <param name="n">The cardinality (i.e. number of elements) in the set.</param>
        /// <param name="k">The number of non-empty subsets into which the set is to be partitioned.</param>
        /// <returns>The Stirling number of the second kind for n and k.</returns>
        public static int StirlingNumber(int n, int k)
        {
            if (n < 0)
                throw new ArgumentException(nameof(n) + " must be greater than or equal to 0.");
            if (k < 0)
                throw new ArgumentException(nameof(k) + " must be greater than or equal to 0.");

            if (k > n)
                return 0;
            if (k == 1)
                return 1;
            else if (k == 0)
            {
                if (n == 0)
                    return 1;
                else
                    return 0;
            }

            return k * StirlingNumber(n - 1, k) + StirlingNumber(n - 1, k - 1);
        }

        /// <summary>
        /// Returns the Bell number, which gives the number of ways to partition a set of n elements.
        /// </summary>
        /// <param name="n">The cardinality (i.e. number of elements) of the set.</param>
        /// <returns>The bell number for a set of cardinality n.</returns>
        public static int BellNumber(int n)
        {
            if (n < 0) throw new ArgumentException(nameof(n) + " must be greater than or equal to 0.");

            if (n == 0)
                return 1;

            int bn = 0;
            for (int j = 1; j <= n; j++)
            {
                bn += StirlingNumber(n, j);
            }

            return bn;
        }

        private static long GetBinCoeff(long N, long K)
        {
            // This function gets the total number of unique combinations based upon N and K.
            // N is the total number of items.
            // K is the size of the group.
            // Total number of unique combinations = N! / ( K! (N - K)! ).
            // This function is less efficient, but is more likely to not overflow when N and K are large.
            // Taken from:  http://blog.plover.com/math/choose.html
            //
            long r = 1;
            long d;
            if (K > N) return 0;
            for (d = 1; d <= K; d++)
            {
                r *= N--;
                r /= d;
            }
            return r;
        }

        public static long MaxPartitions(int fCardinality, int k)
        {
            if (fCardinality < 1)
                throw new ArgumentException(nameof(fCardinality) + " must be greater than or equal to 1.");

            long w = 0;

            for (int j = 1; j <= fCardinality; j++)
            {
                int m = Math.Min(k, j);
                long maxMarkings = 0;
                for (int l = 0; l <= m; l++)
                {
                    maxMarkings += GetBinCoeff(j, l);
                }

                w += StirlingNumber(fCardinality, j) * maxMarkings;
            }

            return w;
        }

        public readonly byte[] VertexPartitions;

        public readonly bool[] PartitionMarkings;

        private readonly int _hashcode;

        public bool HasEmptyMarkedPartition
        {
            get
            {
                bool hasEmpty;
                if (VertexPartitions.Length > 0)
                {
                    hasEmpty = (VertexPartitions.Max() + 1) < PartitionMarkings.Length;
                }
                else
                {
                    hasEmpty = PartitionMarkings.Length > 0;
                }

                return hasEmpty;
            }
        }

        public bool HasMultipleMarkedPartitions
        {
            get
            {
                bool markedEncountered = false;
                for (int i = 0; i < PartitionMarkings.Length; i++)
                {
                    if (PartitionMarkings[i])
                    {
                        if (markedEncountered)
                            return true;
                        else
                        {
                            markedEncountered = true;
                        }
                    }
                }

                return false;
            }
        }

    public static BoundarySetPartitionSet ParseString(string bsps)
        {
            string[] seperatedStrings = bsps.Split('|');
            string partitionsString = seperatedStrings[0];
            string markingsString = seperatedStrings[1];

            byte[] vertexPartitions = partitionsString.Split(',').Select(byte.Parse).ToArray();
            bool[] partitionMarkings = markingsString.Split(',').Select(m => m == "T").ToArray();
            bool hasEmptyMarkedPartition = partitionMarkings.Skip(vertexPartitions.Max() + 1).Any(m => m);

            return new BoundarySetPartitionSet(vertexPartitions, partitionMarkings);
        }

        public BoundarySetPartitionSet(byte[] vertexPartitions, bool[] partitionMarkings)
        {
            VertexPartitions = vertexPartitions;
            PartitionMarkings = partitionMarkings;

            // Set hash code.
            unchecked
            {
                _hashcode = 17;

                for (int i = 0; i < VertexPartitions.Length; i++)
                {
                    _hashcode = _hashcode * 23 + (VertexPartitions[i]);
                }

                for (int i = 0; i < PartitionMarkings.Length; i++)
                {
                    _hashcode = _hashcode * 23 + (PartitionMarkings[i].GetHashCode());
                }
            }
        }

        public bool IsValid()
        {
            // Check that partition numbers are allocated from 0 and in correct order.
            int highestEncountered = -1;
            for (int i = 0; i < VertexPartitions.Length; i++)
            {
                if (VertexPartitions[i] > highestEncountered)
                {
                    if (VertexPartitions[i] != highestEncountered + 1)
                        return false;

                    highestEncountered++;
                }
            }

            // Check that all empty partitions are marked.
            for (int i = VertexPartitions.Max() + 1; i < PartitionMarkings.Length; i++)
            {
                if (!PartitionMarkings[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return _hashcode;
        }


        public bool Equals(BoundarySetPartitionSet other)
        {
            if (other == null)
            {
                return false;
            }

            if (VertexPartitions.Length != other.VertexPartitions.Length || PartitionMarkings.Length !=
                other.PartitionMarkings.Length)
            {
                return false;
            }

            for (int i = 0; i < VertexPartitions.Length; i++)
            {
                if (VertexPartitions[i] != other.VertexPartitions[i])
                    return false;
            }

            for (int i = 0; i < PartitionMarkings.Length; i++)
            {
                if (PartitionMarkings[i] != other.PartitionMarkings[i])
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            BoundarySetPartitionSet other = obj as BoundarySetPartitionSet;

            if (VertexPartitions.Length != other?.VertexPartitions.Length || PartitionMarkings.Length !=
                other.PartitionMarkings.Length)
            {
                return false;
            }

            for (int i = 0; i < VertexPartitions.Length; i++)
            {
                if (VertexPartitions[i] != other.VertexPartitions[i])
                    return false;
            }

            for (int i = 0; i < PartitionMarkings.Length; i++)
            {
                if (PartitionMarkings[i] != other.PartitionMarkings[i])
                    return false;
            }

            return true;
        }

        public string ToCompactString()
        {
            return String.Join(",", VertexPartitions) + "|" +
                   String.Join(",", PartitionMarkings.Select(marking => marking ? "T" : "F"));
        }

        public string ToString(IList<string> boundarySetVertexLabels)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < PartitionMarkings.Length; i++)
            {
                result.Append("[");
                List<string> partitionVertexLabels = new List<string>();
                for (int j = 0; j < VertexPartitions.Length; j++)
                {
                    if (VertexPartitions[j] == i)
                    {
                        partitionVertexLabels.Add(boundarySetVertexLabels[j]);
                    }
                }
                result.Append(String.Join(",", partitionVertexLabels));
                result.Append("]");
                if (PartitionMarkings[i])
                {
                    result.Append("*");
                }
            }

            if (HasEmptyMarkedPartition)
            {
                result.Append("[]*");
            }

            return result.ToString();
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < PartitionMarkings.Length; i++)
            {
                result.Append("[");
                for (int j = 0; j < VertexPartitions.Length; j++)
                {
                    if (VertexPartitions[j] == i)
                    {
                        result.Append(j);
                    }
                }
                result.Append("]");
                if (PartitionMarkings[i])
                {
                    result.Append("*");
                }
            }

            return result.ToString();
        }
    }
}
