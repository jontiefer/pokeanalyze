using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PokeAnalyze.Analytics;

namespace PokeAnalyze.CLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int limit = 0;
            int offset = 0;

            if (args.Length == 0)
                limit = 2000;
            else
            {
                limit = Convert.ToInt32(args[0]);

                if (args.Length > 1)
                    offset = Convert.ToInt32(args[1]);
            }

            PokemonAnalyzer analyzer = new PokemonAnalyzer();
            await analyzer.CalculatePokemonHeightWeightAverages(limit, offset);
            //FOR TESTING
            //await analyzer.TestSpeed();
        }
    }
}
