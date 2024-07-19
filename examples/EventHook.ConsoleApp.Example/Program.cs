using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EventHook.ConsoleApp.Example
{
    public class WinAPI
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);
    }

    internal class Program
    {
        public static string GetActiveWindowTitle()
        {
            // 获取当前活动窗口的句柄
            IntPtr handle = WinAPI.GetForegroundWindow();

            // 获取窗口标题的长度，加1是为了容纳终止符
            int length = WinAPI.GetWindowTextLength(handle) + 1;

            // 创建一个StringBuilder来接收窗口标题
            StringBuilder sb = new StringBuilder(length);

            // 获取窗口标题
            WinAPI.GetWindowText(handle, sb, length);

            return sb.ToString();
        }

        private static void Main(string[] args)
        {
            var eventHookFactory = new EventHookFactory();

            var keyboardWatcher = eventHookFactory.GetKeyboardWatcher();
            keyboardWatcher.Start();
            keyboardWatcher.OnKeyInput += (s, e) =>
            {
                Console.WriteLine("Key {0} event of key {1}", e.KeyData.EventType, e.KeyData.Keyname);
            };

            var mouseWatcher = eventHookFactory.GetMouseWatcher();
            mouseWatcher.Start();
            mouseWatcher.OnMouseInput += (s, e) =>
            {
                Console.WriteLine("Mouse event {0} at point {1},{2} - {3}", e.Message.ToString(), e.Point.x, e.Point.y, GetActiveWindowTitle());
            };

            var clipboardWatcher = eventHookFactory.GetClipboardWatcher();
            clipboardWatcher.Start();
            clipboardWatcher.OnClipboardModified += (s, e) =>
            {
                Console.WriteLine("Clipboard updated with data '{0}' of format {1}", e.Data,
                    e.DataFormat.ToString());
            };


            var applicationWatcher = eventHookFactory.GetApplicationWatcher();
            applicationWatcher.Start();
            applicationWatcher.OnApplicationWindowChange += (s, e) =>
            {
                Console.WriteLine("Application window of '{0}' with the title '{1}' was {2}",
                    e.ApplicationData.AppName, e.ApplicationData.AppTitle, e.Event);
            };

            var printWatcher = eventHookFactory.GetPrintWatcher();
            printWatcher.Start();
            printWatcher.OnPrintEvent += (s, e) =>
            {
                Console.WriteLine("Printer '{0}' currently printing {1} pages.", e.EventData.PrinterName,
                    e.EventData.Pages);
            };


            Console.Read();

            keyboardWatcher.Stop();
            mouseWatcher.Stop();
            clipboardWatcher.Stop();
            applicationWatcher.Stop();
            printWatcher.Stop();

            eventHookFactory.Dispose();
        }
    }
}
