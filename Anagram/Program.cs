using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

namespace Anagram
{
    class Program
    {
        #region Variables

        static long totalPermutations;
        static long currentPermutation;
        
        static HashSet<string> dictionary = new HashSet<string>();
        static HashSet<string> found = new HashSet<string>();

        static int leftThreads;
        static List<Thread> threads = new List<Thread>();

        #endregion

        static void Main(string[] args)
        {
            // Check dictionaries directory
            if (!Directory.Exists("dictionaries"))
            {
                Console.WriteLine("'dictionaries' directory does not exist. Please create it and add some dictionary files");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }

            // Add languages
            List<string> dicts = new List<string>();
            foreach (var file in Directory.GetFiles("dictionaries"))
                dicts.Add(Path.GetFileNameWithoutExtension(file));

            if (dicts.Count == 0)
            {
                Console.WriteLine("There was not found any dictionary files. Please add some and try again");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }

            // Select dictionary:
            Console.Write("Available dictionaries:");
            foreach (var d in dicts)
                Console.Write(" '" + d + "'");
            Console.WriteLine();

            Console.Write("Dictionary to use (default '" + dicts[0] + "'): ");
            var dict = Console.ReadLine();
            if (!dicts.Contains(dict))
                dict = dicts[0];

            // Read dicionary
            Console.WriteLine("Reading dictionary '" + dict + "'...");
            foreach (var line in File.ReadAllLines("dictionaries/" + dict + ".dic"))
                dictionary.Add(line);
            Console.WriteLine("Dictionary read.");

            // Select thread count
            int threadCount = 1;
            Console.WriteLine();
            Console.Write("Number of threads (default 1): ");
            int.TryParse(Console.ReadLine(), out threadCount);
            if (threadCount < 1)
                threadCount = 1;
            Console.WriteLine("Now using " + threadCount + " thread" + (threadCount == 1 ? "" : "s") + ".");

            // Compute anagram
            Console.WriteLine();
            Console.Write("Type anagram: ");
            var chars = Console.ReadLine().ToLower().ToList();
            totalPermutations = Factorial(chars.Count);
            Console.Clear();
            Console.WriteLine("Computing " + totalPermutations + " permutations...");
            //Compute(chars); // Single thread option
            ComputeMultithread(chars, 1);
            while (leftThreads > 0)
                Thread.Sleep(100);
            WritePercentage(100);
            Console.WriteLine(" Done. Results:");
            foreach (var f in found)
                Console.WriteLine(f);

            Console.ReadLine();
        }

        #region Compute

        static void ComputeMultithread(List<char> chars, int threadCount)
        {
            leftThreads = threadCount;

            List<char>[] tchars = new List<char>[threadCount];
            for (int i = 0; i < threadCount; i++)
                tchars[i] = new List<char>();

            for (int i = 0; i < chars.Count; i++)
                tchars[i % threadCount].Add(chars[i]);

            for (int i = 0; i < threadCount; i++)
            {
                new Thread(new ParameterizedThreadStart((j) =>
                {
                    foreach (var c in tchars[(int)j])
                    {
                        var newl = new List<char>(chars);
                        newl.Remove(c);
                        Compute(newl, c.ToString());
                    }
                    leftThreads--;
                }))
                { Priority = ThreadPriority.Lowest }.Start(i);
            }
        }

        static void Compute(List<char> chars, string current = "")
        {
            if (chars.Count == 0)
                ValidateAndAdd(current);
            else
                foreach (var c in chars)
                {
                    var newl = new List<char>(chars);
                    newl.Remove(c);

                    Compute(newl, current + c);
                }
        }

        #endregion

        #region Validate string

        static void ValidateAndAdd(string str)
        {
            PrintStatus();

            var spl = str.Split(' ');
            bool ok = true;
            foreach (var s in spl)
                if (!dictionary.Contains(s))
                {
                    ok = false;
                    break;
                }
            if (ok)
                found.Add(str);
        }

        // This should be used to avoid uses like "kk" and then 80 letters, it would save a lot of time
        //static bool Valid(string before, char after)
        //{
        //    if (before.Length == 0)
        //        return true;

        //    var bf = before[before.Length - 1];

        //    if (Vowel(bf))
        //        return true;

        //    //abbreviators
        //    // weekend
        //    // shoot
        //    // shader
        //    else
        //        return
        //            (bf == 'c' && after == 'k') ||
        //            (bf == 'n' && after == 'd') ||
        //            (bf == 'h' && after == 't') ||

        //            ((bf == 'c' || bf == 's' || bf == 'g') && after == 'h') ||
        //            ((bf == 'f' || bf == 'b' || bf == 'h') && after == 'r') ||

        //            (bf == 'b' && (after == 'b' || after == 'v' || after == 'r')) ||
        //            (bf == 'r' && (after == 's' || after == 'v' || after == 'r')) ||
        //            Vowel(after);
        //}

        //static bool Vowel(char c)
        //{
        //    return c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u';
        //}

        #endregion

        #region Information

        static void PrintStatus()
        {
            currentPermutation++;

            if (currentPermutation % 50000 == 0)
                WritePercentage((float)currentPermutation / (float)totalPermutations * 100f);
        }

        static void WritePercentage(float percentage)
        {
            Console.Write("\r" + percentage.ToString("00.00") + "%...");
        }

        static long Factorial(long i)
        {
            if (i <= 1)
                return 1;
            return i * Factorial(i - 1);
        }

        #endregion
    }
}
