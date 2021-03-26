using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ctc;

namespace ClassLibrary1
{
    public class Class1
    {
        public void Run()
        {
            Url.Instance().Run("url('https://www.youtube.com/watch?v=36YnV9STBqc')");
            MessageBox.Show("hello");
        }
    }
}
