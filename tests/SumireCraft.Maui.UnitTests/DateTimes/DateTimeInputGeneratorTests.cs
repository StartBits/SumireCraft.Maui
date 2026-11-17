using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SumireCraft.DateTimes;
using SumireCraft.Maui.Controls;
using SumireCraft.Maui.Controls.Generators;

namespace SumireCraft.Maui.UnitTests.DateTimes;

// What the source generator says about a class it cannot give a DateTimeInput to. Each case is
// compiled here, with the generator run over it, and is read for the generator's errors alone; what
// a well-formed class does when it runs is GeneratedPickerTests'.
public class DateTimeInputGeneratorTests
{
    private const string Usings = """
        using System;
        using Microsoft.Maui.Controls;
        using SumireCraft.Maui.Controls;

        """;

    private const string Vendor = """
        public class VendorPicker : ContentView
        {
            public DateOnly? Day { get; set; }
            public DateTime? Moment { get; set; }
            public TimeSpan? Span { get; set; }
            public DateTime Plain { get; set; }
            public string? Text { get; set; }
            public DateOnly? ReadOnlyDay { get; }
            public DateOnly? PrivateSetDay { get; private set; }
            public DateOnly? InitDay { get; init; }
            public DateOnly? @event { get; set; }
        }

        public class OptionalPicker : ContentView
        {
            public OptionalPicker(int start = 0)
            {
            }

            public DateOnly? Day { get; set; }
        }

        public class HookedPicker : ContentView
        {
            public DateOnly? Day { get; set; }

            protected virtual void OnCreated()
            {
            }
        }

        public class NoDefaultPicker : ContentView
        {
            public NoDefaultPicker(int start)
            {
            }

            public DateOnly? Day { get; set; }
        }

        """;

    [Fact]
    public void AWellFormedClass_IsGeneratedWithoutErrors()
    {
        AssertGenerated("Picker", """
            [DateTimeInputDate("Day")]
            public partial class Picker : VendorPicker;
            """);
    }

    [Fact]
    public void AWellFormedClassInANamespace_IsGeneratedWithoutErrors()
    {
        AssertGenerated("Abc.Pickers.Picker", """
            namespace Abc.Pickers
            {
                [DateTimeInputTime("Span")]
                public sealed partial class Picker : VendorPicker;
            }
            """);
    }

    [Fact]
    public void AClassWithAConstructorThatTakesArguments_IsGeneratedWithoutErrors()
    {
        AssertGenerated("Picker", """
            [DateTimeInputDate("Day")]
            public partial class Picker : VendorPicker
            {
                public Picker(string name)
                    : this()
                {
                }
            }
            """);
    }

    [Fact]
    public void AClassThatImplementsOnCreated_IsGeneratedWithoutErrors()
    {
        AssertGenerated("Picker", """
            [DateTimeInputDate("Day")]
            public partial class Picker : VendorPicker
            {
                partial void OnCreated() => StyleId = "set up";
            }
            """);
    }

    [Fact]
    public void APropertyNamedAsAKeyword_IsGeneratedWithoutErrors()
    {
        AssertGenerated("Picker", """
            [DateTimeInputDate("event")]
            public partial class Picker : VendorPicker;
            """);
    }

    [Fact]
    public void AClassNotPartial_IsReported()
    {
        AssertReported("SMR0001", """
            [DateTimeInputDate("Day")]
            public class Picker : VendorPicker;
            """);
    }

    [Fact]
    public void ANestedClass_IsReported()
    {
        AssertReported("SMR0002", """
            public partial class Outer
            {
                [DateTimeInputDate("Day")]
                public partial class Picker : VendorPicker;
            }
            """);
    }

    [Fact]
    public void AGenericClass_IsReported()
    {
        AssertReported("SMR0002", """
            [DateTimeInputDate("Day")]
            public partial class Picker<T> : VendorPicker;
            """);
    }

    [Fact]
    public void AClassThatIsNotBindable_IsReported()
    {
        AssertReported("SMR0003", """
            [DateTimeInputDate("Day")]
            public partial class Picker;
            """);
    }

    [Theory]
    [InlineData("Missing")]
    [InlineData("ReadOnlyDay")]
    [InlineData("PrivateSetDay")]
    [InlineData("InitDay")]
    public void APropertyThatCannotBeReadAndWritten_IsReported(string property)
    {
        AssertReported("SMR0004", $$"""
            [DateTimeInputDate("{{property}}")]
            public partial class Picker : VendorPicker;
            """);
    }

    [Theory]
    [InlineData("DateTimeInputDate", "Span")]
    [InlineData("DateTimeInputDate", "Plain")]
    [InlineData("DateTimeInputDate", "Text")]
    [InlineData("DateTimeInputTime", "Day")]
    [InlineData("DateTimeInputTime", "Plain")]
    public void APropertyOfATypeThePartCannotTake_IsReported(string attribute, string property)
    {
        AssertReported("SMR0005", $$"""
            [{{attribute}}("{{property}}")]
            public partial class Picker : VendorPicker;
            """);
    }

