using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace steg_poc
{
    class Stega
    {
        public Stega() { }

        public void EncodeImage(byte[] encodeFile)
        {
            EncodeImage(encodeFile, new string[] { @"..\..\testpic.png" });
        }

        public void EncodeImage(byte[] encodeFile, string[] imagePool)
        {
            try
            {
                int idx = 0;
                int imagePoolIdx = 0;
                foreach (string image in imagePool)
                {
                    Bitmap bitmap = new Bitmap(image);

                    // Lock the bitmap's bits.  
                    Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite,
                        bitmap.PixelFormat);

                    // Get the address of the first line.
                    IntPtr ptr = bitmapData.Scan0;

                    // Declare an array to hold the bytes of the bitmap.
                    int bytes = Math.Abs(bitmapData.Stride) * bitmap.Height;
                    byte[] rgbValues = new byte[bytes];

                    // Copy the RGB values into the array. 
                    Marshal.Copy(ptr, rgbValues, 0, bytes);

                    int rgbArrayIdx = 0;
                    int offset = rgbValues.Length - (rgbValues.Length % 9);
                    while (idx < encodeFile.Length)
                    {
                        var bits = new BitArray(new byte[] { encodeFile[idx] });
                        foreach (bool bit in bits)
                        {
                            rgbValues[rgbArrayIdx] = (byte)EncodeRgbValue(bit, rgbValues[rgbArrayIdx]);
                            rgbArrayIdx++;
                        }

                        rgbValues[rgbArrayIdx] =
                            (byte)EncodeRgbValue(idx != encodeFile.Length - 1 ? true : false,
                            rgbValues[rgbArrayIdx]);
                        rgbArrayIdx++;

                        if (idx == encodeFile.Length - 1 || rgbArrayIdx + 1 == offset)
                        {
                            SaveBitmap(bitmap, bitmapData, ptr, rgbValues, bytes, imagePoolIdx);
                            break;
                        }

                        idx++;
                    }

                    imagePoolIdx++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                // Console.WriteLine(rgbArrayIdx);
            }
        }

        private void SaveBitmap(Bitmap bitmap, BitmapData bitmapData, IntPtr ptr, 
            byte[] rgbValues, int bytes, int imagePoolIdx)
        {
            // Copy the RGB values back to the bitmap
            Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bitmap.UnlockBits(bitmapData);
            bitmap.Save(@"/home/ghost/Projects/steg_poc/steg_poc/output_" + imagePoolIdx.ToString() + ".png", ImageFormat.Png);
        }

        private int EncodeRgbValue(bool bit, int rgbValue)
        {
            int b = bit ? 1 : 0;
            if ((rgbValue % 2) != b)
                return rgbValue == 255 ? rgbValue - 1 : rgbValue + 1;

            return rgbValue;
        }

        public byte[] DecodeImage(string filePath)
        {
            return DecodeImage(new string[] { filePath });
        }

        private static byte ConvertBoolArrayToByte(bool[] boolArray)
        {
            byte result = 0;

            for (int i = 0; i < 8; i++)
                if (boolArray[i])
                    result = (byte) (result | (1 << i));

            return result;
        }

        public byte[] DecodeImage(string[] filePaths)
        {
            List<byte> fileBytes = new List<byte>();

            try
            {
                foreach (string filepath in filePaths)
                {
                    Bitmap bitmap = new Bitmap(filepath);

                    // Lock the bitmap's bits.  
                    Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite,
                        bitmap.PixelFormat);

                    // Get the address of the first line.
                    IntPtr ptr = bitmapData.Scan0;

                    // Declare an array to hold the bytes of the bitmap.
                    int bytes = Math.Abs(bitmapData.Stride) * bitmap.Height;
                    byte[] rgbValues = new byte[bytes];

                    // Copy the RGB values into the array. 
                    Marshal.Copy(ptr, rgbValues, 0, bytes);

                    byte[][] chunks = rgbValues
                    .Select((s, i) => new { Value = s, Index = i })
                    .GroupBy(x => x.Index / 9)
                    .Select(grp => grp.Select(x => x.Value).ToArray())
                    .ToArray();

                    bool[] bits = new bool[8];
                    foreach (byte[] chunk in chunks)
                    {
                        try
                        {
                            for (int idx = 0; idx < bits.Length; idx++)
                                bits[idx] = (chunk[idx] % 2 == 0) ? false : true;

                            fileBytes.Add(ConvertBoolArrayToByte(bits));

                            if (chunk[8] % 2 == 0)
                                break;
                        } 
                        catch (IndexOutOfRangeException ex)
                        {
                            Console.WriteLine("[*] Loading next image...");
                        }
                    }
                }

                byte[] result = new byte[fileBytes.Count()];
                for (int index = 0; index < fileBytes.Count(); index++)
                    result[index] = fileBytes.ElementAt(index);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);

                return null;
            }
        }

    }
}
