using SuperTools.Core;
using SuperTools.Core.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperTools.Timers
{
    /// <summary>
    /// SuperAccurateTimer is accurate with precision < 1ms.
    /// </summary>
    public class SuperAccurateTimer
    {
        public bool Scheduled { get; private set; } = false;

        private const int ONE_SECOND_IN_MS = 1_000;
        private readonly TimeSpan _halfSecond;
        private readonly SemaphoreSlim _semaphore;
        private readonly SuperTimer _superTimer;
        private readonly Thread _thread;
        private readonly Action _callback;

        private CancellationTokenSource _cancellationToken;

        private DateTime _lastTriggeredTime;
        private int _interval;
        private DateTime _expectedTrigger;

        private bool _started = false;
        

        public SuperAccurateTimer(Action callback)
        {
            Ensure.NotNull(callback);
            _cancellationToken = new CancellationTokenSource();

            _halfSecond = TimeSpan.FromMilliseconds(500);

            _semaphore = new SemaphoreSlim(1);
            _semaphore.Wait();

            _superTimer = new SuperTimer(SchedulerTimeout);
            _thread = PrepareHighPriorityThread();
            _callback = callback;
        }

        public void Start(int intervalMs)
        {
            SetLastTriggerNow();

            Ensure.PositiveOrZero(nameof(intervalMs), intervalMs);
            EnsureSingleStart();

            _interval = intervalMs;
            PrepareForNextTrigger();
        }

        public void Stop()
        {
            if (_cancellationToken == null)
            {
                throw new Exception("Already stopped");
            }
            _cancellationToken?.Cancel();
            _cancellationToken = null;
        }

        private void EnsureSingleStart()
        {
            if (_started)
            {
                throw new SuperToolException($"Can't start a {nameof(SuperAccurateTimer)} twice.");
            }
            _started = true;
        }

        private void SetLastTriggerNow()
        {
            _lastTriggeredTime = DateTime.UtcNow;
        }

        private void PrepareForNextTrigger() {
            _expectedTrigger = _lastTriggeredTime.Add(TimeSpan.FromMilliseconds(_interval));
            var difference = _expectedTrigger.Subtract(_lastTriggeredTime);
            if (difference.TotalMilliseconds <= ONE_SECOND_IN_MS)
            {
                EnableTriggerThread();
            }
            else
            {
                ScheduleAccurateThreadTrigger();
            }
        }
       

        private void ScheduleAccurateThreadTrigger()
        {
            Scheduled = true;
            var triggerInterval = _expectedTrigger.Subtract(_halfSecond).Millisecond;
            _superTimer.Start(triggerInterval, true);
        }

        private void SchedulerTimeout()
        {
            Scheduled = false;
            EnableTriggerThread();
        }

        private Thread PrepareHighPriorityThread()
        {
            var thread = new Thread(TriggerThread);
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
            return thread;
        }

        private void EnableTriggerThread()
        {
            _semaphore.Release();
        }

        private void TriggerThread(object obj)
        {
            while (!_cancellationToken.Token.IsCancellationRequested)
            {
                _semaphore.Wait(_cancellationToken.Token);
                if (_cancellationToken.Token.IsCancellationRequested)
                {
                    break;
                }

                while (DateTime.UtcNow < _expectedTrigger);

                Task.Run(() => _callback.Invoke());

                _lastTriggeredTime = DateTime.UtcNow;
                PrepareForNextTrigger();
            }
        }
    }
}
