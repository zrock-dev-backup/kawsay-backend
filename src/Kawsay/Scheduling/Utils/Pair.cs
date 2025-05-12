namespace kawsay.Scheduling.Utils;

public class Pair(int a, int b)
{
    private readonly int _a = a;
    private readonly int _b = b;

    public override bool Equals(object? obj)
    {
        return obj is Pair other && Equals(other);
    }

    private bool Equals(Pair other)
    {
        return _a == other._a && _b == other._b;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_a, _b);
    }
}