using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PokeAnalyze.Tools
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        //private SemaphoreSlim _semaphoreClient;
        //private readonly string _baseUrl;

        private JsonSerializerOptions defaultJsonSerializerOptions =>
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        public HttpService()
        {
            //var handler = new HttpClientHandler() { UseProxy = false, MaxRequestContentBufferSize = 500000, MaxConnectionsPerServer = int.MaxValue };
            var handlerSocket = new SocketsHttpHandler()
            {
                UseProxy = false,
                MaxConnectionsPerServer = int.MaxValue,
                PooledConnectionLifetime = TimeSpan.FromSeconds(500),
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(200)
            };

            _httpClient = new HttpClient(handlerSocket);
        }

        public async Task<string> GetAsync(string url)
        {
            var response = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
            return response;
        }

        public async Task<HttpResponseWrapper<T>> GetAsync<T>(string url)
        {
            var responseHTTP = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            if (responseHTTP.IsSuccessStatusCode)
            {
                var response = await DeserializeAsync<T>(responseHTTP, defaultJsonSerializerOptions)
                    .ConfigureAwait(false);
                
                return new HttpResponseWrapper<T>(response, true, responseHTTP);
            }
            else
            {
                return new HttpResponseWrapper<T>(default, false, default);
            } //end if
        }

        private async Task<T> DeserializeAsync<T>(HttpResponseMessage httpResponse, JsonSerializerOptions options)
        {
            var responseString = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(responseString, options);
        }

        //private async Task<T> DeserializeAsync<T>(HttpResponseMessage httpResponse, JsonSerializerOptions options)
        //{
        //    await using Stream stream = await httpResponse.Content.ReadAsStreamAsync();
        //    return await JsonSerializer.DeserializeAsync<T>(stream);
        //}
    }
}
