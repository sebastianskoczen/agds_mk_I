using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AGDS_mk_I
{
    [Serializable]
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

        public double NumericallySimilarTo(Entity entity)
        {
            List<Tuple<Attribute, Value>> e1Features = new List<Tuple<Attribute, Value>>();
            List<Tuple<Attribute, Value>> e2Features = new List<Tuple<Attribute, Value>>();

            double tmpNumber;

            foreach (Value v in this.values)
                if (double.TryParse(v.value, out tmpNumber)) e1Features.Add(new Tuple<Attribute, Value>(v.attribute, v));

            foreach (Value v in entity.values)
                if (double.TryParse(v.value, out tmpNumber)) e2Features.Add(new Tuple<Attribute, Value>(v.attribute, v));

            double numberOfValues = e1Features.Count();

            Tuple<Attribute, Value> discovery;
            double number, similarity = 0.0d, min = Double.PositiveInfinity, max = Double.NegativeInfinity, tmp;

            foreach (Tuple<Attribute, Value> t in e1Features)
            {
                discovery = e2Features.Find(x => x.Item1.name == t.Item1.name);
                if (discovery != null)
                {
                    number = Double.Parse(discovery.Item2.value, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo);
                    min = Double.PositiveInfinity;
                    max = Double.NegativeInfinity;
                    foreach (Value v in t.Item1.values)
                    {
                        tmp = Double.Parse(v.value, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo);
                        if (min > tmp) min = tmp;
                        if (max < tmp) max = tmp;
                    }
                    double featureValue = double.Parse(t.Item2.value, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo);
                    similarity += (1 - (Math.Abs(number - featureValue)) / (max - min));
                }
            }
            return similarity / numberOfValues;
        }

    }
}
