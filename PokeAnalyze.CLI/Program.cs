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
            int fetchType = 1;

#if DEBUG
            args[0] = "100";
            args[1] = "0";
            args[2] = "1";
#endif

            if (args.Length == 0)
                limit = 2000;
            else
            {
                limit = Convert.ToInt32(args[0]);

                if (args.Length > 1)
                    offset = Convert.ToInt32(args[1]);

                if (args.Length > 2)
                    fetchType = Convert.ToInt32(args[2]);
            }

            PokemonAnalyzer analyzer = new PokemonAnalyzer();
            await analyzer.CalculatePokemonHeightWeightAverages(limit, offset, fetchType);
            
            //FOR TESTING
            //analyzer.TestSpeed();
        }
    }
}
