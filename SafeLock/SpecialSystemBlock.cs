using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SafeLock
{
    class SpecialSystemBlock
    {
        int lockoutAttempts = 1;
        Form1 mainForm;

        public SpecialSystemBlock(Form1 form)
        {
            mainForm = form;
            // hide ctor
        }

        public Task AttemptedBypass()
        {
            if (lockoutAttempts <= 0)
            {
                SystemLockout();
                return null;
            }
            MessageBox.Show($"Trying to bypass this will result in a lockout.\r\nYou have {lockoutAttempts} attempts left!", "Bypass detected");
            lockoutAttempts -= 1;
            return null;
        }

        public Task SystemLockout()
        {
            mainForm.password_Input.Visible = mainForm.login_Btn.Visible = mainForm.label1.Visible = false;
            MessageBox.Show("You have been locked out from the system due to too many failed attempts!", "System Lockout", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            return null;
        }

        #region Windows Special Keys
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }
        //System level functions to be used for hook and unhook keyboard input  
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);
        //Declaring Global objects     
        private IntPtr ptrHook;
        private LowLevelKeyboardProc objKeyboardProcess;

        private IntPtr captureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));

                // Disabling Windows keys 

                if (objKeyInfo.key == Keys.RWin || objKeyInfo.key == Keys.LWin || objKeyInfo.key == Keys.Tab && HasAltModifier(objKeyInfo.flags) || objKeyInfo.key == Keys.Escape && (Control.ModifierKeys & Keys.Control) == Keys.Control || objKeyInfo.key == Keys.Delete && (Control.ModifierKeys & Keys.Control) == Keys.Control && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                    return (IntPtr)1; // if 0 is returned then All the above keys will be enabled
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        bool HasAltModifier(int flags)
        {
            return (flags & 0x20) == 0x20;
        }

        Task KillTaskManager()
        {
            Process[] processes;
            while (true)
            {
                Thread.Sleep(100);
                if ((processes = Process.GetProcessesByName("Taskmgr")).Length <= 0) continue;
                Task.Run(() => AttemptedBypass());
                foreach (Process p in processes)
                    p.Kill();
                mainForm.Focus();
            }
        }
        #endregion
        #region Taskbar
        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        [DllImport("user32.dll")]
        public static extern int FindWindowEx(int parentHandle, int childAfter, string className, int windowTitle);

        [DllImport("user32.dll")]
        private static extern int GetDesktopWindow();

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;

        protected static int Handle
        {
            get
            {
                return FindWindow("Shell_TrayWnd", "");
            }
        }

        protected static int HandleOfStartButton
        {
            get
            {
                int handleOfDesktop = GetDesktopWindow();
                int handleOfStartButton = FindWindowEx(handleOfDesktop, 0, "button", 0);
                return handleOfStartButton;
            }
        }
        #endregion

        public void Show()
        {
            ShowWindow(Handle, SW_SHOW);
            ShowWindow(HandleOfStartButton, SW_SHOW);
        }

        public void Hide()
        {
            Task.Run(() => KillTaskManager());
            ShowWindow(Handle, SW_HIDE);
            ShowWindow(HandleOfStartButton, SW_HIDE);
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            objKeyboardProcess = new LowLevelKeyboardProc(captureKey);
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);
        }
    }
}
