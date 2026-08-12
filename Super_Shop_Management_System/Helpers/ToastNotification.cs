using System;
using System.Drawing;
using System.Windows.Forms;

namespace Super_Shop_Management_System.Helpers
{
    public enum ToastType
    {
        Success,
        Warning,
        Error,
        Info
    }

    public static class ToastNotification
    {
        // Show a toast notification using the owner form's status strip or a simple approach
        // Since we're not replacing MessageBox calls yet, this is infrastructure-only
        public static void Show(string message, ToastType type = ToastType.Info, Control owner = null)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            // For Phase UI-01, just output to debug/conole as infrastructure placeholder
            // Full implementation will be Phase UI-02+
            System.Diagnostics.Debug.WriteLine($"[Toast {type}] {message}");

            // Alternative: could set owner's Text or status, but not modifying existing forms
            if (owner != null)
            {
                // Could mark owner for redraw or set a temporary status
                // owner.Tag = $"TOAST:{type}:{message}";
            }
        }

        // Batch show - queue toasts if multiple called rapidly
        private static readonly System.Collections.Generic.Queue<string> _toastQueue =
            new System.Collections.Generic.Queue<string>();

        public static void ProcessQueue()
        {
            if (_toastQueue.Count > 0)
            {
                var msg = _toastQueue.Dequeue();
                ToastNotification.Show(_toastQueue.Count > 0 ? _toastQueue.Peek() : "");
            }
        }
    }
}