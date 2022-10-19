using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tests
{
    public class PythonEvaluator
    {
        public static string Evaluate(string expression)
        {
            try
            {
                expression = expression.ToLower().Replace("&", "**");
                var procStartInfo = new ProcessStartInfo("python", "-c \"from math import *; r=" + expression + "; print(r if r<1e14 else '{0:.12e}'.format(r))\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var proc = new Process
                {
                    StartInfo = procStartInfo
                };
                proc.Start();
                var result = proc.StandardOutput.ReadToEnd();
                return result == "" ? "ERROR" : result.Trim();
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); return "ERROR"; }
        }
    }
}
