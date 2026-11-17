namespace SumireCraft.Maui.Controls.Generators;

/// <summary>
/// A class to give a <c>DateTimeInput</c> to, as plain values, so that the generator writes it again
/// only when one of them changes.
/// </summary>
/// <param name="Namespace">The class's namespace, or <see langword="null"/> for the global one.</param>
/// <param name="Name">The class's name.</param>
/// <param name="Property">The picker's date or time property.</param>
/// <param name="InputProperty">The input's property in the same type, such as <c>DateAsDateTime</c>.</param>
/// <param name="Part">What the property holds, <c>date</c> or <c>time</c>, for the documentation.</param>
internal sealed record PickerTarget(string? Namespace, string Name, string Property, string InputProperty, string Part);
