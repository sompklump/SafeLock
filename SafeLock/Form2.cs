using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SafeLock
{
    public partial class Form2 : Form
    {
        Form mainForm;
        public Form2(Form form)
        {
            InitializeComponent();
            mainForm = form;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void Form2_Activated(object sender, EventArgs e)
        {
            mainForm.Focus();
        }
    }
}
