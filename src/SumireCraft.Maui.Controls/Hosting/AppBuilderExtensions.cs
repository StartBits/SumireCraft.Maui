namespace SumireCraft.Maui.Controls;

/// <summary>
/// Extension methods that register SumireCraft.Maui.Controls with a <see cref="MauiAppBuilder"/>.
/// </summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Initializes the controls and handlers of SumireCraft.Maui.Controls.
    /// </summary>
    /// <param name="builder">The <see cref="MauiAppBuilder"/> to initialize.</param>
    /// <param name="configure">
    /// Changes the defaults of SumireCraft.Maui.Core before anything is registered. Applications
    /// with no opinion pass nothing.
    /// </param>
    /// <returns>The same <see cref="MauiAppBuilder"/> instance, so calls can be chained.</returns>
    /// <remarks>
    /// <para>
    /// This method also initializes SumireCraft.Maui.Core by calling
    /// <c>UseSumireCraftCore()</c>, so there is no need to call both.
    /// </para>
    /// <para>
    /// Only the first call on a builder, whichever of the two it is, counts. A later call with no
    /// <paramref name="configure"/> does nothing; one that passes it throws, because its options
    /// could only be dropped.
    /// </para>
    /// <para>
    /// No control in this package needs registering yet, so for now this method does no more than
    /// <c>UseSumireCraftCore()</c>. Call it all the same: a control built on a platform handler
    /// registers that handler here, so an application that calls this method keeps working as
    /// controls of that kind are added.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <see cref="SumireCraftOptions.BusyTimeout"/> is set to zero or a negative value.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="configure"/> is passed, and this method or <c>UseSumireCraftCore()</c> has
    /// already been called on <paramref name="builder"/>.
    /// </exception>
    public static MauiAppBuilder UseSumireCraft(
        this MauiAppBuilder builder,
        Action<SumireCraftOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseSumireCraftCore(configure);

        return builder;
    }
}
