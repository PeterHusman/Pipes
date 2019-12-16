using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pipes
{

    public interface IDescriptor
    {
        string Description { get; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    class HelpDescriptorAttribute : Attribute, IDescriptor
    {
        public string Description { get; }

        public HelpDescriptorAttribute(string description)
        {
            Description = description;
        }
    }

    enum ConsoleCommands
    {
        [HelpDescriptor("Lists the contents of the current directory.")]
        ls,

        [HelpDescriptor("Prints out the contents of a file given as an input parameter.")]
        cat,

        [HelpDescriptor("Deletes a file. Be careful with this one!")]
        rm,

        [HelpDescriptor("Copies a file. Very useful.")]
        cp
    }


    public class ConsoleHelper<TData, TAttribute>
        where TData : Enum
        where TAttribute : IDescriptor
    {
        public Dictionary<TData, string> HelpDescriptions { get; }

        public ConsoleHelper()
        {
            HelpDescriptions = typeof(TData).GetFields().Where(field => Attribute.IsDefined(field, typeof(TAttribute)))
                                            .Select(field => (name: (TData)Enum.Parse(typeof(TData), field.Name), description: (field.GetCustomAttribute(typeof(TAttribute)) as IDescriptor).Description))
                                            .ToDictionary(pair => pair.name, pair => pair.description);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class WaterAlgorithmAttribute : Attribute
    {
        public WaterAlgorithmAttribute()
        {

        }
    }

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
            (Func<int[], (int, int)> func, string name)[] methods = typeof(Program).GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Where(a => Attribute.IsDefined(a, typeof(WaterAlgorithmAttribute))).Select(method => ((Func<int[], (int, int)>)method.CreateDelegate(typeof(Func<int[], (int, int)>)), method.Name)).ToArray();
            //Func<int[], (int, int)>[] algos = { WaterAlgo1, WaterAlgo2 };
            while (true)
            {
                int size = Prompt("Width: ", FancyNaturalNumParse);
                int maxHeight = Prompt("Height: ", FancyNaturalNumParse);
                int[] pipes = new int[size];
                for (int i = 0; i < size; i++)
                {
                    pipes[i] = random.Next(1, maxHeight + 1);
                }
                Console.WriteLine();
                if (size < Console.BufferWidth)
                {
                    Render(pipes);
                }

                for (int i = 0; i < methods.Length; i++)
                {
                    (int water, int itrs) = methods[i].func(pipes);
                    Console.WriteLine($"\n{methods[i].name}\nWater: {water}\nIterations: {itrs}");
                }
                Console.WriteLine();
            }
        }

        static void Render(int[] pipes)
        {
            int max = pipes.Max();
            for (int h = max; h >= 0; h--)
            {
                int lastPipe = -1;
                int row = Console.CursorTop;
                for (int x = 0; x < pipes.Length; x++)
                {
                    if (pipes[x] >= h)
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
                        }
                        Console.BackgroundColor = ConsoleColor.Black;
                        lastPipe = x;
                        continue;
                    }
                    Console.Write(' ');
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
            }
        }

        /// <summary>
        /// WIP, non-functional
        /// </summary>
        /// <param name="pipes"></param>
        /// <param name="translation"></param>
        /// <param name="maxWidth"></param>
        static void TranslatedRender(int[] pipes, int translation, int maxWidth)
        {
            int maxHeight = pipes.Max();
            int maxX = maxWidth + translation;
            for(int h = maxHeight; h >= 0; h--)
            {
                int lastPipe = -1;
                int row = Console.CursorTop;
                for(int x = 0; x < pipes.Length; x++)
                {
                    if (pipes[x] >= h)
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
                        }
                        Console.BackgroundColor = ConsoleColor.Black;
                        lastPipe = x;
                        continue;
                    }
                    Console.Write(' ');
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
            }
        }

        [WaterAlgorithm]
        static (int, int) WaterAlgo1(int[] pipes)
        {
            int water = 0;
            int itrs = 0;
            int tempWater = 0;
            int max = pipes.Max();
            for (int h = max; h >= 0; h--)
            {
                tempWater = -1;
                for (int x = 0; x < pipes.Length; x++)
                {
                    itrs++;
                    if (pipes[x] >= h)
                    {
                        water += tempWater > 0 ? tempWater : 0;
                        tempWater = 0;
                        continue;
                    }
                    if (tempWater == -1)
                    {
                        continue;
                    }

                    tempWater++;
                }
            }
            return (water, itrs);
        }

        [WaterAlgorithm]
        static (int, int) WaterAlgo2(int[] pipes)
        {
            int water = 0;
            int itrs = 0;
            int stackPtr = 0;
            int highestEncountered = pipes[0];
            (int x, int y)[] downSteps = new (int, int)[pipes.Length];
            for (int x = 1; x < pipes.Length; x++)
            {

                // Step down
                if (pipes[x] < pipes[x - 1])
                {
                    downSteps[stackPtr] = (x, pipes[x]);
                    stackPtr++;
                }

                // Step up
                else if (stackPtr > 0 && pipes[x] > pipes[x - 1])
                {
                    itrs--;
                    int levelToFillTo = 0;
                    do
                    {
                        itrs++;
                        levelToFillTo = Math.Min(Math.Min(pipes[x], highestEncountered), pipes[downSteps[stackPtr - 1].x - 1]);
                        water += (x - downSteps[stackPtr - 1].x) * (levelToFillTo - downSteps[stackPtr - 1].y);
                        downSteps[stackPtr - 1].y = levelToFillTo;
                        if (downSteps[stackPtr - 1].y == highestEncountered || (stackPtr > 1 && downSteps[stackPtr - 1].y == downSteps[stackPtr - 2].y))
                        {
                            stackPtr--;
                        }
                    } while (stackPtr > 0 && Math.Min(pipes[x], highestEncountered) > downSteps[stackPtr - 1].y);
                }

                if (pipes[x] > highestEncountered)
                {
                    highestEncountered = pipes[x];
                }

                itrs++;
            }

            return (water, itrs);
        }
    }
}
