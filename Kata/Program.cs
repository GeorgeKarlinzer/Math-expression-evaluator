using Evaluation;
using System;
using System.Text.RegularExpressions;



var ev = new Evaluate();

var res = ev.eval("sqrt(--7&(-7+3--3+--7))");
Console.WriteLine(res);