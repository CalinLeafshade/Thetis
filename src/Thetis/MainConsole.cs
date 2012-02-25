using System;
using System.Collections.Generic;
using System.Text;

namespace Thetis
{
    public class ConsoleLine
    {
        public String Line;
        public ConsoleColor Color;
        
        public ConsoleLine(string line, ConsoleColor color)
        {
            this.Line = line;
            this.Color = color;
        }

        public override string ToString()
        {
            return Line;
        }

        public void Print()
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = Color;
            Console.WriteLine(Line);
            Console.ForegroundColor = old;
        }
    }

    public class MainConsole
    {

        List<ConsoleLine> lines = new List<ConsoleLine>();
        
        int topLine;
        string currentInput = "";

        public MainConsole()
        {

        }

        public void Init()
        {

            Console.Clear();
            
        }

        int linesNeeded(String line)
        {
            double d = (double)line.Length / Console.BufferWidth;
            return (int)Math.Ceiling(d);
        }

        String statusLine()
        {
            return String.Format("{0} Lines - Position {1}", lines.Count, topLine);
        }

        void RefreshConsole(bool justInput = false)
        {
            Console.BufferHeight = Console.WindowHeight;
            if (!justInput)
            {
                Console.Clear();
                int line = 1;
                for (int i = topLine; i >= 0; i--)
                {
                    line += linesNeeded(lines[i].Line);
                    if (line > Console.WindowHeight) break;
                    Console.SetCursorPosition(0, Console.WindowHeight - line);
                    lines[i].Print();

                }
                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.DarkGray;
                String status = statusLine();
                int padding = (Console.WindowWidth - status.Length) / 2;
                status = status.PadLeft(padding + status.Length);
                status = status.PadRight(Console.WindowWidth);
                Console.WriteLine(status);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
            
            Console.SetCursorPosition(0, Console.BufferHeight - 1);
            String blank = "";
            blank = blank.PadLeft(Console.WindowWidth);
            Console.Write(blank);
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write(":{0}", currentInput);
            
        }

        public void WriteLine(String line, params object[] obj)
        {
            WriteLine(ConsoleColor.DarkGray, line, obj);
        }

        public void WriteLine(ConsoleColor color, String line, params object[] obj)
        {
            if (obj != null) line = String.Format(line, obj);
            lines.Add(new ConsoleLine(line,color));
            topLine = lines.Count - 1;
            RefreshConsole();
        }

        public String ReadLine()
        {
            bool done = false;
            String toReturn = "";
            while (!done)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Enter)
                {
                    toReturn = currentInput;
                    currentInput = "";
                    RefreshConsole();
                    done = true;

                }
                else if (cki.Key == ConsoleKey.Backspace)
                {
                    if (currentInput.Length == 0) Console.Beep();
                    else currentInput = currentInput.Remove(currentInput.Length - 1);
                    RefreshConsole();
                }
                else if (cki.Key == ConsoleKey.PageUp)
                {
                    topLine = Math.Max(topLine - 1, 0);
                    RefreshConsole();
                }
                else if (cki.Key == ConsoleKey.PageDown)
                {
                    topLine = Math.Min(topLine + 1, lines.Count - 1);
                    RefreshConsole();
                }
                else
                {
                    currentInput += cki.KeyChar;
                    RefreshConsole(true);
                }
            }
            return toReturn;
        }


    }
}
