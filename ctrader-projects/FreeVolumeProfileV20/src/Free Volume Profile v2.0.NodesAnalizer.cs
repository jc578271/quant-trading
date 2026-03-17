using cAlgo.API;
using cAlgo.API.Indicators;
using static cAlgo.FreeVolumeProfileV20;
using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;

namespace cAlgo
{
    // ================ HVN + LVN ================
    public static class NodesAnalizer {
        
        public static double[] FixedKernel(double sigma = 2.0) {
            int radius = (int)(3 * sigma);
            int size = radius * 2 + 1;

            double[] kernel = new double[size];
            
            double sigma2 = sigma * sigma;
            double twoSigma2 = 2.0 * sigma2;
            double invSigma2 = 1.0 / twoSigma2;

            double sum = 0.0;
            for (int i = -radius; i <= radius; i++)
            {
                double v = Math.Exp(-(i * i) * invSigma2);
                kernel[i + radius] = v;
                sum += v;
            }

            // Normalize
            double invSum = 1.0 / sum;
            for (int i = 0; i < size; i++)
                kernel[i] *= invSum;

            return kernel;
        }

        public static double[] FixedCoefficients(int windowSize = 9) {
            if (windowSize % 2 == 0)
                throw new ArgumentException("windowSize must be odd");
            
            int polyOrder = 3;
            if (polyOrder >= windowSize)
                throw new ArgumentException("polyOrder must be < windowSize");

            int half = windowSize / 2;
            int size = windowSize;
            int cols = polyOrder + 1;

            // --- Design matrix A ---
            double[,] A = new double[size, cols];
            double power = 1.0;
            for (int i = -half; i <= half; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    A[i + half, j] = power;
                    power *= i;
                }
            }

            // --- Pseudoinverse (AᵀA)⁻¹Aᵀ ---
            double[,] AT = Transpose(A);
            double[,] ATA = Multiply(AT, A);
            double[,] ATAInv = Invert(ATA);
            double[,] pinv = Multiply(ATAInv, AT);

            // First row = smoothing coefficients
            double[] coeffs = new double[size];
            for (int i = 0; i < size; i++)
                coeffs[i] = pinv[0, i];

            return coeffs;
        }
        
        // === Smoothing ==
        // logic generated/converted by LLM
        // Added fixed kernel/coefficients
        public static double[] GaussianSmooth(double[] arr, double[] fixedKernel = null, double sigma = 2.0)
        {
            int radius = (int)(3 * sigma);

            fixedKernel ??= Array.Empty<double>();
            
            double[] kernel;
            if (fixedKernel.Length == 0)
            {
                int size = radius * 2 + 1;
                kernel = new double[size];

                // Build kernel
                double sum = 0.0;
                for (int i = -radius; i <= radius; i++)
                {
                    double value = Math.Exp(-(i * i) / (2.0 * sigma * sigma));
                    kernel[i + radius] = value;
                    sum += value;
                }

                // Normalize kernel
                for (int i = 0; i < size; i++)
                    kernel[i] /= sum;
            }
            else
                kernel = fixedKernel;

            int n = arr.Length;
            double[] result = new double[n];

            // Convolution (mode="same")
            for (int i = 0; i < n; i++)
            {
                double acc = 0.0;

                for (int k = -radius; k <= radius; k++)
                {
                    int idx = i + k;
                    if (idx >= 0 && idx < n)
                        acc += arr[idx] * kernel[k + radius];
                }

                result[i] = acc;
            }

            return result;
        }

        public static double[] SavitzkyGolay(double[] y, double [] fixedCoeff = null, int windowSize = 9)
        {
            if (windowSize % 2 == 0)
                throw new ArgumentException("windowSize must be odd");
            
            int polyOrder = 3;
            if (polyOrder >= windowSize)
                throw new ArgumentException("polyOrder must be < windowSize");

            fixedCoeff ??= Array.Empty<double>();
            
            double[] coeffs;
            if (fixedCoeff.Length == 0)
                coeffs = FixedCoefficients(windowSize);
            else
                coeffs = fixedCoeff;
                
            int half = windowSize / 2;
            int size = windowSize;
            
            // --- Pad signal (edge mode) ---
            int n = y.Length;
            double[] padded = new double[n + 2 * half];

            for (int i = 0; i < half; i++)
                padded[i] = y[0];

            for (int i = 0; i < n; i++)
                padded[i + half] = y[i];
            
            for (int i = 0; i < half; i++)
                padded[n + half + i] = y[n - 1];

            // --- Convolution (valid) ---
            double[] result = new double[n];

            for (int i = 0; i < n; i++)
            {
                double acc = 0.0;
                for (int j = 0; j < size; j++)
                    acc += padded[i + j] * coeffs[size - 1 - j];

                result[i] = acc;
            }

            return result;
        }
        private static double[,] Transpose(double[,] m)
        {
            int r = m.GetLength(0);
            int c = m.GetLength(1);
            double[,] t = new double[c, r];

            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    t[j, i] = m[i, j];

            return t;
        }
        private static double[,] Multiply(double[,] a, double[,] b)
        {
            int r = a.GetLength(0);
            int c = b.GetLength(1);
            int n = a.GetLength(1);

            double[,] m = new double[r, c];

            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    for (int k = 0; k < n; k++)
                        m[i, j] += a[i, k] * b[k, j];

            return m;
        }
        private static double[,] Invert(double[,] m)
        {
            int n = m.GetLength(0);
            double[,] a = new double[n, n * 2];

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    a[i, j] = m[i, j];
                    a[i, j + n] = (i == j) ? 1.0 : 0.0;
                }

