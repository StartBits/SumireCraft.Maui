namespace SumireCraft.Maui.UITests;

/// <summary>
/// Finds the sample application this project drives.
/// </summary>
/// <remarks>
/// The path is worked out rather than configured, so that running the tests takes no setup beyond
/// building the sample. <c>SUMIRE_SAMPLE_WINDOWS</c>, <c>SUMIRE_SAMPLE_ANDROID</c>,
/// <c>SUMIRE_SAMPLE_IOS</c> and <c>SUMIRE_SAMPLE_MACCATALYST</c> point somewhere else.
/// </remarks>
internal static class SampleApp
{
    private const string SolutionFile = "SumireCraft.Maui.slnx";
    private const string Bundle = "SumireCraft.Maui.Sample.app";

    private static readonly string? Root = FindRepositoryRoot();

    // Debug and Consumer, not Release. The sample is built from a project reference in both Debug
    // and Release, so a Release build proves nothing a Debug one does not; Consumer is the one that
    // builds it against the published packages, and that is worth driving.
    private static readonly string[] Configurations = ["Debug", "Consumer"];

    /// <summary>
    /// The most recently built application for a platform, or <see langword="null"/> when there is
    /// none.
    /// </summary>
    public static string? Find(SamplePlatform platform) => platform switch
    {
        SamplePlatform.Windows => Newest("SUMIRE_SAMPLE_WINDOWS", "SumireCraft.Maui.Sample.exe"),
        SamplePlatform.Android => Newest("SUMIRE_SAMPLE_ANDROID", "*-Signed.apk"),
        // A bundle built for a device cannot be installed on a simulator.
        SamplePlatform.Ios => NewestBundle("SUMIRE_SAMPLE_IOS", "-ios", "iossimulator-"),
        SamplePlatform.MacCatalyst => NewestBundle("SUMIRE_SAMPLE_MACCATALYST", "-maccatalyst", ""),
        _ => null,
    };

    public static string BuildHint(SamplePlatform platform) => platform switch
    {
        SamplePlatform.Windows =>
            "The sample has not been built for Windows: " +
            "dotnet build samples/SumireCraft.Maui.Sample -f net10.0-windows10.0.19041.0",
        SamplePlatform.Android =>
            "The sample has not been built for Android: dotnet build " +
            "samples/SumireCraft.Maui.Sample -f net10.0-android -p:EmbedAssembliesIntoApk=true " +
            "(without that switch a Debug build leaves its assemblies out of the package for fast " +
            "deployment to put there, and an installed package alone will not start)",
        SamplePlatform.Ios =>
            "The sample has not been built for the iOS simulator: dotnet build " +
            "samples/SumireCraft.Maui.Sample -f net10.0-ios -p:RuntimeIdentifier=iossimulator-arm64 " +
            "(iossimulator-x64 on an Intel Mac)",
        SamplePlatform.MacCatalyst =>
            "The sample has not been built for Mac Catalyst: dotnet build " +
            "samples/SumireCraft.Maui.Sample -f net10.0-maccatalyst",
        _ => $"Unknown platform {platform}.",
    };

    private static string? Newest(string variable, string pattern)
    {
        var configured = Environment.GetEnvironmentVariable(variable);

        if (!string.IsNullOrEmpty(configured))
        {
            return File.Exists(configured) ? configured : null;
        }

        var bin = Root is null
            ? null
            : Path.Combine(Root, "samples", "SumireCraft.Maui.Sample", "bin");

        if (bin is null || !Directory.Exists(bin))
        {
            return null;
        }

        // Newest wins. Each configuration and target framework leaves something behind, and the
        // one built last is the one whoever is running these tests just changed.
        return Configurations
            .Select(configuration => Path.Combine(bin, configuration))
            .Where(Directory.Exists)
            .SelectMany(directory => Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    /// <summary>
    /// The most recently built application bundle under a target framework that ends as given,
    /// in a runtime identifier's folder that starts as given.
    /// </summary>
    /// <remarks>
    /// A bundle is a directory, and iOS and Mac Catalyst give theirs the same name, so it is the
    /// target framework's folder that tells them apart.
    /// </remarks>
    private static string? NewestBundle(string variable, string platformSuffix, string runtimePrefix)
    {
        var configured = Environment.GetEnvironmentVariable(variable);

        if (!string.IsNullOrEmpty(configured))
        {
            return Directory.Exists(configured) ? configured : null;
        }

        var bin = Root is null
            ? null
            : Path.Combine(Root, "samples", "SumireCraft.Maui.Sample", "bin");

        if (bin is null || !Directory.Exists(bin))
        {
            return null;
        }

        return Configurations
            .Select(configuration => Path.Combine(bin, configuration))
            .Where(Directory.Exists)
            .SelectMany(Directory.EnumerateDirectories)
            .Where(framework => Path.GetFileName(framework).EndsWith(platformSuffix, StringComparison.Ordinal))
            .SelectMany(framework => Directory.EnumerateDirectories(framework, Bundle, SearchOption.AllDirectories))
            .Where(bundle => Path.GetFileName(Path.GetDirectoryName(bundle))!.StartsWith(runtimePrefix, StringComparison.Ordinal))
            .OrderByDescending(BuiltAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// When a bundle was last built.
    /// </summary>
    /// <remarks>
    /// Read from its Info.plist, which every build writes again. The bundle's own directory keeps
    /// the time its entries last changed, so a bundle left behind by an earlier build, such as a
    /// universal one beside the one built for this machine alone, can look newer than it is. iOS
    /// keeps the file at the top of the bundle and Mac Catalyst under Contents.
    /// </remarks>
    private static DateTime BuiltAt(string bundle)
    {
        var info = Path.Combine(bundle, "Info.plist");

        return File.GetLastWriteTimeUtc(File.Exists(info) ? info : Path.Combine(bundle, "Contents", "Info.plist"));
    }

    private static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFile)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
