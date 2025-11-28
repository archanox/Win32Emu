using System;

namespace CHDReaderTest.Flac.FlacDeps
{
    public class lpc
    {
        public const int MAX_LPC_ORDER = 32;
        public const int MAX_LPC_WINDOWS = 16;
        public const int MAX_LPC_PRECISIONS = 4;
        public const int MAX_LPC_SECTIONS = 128;

        public static void window_welch(float[] window, int windowOffset, int L)
        {
            int N = L - 1;
            double N2 = N / 2.0;

            for (int n = 0; n <= N; n++)
            {
                double k = (n - N2) / N2;
                k = 1.0 - k * k;
                window[windowOffset + n] = (float)k;
            }
        }

        public static void window_bartlett(float[] window, int windowOffset, int L)
        {
            int N = L - 1;
            double N2 = N / 2.0;
            for (int n = 0; n <= N; n++)
            {
                double k = (n - N2) / N2;
                k = 1.0 - k * k;
                window[windowOffset + n] = (float)(k * k);
            }
        }

        public static void window_rectangle(float[] window, int windowOffset, int L)
        {
            for (int n = 0; n < L; n++)
                window[windowOffset + n] = 1.0F;
        }

        public static void window_flattop(float[] window, int windowOffset, int L)
        {
            int N = L - 1;
            for (int n = 0; n < L; n++)
                window[windowOffset + n] = (float)(1.0 - 1.93 * Math.Cos(2.0 * Math.PI * n / N) + 1.29 * Math.Cos(4.0 * Math.PI * n / N) - 0.388 * Math.Cos(6.0 * Math.PI * n / N) + 0.0322 * Math.Cos(8.0 * Math.PI * n / N));
        }

        public static void window_tukey(float[] window, int windowOffset, int L, double p)
        {
            int z = 0;
            int Np = (int)(p / 2.0 * L) - z;
            if (Np > 0)
            {
                for (int n = 0; n < z; n++)
                    window[windowOffset + n] = window[windowOffset + L - n - 1] = 0;
                for (int n = 0; n < Np - 1; n++)
                    window[windowOffset + n + z] = window[windowOffset + L - n - 1 - z] = (float)(0.5 - 0.5 * Math.Cos(Math.PI * (n + 1) / Np));
                for (int n = z + Np - 1; n < L - z - Np + 1; n++)
                    window[windowOffset + n] = 1.0F;
            }
        }

        public static void window_punchout_tukey(float[] window, int windowOffset, int L, double p, double p1, double start, double end)
        {
            int start_n = (int)(start * L);
            int end_n = (int)(end * L);
            int Np = (int)(p / 2.0 * L);
            int Np1 = (int)(p1 / 2.0 * L);
            int i, n = 0;

            if (start_n != 0)
            {
                for (i = 1; n < Np; n++, i++)
                    window[windowOffset + n] = (float)(0.5 - 0.5 * Math.Cos(Math.PI * i / Np));
                for (; n < start_n - Np1; n++)
                    window[windowOffset + n] = 1.0f;
                for (i = Np1; n < start_n; n++, i--)
                    window[windowOffset + n] = (float)(0.5 - 0.5 * Math.Cos(Math.PI * i / Np1));
            }
            for (; n < end_n; n++)
                window[windowOffset + n] = 0.0f;
            if (end_n != L)
            {
                for (i = 1; n < end_n + Np1; n++, i++)
                    window[windowOffset + n] = (float)(0.5 - 0.5 * Math.Cos(Math.PI * i / Np1));
                for (; n < L - Np; n++)
                    window[windowOffset + n] = 1.0f;
                for (i = Np; n < L; n++, i--)
                    window[windowOffset + n] = (float)(0.5 - 0.5 * Math.Cos(Math.PI * i / Np));
            }
        }

        public static void window_hann(float[] window, int windowOffset, int L)
        {
            int N = L - 1;
            for (int n = 0; n < L; n++)
                window[windowOffset + n] = (float)(0.5 - 0.5 * Math.Cos(2.0 * Math.PI * n / N));
        }

