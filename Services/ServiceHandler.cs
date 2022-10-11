using Microsoft.Extensions.DependencyInjection;
using System;
using SpotifyAPI;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web.Auth;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using FlipTrixOSC.Services.API;
using FlipTrixOSC.Services.OSC;

namespace FlipTrixOSC.Services
{
    public class ServiceHandler
    {
        private static IConfigurationRoot _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        public ServiceHandler()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IConfigurationRoot>(_configuration);
            services.AddSingleton<Spotify>();
            services.AddSingleton<OSCSender>();
            ServiceProvider = services.BuildServiceProvider();
        }

        private static ServiceHandler? _instance;
        public IServiceProvider ServiceProvider { get; }
        private static readonly object _instanceLock = new object();

        private static ServiceHandler GetInstance()
        {
            lock (_instanceLock)
            {
                return _instance ?? (_instance = new ServiceHandler());
            }
        }

        public static ServiceHandler Instance => _instance ?? GetInstance();
    }
}
