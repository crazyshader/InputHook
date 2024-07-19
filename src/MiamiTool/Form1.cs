using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EventHook;

namespace MiamiTool
{
    public partial class Form1 : Form
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID_F8 = 9000; // 热键ID F8
        private const int HOTKEY_ID_F7 = 9001; // 热键ID F7
        private const int HOTKEY_ID_F9 = 9002; // 热键ID F9
        private const int HOTKEY_ID_F6 = 9003; // 热键ID F6
        private const uint MOD_NONE = 0x0000;
        private const uint VK_F9 = 0x78; // F9 键
        private const uint VK_F8 = 0x77; // F8 键
        private const uint VK_F7 = 0x76; // F7 键
        private const uint VK_F6 = 0x75; // F6 键

        private int startIndex = 0;
        private int passCount = 0;
        private int deathCount = 0;
        private bool isRestart = false;
        private DateTime startTime = DateTime.Now;
        private Dictionary<int, DateTime> dateTimes = new Dictionary<int, DateTime>();
        private Dictionary<int, LevelTime> levelTimes = new Dictionary<int, LevelTime>();

        private readonly EventHookFactory eventHookFactory = new EventHookFactory();
        private readonly KeyboardWatcher keyboardWatcher;

        public Form1()
        {
            Application.ApplicationExit += OnApplicationExit;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            InitializeComponent();

            keyboardWatcher = eventHookFactory.GetKeyboardWatcher();
            keyboardWatcher.OnKeyInput += (s, e) =>
            {
                WatchKeyboardEvent(e);
            };

            SetListerState();

            WinAPI.RegisterHotKey(this.Handle, HOTKEY_ID_F7, MOD_NONE, VK_F7);
            WinAPI.RegisterHotKey(this.Handle, HOTKEY_ID_F8, MOD_NONE, VK_F8);
            WinAPI.RegisterHotKey(this.Handle, HOTKEY_ID_F9, MOD_NONE, VK_F9);
            WinAPI.RegisterHotKey(this.Handle, HOTKEY_ID_F6, MOD_NONE, VK_F6);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                switch (m.WParam.ToInt32())
                {
                    case HOTKEY_ID_F6:
                        button3_Click(null, null);
                        break;
                    case HOTKEY_ID_F7:
                        button1_Click(null , null);
                        break;
                    case HOTKEY_ID_F8:
                        button2_Click(null, null);
                        break;
                    case HOTKEY_ID_F9:
                        button4_Click(null, null);
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private void WatchKeyboardEvent(KeyInputEventArgs e)
        {
            var appTitleName = GetActiveWindowTitle();
            if (appTitleName.Contains("Hotline Miami") 
                && e.KeyData.EventType == KeyEvent.down && e.KeyData.Keyname == "R")
            {
                ++deathCount;
                DateTime currentTime = DateTime.Now;
                dateTimes.Add(deathCount, currentTime);

                var item = new ListViewItem((deathCount - startIndex).ToString());
                item.SubItems.Add(currentTime.ToString("yyyy-MM-dd HH:mm:ss"));

                if (deathCount > 1 && !isRestart)
                {
                    var diff = currentTime - dateTimes[deathCount - 1];
                    item.SubItems.Add(diff.ToString(@"hh\:mm\:ss"));
                }
                else
                {
                    var diff = currentTime - startTime;
                    item.SubItems.Add(diff.ToString(@"hh\:mm\:ss"));
                }

                isRestart = false;

                if (listView1.InvokeRequired)
                {
                    listView1.Invoke(new Action(() =>
                    {
                        listView1.Items.Add(item);
                    }));
                }
                else
                {
                    listView1.Items.Add(item);
                }
            }
        }

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

        private void button1_Click(object sender, EventArgs e)
        {
            if (!keyboardWatcher.isRunning)
            {
                isRestart = true;
                startIndex = deathCount;
                startTime = DateTime.Now;
                keyboardWatcher.Start();
                SetListerState();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (keyboardWatcher.isRunning)
            {
                var levelTime = new LevelTime();
                levelTime.deathPassTime = DateTime.Now - startTime;
                if (dateTimes.Count > 0)
                {
                    levelTime.deathNoPassTime = DateTime.Now - dateTimes[dateTimes.Count];
                }
                else
                {
                    levelTime.deathNoPassTime = levelTime.deathPassTime;
                }
                levelTimes.Add(++passCount, levelTime);

                var item = new ListViewItem("已通" + (passCount).ToString() + "层");
                item.SubItems.Add("非死亡通关时间:" + (levelTime.deathNoPassTime).ToString(@"hh\:mm\:ss"));
                item.SubItems.Add("死亡通关时间:" + (levelTime.deathPassTime).ToString(@"hh\:mm\:ss"));

                if (listView1.InvokeRequired)
                {
                    listView1.Invoke(new Action(() =>
                    {
                        listView1.Items.Add(item);
                    }));
                }
                else
                {
                    listView1.Items.Add(item);
                }

                keyboardWatcher.Stop();
                SetListerState();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            startIndex = 0;
            passCount = 0;
            deathCount = 0;
            isRestart = false;
            startTime = new DateTime();
            dateTimes.Clear();
            levelTimes.Clear();
            listView1.Items.Clear();

            if (keyboardWatcher.isRunning)
            {
                keyboardWatcher.Stop();
                SetListerState();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (levelTimes.Count == 0)
            {
                return;
            }

            if (keyboardWatcher.isRunning)
            {
                keyboardWatcher.Stop();
                SetListerState();
            }

            TimeSpan deathPassTime = new TimeSpan();
            TimeSpan deathNoPassTime = new TimeSpan();
            foreach (var levelTime in levelTimes)
            {
                deathPassTime += levelTime.Value.deathPassTime;
                deathNoPassTime += levelTime.Value.deathNoPassTime;
            }

            var item = new ListViewItem("已通章节，死亡" + (deathCount).ToString() + "次");
            item.SubItems.Add("非死亡通关时间:" + (deathNoPassTime).ToString(@"hh\:mm\:ss"));
            item.SubItems.Add("死亡通关时间:" + (deathPassTime).ToString(@"hh\:mm\:ss"));

            string clipboardText = string.Format("{0}\t{1}\t{2}", 
                deathCount, (deathNoPassTime).ToString(@"hh\:mm\:ss"), (deathPassTime).ToString(@"hh\:mm\:ss"));
            Clipboard.SetText(clipboardText);

            if (listView1.InvokeRequired)
            {
                listView1.Invoke(new Action(() =>
                {
                    listView1.Items.Add(item);
                }));
            }
            else
            {
                listView1.Items.Add(item);
            }
        }

        private void SetListerState()
        {
            label1.Text = "状态：" + (keyboardWatcher.isRunning ? "监控" : "停止");
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            keyboardWatcher.Stop();
            eventHookFactory.Dispose();

            WinAPI.UnregisterHotKey(this.Handle, HOTKEY_ID_F6);
            WinAPI.UnregisterHotKey(this.Handle, HOTKEY_ID_F7);
            WinAPI.UnregisterHotKey(this.Handle, HOTKEY_ID_F8);
            WinAPI.UnregisterHotKey(this.Handle, HOTKEY_ID_F9);
        }

        private struct LevelTime
        {
            public TimeSpan deathPassTime;
            public TimeSpan deathNoPassTime;
        }

        public class WinAPI
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowTextLength(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

            [DllImport("user32.dll")]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        }
    }
}
