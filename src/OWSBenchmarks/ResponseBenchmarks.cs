using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using OWSPublicAPI;
using OWSPublicAPI.Requests.Users;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OWSBenchmarks
{
    [InProcess]
    [MemoryDiagnoser]
    public class ResponseBenchmarks
    {
        private HttpClient client;
        private StringContent postContent;
        //GetUserSession userSession;
        //StringContent userSessionStringContent;

        private GetAllCharactersRequest _getAllCharactersRequest = new GetAllCharactersRequest() 
        { UserSessionGUID = Guid.Parse("13fcf94e-f002-4355-aee0-ad5950d6d1b0") };

        private GetServerToConnectToRequest _getServerToConnectToRequest = new GetServerToConnectToRequest()
        {
            CharacterName = "name",
            PlayerGroupType = 0,
            UserSessionGUID = Guid.Parse("13fcf94e-f002-4355-aee0-ad5950d6d1b0"),
            ZoneName = "HubWorld"
        };

        [GlobalSetup]
        public void GlobalSetup()
        {
            var factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(configuration =>
                {
                    configuration.ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.ClearProviders();
                    });
                });

            client = factory.CreateClient();

            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            client.DefaultRequestHeaders.Add("X-CustomerGUID", "E2FED99F-2F3A-4BFB-AB00-A586B92B5549");
            //client.DefaultRequestHeaders.Add("X-CustomerGUID", "");
            //client.DefaultRequestHeaders.Add("User-Agent", "Fiddler");

            postContent = new StringContent("");
        }

        [Benchmark]
        public async Task GetServerToConnectTo()
        {
            var response = await client.PostAsync("https://localhost:44303/api/Users/GetServerToConnectTo", JsonContent.Create(_getServerToConnectToRequest));
            response.EnsureSuccessStatusCode();
            await response.Content.ReadAsStringAsync();
        }

        public async Task GetAllCharacters()
        {

            var response = await client.PostAsync("https://localhost:44303/api/Users/GetAllCharacters", JsonContent.Create(_getAllCharactersRequest));
            response.EnsureSuccessStatusCode();
            await response.Content.ReadAsStringAsync();
        }

        public async Task GetUserSessionTime()
        {
            var response = await client.GetAsync("https://localhost:44303/api/Users/GetUserSession?UserSessionGUID=13fcf94e-f002-4355-aee0-ad5950d6d1b0");
            response.EnsureSuccessStatusCode();
            await response.Content.ReadAsStringAsync();
            //return client.PostAsync("http://localhost:52611/RPGUser/GetUserSession/147DBA25-5689-42A4-A52D-8621F17BB99D?CustomerGUID=EEE65F97-BAB1-482E-8439-9A14AE7366B5", postContent);
        }
    }
}
