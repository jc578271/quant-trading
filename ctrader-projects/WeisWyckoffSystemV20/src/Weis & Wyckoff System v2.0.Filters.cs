using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using static cAlgo.WeisWyckoffSystemV20;
using System;
using System.Linq;
using System.Globalization;
using System.Threading;
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
        public static double RollingPercentile(double[] window)
        {
            // generated/converted by LLM
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

        public static double L1NormStrength(double[] window)
        {
            // generated/converted by LLM
            if (window == null || window.Length == 0)
                return 0.0;

            double denom = 0.0;

            for (int i = 0; i < window.Length; i++)
                denom += Math.Abs(window[i]);

            return denom != 0.0
                ? window[window.Length - 1] / denom
                : 1.0;
        }
    }

}
