using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Archiver
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int progress, string status)
        {
            
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new Action<int, string>(UpdateProgress), progress, status);
                return;
            }

            progressBar1.Value = Math.Min(Math.Max(progress, 0), 100);
            label1.Text = status;
            
            
        }

        private System.Windows.Forms.ProgressBar progressBar1;

        private void ProgressForm_Load(object sender, EventArgs e)
        {

        }
    }
}
