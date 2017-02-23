/////////////////////////////////////////////////////////////////////////
//// Copyright, (c) Shanghai United Imaging Healthcare Inc
//// All rights reserved. 
//// 
//// author: qiuyang.cao@united-imaging.com
////
//// File: ByteConverter.cs
////
//// Summary:
////
////
//// Date: 2014/08/18
//////////////////////////////////////////////////////////////////////////
#region License

// Copyright (c) 2011 - 2013, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System;

namespace UIH.Dicom.IO
{
    internal static class ByteConverter
    {
        /// <summary>
        /// Determines if this machine has the same byte
        /// order as endian.
        /// </summary>
        /// <param name="endian">endianness</param>
        /// <returns>true - byte swapping is required</returns>
        public static bool NeedToSwapBytes(Endian endian)
        {
            if (BitConverter.IsLittleEndian)
            {
                if (Endian.Little == endian)
                    return false;
                return true;
            }
            else
            {
                if (Endian.Big == endian)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Converts an array of ushorts to an array of bytes.
        /// </summary>
        /// <param name="words">Array of ushorts</param>
        /// <returns>Newly allocated byte array</returns>
        public static byte[] ToByteArray(ushort[] words)
        {
            int count = words.Length;
            byte[] bytes = new byte[words.Length * 2];
            for (int i = 0; i < count; i++)
            {
                // slow as shit, should be able to use Buffer.BlockCopy for this
                Array.Copy(BitConverter.GetBytes(words[i]), 0, bytes, i * 2, 2);
            }

            return bytes;
        }

        /// <summary>
        /// Converts an array of bytes to an array of ushorts.
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <returns>Newly allocated ushort array</returns>
        public static ushort[] ToUInt16Array(byte[] bytes)
        {
            int count = bytes.Length / 2;
            ushort[] words = new ushort[count];
            for (int i = 0, p = 0; i < count; i++, p += 2)
            {
                words[i] = BitConverter.ToUInt16(bytes, p);
            }

            return words;
        }

        /// <summary>
        /// Converts an array of bytes to an array of shorts.
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <returns>Newly allocated short array</returns>
        public static short[] ToInt16Array(byte[] bytes)
        {
            int count = bytes.Length / 2;
            short[] words = new short[count];
            for (int i = 0, p = 0; i < count; i++, p += 2)
            {
                words[i] = BitConverter.ToInt16(bytes, p);
            }

            return words;
        }

        /// <summary>
        /// Converts an array of bytes to an array of uints.
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <returns>Newly allocated uint array</returns>
        public static uint[] ToUInt32Array(byte[] bytes)
        {
            int count = bytes.Length / 4;
            var dwords = new uint[count];
            for (int i = 0, p = 0; i < count; i++, p += 4)
            {
                dwords[i] = BitConverter.ToUInt32(bytes, p);
            }
            return dwords;
        }
        /// <summary>
        /// Converts an array of bytes to an array of uints.
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <returns>Newly allocated int array</returns>
        public static int[] ToInt32Array(byte[] bytes)
        {
            int count = bytes.Length / 4;
            var dwords = new int[count];
            for (int i = 0, p = 0; i < count; i++, p += 4)
            {
                dwords[i] = (int)BitConverter.ToUInt32(bytes, p);
            }
            return dwords;
        }

        /// <summary>
        /// Converts an array of bytes to an array of floats.
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <returns>Newly allocated float array</returns>
        public static float[] ToFloatArray(byte[] bytes)
        {
            int count = bytes.Length / 4;
            var floats = new float[count];
            for (int i = 0, p = 0; i < count; i++, p += 4)
            {
                floats[i] = BitConverter.ToSingle(bytes, p);
            }
            return floats;
        }

        /// <summary>
        /// Converts an array of bytes to an array of doubles.
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <returns>Newly allocated double array</returns>
        public static double[] ToDoubleArray(byte[] bytes)
        {
            int count = bytes.Length / 8;
            var doubles = new double[count];
            for (int i = 0, p = 0; i < count; i++, p += 8)
            {
                doubles[i] = BitConverter.ToDouble(bytes, p);
            }
            return doubles;
        }

        /// <summary>
        /// Swaps the bytes of an array of unsigned words.
        /// </summary>
        /// <param name="words">Array of ushorts</param>
        public static void SwapBytes(ushort[] words)
        {
            int count = words.Length;
            for (int i = 0; i < count; i++)
            {
                ushort u = words[i];
                words[i] = unchecked((ushort) ((u >> 8) | (u << 8)));
            }
        }

        /// <summary>
        /// Swaps the bytes of an array of signed words.
        /// </summary>
        /// <param name="words">Array of shorts</param>
        public static void SwapBytes(short[] words)
        {
            int count = words.Length;
            for (int i = 0; i < count; i++)
            {
                short u = words[i];
                words[i] = unchecked((short)((u >> 8) | (u << 8)));
            }
        }
    }
}