        private static short sign_only(int val)
        {
            return (short)((val >> 31) + (val - 1 >> 31) + 1);
        }

        /// <summary>
        /// Calculates autocorrelation data from audio samples
        /// A window function is applied before calculation.
        /// </summary>
        public static void compute_autocorr(int[] data, int dataOffset, float[] window, int windowOffset, int len, int min, int lag, double[] autoc, int autocOffset)
        {
            double[] data1 = new double[len];

            for (int i = 0; i < len; i++)
                data1[i] = data[dataOffset + i] * window[windowOffset + i];

            for (int i = min; i <= lag; ++i)
            {
                double temp = 0;
                double temp2 = 0;
                int pdataIdx = 0;
                int finish = len - 1 - i;

                while (pdataIdx < finish)
                {
                    temp += data1[pdataIdx + i] * data1[pdataIdx];
                    pdataIdx++;
                    temp2 += data1[pdataIdx + i] * data1[pdataIdx];
                    pdataIdx++;
                }
                if (pdataIdx <= finish)
                    temp += data1[pdataIdx + i] * data1[pdataIdx];

                autoc[autocOffset + i] += temp + temp2;
            }
        }

        public static void compute_autocorr_windowless(int[] data, int dataOffset, int len, int min, int lag, double[] autoc, int autocOffset)
        {
            for (int i = min; i <= lag; ++i)
            {
                long temp = 0;
                long temp2 = 0;
                int pdataIdx = 0;
                int finish = len - i - 1;
                while (pdataIdx < finish)
                {
                    temp += (long)data[dataOffset + pdataIdx + i] * data[dataOffset + pdataIdx];
                    pdataIdx++;
                    temp2 += (long)data[dataOffset + pdataIdx + i] * data[dataOffset + pdataIdx];
                    pdataIdx++;
                }
                if (pdataIdx <= finish)
                    temp += (long)data[dataOffset + pdataIdx + i] * data[dataOffset + pdataIdx];
                autoc[autocOffset + i] += temp + temp2;
            }
        }

        public static void compute_autocorr_windowless_large(int[] data, int dataOffset, int len, int min, int lag, double[] autoc, int autocOffset)
        {
            for (int i = min; i <= lag; ++i)
            {
                double temp = 0;
                double temp2 = 0;
                int pdataIdx = 0;
                int finish = len - i - 1;
                while (pdataIdx < finish)
                {
                    temp += (long)data[dataOffset + pdataIdx + i] * data[dataOffset + pdataIdx];
                    pdataIdx++;
                    temp2 += (long)data[dataOffset + pdataIdx + i] * data[dataOffset + pdataIdx];
                    pdataIdx++;
                }
                if (pdataIdx <= finish)
                    temp += (long)data[dataOffset + pdataIdx + i] * data[dataOffset + pdataIdx];
                autoc[autocOffset + i] += temp + temp2;
            }
        }

        public static void compute_autocorr_glue(int[] data, int dataOffset, float[] window, int windowOffset, int offs, int offs1, int min, int lag, double[] autoc, int autocOffset)
        {
            double[] data1 = new double[lag + lag];
            for (int i = -lag; i < lag; i++)
                data1[i + lag] = offs + i >= 0 && offs + i < offs1 ? data[dataOffset + offs + i] * window[windowOffset + offs + i] : 0;
            for (int i = min; i <= lag; ++i)
            {
                double temp = 0;
                int pdataIdx = lag - i;
                int finish = lag;
                while (pdataIdx < finish)
                    temp += data1[pdataIdx + i] * data1[pdataIdx++];
                autoc[autocOffset + i] += temp;
            }
        }

        public static void compute_autocorr_glue(int[] data, int dataOffset, int min, int lag, double[] autoc, int autocOffset)
        {
            for (int i = min; i <= lag; ++i)
            {
                long temp = 0;
                int pdataIdx = -i;
                int finish = 0;
                while (pdataIdx < finish)
                    temp += (long)data[dataOffset + pdataIdx + i] * data[dataOffset + pdataIdx++];
                autoc[autocOffset + i] += temp;
            }
        }

