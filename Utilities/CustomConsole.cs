using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwentyFiveDotNet.Config;

namespace TwentyFiveDotNet.Utilities
{
    public static class CustomConsole
    {
        public static readonly String DevPrefix = "[DEV LOG]";

        // Wrapper for Console.WriteLine
        public static void WriteLine(string message, int delay)
        {
            Console.WriteLine(message);

            if (!GameConfig.DevMode)
            {
                Thread.Sleep(delay); // Introduce delay
            }
        }

        public static void WriteLine(string message)
        {
            Console.WriteLine(message);

            if (!GameConfig.DevMode)
            {
                Thread.Sleep(GameConfig._delayInMilliseconds); // Introduce delay
            }
        }

        public static void WriteLineNoDelay(string message)
        {
            Console.WriteLine(message);
        }

        public static void DevWriteLine(string message)
        {
            if (GameConfig.DevMode)
            {
                Console.WriteLine($"{DevPrefix} {message}");
                Thread.Sleep(GameConfig._delayInMilliseconds); // Introduce delay
            }
        }
        public static void DevWriteLineNoDelay(string message)
        {
            if (GameConfig.DevMode)
            {
                Console.WriteLine($"{DevPrefix} {message}");
            }
        }
        public static void DevWriteLineNoDelayNoPrefix(string message)
        {
            if (GameConfig.DevMode)
            {
                Console.WriteLine(message);
            }
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }

        public static void DevWriteLine()
        {
            if (GameConfig.DevMode)
            {
                Console.WriteLine();
            }
        }

        // Wrapper for Console.Write
        public static void Write(string message)
        {
            Console.Write(message);

            if (!GameConfig.DevMode)
            {
                Thread.Sleep(GameConfig._delayInMilliseconds); // Introduce delay
            }
        }

        public static void DevWriteNoDelay(string message)
        {
            if (GameConfig.DevMode)
            {
                Console.Write(message);
            }
            
        }

        public static void WaitForKeyPress()
        {
            Console.ReadKey(true); // Waits for a key press, but doesn't display the key on the console
        }

        public static void Clear()
        {
            Console.Clear();
        }

        public static void DevTagPrint()
        {
            if (GameConfig.DevMode) Console.Write($"{DevPrefix} ");
        }
    }
}
