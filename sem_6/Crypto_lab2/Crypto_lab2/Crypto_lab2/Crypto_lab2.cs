using System;
using System.Text;

namespace RC5Implementation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            while (true)
            {
                Console.WriteLine("RC5");
                Console.WriteLine();
                Console.WriteLine("1. Зашифрувати і розшифрувати текст");
                Console.WriteLine("0. Вихід");
                Console.WriteLine();

                Console.Write("Ваш вибір: ");
                string choice = Console.ReadLine();

                Console.WriteLine();

                if (choice == "1")
                {
                    RunRC5();
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


        static void RunRC5()
        {
            Console.Write("Введіть текст: ");
            string text = Console.ReadLine();

            Console.Write("Введіть ключ: ");
            string keyText = Console.ReadLine();

            if (string.IsNullOrEmpty(keyText))
            {
                Console.WriteLine("Ключ не може бути порожнім");
                return;
            }

            byte[] key = Encoding.UTF8.GetBytes(keyText);

            RC5 rc5 = new RC5(key);

            byte[] plainData = Encoding.UTF8.GetBytes(text);

            byte[] encryptedData = rc5.Encrypt(plainData);

            byte[] decryptedData = rc5.Decrypt(encryptedData);

            string decryptedText = Encoding.UTF8.GetString(decryptedData);

            Console.WriteLine();

            Console.WriteLine("Зашифрований текст у HEX:");
            Console.WriteLine(BytesToHex(encryptedData));

            Console.WriteLine();

            Console.WriteLine("Розшифрований текст:");
            Console.WriteLine(decryptedText);
        }


        static string BytesToHex(byte[] data)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                result.Append(data[i].ToString("X2"));
            }

            return result.ToString();
        }
    }


    internal class RC5
    {
        private const int WordSize = 32;
        private const int Rounds = 12;
        private const int BytesPerWord = WordSize / 8;
        private const int BlockSize = 8;
        private const int SubKeyCount = 2 * (Rounds + 1);

        private const uint P = 0xB7E15163;
        private const uint Q = 0x9E3779B9;

        private readonly uint[] S;


        public RC5(byte[] key)
        {
            S = GenerateSubKeys(key);
        }


        public byte[] Encrypt(byte[] data)
        {
            byte[] paddedData = AddPadding(data);

            byte[] result = new byte[paddedData.Length];

            for (int i = 0; i < paddedData.Length; i += BlockSize)
            {
                EncryptBlock(paddedData, i, result, i);
            }

            return result;
        }


        public byte[] Decrypt(byte[] data)
        {
            if (data.Length % BlockSize != 0)
            {
                throw new Exception("Неправильна довжина зашифрованих даних");
            }

            byte[] result = new byte[data.Length];

            for (int i = 0; i < data.Length; i += BlockSize)
            {
                DecryptBlock(data, i, result, i);
            }

            return RemovePadding(result);
        }


        private uint[] GenerateSubKeys(byte[] key)
        {
            unchecked
            {
                int c = key.Length / BytesPerWord;

                if (key.Length % BytesPerWord != 0)
                {
                    c++;
                }

                uint[] L = new uint[c];

                for (int i = key.Length - 1; i >= 0; i--)
                {
                    L[i / BytesPerWord] = (L[i / BytesPerWord] << 8) + key[i];
                }

                uint[] subKeys = new uint[SubKeyCount];

                subKeys[0] = P;

                for (int i = 1; i < SubKeyCount; i++)
                {
                    subKeys[i] = subKeys[i - 1] + Q;
                }

                uint A = 0;
                uint B = 0;

                int x = 0;
                int y = 0;

                int iterations = 3 * Math.Max(SubKeyCount, c);

                for (int i = 0; i < iterations; i++)
                {
                    subKeys[x] = RotateLeft(subKeys[x] + A + B, 3);
                    A = subKeys[x];

                    L[y] = RotateLeft(L[y] + A + B, (int)(A + B));
                    B = L[y];

                    x = (x + 1) % SubKeyCount;
                    y = (y + 1) % c;
                }

                return subKeys;
            }
        }


        private void EncryptBlock(byte[] input, int inputOffset, byte[] output, int outputOffset)
        {
            unchecked
            {
                uint A = BytesToUInt(input, inputOffset);
                uint B = BytesToUInt(input, inputOffset + 4);

                A = A + S[0];
                B = B + S[1];

                for (int i = 1; i <= Rounds; i++)
                {
                    A = RotateLeft(A ^ B, (int)B) + S[2 * i];
                    B = RotateLeft(B ^ A, (int)A) + S[2 * i + 1];
                }

                UIntToBytes(A, output, outputOffset);
                UIntToBytes(B, output, outputOffset + 4);
            }
        }


        private void DecryptBlock(byte[] input, int inputOffset, byte[] output, int outputOffset)
        {
            unchecked
            {
                uint A = BytesToUInt(input, inputOffset);
                uint B = BytesToUInt(input, inputOffset + 4);

                for (int i = Rounds; i >= 1; i--)
                {
                    B = RotateRight(B - S[2 * i + 1], (int)A) ^ A;
                    A = RotateRight(A - S[2 * i], (int)B) ^ B;
                }

                B = B - S[1];
                A = A - S[0];

                UIntToBytes(A, output, outputOffset);
                UIntToBytes(B, output, outputOffset + 4);
            }
        }


        private static uint RotateLeft(uint value, int shift)
        {
            int s = shift & 31;

            return (value << s) | (value >> ((32 - s) & 31));
        }


        private static uint RotateRight(uint value, int shift)
        {
            int s = shift & 31;

            return (value >> s) | (value << ((32 - s) & 31));
        }


        private static uint BytesToUInt(byte[] data, int offset)
        {
            return (uint)(data[offset] |
                         data[offset + 1] << 8 |
                         data[offset + 2] << 16 |
                         data[offset + 3] << 24);
        }


        private static void UIntToBytes(uint value, byte[] data, int offset)
        {
            data[offset] = (byte)value;
            data[offset + 1] = (byte)(value >> 8);
            data[offset + 2] = (byte)(value >> 16);
            data[offset + 3] = (byte)(value >> 24);
        }


        private static byte[] AddPadding(byte[] data)
        {
            int padding = BlockSize - data.Length % BlockSize;

            byte[] result = new byte[data.Length + padding];

            Array.Copy(data, result, data.Length);

            for (int i = data.Length; i < result.Length; i++)
            {
                result[i] = (byte)padding;
            }

            return result;
        }


        private static byte[] RemovePadding(byte[] data)
        {
            int padding = data[data.Length - 1];

            if (padding < 1 || padding > BlockSize)
            {
                throw new Exception("Неправильне доповнення даних");
            }

            for (int i = data.Length - padding; i < data.Length; i++)
            {
                if (data[i] != padding)
                {
                    throw new Exception("Неправильне доповнення даних");
                }
            }

            byte[] result = new byte[data.Length - padding];

            Array.Copy(data, result, result.Length);

            return result;
        }
    }
}