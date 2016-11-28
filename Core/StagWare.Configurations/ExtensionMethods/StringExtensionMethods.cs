using System;
using System.Collections.Generic;
using System.Linq;

namespace StagWare.ExtensionMethods
{
    public static class StringExtensionMethods
    {
        public static string GetLongestCommonSubstring(this string self, string str)
        {
            List<string> list = GetLongestCommonSubstrings(self, str);

            if(list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return string.Empty;
            }
        }

        public static List<string> GetLongestCommonSubstrings(this string self, string str)
        {
            if(str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            string minString, maxString;

            if(self.Length > str.Length)
            {
                maxString = self;
                minString = str;
            }
            else
            {
                maxString = str;
                minString = self;
            }

            var lookup = new int[2][] { new int[minString.Length], new int[minString.Length] };
            var substrEndIndices = new List<int>();
            int substrLength = 0;

            for(int i = 0; i < maxString.Length; i++)
            {
                int rowIdx = i % 2;

                for(int j = 0; j < minString.Length; j++)
                {
                    if(maxString[i] == minString[j])
                    {
                        if ((i == 0) || (j == 0))
                        {
                            lookup[rowIdx][j] = 1;
                        }
                        else
                        {
                            lookup[rowIdx][j] = lookup[(rowIdx + 1) % 2][j - 1] + 1;
                        }

                        if(lookup[rowIdx][j] > substrLength)
                        {
                            substrLength = lookup[rowIdx][j];
                            substrEndIndices.Clear();
                            substrEndIndices.Add(i);
                        }
                        else if(lookup[rowIdx][j] == substrLength)
                        {
                            substrEndIndices.Add(i);
                        }
                    }
                    else
                    {
                        lookup[rowIdx][j] = 0;
                    }
                }
            }

            return substrEndIndices
                .Select(x => maxString.Substring((x - substrLength) + 1, substrLength))
                .ToList();
        }
    }
}
