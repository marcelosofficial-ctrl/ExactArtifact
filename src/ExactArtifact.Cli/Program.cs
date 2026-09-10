using ExactArtifact.Core;

if (args.Length == 0)
{
    ShowHelp();
    return 1;
}

try
{
    return args[0].ToLowerInvariant() switch
    {
        "hash" => await HashFileAsync(args),
        "verify" => await VerifyFileAsync(args),
        "manifest" => await ManifestAsync(args),
        "--help" or "-h" or "help" => ShowHelpAndReturn(),
        _ => UnknownCommand(args[0])
    };
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Operation cancelled.");
    return 2;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}

static async Task<int> HashFileAsync(string[] args)
{
    if (args.Length != 2)
    {
        Console.Error.WriteLine(
            "Usage: exactartifact hash <file>");
        return 1;
    }

    var hasher = new FileHasher();
    var hash = await hasher.ComputeSha256Async(args[1]);

    Console.WriteLine(hash);
    return 0;
}

static async Task<int> VerifyFileAsync(string[] args)
{
    if (args.Length != 3)
    {
        Console.Error.WriteLine(
            "Usage: exactartifact verify <file> <expected-sha256>");
        return 1;
    }

    var hasher = new FileHasher();
    var calculatedHash = await hasher.ComputeSha256Async(args[1]);
    var matches = HashVerifier.IsMatch(
        calculatedHash,
        args[2]);

    Console.WriteLine(
        $"File:     {Path.GetFileName(args[1])}");
    Console.WriteLine(
        $"SHA-256:  {calculatedHash}");
    Console.WriteLine();

    if (matches)
    {
        Console.WriteLine("MATCH");
        return 0;
    }

    Console.WriteLine("MISMATCH");
    Console.WriteLine(
        $"Expected: {HashVerifier.NormalizeSha256(args[2])}");
    return 3;
}

static async Task<int> ManifestAsync(string[] args)
{
    if (args.Length != 4)
    {
        Console.Error.WriteLine(
            "Usage: exactartifact manifest <create|verify> <directory> <manifest-file>");
        return 1;
    }

    var service = new ManifestService();
    var action = args[1].ToLowerInvariant();

    if (action == "create")
    {
        var manifest = await service.CreateAsync(
            args[2],
            args[3]);

        Console.WriteLine(
            $"Manifest: {Path.GetFullPath(args[3])}");
        Console.WriteLine(
            $"Files:    {manifest.Files.Count}");
        Console.WriteLine("CREATED");
        return 0;
    }

    if (action == "verify")
    {
        var result = await service.VerifyAsync(
            args[2],
            args[3]);

        Console.WriteLine(
            $"Expected:   {result.ExpectedCount}");
        Console.WriteLine(
            $"Matched:    {result.MatchedCount}");
        Console.WriteLine(
            $"Modified:   {result.Modified.Count}");
        Console.WriteLine(
            $"Missing:    {result.Missing.Count}");
        Console.WriteLine(
            $"Unexpected: {result.Unexpected.Count}");
        Console.WriteLine();

        if (result.IsExactMatch)
        {
            Console.WriteLine("EXACT MATCH");
            return 0;
        }

        foreach (var path in result.Modified)
        {
            Console.WriteLine($"MODIFIED   {path}");
        }

        foreach (var path in result.Missing)
        {
            Console.WriteLine($"MISSING    {path}");
        }

        foreach (var path in result.Unexpected)
        {
            Console.WriteLine($"UNEXPECTED {path}");
        }

        Console.WriteLine("MISMATCH");
        return 4;
    }

    Console.Error.WriteLine(
        $"Unknown manifest action: {args[1]}");
    return 1;
}

static int ShowHelpAndReturn()
{
    ShowHelp();
    return 0;
}

static int UnknownCommand(string command)
{
    Console.Error.WriteLine(
        $"Unknown command: {command}");
    Console.Error.WriteLine();
    ShowHelp();
    return 1;
}

static void ShowHelp()
{
    Console.WriteLine(
        """
        ExactArtifact

        Prove that files and release directories contain exactly the expected bytes.

        Usage:
          exactartifact hash <file>
          exactartifact verify <file> <expected-sha256>
          exactartifact manifest create <directory> <manifest-file>
          exactartifact manifest verify <directory> <manifest-file>

        Exit codes:
          0  Success / exact match
          1  Invalid input or operational error
          2  Cancelled
          3  Single-file hash mismatch
          4  Manifest mismatch
        """);
}