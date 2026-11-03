// The URI an application writes for the widgets in XAML, instead of the clr-namespace form. It is
// an identifier, not a page anyone needs to reach, and once published it cannot change without
// breaking every page that uses it. Every public type in this assembly is in the one namespace,
// so the one mapping reaches them all.
//
// XmlnsPrefix is written in full because Microsoft.Maui.Graphics, imported implicitly too, has an
// attribute of the same name.
[assembly: XmlnsDefinition("https://github.com/startbits/SumireCraft.Maui", "SumireCraft.Maui.Controls")]
[assembly: Microsoft.Maui.Controls.XmlnsPrefix("https://github.com/startbits/SumireCraft.Maui", "smr")]
