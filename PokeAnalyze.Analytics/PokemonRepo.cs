using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokeAnalyze.Tools;
using PokeAnalyze.Types;

namespace PokeAnalyze.Analytics
{
    public class PokemonRepo
    {
        private readonly IHttpService _httpService;
        private const string Url = "https://pokeapi.co/api/v2/pokemon";

        public PokemonRepo()
        {
            _httpService = new HttpService();
        }

        public async Task<PokemonQueryList> QueryPokemonSet(int limit, int offset)
        {
            var response = await _httpService.GetAsync<PokemonQueryList>(Url + $"?limit={limit}&offset={offset}");
            if (!response.Success)
                throw new ApplicationException(await response.GetBodyAsync());
            return response.Response;
        }

        public async Task<Pokemon> GetPokemonItem(string url)
        {
            var response = await _httpService.GetAsync<Pokemon>(url);
            if (!response.Success)
                throw new ApplicationException(await response.GetBodyAsync());
            return response.Response;
        }

        public async Task SpeedTest(int max)
        {
            for (int i = 0; i < max; i++)
            {
                var response = await _httpService.GetAsync<PokemonQueryList>(Url + "?limit=100&offset=0");
                if (!response.Success)
                    throw new ApplicationException(await response.GetBodyAsync());
            }//next i
        }
    }
}
