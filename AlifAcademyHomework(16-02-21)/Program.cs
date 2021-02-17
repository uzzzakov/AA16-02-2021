using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlifAcademyHomework_16_02_21_
{
    public class Program
    {
        public static class Matrix
        {
            public const int Width = 130;
            public const int Height = 30;
            public const int Lines = 50;
            public const int StartDelay = 10;

            public static async Task Start()
            {
                var tasks = new List<Task>();
                for (var a = 0; a < Lines; a++)
                {
                    var task = Task.Run(ContourLine);
                    tasks.Add(task);
                    await Task.Delay(StartDelay);
                }
            }

            private static async Task ContourLine()
            {
                while (true)
                {
                    var column = RandomHelper.Rand(0, Width);
                    await FallingLine.StartNew(column);
                }
            }
        }

        public class FallingLine
        {            
            private string symbs = "@#%$^!*/+-=?~abcdefghijklmnopqrstuvwxyz1234567890";
            private int column;
            private int row;
            private int length;
            private int minLength = 5;
            private int maxLength = 23;
            private int minUpdateTime = 15;
            private int maxUpdateTime = 60;            
            private char previous1 = ' ';
            private char previous2 = ' ';
            private int updateTime;

            private FallingLine(int column)
            {
                length = RandomHelper.Rand(minLength, maxLength + 1);
                updateTime = RandomHelper.Rand(minUpdateTime, maxUpdateTime + 1);
                this.column = column;
            }

            public static async Task StartNew(int column)
            {
                var n = new FallingLine(column);
                await n.Start();
            }

            private async Task Start()
            {
                for (var a = 0; a < Matrix.Height + length; a++)
                {
                    Step();
                    await Task.Delay(updateTime);
                }
            }

            private static bool InBorders(int row)
            {
                return row > 0 && row < Matrix.Height;
            }

            private void Step()
            {
                if (InBorders(row))
                {
                    var symbol = symbs[RandomHelper.Rand(0, symbs.Length)];
                    ConsoleHelper.Display(new ConsoleTask(column, row, symbol, ConsoleColor.White));
                    previous1 = symbol;
                }

                if (InBorders(row - 1))
                {
                    ConsoleHelper.Display(new ConsoleTask(column, row - 1, previous1, ConsoleColor.Green));
                    previous2 = previous1;
                }

                if (InBorders(row - 2))
                {
                    ConsoleHelper.Display(new ConsoleTask(column, row - 2, previous2, ConsoleColor.DarkGreen));
                }

                if (InBorders(row - length))
                {
                    ConsoleHelper.Display(new ConsoleTask(column, row - length, ' ', ConsoleColor.Black));
                }

                row++;
            }
        }

        public class ConsoleTask
        {
            public readonly ConsoleColor Color;
            public readonly int Column;
            public readonly int Row;
            public readonly char Symbol;

            public ConsoleTask(int column, int row, char symbol, ConsoleColor color)
            {
                Color = color;
                Column = column;
                Row = row;
                Symbol = symbol;
            }
        }

        public static class ConsoleHelper
        {
            private static readonly ConcurrentQueue<ConsoleTask> Queue = new ConcurrentQueue<ConsoleTask>();
            private static bool _inProcess;

            static ConsoleHelper()
            {
                Console.CursorVisible = false;
                Console.OutputEncoding = Encoding.UTF8;
            }

            public static void Display(ConsoleTask task)
            {
                Queue.Enqueue(task);
                DisplayCore();
            }

            private static void DisplayCore()
            {
                while (true)
                {
                    if (_inProcess)
                    {
                        return;
                    }

                    lock (Queue)
                    {
                        if (_inProcess)
                        {
                            return;
                        }

                        _inProcess = true;
                    }

                    while (Queue.TryDequeue(out var task))
                    {
                        Console.SetCursorPosition(task.Column, task.Row);
                        Console.ForegroundColor = task.Color;
                        Console.Write(task.Symbol);
                    }

                    lock (Queue)
                    {
                        _inProcess = false;
                        if (!Queue.IsEmpty)
                        {
                            continue;
                        }
                    }
                    break;
                }
            }
        }

        public static class RandomHelper
        {
            private static int goOff = Environment.TickCount;

            private static readonly ThreadLocal<Random> Random =
                    new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref goOff)));

            public static int Rand(int min, int max)
            {
                return Random.Value.Next(min, max);
            }
        }
        public static void Main()
        {
            var task = Task.Run(Matrix.Start);

            Console.ReadKey();
        }
    }
}
