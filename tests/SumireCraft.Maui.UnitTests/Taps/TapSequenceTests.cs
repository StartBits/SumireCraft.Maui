using System.Collections;
using System.Collections.Concurrent;
using Microsoft.Extensions.Time.Testing;
using SumireCraft.Maui.Taps;

namespace SumireCraft.Maui.UnitTests.Taps;

public class TapSequenceTests
{
    private const int Interval = 300;

    private sealed record Trigger(int Count, TapMatch Match) : ITapTrigger;

    [Fact]
    public void ALoneAtLeastOneTrigger_WaitsForTheTappingToStopLikeEverythingElse()
    {
        var trigger = new Trigger(1, TapMatch.AtLeast);
        var (sequence, fired, time) = Watch(trigger);

        sequence.Tap();

        // No exception is made for the simplest configuration there is. Somewhere that only ever
        // wants one tap wants a gesture recognizer, not something that counts taps.
        Assert.Empty(fired);

        Advance(time);

        Assert.Same(trigger, Assert.Single(fired).Trigger);
    }

    [Fact]
    public void ALoneExactlyOneTrigger_IsDisqualifiedByASecondTap()
    {
        var (sequence, fired, time) = Watch(new Trigger(1, TapMatch.Exactly));

        sequence.Tap();
        sequence.Tap();
        Advance(time);

        // Exactly means exactly, here as everywhere else.
        Assert.Empty(fired);
    }

    [Fact]
    public void ALoneAtLeastTwoTrigger_WaitsForTheTappingToStop()
    {
        var trigger = new Trigger(2, TapMatch.AtLeast);
        var (sequence, fired, time) = Watch(trigger);

        sequence.Tap();
        sequence.Tap();
        Assert.Empty(fired);

        Advance(time);

        Assert.Same(trigger, Assert.Single(fired).Trigger);
    }

    [Fact]
    public void ALoneExactlyTwoTrigger_WaitsToSeeWhetherAThirdTapArrives()
    {
        var trigger = new Trigger(2, TapMatch.Exactly);
        var (sequence, fired, time) = Watch(trigger);

        sequence.Tap();
        sequence.Tap();
        Assert.Empty(fired);

        Advance(time);

        Assert.Same(trigger, Assert.Single(fired).Trigger);
    }

    [Fact]
    public void AThirdTap_DisqualifiesAnExactlyTwoTrigger()
    {
        var (sequence, fired, time) = Watch(new Trigger(2, TapMatch.Exactly));

        sequence.Tap();
        sequence.Tap();
        sequence.Tap();
        Advance(time);

        Assert.Empty(fired);
    }

    [Fact]
    public void TheTriggerAskingForTheMostTaps_Wins()
    {
        var navigate = new Trigger(1, TapMatch.AtLeast);
        var debugRoom = new Trigger(5, TapMatch.AtLeast);
        var (sequence, fired, time) = Watch(navigate, debugRoom);

        Tap(sequence, 5);
        Advance(time);

        // The whole reason a sequence waits: five taps must not also navigate.
        Assert.Same(debugRoom, Assert.Single(fired).Trigger);
    }

    [Fact]
    public void TappingThatCarriesOnPastTheHighestCount_StillAnswersOnlyOnce()
    {
        var navigate = new Trigger(1, TapMatch.AtLeast);
        var debugRoom = new Trigger(5, TapMatch.AtLeast);
        var (sequence, fired, time) = Watch(navigate, debugRoom);

        Tap(sequence, 12);

        // Twelve taps and nothing has happened yet. Answering the fifth on the spot would have
        // opened the debug room under someone who was still tapping, and answering the tenth
        // would have opened it again.
        Assert.Empty(fired);

        Advance(time);

        Assert.Same(debugRoom, Assert.Single(fired).Trigger);
    }

    [Fact]
    public void TheTopTrigger_WaitsWhenItMatchesExactly()
    {
        var debugRoom = new Trigger(5, TapMatch.Exactly);
        var (sequence, fired, time) = Watch(new Trigger(1, TapMatch.AtLeast), debugRoom);

        Tap(sequence, 5);
        Assert.Empty(fired);

        Advance(time);

        Assert.Same(debugRoom, Assert.Single(fired).Trigger);
    }

    [Fact]
    public void TheLowerTrigger_WinsWhenTheBurstStopsShort()
    {
        var navigate = new Trigger(1, TapMatch.AtLeast);
        var (sequence, fired, time) = Watch(navigate, new Trigger(5, TapMatch.AtLeast));

        Tap(sequence, 3);
        Assert.Empty(fired);

        Advance(time);

        var settled = Assert.Single(fired);
        Assert.Same(navigate, settled.Trigger);
        Assert.Equal(3, settled.TapCount);
    }

    [Fact]
    public void NothingWins_WhenAnExactTriggerIsOvershotAndNoOtherMatches()
    {
        var (sequence, fired, time) = Watch(
            new Trigger(1, TapMatch.Exactly),
            new Trigger(5, TapMatch.AtLeast));

        Tap(sequence, 3);
        Advance(time);

        Assert.Empty(fired);
    }