    [Fact]
    public void AClassWithBothAttributes_IsReportedOnce()
    {
        var (diagnostics, _, _) = Run("""
            [DateTimeInputDate("Day")]
            [DateTimeInputTime("Span")]
            public partial class Picker : VendorPicker;
            """);

        Assert.Equal("SMR0006", Assert.Single(diagnostics).Id);
    }

    [Theory]
    [InlineData("public DateTimeInput2 DateTimeInput { get; set; } = new();")]
    [InlineData("public static readonly BindableProperty DateTimeInputProperty = null!;")]
    [InlineData("protected override void OnPropertyChanged(string? propertyName = null) => base.OnPropertyChanged(propertyName);")]
    [InlineData("public Picker() { }")]
    [InlineData("protected void OnCreated() { }")]
    [InlineData("public int OnCreated { get; set; }")]
    public void AClassThatAlreadyHasAMemberTheGeneratorWrites_IsReported(string member)
    {
        AssertReported("SMR0007", $$"""
            public class DateTimeInput2;

            [DateTimeInputDate("Day")]
            public partial class Picker : VendorPicker
            {
                {{member}}
            }
            """);
    }

    [Fact]
    public void AClassWhoseBaseTakesOnlyOptionalArguments_IsGeneratedWithoutErrors()
    {
        AssertGenerated("Picker", """
            [DateTimeInputDate("Day")]
            public partial class Picker : OptionalPicker;
            """);
    }

    [Fact]
    public void AClassWhoseBaseHasOnCreatedAlready_IsReported()
    {
        AssertReported("SMR0007", """
            [DateTimeInputDate("Day")]
            public partial class Picker : HookedPicker;
            """);
    }

    // A constructor of the class's own that does not start from the generated one would make a
    // picker that is not empty and never ran OnCreated.

    [Fact]
    public void AConstructorThatDoesNotCallThis_IsReported()
    {
        AssertReported("SMR0009", """
            [DateTimeInputDate("Day")]
            public partial class Picker : VendorPicker
            {
                public Picker(string name)
                {
                }
            }
            """);
    }

    [Fact]
    public void APrimaryConstructor_IsReported()
    {
        AssertReported("SMR0009", """
            [DateTimeInputDate("Day")]
            public partial class Picker(string name) : VendorPicker;
            """);
    }

    [Fact]
    public void AClassWhoseBaseHasNoConstructorWithoutArguments_IsReported()
    {
        AssertReported("SMR0008", """
            [DateTimeInputDate("Day")]
            public partial class Picker : NoDefaultPicker;
            """);
    }

    // No error from the generator, none from compiling what it wrote, and what it wrote is there: a
    // generator that wrote nothing would pass the first two.
    private static void AssertGenerated(string typeName, string source)
    {
        var (diagnostics, compiled, compilation) = Run(source);

        Assert.Empty(diagnostics);
        Assert.DoesNotContain(compiled, d => d.Severity == DiagnosticSeverity.Error);

        var type = compilation.GetTypeByMetadataName(typeName);
        Assert.NotNull(type);
        Assert.NotEmpty(type.GetMembers("DateTimeInput"));
        Assert.NotEmpty(type.GetMembers("DateTimeInputProperty"));
        Assert.Contains(type.InstanceConstructors, c => c.Parameters.IsEmpty && !c.IsImplicitlyDeclared);
    }

    private static void AssertReported(string id, string source)
    {
        var (diagnostics, _, _) = Run(source);

        var reported = Assert.Single(diagnostics);
        Assert.Equal(id, reported.Id);
        Assert.Equal(DiagnosticSeverity.Error, reported.Severity);
    }

    // Compiles the source with the vendor picker beside it, runs the generator over it, and returns
    // what the generator reported, what compiling its output reported, and that compilation.
    private static (IReadOnlyList<Diagnostic> Generator, IReadOnlyList<Diagnostic> Compiled, Compilation Output) Run(string source)
    {
        var compilation = CSharpCompilation.Create(
            "Generated",
            [CSharpSyntaxTree.ParseText(Usings + Vendor + source, new CSharpParseOptions(LanguageVersion.Latest), cancellationToken: TestContext.Current.CancellationToken)],
            References.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        CSharpGeneratorDriver.Create(new DateTimeInputGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics, TestContext.Current.CancellationToken);

        return (diagnostics, output.GetDiagnostics(TestContext.Current.CancellationToken), output);
    }

    // The framework the tests run on, and the assemblies a picker is written against.
    private static readonly Lazy<MetadataReference[]> References = new(() =>
    {
        var framework = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        Assembly[] libraries =
        [
            typeof(BindableObject).Assembly,
            typeof(IView).Assembly,
            typeof(SmrDatePicker).Assembly,
            typeof(DateTimeInput).Assembly,
        ];

        return [.. framework.Concat(libraries.Select(a => a.Location)).Distinct().Select(p => MetadataReference.CreateFromFile(p))];
    });
}