        /// <summary>
        /// Levinson-Durbin recursion.
        /// Produces LPC coefficients from autocorrelation data.
        /// </summary>
        public static void compute_lpc_coefs(uint max_order, double[] reff, int reffOffset, float[] lpc_out, int lpcOffset)
        {
            double[] lpc_tmp = new double[MAX_LPC_ORDER];

            if (max_order > MAX_LPC_ORDER)
                throw new Exception("weird");

            for (int i = 0; i < max_order; i++)
                lpc_tmp[i] = 0;

            for (int i = 0; i < max_order; i++)
            {
                double r = reff[reffOffset + i];
                int i2 = i >> 1;
                lpc_tmp[i] = r;
                for (int j = 0; j < i2; j++)
                {
                    double tmp = lpc_tmp[j];
                    lpc_tmp[j] += r * lpc_tmp[i - 1 - j];
                    lpc_tmp[i - 1 - j] += r * tmp;
                }

                if (0 != (i & 1))
                    lpc_tmp[i2] += lpc_tmp[i2] * r;

                for (int j = 0; j <= i; j++)
                    lpc_out[lpcOffset + i * MAX_LPC_ORDER + j] = (float)-lpc_tmp[j];
            }
        }

        public static void compute_schur_reflection(double[] autoc, int autocOffset, uint max_order, double[] reff, int reffOffset, double[] err, int errOffset)
        {
            double[] gen0 = new double[MAX_LPC_ORDER];
            double[] gen1 = new double[MAX_LPC_ORDER];

            // Schur recursion
            for (uint i = 0; i < max_order; i++)
                gen0[i] = gen1[i] = autoc[autocOffset + i + 1];

            double error = autoc[autocOffset + 0];
            reff[reffOffset + 0] = -gen1[0] / error;
            error += gen1[0] * reff[reffOffset + 0];
            err[errOffset + 0] = error;
            for (uint i = 1; i < max_order; i++)
            {
                for (uint j = 0; j < max_order - i; j++)
                {
                    gen1[j] = gen1[j + 1] + reff[reffOffset + i - 1] * gen0[j];
                    gen0[j] = gen1[j + 1] * reff[reffOffset + i - 1] + gen0[j];
                }
                reff[reffOffset + i] = -gen1[0] / error;
                error += gen1[0] * reff[reffOffset + i];
                err[errOffset + i] = error;
            }
        }

        /// <summary>
        /// Quantize LPC coefficients
        /// </summary>
        public static void quantize_lpc_coefs(float[] lpc_in, int lpcInOffset, int order, uint precision, int[] lpc_out, int lpcOutOffset, out int shift, int max_shift, int zero_shift)
        {
            int i;
            float d, cmax, error;
            int qmax;
            int sh, q;

            // define maximum levels
            qmax = (1 << (int)precision - 1) - 1;

            // find maximum coefficient value
            cmax = 0.0F;
            for (i = 0; i < order; i++)
            {
                d = Math.Abs(lpc_in[lpcInOffset + i]);
                if (d > cmax)
                    cmax = d;
            }
            // if maximum value quantizes to zero, return all zeros
            if (cmax * (1 << max_shift) < 1.0)
            {
                shift = zero_shift;
                for (i = 0; i < order; i++)
                    lpc_out[lpcOutOffset + i] = 0;
                return;
            }

            // calculate level shift which scales max coeff to available bits
            sh = max_shift;
            while (cmax * (1 << sh) > qmax && sh > 0)
            {
                sh--;
            }

            // since negative shift values are unsupported in decoder, scale down
            // coefficients instead
            if (sh == 0 && cmax > qmax)
            {
                float scale = qmax / cmax;
                for (i = 0; i < order; i++)
                {
                    lpc_in[lpcInOffset + i] *= scale;
                }
            }

            // output quantized coefficients and level shift
            error = 0;
            for (i = 0; i < order; i++)
            {
                error += lpc_in[lpcInOffset + i] * (1 << sh);
                q = (int)(error + 0.5);
                if (q < -(qmax + 1)) q = -(qmax + 1);
                if (q > qmax) q = qmax;
                error -= q;
                lpc_out[lpcOutOffset + i] = q;
            }
            shift = sh;
        }

