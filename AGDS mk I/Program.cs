using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGDS_mk_I
{
    class Program
    {
        static void Main(string[] args)
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
            List<Tuple<string, double>> weights = new List<Tuple<string, double>>();
            weights = graph.ReadWeights("./data/inputweights.txt");

            List<Entity> candidates = graph.FindBestCandidates("regionalization", "leader",weights);
            //foreach (Tuple<Entity, double> r in result)
            //{
            //    Console.WriteLine("ID: " + r.Item1.id + "   Similarity: " + r.Item2);
            //}
            foreach(Entity e in candidates)
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
                foreach(Value v in e.values)
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
                Console.ReadKey();
            }
            //graph.GenerateTest();
            Console.ReadKey();
        }
    }
}
