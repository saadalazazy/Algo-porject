using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ImageEncryptCompress
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

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
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void makeOperation_Click(object sender, EventArgs e)
        {
            RGBPixel[,] encryptionImage = ImageOperations.EncryptionImage(ImageMatrix, textBox1.Text, Int32.Parse(textBox2.Text), 8);
            ImageOperations.DisplayImage(encryptionImage, pictureBox2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<Dictionary<short, int>> rgbFreq = ImageOperations.CalculateColorFrequencies(ImageMatrix);
            string code = "";
            long[] total = new long[3];
            //Console.WriteLine("Red");
            ImageOperations.Node redRoot = ImageOperations.buildTree(rgbFreq[0]);
            ImageOperations.Node greenRoot = ImageOperations.buildTree(rgbFreq[1]);
            ImageOperations.Node blueRoot = ImageOperations.buildTree(rgbFreq[2]);
            ImageOperations.DFS(redRoot, code, ref total[0], 'r');
            //Console.WriteLine($"Total: {total[0]}");
            //Console.WriteLine("Green");
            ImageOperations.DFS(greenRoot, code, ref total[1], 'g');
            //Console.WriteLine($"Total: {total[1]}");
            //Console.WriteLine("Blue");
            ImageOperations.DFS(blueRoot, code, ref total[2], 'b');
            //Console.WriteLine($"Total: {total[2]}");
            double totalSum = 0;
            foreach (var it in total)
            {
                totalSum += it;
            }
            //Console.WriteLine($"Compression Output: {totalSum / 8} bytes");
            //ImageOperations.TableItration(ImageOperations.redHuffmanTable);
            //ImageOperations.TableItration(ImageOperations.greenHuffmanTable);
            //ImageOperations.TableItration(ImageOperations.blueHuffmanTable);
            //ImageOperations.imageItration(ImageMatrix);
            ImageOperations.imageItration(ImageOperations.compressedImage(ImageMatrix));
        }


    }
}