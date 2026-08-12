using System;
using System.Drawing;
using System.Windows.Forms;

namespace Super_Shop_Management_System.Animations
{
    public static class Transition
    {
        // Animate a control's bounds (position/size) from start to end over duration milliseconds
        public static void AnimateBounds(Control control, Rectangle startRect, Rectangle endRect, int duration, Action onComplete = null)
        {
            if (duration <= 0)
            {
                control.Bounds = endRect;
                onComplete?.Invoke();
                return;
            }

            int steps = duration / 16;
            int stepX = (endRect.X - startRect.X) / steps;
            int stepY = (endRect.Y - startRect.Y) / steps;
            int stepWidth = (endRect.Width - startRect.Width) / steps;
            int stepHeight = (endRect.Height - startRect.Height) / steps;

            int currentX = startRect.X;
            int currentY = startRect.Y;
            int currentWidth = startRect.Width;
            int currentHeight = startRect.Height;

            var form = control.FindForm();
            if (form == null) return;

            var timer = new System.Windows.Forms.Timer { Interval = 16, Enabled = true };
            int stepCount = 0;

            timer.Tick += (s, E) =>
            {
                stepCount++;
                currentX += stepX;
                currentY += stepY;
                currentWidth += stepWidth;
                currentHeight += stepHeight;

                control.Bounds = new Rectangle(currentX, currentY, currentWidth, currentHeight);

                if (stepCount >= steps)
                {
                    timer.Enabled = false;
                    control.Bounds = endRect;
                    timer.Dispose();
                    onComplete?.Invoke();
                }
            };

            timer.Start();
        }

        // Animate a control's width from start to end
        public static void AnimateWidth(Control control, int startWidth, int endWidth, int duration, Action onComplete = null)
        {
            if (duration <= 0)
            {
                control.Width = endWidth;
                onComplete?.Invoke();
                return;
            }

            int steps = duration / 16;
            int stepValue = (endWidth - startWidth) / steps;
            int currentWidth = startWidth;

            var form = control.FindForm();
            if (form == null) return;

            var timer = new System.Windows.Forms.Timer { Interval = 16, Enabled = true };
            int stepCount = 0;

            timer.Tick += (s, e) =>
            {
                stepCount++;
                currentWidth += stepValue;
                control.Width = currentWidth;

                if (stepCount >= steps)
                {
                    timer.Enabled = false;
                    control.Width = endWidth;
                    timer.Dispose();
                    onComplete?.Invoke();
                }
            };

            timer.Start();
        }

        // Animate a control's height from start to end
        public static void AnimateHeight(Control control, int startHeight, int endHeight, int duration, Action onComplete = null)
        {
            if (duration <= 0)
            {
                control.Height = endHeight;
                onComplete?.Invoke();
                return;
            }

            int steps = duration / 16;
            int stepValue = (endHeight - startHeight) / steps;
            int currentHeight = startHeight;

            var form = control.FindForm();
            if (form == null) return;

            var timer = new System.Windows.Forms.Timer { Interval = 16, Enabled = true };
            int stepCount = 0;

            timer.Tick += (s, e) =>
            {
                stepCount++;
                currentHeight += stepValue;
                control.Height = currentHeight;

                if (stepCount >= steps)
                {
                    timer.Enabled = false;
                    control.Height = endHeight;
                    timer.Dispose();
                    onComplete?.Invoke();
                }
            };

            timer.Start();
        }
    }
}