// Scheduling/SchedulingMatrix.cs

using System.Text;

namespace KawsayApiMockup.Scheduling
{
    // Represents a matrix (grid) for tracking availability or constraints over days and periods.
    public class SchedulingMatrix
    {
        public int Rows { get; } // Number of days
        public int Columns { get; } // Number of periods
        private readonly int[,] _matrix;

        // Constructor takes dimensions based on the timetable
        public SchedulingMatrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            _matrix = new int[Rows, Columns];
            // Matrix is initialized to all 0s (available) by default
        }

        // Get the value at a specific day and period
        public int Get(int row, int column)
        {
            // Defensive check for out-of-bounds access
            if (row < 0 || row >= Rows || column < 0 || column >= Columns)
            {
                // If access is out of bounds, treat it as unavailable (1)
                // This prevents errors if the algorithm tries to check beyond the grid
                System.Console.WriteLine($"Warning: Attempted to Get matrix out of bounds [{row},{column}]. Matrix size: [{Rows},{Columns}]. Returning 1 (unavailable).");
                return 1;
            }
            return _matrix[row, column];
        }

        // Set the value at a specific day and period
        public void Set(int row, int column, int value)
        {
             // Defensive check for out-of-bounds access
             if (row < 0 || row >= Rows || column < 0 || column >= Columns)
            {
                System.Console.WriteLine($"Warning: Attempted to Set matrix out of bounds [{row},{column}]. Matrix size: [{Rows},{Columns}]. Ignoring set operation.");
                 return;
            }
            _matrix[row, column] = value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    sb.Append(_matrix[i, j] + " ");
                }

                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
