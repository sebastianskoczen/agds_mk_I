using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGDS_mk_I
{
    class Attribute
    {
        public string name { get; set; }
        public List <Value> values { get; set; }

        public Attribute(string n)
        {
            name = n;
            values = new List<Value>();
        }
    }
}
