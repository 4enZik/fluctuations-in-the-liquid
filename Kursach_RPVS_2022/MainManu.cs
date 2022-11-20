using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kursach_RPVS_2022
{
    public partial class MainManu : Form
    {
        int count = 0;
        int max = 10;
        Menu form1 = new Menu();
        public MainManu()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Calculate();
        }

        public void Calculate()
        {
            count++;
            progressBar1.Value = count;
            if (count == max)
            {
                timer1.Stop();
                this.Hide();
                form1.ShowDialog();
                this.Close();
            }
        }
    }
}
