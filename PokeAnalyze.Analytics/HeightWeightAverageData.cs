using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeAnalyze.Analytics
{
    public class HeightWeightAverageData
    {
        public HeightWeightAverageData()
        {
            
        }

        public double PokemonAverageHeight { get; set; }
        public double PokemonAverageWeight { get; set; }

        public List<HeightWeightAveragesByTypeData> PokemonAveragesByType { get; } =
            new List<HeightWeightAveragesByTypeData>();
    }

    public class HeightWeightAveragesByTypeData
    {
        public string PokemonType { get; set; }
        public double PokemonAverageHeight { get; set; }
        public double PokemonAverageWeight { get; set; }
    }
}
