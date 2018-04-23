using System;
using FindAnswer;

namespace SandBox
{
    class Program
    {
        static void Main(string[] args)
        {
            Rebackfill();
        }

        static void Rebackfill()
        {
            var backfiller = new Backfiller();
            var analyzer = new Analyzer();

            var sets = analyzer.LoadQuestionDataSets();
            backfiller.ReBackfill(sets);

        }

        static void Explore(){
            var analyzer = new Analyzer();
            analyzer.Explore();
        }
    }
}
