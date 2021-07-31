using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokeAnalyze.Analytics;
using PokeAnalyze.Tools;
using Xunit;

namespace PokeAnalyze.Tests
{
    public class RepoTests
    {
        [Theory]
        [InlineData(20, 40, 20)]
        [InlineData(100, 50, 100)]
        public async Task QueryPokemonRecords_ShouldWork(int limit, int offset, int resultCount)
        {
            PokemonRepo repo = new PokemonRepo();
            var pokemonQuery = await repo.QueryPokemonSet(limit, offset);

            Assert.True(pokemonQuery.Items.Count == resultCount);
        }

        [Theory]
        [InlineData(200, 50, 7, 333)]
        [InlineData(400, 75, 14, 3000)]
        public async Task GetFirstPokemonItemHeightWeightInQuery_ShouldWork(int limit, int offset, int pokemonHeight, int pokemonWeight)
        {
            PokemonRepo repo = new PokemonRepo();
            var pokemonQuery = await repo.QueryPokemonSet(limit, offset);

            var pokemon = await repo.GetPokemonItem(pokemonQuery.Items[0].Url);

            Assert.True(pokemon.Height == pokemonHeight && pokemon.Weight == pokemonWeight);
        }
    }
}
