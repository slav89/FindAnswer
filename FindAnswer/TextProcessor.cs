using System;
using System.Collections.Generic;
using System.Text;

namespace FindAnswer
{
    class TextProcessor
    {
        public static string normalize(string orig)
        {
            string result = orig.ToLowerInvariant();
            var strs = new string[]
            {
                "ˈ",  
                "\"", "'", "`", "\'",
                "‘", "’", "“", "”",
                ",", "،", "、",
                    "‹", "›", "«", "»",
                    "&", "′", "″", "‴"
//                "é", "ɑ"
            };
            foreach (var str in strs)
            {
                result = result.Replace(str, "");
            }

            return result;
        }

        public static int FindNumberOfAnswersInString(string textToSearch, string textToSearchFor, int fuzzyness = 0)
        {
            textToSearch = normalize(textToSearch);

            textToSearchFor = normalize(textToSearchFor);

            int foundIndex = FuzzySearch(textToSearch, textToSearchFor, fuzzyness);
            int foundResults = 0;
            while (foundIndex != -1)
            {
                foundResults++;
                try
                {
                    textToSearch = textToSearch.Substring(foundIndex + textToSearchFor.Length);
                    foundIndex = FuzzySearch(textToSearch, textToSearchFor, fuzzyness);
                }
                catch
                {
                    return foundResults;
                }
            }

            return foundResults;
        }

        public static int FuzzySearch(string text, string pattern, int k)
        {
            int result = -1;
            int m = pattern.Length;
            int[] R;
            int[] patternMask = new int[70240];
            int i, d;

            if (string.IsNullOrEmpty(pattern)) return 0;
            if (m > 31) return -1; //Error: The pattern is too long!

            R = new int[(k + 1) * sizeof(int)];
            for (i = 0; i <= k; ++i)
                R[i] = ~1;

            for (i = 0; i <= 127; ++i)
                patternMask[i] = ~0;

            for (i = 0; i < m; ++i)
                patternMask[pattern[i]] &= ~(1 << i);

            for (i = 0; i < text.Length; ++i)
            {
                int oldRd1 = R[0];

                try
                {
                    R[0] |= patternMask[text[i]];
                }
                catch (Exception e)
                {
                    Console.Write(text[i]);
                    throw;
                }
                
                R[0] <<= 1;

                for (d = 1; d <= k; ++d)
                {
                    int tmp = R[d];

                    R[d] = (oldRd1 & (R[d] | patternMask[text[i]])) << 1;
                    oldRd1 = tmp;
                }

                if (0 == (R[k] & (1 << m)))
                {
                    result = (i - m) + 1;
                    break;
                }
            }

            return result;
        }
    }
}
