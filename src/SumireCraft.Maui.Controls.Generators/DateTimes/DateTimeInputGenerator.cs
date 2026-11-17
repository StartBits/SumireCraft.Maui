using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SumireCraft.Maui.Controls.Generators;

/// <summary>
/// Gives a picker marked with <c>DateTimeInputDate</c> or <c>DateTimeInputTime</c> its
/// <c>DateTimeInput</c>: the bindable property and its field, an override of <c>OnPropertyChanged</c>
/// that writes what is picked into the input, and a constructor that starts the picker empty and
/// then calls <c>partial void OnCreated()</c>.
/// </summary>
/// <remarks>
/// What it writes is what <c>SmrDatePicker</c> and <c>SmrTimePicker</c> are made of, and the unit
/// tests of those two are the tests of what it writes. A class it cannot write for is reported as an
/// error, never passed over: the class would build without the property its XAML binds to, and
/// fail only when the page is shown.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class DateTimeInputGenerator : IIncrementalGenerator
{
    private const string DateAttribute = "SumireCraft.Maui.Controls.DateTimeInputDateAttribute";
    private const string TimeAttribute = "SumireCraft.Maui.Controls.DateTimeInputTimeAttribute";
    private const string BindableObject = "Microsoft.Maui.Controls.BindableObject";
    private const string Hook = "OnCreated";

    // The members the generator writes, but for the constructor and the hook, which are looked for
    // apart. A class that declares one of them already would not build with the generated part beside
    // it, and the compiler's own error would not say why.
    private static readonly string[] WrittenMembers = ["DateTimeInput", "DateTimeInputProperty", "OnPropertyChanged"];

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Register(context, DateAttribute, isDate: true);
        Register(context, TimeAttribute, isDate: false);
    }

    private static void Register(IncrementalGeneratorInitializationContext context, string attribute, bool isDate)
    {
        var outcomes = context.SyntaxProvider.ForAttributeWithMetadataName(
            attribute,
            static (node, _) => node is ClassDeclarationSyntax,
            (attributed, _) => Analyze(attributed, isDate));

        context.RegisterSourceOutput(outcomes, static (output, outcome) =>
        {
            if (outcome.Problem is { } problem)
            {
                output.ReportDiagnostic(problem.ToDiagnostic());
            }

            if (outcome.Target is { } target)
            {
                output.AddSource(HintName(target), Write(target));
            }
        });
    }

    private static (PickerTarget? Target, Problem? Problem) Analyze(GeneratorAttributeSyntaxContext attributed, bool isDate)
    {
        var type = (INamedTypeSymbol)attributed.TargetSymbol;
        var syntax = (ClassDeclarationSyntax)attributed.TargetNode;
        var location = syntax.Identifier.GetLocation();
        var attributeName = isDate ? "DateTimeInputDate" : "DateTimeInputTime";

        // Both attributes on one class: reported once, by the date's side.
        var other = isDate ? TimeAttribute : DateAttribute;
        if (type.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == other))
        {
            return (null, isDate ? Problem.At(Descriptors.Both, location, type.Name) : null);
        }

        if (!syntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return (null, Problem.At(Descriptors.NotPartial, location, type.Name, attributeName));
        }

        if (type.ContainingType is not null || type.IsGenericType)
        {
            return (null, Problem.At(Descriptors.NestedOrGeneric, location, type.Name, attributeName));
        }

        if (!DerivesFrom(type, BindableObject))
        {
            return (null, Problem.At(Descriptors.NotBindable, location, type.Name, attributeName));
        }

        var compilation = attributed.SemanticModel.Compilation;
        if (Taken(type, compilation) is { } taken)
        {
            return (null, Problem.At(Descriptors.MemberTaken, location, type.Name, taken.Member, attributeName, taken.Hint));
        }

        if (Unchained(type) is { } unchained)
        {
            return (null, Problem.At(Descriptors.Unchained, location, type.Name, unchained, attributeName));
        }

        // The generated constructor calls the base class's constructor with no arguments, which one
        // whose arguments are all optional takes too.
        if (type.BaseType is not { } baseType
            || !baseType.InstanceConstructors.Any(c => c.Parameters.All(p => p.IsOptional) && compilation.IsSymbolAccessibleWithin(c, type)))
        {
            return (null, Problem.At(Descriptors.NoBaseConstructor, location, type.Name, attributeName));
        }

        var arguments = attributed.Attributes[0].ConstructorArguments;
        var propertyName = arguments.Length == 1 && arguments[0].Value is string named ? named : string.Empty;
        if (FindProperty(type, propertyName, compilation) is not { } property)
        {
            return (null, Problem.At(Descriptors.NoProperty, location, type.Name, propertyName));
        }

        if (InputProperty(property.Type, isDate) is not { } inputProperty)
        {
            return (null, Problem.At(
                Descriptors.WrongType,
                location,
                property.Name,
                property.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                attributeName,
                isDate ? "DateTime? or DateOnly?" : "TimeSpan?, TimeOnly? or DateTime?"));
        }

        var ns = type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace.ToDisplayString();
        return (new PickerTarget(ns, type.Name, property.Name, inputProperty, isDate ? "date" : "time"), null);
    }

    // A member that the generated part would collide with or hide, as the message names it, and what
    // to do instead. The hook is the class's to implement, so a partial method of that name is what
    // is wanted; one it inherits would be hidden by the generated declaration.
    private static (string Member, string Hint)? Taken(INamedTypeSymbol type, Compilation compilation)
    {
        if (WrittenMembers.FirstOrDefault(name => !type.GetMembers(name).IsEmpty) is { } member)
        {
            return ("'" + member + "'", string.Empty);
        }

        if (type.InstanceConstructors.Any(c => c.Parameters.IsEmpty && !c.IsImplicitlyDeclared))
        {
            return ("a constructor without arguments", "; what it does goes in partial void OnCreated() instead");
        }

        if (type.GetMembers(Hook).Any(m => m is not IMethodSymbol { ReturnsVoid: true, Parameters.IsEmpty: true } || !IsPartial(m)))
        {
            return ("'" + Hook + "' other than as partial void " + Hook + "()", string.Empty);
        }

        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.GetMembers(Hook).Any(m => compilation.IsSymbolAccessibleWithin(m, type)))
            {
                return ("'" + Hook + "' from '" + current.Name + "'", ", and would hide it");
            }
        }

        return null;
    }

    // A constructor of the class's own that does not start from the generated one, as the message
    // names it. A primary constructor cannot: every other constructor would have to call it instead.
    private static string? Unchained(INamedTypeSymbol type)
    {
        foreach (var constructor in type.InstanceConstructors.Where(c => !c.Parameters.IsEmpty && !c.IsImplicitlyDeclared))
        {
            foreach (var reference in constructor.DeclaringSyntaxReferences)
            {
                switch (reference.GetSyntax())
                {
                    case TypeDeclarationSyntax:
                        return "a primary constructor";
                    case ConstructorDeclarationSyntax { Initializer: not null } declared
                        when declared.Initializer.IsKind(SyntaxKind.ThisConstructorInitializer):
                        break;
                    default:
                        return "a constructor that does not call this()";
                }
            }
        }

        return null;
    }

    private static bool IsPartial(ISymbol method) =>
        method.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is MethodDeclarationSyntax m && m.Modifiers.Any(SyntaxKind.PartialKeyword));

    private static bool DerivesFrom(INamedTypeSymbol type, string baseName)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == baseName)
            {
                return true;
            }
        }

        return false;
    }

    // The property the class reads and writes under that name, its own or inherited. Not one set
    // only while initializing an object, which the generated code could not write to; and not one
    // whose name C# cannot spell, as a library in another language might give a property.
    private static IPropertySymbol? FindProperty(INamedTypeSymbol type, string name, Compilation compilation)
    {
        if (!SyntaxFacts.IsValidIdentifier(name))
        {
            return null;
        }

        for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            foreach (var property in current.GetMembers(name).OfType<IPropertySymbol>())
            {
                if (!property.IsStatic
                    && property.GetMethod is { } getter && compilation.IsSymbolAccessibleWithin(getter, type)
                    && property.SetMethod is { IsInitOnly: false } setter && compilation.IsSymbolAccessibleWithin(setter, type))
                {
                    return property;
                }
            }
        }

        return null;
    }

    // The input's property that holds the same part in the same type. Only a type that can be empty:
    // an empty input would have nothing to write into any other.
    private static string? InputProperty(ITypeSymbol type, bool isDate)
    {
        if (type is not INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            return null;
        }

        return (nullable.TypeArguments[0].ToDisplayString(), isDate) switch
        {
            ("System.DateTime", true) => "DateAsDateTime",
            ("System.DateOnly", true) => "DateAsDateOnly",
            ("System.TimeSpan", false) => "TimeAsTimeSpan",
            ("System.TimeOnly", false) => "TimeAsTimeOnly",
            ("System.DateTime", false) => "TimeAsDateTime",
            _ => null,
        };
    }

    private static string HintName(PickerTarget target) =>
        (target.Namespace is null ? target.Name : target.Namespace + "." + target.Name) + ".DateTimeInput.g.cs";

    // A name as code spells it: a keyword such as event is written @event.
    private static string Code(string name) =>
        SyntaxFacts.GetKeywordKind(name) == SyntaxKind.None && SyntaxFacts.GetContextualKeywordKind(name) == SyntaxKind.None
            ? name
            : "@" + name;

    private static string Write(PickerTarget target)
    {
        var name = Code(target.Name);
        var ns = target.Namespace is null ? null : string.Join(".", target.Namespace.Split('.').Select(Code));
        var type = ns is null ? "global::" + name : "global::" + ns + "." + name;
        var property = Code(target.Property);

        var body = $$"""
            partial class {{name}}
            {
                /// <summary>
                /// Backs the <see cref="DateTimeInput"/> property.
                /// </summary>
                public static readonly global::Microsoft.Maui.Controls.BindableProperty DateTimeInputProperty =
                    global::Microsoft.Maui.Controls.BindableProperty.Create(
                        "DateTimeInput",
                        typeof(global::SumireCraft.DateTimes.DateTimeInput),
                        typeof({{type}}),
                        defaultBindingMode: global::Microsoft.Maui.Controls.BindingMode.TwoWay,
                        propertyChanged: static (bindable, _, newValue) =>
                        {
                            var picker = ({{type}})bindable;
                            if (newValue is not global::SumireCraft.DateTimes.DateTimeInput input)
                            {
                                // Set as any other value is, so that a binding carries the new input back to its source.
                                picker.DateTimeInput = new global::SumireCraft.DateTimes.DateTimeInput();
                                return;
                            }

                            picker.{{property}} = input.{{target.InputProperty}};
                        },
                        defaultValueCreator: static _ => new global::SumireCraft.DateTimes.DateTimeInput());

                /// <summary>
                /// Initializes a new instance of the <see cref="{{name}}"/> class, empty as its input is.
                /// </summary>
                public {{name}}()
                {
                    // A picker starts at a {{target.Part}} of its own choosing, which the input does not hold, and
                    // which picked again unchanged would never reach it. So it starts as its input does.
                    this.{{property}} = null;
                    this.OnCreated();
                }

                /// <summary>
                /// Gets or sets the input whose {{target.Part}} the picker reads and writes. Never <see langword="null"/>:
                /// writing <see langword="null"/> gives the picker a new, empty input.
                /// </summary>
                public global::SumireCraft.DateTimes.DateTimeInput DateTimeInput
                {
                    get => (global::SumireCraft.DateTimes.DateTimeInput)this.GetValue(DateTimeInputProperty);
                    set => this.SetValue(DateTimeInputProperty, value);
                }

                /// <inheritdoc/>
                protected override void OnPropertyChanged([global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
                {
                    base.OnPropertyChanged(propertyName);

                    if (propertyName == "{{target.Property}}")
                    {
                        this.DateTimeInput.{{target.InputProperty}} = this.{{property}};
                    }
                }

                // Called by the constructor once the picker is empty. Implement it for what every picker of
                // this kind should start with; left unimplemented, it and the call to it are compiled away.
                partial void OnCreated();
            }
            """;

        var header = """
            // <auto-generated/>
            // Written by SumireCraft.Maui.Controls.Generators.DateTimeInputGenerator.
            #nullable enable

            """;

        return ns is null
            ? header + body + "\n"
            : header + "namespace " + ns + "\n{\n" + Indent(body) + "\n}\n";
    }

    private static string Indent(string text) =>
        string.Join("\n", text.Split('\n').Select(line => line.Length == 0 ? line : "    " + line));
}
