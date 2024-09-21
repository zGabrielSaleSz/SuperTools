using SuperToolsTests.Core;
using System.Diagnostics;
using Xunit.Abstractions;
using SuperTools.Timers;

namespace SuperToolsTests.Timers
{
    public class SuperTimerTest : TestBase
    {
        public SuperTimerTest(ITestOutputHelper output) : base(output) { }

        [Fact(DisplayName = nameof(ShouldStop_When_TurnOffAfterTrigger))]
        [Trait("SuperTimer", "")]
        public void ShouldStop_When_TurnOffAfterTrigger()
        {
            AutoResetEvent callbackHit = new AutoResetEvent(false);
            Stopwatch sw = new Stopwatch();
            int hitCount = 0;
            Action callback = () =>
            {
                Log($"Callback hitted for {++hitCount}x after {sw.Elapsed.TotalMilliseconds} time!");
                callbackHit.Set();
            };

            SuperTimer superTimer = new SuperTimer(callback);
            sw.Start();
            superTimer.Start(500, true);

            callbackHit.WaitOne();
            Assert.True(hitCount == 1);
            callbackHit.WaitOne(1000);

            Assert.True(hitCount == 1);
        }

        [Fact(DisplayName = nameof(ShouldRepeat_When_DefaultSetup))]
        [Trait("SuperTimer", "")]
        public void ShouldRepeat_When_DefaultSetup()
        {
            AutoResetEvent callbackHit = new AutoResetEvent(false);
            Stopwatch sw = new Stopwatch();
            int hitCount = 0;
            int expectedHitCount = 3;
            Action callback = () =>
            {
                Log($"Callback hitted for {++hitCount}x after {sw.Elapsed.TotalMilliseconds} time!");
                if (hitCount == expectedHitCount)
                {
                    callbackHit.Set();
                }
            };

            SuperTimer superTimer = new SuperTimer(callback);
            sw.Start();
            superTimer.Start(100);

            callbackHit.WaitOne();
            Assert.True(hitCount == 3);
        }
    }
}
