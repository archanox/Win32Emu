using System;

namespace CHDReaderTest.Flac.FlacDeps
{
    public static class Crc16
    {
        const int GF2_DIM = 16;
        public static ushort[] table = new ushort[256];
        private static readonly ushort[,] combineTable = new ushort[GF2_DIM, GF2_DIM];
        private static readonly ushort[,] substractTable = new ushort[GF2_DIM, GF2_DIM];

        public static ushort ComputeChecksum(ushort crc, byte[] bytes, int pos, int count)
        {
            for (int i = 0; i < count; i++)
            {
                crc = (ushort)(crc << 8 ^ table[crc >> 8 ^ bytes[pos + i]]);
            }
            return crc;
        }

        const ushort polynomial = 0x8005;
        const ushort reversePolynomial = 0x4003;

        static Crc16()
        {
            for (ushort i = 0; i < table.Length; i++)
            {
                int crc = i;
                for (int j = 0; j < GF2_DIM; j++)
                {
                    if ((crc & 1U << GF2_DIM - 1) != 0)
                        crc = crc << 1 ^ polynomial;
                    else
                        crc <<= 1;
                }
                table[i] = (ushort)(crc & (1 << GF2_DIM) - 1);
            }

            combineTable[0, 0] = Reflect(polynomial);
            substractTable[0, GF2_DIM - 1] = reversePolynomial;
            for (int n = 1; n < GF2_DIM; n++)
            {
                combineTable[0, n] = (ushort)(1 << n - 1);
                substractTable[0, n - 1] = (ushort)(1 << n);
            }

            for (int i = 1; i < GF2_DIM; i++)
            {
                gf2_matrix_square(combineTable, i, combineTable, i - 1);
                gf2_matrix_square(substractTable, i, substractTable, i - 1);
            }
        }

        private static ushort gf2_matrix_times(ushort[,] mat, int matRow, ushort uvec)
        {
            int vec = uvec << 16;
            ushort result = 0;
            for (int i = 0; i < GF2_DIM; i++)
            {
                result ^= (ushort)(mat[matRow, i] & (vec << (15 - i) >> 31));
            }
            return result;
        }

        private static void gf2_matrix_square(ushort[,] square, int squareRow, ushort[,] mat, int matRow)
        {
            for (int n = 0; n < GF2_DIM; n++)
                square[squareRow, n] = gf2_matrix_times(mat, matRow, mat[matRow, n]);
        }

        public static ushort Reflect(ushort crc)
        {
            return (ushort)Crc32.Reflect(crc, 16);
        }

        public static ushort Combine(ushort crc1, ushort crc2, long len2)
        {
            crc1 = Reflect(crc1);
            crc2 = Reflect(crc2);

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
            crc1 = Reflect(crc1);
            return crc1;
        }

        public static ushort Subtract(ushort crc1, ushort crc2, long len2)
        {
            crc1 = Reflect(crc1);
            crc2 = Reflect(crc2);
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
            crc1 = Reflect(crc1);
            return crc1;
        }
    }
}