        public static void decode_residual(int[] res, int resOffset, int[] smp, int smpOffset, int n, int order, int[] coefs, int coefsOffset, int shift)
        {
            for (int i = 0; i < order; i++)
                smp[smpOffset + i] = res[resOffset + i];

            int s = 0;
            int rIdx = order;
            int c0 = coefs[coefsOffset + 0];
            int c1 = coefs[coefsOffset + 1];
            switch (order)
            {
                case 1:
                    for (int i = n - order; i > 0; i--)
                    {
                        int pred = c0 * smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (pred >> shift);
                    }
                    break;
                case 2:
                    for (int i = n - order; i > 0; i--)
                    {
                        int pred = c1 * smp[smpOffset + s++] + c0 * smp[smpOffset + s++];
                        smp[smpOffset + s--] = res[resOffset + rIdx++] + (pred >> shift);
                    }
                    break;
                case 3:
                    for (int i = n - order; i > 0; i--)
                    {
                        int co = order - 1;
                        int pred =
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            c1 * smp[smpOffset + s++] + c0 * smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (pred >> shift);
                        s -= 2;
                    }
                    break;
                case 4:
                    for (int i = n - order; i > 0; i--)
                    {
                        int co = order - 1;
                        int pred =
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            c1 * smp[smpOffset + s++] + c0 * smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (pred >> shift);
                        s -= 3;
                    }
                    break;
                case 5:
                    for (int i = n - order; i > 0; i--)
                    {
                        int co = order - 1;
                        int pred =
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            c1 * smp[smpOffset + s++] + c0 * smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (pred >> shift);
                        s -= 4;
                    }
                    break;
                case 6:
                    for (int i = n - order; i > 0; i--)
                    {
                        int co = order - 1;
                        int pred =
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            c1 * smp[smpOffset + s++] + c0 * smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (pred >> shift);
                        s -= 5;
                    }
                    break;
                case 7:
                    for (int i = n - order; i > 0; i--)
                    {
                        int co = order - 1;
                        int pred =
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            c1 * smp[smpOffset + s++] + c0 * smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (pred >> shift);
                        s -= 6;
                    }
                    break;
                case 8:
                    for (int i = n - order; i > 0; i--)
                    {
                        int co = order - 1;
                        int pred =
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            c1 * smp[smpOffset + s++] + c0 * smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (pred >> shift);
                        s -= 7;
                    }
                    break;
                case 9:
                    for (int i = n - order; i > 0; i--)
                    {
                        int co = order - 1;
                        int pred =
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            c1 * smp[smpOffset + s++] + c0 * smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (pred >> shift);
                        s -= 8;
                    }
                    break;
                case 10:
                    for (int i = n - order; i > 0; i--)
                    {
                        int co = order - 1;
                        int pred =
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            c1 * smp[smpOffset + s++] + c0 * smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (pred >> shift);
                        s -= 9;
                    }
                    break;
                case 11:
                    for (int i = n - order; i > 0; i--)
                    {
                        int co = order - 1;
                        int pred =
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            c1 * smp[smpOffset + s++] + c0 * smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (pred >> shift);
                        s -= 10;
                    }
                    break;
                case 12:
                    for (int i = n - order; i > 0; i--)
                    {
                        int co = order - 1;
                        int pred =
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            coefs[coefsOffset + co--] * smp[smpOffset + s++] + coefs[coefsOffset + co--] * smp[smpOffset + s++] +
                            c1 * smp[smpOffset + s++] + c0 * smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (pred >> shift);
                        s -= 11;
                    }
                    break;
                default:
                    for (int i = order; i < n; i++)
                    {
                        s = i - order;
                        int pred = 0;
                        int co = order - 1;
                        int c7 = 7;
                        while (co > c7)
                            pred += coefs[coefsOffset + co--] * smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 7] * smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 6] * smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 5] * smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 4] * smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 3] * smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 2] * smp[smpOffset + s++];
                        pred += c1 * smp[smpOffset + s++];
                        pred += c0 * smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (pred >> shift);
                    }
                    break;
            }
        }

        public static void decode_residual_long(int[] res, int resOffset, int[] smp, int smpOffset, int n, int order, int[] coefs, int coefsOffset, int shift)
        {
            for (int i = 0; i < order; i++)
                smp[smpOffset + i] = res[resOffset + i];

            int s = 0;
            int rIdx = order;
            int c0 = coefs[coefsOffset + 0];
            int c1 = coefs[coefsOffset + 1];
            switch (order)
            {
                case 1:
                    for (int i = n - order; i > 0; i--)
                    {
                        long pred = c0 * (long)smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (int)(pred >> shift);
                    }
                    break;
                case 2:
                    for (int i = n - order; i > 0; i--)
                    {
                        long pred = c1 * (long)smp[smpOffset + s++];
                        pred += c0 * (long)smp[smpOffset + s++];
                        smp[smpOffset + s--] = res[resOffset + rIdx++] + (int)(pred >> shift);
                    }
                    break;
                case 3:
                    for (int i = n - order; i > 0; i--)
                    {
                        long pred = coefs[coefsOffset + 2] * (long)smp[smpOffset + s++];
                        pred += c1 * (long)smp[smpOffset + s++];
                        pred += c0 * (long)smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (int)(pred >> shift);
                        s -= 2;
                    }
                    break;
                case 4:
                    for (int i = n - order; i > 0; i--)
                    {
                        long pred = coefs[coefsOffset + 3] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 2] * (long)smp[smpOffset + s++];
                        pred += c1 * (long)smp[smpOffset + s++];
                        pred += c0 * (long)smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (int)(pred >> shift);
                        s -= 3;
                    }
                    break;
                case 5:
                    for (int i = n - order; i > 0; i--)
                    {
                        long pred = coefs[coefsOffset + 4] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 3] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 2] * (long)smp[smpOffset + s++];
                        pred += c1 * (long)smp[smpOffset + s++];
                        pred += c0 * (long)smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (int)(pred >> shift);
                        s -= 4;
                    }
                    break;
                case 6:
                    for (int i = n - order; i > 0; i--)
                    {
                        long pred = coefs[coefsOffset + 5] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 4] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 3] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 2] * (long)smp[smpOffset + s++];
                        pred += c1 * (long)smp[smpOffset + s++];
                        pred += c0 * (long)smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (int)(pred >> shift);
                        s -= 5;
                    }
                    break;
                case 7:
                    for (int i = n - order; i > 0; i--)
                    {
                        long pred = coefs[coefsOffset + 6] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 5] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 4] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 3] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 2] * (long)smp[smpOffset + s++];
                        pred += c1 * (long)smp[smpOffset + s++];
                        pred += c0 * (long)smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (int)(pred >> shift);
                        s -= 6;
                    }
                    break;
                case 8:
                    for (int i = n - order; i > 0; i--)
                    {
                        long pred = coefs[coefsOffset + 7] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 6] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 5] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 4] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 3] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 2] * (long)smp[smpOffset + s++];
                        pred += c1 * (long)smp[smpOffset + s++];
                        pred += c0 * (long)smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (int)(pred >> shift);
                        s -= 7;
                    }
                    break;
                default:
                    for (int i = order; i < n; i++)
                    {
                        s = i - order;
                        long pred = 0;
                        int co = order - 1;
                        int c7 = 7;
                        while (co > c7)
                            pred += coefs[coefsOffset + co--] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 7] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 6] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 5] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 4] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 3] * (long)smp[smpOffset + s++];
                        pred += coefs[coefsOffset + 2] * (long)smp[smpOffset + s++];
                        pred += c1 * (long)smp[smpOffset + s++];
                        pred += c0 * (long)smp[smpOffset + s++];
                        smp[smpOffset + s] = res[resOffset + rIdx++] + (int)(pred >> shift);
                    }
                    break;
            }
        }
    }
}
