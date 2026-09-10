using ExactArtifact.Core;

if (args.Length == 0)
{
    ShowHelp();
    return 1;
}

var command = args[0].ToLowerInvariant();

try
{
    switch (command)
    {
        case "hash":
            return await HashFileAsync(args);

        case "verify":
            return await VerifyFileAsync(args);

        case "--help":
        case "-h":
        case "help":
            ShowHelp();
            return 0;

        default:
            Console.Error.WriteLine($"Unknown command: {command}");
            Console.Error.WriteLine();
            ShowHelp();
            return 1;
    }
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
        Console.Error.WriteLine("Usage: exactartifact hash <file>");
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
    var matches = HashVerifier.IsMatch(calculatedHash, args[2]);

    Console.WriteLine($"File:     {Path.GetFileName(args[1])}");
    Console.WriteLine($"SHA-256:  {calculatedHash}");
    Console.WriteLine();

    if (matches)
    {
        Console.WriteLine("MATCH");
        return 0;
    }

    Console.WriteLine("MISMATCH");
    Console.WriteLine($"Expected: {args[2]}");
    return 3;
}

static void ShowHelp()
{
    Console.WriteLine(
        """
        ExactArtifact

        Verify that the bytes you have are exactly the bytes you expected.

        Usage:
          exactartifact hash <file>
          exactartifact verify <file> <expected-sha256>

        Commands:
          hash      Calculate a file's SHA-256 hash
          verify    Compare a file against an expected SHA-256 hash
        """);
}