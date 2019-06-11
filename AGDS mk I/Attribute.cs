using System;
using System.Collections.Generic;

namespace AGDS_mk_I
{
    [Serializable]
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
