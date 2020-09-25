using FantasyNameGenerator.NameGen2;
using System;
using System.IO;

namespace FantasyNameGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var set = new WordSet(2);
            set.addWords(File.ReadAllText("..\\..\\..\\NameGen2\\sets\\set1.txt"),",");

            while (1 == 1)
            {
                Console.WriteLine(NameGen2.MarkovChain.Generate(set,5,10));
                Console.ReadLine();
            }
        }
    }
}
