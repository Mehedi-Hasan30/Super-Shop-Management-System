using System;
using System.Windows.Forms;

namespace Super_Shop_Management_System.Animations
{
    public class SlideAnimator
    {
        private readonly Control _target;
        private readonly Timer _timer;
        private readonly int _duration;
        private readonly SlideDirection _direction;
        private int _currentPosition;
        private readonly int _startPosition;
        private readonly int _endPosition;
        private readonly int _step;

        public enum SlideDirection
        {
            LeftToRight,
            RightToLeft,
            TopToBottom,
            BottomToTop
        }

        public SlideAnimator(Control target, SlideDirection direction, int duration = 300)
        {
            _target = target;
            _direction = direction;
            _duration = duration;
            _timer = new Timer { Interval = 16, Enabled = false };

            int controlSize = _direction == SlideDirection.LeftToRight || _direction == SlideDirection.RightToLeft
                ? target.Width
                : target.Height;

            switch (direction)
            {
                case SlideDirection.LeftToRight:
                    _startPosition = -controlSize;
                    _endPosition = 0;
                    break;
                case SlideDirection.RightToLeft:
                    _startPosition = controlSize;
                    _endPosition = 0;
                    break;
                case SlideDirection.TopToBottom:
                    _startPosition = -controlSize;
                    _endPosition = 0;
                    break;
                case SlideDirection.BottomToTop:
                    _startPosition = controlSize;
                    _endPosition = 0;
                    break;
            }

            int steps = duration / 16;
            _step = Math.Max(1, (int)Math.Abs((_endPosition - _startPosition) / (double)steps));

            _currentPosition = _startPosition;
        }

        public void Start()
        {
            _timer.Interval = 16;
            _timer.Tick += OnTick;
            _timer.Enabled = true;
        }

        private void OnTick(object sender, EventArgs e)
        {
            switch (_direction)
            {
                case SlideDirection.LeftToRight:
                    _currentPosition += _step;
                    if (_currentPosition >= _endPosition)
                    {
                        _currentPosition = _endPosition;
                        _timer.Enabled = false;
                        if (_direction == SlideDirection.LeftToRight)
                            _target.Left = _endPosition;
                        else if (_direction == SlideDirection.RightToLeft)
                            _target.Left = _endPosition;
                    }
                    else
                    {
                        if (_direction == SlideDirection.LeftToRight)
                            _target.Left = _currentPosition;
                        else if (_direction == SlideDirection.RightToLeft)
                            _target.Left = _currentPosition;
                    }
                    break;

                case SlideDirection.TopToBottom:
                    _currentPosition += _step;
                    if (_currentPosition >= _endPosition)
                    {
                        _currentPosition = _endPosition;
                        _timer.Enabled = false;
                        if (_direction == SlideDirection.TopToBottom)
                            _target.Top = _endPosition;
                        else if (_direction == SlideDirection.BottomToTop)
                            _target.Top = _endPosition;
                    }
                    else
                    {
                        if (_direction == SlideDirection.TopToBottom)
                            _target.Top = _currentPosition;
                        else if (_direction == SlideDirection.BottomToTop)
                            _target.Top = _currentPosition;
                    }
                    break;

                case SlideDirection.BottomToTop:
                    _currentPosition -= _step;
                    if (_currentPosition <= _endPosition)
                    {
                        _currentPosition = _endPosition;
                        _timer.Enabled = false;
                        if (_direction == SlideDirection.BottomToTop)
                            _target.Top = _endPosition;
                    }
                    else
                    {
                        if (_direction == SlideDirection.BottomToTop)
                            _target.Top = _currentPosition;
                    }
                    break;
            }
        }
    }
}