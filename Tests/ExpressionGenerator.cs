using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class ExpressionGenerator
    {
        private readonly string[] twoArgsFuncs = new[]
        {
            "+", "-", "*", "/", "&"
        };

        private readonly string[] oneArgFuncs = new[]
        {
            "sin", "cos", "tan", "asin", "acos", "atan",
            "sinh", "cosh", "tanh",
            "log", "ln", "exp", "sqrt", "abs"
        };

        private readonly int minN = -20;
        private readonly int maxN = 20;
        private readonly int minL = 1;
        private readonly int maxL = 10;

        private readonly int twoArgsFuncProbability = 80;
        private readonly int bracketsProbability = 20;

        private T PopRandom<T>(List<T> list, Random rand)
        {
            var randIndex = rand.Next(0, list.Count);
            var item = list[randIndex];
            list.RemoveAt(randIndex);
            return item;
        }

        public string Generate(int? seed = null)
        {
            var rand = seed is not null ? new Random((int)seed) : new Random();

            var l = rand.Next(minL, maxL);
            var nums = new List<string>();
            var actions = new List<string>();

            var hasOneArgFunc = false;
            for (int i = 0; i < l; i++)
            {
                actions.Add(twoArgsFuncs[rand.Next(0, twoArgsFuncs.Length)]);
                nums.Add(rand.Next(minN, maxN).ToString());
                if (rand.Next(0, 100) > twoArgsFuncProbability)
                {
                    actions.Add(oneArgFuncs[rand.Next(0, oneArgFuncs.Length)]);
                    nums.Add(rand.Next(minN, maxN).ToString());
                    hasOneArgFunc = true;
                }
            }

            if (hasOneArgFunc)
            {
                actions.RemoveAt(0);
                nums.RemoveAt(0);
            }
            else
                nums.Add(rand.Next(minN, maxN).ToString());

            while (actions.Any())
            {
                var arg1 = PopRandom(nums, rand);
                var action = PopRandom(actions, rand);
                string newArg;
                if (twoArgsFuncs.Contains(action))
                {
                    var arg2 = PopRandom(nums, rand);
                    if (action == "&" && arg2[0] == '-')
                        newArg = $"{arg1} {action} ({arg2})";
                    else
                        newArg = $"{arg1} {action} {arg2}";

                    if (rand.Next(100) < bracketsProbability)
                        newArg = $"({newArg})";
                }
                else
                {
                    if (arg1[0] == '(') newArg = $"{action}{arg1}";
                    else newArg = $"{action}({arg1})";
                }

                nums.Add(newArg);
            }

            return nums[0];
        }
    }
}
