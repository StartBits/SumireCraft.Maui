using System.ComponentModel;
using System.Runtime.CompilerServices;
using SumireCraft.DateTimes;

namespace SumireCraft.Maui.Controls;

// What joins one picker to its inputs: the input its date goes into and the one its time goes into,
// each written whenever the property that part is held in changes. Kept beside the picker rather than
// in a property of its, so that nothing of the library's shows among the picker's values; the table
// lets the picker go with its page, and the handler reads the picker from the event, so the join holds
// nothing that holds the picker.
internal sealed class DateTimeInputJoin
{
    private static readonly ConditionalWeakTable<BindableObject, DateTimeInputJoin> Joins = [];

    private DateTimeInput? _date;
    private DateTimeInput? _time;

    // The part of an input a picker is joined to.
    internal enum Part
    {
        Date,
        Time,
    }

    // Joins the picker's part to the input given, or unjoins it for null, and shows the input. The
    // picker is listened to from its first join on: unjoined, it has no input to write to.
    public static void Join(BindableObject picker, Part part, DateTimeInput? input)
    {
        var join = Joins.GetValue(picker, static first =>
        {
            var created = new DateTimeInputJoin();
            first.PropertyChanged += created.OnPropertyChanged;
            return created;
        });

        if (part == Part.Date)
        {
            join._date = input;
        }
        else
        {
            join._time = input;
        }

        join.Show(picker, part);
    }

    // Shows the input again, in the property the part is now held in.
    public static void Retarget(BindableObject picker, Part part)
    {
        if (Joins.TryGetValue(picker, out var join))
        {
            join.Show(picker, part);
        }
    }

    private DateTimeInput? InputOf(Part part) => part == Part.Date ? _date : _time;

    private void Show(BindableObject picker, Part part)
    {
        if (InputOf(part) is not { } input
            || TargetOf(picker, part) is not { } target
            || Accessor.For(part, target.ReturnType) is not { } accessor)
        {
            return;
        }

        // A picker may move the value, as a date picker does with a date outside its range, and what it
        // shows goes back into the input. Read back rather than awaited as a change: a value moved to
        // where the picker already was raises none.
        picker.SetValue(target, accessor.Read(input));
        accessor.Write(input, picker.GetValue(target));
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var picker = (BindableObject)sender!;
        Write(picker, Part.Date, e.PropertyName);
        Write(picker, Part.Time, e.PropertyName);
    }

    private void Write(BindableObject picker, Part part, string? propertyName)
    {
        if (InputOf(part) is not { } input
            || TargetOf(picker, part) is not { } target
            || target.PropertyName != propertyName
            || Accessor.For(part, target.ReturnType) is not { } accessor)
        {
            return;
        }

        accessor.Write(input, picker.GetValue(target));
    }

    private static BindableProperty? TargetOf(BindableObject picker, Part part)
    {
        var date = SmrDateTimeInput.GetDateTarget(picker) ?? (picker is DatePicker ? DatePicker.DateProperty : null);
        if (part == Part.Date)
        {
            return date;
        }

        // A property holds one part. One given for both is the date's: as the time, it would carry a
        // date of its own into the input's date whenever it changed.
        var time = SmrDateTimeInput.GetTimeTarget(picker) ?? (picker is TimePicker ? TimePicker.TimeProperty : null);
        return time == date ? null : time;
    }

    // Reads and writes one part of an input in the type of the property it is held in. Only a type
    // that can be empty has one: a picker whose date cannot be empty cannot show an empty input.
    private sealed class Accessor(Func<DateTimeInput, object?> read, Action<DateTimeInput, object?> write)
    {
        private static readonly Accessor DateAsDateTime = new(static i => i.DateAsDateTime, static (i, v) => i.DateAsDateTime = (DateTime?)v);
        private static readonly Accessor DateAsDateOnly = new(static i => i.DateAsDateOnly, static (i, v) => i.DateAsDateOnly = (DateOnly?)v);
        private static readonly Accessor TimeAsTimeSpan = new(static i => i.TimeAsTimeSpan, static (i, v) => i.TimeAsTimeSpan = (TimeSpan?)v);
        private static readonly Accessor TimeAsTimeOnly = new(static i => i.TimeAsTimeOnly, static (i, v) => i.TimeAsTimeOnly = (TimeOnly?)v);
        private static readonly Accessor TimeAsDateTime = new(static i => i.TimeAsDateTime, static (i, v) => i.TimeAsDateTime = (DateTime?)v);

        public static Accessor? For(Part part, Type type)
        {
            if (part == Part.Date)
            {
                return type == typeof(DateTime?) ? DateAsDateTime
                    : type == typeof(DateOnly?) ? DateAsDateOnly
                    : null;
            }

            return type == typeof(TimeSpan?) ? TimeAsTimeSpan
                : type == typeof(TimeOnly?) ? TimeAsTimeOnly
                : type == typeof(DateTime?) ? TimeAsDateTime
                : null;
        }

        public object? Read(DateTimeInput input) => read(input);

        public void Write(DateTimeInput input, object? value) => write(input, value);
    }
}
