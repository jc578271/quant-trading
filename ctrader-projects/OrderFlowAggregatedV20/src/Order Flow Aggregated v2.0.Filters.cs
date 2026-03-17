using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using static cAlgo.OrderFlowTicksV20;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;

namespace cAlgo
{
    public static class Filters
    {
        // logic generated/converted by LLM
        public static double RollingPercentile(double[] window)
        {
            if (window == null || window.Length == 0)
                return 0.0;

            double last = window[window.Length - 1];
            int count = 0;

            for (int i = 0; i < window.Length; i++)
            {
                if (window[i] <= last)
                    count++;
            }

            return 100.0 * count / window.Length;
        }
        public static double PowerSoftmax_Strength(double[] window, double alpha = 1.0)
        {
            if (window == null || window.Length == 0)
                return 0.0;

            double sum = 0.0;
            double lastP = 0.0;

            for (int i = 0; i < window.Length; i++)
            {
                double w = Math.Max(window[i], 1e-12);
                double p = Math.Pow(w, alpha);

                sum += p;

                if (i == window.Length - 1)
                    lastP = p;
            }

            return sum != 0.0 ? lastP / sum : 0.0;
        }

        public static double[] PowerSoftmax_Profile(double[] window, double alpha = 1.0)
        {
            int n = window.Length;
            double[] result = new double[n];

            if (n == 0)
                return result;

            // First pass: compute powered values
            double sum = 0.0;
            for (int i = 0; i < n; i++)
            {
                double w = Math.Max(window[i], 1e-12);
                double p = Math.Pow(w, alpha);

                result[i] = p;
                sum += p;
            }

            // Second pass: normalize
            if (sum != 0.0)
            {
                for (int i = 0; i < n; i++)
                    result[i] /= sum;
            }

            return result;
        }


        public static double L1Norm_Strength(double[] window)
        {
            if (window == null || window.Length == 0)
                return 0.0;

            double denom = 0.0;
            for (int i = 0; i < window.Length; i++)
                denom += Math.Abs(window[i]);

            return denom != 0.0
                ? window[window.Length - 1] / denom
                : 1.0;
        }
        public static double[] L1Norm_Profile(double[] window)
        {
            int n = window.Length;
            double[] result = new double[n];

            if (n == 0)
                return result;

            double denom = 0.0;
            for (int i = 0; i < n; i++)
                denom += Math.Abs(window[i]);

            for (int i = 0; i < n; i++) {
                if (denom != 0.0)
                    result[i] = window[i] / denom;
                else
                    result[i] = 1.0;
            }

            return result;
        }

        public static double L2Norm_Strength(double[] window)
        {
            if (window == null || window.Length == 0)
                return 0.0;

            double sumSq = 0.0;

            for (int i = 0; i < window.Length; i++)
                sumSq += window[i] * window[i];

            double denom = Math.Sqrt(sumSq);

            return denom != 0.0
                ? window[window.Length - 1] / denom
                : 0.0;
        }

        public static double MinMax_Strength(double[] window)
        {
            if (window == null || window.Length == 0)
                return 0.0;

            double min = double.MaxValue;
            double max = double.MinValue;

            for (int i = 0; i < window.Length; i++)
            {
                double v = window[i];
                if (v < min) min = v;
                if (v > max) max = v;
            }

            double range = max - min;
            if (range <= 0.0)
                return 0.0;

            return (window[window.Length - 1] - min) / range;
        }

    }
}
