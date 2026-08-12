using System;
using System.Drawing;
using System.Windows.Forms;

namespace Super_Shop_Management_System.Helpers
{
    public class LoadingIndicator : IDisposable
    {
        private readonly Control _parent;
        private readonly Panel _overlay;
        private readonly PictureBox _spinner;
        private readonly Label _textLabel;
        private bool _isVisible;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                _overlay.Visible = value;
                if (value)
                {
                    _overlay.BringToFront();
                }
            }
        }

        public LoadingIndicator(Control parent, string text = "Loading...")
        {
            _parent = parent;

            _overlay = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackColor = Color.FromArgb(100, 0, 0, 0),
                Cursor = Cursors.WaitCursor
            };

            // Spinner using a simple approach - draw with Paint event
            _spinner = new PictureBox
            {
                Location = new Point((_parent.Width - 32) / 2, (_parent.Height - 32) / 2),
                Size = new Size(32, 32),
                SizeMode = PictureBoxSizeMode.AutoSize,
                Visible = false
            };

            // Use a Label for the spinner indicator instead of an image
            Label spinnerLabel = new Label
            {
                Location = new Point(0, 0),
                Size = new Size(32, 32),
                Text = "⏳",
                Font = new Font("Segoe UI Emoji", 16F),
                ForeColor = Color.FromArgb(155, 160, 165),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _textLabel = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(155, 160, 165),
                Location = new Point(0, 42),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _parent.Controls.Add(_overlay);
            _overlay.Controls.Add(spinnerLabel);
            _overlay.Controls.Add(_textLabel);
        }

        public void Show()
        {
            IsVisible = true;
            _overlay.BringToFront();
        }

        public void Hide()
        {
            IsVisible = false;
            _overlay.Visible = false;
        }

        public void Dispose()
        {
            if (_overlay != null && _parent != null) _parent.Controls.Remove(_overlay);
            _overlay?.Dispose();
            // spinner and spinnerLabel are owned by _overlay, so disposing parent removes them
            _textLabel?.Dispose();
        }
    }
}