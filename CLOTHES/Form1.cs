using System;
using System.Windows.Forms;
using QLBH.DAL;

namespace CLOTHES
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;   // gắn sự kiện load
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            var name = Db.Scalar("SELECT DB_NAME()");
            MessageBox.Show("OK: " + name);
        }
    }
}
