using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;


namespace ImageEncryptCompress
{
    public class LFSROperations
    {
        public struct key
        {
            public String keyRed, keyGreen, keyBlue;
        }
        public static key LFSR(String binary, int tap, int N, ref String lastBinary)
        {
            int initSeed = Convert.ToInt32(binary, 2);
            int returnValue;
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
        static int getValueBitPosition(int bits, int position)
        {
            bits = bits & (1 << position);
            bits = bits >> position;
            return bits;
        }
    }

}