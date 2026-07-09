#pragma warning disable IDE0130
namespace CKL.Libs.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// Represents a single entry in the propagation chain of a result.
/// Populated automatically — manual construction is not intended.
/// </summary>
public sealed record CallerInfo
{
    /// <summary>
    /// Method name.
    /// </summary>
    public string MethodName { get; init; } = "";

    /// <summary>
    /// Name of the declaring type.
    /// </summary>
    public string ClassName { get; init; } = "";

    /// <summary>
    /// Path to the source file. Empty if no PDB file is available.
    /// </summary>
    public string FilePath { get; init; } = "";

    /// <summary>
    /// Line number. Zero if no PDB file is available.
    /// </summary>
    public int LineNumber { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        if (string.IsNullOrEmpty(FilePath) || LineNumber == 0) return $"{ClassName}.{MethodName}()";
        return $"{ClassName}.{MethodName}() in {GetFileName(FilePath)}:{LineNumber}";
    }

    /// <summary>
    /// Extracts the filename from a path, independent of which OS produced it.
    /// <see cref="Path.GetFileName(string)"/> only recognizes the current OS's
    /// separator, so a Windows-style path (backslashes) survives unchanged when
    /// this runs on Linux/macOS, and vice versa — <c>FilePath</c> is a
    /// compile-time <c>[CallerFilePath]</c> value baked in by whichever machine
    /// built the assembly, not necessarily the machine running it.
    /// </summary>
    private static string GetFileName(string path)
    {
        var lastSeparator = path.LastIndexOfAny(['\\', '/']);
        return lastSeparator < 0 ? path : path[(lastSeparator + 1)..];
    }
}