            for (int i = 0; i < n; i++)
            {
                double diag = a[i, i];
                for (int j = 0; j < n * 2; j++)
                    a[i, j] /= diag;

                for (int k = 0; k < n; k++)
                {
                    if (k == i) continue;
                    double factor = a[k, i];
                    for (int j = 0; j < n * 2; j++)
                        a[k, j] -= factor * a[i, j];
                }
            }

            double[,] inv = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    inv[i, j] = a[i, j + n];

            return inv;
        }

        // === Volume Node => Detection
        public static (List<int> maximum, List<int> minimum) FindLocalMinMax(double[] arr)
        {
            List<int> minimum = new();
            List<int> maximum = new();

            int n = arr.Length;
            if (n < 3)
                return (maximum, minimum);

            for (int i = 1; i < n - 1; i++)
            {
                if (arr[i] < arr[i - 1] && arr[i] < arr[i + 1])
                    minimum.Add(i);

                if (arr[i] > arr[i - 1] && arr[i] > arr[i + 1])
                    maximum.Add(i);
            }
            
            return (maximum, minimum);
        }
        public static (List<int> peaks, List<int> valleys) ProfileTopology(double[] profile)
        {
            int n = profile.Length;

            List<int> peaks = new();
            List<int> valleys = new();

            if (n < 3)
                return (peaks, valleys);

            // --- First derivative ---
            double[] d1 = new double[n];
            for (int i = 1; i < n - 1; i++)
                d1[i] = (profile[i + 1] - profile[i - 1]) * 0.5;

            d1[0] = profile[1] - profile[0];
            d1[n - 1] = profile[n - 1] - profile[n - 2];

            // --- Second derivative ---
            double[] d2 = new double[n];
            for (int i = 1; i < n - 1; i++)
                d2[i] = (d1[i + 1] - d1[i - 1]) * 0.5;

            // --- Peak & Valley detection ---
            for (int i = 1; i < n - 1; i++)
            {
                double s1 = Math.Sign(d1[i - 1]);
                double s2 = Math.Sign(d1[i]);

                // Peak (HVN / POC)
                if (s1 > 0 && s2 < 0 && d2[i] < 0)
                    peaks.Add(i);

                // Valley (LVN)
                if (s1 < 0 && s2 > 0 && d2[i] > 0)
                    valleys.Add(i);
            }
            
            return (peaks, valleys);
        }
        public static (List<int> hvnIdx, List<int> lvnIdx) PercentileNodes(double[] profile, int hvnPct, int lvnPct)
        {
            List<int> hvnIdx = new();
            List<int> lvnIdx = new();

            if (profile.Length == 0)
                return (hvnIdx, lvnIdx);

            double hvnThreshold = Percentile(profile, hvnPct);
            double lvnThreshold = Percentile(profile, lvnPct);

            for (int i = 0; i < profile.Length; i++)
            {
                if (profile[i] >= hvnThreshold)
                    hvnIdx.Add(i);

                if (profile[i] <= lvnThreshold)
                    lvnIdx.Add(i);
            }
            
            return (hvnIdx, lvnIdx);
        }

        private static double Percentile(double[] data, double percentile)
        {
            if (data.Length == 0)
                return 0.0;

            double[] copy = (double[])data.Clone();
            Array.Sort(copy);

            double pos = (percentile / 100.0) * (copy.Length - 1);
            int lo = (int)Math.Floor(pos);
            int hi = (int)Math.Ceiling(pos);

            if (lo == hi)
                return copy[lo];

            double frac = pos - lo;
            return copy[lo] * (1.0 - frac) + copy[hi] * frac;
        }
        
        // === Volume Node => Levels
        public static (int Low, int High) HVN_SymmetricVA(int startIdx, int endIdx, int pocIdx, double vaPct = 0.70)
        {
            int width = endIdx - startIdx;
            int half = (int)(width * vaPct / 2.0);

            int low = Math.Max(startIdx, pocIdx - half);
            int high = Math.Min(endIdx, pocIdx + half);

            return (low, high);
        }
        public static (int Low, int High) LVN_SymmetricBand(int lvn, int nextLvn, double bandPct = 0.25)
        {
            int width = nextLvn - lvn;
            int radius = (int)(width * bandPct / 2.0);

            int low = Math.Max(0, lvn - radius);
            int high = Math.Min(nextLvn, lvn + radius);

            return (low, high);
        }
        public static List<List<int>> GroupConsecutiveIndexes(IList<int> indices)
        {
            var groups = new List<List<int>>();

            if (indices == null || indices.Count == 0)
                return groups;

            var current = new List<int> { indices[0] };
            groups.Add(current);

            for (int i = 1; i < indices.Count; i++)
            {
                if (indices[i] == indices[i - 1] + 1) 
                    current.Add(indices[i]);
                else {
                    current = new List<int> { indices[i] };
                    groups.Add(current);
                }
            }

            return groups;
        }
    }
}
