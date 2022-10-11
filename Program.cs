using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FlipTrixOSC.Services;
using FlipTrixOSC.Services.API;

internal class Program
{

    private static IConfigurationRoot? _configuration;
    public static string? SpotifyClientID;
    public static string? SpotifyClientSecret;
    public static string? SpotifyAccessToken;

    public Program()
    {
    }

    private static async Task Main()
    {
        try
        {
            // Request Spotify Token

            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine(@"  ______ _      _____ _____ _______ _____  _______   __   ____   _____  _____ ");
            Console.WriteLine(@" |  ____| |    |_   _|  __ \__   __|  __ \|_   _\ \ / /  / __ \ / ____|/ ____|");
            Console.WriteLine(@" | |__  | |      | | | |__) | | |  | |__) | | |  \ V /  | |  | | (___ | |     ");
            Console.WriteLine(@" |  __| | |      | | |  ___/  | |  |  _  /  | |   > <   | |  | |\___ \| |     ");
            Console.WriteLine(@" | |    | |____ _| |_| |      | |  | | \ \ _| |_ / . \  | |__| |____) | |____ ");
            Console.WriteLine(@" |_|    |______|_____|_|      |_|  |_|  \_\_____/_/ \_\  \____/|_____/ \_____|");
            Console.WriteLine(" ");
            Console.WriteLine("-------------------------------------------------------------------------------");

            // INITIALIZE SERVICES
            ServiceHandler serviceHandler = new ServiceHandler();
            var spotify = serviceHandler.ServiceProvider.GetService<Spotify>();
            Task enableSpotifyWebService = Task.Factory.StartNew(() => spotify?.StartWebServer());
            enableSpotifyWebService.Wait();

        }
        catch (NullReferenceException nex)
        {
            Console.WriteLine(nex.Message + " " + nex.StackTrace);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Console.Read();
        }
    }
}