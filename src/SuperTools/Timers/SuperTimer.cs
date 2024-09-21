using System.Threading;
using SuperTools.Core;
using System;

namespace SuperTools.Timers
{
    public class SuperTimer
    {
        private Timer _timer;
        private int _configStartsIn;
        private int _configInterval;
        private bool _turnOffNextTime = false;
        private readonly Action _callback;

        public SuperTimer(Action callback)
        {
            Ensure.NotNull(callback);
            _callback = callback;
        }

        public void Start(int interval, bool turnOffAfterTrigger = false)
        {
            Ensure.PositiveOrZero(nameof(interval), interval);
            _configStartsIn = interval;
            _configInterval = interval;
            _turnOffNextTime = turnOffAfterTrigger;
            StartNew();
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }

        public void Restart()
        {
            StartNew();
        }

        public void Change(int startsIn, int interval)
        {
            Ensure.PositiveOrZero(nameof(startsIn), startsIn);
            Ensure.PositiveOrZero(nameof(interval), interval);
            _configInterval = interval;
            _configStartsIn = startsIn;
            if (_timer == null)
            {
                StartNew();
            }
            else
            {
                _timer.Change(_configStartsIn, _configInterval);
            }
        }

        private void StartNew()
        {
            Stop();
            _timer = new Timer(timerCallback, null, _configStartsIn, _configInterval);
        }

        private void timerCallback(object state)
        {
            _callback.Invoke();
            if (_turnOffNextTime && _timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
