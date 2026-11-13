namespace Bloom.Domain.Exercises;

/// <summary>
/// The muscles an exercise can list: body areas, each with the finer muscles in it.
/// An exercise lists an area on its own ("Back": the area in general) or muscles in it
/// ("Lats"); a listed muscle implies its area. Custom exercises may also use names
/// outside the tree, which then stand as an area of their own.
/// </summary>
public static class MuscleTree
{
    public static readonly IReadOnlyList<(string Area, string[] Muscles)> Areas =
    [
        ("Chest", []),
        ("Back", ["Lats", "Traps", "Mid back", "Lower back"]),
        ("Shoulders", ["Front delts", "Side delts", "Rear delts"]),
        ("Arms", ["Biceps", "Triceps", "Forearms"]),
        ("Core", ["Abs", "Obliques"]),
        ("Legs", ["Quads", "Hamstrings", "Glutes", "Adductors", "Abductors", "Calves"]),
    ];

    /// <summary>Older or alternative names, mapped onto the tree.</summary>
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Quadriceps"] = "Quads",
        ["Upper chest"] = "Chest",
        ["Lower chest"] = "Chest",
        ["Pecs"] = "Chest",
        ["Upper back"] = "Mid back",
        ["Rhomboids"] = "Mid back",
        ["Trapezius"] = "Traps",
        ["Hamstring"] = "Hamstrings",
        ["Abdominals"] = "Abs",
        ["Calf"] = "Calves",
        ["Bicep"] = "Biceps",
        ["Tricep"] = "Triceps",
        ["Forearm"] = "Forearms",
        ["Glute"] = "Glutes",
    };

    /// <summary>A whole-body tag stands for every area.</summary>
    private static readonly HashSet<string> WholeBody = new(StringComparer.OrdinalIgnoreCase) { "Full Body", "Full body", "Whole body" };

    private static readonly Dictionary<string, string> AreaByName = BuildIndex();

    private static Dictionary<string, string> BuildIndex()
    {
        var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (area, muscles) in Areas)
        {
            index[area] = area;
            foreach (var m in muscles) index[m] = area;
        }
        return index;
    }

    /// <summary>True for an area or a muscle in the tree (any case, aliases included).</summary>
    public static bool IsKnown(string name) => WholeBody.Contains(name.Trim()) || (Canonical(name) is { } c && AreaByName.ContainsKey(c));

    public static bool IsArea(string name) => Areas.Any(a => a.Area.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>The area a name belongs to; a name outside the tree is its own area.</summary>
    public static string AreaOf(string name)
    {
        var c = Canonical(name) ?? name;
        return AreaByName.TryGetValue(c, out var area) ? area : c;
    }

    /// <summary>The tree's spelling of a name (aliases resolved), or the trimmed name if it isn't in the tree.</summary>
    public static string? Canonical(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.Length == 0) return null;
        if (Aliases.TryGetValue(trimmed, out var alias)) trimmed = alias;
        foreach (var (area, muscles) in Areas)
        {
            if (area.Equals(trimmed, StringComparison.OrdinalIgnoreCase)) return area;
            var m = muscles.FirstOrDefault(x => x.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
            if (m is not null) return m;
        }
        return trimmed;
    }

    /// <summary>
    /// Tidies a muscle list: tree spelling, "Full Body" as every area, no duplicates,
    /// and an area dropped when a muscle in it is listed (the muscle implies it).
    /// </summary>
    public static List<string> Normalize(IEnumerable<string> names)
    {
        var list = new List<string>();
        foreach (var raw in names)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var expanded = WholeBody.Contains(raw.Trim()) ? Areas.Select(a => a.Area) : [Canonical(raw)!];
            foreach (var n in expanded)
                if (!list.Contains(n, StringComparer.OrdinalIgnoreCase)) list.Add(n);
        }
        return list
            .Where(n => !IsArea(n) || !list.Any(other => !IsArea(other) && AreaOf(other) == n))
            .ToList();
    }
}
