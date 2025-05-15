namespace Application.Features.Scheduling.Models;

public class Pair(int a, int b)
{
    public int A { get; } = a;
    public int B { get; } = b;

    public override bool Equals(object? obj)
    {
        return obj is Pair other && Equals(other);
    }

    private bool Equals(Pair other)
    {
        return A == other.A && B == other.B;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(A, B);
    }
}