using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PokeAnalyze.Types
{
    public class PokemonType
    {
        public int Slot { get; set; }
        public PokemonTypeMap Type { get; set; }
    }

    public struct PokemonTypeMap
    {

        public string Name { get; set; }
        //public string Url { get; set; }
    }
}
