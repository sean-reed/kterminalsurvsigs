

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace KTerminalNetworkBDD
{

				/// <summary>
    /// Numpy style multidimensional array.
    /// </summary>
    public class NDArrayInt32
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

        private Int32[] _values; // Values stored in the array.

        public static NDArrayInt32 FromValues(Int32[] values)
        {
            int[] shape = new int[] { values.Length };
            NDArrayInt32 array = new NDArrayInt32(shape);
            for (int i = 0; i < values.Length; i++)
            {
                array._values[i] = values[i];
            }

            return array;
        }

        public static NDArrayInt32 FromValues(Int32[,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
                
            int[] shape = new int[] {dim0Length, dim1Length};
            NDArrayInt32 array = new NDArrayInt32(shape);

            for (int i = 0; i < dim0Length; i++)
            {
                for (int j = 0; j < dim1Length; j++)
                {
                    array._values[i * array.Strides[0] + j * array.Strides[1]] = values[i, j];
                }
            }

            return array;
        }

        public static NDArrayInt32 FromValues(Int32[,,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
            int dim2Length = values.GetLength(2);

            int[] shape = new int[] { dim0Length, dim1Length, dim2Length };
            NDArrayInt32 array = new NDArrayInt32(shape);

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

        public static NDArrayInt32 Sum(NDArrayInt32 a, NDArrayInt32 b)
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

            NDArrayInt32 summed = new NDArrayInt32(a.Shape);
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
        public static NDArrayInt32 ShiftOne(NDArrayInt32 a, int dim)
        {
            if (dim < 0 || dim > a.Rank) throw new ArgumentOutOfRangeException("dim");

            NDArrayInt32 result = new NDArrayInt32(a.Shape);

            int dimShape = a.Shape[dim];
            int shiftStride = a.Strides[dim];
            int consecutiveShifts = shiftStride * (dimShape - 1); // Number of consecutive elements in _values that are shifted.
            int consecutiveSameDimIndex = shiftStride * dimShape; // Number of consecutive elements in _values that have the same index in dimension dim.

            for (int i = 0; i < a._values.Length; i += consecutiveSameDimIndex) // Jump to first element at next index in dimension dim at each iteration.
            {
                for (int j = i; j < consecutiveShifts + i; j++) // Copy the shifted values from the next index in dimension dim to the result array.
                {
                    result._values[j + shiftStride] = a._values[j];
                }
            }

            return result;
        }

        /// <summary>
        /// Non-rotational shift.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static NDArrayInt32 Shift(NDArrayInt32 a, int[] shift)
        {
            if (shift.Length != a.Shape.Length) throw new ArgumentException("Dimensions of shift do not match those of a.");

            // Initialise shifted as a zeroed signature of same shape as a.
            NDArrayInt32 result = new NDArrayInt32(a.Shape);

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

        public static bool ArrayEqual(NDArrayInt32 a, NDArrayInt32 b)
        {
            return a.Shape.SequenceEqual(b.Shape) && a._values.SequenceEqual(b._values);
        }

        public NDArrayInt32(int[] shape)
        {
            this.Shape = shape;

            // Create array to hold values.
            int valuesCount = 1;
            for (int i = 0; i < shape.Length; i++)
            {
                valuesCount *= shape[i];
            }
            this._values = new Int32[valuesCount];

            // Compute strides.
            Strides = new int[shape.Length];
            Strides[shape.Length - 1] = 1;
            for (int i = Strides.Length - 2; i >= 0; i--)
            {
                Strides[i] = Strides[i + 1] * Shape[i + 1];
            }
        }
        /// <summary>
        /// Creates a NDArrayInt32 as a deep copy of an existing NDArrayInt32.
        /// </summary>
        /// <param name="source">The NDArray to copy.</param>
        public NDArrayInt32(NDArrayInt32 source)
        { 
            this.Shape = new int[source.Shape.Length];
            source.Shape.CopyTo(this.Shape, 0);
            this._values = new Int32[source._values.Length];
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

        public Int32 GetValue(int[] index)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);
       
            return _values[arrayIndex];
        }

        public void SetValue(int[] index, Int32 value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            _values[arrayIndex] = value;
        }

        public Int32 GetAndSetValue(int[] index, Int32 value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            Int32 oldValue = _values[arrayIndex];

            _values[arrayIndex] = value;

            return oldValue;
        }

        public void SumWith(NDArrayInt32 a)
        {
            if (!Shape.SequenceEqual(a.Shape)) throw new ArgumentException("Shapes of arrays do not match.");

            for (int i = 0; i < a._values.Length; i++)
            {
                _values[i] += a._values[i];
            }
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
			/// <summary>
    /// Numpy style multidimensional array.
    /// </summary>
    public class NDArrayInt64
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

        private Int64[] _values; // Values stored in the array.

        public static NDArrayInt64 FromValues(Int64[] values)
        {
            int[] shape = new int[] { values.Length };
            NDArrayInt64 array = new NDArrayInt64(shape);
            for (int i = 0; i < values.Length; i++)
            {
                array._values[i] = values[i];
            }

            return array;
        }

        public static NDArrayInt64 FromValues(Int64[,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
                
            int[] shape = new int[] {dim0Length, dim1Length};
            NDArrayInt64 array = new NDArrayInt64(shape);

            for (int i = 0; i < dim0Length; i++)
            {
                for (int j = 0; j < dim1Length; j++)
                {
                    array._values[i * array.Strides[0] + j * array.Strides[1]] = values[i, j];
                }
            }

            return array;
        }

        public static NDArrayInt64 FromValues(Int64[,,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
            int dim2Length = values.GetLength(2);

            int[] shape = new int[] { dim0Length, dim1Length, dim2Length };
            NDArrayInt64 array = new NDArrayInt64(shape);

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

        public static NDArrayInt64 Sum(NDArrayInt64 a, NDArrayInt64 b)
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

            NDArrayInt64 summed = new NDArrayInt64(a.Shape);
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
        public static NDArrayInt64 ShiftOne(NDArrayInt64 a, int dim)
        {
            if (dim < 0 || dim > a.Rank) throw new ArgumentOutOfRangeException("dim");

            NDArrayInt64 result = new NDArrayInt64(a.Shape);

            int dimShape = a.Shape[dim];
            int shiftStride = a.Strides[dim];
            int consecutiveShifts = shiftStride * (dimShape - 1); // Number of consecutive elements in _values that are shifted.
            int consecutiveSameDimIndex = shiftStride * dimShape; // Number of consecutive elements in _values that have the same index in dimension dim.

            for (int i = 0; i < a._values.Length; i += consecutiveSameDimIndex) // Jump to first element at next index in dimension dim at each iteration.
            {
                for (int j = i; j < consecutiveShifts + i; j++) // Copy the shifted values from the next index in dimension dim to the result array.
                {
                    result._values[j + shiftStride] = a._values[j];
                }
            }

            return result;
        }

        /// <summary>
        /// Non-rotational shift.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static NDArrayInt64 Shift(NDArrayInt64 a, int[] shift)
        {
            if (shift.Length != a.Shape.Length) throw new ArgumentException("Dimensions of shift do not match those of a.");

            // Initialise shifted as a zeroed signature of same shape as a.
            NDArrayInt64 result = new NDArrayInt64(a.Shape);

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

        public static bool ArrayEqual(NDArrayInt64 a, NDArrayInt64 b)
        {
            return a.Shape.SequenceEqual(b.Shape) && a._values.SequenceEqual(b._values);
        }

        public NDArrayInt64(int[] shape)
        {
            this.Shape = shape;

            // Create array to hold values.
            int valuesCount = 1;
            for (int i = 0; i < shape.Length; i++)
            {
                valuesCount *= shape[i];
            }
            this._values = new Int64[valuesCount];

            // Compute strides.
            Strides = new int[shape.Length];
            Strides[shape.Length - 1] = 1;
            for (int i = Strides.Length - 2; i >= 0; i--)
            {
                Strides[i] = Strides[i + 1] * Shape[i + 1];
            }
        }
        /// <summary>
        /// Creates a NDArrayInt64 as a deep copy of an existing NDArrayInt64.
        /// </summary>
        /// <param name="source">The NDArray to copy.</param>
        public NDArrayInt64(NDArrayInt64 source)
        { 
            this.Shape = new int[source.Shape.Length];
            source.Shape.CopyTo(this.Shape, 0);
            this._values = new Int64[source._values.Length];
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

        public Int64 GetValue(int[] index)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);
       
            return _values[arrayIndex];
        }

        public void SetValue(int[] index, Int64 value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            _values[arrayIndex] = value;
        }

        public Int64 GetAndSetValue(int[] index, Int64 value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            Int64 oldValue = _values[arrayIndex];

            _values[arrayIndex] = value;

            return oldValue;
        }

        public void SumWith(NDArrayInt64 a)
        {
            if (!Shape.SequenceEqual(a.Shape)) throw new ArgumentException("Shapes of arrays do not match.");

            for (int i = 0; i < a._values.Length; i++)
            {
                _values[i] += a._values[i];
            }
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
			/// <summary>
    /// Numpy style multidimensional array.
    /// </summary>
    public class NDArrayUInt32
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

        private UInt32[] _values; // Values stored in the array.

        public static NDArrayUInt32 FromValues(UInt32[] values)
        {
            int[] shape = new int[] { values.Length };
            NDArrayUInt32 array = new NDArrayUInt32(shape);
            for (int i = 0; i < values.Length; i++)
            {
                array._values[i] = values[i];
            }

            return array;
        }

        public static NDArrayUInt32 FromValues(UInt32[,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
                
            int[] shape = new int[] {dim0Length, dim1Length};
            NDArrayUInt32 array = new NDArrayUInt32(shape);

            for (int i = 0; i < dim0Length; i++)
            {
                for (int j = 0; j < dim1Length; j++)
                {
                    array._values[i * array.Strides[0] + j * array.Strides[1]] = values[i, j];
                }
            }

            return array;
        }

        public static NDArrayUInt32 FromValues(UInt32[,,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
            int dim2Length = values.GetLength(2);

            int[] shape = new int[] { dim0Length, dim1Length, dim2Length };
            NDArrayUInt32 array = new NDArrayUInt32(shape);

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

        public static NDArrayUInt32 Sum(NDArrayUInt32 a, NDArrayUInt32 b)
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

            NDArrayUInt32 summed = new NDArrayUInt32(a.Shape);
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
        public static NDArrayUInt32 ShiftOne(NDArrayUInt32 a, int dim)
        {
            if (dim < 0 || dim > a.Rank) throw new ArgumentOutOfRangeException("dim");

            NDArrayUInt32 result = new NDArrayUInt32(a.Shape);

            int dimShape = a.Shape[dim];
            int shiftStride = a.Strides[dim];
            int consecutiveShifts = shiftStride * (dimShape - 1); // Number of consecutive elements in _values that are shifted.
            int consecutiveSameDimIndex = shiftStride * dimShape; // Number of consecutive elements in _values that have the same index in dimension dim.

            for (int i = 0; i < a._values.Length; i += consecutiveSameDimIndex) // Jump to first element at next index in dimension dim at each iteration.
            {
                for (int j = i; j < consecutiveShifts + i; j++) // Copy the shifted values from the next index in dimension dim to the result array.
                {
                    result._values[j + shiftStride] = a._values[j];
                }
            }

            return result;
        }

        /// <summary>
        /// Non-rotational shift.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static NDArrayUInt32 Shift(NDArrayUInt32 a, int[] shift)
        {
            if (shift.Length != a.Shape.Length) throw new ArgumentException("Dimensions of shift do not match those of a.");

            // Initialise shifted as a zeroed signature of same shape as a.
            NDArrayUInt32 result = new NDArrayUInt32(a.Shape);

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

        public static bool ArrayEqual(NDArrayUInt32 a, NDArrayUInt32 b)
        {
            return a.Shape.SequenceEqual(b.Shape) && a._values.SequenceEqual(b._values);
        }

        public NDArrayUInt32(int[] shape)
        {
            this.Shape = shape;

            // Create array to hold values.
            int valuesCount = 1;
            for (int i = 0; i < shape.Length; i++)
            {
                valuesCount *= shape[i];
            }
            this._values = new UInt32[valuesCount];

            // Compute strides.
            Strides = new int[shape.Length];
            Strides[shape.Length - 1] = 1;
            for (int i = Strides.Length - 2; i >= 0; i--)
            {
                Strides[i] = Strides[i + 1] * Shape[i + 1];
            }
        }
        /// <summary>
        /// Creates a NDArrayUInt32 as a deep copy of an existing NDArrayUInt32.
        /// </summary>
        /// <param name="source">The NDArray to copy.</param>
        public NDArrayUInt32(NDArrayUInt32 source)
        { 
            this.Shape = new int[source.Shape.Length];
            source.Shape.CopyTo(this.Shape, 0);
            this._values = new UInt32[source._values.Length];
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

        public UInt32 GetValue(int[] index)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);
       
            return _values[arrayIndex];
        }

        public void SetValue(int[] index, UInt32 value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            _values[arrayIndex] = value;
        }

        public UInt32 GetAndSetValue(int[] index, UInt32 value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            UInt32 oldValue = _values[arrayIndex];

            _values[arrayIndex] = value;

            return oldValue;
        }

        public void SumWith(NDArrayUInt32 a)
        {
            if (!Shape.SequenceEqual(a.Shape)) throw new ArgumentException("Shapes of arrays do not match.");

            for (int i = 0; i < a._values.Length; i++)
            {
                _values[i] += a._values[i];
            }
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
			/// <summary>
    /// Numpy style multidimensional array.
    /// </summary>
    public class NDArrayUInt64
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

        private UInt64[] _values; // Values stored in the array.

        public static NDArrayUInt64 FromValues(UInt64[] values)
        {
            int[] shape = new int[] { values.Length };
            NDArrayUInt64 array = new NDArrayUInt64(shape);
            for (int i = 0; i < values.Length; i++)
            {
                array._values[i] = values[i];
            }

            return array;
        }

        public static NDArrayUInt64 FromValues(UInt64[,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
                
            int[] shape = new int[] {dim0Length, dim1Length};
            NDArrayUInt64 array = new NDArrayUInt64(shape);

            for (int i = 0; i < dim0Length; i++)
            {
                for (int j = 0; j < dim1Length; j++)
                {
                    array._values[i * array.Strides[0] + j * array.Strides[1]] = values[i, j];
                }
            }

            return array;
        }

        public static NDArrayUInt64 FromValues(UInt64[,,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
            int dim2Length = values.GetLength(2);

            int[] shape = new int[] { dim0Length, dim1Length, dim2Length };
            NDArrayUInt64 array = new NDArrayUInt64(shape);

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

        public static NDArrayUInt64 Sum(NDArrayUInt64 a, NDArrayUInt64 b)
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

            NDArrayUInt64 summed = new NDArrayUInt64(a.Shape);
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
        public static NDArrayUInt64 ShiftOne(NDArrayUInt64 a, int dim)
        {
            if (dim < 0 || dim > a.Rank) throw new ArgumentOutOfRangeException("dim");

            NDArrayUInt64 result = new NDArrayUInt64(a.Shape);

            int dimShape = a.Shape[dim];
            int shiftStride = a.Strides[dim];
            int consecutiveShifts = shiftStride * (dimShape - 1); // Number of consecutive elements in _values that are shifted.
            int consecutiveSameDimIndex = shiftStride * dimShape; // Number of consecutive elements in _values that have the same index in dimension dim.

            for (int i = 0; i < a._values.Length; i += consecutiveSameDimIndex) // Jump to first element at next index in dimension dim at each iteration.
            {
                for (int j = i; j < consecutiveShifts + i; j++) // Copy the shifted values from the next index in dimension dim to the result array.
                {
                    result._values[j + shiftStride] = a._values[j];
                }
            }

            return result;
        }

        /// <summary>
        /// Non-rotational shift.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static NDArrayUInt64 Shift(NDArrayUInt64 a, int[] shift)
        {
            if (shift.Length != a.Shape.Length) throw new ArgumentException("Dimensions of shift do not match those of a.");

            // Initialise shifted as a zeroed signature of same shape as a.
            NDArrayUInt64 result = new NDArrayUInt64(a.Shape);

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

        public static bool ArrayEqual(NDArrayUInt64 a, NDArrayUInt64 b)
        {
            return a.Shape.SequenceEqual(b.Shape) && a._values.SequenceEqual(b._values);
        }

        public NDArrayUInt64(int[] shape)
        {
            this.Shape = shape;

            // Create array to hold values.
            int valuesCount = 1;
            for (int i = 0; i < shape.Length; i++)
            {
                valuesCount *= shape[i];
            }
            this._values = new UInt64[valuesCount];

            // Compute strides.
            Strides = new int[shape.Length];
            Strides[shape.Length - 1] = 1;
            for (int i = Strides.Length - 2; i >= 0; i--)
            {
                Strides[i] = Strides[i + 1] * Shape[i + 1];
            }
        }
        /// <summary>
        /// Creates a NDArrayUInt64 as a deep copy of an existing NDArrayUInt64.
        /// </summary>
        /// <param name="source">The NDArray to copy.</param>
        public NDArrayUInt64(NDArrayUInt64 source)
        { 
            this.Shape = new int[source.Shape.Length];
            source.Shape.CopyTo(this.Shape, 0);
            this._values = new UInt64[source._values.Length];
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

        public UInt64 GetValue(int[] index)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);
       
            return _values[arrayIndex];
        }

        public void SetValue(int[] index, UInt64 value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            _values[arrayIndex] = value;
        }

        public UInt64 GetAndSetValue(int[] index, UInt64 value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            UInt64 oldValue = _values[arrayIndex];

            _values[arrayIndex] = value;

            return oldValue;
        }

        public void SumWith(NDArrayUInt64 a)
        {
            if (!Shape.SequenceEqual(a.Shape)) throw new ArgumentException("Shapes of arrays do not match.");

            for (int i = 0; i < a._values.Length; i++)
            {
                _values[i] += a._values[i];
            }
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
			/// <summary>
    /// Numpy style multidimensional array.
    /// </summary>
    public class NDArraySingle
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

        private Single[] _values; // Values stored in the array.

        public static NDArraySingle FromValues(Single[] values)
        {
            int[] shape = new int[] { values.Length };
            NDArraySingle array = new NDArraySingle(shape);
            for (int i = 0; i < values.Length; i++)
            {
                array._values[i] = values[i];
            }

            return array;
        }

        public static NDArraySingle FromValues(Single[,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
                
            int[] shape = new int[] {dim0Length, dim1Length};
            NDArraySingle array = new NDArraySingle(shape);

            for (int i = 0; i < dim0Length; i++)
            {
                for (int j = 0; j < dim1Length; j++)
                {
                    array._values[i * array.Strides[0] + j * array.Strides[1]] = values[i, j];
                }
            }

            return array;
        }

        public static NDArraySingle FromValues(Single[,,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
            int dim2Length = values.GetLength(2);

            int[] shape = new int[] { dim0Length, dim1Length, dim2Length };
            NDArraySingle array = new NDArraySingle(shape);

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

        public static NDArraySingle Sum(NDArraySingle a, NDArraySingle b)
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

            NDArraySingle summed = new NDArraySingle(a.Shape);
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
        public static NDArraySingle ShiftOne(NDArraySingle a, int dim)
        {
            if (dim < 0 || dim > a.Rank) throw new ArgumentOutOfRangeException("dim");

            NDArraySingle result = new NDArraySingle(a.Shape);

            int dimShape = a.Shape[dim];
            int shiftStride = a.Strides[dim];
            int consecutiveShifts = shiftStride * (dimShape - 1); // Number of consecutive elements in _values that are shifted.
            int consecutiveSameDimIndex = shiftStride * dimShape; // Number of consecutive elements in _values that have the same index in dimension dim.

            for (int i = 0; i < a._values.Length; i += consecutiveSameDimIndex) // Jump to first element at next index in dimension dim at each iteration.
            {
                for (int j = i; j < consecutiveShifts + i; j++) // Copy the shifted values from the next index in dimension dim to the result array.
                {
                    result._values[j + shiftStride] = a._values[j];
                }
            }

            return result;
        }

        /// <summary>
        /// Non-rotational shift.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static NDArraySingle Shift(NDArraySingle a, int[] shift)
        {
            if (shift.Length != a.Shape.Length) throw new ArgumentException("Dimensions of shift do not match those of a.");

            // Initialise shifted as a zeroed signature of same shape as a.
            NDArraySingle result = new NDArraySingle(a.Shape);

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

        public static bool ArrayEqual(NDArraySingle a, NDArraySingle b)
        {
            return a.Shape.SequenceEqual(b.Shape) && a._values.SequenceEqual(b._values);
        }

        public NDArraySingle(int[] shape)
        {
            this.Shape = shape;

            // Create array to hold values.
            int valuesCount = 1;
            for (int i = 0; i < shape.Length; i++)
            {
                valuesCount *= shape[i];
            }
            this._values = new Single[valuesCount];

            // Compute strides.
            Strides = new int[shape.Length];
            Strides[shape.Length - 1] = 1;
            for (int i = Strides.Length - 2; i >= 0; i--)
            {
                Strides[i] = Strides[i + 1] * Shape[i + 1];
            }
        }
        /// <summary>
        /// Creates a NDArraySingle as a deep copy of an existing NDArraySingle.
        /// </summary>
        /// <param name="source">The NDArray to copy.</param>
        public NDArraySingle(NDArraySingle source)
        { 
            this.Shape = new int[source.Shape.Length];
            source.Shape.CopyTo(this.Shape, 0);
            this._values = new Single[source._values.Length];
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

        public Single GetValue(int[] index)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);
       
            return _values[arrayIndex];
        }

        public void SetValue(int[] index, Single value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            _values[arrayIndex] = value;
        }

        public Single GetAndSetValue(int[] index, Single value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            Single oldValue = _values[arrayIndex];

            _values[arrayIndex] = value;

            return oldValue;
        }

        public void SumWith(NDArraySingle a)
        {
            if (!Shape.SequenceEqual(a.Shape)) throw new ArgumentException("Shapes of arrays do not match.");

            for (int i = 0; i < a._values.Length; i++)
            {
                _values[i] += a._values[i];
            }
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
			/// <summary>
    /// Numpy style multidimensional array.
    /// </summary>
    public class NDArrayDouble
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

        private Double[] _values; // Values stored in the array.

        public static NDArrayDouble FromValues(Double[] values)
        {
            int[] shape = new int[] { values.Length };
            NDArrayDouble array = new NDArrayDouble(shape);
            for (int i = 0; i < values.Length; i++)
            {
                array._values[i] = values[i];
            }

            return array;
        }

        public static NDArrayDouble FromValues(Double[,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
                
            int[] shape = new int[] {dim0Length, dim1Length};
            NDArrayDouble array = new NDArrayDouble(shape);

            for (int i = 0; i < dim0Length; i++)
            {
                for (int j = 0; j < dim1Length; j++)
                {
                    array._values[i * array.Strides[0] + j * array.Strides[1]] = values[i, j];
                }
            }

            return array;
        }

        public static NDArrayDouble FromValues(Double[,,] values)
        {
            int dim0Length = values.GetLength(0);
            int dim1Length = values.GetLength(1);
            int dim2Length = values.GetLength(2);

            int[] shape = new int[] { dim0Length, dim1Length, dim2Length };
            NDArrayDouble array = new NDArrayDouble(shape);

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

        public static NDArrayDouble Sum(NDArrayDouble a, NDArrayDouble b)
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

            NDArrayDouble summed = new NDArrayDouble(a.Shape);
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
        public static NDArrayDouble ShiftOne(NDArrayDouble a, int dim)
        {
            if (dim < 0 || dim > a.Rank) throw new ArgumentOutOfRangeException("dim");

            NDArrayDouble result = new NDArrayDouble(a.Shape);

            int dimShape = a.Shape[dim];
            int shiftStride = a.Strides[dim];
            int consecutiveShifts = shiftStride * (dimShape - 1); // Number of consecutive elements in _values that are shifted.
            int consecutiveSameDimIndex = shiftStride * dimShape; // Number of consecutive elements in _values that have the same index in dimension dim.

            for (int i = 0; i < a._values.Length; i += consecutiveSameDimIndex) // Jump to first element at next index in dimension dim at each iteration.
            {
                for (int j = i; j < consecutiveShifts + i; j++) // Copy the shifted values from the next index in dimension dim to the result array.
                {
                    result._values[j + shiftStride] = a._values[j];
                }
            }

            return result;
        }

        /// <summary>
        /// Non-rotational shift.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static NDArrayDouble Shift(NDArrayDouble a, int[] shift)
        {
            if (shift.Length != a.Shape.Length) throw new ArgumentException("Dimensions of shift do not match those of a.");

            // Initialise shifted as a zeroed signature of same shape as a.
            NDArrayDouble result = new NDArrayDouble(a.Shape);

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

        public static bool ArrayEqual(NDArrayDouble a, NDArrayDouble b)
        {
            return a.Shape.SequenceEqual(b.Shape) && a._values.SequenceEqual(b._values);
        }

        public NDArrayDouble(int[] shape)
        {
            this.Shape = shape;

            // Create array to hold values.
            int valuesCount = 1;
            for (int i = 0; i < shape.Length; i++)
            {
                valuesCount *= shape[i];
            }
            this._values = new Double[valuesCount];

            // Compute strides.
            Strides = new int[shape.Length];
            Strides[shape.Length - 1] = 1;
            for (int i = Strides.Length - 2; i >= 0; i--)
            {
                Strides[i] = Strides[i + 1] * Shape[i + 1];
            }
        }
        /// <summary>
        /// Creates a NDArrayDouble as a deep copy of an existing NDArrayDouble.
        /// </summary>
        /// <param name="source">The NDArray to copy.</param>
        public NDArrayDouble(NDArrayDouble source)
        { 
            this.Shape = new int[source.Shape.Length];
            source.Shape.CopyTo(this.Shape, 0);
            this._values = new Double[source._values.Length];
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

        public Double GetValue(int[] index)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);
       
            return _values[arrayIndex];
        }

        public void SetValue(int[] index, Double value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            _values[arrayIndex] = value;
        }

        public Double GetAndSetValue(int[] index, Double value)
        {
            CheckIndexValid(index);

            int arrayIndex = ConvertIndexToArrayIndex(index);

            Double oldValue = _values[arrayIndex];

            _values[arrayIndex] = value;

            return oldValue;
        }

        public void SumWith(NDArrayDouble a)
        {
            if (!Shape.SequenceEqual(a.Shape)) throw new ArgumentException("Shapes of arrays do not match.");

            for (int i = 0; i < a._values.Length; i++)
            {
                _values[i] += a._values[i];
            }
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
	