using System;
using System.Text.RegularExpressions;

namespace FindAnswer
{
    public class QuestionSplitter
    {
        string _text;
        string[] _cases = null;

        public QuestionSplitter(string text)

        {
            _text = text;
        }

        public string GetQuestion()
        {
            return _text.Contains("CASH")
                       ? GetQuestionCashShow()
                            : GetQuestionHq();
        }

        public string GetQuestionCashShow()
        {
            Regex regex = new Regex("ound(.*)\\?", RegexOptions.Singleline);

            Match match = regex.Match(_text);

            Regex regex1 = new Regex("\n(.*)\\?", RegexOptions.Singleline);

            var m = regex1.Match(match.Value.Split("\n", 2)[1]);
            var question = m.Groups[1].Value;
            if (question.StartsWith("Spectator"))
                question = question.Split("Spectator", 2)[1];

            var tidyquestion = question.Replace("\n", " ").Replace("-", "").Trim();

            return tidyquestion;
        }

        public string GetQuestionHq()
        {           
            Regex regex = new Regex("\n[0-9][0-9]?\n(.*)\\?", RegexOptions.Singleline);
            Match match = regex.Match(_text);

            if (!match.Success)
            {
                regex = new Regex("\nна\n(.*)\\?", RegexOptions.Singleline);
                match = regex.Match(_text);
            }
            if (!match.Success)
            {
                regex = new Regex("Eliminated\n(.*)\\?", RegexOptions.Singleline);
                match = regex.Match(_text);
            }
            if (!match.Success)
            {
                regex = new Regex(" up!\n(.*)\\?", RegexOptions.Singleline);
                match = regex.Match(_text);
            }
            if (!match.Success)
            {
                regex = new Regex("\n[0-9][0-9]?\n(.*l)\n", RegexOptions.Singleline);
                match = regex.Match(_text);
            }

            var question = match.Groups[1].Value;

            if (question.Equals(string.Empty))
            {
                Console.Beep();
                return "";
            }

            if (question.Contains("?"))
            {
                question = question.Split("?", 2)[0];
            }

            if (question.StartsWith("Eliminated"))
            {
                question = question.Replace("Eliminated", "");
            }

            var tidyquestion = question.Replace("\n", " ").Replace("-", "").Trim();

            return tidyquestion;
        }

        public string[] GetCases()
        {
            if (_cases == null)
            {
                Regex regex = new Regex("\\?\n([a-z,0-9, ]*\n.*\n.*)\n", RegexOptions.Singleline);
                Match match = regex.Match(_text);

                if (!match.Success)
                {
                    regex = new Regex("l\n([a-z,0-9, ]*\n.*\n.*)\n", RegexOptions.Singleline);
                    match = regex.Match(_text);
                }

                var casesString = _text.Split("?", 2)[1];

                Regex reg = new Regex("\nPrize.*\n");

                var m = reg.Match(casesString);


                if (m.Success)
                {
                    casesString = casesString.Replace(m.Groups[0].Value, "\n");
                }


                _cases = casesString.Split("\n", 5);

                if (_cases.Length < 5)
                {
                    casesString = _text.Split("l\n", 2)[1];
                    casesString = "\n" + casesString;
                    _cases = casesString.Split("\n", 5);
                }
            }
            return _cases;
        }

        public string GetCaseA()
        {
            return GetCases()[1];
        }

        public string GetCaseB()
        {
            return GetCases()[2];
        }

        public string GetCaseC()
        {
            return GetCases()[3];
        }
    }
}
