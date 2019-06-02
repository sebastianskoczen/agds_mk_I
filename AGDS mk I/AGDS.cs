using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Globalization;

namespace AGDS_mk_I
{
    class AGDS
    {
        public string name;
        public List<Attribute> attributes;
        List<Value> values;
        public List<Entity> entities;

        public AGDS(string inname)
        {
            name = inname;
            attributes = new List<Attribute>();
            values = new List<Value>();
            entities = new List<Entity>();
        }

        public AGDS(string inname, ref List<Entity> inentity)
        {
            name = inname;
            attributes = new List<Attribute>();
            values = new List<Value>();
            entities = inentity;
        }

        public Entity FindTheMostSimilarTo(Entity e)
        {
            List<Tuple<Entity, double>> results = CalculateAllSimilarities(e);
            //foreach(Tuple<Entity,double> r in results)
            //{
            //    Console.WriteLine(e.id + " -> " + r.Item1.id + " = " + r.Item2);
            //}
            return results.First().Item1;
        }

        public List<Tuple<Entity,double>> FindTheMostSimilarTo(Entity e, int n)
        {
            List<Tuple<Entity, double>> results = CalculateAllSimilarities(e);
            return results.Take(n).ToList();
        }
        public List<Tuple<Entity,double>> CalculateAllSimilarities(Entity e)
        {
            List<Tuple<Entity, double>> results = new List<Tuple<Entity, double>>();
            foreach(Entity e2 in entities)
            {
                if(e.id != e2.id) results.Add(new Tuple<Entity, double>(e2, SimilarityBetween(e, e2)));
            }
            List<Tuple<Entity,double>> final = results.OrderByDescending(x => x.Item2).ToList();
            return final;
        }
        public double SimilarityBetween(Entity e1, Entity e2)
        {
            List<Tuple<Attribute, Value>> e1Features = new List<Tuple<Attribute, Value>>();
            List<Tuple<Attribute, Value>> e2Features = new List<Tuple<Attribute, Value>>();

            foreach(Value v in e1.values)
                if (v.attribute.name != "name" && v.attribute.name != "surname") e1Features.Add(new Tuple<Attribute, Value>(v.attribute,v));

            foreach(Value v in e2.values)
                if (v.attribute.name != "name" && v.attribute.name != "surname") e2Features.Add(new Tuple<Attribute, Value>(v.attribute, v));

            double numberOfValues = e1Features.Count();
            //double attributeWeight = 1/ numberOfValues;
            Tuple<Attribute, Value> discovery;
            double number,similarity = 0.0d, min = Double.PositiveInfinity, max = Double.NegativeInfinity, tmp;

            foreach (Tuple<Attribute,Value> t in e1Features)
            {
                discovery = e2Features.Find(x => x.Item1.name == t.Item1.name);
                if(discovery != null)
                {
                    if (Double.TryParse(discovery.Item2.value, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out number)) //double.TryParse(discovery.Item2.value, out number));
                    {
                        min = Double.PositiveInfinity;
                        max = Double.NegativeInfinity;
                        foreach(Value v in t.Item1.values)
                        {
                            tmp = Double.Parse(v.value, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo);
                            if (min > tmp) min = tmp;
                            if (max < tmp) max = tmp;
                        }
                        double featureValue = double.Parse(t.Item2.value, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo);
                        similarity += (1 - (Math.Abs(number - featureValue)) / (max - min));
                    }
                    else
                    {
                        if(t.Item2.value == discovery.Item2.value) similarity += 1;
                    }
                }
            }
            return similarity/numberOfValues;
        }

        public void GenerateFromCSV(String path)
        {
            path = path.Replace("/", "\\");
            String line = String.Empty;
            StreamReader file = new StreamReader(path);
            List<Tuple<string, string>> tuples;
            String[] fields,currentRow;
            if ((line = file.ReadLine()) != null)
            {
                fields = line.Split(',');
                for (int i = 0; i < fields.Length; i++)
                {
                    fields[i] = fields[i].Trim();
                }
                while ((line = file.ReadLine()) != null)
                {
                    tuples = new List<Tuple<string, string>>();
                    currentRow = line.Split(',');
                    Entity e = entities.Find(x => x.id == currentRow[0]);
                    for (int i = 1; i < currentRow.Length; i++)
                    {
                        currentRow[i] = currentRow[i].Trim();
                        if (currentRow[i] != "") tuples.Add(new Tuple<string, string>(fields[i], currentRow[i]));
                    }
                    if (e == null)
                        entities.Add(new Entity(currentRow[0], tuples, values, attributes));
                    else
                        e.AddFeatures(tuples, values, attributes);

                }
            }
            file.Close();
        }

        public void GeneratePNG(String path, bool withEntities)
        {
            StreamWriter file = new StreamWriter(path);
            file.WriteLine("graph {");
            List<String> buffer = new List<string>();
            List<String> input = new List<string>();
            String line = String.Empty;
            foreach (Attribute a in attributes)
            {
                line = name + " -- " + a.name;
                buffer.Add(line);
            }
            foreach(Attribute a in attributes)
            {
                foreach (Value v in a.values)
                {
                    line = a.name + " -- " + v.value;
                    buffer.Add(line);
                }
            }
            if(withEntities)
            {
                foreach (Entity e in entities)
                {
                    foreach (Value v in e.values)
                    {
                        line = v.value + " -- " + e.id;
                        buffer.Add(line);
                    }
                }
            }
            input = buffer.Distinct().ToList();
            foreach (String s in input)
            {
                file.WriteLine(s);
            }
            file.WriteLine("}");
            file.Close();

            Process myProcess = new Process();
            try
            {
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.FileName = @"C:\Program Files (x86)\Graphviz2.38\bin\dot.exe";
                myProcess.StartInfo.Arguments = @"-Tpng graph.dot -o graph.png";
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                myProcess.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void GenerateTest()
        {
            //Data Mock
            Tuple<string, string> s = new Tuple<string, string>("name", "Seba");
            Tuple<string, string> a = new Tuple<string, string>("name", "Anna");
            Tuple<string, string> gs = new Tuple<string, string>("grade", "5");
            Tuple<string, string> ga = new Tuple<string, string>("grade", "5");
            Tuple<string, string> aa = new Tuple<string, string>("age", "22");
            //Tuple<string, string> ga = new Tuple<string, string>("grade", "5");
            List<Tuple<string, string>> slist = new List<Tuple<string, string>>();
            List<Tuple<string, string>> alist = new List<Tuple<string, string>>();
            slist.Add(s);
            slist.Add(gs);
            alist.Add(a);
            alist.Add(ga);
            alist.Add(aa); 
            entities.Add(new Entity("1", slist, values, attributes)); //entities.Add(new Entity(int.Parse("1"), slist,values,attributes));
            entities.Add(new Entity("2", alist, values, attributes)); //entities.Add(new Entity(int.Parse("2"), alist, values, attributes));
        }
    }
}
