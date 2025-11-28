using System;

namespace CHDReaderTest.Flac.FlacDeps
{
    public static class Crc32
    {
        public static readonly uint[] table;

        public static uint ComputeChecksum(uint crc, byte val)
        {
            return crc >> 8 ^ table[crc & 0xff ^ val];
        }

        public static uint ComputeChecksum(uint crc, byte[] bytes, int pos, int count)
        {
            for (int i = 0; i < count; i++)
                crc = crc >> 8 ^ table[(crc ^ bytes[pos + i]) & 0xff];
            return crc;
        }

        public static uint ComputeChecksum(uint crc, uint s)
        {
            return ComputeChecksum(ComputeChecksum(ComputeChecksum(ComputeChecksum(
                crc, (byte)s), (byte)(s >> 8)), (byte)(s >> 16)), (byte)(s >> 24));
        }

        public static uint ComputeChecksum(uint crc, int[] samples, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int s1 = samples[offset + 2 * i], s2 = samples[offset + 2 * i + 1];
                crc = ComputeChecksum(ComputeChecksum(ComputeChecksum(ComputeChecksum(
                    crc, (byte)s1), (byte)(s1 >> 8)), (byte)s2), (byte)(s2 >> 8));
            }
            return crc;
        }

        internal static uint Reflect(uint val, int ch)
        {
            uint value = 0;
            // Swap bit 0 for bit 7
            // bit 1 for bit 6, etc.
            for (int i = 1; i < ch + 1; i++)
            {
                if (0 != (val & 1))
                    value |= 1U << ch - i;
                val >>= 1;
            }
            return value;
        }

        const uint uPolynomial = 0x04c11db7;
        const uint uReversePolynomial = 0xedb88320;
        const uint uReversePolynomial2 = 0xdb710641;

        private static readonly uint[,] combineTable;
        private static readonly uint[,] substractTable;

        static Crc32()
        {
            table = new uint[256];
            for (uint i = 0; i < table.Length; i++)
            {
                table[i] = Reflect(i, 8) << 24;
                for (int j = 0; j < 8; j++)
                    table[i] = table[i] << 1 ^ ((table[i] & 1U << 31) == 0 ? 0 : uPolynomial);
                table[i] = Reflect(table[i], 32);
            }
            combineTable = new uint[GF2_DIM, GF2_DIM];
            substractTable = new uint[GF2_DIM, GF2_DIM];
            combineTable[0, 0] = uReversePolynomial;
            substractTable[0, 31] = uReversePolynomial2;
            for (int n = 1; n < GF2_DIM; n++)
            {
                combineTable[0, n] = 1U << n - 1;
                substractTable[0, n - 1] = 1U << n;
            }
            for (int i = 1; i < GF2_DIM; i++)
            {
                gf2_matrix_square(combineTable, i, combineTable, i - 1);
                gf2_matrix_square(substractTable, i, substractTable, i - 1);
            }
        }

        const int GF2_DIM = 32;

        private static uint gf2_matrix_times(uint[,] umat, int matRow, uint uvec)
        {
            int vec = (int)uvec;
            uint result = 0;
            for (int i = 0; i < GF2_DIM; i++)
            {
                result ^= (uint)((int)umat[matRow, i] & (vec << (31 - i) >> 31));
            }
            return result;
        }

        /* ========================================================================= */
        private static void gf2_matrix_square(uint[,] square, int squareRow, uint[,] mat, int matRow)
        {
            for (int n = 0; n < GF2_DIM; n++)
                square[squareRow, n] = gf2_matrix_times(mat, matRow, mat[matRow, n]);
        }

        public static uint Combine(uint crc1, uint crc2, long len2)
        {
            /* degenerate case */
            if (len2 == 0)
                return crc1;
            if (crc1 == 0)
                return crc2;
            if (len2 < 0)
                throw new ArgumentException("crc.Combine length cannot be negative", "len2");

            int n = 3;
            do
            {
                /* apply zeros operator for this bit of len2 */
                if ((len2 & 1) != 0)
                    crc1 = gf2_matrix_times(combineTable, n, crc1);
                len2 >>= 1;
                n = n + 1 & GF2_DIM - 1;
                /* if no more bits set, then done */
            } while (len2 != 0);

            /* return combined crc */
            crc1 ^= crc2;
            return crc1;
        }

        public static uint Subtract(uint crc1, uint crc2, long len2)
        {
            /* degenerate case */
            if (len2 == 0)
                return crc1;
            if (len2 < 0)
                throw new ArgumentException("crc.Combine length cannot be negative", "len2");

            crc1 ^= crc2;

            int n = 3;
            do
            {
                /* apply zeros operator for this bit of len2 */
                if ((len2 & 1) != 0)
                    crc1 = gf2_matrix_times(substractTable, n, crc1);
                len2 >>= 1;
                n = n + 1 & GF2_DIM - 1;
                /* if no more bits set, then done */
            } while (len2 != 0);

            /* return combined crc */
            return crc1;
        }
    }
}