    [Fact]
    public void TheFirstTap_WaitsWhileAHigherTriggerIsStillPossible()
    {
        var navigate = new Trigger(1, TapMatch.AtLeast);
        var (sequence, fired, time) = Watch(navigate, new Trigger(5, TapMatch.AtLeast));

        sequence.Tap();
        Assert.Empty(fired);

        Advance(time);

        Assert.Same(navigate, Assert.Single(fired).Trigger);
    }

    [Fact]
    public void AnExactTrigger_BeatsAnAtLeastTriggerAskingForTheSameCount()
    {
        var exact = new Trigger(2, TapMatch.Exactly);
        var (sequence, fired, time) = Watch(exact, new Trigger(2, TapMatch.AtLeast));

        sequence.Tap();
        sequence.Tap();
        Advance(time);

        Assert.Same(exact, Assert.Single(fired).Trigger);
    }

    [Fact]
    public void TheAtLeastTrigger_TakesOverOnceTheExactCountIsPassed()
    {
        var atLeast = new Trigger(2, TapMatch.AtLeast);
        var (sequence, fired, time) = Watch(new Trigger(2, TapMatch.Exactly), atLeast);

        Tap(sequence, 3);
        Advance(time);

        // The exact trigger is out of the running, and the one left still matches.
        Assert.Same(atLeast, Assert.Single(fired).Trigger);
    }

    [Fact]
    public void ATapWithinTheInterval_StartsTheWaitAgain()
    {
        var trigger = new Trigger(2, TapMatch.Exactly);
        var (sequence, fired, time) = Watch(trigger);

        sequence.Tap();
        time.Advance(TimeSpan.FromMilliseconds(Interval - 1));
        sequence.Tap();
        time.Advance(TimeSpan.FromMilliseconds(Interval - 1));

        // Nearly twice the interval has passed, but never a whole one without a tap.
        Assert.Empty(fired);

        time.Advance(TimeSpan.FromMilliseconds(1));

        Assert.Same(trigger, Assert.Single(fired).Trigger);
    }

    [Fact]
    public void ASequenceThatHasSettled_StartsCountingFromZeroAgain()
    {
        var (sequence, fired, time) = Watch(new Trigger(1, TapMatch.AtLeast));

        sequence.Tap();
        Advance(time);
        sequence.Tap();
        Advance(time);

        Assert.Equal(2, fired.Count);
        Assert.All(fired, settled => Assert.Equal(1, settled.TapCount));
    }

    [Fact]
    public void TapsThatArriveAfterTheIntervalHasPassed_AreANewSequence()
    {
        var (sequence, fired, time) = Watch(new Trigger(2, TapMatch.Exactly));

        sequence.Tap();
        Advance(time);
        sequence.Tap();
        sequence.Tap();
        Advance(time);

        // The first tap settled on nothing rather than counting towards the pair that followed.
        Assert.Equal(2, Assert.Single(fired).TapCount);
    }

    [Fact]
    public void TheReportedTapCount_IsWhatArrivedNotWhatTheTriggerAskedFor()
    {
        var (sequence, fired, time) = Watch(
            new Trigger(2, TapMatch.AtLeast),
            new Trigger(9, TapMatch.Exactly));

        Tap(sequence, 3);
        Advance(time);

        Assert.Equal(3, Assert.Single(fired).TapCount);
    }

    [Fact]
    public void NoTriggersAtAll_MeanNothingEverFires()
    {
        var (sequence, fired, time) = Watch();

        sequence.Tap();
        Advance(time);

        Assert.Empty(fired);
    }

    [Fact]
    public void ATriggerAskingForFewerThanOneTap_IsIgnored()
    {
        var (sequence, fired, time) = Watch(new Trigger(0, TapMatch.AtLeast));

        sequence.Tap();
        Advance(time);

        Assert.Empty(fired);
    }

    // The lines belong to whoever taps -- a user interface, usually, which may be changing them --
    // while the clock settles a sequence on a thread of its own. So they are read as each tap
    // arrives, on the thread that tapped, and the clock works from what was read then.

    [Fact]
    public void TheLines_AreReadOnTheThreadThatTaps_AndNeverByTheClock()
    {
        var trigger = new WatchedTrigger(2, TapMatch.AtLeast);
        var lines = new WatchedList(trigger);
        var time = new FakeTimeProvider();
        var sequence = new TapSequence(lines, Interval, time);
        var fired = new List<TapTriggeredEventArgs>();
        sequence.Triggered += (_, e) => fired.Add(e);
        var tapping = Environment.CurrentManagedThreadId;

        sequence.Tap();
        sequence.Tap();
        OnAnotherThread(() => Advance(time));

        Assert.Same(trigger, Assert.Single(fired).Trigger);
        Assert.All(lines.ReadOn, thread => Assert.Equal(tapping, thread));
        Assert.All(trigger.ReadOn, thread => Assert.Equal(tapping, thread));
    }

