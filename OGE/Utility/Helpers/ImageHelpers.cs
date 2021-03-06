﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using RfgTools.Dependencies;
using RfgTools.Formats.Textures;

namespace OGE.Utility.Helpers
{
    //Todo: Might be better to have this class in the RfgTools repo
    //Todo: Consider replacing with ImageSharp, assuming it can more easily handle a wide variety of image formats. Maybe could handle DXT compression too
    public static class ImageHelpers
    {
        public static Bitmap EntryDataToBitmap(PegEntry entry)
        {
            if (entry.bitmap_format == PegFormat.PC_DXT1)
            {
                var decompressBuffer = Squish.Decompress(entry.RawData, entry.width, entry.height, Squish.Flags.DXT1);
                return MakeBitmapFromDXT(entry.width, entry.height, decompressBuffer, true);
            }
            else if (entry.bitmap_format == PegFormat.PC_DXT3)
            {
                var decompressBuffer = Squish.Decompress(entry.RawData, entry.width, entry.height, Squish.Flags.DXT3);
                return MakeBitmapFromDXT(entry.width, entry.height, decompressBuffer, true);
            }
            else if (entry.bitmap_format == PegFormat.PC_DXT5)
            {
                var decompressBuffer = Squish.Decompress(entry.RawData, entry.width, entry.height, Squish.Flags.DXT5);
                return MakeBitmapFromDXT(entry.width, entry.height, decompressBuffer, true);
            }
            else
            {
                throw new Exception($"Unsupported PEG data format detected! {entry.bitmap_format.ToString()} is not yet supported.");
            }
        }

        public static Bitmap MakeBitmapFromDXT(uint width, uint height, byte[] buffer, bool keepAlpha)
        {
            Bitmap bitmap = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
            for (uint num = 0u; num < width * height * 4u; num += 4u)
            {
                byte b = buffer[(int)((UIntPtr)num)];
                buffer[(int)((UIntPtr)num)] = buffer[(int)((UIntPtr)(num + 2u))];
                buffer[(int)((UIntPtr)(num + 2u))] = b;
            }
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(buffer, 0, bitmapData.Scan0, (int)(width * height * 4u));
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png); //Use here to keep transparency
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        public static byte[] ConvertBitmapToByteArray(Bitmap bitmap)
        {
            if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
                byte[] data = new byte[bitmap.Width * bitmap.Height * 4]; //* 3 for rgb

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = Marshal.ReadByte(bitmapData.Scan0, i);
                }
                bitmap.UnlockBits(bitmapData);

                //At this point the data is in BGRA arrangement, need to make it RGBA for DXT compression purposes.
                var redChannel = new byte[data.Length / 4];
                var greenChannel = new byte[data.Length / 4];
                var blueChannel = new byte[data.Length / 4];
                var alphaChannel = new byte[data.Length / 4];

                int pixelIndex = 0;
                for (int i = 0; i < data.Length - 3; i += 4)
                {
                    blueChannel[pixelIndex] = data[i];
                    greenChannel[pixelIndex] = data[i + 1];
                    redChannel[pixelIndex] = data[i + 2];
                    alphaChannel[pixelIndex] = data[i + 3];
                    pixelIndex++;
                }

                pixelIndex = 0;
                for (int i = 0; i < data.Length - 3; i += 4)
                {
                    data[i] = redChannel[pixelIndex];
                    data[i + 1] = greenChannel[pixelIndex];
                    data[i + 2] = blueChannel[pixelIndex];
                    data[i + 3] = alphaChannel[pixelIndex];
                    pixelIndex++;
                }

                return data;
            }
            //else if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
            //{

            //}
            else
            {
                throw new Exception($"Texture import failed! {bitmap.PixelFormat.ToString()} is currently an unsupported import pixel format.");
            }
        }
    }
}
