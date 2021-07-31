using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokeAnalyze.Analytics;
using Xunit;

namespace PokeAnalyze.Tests
{
    public class AnalyzerTests
    {
        [Fact]
        public async Task ContainsPsychicType_ShouldWork()
        {
            var analyzer = new PokemonAnalyzer();
            var results = await analyzer.CalculatePokemonHeightWeightAverages(100, 200);

            var pokemonTypes = results.PokemonAveragesByType.Select(a => a.PokemonType).ToList();
            Assert.Contains("psychic", pokemonTypes);
        }

        [Fact]
        public async Task ContainsPsychicType_ShouldFail()
        {
            var analyzer = new PokemonAnalyzer();
            var results = await analyzer.CalculatePokemonHeightWeightAverages(2, 220);

            var pokemonTypes = results.PokemonAveragesByType.Select(a => a.PokemonType).ToList();
            Assert.Contains("psychic", pokemonTypes);
        }
    }
}
