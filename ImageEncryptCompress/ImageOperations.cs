using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageEncryptCompress
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelStr
    {
        public string red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }
    
  
    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }
        
        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }
        public static int GetHeight(RGBPixelStr[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }
        public static int GetWidth(RGBPixelStr[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


       /// <summary>
       /// Apply Gaussian smoothing filter to enhance the edge detection 
       /// </summary>
       /// <param name="ImageMatrix">Colored image matrix</param>
       /// <param name="filterSize">Gaussian mask size</param>
       /// <param name="sigma">Gaussian sigma</param>
       /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];

           
            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }
        public struct key
        {
            public String keyRed, keyGreen, keyBlue;
        }
        public static key LFSR(String binary, int tap, int N, ref String lastBinary)
        {
            long initSeed = Convert.ToInt64(binary, 2);
            long returnValue;
            key password = new key();
            password.keyRed = "";
            password.keyGreen = "";
            password.keyBlue = "";
            for (int i = 0; i < (N * 3); i++)
            {
                returnValue = getValueBitPosition(initSeed, tap) ^ getValueBitPosition(initSeed, binary.Length - 1);
                initSeed = (initSeed << 1 | returnValue) & ((1 << binary.Length) - 1);
                if (i >= 0 && i < N)
                    password.keyRed = password.keyRed + returnValue.ToString();
                else if (i >= N && i < N * 2)
                    password.keyGreen = password.keyGreen + returnValue.ToString();
                else if (i >= N * 2 && i < N * 3)
                    password.keyBlue = password.keyBlue + returnValue.ToString();
            }
            lastBinary = Convert.ToString(initSeed, 2).PadLeft(binary.Length, '0');
            return password;
        }
        static long getValueBitPosition(long bits, int position)
        {
            bits = bits & (1 << position);
            bits = bits >> position;
            return bits;
        }
        public static RGBPixel[,] EncryptionImage(RGBPixel[,] ImageMatrix ,String binary, int tap, int N)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);
            RGBPixel[,] encryImage = new RGBPixel[Height, Width];
            String last = "";
            long initSead = Convert.ToInt64(binary, 2);
            if (initSead == 0)
            {
                return ImageMatrix;
            }    
            key passwords = LFSR(binary, tap, N, ref last);
            List<byte> keyValues = new List<byte>();
            for (int i = 0; i < Height;i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    encryImage[i, j].red = (byte)(ImageMatrix[i, j].red ^ Convert.ToByte(passwords.keyRed, 2));
                    encryImage[i, j].green = (byte)(ImageMatrix[i, j].green ^ Convert.ToByte(passwords.keyGreen, 2));
                    encryImage[i, j].blue = (byte)(ImageMatrix[i, j].blue ^ Convert.ToByte(passwords.keyBlue, 2));
                    passwords = LFSR(last, tap, N, ref last);
                }
            }
            return encryImage;
        }






        public static Dictionary<short, Tuple<long, string>> redHuffmanTable = new Dictionary<short, Tuple<long, string>>();
        public static Dictionary<short, Tuple<long, string>> greenHuffmanTable = new Dictionary<short, Tuple<long, string>>();
        public static Dictionary<short, Tuple<long, string>> blueHuffmanTable = new Dictionary<short, Tuple<long, string>>();
        public class Node
        {
            public short color = -1;
            public long freq;
            public Node left;
            public Node right;

            public Node(short color, long freq, Node left, Node right)
            {
                this.color = color;
                this.freq = freq;
                this.left = left;
                this.right = right;
            }
            public Node(long freq, Node left, Node right)
            {
                this.freq = freq;
                this.left = left;
                this.right = right;
            }
            public Node(short color, int freq)
            {
                this.color = color;
                this.freq = freq;
            }
        }
        class PriorityQueue
        {
            private List<Node> heap = new List<Node>();

            public int Count => heap.Count;

            private void HeapifyUp()
            {
                int index = heap.Count - 1;
                while (index > 0)
                {
                    int parentIndex = (index - 1) / 2;
                    if (heap[parentIndex].freq > heap[index].freq)
                    {
                        Swap(parentIndex, index);
                        index = parentIndex;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            private void HeapifyDown()
            {
                int index = 0;
                while (index < heap.Count)
                {
                    int leftChildIndex = 2 * index + 1;
                    int rightChildIndex = 2 * index + 2;
                    int smallestChildIndex = index;

                    if (leftChildIndex < heap.Count && heap[leftChildIndex].freq < heap[smallestChildIndex].freq)
                    {
                        smallestChildIndex = leftChildIndex;
                    }
                    if (rightChildIndex < heap.Count && heap[rightChildIndex].freq < heap[smallestChildIndex].freq)
                    {
                        smallestChildIndex = rightChildIndex;
                    }

                    if (smallestChildIndex != index)
                    {
                        Swap(smallestChildIndex, index);
                        index = smallestChildIndex;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            public void Enqueue(Node node)
            {
                heap.Add(node);
                HeapifyUp();
            }

            public Node Dequeue()
            {
                if (heap.Count == 0)
                    throw new InvalidOperationException("Priority queue is empty");

                Node minNode = heap[0];
                heap[0] = heap[heap.Count - 1];
                heap.RemoveAt(heap.Count - 1);
                HeapifyDown();
                return minNode;
            }

            private void Swap(int i, int j)
            {
                Node temp = heap[i];
                heap[i] = heap[j];
                heap[j] = temp;
            }
        }
        static public Node buildTree(Dictionary<short, int> colorFreq)
        {
            List<Node> nodes = new List<Node>();
            foreach (var it in colorFreq)
            {
                nodes.Add(new Node(it.Key, it.Value));
            }

            PriorityQueue priorityQueue = new PriorityQueue();
            foreach (var node in nodes)
            {
                priorityQueue.Enqueue(node);
            }

            while (priorityQueue.Count > 1)
            {
                Node right = priorityQueue.Dequeue();
                Node left = priorityQueue.Dequeue();
                long newFreq = left.freq + right.freq;
                Node parent = new Node(newFreq, left, right);
                priorityQueue.Enqueue(parent);
            }
            return priorityQueue.Dequeue();
        }
        public static void DFS(Node node, string code, ref long total , char color)
        {
            if (node == null)
                return;
            if (node.color != -1)
            {
                //Console.WriteLine($"Node color: {node.color} freq: {node.freq} code: {code}");
                if (color == 'r')
                {
                    redHuffmanTable.Add(node.color, new Tuple<long, string>(node.freq, code));
                }
                else if (color == 'g')
                {
                    greenHuffmanTable.Add(node.color, new Tuple<long, string>(node.freq, code));
                }
                else if (color == 'b')
                {
                    blueHuffmanTable.Add(node.color, new Tuple<long, string>(node.freq, code));
                }
                total += (code.Length * node.freq);

                return;
            }
            DFS(node.left,code + "0", ref total, color);
            DFS(node.right, code + "1", ref total, color);
        }
        public static List<Dictionary<short, int>> CalculateColorFrequencies(RGBPixel[,] image)
        {
            int x = GetWidth(image);
            int y = GetHeight(image);
            Dictionary<short, int> red = new Dictionary<short, int>();
            Dictionary<short, int> green = new Dictionary<short, int>();
            Dictionary<short, int> blue = new Dictionary<short, int>();
            List<Dictionary<short, int>> frequencies = new List<Dictionary<short, int>>();
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    short res = image[i, j].red;
                    if (red.ContainsKey(res))
                    {
                        red[res]++;
                    }
                    else
                    {
                        red.Add(res, 1);
                    }
                    res = image[i, j].green;
                    if (green.ContainsKey(res))
                    {
                        green[res]++;
                    }
                    else
                    {
                        green.Add(res, 1);
                    }
                    res = image[i, j].blue;
                    if (blue.ContainsKey(res))
                    {
                        blue[res]++;
                    }
                    else
                    {
                        blue.Add(res, 1);
                    }
                }
            }
            frequencies.Add(red);
            frequencies.Add(green);
            frequencies.Add(blue);
            return frequencies;
        }

        public static void TableItration(Dictionary<short, Tuple<long, string>> colorTable)
        {
            foreach(var it in colorTable)
            {
                Console.WriteLine($"color: {it.Key} freq: {it.Value.Item1} code: {it.Value.Item2}");
            }
        }

        public static RGBPixelStr[,] compressedImage(RGBPixel[,] image)
        {
            int hight = GetHeight(image);
            int width = GetWidth(image);
            RGBPixelStr[,] compimage = new RGBPixelStr[hight, width];
            for(int i = 0; i< hight; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    if(redHuffmanTable.ContainsKey(image[i, j].red))
                        compimage[i, j].red = redHuffmanTable[image[i, j].red].Item2;
                    if(greenHuffmanTable.ContainsKey(image[i, j].green))
                        compimage[i, j].green = greenHuffmanTable[image[i, j].green].Item2;
                    if(blueHuffmanTable.ContainsKey(image[i, j].blue))
                        compimage[i, j].blue = blueHuffmanTable[image[i, j].blue].Item2;
                }
            }
            return compimage;
        }
        public static void imageItration(RGBPixel[,] image)
        {
            int hight = GetHeight(image);
            int width = GetWidth(image);
            for (int i = 0; i < hight; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Console.Write($"{image[i, j].red}, {image[i, j].green}, {image[i, j].blue},");
                }
                Console.WriteLine("");
            }
        }
        public static RGBPixel[,] decompressionImage(RGBPixelStr [,] image , Node redRoot , Node greenRoot , Node blueRoot)
        {
            int hight = GetHeight(image);
            int width = GetWidth(image);
            RGBPixel[,] correctImage = new RGBPixel[hight,width];
            Dictionary<string, int> redTable = new Dictionary<string, int>();
            Dictionary<string, int> greenTable = new Dictionary<string, int>();
            Dictionary<string, int> blueTable = new Dictionary<string, int>();
            for (int i = 0; i < hight; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (redTable.ContainsKey(image[i, j].red))
                    {
                        correctImage[i, j].red = (byte)redTable[image[i, j].red];
                    }
                    else
                    {
                        byte correctColorRed = (byte)getColorFromHuffmanTree(image[i, j].red, redRoot);
                        redTable.Add(image[i, j].red, correctColorRed);
                        correctImage[i, j].red = correctColorRed;
                    }
                    if (greenTable.ContainsKey(image[i, j].green))
                    {
                        correctImage[i, j].green = (byte)greenTable[image[i, j].green];
                    }
                    else
                    {
                        byte correctColorGreen = (byte)getColorFromHuffmanTree(image[i, j].green, greenRoot);
                        greenTable.Add(image[i, j].green, correctColorGreen);
                        correctImage[i, j].green = correctColorGreen;
                    }
                    if (blueTable.ContainsKey(image[i, j].blue))
                    {
                        correctImage[i, j].blue = (byte)blueTable[image[i, j].blue];
                    }
                    else
                    {
                        byte correctColorBlue = (byte)getColorFromHuffmanTree(image[i, j].blue, blueRoot);
                        blueTable.Add(image[i, j].blue, correctColorBlue);
                        correctImage[i, j].blue = correctColorBlue;
                    }
                }
            }
            return correctImage;
        }
        public static short getColorFromHuffmanTree(string binaryCode, Node root)
        {
            int i = 0;
            Node currentNode = root;
            while(currentNode.color == -1)
            {
                if(binaryCode[i] == '1')
                {
                    currentNode = currentNode.right;
                }
                else
                {
                    currentNode = currentNode.left;
                }
                i++;
            }
            return currentNode.color;
        }

    }
}
