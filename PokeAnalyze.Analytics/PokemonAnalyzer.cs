using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using PokeAnalyze.Types;

namespace PokeAnalyze.Analytics
{
    public class PokemonAnalyzer
    {
        private readonly PokemonRepo _repo;

        private int _pokemonHeightTotal = 0;
        private int _pokemonWeightTotal = 0;

        private List<string> _pokemonTypes = null;
        private ConcurrentDictionary<string, int> _pokemonTypeCount = null;
        private ConcurrentDictionary<string, int> _pokemonTypeHeightTotal = null;
        private ConcurrentDictionary<string, int> _pokemonTypeWeightTotal = null;

        private DateTime _timeStart;

        private static object _lock = new object();

        public PokemonAnalyzer()
        {
            _repo = new PokemonRepo();
        }

        private void InitCalcData()
        {
            _pokemonHeightTotal = 0;
            _pokemonWeightTotal = 0;
            _pokemonTypes = new List<string>();
            _pokemonTypeCount = new ConcurrentDictionary<string, int>();
            _pokemonTypeHeightTotal = new ConcurrentDictionary<string, int>();
            _pokemonTypeWeightTotal = new ConcurrentDictionary<string, int>();
        }

        public double ElapsedTime { get; private set; }

        public void TestSpeed()
        {
            _timeStart = DateTime.Now;
            _repo.SpeedTest(100);

            ElapsedTime = DateTime.Now.Subtract(_timeStart).TotalSeconds;

            Console.WriteLine(new string('-', 25));
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine($"*** Total Time (Seconds): {ElapsedTime} ***");
        }

        public async Task<HeightWeightAverageData> CalculatePokemonHeightWeightAverages(int limit, int offset)
        {
            _timeStart = DateTime.Now;

            InitCalcData();
            var pokemonQueryList = await _repo.QueryPokemonSet(limit, offset);

            Task.WhenAll(Enumerable.Range(0, pokemonQueryList.Items.Count - 1)
                .Select(async i =>
                {
                    var pokemon = await _repo.GetPokemonItem(pokemonQueryList.Items[i].Url);

                    _pokemonHeightTotal += pokemon.Height;
                    _pokemonWeightTotal += pokemon.Weight;

                    AddHeightWeightByTypeData(pokemon);
                })).GetAwaiter().GetResult();

            double pokemonHeightAvg = _pokemonHeightTotal / Convert.ToDouble(pokemonQueryList.Items.Count);
            double pokemonWeightAvg = _pokemonWeightTotal / Convert.ToDouble(pokemonQueryList.Items.Count);

            var pokemonHeightAvgByType = CalculateAveragesByType(_pokemonTypeHeightTotal);
            var pokemonWeightAvgByType = CalculateAveragesByType(_pokemonTypeWeightTotal);

            Console.WriteLine(new string('-', 25));
            Console.WriteLine($"Pokemon Average Height: {pokemonHeightAvg}");
            Console.WriteLine($"Pokemon Average Weight: {pokemonWeightAvg}");
            Console.WriteLine(new string('-', 25));

            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine(new string('-', 25));
            Console.WriteLine("Average Heights By Type:\r\n");

            string avgHeightByTypeOutput = "";
            pokemonHeightAvgByType.ForEach(a => avgHeightByTypeOutput += $"Type Name: {a.Item1} - Average: {a.Item2}\r\n");

            Console.WriteLine(avgHeightByTypeOutput);
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Average Weights By Type:\r\n");
            string avgWeightByTypeOutput = "";
            pokemonWeightAvgByType.ForEach(a => avgWeightByTypeOutput += $"Type Name: {a.Item1} - Average: {a.Item2}\r\n");

            Console.WriteLine(avgWeightByTypeOutput);

            ElapsedTime = DateTime.Now.Subtract(_timeStart).TotalSeconds;

            Console.WriteLine(new string('-', 25));
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine($"*** Total Time (Seconds): {ElapsedTime} ***");

            var averagesData = GeneratePokemonAveragesDataObject(pokemonHeightAvg, pokemonWeightAvg,
                pokemonHeightAvgByType, pokemonWeightAvgByType);

            return averagesData;
        }

        private void AddHeightWeightByTypeData(Pokemon pokemon)
        {
            foreach (var pokType in pokemon.Types)
            {
                lock (_lock)
                {
                    if (!_pokemonTypeHeightTotal.ContainsKey(pokType.Type.Name))
                    {
                        _pokemonTypes.Add(pokType.Type.Name);
                        _pokemonTypeCount[pokType.Type.Name] = 1;
                        _pokemonTypeHeightTotal[pokType.Type.Name] = pokemon.Height;
                        _pokemonTypeWeightTotal[pokType.Type.Name] = pokemon.Weight;
                    }
                    else
                    {
                        _pokemonTypeCount[pokType.Type.Name]++;
                        _pokemonTypeHeightTotal[pokType.Type.Name] += pokemon.Height;
                        _pokemonTypeWeightTotal[pokType.Type.Name] += pokemon.Weight;
                    } //end if
                }//end lock
            }//next pokType
        }

        private List<Tuple<string, double>> CalculateAveragesByType(ConcurrentDictionary<string, int> totalsByType)
        {
            List<Tuple<string, double>> averagesByType = new List<Tuple<string, double>>();

            foreach (var key in _pokemonTypes)
            {
                double average = totalsByType[key] / Convert.ToDouble(_pokemonTypeCount[key]);
                averagesByType.Add(new Tuple<string, double>(key, average));
            }//next key

            return averagesByType;
        }

        private HeightWeightAverageData GeneratePokemonAveragesDataObject(
            double pokemonHeightAvg, double pokemonWeightAvg,
            List<Tuple<string, double>> pokemonHeightAvgByType, List<Tuple<string, double>> pokemonWeightAvgByType)
        {
            var heightWeightAvgData = new HeightWeightAverageData()
            { PokemonAverageHeight = pokemonHeightAvg, PokemonAverageWeight = pokemonWeightAvg };

            for (int i = 0; i < pokemonWeightAvgByType.Count; i++)
            {
                var averagesByType = new HeightWeightAveragesByTypeData()
                {
                    PokemonType = pokemonHeightAvgByType[i].Item1,
                    PokemonAverageHeight = pokemonHeightAvgByType[i].Item2,
                    PokemonAverageWeight = pokemonWeightAvgByType[i].Item2
                };

                heightWeightAvgData.PokemonAveragesByType.Add(averagesByType);
            }//next i

            return heightWeightAvgData;
        }
    }
}