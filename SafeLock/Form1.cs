using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SafeLock
{
    public partial class Form1 : Form
    {
        string darkMode_color1 = "#242424";
        string darkMode_color2 = "#4f4f4f";
        string darkMode_color3 = "#707070";
        SpecialSystemBlock taskbar;
        List<Form2> screens = new List<Form2>();
        bool allowedClosing = false;
        string ALLOWED_LOGIN_FILE_PATH = $"{Application.LocalUserAppDataPath}/validation/ALLOWED";

        public Form1()
        {
            InitializeComponent();
            taskbar = new SpecialSystemBlock(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach(Screen sc in Screen.AllScreens)
            {
                if (sc.Primary) continue;
                Form2 form2 = new Form2(this);
                screens.Add(form2);
                form2.Location = sc.WorkingArea.Location;
                form2.Show();
            }
            taskbar.Hide();
            label1.Location = new Point((panel1.Size.Width / 2) - (label1.Size.Width / 2), (panel1.Size.Height / 2) - 55);
            password_Input.Location = new Point((panel1.Size.Width / 2) - (password_Input.Size.Width / 2), panel1.Size.Height / 2);
            login_Btn.Location = new Point((panel1.Size.Width / 2) - (login_Btn.Size.Width / 2), (panel1.Size.Height / 2) + 35);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!allowedClosing) e.Cancel = true;
        }

        private void close_Btn_Click(object sender, EventArgs e)
        {
            CloseProcess();
        }

        public void CloseProcess()
        {
            foreach (Form2 f2 in screens)
                f2.Close();
            screens.Clear();
            taskbar.Show();
            allowedClosing = true;
            Environment.Exit(1);
        }

        private void login_Btn_Click(object sender, EventArgs e)
        {
            TryLogin();
        }

        bool TryLogin()
        {
            LoginCredentials lc = new LoginCredentials();
            lc.Username = "klump";
            lc.Password = password_Input.Text;

            RequestHandler rh = new RequestHandler();
            string rcd = null;
            Login();
            if(rh.SendData(REQUEST_TYPE_.LOGIN, lc, out rcd))
            {
                MessageBox.Show(rcd);
                Login();
                return true;
            }
            MessageBox.Show(rcd);
            return false;
        }

        void Login()
        {
            string ALLOWED_LOGIN_DIR_PATH = Application.LocalUserAppDataPath + "/validation";
            if (!Directory.Exists(ALLOWED_LOGIN_DIR_PATH))
                Directory.CreateDirectory(ALLOWED_LOGIN_DIR_PATH);
            string data = $@"// Information for Safe Lock backend
CloseTime: {DateTime.Now}";
            File.WriteAllText(ALLOWED_LOGIN_FILE_PATH, data);
        }

        private void password_Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F9 && Control.ModifierKeys == Keys.Control)
                CloseProcess();
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                TryLogin();
            }
            if (e.KeyCode == Keys.Delete && ModifierKeys == Keys.Alt && ModifierKeys == Keys.Control)
                e.Handled = true;
        }
    }
    public struct LoginCredentials
    {
        public string Username;
        public string Password;
        public string MAC;
    }
    class SpecialSystemBlock
    {
        int lockoutAttempts = 3;
        Form1 mainForm;

        void AttemptedBypass()
        {
            lockoutAttempts -= 1;
            if (lockoutAttempts <= 0)
                BlockSystem();
        }

        void BlockSystem() { 

        }

        public SpecialSystemBlock(Form1 form)
        {
            mainForm = form;
            // hide ctor
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
                Thread.Sleep(150);
                if ((processes = Process.GetProcessesByName("Taskmgr")).Length <= 0) continue;
                AttemptedBypass();
                foreach (Process p in processes)
                    p.Kill();
                mainForm.Focus();
                MessageBox.Show($"Trying to bypass this will result in a lockout.\r\nYou have {lockoutAttempts} left!", "Bypass detected");
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
