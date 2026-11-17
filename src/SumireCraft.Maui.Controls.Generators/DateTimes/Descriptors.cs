using Microsoft.CodeAnalysis;

namespace SumireCraft.Maui.Controls.Generators;

/// <summary>
/// What the generator reports about a class it cannot give a <c>DateTimeInput</c> to. Every one is
/// an error: the class would otherwise build without the property its XAML binds to.
/// </summary>
internal static class Descriptors
{
    private const string Category = "SumireCraft";

    public static readonly DiagnosticDescriptor NotPartial = Error(
        "SMR0001",
        "The class must be partial",
        "'{0}' must be partial for [{1}] to give it a DateTimeInput");

    public static readonly DiagnosticDescriptor NestedOrGeneric = Error(
        "SMR0002",
        "The class must be top-level and not generic",
        "'{0}' must be a top-level class that is not generic for [{1}] to give it a DateTimeInput");

    public static readonly DiagnosticDescriptor NotBindable = Error(
        "SMR0003",
        "The class must derive from BindableObject",
        "'{0}' must derive from BindableObject for [{1}] to give it a DateTimeInput");

    public static readonly DiagnosticDescriptor NoProperty = Error(
        "SMR0004",
        "The property must be one the class can read and write",
        "'{0}' has no property '{1}' that it can both read and write");

    public static readonly DiagnosticDescriptor WrongType = Error(
        "SMR0005",
        "The property's type must be one the attribute takes",
        "'{0}' is a {1}, and [{2}] takes {3}");

    public static readonly DiagnosticDescriptor Both = Error(
        "SMR0006",
        "A class takes one of the two attributes",
        "'{0}' takes [DateTimeInputDate] or [DateTimeInputTime], not both: each gives it the one DateTimeInput");

    public static readonly DiagnosticDescriptor MemberTaken = Error(
        "SMR0007",
        "The class must leave the generator's members to it",
        "'{0}' already has {1}, which [{2}] writes{3}");

    public static readonly DiagnosticDescriptor NoBaseConstructor = Error(
        "SMR0008",
        "The base class must have a constructor without arguments",
        "'{0}' derives from a class without a constructor it can call with no arguments, which the constructor [{1}] writes needs");

    public static readonly DiagnosticDescriptor Unchained = Error(
        "SMR0009",
        "The class's own constructors must start from this()",
        "'{0}' has {1}, which does not start from the constructor [{2}] writes: the picker it makes would neither start empty nor run OnCreated");

    private static DiagnosticDescriptor Error(string id, string title, string message) =>
        new(id, title, message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
}
