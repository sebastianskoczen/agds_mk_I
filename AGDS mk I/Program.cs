using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace AGDS_mk_I
{
    class Program
    {
        static void Main(string[] args)
        {
            TestIris();
            // TestProject();
        }

        static void TestIris()
        {
            AGDS graph = new AGDS("graph1");
            graph.GenerateFromCSV("./data/iris.csv");
            List<Tuple<Entity,Entity,double>> results = graph.CalculateSimilarities(graph.entities[1]).OrderByDescending(x => x.Item3).ToList();
            foreach(Tuple<Entity,Entity,double> r in results)
            {
                Console.WriteLine(r.Item1.values.Find(x => x.attribute.name == "class").value + " : " + r.Item1.id + " <-> " + r.Item2.values.Find(x => x.attribute.name == "class").value + " : " + r.Item2.id + " = \t" + r.Item3);
            }
            Console.ReadKey();
        }

        static void TestProject()
        {
            //AGDS graph = new AGDS("root");
            //graph.GenerateFromCSV("./input.csv");
            //graph.GeneratePNG("./graph.dot");

            MAGDS graph = new MAGDS();
            graph.GenerateFromCSVS("./data/inputlist.txt");
            //graphs.GeneratePNG("./graphs.dot",false);
            //foreach(Entity e in graphs.entities)
            //{
            //    Console.WriteLine("ID: " + e.id);
            //    foreach(Value v in e.values)
            //    {
            //        Console.WriteLine(v.attribute.name + ": " + v.value);
            //    }
            //    Console.WriteLine("\n\n");
            //}
            //List<Tuple<Entity, double>> result = graph.FindTheMostSimilarTo(graph.entities[0]);

            //Read weights
            //watch.Start();

            List<Tuple<string, double>> weights = new List<Tuple<string, double>>();
            weights = graph.ReadWeights("./data/inputweights.txt");
            List<Entity> candidates = graph.FindBestCandidates("regionalization", "leader", weights);

            //watch.Stop();
            //Console.WriteLine(watch.Elapsed.TotalMilliseconds);
            //foreach (Tuple<Entity, double> r in result)
            //{
            //    Console.WriteLine("ID: " + r.Item1.id + "   Similarity: " + r.Item2);
            //}
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int c = graph.entities[1].values.Count;
            watch.Stop();
            Console.WriteLine(watch.Elapsed.TotalMilliseconds);
            foreach (Entity e in candidates)
            {
                string name = e.values.Find(x => x.attribute.name == "name").value;
                string surname = e.values.Find(x => x.attribute.name == "surname").value;
                string lc = e.values.Find(x => x.attribute.name == "lc").value;
                string joined = e.values.Find(x => x.attribute.name == "joined").value;
                Console.WriteLine("ID: " + e.id);
                Console.WriteLine("Name: " + name);
                Console.WriteLine("Surname: " + surname);
                Console.WriteLine("LC: " + lc);
                Console.WriteLine("Member Since: " + joined);
                Console.WriteLine("\n" + "Experience");
                foreach (Value v in e.values)
                {
                    if (v.attribute.name != "name" &&
                        v.attribute.name != "surname" &&
                        v.attribute.name != "lc" &&
                        v.attribute.name != "joined" &&
                        v.attribute.name != "mandate")
                    {
                        Console.WriteLine(v.value + "  " + v.attribute.name);
                    }
                }
                Console.WriteLine("______________________________________________________ \n");
                //Console.ReadKey();
            }

            Stream s = File.Open("temp.dat", FileMode.Create);
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(s, graph);
            s.Close();
            //graph.GenerateTest();
            Console.ReadKey();
        }
    }
}
