using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChunjiinKeyPad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Button[] btns = new Button[]
            {
                this.button1,
                this.button2,
                this.button3,
                this.button4,
                this.button5,
                this.button6,
                this.button7,
                this.button8,
                this.button9,
                this.button10,
                this.button11,
                this.button12,
                this.button13,
            };
            Chunjiin chunjiin = new Chunjiin(this.textBox1,btns);
            
        }
    }
}
