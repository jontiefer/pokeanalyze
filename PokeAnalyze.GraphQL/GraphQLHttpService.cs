using System;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

namespace PokeAnalyze.GraphQL
{
    public class GraphQLHttpService
    {
        private readonly string _url;
        private readonly GraphQLHttpClient _graphQLClient;

        public GraphQLHttpService(string url)
        {
            _url = url;
            _graphQLClient = new GraphQLHttpClient(url, new NewtonsoftJsonSerializer());
        }

        public async Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
        {
            var graphQLResponse = await _graphQLClient.SendQueryAsync<TResponse>(request, cancellationToken);

            return graphQLResponse;
        }
    }
}
