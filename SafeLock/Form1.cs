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
            if (!TryLogin())
            {
                if (MessageBox.Show("Wrong passphrase!", "Could Not Log In", MessageBoxButtons.RetryCancel, MessageBoxIcon.Asterisk) == DialogResult.Cancel) Close();

                return;
            }
        }

        bool TryLogin()
        {
            LoginCredentials lc = new LoginCredentials();
            lc.Username = "klump";
            lc.Password = password_Input.Text;

            RequestHandler rh = new RequestHandler();
            string rcd;
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
}
