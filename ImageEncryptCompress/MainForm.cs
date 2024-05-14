using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace ImageEncryptCompress
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        RGBPixel[,] encryptionImage;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
        }
        bool isEncrypt = false;
        private void makeOperation_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string newPass = ConvertToBinary(textBox3.Text);
            encryptionImage = ImageOperations.EncryptionImage(ImageMatrix, textBox3.Text, Int32.Parse(textBox2.Text), 8);
            ImageOperations.DisplayImage(encryptionImage, pictureBox2);
            checkBox2.Visible = true;
            checkBox1.Visible = true;
            isEncrypt = true;
            TimeSpan elapsedTime = stopwatch.Elapsed;
            MessageBox.Show($"encryption done with elapsedTime {elapsedTime}");
        }
        static string ConvertToBinary(string password)
        {
            StringBuilder binaryPassword = new StringBuilder();
            foreach (char c in password)
            {
                string binary = Convert.ToString(c, 2).PadLeft(8, '0');
                binaryPassword.Append(binary);
            }
            return binaryPassword.ToString();
        }
        ImageOperations.Node redRoot, greenRoot, blueRoot;


        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Dictionary<short, int>> rgbFreq = null;
            if (checkBox1.Checked == true || isEncrypt == false)
                rgbFreq = ImageOperations.CalculateColorFrequencies(ImageMatrix);
            else if (checkBox2.Checked == true)
            {
                rgbFreq = ImageOperations.CalculateColorFrequencies(encryptionImage);
                ImageOperations.DisplayImage(encryptionImage, pictureBox1);
            }

            redRoot = ImageOperations.buildTree(rgbFreq[0]);
            greenRoot = ImageOperations.buildTree(rgbFreq[1]);
            blueRoot = ImageOperations.buildTree(rgbFreq[2]);
            string code = "";
            long[] total = new long[3];
            ImageOperations.DFS(redRoot, code, ref total[0], 'r');
            ImageOperations.DFS(greenRoot, code, ref total[1], 'g');
            ImageOperations.DFS(blueRoot, code, ref total[2], 'b');
            double totalSum = 0;
            foreach (var it in total)
            {
                totalSum += it;
            }

            button2.Visible = true;
            button1.Visible = false;
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            string binaryFilePath = @"C:\\Users\\saada\\project\\algo project\\algo project\\test2.bin";
            using (FileStream fs = new FileStream(binaryFilePath, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    writer.Write(textBox3.Text);
                    writer.Write(textBox2.Text);
                    writer.Write(ImageOperations.Serialize(redRoot));
                    writer.Write(ImageOperations.Serialize(greenRoot));
                    writer.Write(ImageOperations.Serialize(blueRoot));
                    writer.Write(total[0].ToString());
                    writer.Write(total[1].ToString());
                    writer.Write(total[2].ToString());
                    writer.Write(ImageOperations.GetHeight(ImageMatrix).ToString());
                    writer.Write(ImageOperations.GetWidth(ImageMatrix).ToString());
                    if (checkBox1.Checked == true || isEncrypt == false)
                        ImageOperations.compressedImage(ImageMatrix, writer, total);
                    else if (checkBox2.Checked == true)
                        ImageOperations.compressedImage(encryptionImage, writer, total);
                }
            }
            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            MessageBox.Show($"compression done with elapsedTime {elapsedTime} Compretion Image size: {totalSum / 8 } byte");
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string binaryFilePath = @"C:\\Users\\saada\\project\\algo project\\algo project\\test2.bin";
            using (FileStream fs = new FileStream(binaryFilePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    string initSead = reader.ReadString();
                    string tap = reader.ReadString();
                    string redTree = reader.ReadString();
                    string greenTree = reader.ReadString();
                    string blueTree = reader.ReadString();
                    string redTotalBits = reader.ReadString();
                    string greenTotalBits = reader.ReadString();
                    string blueTotalBits = reader.ReadString();
                    string hight = reader.ReadString();
                    string width = reader.ReadString();
                    int offset = initSead.Length + tap.Length + redTree.Length + greenTree.Length + blueTree.Length + redTotalBits.Length
                        + greenTotalBits.Length + blueTotalBits.Length + width.Length + hight.Length;
                    reader.BaseStream.Seek(offset + 13, SeekOrigin.Begin);
                    long redTotalLength, greenTotalLength, blueTotalLength , imageWidth , imageHight;
                    long.TryParse(redTotalBits, out redTotalLength);
                    long.TryParse(greenTotalBits, out greenTotalLength);
                    long.TryParse(blueTotalBits, out blueTotalLength);
                    long.TryParse(hight, out imageHight);
                    long.TryParse(width, out imageWidth);
                    long fileLength = (redTotalLength + greenTotalLength + blueTotalLength) / 8;
                    if ((redTotalLength + greenTotalLength + blueTotalLength) / 8 != 0)
                        fileLength++;

                    byte[] dataRead = reader.ReadBytes((int)(fileLength));
                    redRoot = ImageOperations.DeserializeHuffmanTree(redTree);
                    greenRoot = ImageOperations.DeserializeHuffmanTree(greenTree);
                    blueRoot = ImageOperations.DeserializeHuffmanTree(blueTree);
                    long redStart, redEnd, greenStart, greenEnd, blueStart, blueEnd;
                    redStart = 0;
                    if (redTotalLength % 8 != 0)
                        redEnd = (redTotalLength / 8) + 1;
                    else
                        redEnd = (redTotalLength / 8);
                    greenStart = redEnd;
                    if (greenTotalLength % 8 != 0)
                        greenEnd =  redEnd +(greenTotalLength / 8) + 1;
                    else
                        greenEnd = redEnd + (greenTotalLength / 8);
                    blueStart = greenEnd;
                    if (blueTotalLength % 8 != 0)
                    {
                        blueEnd = greenEnd + (blueTotalLength / 8) + 1;
                    }
                    else
                    {
                        blueEnd = greenEnd + (blueTotalLength / 8);
                    }
                    StringBuilder redCode = new StringBuilder();
                    for (long i = redStart; i < redEnd; i++)
                    {
                        redCode.Append(Convert.ToString(dataRead[i], 2).PadLeft(8, '0'));
                    }
                    string resultRedCode = redCode.ToString();
                    StringBuilder greenCode = new StringBuilder();
                    for (long i = greenStart; i < greenEnd; i++)
                    {
                        greenCode.Append(Convert.ToString(dataRead[i], 2).PadLeft(8, '0'));
                    }
                    string resultGreenCode = greenCode.ToString();
                    Console.WriteLine(dataRead.Length);
                    Console.WriteLine(blueEnd);
                    StringBuilder blueCode = new StringBuilder();
                    for (long i = blueStart; i < blueEnd && i < dataRead.Length; i++)
                    {
                        blueCode.Append(Convert.ToString(dataRead[i], 2).PadLeft(8, '0'));
                    }
                    string resultBlueCode = blueCode.ToString();
                    RGBPixel[,]newImage = new RGBPixel[imageHight, imageWidth];
                    ImageOperations.getColorFromHuffmanTree(newImage,resultRedCode, redRoot, redTotalLength , imageHight , imageWidth , 'R');
                    ImageOperations.getColorFromHuffmanTree(newImage,resultGreenCode, greenRoot, greenTotalLength, imageHight, imageWidth, 'G');
                    ImageOperations.getColorFromHuffmanTree(newImage,resultBlueCode, blueRoot, blueTotalLength, imageHight, imageWidth, 'B');
                    ImageOperations.DisplayImage(newImage, pictureBox2);
                    TimeSpan elapsedTime = stopwatch.Elapsed;
                    MessageBox.Show($"compression done with elapsedTime {elapsedTime}");
                }
            }
            

        }
    }
}
                    /*string reamningBits = Convert.ToString(dataRead[(redTotalLength / 8)], 2).PadLeft(8, '0');
                    for (int i = 0; i < (redTotalLength - (redTotalLength / 8) * 8); i++)
                    {
                        redCode.Append(reamningBits[i]);
                    }*/