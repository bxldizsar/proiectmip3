using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace proiectmip3
{
    public partial class KeywordInput : Form
    {
        public KeywordInput()
        {
            InitializeComponent();
        }
        public string getInputText()
        {
            return textBox1.Text;
        }
    }
}
