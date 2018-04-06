using System;
using System.Collections.Generic;
using System.Text;

namespace FindAnswer
{
    class TextProcessor
    {
        public static int FindNumberOfAnswersInString(string textToSearch, string textToSearchFor)
        {
            textToSearch = textToSearch.ToLowerInvariant();
            textToSearchFor = textToSearchFor.ToLowerInvariant();

            int foundIndex = textToSearch.IndexOf(textToSearchFor);
            int foundResults = 0;
            while (foundIndex != -1)
            {
                foundResults++;
                try
                {
                    textToSearch = textToSearch.Substring(foundIndex + textToSearchFor.Length);
                    foundIndex = textToSearch.IndexOf(textToSearchFor);
                }
                catch
                {
                    return foundResults;
                }
            }

            return foundResults;
        }
    }
}
