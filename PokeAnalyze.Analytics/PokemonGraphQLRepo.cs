using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL;
using PokeAnalyze.GraphQL;
using PokeAnalyze.Types;

namespace PokeAnalyze.Analytics
{
    public class PokemonGraphQLRepo
    {
        private const string Url = "https://beta.pokeapi.co/graphql/v1beta";
        private readonly GraphQLHttpService _graphQlHttpService;

        public PokemonGraphQLRepo()
        {
            _graphQlHttpService = new GraphQLHttpService(Url);
        }

        public async Task<GraphQLResponse<PokemonGraphQLResponse>> QueryPokemonItems()
        {
            var pokemonItemsRequest = new GraphQLRequest()
            {
                Query = @"
                    query samplePokeAPIquery {
                        Gen3Species: pokemon_v2_pokemonspecies(where: {pokemon_v2_generation: {name: {_eq: ""generation - iii""}}, pokemon_v2_pokemons: {id: {_gte: 1}}}, order_by: {id: asc}) {
                                name
                                id
                                PokemonData: pokemon_v2_pokemons {
                                weight
                                height
                                PokemonDataTypes: pokemon_v2_pokemontypes {
                                    PokemonDataType: pokemon_v2_type {
                                        name
                                    }
                                }
                            }
                        }
                    }
                ",
                OperationName = "samplePokeAPIquery",
            };

            return await _graphQlHttpService.SendQueryAsync<PokemonGraphQLResponse>(pokemonItemsRequest);
        }
    }
}
