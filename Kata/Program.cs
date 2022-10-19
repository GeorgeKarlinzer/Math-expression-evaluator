using Evaluation;
using System;
using System.Text.RegularExpressions;



var ev = new Evaluate();

var res = ev.eval("((   ((( -8   ) )  ))-1)");
Console.WriteLine(res);