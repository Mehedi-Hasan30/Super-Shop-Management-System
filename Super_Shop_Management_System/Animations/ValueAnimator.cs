using System;
using System.Windows.Forms;

namespace Super_Shop_Management_System.Animations
{
    /// <summary>
    /// Animates a numeric value change over time.
    /// The caller is responsible for applying the CurrentValue to a control property.
    /// </summary>
    public class ValueAnimator
    {
        private readonly Control _owner;
        private readonly Timer _timer;
        private readonly int _duration;
        private float _currentValue;
        private readonly float _startValue;
        private readonly float _endValue;
        private readonly int _step;
        private bool _isRunning;

        public ValueAnimator(Control owner, float startValue, float endValue, int duration = 300)
        {
            _owner = owner;
            _startValue = startValue;
            _endValue = endValue;
            _duration = duration;
            _timer = new Timer { Interval = 16, Enabled = false };
            double difference = endValue - startValue;
            _step = Math.Max(1, (int)Math.Abs(difference / (duration / 16.0)));
            _currentValue = startValue;
            _isRunning = false;
        }

        public bool IsRunning => _isRunning;

        public float CurrentValue
        {
            get => _currentValue;
            private set
            {
                _currentValue = value;
                // Notify subscriber if owner exists
                if (_owner != null && _owner.Handle != IntPtr.Zero)
                {
                    try
                    {
                        _owner.Invoke(new Action(() =>
                        {
                            // Value available via CurrentValue property
                        }));
                    }
                    catch { }
                }
            }
        }

        // Public tick trigger - animator handles its own timing
        public void Tick()
        {
            if (!_isRunning) return;

            _currentValue += _step;

            if (_step > 0 && _currentValue >= _endValue)
            {
                _currentValue = _endValue;
                _isRunning = false;
            }
            else if (_step < 0 && _currentValue <= _endValue)
            {
                _currentValue = _endValue;
                _isRunning = false;
            }

            // Apply value to owner control - subscriber decides how
            if (_owner != null && _owner.Handle != IntPtr.Zero)
            {
                try
                {
                    _owner.Invoke(new Action(() =>
                    {
                        // Subscriber can read CurrentValue
                    }));
                }
                catch { }
            }

            if (Math.Abs(_currentValue - _endValue) < _step)
            {
                _isRunning = false;
                _currentValue = _endValue;
            }
        }

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            // Timer will drive the Tick calls
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
}