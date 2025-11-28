using System;

namespace CHDReaderTest.Flac.FlacDeps
{
    public class AudioSamples
    {
        public const uint UINT32_MAX = 0xffffffff;

        public static void Interlace(int[] res, int resOffset, int[] src1, int src1Offset, int[] src2, int src2Offset, int n)
        {
            int resIdx = resOffset;
            int src1Idx = src1Offset;
            int src2Idx = src2Offset;
            for (int i = 0; i < n; i++)
            {
                res[resIdx++] = src1[src1Idx++];
                res[resIdx++] = src2[src2Idx++];
            }
        }

        public static void Deinterlace(int[] dst1, int dst1Offset, int[] dst2, int dst2Offset, int[] src, int srcOffset, int n)
        {
            int dst1Idx = dst1Offset;
            int dst2Idx = dst2Offset;
            int srcIdx = srcOffset;
            for (int i = 0; i < n; i++)
            {
                dst1[dst1Idx++] = src[srcIdx++];
                dst2[dst2Idx++] = src[srcIdx++];
            }
        }

        public static bool MemCmp(int[] res, int resOffset, int[] smp, int smpOffset, int n)
        {
            for (int i = 0; i < n; i++)
                if (res[resOffset + i] != smp[smpOffset + i])
                    return true;
            return false;
        }

        public static void MemCpy(uint[] res, int resOffset, uint[] smp, int smpOffset, int n)
        {
            Array.Copy(smp, smpOffset, res, resOffset, n);
        }

        public static void MemCpy(int[] res, int resOffset, int[] smp, int smpOffset, int n)
        {
            Array.Copy(smp, smpOffset, res, resOffset, n);
        }

        public static void MemCpy(long[] res, int resOffset, long[] smp, int smpOffset, int n)
        {
            Array.Copy(smp, smpOffset, res, resOffset, n);
        }

        public static void MemCpy(short[] res, int resOffset, short[] smp, int smpOffset, int n)
        {
            Array.Copy(smp, smpOffset, res, resOffset, n);
        }

        public static void MemCpy(byte[] res, int resOffset, byte[] smp, int smpOffset, int n)
        {
            Array.Copy(smp, smpOffset, res, resOffset, n);
        }

        public static void MemSet(byte[] res, int offs, byte smp, int n)
        {
            Array.Fill(res, smp, offs, n);
        }

        public static void MemSet(int[] res, int offs, int smp, int n)
        {
            Array.Fill(res, smp, offs, n);
        }

        public static void MemSet(long[] res, int offs, long smp, int n)
        {
            Array.Fill(res, smp, offs, n);
        }
    }

}
