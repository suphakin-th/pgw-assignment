using Reconciliation.Recon;

if (!TryParseArgs(args, out var pathA, out var pathB, out var outDir))
{
    Console.Error.WriteLine("usage: reconcile --a <listA.csv> --b <listB.csv> [--out <dir>]");
    return 2;
}

if (!File.Exists(pathA))
{
    Console.Error.WriteLine($"list A not found: {pathA}");
    return 2;
}
if (!File.Exists(pathB))
{
    Console.Error.WriteLine($"list B not found: {pathB}");
    return 2;
}

try
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var engine = new ReconEngine(pathA, pathB, outDir, msg => Console.Error.WriteLine($"[warn] {msg}"));
    var stats = engine.Run();
    sw.Stop();

    Console.WriteLine("reconciliation done");
    Console.WriteLine($"  list A rows        : {stats.ARows} (invalid {stats.AInvalid})");
    Console.WriteLine($"  list B rows        : {stats.BRows} (invalid {stats.BInvalid})");
    Console.WriteLine($"  matched            : {stats.Matched}");
    Console.WriteLine($"  missing in B (A\\B) : {stats.MissingInB}");
    Console.WriteLine($"  missing in A (B\\A) : {stats.MissingInA}");
    Console.WriteLine($"  output dir         : {Path.GetFullPath(outDir)}");
    Console.WriteLine($"  elapsed            : {sw.ElapsedMilliseconds} ms");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[error] {ex.Message}");
    return 1;
}

static bool TryParseArgs(string[] args, out string pathA, out string pathB, out string outDir)
{
    pathA = string.Empty;
    pathB = string.Empty;
    outDir = "output";

    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--a" when i + 1 < args.Length:
                pathA = args[++i];
                break;
            case "--b" when i + 1 < args.Length:
                pathB = args[++i];
                break;
            case "--out" when i + 1 < args.Length:
                outDir = args[++i];
                break;
        }
    }

    return pathA.Length > 0 && pathB.Length > 0;
}
