using System;
using System.Collections.Generic;
using CUETools.Codecs;

namespace CHDReaderTest.Flac.FlacDeps
{
    public class LpcSubframeInfo
    {
        public LpcSubframeInfo()
        {
            autocorr_section_values = new double[lpc.MAX_LPC_SECTIONS, lpc.MAX_LPC_ORDER + 1];
            autocorr_section_orders = new int[lpc.MAX_LPC_SECTIONS];
        }

        public double[,] autocorr_section_values;
        public int[] autocorr_section_orders;

        public void Reset()
        {
            for (int sec = 0; sec < autocorr_section_orders.Length; sec++)
                autocorr_section_orders[sec] = 0;
        }
    }

    public struct LpcWindowSection
    {
        public enum SectionType
        {
            Zero,
            One,
            OneLarge,
            Data,
            OneGlue,
            Glue
        };
        public int m_start;
        public int m_end;
        public SectionType m_type;
        public int m_id;
        public LpcWindowSection(int end)
        {
            m_id = -1;
            m_start = 0;
            m_end = end;
            m_type = SectionType.Data;
        }
        public void setData(int start, int end)
        {
            m_id = -1;
            m_start = start;
            m_end = end;
            m_type = SectionType.Data;
        }
        public void setOne(int start, int end)
        {
            m_id = -1;
            m_start = start;
            m_end = end;
            m_type = SectionType.One;
        }
        public void setGlue(int start)
        {
            m_id = -1;
            m_start = start;
            m_end = start;
            m_type = SectionType.Glue;
        }
        public void setZero(int start, int end)
        {
            m_id = -1;
            m_start = start;
            m_end = end;
            m_type = SectionType.Zero;
        }

        public void compute_autocorr(int[] data, int dataOffset, float[] window, int windowOffset, int min_order, int order, int blocksize, double[] autoc, int autocOffset)
        {
            if (m_type == SectionType.OneLarge)
                lpc.compute_autocorr_windowless_large(data, dataOffset + m_start, m_end - m_start, min_order, order, autoc, autocOffset);
            else if (m_type == SectionType.One)
                lpc.compute_autocorr_windowless(data, dataOffset + m_start, m_end - m_start, min_order, order, autoc, autocOffset);
            else if (m_type == SectionType.Data)
                lpc.compute_autocorr(data, dataOffset + m_start, window, windowOffset + m_start, m_end - m_start, min_order, order, autoc, autocOffset);
            else if (m_type == SectionType.Glue)
                lpc.compute_autocorr_glue(data, dataOffset, window, windowOffset, m_start, m_end, min_order, order, autoc, autocOffset);
            else if (m_type == SectionType.OneGlue)
                lpc.compute_autocorr_glue(data, dataOffset + m_start, min_order, order, autoc, autocOffset);
        }

