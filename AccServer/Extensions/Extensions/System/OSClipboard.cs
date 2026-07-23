namespace System
{
    using System.Runtime.InteropServices;

    public class OSClipboard
    {
        public const uint GMEM_DDESHARE = 0x2000;
        public const uint GMEM_MOVEABLE = 2;

        [DllImport("user32.dll")]
        public static extern bool CloseClipboard();
        [DllImport("user32.dll")]
        public static extern bool EmptyClipboard();
        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardData(uint uFormat);
        public static string GetText()
        {
            OpenClipboard(IntPtr.Zero);
            string str = Marshal.PtrToStringUni(GetClipboardData(13));
            CloseClipboard();
            return str;
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalFree(IntPtr hMem);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll")]
        public static extern UIntPtr GlobalSize(IntPtr hMem);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalUnlock(IntPtr hMem);
        [DllImport("user32.dll")]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("Kernel32.dll")]
        public static extern void RtlMoveMemory(IntPtr dest, IntPtr src, int size);
        [DllImport("user32.dll")]
        public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
        public static bool SetText(string text)
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                return false;
            }
            EmptyClipboard();
            IntPtr hMem = GlobalAlloc(0x2002, (UIntPtr) (2 * (text.Length + 1)));
            IntPtr destination = GlobalLock(hMem);
            if (text.Length > 0)
            {
                Marshal.Copy(text.ToCharArray(), 0, destination, text.Length);
            }
            GlobalUnlock(hMem);
            SetClipboardData(13, hMem);
            CloseClipboard();
            return true;
        }
    }
}

