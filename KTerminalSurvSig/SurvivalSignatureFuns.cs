using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTerminalNetworkBDD
{
    public static class SurvivalSignatureFuns
    {
        private static double GetBinCoeff(uint N, uint K)
        {
            double r = 1;
            double d;
            if (K > N) return 0;
            for (d = 1; d <= K; d++)
            {
                r *= N--;
                r /= d;
            }
            return r;
        }

        public static double GetProbability(NDArray unnormalisedSignature, double[] compSurvivalProbabilityByDim)
        {
            return UpdateProbabilityInner(unnormalisedSignature, compSurvivalProbabilityByDim, new int[unnormalisedSignature.Shape.Length], 0);
        }

        private static double UpdateProbabilityInner(NDArray unnormalisedSignature, double[] dimToSurvivalProbability, int[] dimIndices, int dim)
        {
            double accumulatedProbability = 0;
            if (dim == unnormalisedSignature.Shape.Length - 1)
            {
                for (int valueDimIndex = 0; valueDimIndex < unnormalisedSignature.Shape[dim]; valueDimIndex++)
                {
                    dimIndices[dim] = valueDimIndex;
                    double allComponentsSurviveProbability = 1;
                    for (int i = 0; i < dimIndices.Length; i++)
                    {
                        allComponentsSurviveProbability *= Math.Pow(dimToSurvivalProbability[i], dimIndices[i]) * Math.Pow(1.0 - dimToSurvivalProbability[i], unnormalisedSignature.Shape[i] - dimIndices[i] - 1);
                        
                    }
                    accumulatedProbability += allComponentsSurviveProbability * unnormalisedSignature.GetValue(dimIndices);
                }
            }
            else
            {
                for (int valueDimIndex = 0; valueDimIndex < unnormalisedSignature.Shape[dim]; valueDimIndex++)
                {
                    dimIndices[dim] = valueDimIndex;
                    accumulatedProbability += UpdateProbabilityInner(unnormalisedSignature, dimToSurvivalProbability, dimIndices, dim + 1);
                }
            }

            return accumulatedProbability;
        }

        public static NDArray GetNormalisationArray(int[] shape)
        {
            NDArray output = new NDArray(shape);
            SetNormalisationValues(shape, new int[shape.Length], 0, output);

            return output;
        }

        private static void SetNormalisationValues(int[] shape, int[] dimIndices, int dim, NDArray output)
        {
            if (dim == shape.Length - 1)
            {
                for (int valueDimIndex = 0; valueDimIndex < shape[dim]; valueDimIndex++)
                {
                    dimIndices[dim] = valueDimIndex;
                    double normalisationFactorAtIndex = 1;
                    for (int i = 0; i < dimIndices.Length; i++)
                    {
                        normalisationFactorAtIndex *= GetBinCoeff((uint)output.Shape[i] - 1, (uint)dimIndices[i]);
                    }
                    output.SetValue(dimIndices, normalisationFactorAtIndex);  
                }
            }
            else
            {
                for (int valueDimIndex = 0; valueDimIndex < shape[dim]; valueDimIndex++)
                {
                    dimIndices[dim] = valueDimIndex;
                    SetNormalisationValues(shape, dimIndices, dim + 1, output);
                }
            }
        }

        public static Dictionary<Edge, int> GetEdgeTypesFromReliability(IList<Edge> edges)
        {
            Dictionary<double, int> reliabilityToType = new Dictionary<double, int>();
            Dictionary<Edge, int> edgeToType = new Dictionary<Edge, int>();
            int nextFreeType = 0;
            foreach (var edge in edges)
            {
                int edgeType;
                if(reliabilityToType.ContainsKey(edge.Reliability))
                {
                    edgeType = reliabilityToType[edge.Reliability];
                }
                else
                {
                    edgeType = nextFreeType;
                    reliabilityToType[edge.Reliability] = edgeType;
                    nextFreeType++;
                }

                edgeToType[edge] = edgeType;
            }

            return edgeToType;
        }

        public static string PrintAsTable(NDArray values, int[] dimToComponentType)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(String.Join(",", dimToComponentType));
            BuildDimIndices(values, new int[values.Rank], 0, output);

            return output.ToString();
        }

        private static void BuildDimIndices(NDArray values, int[] dimIndices, int dim, StringBuilder output)
        {
            if(dim == values.Rank - 1)
            {
                for (int valueDimIndex = 0; valueDimIndex < values.Shape[dim]; valueDimIndex++)
                {
                    dimIndices[dim] = valueDimIndex;
                    output.AppendLine(String.Join(",", dimIndices) + String.Format(": {0}", values.GetValue(dimIndices)));
                }
            }
            else
            {
                for (int valueDimIndex = 0; valueDimIndex < values.Shape[dim]; valueDimIndex++)
                {
                    dimIndices[dim] = valueDimIndex;
                    BuildDimIndices(values, dimIndices, dim + 1, output);
                }
            }
        }
    }
}
