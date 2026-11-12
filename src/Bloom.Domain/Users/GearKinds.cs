namespace Bloom.Domain.Users;

/// <summary>The icons a gear item can have. The frontend draws one per kind.</summary>
public static class GearKinds
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        "shoe", "bike", "belt", "wrap", "watch", "swim", "weights", "racket", "other",
    };
}
