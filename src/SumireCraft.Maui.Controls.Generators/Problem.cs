using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SumireCraft.Maui.Controls.Generators;

/// <summary>
/// What is wrong with a class, held as plain values rather than as a <see cref="Diagnostic"/>, whose
/// location keeps the whole syntax tree alive.
/// </summary>
/// <param name="Descriptor">What is reported.</param>
/// <param name="FilePath">The file the class is in.</param>
/// <param name="Span">Where in it.</param>
/// <param name="LineSpan">The same place as lines and columns.</param>
/// <param name="Arguments">The words the message is filled in with, joined by a tab.</param>
internal sealed record Problem(DiagnosticDescriptor Descriptor, string FilePath, TextSpan Span, LinePositionSpan LineSpan, string Arguments)
{
    /// <summary>
    /// Makes a problem reported at the place given.
    /// </summary>
    public static Problem At(DiagnosticDescriptor descriptor, Location location, params string[] arguments)
    {
        var lines = location.GetLineSpan();
        return new Problem(descriptor, lines.Path, location.SourceSpan, lines.Span, string.Join("\t", arguments));
    }

    /// <summary>
    /// Makes the diagnostic to report.
    /// </summary>
    public Diagnostic ToDiagnostic() =>
        Diagnostic.Create(Descriptor, Location.Create(FilePath, Span, LineSpan), Arguments.Split('\t'));
}
