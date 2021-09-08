using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTerminalNetworkBDD
{
    /// <summary>
    /// Numpy style multidimensional array.
    /// </summary>
    public class NDArray
    {
        /// <summary>
        /// Length of each dimension.
        /// </summary>
        public int[] Shape { get; private set; }

        /// <summary>
        /// Number of dimensions.
        /// </summary>
        public int Rank { get { return Shape.Length; } }

        /// <summary>
        /// Distance between adjacent elements in each dimension.
        /// </summary>
        public int[] Strides { get; private set; }

        /// <summary>
        /// Number of elements in array.
        /// </summary>
        public int Length { get { return _values.Length; } }

        private double[] _values; // Values stored in the array.

        public static double Sparsity(NDArray array)
        {
            return (double)array._values.Count(v => v == 0) / array.Length;
        }

        public static NDArray FromValues(double[] values)
        {
            int[] shape = new int[] { values.Length };
            NDArray array = new NDArray(shape);
            for (int i = 0; i < values.Length; i++)
            {
                array._values[i] = values[i];
            }

            return array;
        }

        public static NDArray FromValues(double[,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);

            int[] shape = new int[] { dim0Length, dim1Length };
            NDArray array = new NDArray(shape);

            for (int i = 0; i < dim0Length; i++)
            {
                for (int j = 0; j < dim1Length; j++)
                {
                    array._values[i * array.Strides[0] + j * array.Strides[1]] = values[i, j];
                }
            }

            return array;
        }

        public static NDArray FromValues(double[,,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
            int dim2Length = values.GetLength(2);

            int[] shape = new int[] { dim0Length, dim1Length, dim2Length };
            NDArray array = new NDArray(shape);

            for (int i = 0; i < dim0Length; i++)
            {
                for (int j = 0; j < dim1Length; j++)
                {
                    for (int k = 0; k < dim2Length; k++)
                    {
                        array._values[i * array.Strides[0] + j * array.Strides[1] + k * array.Strides[2]] = values[i, j, k];
                    }
                }
            }

            return array;
        }

        public static NDArray Divide(NDArray numerator, NDArray denominator)
        {
            if (!numerator.Shape.SequenceEqual(denominator.Shape))
                throw new ArgumentException("Shapes of arrays do not match.");
            else
            {
                for (int i = 0; i < numerator.Shape.Length; i++)
                {
                    if (numerator.Shape[i] != denominator.Shape[i])
                        throw new ArgumentException("Lengths of signatures does not match in dimension {0}.", i.ToString());
                }
            }

            NDArray result = new NDArray(numerator.Shape);
            for (int i = 0; i < result.Length; i++)
            {
                result._values[i] = numerator._values[i] / denominator._values[i];
            }

            return result;
        }

        public static NDArray Multiply(NDArray a, NDArray b)
        {
            if (!a.Shape.SequenceEqual(b.Shape))
                throw new ArgumentException("Shapes of arrays do not match.");
            else
            {
                for (int i = 0; i < a.Shape.Length; i++)
                {
                    if (a.Shape[i] != b.Shape[i])
                        throw new ArgumentException("Lengths of signatures does not match in dimension {0}.", i.ToString());
                }
            }

            NDArray result = new NDArray(a.Shape);
            for (int i = 0; i < result.Length; i++)
            {
                result._values[i] = a._values[i] * b._values[i];
            }

            return result;
        }

        public static NDArray Sum(NDArray a, NDArray b)
        {
            if (a.Shape.Length != b.Shape.Length)
                throw new ArgumentException("Dimensions of arrays to be summed to do match.");
            else
            {
                for (int i = 0; i < a.Shape.Length; i++)
                {
                    if (a.Shape[i] != b.Shape[i])
                        throw new ArgumentException("Lengths of signatures does not match in dimension {0}.", i.ToString());
                }
            }

            NDArray summed = new NDArray(a.Shape);
            for (int i = 0; i < summed.Length; i++)
            {
                summed._values[i] = a._values[i] + b._values[i];
            }

            return summed;
        }

        private static IEnumerable<int> ValueIndices(int rank, int[] strides, int[] lowerIndices, int[] upperIndices)
        {
            return ValueIndices(rank, strides, lowerIndices, upperIndices, 0, 0);
        }

        private static IEnumerable<int> ValueIndices(int rank, int[] strides, int[] lowerIndices, int[] upperIndices, int currentDimension, int valueIndex)
        {
            if (currentDimension == rank)
            {
                yield return valueIndex;
            }
            else
            {
                for (int i = lowerIndices[currentDimension]; i <= upperIndices[currentDimension]; i++)
                {
                    valueIndex += strides[currentDimension] * i;

                    ValueIndices(rank, strides, lowerIndices, upperIndices, currentDimension + 1, valueIndex);
                }
            }
        }

        /// <summary>
        /// Non-rotational shift of an array in a specified dimension by one position forward.
        /// </summary>
        /// <param name="a">The input array.</param>
        /// <param name="dim">The dimension in which to shift.</param>
        /// <returns>The shifted output array.</returns>
        public static NDArray GetOneShifted(NDArray a, int dim)
        {
            if (dim < 0 || dim > a.Rank) throw new ArgumentOutOfRangeException("dim");

            NDArray result = new NDArray(a.Shape);

            int dimLength = a.Shape[dim];
            int shiftStride = a.Strides[dim];
            int consecutiveShifts = shiftStride * (dimLength - 1); // Number of consecutive elements in _values that are shifted.
            int consecutiveSameDimIndex = shiftStride * dimLength; // Number of consecutive elements in _values that have the same index in dimension dim.

            for (int i = 0; i < a._values.Length; i += consecutiveSameDimIndex) // Jump to first element at next index in dimension dim at each iteration.
            {
                for (int j = i; j < consecutiveShifts + i; j++) // Copy the shifted values from the next index in dimension dim to the result array.
                {
                    result._values[j + shiftStride] = a._values[j];
                }
            }

            return result;
        }

        public void OneShifted(int dim)
        {
            if (dim < 0 || dim > Rank) throw new ArgumentOutOfRangeException("dim");

            int dimLength = Shape[dim];
            int shiftStride = Strides[dim];
            int consecutiveShifts = shiftStride * (dimLength - 1); // Number of consecutive elements in _values that are shifted.
            int consecutiveSameDimIndex = shiftStride * dimLength; // Number of consecutive elements in _values that have the same index in dimension dim.

            for (int j = _values.Length - 1; j >= 0;) // Jump to first element at next index in dimension dim at each iteration.
            {
                int i = j;
                for (; j > i - consecutiveShifts; j--) // Copy the shifted values from the next index in dimension dim to the result array.
                {
                    _values[j] = _values[j - shiftStride];
                }
                for (; j > i - consecutiveSameDimIndex; j--)
                {
                    _values[j] = 0;
                }
            }
        }

        public void SumWithShiftOne(int dim)
        {
            if (dim < 0 || dim > Rank) throw new ArgumentOutOfRangeException("dim");

            int dimLength = Shape[dim];
            int shiftStride = Strides[dim];
            int consecutiveShifts = shiftStride * (dimLength - 1); // Number of consecutive elements in _values that are shifted.
            int consecutiveSameDimIndex = shiftStride * dimLength; // Number of consecutive elements in _values that have the same index in dimension dim.

            for (int i = _values.Length - 1; i >= 0; i -= consecutiveSameDimIndex) // Jump to first element at next index in dimension dim at each iteration.
            {
                for (int j = i; j > i - consecutiveShifts; j--) // Copy the shifted values from the next index in dimension dim to the result array.
                {
                    _values[j] = _values[j] + _values[j - shiftStride];
                }
            }
        }

        public void SumWithShiftOne(NDArray other, int dim)
        {
            if (!Shape.SequenceEqual(other.Shape))
            {
                throw new ArgumentException("Other not of same shape.");
            }

            if (dim < 0 || dim > Rank) throw new ArgumentOutOfRangeException("dim");

            int dimLength = Shape[dim];
            int shiftStride = Strides[dim];
            int consecutiveShifts = shiftStride * (dimLength - 1); // Number of consecutive elements in _values that are shifted.
            int consecutiveSameDimIndex = shiftStride * dimLength; // Number of consecutive elements in _values that have the same index in dimension dim.

            for (int i = _values.Length - 1; i >= 0; i -= consecutiveSameDimIndex) // Jump to first element at next index in dimension dim at each iteration.
            {
                for (int j = i; j > i - consecutiveShifts; j--) // Copy the shifted values from the next index in dimension dim to the result array.
                {
                    _values[j] = _values[j] + other._values[j - shiftStride];
                }
            }
        }

        public void SumWithAndSumWithShiftOne(NDArray other, int dim)
        {
            //if(!Shape.SequenceEqual(other.Shape))
            //{
            //    throw new ArgumentException("Other not of same shape.");
            //}

            if (dim < 0 || dim > Rank) throw new ArgumentOutOfRangeException("dim");

            var ptr = other._values;

            int dimLength = Shape[dim];
            int shiftStride = Strides[dim];
            int consecutiveShifts = shiftStride * (dimLength - 1); // Number of consecutive elements in _values that are shifted.
            int consecutiveSameDimIndex = shiftStride * dimLength; // Number of consecutive elements in _values that have the same index in dimension dim.

            for (int j = _values.Length - 1; j >= 0;) // Jump to first element at next index in dimension dim at each iteration.
            {
                int i = j;
                for (; j > i - consecutiveShifts; j--) // Copy the shifted values from the next index in dimension dim to the result array.
                {
                    _values[j] = _values[j] + ptr[j] + ptr[j - shiftStride];
                }
                for(; j > i - consecutiveSameDimIndex; j--)
                {
                    _values[j] = _values[j] + ptr[j];
                }
            }
        }

        /// <summary>
        /// Non-rotational shift.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static NDArray Shift(NDArray a, int[] shift)
        {
            if (shift.Length != a.Shape.Length) throw new ArgumentException("Dimensions of shift do not match those of a.");

            // Initialise shifted as a zeroed signature of same shape as a.
            NDArray result = new NDArray(a.Shape);

            int rank = a.Rank;

            // Calculate the total stride between an index and the index to which its value is shifted.
            int shiftStride = 0;
            for (int i = 0; i < rank; i++)
            {
                shiftStride += shift[i] * a.Strides[i];
            }

            // Get the lower and upper indices in each dimension for the values that will be shifted.
            int[] lowerIndices = new int[shift.Length];
            int[] uppderIndices = new int[shift.Length];
            for (int dim = 0; dim < rank; dim++)
            {
                lowerIndices[dim] = 0;
                uppderIndices[dim] = a.Shape[dim] - shift[dim] - 1;
            }

            foreach (var index in ValueIndices(a.Rank, a.Strides, lowerIndices, uppderIndices))
            {
                result._values[index + shiftStride] = a._values[index];
            }

            return result;
        }
        
        public static bool ArrayEqual(NDArray a, NDArray b)
        {
            return a.Shape.SequenceEqual(b.Shape) && a._values.SequenceEqual(b._values);
        }

        public NDArray(int[] shape)
        {
            this.Shape = shape;

            // Create array to hold values.
            int valuesCount = 1;
            for (int i = 0; i < shape.Length; i++)
            {
                valuesCount *= shape[i];
            }
            this._values = new double[valuesCount];

            // Compute strides.
            Strides = new int[shape.Length];
            Strides[shape.Length - 1] = 1;
            for (int i = Strides.Length - 2; i >= 0; i--)
            {
                Strides[i] = Strides[i + 1] * Shape[i + 1];
            }
        }
        /// <summary>
        /// Creates a NDArray as a deep copy of an existing NDArray.
        /// </summary>
        /// <param name="source">The NDArray to copy.</param>
        public NDArray(NDArray source)
        { 
            this.Shape = new int[source.Shape.Length];
            source.Shape.CopyTo(this.Shape, 0);
            this._values = new double[source._values.Length];
            source._values.CopyTo(this._values, 0);
            this.Strides = new int[Shape.Length];
            source.Strides.CopyTo(this.Strides, 0);
        }

        private void CheckIndexValid(int[] index)
        {
            if (index.Length != Shape.Length) throw new ArgumentException("Number of dimensions does not match.");

            for (int i = 0; i < index.Length; i++)
            {
                if (index[i] >= Shape[i]) throw new ArgumentException("Index out of range for dimension {0}", i.ToString());
            }

        }

        private int ConvertIndexToArrayIndex(int[] index)
        {
            int arrayIndex = 0;
            for (int i = 0; i < index.Length; i++)
            {
                arrayIndex += index[i] * Strides[i];
            }

            return arrayIndex;
        }

        public double GetValue(int[] index)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);
       
            return _values[arrayIndex];
        }

        public void SetValue(int[] index, double value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            _values[arrayIndex] = value;
        }

        public double GetAndSetValue(int[] index, double value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            double oldValue = _values[arrayIndex];

            _values[arrayIndex] = value;

            return oldValue;
        }

        public void CopyValues(NDArray source)
        {
            if(_values.Length != source._values.Length) throw new ArgumentException("source length does not match.");

            Array.Copy(source._values, _values, _values.Length);
        }

        public void SumWith(NDArray a)
        {
            if (!Shape.SequenceEqual(a.Shape)) throw new ArgumentException("Shapes of arrays do not match.");

            for (int i = 0; i < a._values.Length; i++)
            {
                _values[i] += a._values[i];
            }
        }

        public double GetSum()
        {
            return _values.Sum();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("array(");

            BuildString(0, 0, builder);

            builder.Append(")");

            return builder.ToString();
        }

        private void BuildString(int dim, int currentIndex, StringBuilder builder)
        {
            if(dim == Rank - 1)
            {
                builder.Append("[");
                for (int i = currentIndex; i <currentIndex + Shape[Rank - 1]; i++)
                {
                    builder.Append(_values[i].ToString() + ", ");
                }
                builder.Append("]");
            }
            else
            {
                builder.Append("[");
                while(currentIndex < _values.Length)
                {
                    BuildString(dim + 1, currentIndex, builder);
                    currentIndex += Strides[dim];
                }
                builder.Append("],");
            }
        }


    }
}
