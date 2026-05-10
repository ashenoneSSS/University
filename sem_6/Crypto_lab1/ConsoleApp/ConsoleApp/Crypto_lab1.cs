using System;
using System.Text;

namespace Adler32Implementation
{
    internal class Crypto_lab1
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            while (true)
            {
                Console.WriteLine("Adler-32");
                Console.WriteLine();
                Console.WriteLine("1. Хешувати текст");
                Console.WriteLine("2. Тестовий приклад");
                Console.WriteLine("0. Вихід");
                Console.WriteLine();

                Console.Write("Ваш вибір: ");
                string choice = Console.ReadLine();

                Console.WriteLine();

                if (choice == "1")
                {
                    HashText();
                }
                else if (choice == "2")
                {
                    RunTest();
                }
                else if (choice == "0")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Невірний вибір");
                }

                Console.WriteLine();
                Console.WriteLine("Натисніть будь-яку клавішу...");
                Console.ReadKey();
                Console.Clear();
            }
        }


        static void HashText()
        {
            Console.Write("Введіть текст: ");

            string input = Console.ReadLine();

            byte[] data = Encoding.UTF8.GetBytes(input);

            uint result = Adler32(data);

            PrintResult(result);
        }


        static void RunTest()
        {
            string test = "Wikipedia";

            byte[] data = Encoding.ASCII.GetBytes(test);

            uint result = Adler32(data);

            Console.WriteLine("Тестовий текст:");
            Console.WriteLine(test);

            Console.WriteLine();

            PrintResult(result);
        }


        static uint Adler32(byte[] data)
        {
            const uint modAdler = 65521;

            uint a = 1;
            uint b = 0;


            for (int i = 0; i < data.Length; i++)
            {
                a = (a + data[i]) % modAdler;

                b = (b + a) % modAdler;
            }


            return (b << 16) | a;
        }


        static void PrintResult(uint result)
        {
            Console.WriteLine();

            Console.WriteLine("Adler-32 у десятковому вигляді:");
            Console.WriteLine(result);

            Console.WriteLine();

            Console.WriteLine("Adler-32 у HEX:");
            Console.WriteLine(result.ToString("X8"));
        }
    }
}