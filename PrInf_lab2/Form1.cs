using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;

namespace PrInf_lab2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int numBitsToHide = 160; // Количество бит, которые нужно внедрить
            int maxByteIndex = 431 * 378 * 3; // Общее количество байтов в BMP-файле

            // Генерация списка номеров байтов-контейнеров
            List<int> byteIndices = new List<int>();


            Random random = new();
            int temp = random.Next(122, maxByteIndex);
            for (int i = 0; i < numBitsToHide; i++)
            {
                while (byteIndices.Contains(temp))
                {
                    temp = random.Next(122, maxByteIndex);
                }
                byteIndices.Add(temp);

            }

            using (FileStream keyFileStream = new FileStream(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\key.txt", FileMode.Truncate))
            {
                using (StreamWriter writer = new StreamWriter(keyFileStream, Encoding.UTF8))
                {
                    for (int i = 0; i < numBitsToHide; i++)
                    {
                        writer.Write(byteIndices[i]);
                        writer.Write(" ");
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Считываем байты из BMP-файла
            byte[] bmpBytes = File.ReadAllBytes(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\28.bmp");

            // Считываем ключ из файла           
            string lines = File.ReadAllText(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\key.txt");
            lines = lines.Substring(0, lines.Length - 1);
            // Создаем массив для хранения чисел
            int[] keyBytes = Array.ConvertAll(lines.Split(' '), int.Parse);

            string hash = getHashSha1(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\leasing.txt");

            byte[] hashBytes;
            int[] bitValues;
            byte[] fileBytes = File.ReadAllBytes(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\leasing.txt");
            using (SHA1 sha1 = SHA1.Create())
            {
                hashBytes = sha1.ComputeHash(fileBytes);
                // Создание массива битов из массива байтов
                BitArray bits = new BitArray(hashBytes);

                // Преобразование массива битов в массив bool
                bitValues = new int[bits.Length];
                for (int i = 0; i < bitValues.Length/8; i++)
                {
                    for (int j = 7; j >= 0; j--)
                    {
                        bitValues[8 * i + j] = bits[8 * i + (7-j)] ? 1 : 0;
                    }
                }          
            }           


            // Заменяем LSB биты в байтах BMP-файла
            for (int i = 0; i < keyBytes.Length; i++)
            {
                int byteIndex = keyBytes[i];
                bmpBytes[byteIndex] = (byte)((bmpBytes[byteIndex] & 0xFE) | (bitValues[i] & 0x01));
            }
            // Сохраняем измененный BMP-файл
            File.WriteAllBytes(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\28_hidden.bmp", bmpBytes);

        }

        private string getHashSha1(string filePath)
        {
            string command = "openssl"; // имя приложения
            string arguments = $"dgst -sha1 {filePath}"; // аргументы

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = command;
            startInfo.Arguments = arguments;
            startInfo.RedirectStandardOutput = true; // перенаправляем стандартный вывод в процесс

            Process process = new();
            process.StartInfo = startInfo;
            process.Start();
            string a = process.StandardOutput.ReadToEnd().Replace($"SHA1({filePath})= ", "").Replace("\r\n", "");
            textBox1.Text += a + Environment.NewLine;
            process.WaitForExit();

            return a;


        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Считываем байты из BMP-файла
            byte[] bmpBytes = File.ReadAllBytes(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\28_hidden.bmp");
            // Считываем ключ из файла           
            string lines = File.ReadAllText(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\key.txt");
            lines = lines.Substring(0, lines.Length - 1);

            string itog = "";

            // Создаем массив для хранения чисел
            int[] keyBytes = Array.ConvertAll(lines.Split(' '), int.Parse);
            bool lastBit = false;
            for (int i = 0; i < keyBytes.Length; i++)
            {
                int byteIndex = keyBytes[i];
                lastBit = (bmpBytes[byteIndex] & 0x01) != 0;
                if (lastBit) { itog += "1"; } else { itog += "0"; }
            }
            textBox1.Text += itog+Environment.NewLine;

            byte[] hexHash = new byte[20]; // массив для 16-ричного хеш-кода

            for (int i = 0; i < 20; i++)
            {
                string byteString = itog.Substring(i * 8, 8); // выбираем 8 бит для каждого байта
                hexHash[i] = Convert.ToByte(byteString, 2); // конвертируем двоичное значение в байт
            }

            string hexHashString = BitConverter.ToString(hexHash).Replace("-", ""); // конвертируем в строку 16-ричных значений без разделителей
            textBox1.Text += hexHashString;
        }
    }
}