using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipes
{
    class Program
    {
        static T Prompt<T>(string promptString, Func<string, (bool, T)> parse)
        {
            bool valid = false;
            T val = default(T);
            while (!valid)
            {
                Console.Write(promptString);
                (valid, val) = parse(Console.ReadLine());
            }
            return val;
        }

        static (bool, int) FancyIntParse(string str)
        {
            bool b;
            return (b = int.TryParse(str, out int v), b ? v : 0);
        }

        static (bool, int) FancyNaturalNumParse(string str)
        {
            bool b;
            return (b = (int.TryParse(str, out int v) && v > 0), b ? v : 0);
        }

        static void Main(string[] args)
        {
            Random random = new Random();
            Func<int[], (int, int)>[] algos = { WaterAlgo1 };
            while (true)
            {
                int size = Prompt("Width: ", FancyNaturalNumParse);
                int maxHeight = Prompt("Height: ", FancyNaturalNumParse);
                int[] pipes = new int[size];
                for(int i = 0; i < size; i++)
                {
                    pipes[i] = random.Next(1, maxHeight + 1);
                }
                Console.WriteLine();
                Render(pipes);
                
                for(int i = 0; i < algos.Length; i++)
                {
                    (int water, int itrs) = algos[i](pipes);
                    Console.WriteLine($"\nAlgorithm #{i + 1}\nWater: {water}\nIterations: {itrs}\n");
                }
            }
        }

        static void Render(int[] pipes)
        {
            int max = pipes.Max();
            for(int h = max; h >= 0; h--)
            {
                int lastPipe = -1;
                int row = Console.CursorTop;
                for(int x = 0; x < pipes.Length; x++)
                {
                    if(pipes[x] >= h)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.Write(' ');
                        if (lastPipe != -1)
                        {
                            Console.SetCursorPosition(lastPipe + 1, row);
                            Console.BackgroundColor = ConsoleColor.Red;
                            for (int j = lastPipe + 1; j < x; j++)
                            {
                                Console.Write(' ');
                            }
                            Console.SetCursorPosition(x + 1, row);
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        lastPipe = x;
                        continue;
                    }
                    Console.Write(' ');
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
            }
        }

        static (int, int) WaterAlgo1(int[] pipes)
        {
            int water = 0;
            int itrs = 0;
            int tempWater = 0;
            int max = pipes.Max();
            for(int h = max; h >= 0; h--)
            {
                tempWater = -1;
                for(int x = 0; x < pipes.Length; x++)
                {
                    itrs++;
                    if(pipes[x] >= h)
                    {
                        water += tempWater > 0 ? tempWater : 0;
                        tempWater = 0;
                        continue;
                    }
                    if(tempWater == -1)
                    {
                        continue;
                    }

                    tempWater++;
                }
            }
            return (water, itrs);
        }

        static (int, int) WaterAlgo2(int[] pipes)
        {
            int water = 0;
            int itrs = 0;
            for(int x = 0; x < pipes.Length; x++)
            {
                itrs++;
            }

            return (water, itrs);
        }
    }
}