        public static void Detect(int _windowcount, float[] window_segment, int windowOffset, int stride, int sz, int bps, LpcWindowSection[,] sections)
        {
            int section_id = 0;
            var boundaries = new List<int>();
            var types = new SectionType[_windowcount, lpc.MAX_LPC_SECTIONS * 2];
            var alias = new int[_windowcount, lpc.MAX_LPC_SECTIONS * 2];
            var alias_set = new int[_windowcount, lpc.MAX_LPC_SECTIONS * 2];
            for (int x = 0; x < sz; x++)
            {
                for (int i = 0; i < _windowcount; i++)
                {
                    int a = alias[i, boundaries.Count];
                    float w = window_segment[windowOffset + i * stride + x];
                    float wa = window_segment[windowOffset + a * stride + x];
                    if (wa != w)
                    {
                        for (int i1 = i; i1 < _windowcount; i1++)
                            if (alias[i1, boundaries.Count] == a
                                && w == window_segment[windowOffset + i1 * stride + x])
                                alias[i1, boundaries.Count] = i;
                    }
                    if (boundaries.Count >= lpc.MAX_LPC_SECTIONS * 2) throw new IndexOutOfRangeException();
                    types[i, boundaries.Count] =
                        boundaries.Count >= lpc.MAX_LPC_SECTIONS * 2 - 2 ?
                        SectionType.Data : w == 0.0 ?
                        SectionType.Zero : w != 1.0 ?
                        SectionType.Data : bps * 2 + BitReader.log2i(sz) >= 61 ?
                        SectionType.OneLarge :
                        SectionType.One;
                }
                bool isBoundary = false;
                for (int i = 0; i < _windowcount; i++)
                {
                    isBoundary |= boundaries.Count == 0 ||
                        types[i, boundaries.Count - 1] != types[i, boundaries.Count];
                }
                if (isBoundary)
                {
                    for (int i = 0; i < _windowcount; i++)
                        for (int i1 = 0; i1 < _windowcount; i1++)
                            if (i != i1 && alias[i, boundaries.Count] == alias[i1, boundaries.Count])
                                alias_set[i, boundaries.Count] |= 1 << i1;
                    boundaries.Add(x);
                }
            }
            boundaries.Add(sz);
            var secs = new int[_windowcount];
            // Reconstruct segments list.
            for (int j = 0; j < boundaries.Count - 1; j++)
            {
                for (int i = 0; i < _windowcount; i++)
                {
                    // leave room for glue
                    if (secs[i] >= lpc.MAX_LPC_SECTIONS - 1)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    sections[i, secs[i]].setData(boundaries[j], boundaries[j + 1]);
                    sections[i, secs[i]++].m_type = types[i, j];
                }
                for (int i = 0; i < _windowcount; i++)
                {
                    int sec = secs[i] - 1;
                    if (sec > 0
                        && j > 0 && (alias_set[i, j] == alias_set[i, j - 1] || sections[i, sec].m_type == SectionType.Zero)
                        && sections[i, sec].m_start == boundaries[j]
                        && sections[i, sec].m_end == boundaries[j + 1]
                        && sections[i, sec - 1].m_end == boundaries[j]
                        && sections[i, sec - 1].m_type == sections[i, sec].m_type)
                    {
                        sections[i, sec - 1].m_end = sections[i, sec].m_end;
                        secs[i]--;
                        continue;
                    }
                    if (section_id >= lpc.MAX_LPC_SECTIONS) throw new IndexOutOfRangeException();
                    if (alias_set[i, j] != 0
                        && types[i, j] != SectionType.Zero
                        && section_id < lpc.MAX_LPC_SECTIONS)
                    {
                        for (int i1 = i; i1 < _windowcount; i1++)
                            if (alias[i1, j] == i && secs[i1] > 0)
                                sections[i1, secs[i1] - 1].m_id = section_id;
                        section_id++;
                    }
                    if (sec > 0
                        && (sections[i, sec].m_type == SectionType.One || sections[i, sec].m_type == SectionType.OneLarge)
                        && sections[i, sec].m_end - sections[i, sec].m_start >= lpc.MAX_LPC_ORDER
                        && (sections[i, sec - 1].m_type == SectionType.One || sections[i, sec - 1].m_type == SectionType.OneLarge)
                        && sections[i, sec - 1].m_end - sections[i, sec - 1].m_start >= lpc.MAX_LPC_ORDER)
                    {
                        sections[i, sec + 1] = sections[i, sec];
                        sections[i, sec].m_end = sections[i, sec].m_start;
                        sections[i, sec].m_type = SectionType.OneGlue;
                        sections[i, sec].m_id = -1;
                        secs[i]++;
                        continue;
                    }
                    if (sec > 0
                        && sections[i, sec].m_type != SectionType.Zero
                        && sections[i, sec - 1].m_type != SectionType.Zero)
                    {
                        sections[i, sec + 1] = sections[i, sec];
                        sections[i, sec].m_end = sections[i, sec].m_start;
                        sections[i, sec].m_type = SectionType.Glue;
                        sections[i, sec].m_id = -1;
                        secs[i]++;
                        continue;
                    }
                }
            }
            for (int i = 0; i < _windowcount; i++)
            {
                for (int s = 0; s < secs[i]; s++)
                {
                    if (sections[i, s].m_type == SectionType.Glue
                        || sections[i, s].m_type == SectionType.OneGlue)
                    {
                        sections[i, s].m_end = sections[i, s + 1].m_end;
                    }
                }
                while (secs[i] < lpc.MAX_LPC_SECTIONS)
                {
                    sections[i, secs[i]++].setZero(sz, sz);
                }
            }
        }
    }

    /// <summary>
    /// Context for LPC coefficients calculation and order estimation
    /// </summary>
    public class LpcContext
    {
        public LpcContext()
        {
            coefs = new int[lpc.MAX_LPC_ORDER];
            reflection_coeffs = new double[lpc.MAX_LPC_ORDER];
            prediction_error = new double[lpc.MAX_LPC_ORDER];
            autocorr_values = new double[lpc.MAX_LPC_ORDER + 1];
            best_orders = new int[lpc.MAX_LPC_ORDER];
            done_lpcs = new uint[lpc.MAX_LPC_PRECISIONS];
        }

