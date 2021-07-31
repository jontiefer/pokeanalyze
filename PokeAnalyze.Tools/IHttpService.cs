using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeAnalyze.Tools
{
    public interface IHttpService
    {
        Task<string> GetAsync(string url);
        Task<HttpResponseWrapper<T>> GetAsync<T>(string url);
    }
}
