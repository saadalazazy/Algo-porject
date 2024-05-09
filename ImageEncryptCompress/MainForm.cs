using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
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
            encryptionImage = ImageOperations.EncryptionImage(ImageMatrix, textBox1.Text, Int32.Parse(textBox2.Text), 8);
            ImageOperations.DisplayImage(encryptionImage, pictureBox2);
            checkBox2.Visible = true;
            checkBox1.Visible = true;
            isEncrypt = true;
            TimeSpan elapsedTime = stopwatch.Elapsed;
            MessageBox.Show($"encryption done with elapsedTime {elapsedTime}");
        }
        ImageOperations.Node redRoot , greenRoot , blueRoot;

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Dictionary<short, int>> rgbFreq = null;
            if (checkBox1.Checked == true || isEncrypt == false)
                rgbFreq = ImageOperations.CalculateColorFrequencies(ImageMatrix);
            else if(checkBox2.Checked == true)
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
            Console.WriteLine("-------------------------------");
            ImageOperations.DFS(greenRoot, code, ref total[1], 'g');
            Console.WriteLine("-------------------------------");
            ImageOperations.DFS(blueRoot, code, ref total[2], 'b');
            Console.WriteLine("-------------------------------");
            double totalSum = 0;
            foreach (var it in total)
            {
                totalSum += it;
            }
            
            button2.Visible = true;
            button1.Visible = false;
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            MessageBox.Show($"compression done with elapsedTime {elapsedTime} Compretion Image size: {totalSum / 8} byte");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            RGBPixel[,] compressImage = null;
            if (checkBox1.Checked == true || isEncrypt == false)
                compressImage = ImageOperations.decompressionImage(ImageOperations.compressedImage(ImageMatrix),
                redRoot, greenRoot, blueRoot);
            else if (checkBox2.Checked == true)
                compressImage = ImageOperations.decompressionImage(ImageOperations.compressedImage(encryptionImage),
                redRoot, greenRoot, blueRoot);


            ImageOperations.DisplayImage(compressImage , pictureBox2);
            TimeSpan elapsedTime = stopwatch.Elapsed;
            MessageBox.Show($"compression done with elapsedTime {elapsedTime}");
            button2.Visible = false;
        }

 
    }
}