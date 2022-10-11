using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpOSC;
using SpotifyAPI.Web;
using FlipTrixOSC.Services.API;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlipTrixOSC.Services.OSC
{
    public class OSCSender
    {
        private static readonly Object _lockObject = new Object();
        private UDPSender _oscsender = new UDPSender("127.0.0.1", 9000); // default
        private SpotifyClient? _client = null;
        private IServiceProvider _provider;
        private IConfigurationRoot _configuration;

        public OSCSender(IServiceProvider provider)
        {
            try
            {
                _provider = provider;
                _configuration = _provider.GetService<IConfigurationRoot>();
                string host = _configuration.GetValue<string>("OSC:Host");
                int? port = Int32.Parse(_configuration.GetValue<string>("OSC:Port"));
                if (!string.IsNullOrEmpty(host) && !(port is null))
                    _oscsender = new UDPSender(host, (Int32)port);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public async Task SendSpotifyStatusMessage()
        {
            try
            {
                _client = await _provider.GetService<Spotify>().GetClient();
                for (; ; )
                {
                    if (!Spotify._lastReceivedAccessToken.IsExpired)
                    {
                        // SEND OSC HERE
                        string currentTime = DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                        var inputMessage = new OscMessage("/chatbox/input", $"--------- time: {currentTime} --------- playback paused", true);
                        var currentSong = await _client.Player.GetCurrentPlayback();
                        if (currentSong != null)
                        {
                            if (currentSong.IsPlaying)
                            {
                                var currentlyPlaying = await _client.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));
                                dynamic trackId = currentlyPlaying.Item;
                                string currentArtists = "";
                                FullTrack track = await _client.Tracks.Get(trackId.Id);
                                string artist = track.Artists.First().Name.ToLower();

                                if (track.Artists.Count > 1)
                                {
                                    var artists = track.Artists.AsQueryable().ToList();
                                    currentArtists = string.Join(", ", track.Artists.Select(l => "(" + string.Join(", ", l.Name) + ")"));
                                }
                                else
                                {
                                    currentArtists = track.Artists.First().Name;
                                }
                                inputMessage = new OscMessage("/chatbox/input", $"time: {currentTime}\vplaying: {track.Name} - {currentArtists}\vvolume: {currentSong.Device.VolumePercent}%", true);
                            }
                        }
                        _oscsender.Send(inputMessage);
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        await Spotify.RequestAuthToken();
                    }
                }
            }
            catch (Exception ex)
            {
                // in case api not available just try again
                if (ex.Message.Contains("service unavailable"))
                    await SendSpotifyStatusMessage();
                else
                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