    [Fact]
    public void ALineAddedDuringABurst_CountsForIt()
    {
        var lines = new List<ITapTrigger>();
        var (sequence, fired, time) = Watch(lines);

        sequence.Tap();
        var added = new Trigger(1, TapMatch.AtLeast);
        lines.Add(added);
        sequence.Tap();
        Advance(time);

        Assert.Same(added, Assert.Single(fired).Trigger);
    }

    [Fact]
    public void ALineAddedAfterTheLastTap_CountsFromTheNextBurst()
    {
        var lines = new List<ITapTrigger>();
        var (sequence, fired, time) = Watch(lines);

        sequence.Tap();
        lines.Add(new Trigger(1, TapMatch.AtLeast));
        Advance(time);

        // The burst is read as it stood at its last tap.
        Assert.Empty(fired);

        sequence.Tap();
        Advance(time);

        Assert.Single(fired);
    }

    [Fact]
    public void AfterDispose_APendingSequenceNeverSettles()
    {
        var (sequence, fired, time) = Watch(new Trigger(2, TapMatch.Exactly));

        sequence.Tap();
        sequence.Tap();
        sequence.Dispose();
        Advance(time);

        Assert.Empty(fired);
    }

    [Fact]
    public void AfterDispose_FurtherTapsAreIgnored()
    {
        var (sequence, fired, time) = Watch(new Trigger(1, TapMatch.AtLeast));

        sequence.Dispose();
        sequence.Tap();
        Advance(time);

        Assert.Empty(fired);
    }

    [Fact]
    public void DisposingTwice_IsHarmless()
    {
        var (sequence, _, _) = Watch(new Trigger(1, TapMatch.AtLeast));

        sequence.Dispose();
        sequence.Dispose();
    }

    [Fact]
    public void TheConstructor_ChecksItsArguments()
    {
        var time = new FakeTimeProvider();
        IReadOnlyList<ITapTrigger> triggers = [new Trigger(1, TapMatch.AtLeast)];

        Assert.Throws<ArgumentNullException>(() => new TapSequence(null!, Interval, time));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TapSequence(triggers, 0, time));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TapSequence(triggers, -1, time));
    }

    private static (TapSequence Sequence, List<TapTriggeredEventArgs> Fired, FakeTimeProvider Time)
        Watch(params ITapTrigger[] triggers) => Watch((IReadOnlyList<ITapTrigger>)triggers);

    private static (TapSequence Sequence, List<TapTriggeredEventArgs> Fired, FakeTimeProvider Time)
        Watch(IReadOnlyList<ITapTrigger> triggers)
    {
        var time = new FakeTimeProvider();

        // Deliberately never disposed: each test owns its sequence, and the tests that care about
        // disposal do it themselves.
        var sequence = new TapSequence(triggers, Interval, time);
        var fired = new List<TapTriggeredEventArgs>();
        sequence.Triggered += (_, e) => fired.Add(e);

        return (sequence, fired, time);
    }

    private static void Tap(TapSequence sequence, int times)
    {
        for (var i = 0; i < times; i++)
        {
            sequence.Tap();
        }
    }

    private static void Advance(FakeTimeProvider time) =>
        time.Advance(TimeSpan.FromMilliseconds(Interval));

    /// <summary>
    /// Runs <paramref name="action"/> on a thread of its own and waits for it, the way a real clock
    /// would settle a sequence away from the thread that tapped.
    /// </summary>
    private static void OnAnotherThread(Action action)
    {
        var thread = new Thread(() => action());
        thread.Start();
        thread.Join();
    }

    /// <summary>
    /// A trigger that notes every thread its count or match is read on.
    /// </summary>
    private sealed class WatchedTrigger(int count, TapMatch match) : ITapTrigger
    {
        public ConcurrentQueue<int> ReadOn { get; } = new();

        public int Count
        {
            get
            {
                ReadOn.Enqueue(Environment.CurrentManagedThreadId);
                return count;
            }
        }

        public TapMatch Match
        {
            get
            {
                ReadOn.Enqueue(Environment.CurrentManagedThreadId);
                return match;
            }
        }
    }

    /// <summary>
    /// A list of lines that notes every thread it is read on.
    /// </summary>
    private sealed class WatchedList(params ITapTrigger[] items) : IReadOnlyList<ITapTrigger>
    {
        public ConcurrentQueue<int> ReadOn { get; } = new();

        public int Count => Note(items.Length);

        public ITapTrigger this[int index] => Note(items[index]);

        public IEnumerator<ITapTrigger> GetEnumerator() =>
            Note((IEnumerable<ITapTrigger>)items).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private T Note<T>(T value)
        {
            ReadOn.Enqueue(Environment.CurrentManagedThreadId);
            return value;
        }
    }
}
