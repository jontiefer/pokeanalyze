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

        private int _activeThreads = 0;

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

        public async Task<HeightWeightAverageData> CalculatePokemonHeightWeightAverages(int limit, int offset, int fetchType = 1)
        {
            _timeStart = DateTime.Now;

            InitCalcData();
            var pokemonQueryList = await _repo.QueryPokemonSet(limit, offset);

            if (fetchType == 1)
                await FetchPokemonDataUsingAsyncTasks(pokemonQueryList);
            else
                FetchPokemonDataUsingThreadPool(pokemonQueryList);
            

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

        //Running the tasks by calling await on the set of tasks is slightly slower than explicitly blocking
        //the main thread by calling the GetResult function of the Awaiter object associated with the Task.
        //Even though the method is slightly slower, it will avoid potential Thread deadlock and ThreadPool
        //exhaustion.. GetAwaiter and Get Result are considered not best practices for direct use in 
        //deployment code.
        private async Task FetchPokemonDataUsingAsyncTasks(PokemonQueryList pokemonQueryList)
        {
            var tasks = pokemonQueryList.Items.Select(async p =>
            {
                var pokemon = await _repo.GetPokemonItem(p.Url);

                _pokemonHeightTotal += pokemon.Height;
                _pokemonWeightTotal += pokemon.Weight;

                AddHeightWeightByTypeData(pokemon);
            });

            await Task.WhenAll(tasks);
        }

        //Utilizing the ThreadPool to fetch the Pokemon data from the API has similar performance in comparison
        //with the Task library implementation to perform asynchronous execution of simultaneous HTTP requests
        //with the HTTPClient library.
        private void FetchPokemonDataUsingThreadPool(PokemonQueryList pokemonQueryList)
        {
            //Default Thread Pool min/max thread settings are efficient
            //int maxThreads = limit;
            //int minThreads, minCompleteThreads;
            //ThreadPool.GetMinThreads(out minThreads, out minCompleteThreads);
            //if (maxThreads < minThreads) maxThreads = minThreads;
            //ThreadPool.SetMaxThreads(maxThreads, maxThreads);

            _activeThreads = pokemonQueryList.Items.Count;

            var threadProcs = pokemonQueryList.Items.Select(p =>
                new WaitCallback(async (s) =>
                {
                    var pokemon = await _repo.GetPokemonItem(p.Url);

                    _pokemonHeightTotal += pokemon.Height;
                    _pokemonWeightTotal += pokemon.Weight;

                    AddHeightWeightByTypeData(pokemon);
                    _activeThreads--;
                }));

            foreach (var threadProc in threadProcs)
            {
                ThreadPool.QueueUserWorkItem(threadProc);
            }

            while (_activeThreads > 0) { }
        }

        private void AddHeightWeightByTypeData(Pokemon pokemon)
        {
            foreach (var pokType in pokemon.Types)
            {
                //Explicit thread synchronization not required with async/await, but seems to slightly improve performance. 
                //My theory is .Net implements its own thread synchronization utilizing Tasks that can slow 
                //performance compared to an explicit lock.
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