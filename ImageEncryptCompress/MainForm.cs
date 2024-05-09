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
        RGBPixel[,] compressImage;

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
        ImageOperations.Node redRoot , greenRoot , blueRoot;

        private void button1_Click(object sender, EventArgs e)
        {
            List<Dictionary<short, int>> rgbFreq = ImageOperations.CalculateColorFrequencies(ImageMatrix);
           /* foreach (var colorDict in rgbFreq)
            {
                foreach (var kvp in colorDict)
                {
                    short color = kvp.Key;
                    int frequency = kvp.Value;

                    // Do something with the color and its frequency
                    Console.WriteLine($"Color: {color}, Frequency: {frequency}");
                }
            }*/
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
           /* Console.WriteLine("Red");
            ImageOperations.TableItration(ImageOperations.redHuffmanTable);
            Console.WriteLine($"Total: {total[0]}");
            Console.WriteLine("Green");
            ImageOperations.TableItration(ImageOperations.greenHuffmanTable);
            Console.WriteLine($"Total: {total[1]}");
            Console.WriteLine("Blue");
            ImageOperations.TableItration(ImageOperations.blueHuffmanTable);
            Console.WriteLine($"Total: {total[2]}");
            Console.WriteLine($"Compression Output: {totalSum / 8} bytes");
            //ImageOperations.imageItration(ImageMatrix);
            Console.WriteLine("----------------------");
            //compressImage = ImageOperations.compressedImage(ImageMatrix);
            //ImageOperations.imageItration(compressImage);*/
            MessageBox.Show("compression done");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RGBPixel[,] compressImage = ImageOperations.decompressionImage(ImageOperations.compressedImage(ImageMatrix),
                redRoot, greenRoot, blueRoot);
            //ImageOperations.getColorFromHuffmanTree()
            ImageOperations.DisplayImage(compressImage , pictureBox2);

            MessageBox.Show("Decompretion done");
        }

 
    }
}