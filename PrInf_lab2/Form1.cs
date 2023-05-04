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
            int numBitsToHide = 160; // ���������� ���, ������� ����� ��������
            int maxByteIndex = 431 * 378 * 3; // ����� ���������� ������ � BMP-�����

            // ��������� ������ ������� ������-�����������
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
            // ��������� ����� �� BMP-�����
            byte[] bmpBytes = File.ReadAllBytes(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\28.bmp");

            // ��������� ���� �� �����           
            string lines = File.ReadAllText(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\key.txt");
            lines = lines.Substring(0, lines.Length - 1);
            // ������� ������ ��� �������� �����
            int[] keyBytes = Array.ConvertAll(lines.Split(' '), int.Parse);

            string hash = getHashSha1(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\leasing.txt");

            byte[] hashBytes;
            int[] bitValues;
            byte[] fileBytes = File.ReadAllBytes(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\leasing.txt");
            using (SHA1 sha1 = SHA1.Create())
            {
                hashBytes = sha1.ComputeHash(fileBytes);
                // �������� ������� ����� �� ������� ������
                BitArray bits = new BitArray(hashBytes);

                // �������������� ������� ����� � ������ bool
                bitValues = new int[bits.Length];
                for (int i = 0; i < bitValues.Length/8; i++)
                {
                    for (int j = 7; j >= 0; j--)
                    {
                        bitValues[8 * i + j] = bits[8 * i + (7-j)] ? 1 : 0;
                    }
                }          
            }           


            // �������� LSB ���� � ������ BMP-�����
            for (int i = 0; i < keyBytes.Length; i++)
            {
                int byteIndex = keyBytes[i];
                bmpBytes[byteIndex] = (byte)((bmpBytes[byteIndex] & 0xFE) | (bitValues[i] & 0x01));
            }
            // ��������� ���������� BMP-����
            File.WriteAllBytes(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\28_hidden.bmp", bmpBytes);

        }

        private string getHashSha1(string filePath)
        {
            string command = "openssl"; // ��� ����������
            string arguments = $"dgst -sha1 {filePath}"; // ���������

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = command;
            startInfo.Arguments = arguments;
            startInfo.RedirectStandardOutput = true; // �������������� ����������� ����� � �������

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
            // ��������� ����� �� BMP-�����
            byte[] bmpBytes = File.ReadAllBytes(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\28_hidden.bmp");
            // ��������� ���� �� �����           
            string lines = File.ReadAllText(@"C:\VSProjects\PrInf_lab2\PrInf_lab2\Resources\key.txt");
            lines = lines.Substring(0, lines.Length - 1);

            string itog = "";

            // ������� ������ ��� �������� �����
            int[] keyBytes = Array.ConvertAll(lines.Split(' '), int.Parse);
            bool lastBit = false;
            for (int i = 0; i < keyBytes.Length; i++)
            {
                int byteIndex = keyBytes[i];
                lastBit = (bmpBytes[byteIndex] & 0x01) != 0;
                if (lastBit) { itog += "1"; } else { itog += "0"; }
            }
            textBox1.Text += itog+Environment.NewLine;

            byte[] hexHash = new byte[20]; // ������ ��� 16-������� ���-����

            for (int i = 0; i < 20; i++)
            {
                string byteString = itog.Substring(i * 8, 8); // �������� 8 ��� ��� ������� �����
                hexHash[i] = Convert.ToByte(byteString, 2); // ������������ �������� �������� � ����
            }

            string hexHashString = BitConverter.ToString(hexHash).Replace("-", ""); // ������������ � ������ 16-������ �������� ��� ������������
            textBox1.Text += hexHashString;
        }
    }
}