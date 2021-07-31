using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PokeAnalyze.Types
{
    public class PokemonQueryList
    {
        [JsonPropertyName("results")]
        public List<PokemonQueryItem> Items { get; set; }
    }

    public class PokemonQueryItem
    {
        //public string Name { get; set; }
        public string Url { get; set; }
    }
}
