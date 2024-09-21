using SuperToolsTests.Core;
using System.Diagnostics;
using Xunit.Abstractions;
using SuperTools.Timers;

namespace SuperToolsTests.Timers
{
    public class SuperAccurateTimerTest : TestBase
    {
        public SuperAccurateTimerTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(DisplayName = "1ms 20 times")]
        [Trait("SuperAccurateTimer", "")]
        public void Should_1MillisecondAccurate_When_1MillisecondInterval()
        {
            int intervalMs = 1;
            int expectedHits = 20;

            StartSuperAccurateTimer(intervalMs, expectedHits);
        }

        [Fact(DisplayName = "10ms 50 times")]
        [Trait("SuperAccurateTimer", "")]
        public void Should_1MillisecondAccurate_When_10MillisecondInterval()
        {
            int intervalMs = 10;
            int expectedHits = 50;

            StartSuperAccurateTimer(intervalMs, expectedHits);
        }

        [Fact(DisplayName = "100ms 5 times")]
        [Trait("SuperAccurateTimer", "")]
        public void Should_1MillisecondAccurate_When_100MillisecondInterval()
        {
            int intervalMs = 100;
            int expectedHits = 5;

            StartSuperAccurateTimer(intervalMs, expectedHits);
        }

        private void StartSuperAccurateTimer(int intervalMs, int expectedHits)
        {
            int precisionMs = 0;
            int hits = 0;
            var precisionMatch = BuildPrecisionMatch(precisionMs);
            AutoResetEvent callbackDone = new AutoResetEvent(false);
            bool success = true;
            Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                Action callback = () =>
                {
                    var delayDecimal = sw.Elapsed.TotalMilliseconds;
                    var delay = sw.ElapsedMilliseconds;
                    var subtraction = delayDecimal - intervalMs;
                    sw.Restart();
                    hits++;
                    if (!precisionMatch.Contains((int)subtraction))
                    {
                        success = false;
                        callbackDone.Set();
                    }

                    else if (hits == expectedHits)
                    {
                        callbackDone.Set();
                    }
                    Log($"Precision diff: {subtraction} < {precisionMs}");
                };
                SuperAccurateTimer SuperAccurateTimer = new SuperAccurateTimer(callback);
                sw.Start();
                SuperAccurateTimer.Start(intervalMs);
            });
            callbackDone.WaitOne();
            Assert.True(success);
        }

        [Fact(DisplayName = "2000ms 3 times with scheduler")]
        [Trait("SuperAccurateTimer", "")]
        private void Should_Schedule_When_NextTriggerNotThatClose()
        {
            int intervalMs = 2_000;
            int precisionMs = 0;
            int hits = 0;
            int expectedHits = 3;

            var precisionMatch = BuildPrecisionMatch(precisionMs);
            AutoResetEvent callbackDone = new AutoResetEvent(false);
            bool success = true;
            Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                Action callback = () =>
                {
                    var delay = sw.ElapsedMilliseconds;
                    var subtraction = delay - intervalMs;
                    sw.Restart();
                    hits++;
                    if (!precisionMatch.Contains(subtraction))
                    {
                        success = false;
                        callbackDone.Set();
                    }

                    else if (hits == expectedHits)
                    {
                        callbackDone.Set();
                    }
                    Log($"Actual:{subtraction} < Expected: {precisionMs}");
                };
                SuperAccurateTimer superAccurateTimer = new SuperAccurateTimer(callback);
                sw.Start();
                superAccurateTimer.Start(intervalMs);
                if (!superAccurateTimer.Scheduled)
                {
                    success = false;
                    callbackDone.Set();
                }

            });
            callbackDone.WaitOne();
            Assert.True(success);
        }

        private ISet<long> BuildPrecisionMatch(int precisionMs)
        {
            if (precisionMs <= 0)
            {
                precisionMs = 1;
            }

            HashSet<long> possibleValues = new HashSet<long>();
            for (int i = precisionMs * -1; i < precisionMs+1; i++)
            {
                possibleValues.Add(i);
            }
            return possibleValues;
        }
    }
}