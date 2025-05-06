using System.Text;

namespace KawsayApiMockup.Scheduling;

public class SchedulingMatrix
{
    private readonly int[,] _matrix;


    public SchedulingMatrix(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _matrix = new int[Rows, Columns];
    }

    public int Rows { get; }
    public int Columns { get; }


    public int Get(int row, int column)
    {
        if (row < 0 || row >= Rows || column < 0 || column >= Columns) return 1;
        return _matrix[row, column];
    }


    public void Set(int row, int column, int value)
    {
        if (row < 0 || row >= Rows || column < 0 || column >= Columns) return;
        _matrix[row, column] = value;
    }


    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < Rows; i++)
        {
            for (var j = 0; j < Columns; j++) sb.Append(_matrix[i, j] + (j == Columns - 1 ? "" : " "));

            sb.AppendLine();
        }

        return sb.ToString();
    }
}