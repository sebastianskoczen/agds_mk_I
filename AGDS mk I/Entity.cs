using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGDS_mk_I
{
    class Entity
    {
        public string id { get; set; }
        public List<Value> values { get; set; }

        public Entity(string inId, List<Tuple<string,string>> features, List<Value> v, List<Attribute> a)
        {
            id = inId;
            values = new List<Value>();
            this.AddFeatures(features,v,a);
        }

        public void AddFeatures(List<Tuple<string, string>> features, List<Value> v, List<Attribute> a)
        {
            Value val;
            Attribute att;
            foreach (Tuple<string, string> f in features)
            {
                val = null;
                att = null;
                att = a.Find(x => x.name == f.Item1);
                if (att == null)
                {
                    att = new Attribute(f.Item1);
                    a.Add(att);
                }
                val = v.Find(x => x.value == f.Item2);
                if (val != null && val.attribute.name == f.Item1)
                {
                    val.numberOfOccurences += 1;
                }
                else
                {
                    val = new Value(att, f.Item2);
                    v.Add(val);
                }
                val.entities.Add(this);
                values.Add(val);
                att.values.Add(val);
            }
        }
    }
}
