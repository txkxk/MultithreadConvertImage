using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    static class WriteToFile
    {
        static public void WriteInToFile(string str)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"log.txt"))
            {
                file.Write(str);
            }
        }
    }
}
