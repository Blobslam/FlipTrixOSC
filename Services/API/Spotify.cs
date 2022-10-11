using Microsoft.Extensions.Configuration;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using Microsoft.Extensions.DependencyInjection;
using FlipTrixOSC.Services.OSC;

namespace FlipTrixOSC.Services.API
{
    public class Spotify
    {
        private static EmbedIOAuthServer _server;

        public static string? SpotifyClientID;
        public static string? SpotifyClientSecret;
        public static string? SpotifyAccessToken;

        private static IServiceProvider _serviceProvider;
        private static IConfigurationRoot _config;
        public static ClientCredentialsTokenResponse _lastReceivedAccessToken;

        private static SpotifyClient _client;

        private static readonly Object lockObject = new Object();

        public Spotify(IServiceProvider provider)
        {
            _serviceProvider = provider;
            _config = provider.GetRequiredService<IConfigurationRoot>();
            SpotifyClientID = _config.GetValue<string>("Spotify:ClientID");
            SpotifyClientSecret = _config.GetValue<string>("Spotify:ClientSecret");
        }

        public async Task<SpotifyClient> GetClient()
        {
            return _client;
        }

        public Task StartWebServer()
        {
            Task t = Task.Factory.StartNew(async () => await StartWebHandler());
            t.Wait();
            Console.WriteLine("Started Spotify WebServices");
            return Task.CompletedTask;
        }

        public Task StopWebServer()
        {
            // MULTI THREAD ONLY RUN ONCE
            Task t = Task.Factory.StartNew(async () => await StopWebHandler());
            t.Wait();
            Console.WriteLine("Stopped Spotify WebServices");
            return Task.CompletedTask;
        }

        private async Task Login()
        {
            try
            {
                var authToken = await Task.Factory.StartNew(async() => await RequestAuthToken());
                _lastReceivedAccessToken = authToken.Result;
                authToken.Wait();
                string clientId = SpotifyClientID;
                var loginRequest = new LoginRequest(_server.BaseUri, clientId, LoginRequest.ResponseType.Token)
                {
                    Scope = new List<string> { Scopes.UserReadEmail, Scopes.UserReadCurrentlyPlaying, Scopes.UserReadPlaybackState }
                };
                BrowserUtil.Open(loginRequest.ToUri());
                _client = new SpotifyClient(authToken.Result.AccessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
            }
        }
        public static async Task<ClientCredentialsTokenResponse> RequestAuthToken()
        {
            var config = SpotifyClientConfig.CreateDefault();
            var request2 = new ClientCredentialsRequest(SpotifyClientID, SpotifyClientSecret);
            var response = await new OAuthClient(config).RequestToken(request2);
            SpotifyAccessToken = response.AccessToken;
            return response;
        }

        private async Task StopWebHandler()
        {
            lock (lockObject)
            {
                _server.Stop();
                _server.Dispose();
            }
        }

        private async Task StartWebHandler()
        {
            _server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
            lock (lockObject)
            {
                _server.Start();
                _server.ImplictGrantReceived += OnImplicitGrantReceived;
                _server.ErrorReceived += OnErrorReceived;
                Task.Factory.StartNew(() => Login());
            }
        }

        
        async Task OnImplicitGrantReceived(object sender, ImplictGrantResponse response)
        {
            Console.WriteLine("Authorization token received.");
            await StopWebHandler();

            var oscSender = _serviceProvider.GetService<OSCSender>();
            _client = new SpotifyClient(response.AccessToken);
            Task SpotifySender = Task.Factory.StartNew(() => oscSender.SendSpotifyStatusMessage());
            SpotifySender.Wait();
        }

        private static async Task OnErrorReceived(object sender, string error, string state)
        {
            Console.WriteLine($"Aborting authorization, error received: {error}");
        }
    }
}
