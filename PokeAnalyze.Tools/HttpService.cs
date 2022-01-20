using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                MaxConnectionsPerServer = Int32.MaxValue,
                PooledConnectionLifetime = TimeSpan.FromSeconds(500),
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(200),
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
            int retryAttempts = 0;
            
            while (retryAttempts <= 5)
            {
                try
                {
                    var responseHttp = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
                        .ConfigureAwait(false);

                    if (responseHttp.IsSuccessStatusCode)
                    {
                        var response = await DeserializeAsync<T>(responseHttp, defaultJsonSerializerOptions)
                            .ConfigureAwait(false);

                        return new HttpResponseWrapper<T>(response, true, responseHttp);
                    }
                    else
                    {
                        return new HttpResponseWrapper<T>(default, false, default);
                    } //end if
                }
                catch (Exception)
                {
                    retryAttempts++;
                }
            }

            return new HttpResponseWrapper<T>(default, false, default);
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
