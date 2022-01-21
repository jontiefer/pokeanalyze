using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeAnalyze.Types
{
    //public class PokemonGraphQLResponse
    //{
    //    public Data data;
    //}

    public class PokemonGraphQLResponse
    {
        public Gen3Species[] gen3Species;

        public class Gen3Species
        {
            public string name;
            public int id;
            public PokemonData[] pokemons;

            public class PokemonData
            {
                public int weight;
                public int height;
                public PokemonDataTypes[] pokemonDataTypes;

                public class PokemonDataTypes
                {
                    public PokemonDataType pokemonDataType;

                    public class PokemonDataType
                    {
                        public string name;
                    }
                }
            }
        }
    }

}
