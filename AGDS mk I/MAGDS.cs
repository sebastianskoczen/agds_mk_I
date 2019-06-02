using DotNetGraph;
using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGDS_mk_I
{
    class MAGDS
    {
        public List<Entity> entities;
        List<AGDS> graphs;
        public MAGDS()
        {
            entities = new List<Entity>();
            graphs = new List<AGDS>();
        }
        public void GenerateFromCSVS(String list)
        {
            list = list.Replace("/", "\\");
            String line = String.Empty;
            StreamReader file = new StreamReader(list);
            List<String> files = new List<string>();
            if ((line = file.ReadLine()) != null)
            {
                files.Add(line);
                while ((line = file.ReadLine()) != null)
                {
                    files.Add(line);
                }
            }
            for(int i=0; i < files.Count(); i++)
            {
                graphs.Add(new AGDS(i.ToString(),ref entities));
                graphs[i].GenerateFromCSV(files[i]);
            }
        }

        public List<Tuple<string,double>> ReadWeights(String input)
        {
            List<Tuple<string, double>> weights = new List<Tuple<string, double>>();

            input = input.Replace("/", "\\");
            char[] charsToTrim = {' '};
            String line = String.Empty;
            StreamReader file = new StreamReader(input);
            string[] cells;
            if ((line = file.ReadLine()) != null)
            {
                cells = line.Split(',');
                weights.Add(new Tuple<string,double>(cells[0],Double.Parse(cells[1].Trim(charsToTrim), NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo)));
                while ((line = file.ReadLine()) != null)
                {
                    cells = line.Split(',');
                    weights.Add(new Tuple<string, double>(cells[0], Double.Parse(cells[1].Trim(charsToTrim), NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo)));
                }
            }

            return weights;
        }

        public List<Entity> FindBestCandidates(string team,string position)
        {
            List<Tuple<Entity, double>> results = new List<Tuple<Entity, double>>();
            List<Entity> predecessors = new List<Entity>();
            List<Entity> successors = new List<Entity>();

            foreach(AGDS g in graphs)
            {
                foreach(Attribute a in g.attributes)
                {
                    if(a.name == position)
                    {
                        Value v = a.values.Find(x => x.value == team);
                        predecessors = v.entities;
                    }
                }
            }

            foreach(Entity e in entities)
            {
                if(!predecessors.Exists(x => x.id == e.id))
                {
                    successors.Add(e);
                }
            }
            double score;
            foreach(Entity s in successors)
            {
                score = 0.0d;
                foreach (Entity p in predecessors)
                {
                    score += SimilarityBetweenCandidates(p,s);
                }
                score = score / predecessors.Count();
                //Console.WriteLine(s.id + " " + score);
                results.Add(new Tuple<Entity, double>(s,score));
            }

            List<Tuple<Entity, double>> tmp = results.OrderByDescending(x => x.Item2).ToList();
            List<Entity> final = new List<Entity>();
            foreach(Tuple<Entity,double> t in tmp)
            {
                final.Add(t.Item1);
            }
            return final;
        }

        public List<Entity> FindBestCandidates(string team, string position, List<Tuple<string,double>> weights)
        {
            List<Tuple<Entity, double>> results = new List<Tuple<Entity, double>>();
            List<Entity> predecessors = new List<Entity>();
            List<Entity> successors = new List<Entity>();

            foreach (AGDS g in graphs)
            {
                foreach (Attribute a in g.attributes)
                {
                    if (a.name == position)
                    {
                        Value v = a.values.Find(x => x.value == team);
                        predecessors = v.entities;
                    }
                }
            }

            foreach (Entity e in entities)
            {
                if (!predecessors.Exists(x => x.id == e.id))
                {
                    successors.Add(e);
                }
            }
            double score;
            foreach (Entity s in successors)
            {
                score = 0.0d;
                foreach (Entity p in predecessors)
                {
                    score += SimilarityBetweenCandidates(p, s, weights);
                }
                score = score / predecessors.Count();
                //Console.WriteLine(s.id + " " + score);
                results.Add(new Tuple<Entity, double>(s, score));
            }

            List<Tuple<Entity, double>> tmp = results.OrderByDescending(x => x.Item2).ToList();
            List<Entity> final = new List<Entity>();
            foreach (Tuple<Entity, double> t in tmp)
            {
                final.Add(t.Item1);
            }
            return final;
        }

        public List<Tuple<Entity,double>> FindTheMostSimilarTo(Entity e)
        {
            List<Tuple<Entity, double>> results = CalculateAllSimilarities(e);
            return results;
        }

        public List<Tuple<Entity, double>> FindTheMostSimilarTo(Entity e, int n)
        {
            List<Tuple<Entity, double>> results = CalculateAllSimilarities(e);
            return results.Take(n).ToList();
        }
        public List<Tuple<Entity, double>> CalculateAllSimilarities(Entity e)
        {
            List<Tuple<Entity, double>> results = new List<Tuple<Entity, double>>();
            foreach (Entity e2 in entities)
            {
                if (e.id != e2.id) results.Add(new Tuple<Entity, double>(e2, SimilarityBetween(e, e2)));
            }
            List<Tuple<Entity, double>> final = results.OrderByDescending(x => x.Item2).ToList();
            return final;
        }

        public double SimilarityBetweenCandidates(Entity e1, Entity e2)
        {
            List<Tuple<Attribute, Value>> e1Features = new List<Tuple<Attribute, Value>>();
            List<Tuple<Attribute, Value>> e2Features = new List<Tuple<Attribute, Value>>();

            foreach (Value v in e1.values)
                if (v.attribute.name != "name" && v.attribute.name != "surname" && v.attribute.name != "mandate" && v.attribute.name !="lc" && v.attribute.name != "joined") e1Features.Add(new Tuple<Attribute, Value>(v.attribute, v));

            foreach (Value v in e2.values)
                if (v.attribute.name != "name" && v.attribute.name != "surname" && v.attribute.name != "mandate" && v.attribute.name != "lc" && v.attribute.name != "joined") e2Features.Add(new Tuple<Attribute, Value>(v.attribute, v));

            double numberOfFeatures = e1Features.Count();

            return ComputeSimilarity(e1Features, e2Features, numberOfFeatures);
        }

        public double SimilarityBetweenCandidates(Entity e1, Entity e2, List<Tuple<string,double>> weights)
        {
            List<Tuple<Attribute, Value>> e1Features = new List<Tuple<Attribute, Value>>();
            List<Tuple<Attribute, Value>> e2Features = new List<Tuple<Attribute, Value>>();

            foreach (Value v in e1.values)
                if (v.attribute.name != "name" && v.attribute.name != "surname" && v.attribute.name != "mandate" && v.attribute.name != "lc" && v.attribute.name != "joined") e1Features.Add(new Tuple<Attribute, Value>(v.attribute, v));

            foreach (Value v in e2.values)
                if (v.attribute.name != "name" && v.attribute.name != "surname" && v.attribute.name != "mandate" && v.attribute.name != "lc" && v.attribute.name != "joined") e2Features.Add(new Tuple<Attribute, Value>(v.attribute, v));

            double numberOfFeatures = e1Features.Count();

            return ComputeSimilarity(e1Features, e2Features, numberOfFeatures, weights);
        }

        public double SimilarityBetween(Entity e1, Entity e2)
        {
            List<Tuple<Attribute, Value>> e1Features = new List<Tuple<Attribute, Value>>();
            List<Tuple<Attribute, Value>> e2Features = new List<Tuple<Attribute, Value>>();

            foreach (Value v in e1.values)
                if(v.attribute.name != "name" && v.attribute.name != "surname" && v.attribute.name != "mandate") e1Features.Add(new Tuple<Attribute, Value>(v.attribute, v));

            foreach (Value v in e2.values)
                if (v.attribute.name != "name" && v.attribute.name != "surname" && v.attribute.name != "mandate") e2Features.Add(new Tuple<Attribute, Value>(v.attribute, v));

            double numberOfFeatures = e1Features.Count();

            return ComputeSimilarity(e1Features,e2Features,numberOfFeatures);
        }

        public double ComputeSimilarity(List<Tuple<Attribute,Value>> e1Features, List<Tuple<Attribute, Value>> e2Features, double numberOfFeatures)
        {
            List<Tuple<Attribute, Value>> discovery = new List<Tuple<Attribute, Value>>();
            double number, similarity = 0.0d, min = Double.PositiveInfinity, max = Double.NegativeInfinity, tmp;

            foreach (Tuple<Attribute, Value> t in e1Features)
            {
                discovery = e2Features.FindAll(x => x.Item1.name == t.Item1.name);
                if (discovery.Any())
                {
                    foreach (Tuple<Attribute, Value> d in discovery)
                    {
                        if (Double.TryParse(d.Item2.value, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out number)) //double.TryParse(discovery.Item2.value, out number));
                        {
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
                        else
                        {
                            if (t.Item2.value == d.Item2.value) similarity += 1;
                        }
                    }
                }
            }
            return similarity / numberOfFeatures;
        }

        public double ComputeSimilarity(List<Tuple<Attribute, Value>> e1Features, List<Tuple<Attribute, Value>> e2Features, double numberOfFeatures, List<Tuple<string, double>> weights)
        {
            List<Tuple<Attribute, Value>> discovery = new List<Tuple<Attribute, Value>>();
            double number, similarity = 0.0d, min = Double.PositiveInfinity, max = Double.NegativeInfinity, tmp, weight,rest,tmp2 = 0.0;
            List<Tuple<string, double>> matchWeights = new List<Tuple<string, double>>();

            foreach(Tuple<string,double> w in weights)
            {
                tmp2 += w.Item2;
            }

            rest = (1 - tmp2) / (numberOfFeatures - weights.Count());

            foreach (Tuple<Attribute, Value> t in e1Features)
            {
                discovery = e2Features.FindAll(x => x.Item1.name == t.Item1.name);
                if (discovery.Any())
                {
                    foreach (Tuple<Attribute, Value> d in discovery)
                    {
                        matchWeights = weights.FindAll(x => x.Item1 == d.Item1.name);
                        if (matchWeights.Any()) weight = matchWeights.First().Item2;
                        else weight = rest;
                        if (Double.TryParse(d.Item2.value, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out number))
                        {
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
                        else
                        {
                            if (t.Item2.value == d.Item2.value) similarity += weight * 1;
                        }
                    }
                }
            }
            return similarity/numberOfFeatures;
        }

        public void GeneratePNG(String path,bool withEntities)
        {
            if (File.Exists(path))
                File.Delete(path);

            StreamWriter file = new StreamWriter(path);
            file.WriteLine("graph {");

            List<String> buffer = new List<string>();
            List<String> input = new List<string>();
            String line = String.Empty;
            foreach (AGDS graph in graphs)
            {
                foreach (Attribute a in graph.attributes)
                {
                    line = "graph_" + graph.name + " -- " + a.name;
                    buffer.Add(line);
                }
            }
            foreach (AGDS graph in graphs)
            {
                foreach(Attribute a in graph.attributes)
                {
                    foreach(Value v in a.values)
                    {
                        line = v.attribute.name + " -- " + v.value;
                        buffer.Add(line);
                    }
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
            foreach(String s in input)
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
                myProcess.StartInfo.Arguments = @"-Tpng graphs.dot -o graphs.png";
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                myProcess.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}
