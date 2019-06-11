using System;
using System.Collections.Generic;

namespace AGDS_mk_I
{
    [Serializable]
    class Value
    {
        public Attribute attribute { get; } 
        public int numberOfOccurences { get; set; }
        public string value { get; }
        public List<Entity> entities { get; set; }

        public Value (Attribute a, string v)
        {
            attribute = a;
            value = v;
            numberOfOccurences = 1;
            entities = new List<Entity>();
        }
    }
}
