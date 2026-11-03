namespace SumireCraft.Maui.Exceptions;

/// <summary>
/// Hands the application's <see cref="IExceptionSink"/> to <see cref="SumireCraftExceptions"/> once
/// the container that holds it exists.
/// </summary>
/// <param name="sink">The sink resolved from the application's own container.</param>
/// <remarks>
/// <para>
/// <c>UseSumireCraftCore()</c> runs before there is a container to resolve anything from, so the
/// static entry point cannot be connected there. .NET MAUI calls
/// <see cref="IMauiInitializeService"/> once during <c>MauiAppBuilder.Build()</c>, with the root
/// service provider, which is exactly the moment that is needed.
/// </para>
/// <para>
/// Resolving through the container matters. Building a second, throwaway provider inside
/// <c>UseSumireCraftCore()</c> would be simpler and wrong twice over: the sink it produced would be
/// a different instance from the one the application injects, and it would miss every handler
/// registered after that call.
/// </para>
/// </remarks>
internal sealed class ExceptionSinkBridge(IExceptionSink sink) : IMauiInitializeService
{
    /// <inheritdoc />
    public void Initialize(IServiceProvider services) => SumireCraftExceptions.Use(sink);
}