        /// <summary>
        /// Reset to initial (blank) state
        /// </summary>
        public void Reset()
        {
            autocorr_order = 0;
            for (int iPrecision = 0; iPrecision < lpc.MAX_LPC_PRECISIONS; iPrecision++)
                done_lpcs[iPrecision] = 0;
        }

        /// <summary>
        /// Calculate autocorrelation data and reflection coefficients.
        /// </summary>
        public void GetReflection(LpcSubframeInfo subframe, int order, int blocksize, int[] samples, int samplesOffset, float[] window, int windowOffset, LpcWindowSection[] sections, int sectionsOffset)
        {
            if (autocorr_order > order)
                return;
            
            for (int i = autocorr_order; i <= order; i++) autocorr_values[i] = 0;
            for (int section = 0; section < lpc.MAX_LPC_SECTIONS; section++)
            {
                if (sections[sectionsOffset + section].m_type == LpcWindowSection.SectionType.Zero)
                {
                    continue;
                }
                if (sections[sectionsOffset + section].m_id >= 0)
                {
                    if (subframe.autocorr_section_orders[sections[sectionsOffset + section].m_id] <= order)
                    {
                        int min_order = subframe.autocorr_section_orders[sections[sectionsOffset + section].m_id];
                        for (int i = min_order; i <= order; i++) 
                            subframe.autocorr_section_values[sections[sectionsOffset + section].m_id, i] = 0;
                        
                        // Create a temp array to pass as autoc
                        double[] autocTemp = new double[lpc.MAX_LPC_ORDER + 1];
                        for (int i = 0; i < autocTemp.Length && i < subframe.autocorr_section_values.GetLength(1); i++)
                            autocTemp[i] = subframe.autocorr_section_values[sections[sectionsOffset + section].m_id, i];
                        
                        sections[sectionsOffset + section].compute_autocorr(samples, samplesOffset, window, windowOffset, min_order, order, blocksize, autocTemp, 0);
                        
                        for (int i = 0; i < autocTemp.Length && i < subframe.autocorr_section_values.GetLength(1); i++)
                            subframe.autocorr_section_values[sections[sectionsOffset + section].m_id, i] = autocTemp[i];
                        
                        subframe.autocorr_section_orders[sections[sectionsOffset + section].m_id] = order + 1;
                    }
                    for (int i = autocorr_order; i <= order; i++)
                        autocorr_values[i] += subframe.autocorr_section_values[sections[sectionsOffset + section].m_id, i];
                }
                else
                {
                    sections[sectionsOffset + section].compute_autocorr(samples, samplesOffset, window, windowOffset, autocorr_order, order, blocksize, autocorr_values, 0);
                }
            }
            lpc.compute_schur_reflection(autocorr_values, 0, (uint)order, reflection_coeffs, 0, prediction_error, 0);
            autocorr_order = order + 1;
        }

        public double Akaike(int blocksize, int order, double alpha, double beta)
        {
            return blocksize * Math.Log(prediction_error[order - 1]) + Math.Log(blocksize) * order * (alpha + beta * order);
        }

        /// <summary>
        /// Sorts orders based on Akaike's criteria
        /// </summary>
        public void SortOrdersAkaike(int blocksize, int count, int min_order, int max_order, double alpha, double beta)
        {
            for (int i = min_order; i <= max_order; i++)
                best_orders[i - min_order] = i;
            int lim = max_order - min_order + 1;
            for (int i = 0; i < lim && i < count; i++)
            {
                for (int j = i + 1; j < lim; j++)
                {
                    if (Akaike(blocksize, best_orders[j], alpha, beta) < Akaike(blocksize, best_orders[i], alpha, beta))
                    {
                        int tmp = best_orders[j];
                        best_orders[j] = best_orders[i];
                        best_orders[i] = tmp;
                    }
                }
            }
        }

        /// <summary>
        /// Produces LPC coefficients from autocorrelation data.
        /// </summary>
        public void ComputeLPC(float[] lpcs, int lpcsOffset)
        {
            lpc.compute_lpc_coefs((uint)autocorr_order - 1, reflection_coeffs, 0, lpcs, lpcsOffset);
        }

        public double[] autocorr_values;
        double[] reflection_coeffs;
        public double[] prediction_error;
        public int[] best_orders;
        public int[] coefs;
        int autocorr_order;
        public int shift;

        public double[] Reflection
        {
            get
            {
                return reflection_coeffs;
            }
        }

        public uint[] done_lpcs;
    }
}
