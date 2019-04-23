using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace yt2spotify{

    class SpotifyCreater{

        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args){


            Console.WriteLine("####### Spotify API Example #######");
            Console.WriteLine("This example uses AuthorizationCodeAuth.");

            AuthorizationCodeAuth auth =
                new AuthorizationCodeAuth(Credentials.SP_id, Credentials.SP_key, "http://localhost:4002", "http://localhost:4002",
                    Scope.PlaylistReadPrivate | Scope.PlaylistReadCollaborative);
            auth.AuthReceived += AuthOnAuthReceived;
            auth.Start();
            auth.OpenBrowser();

            Console.ReadLine();
            auth.Stop(0);
        }

        private static async void AuthOnAuthReceived(object sender, AuthorizationCode payload)
        {
            AuthorizationCodeAuth auth = (AuthorizationCodeAuth) sender;
            auth.Stop();

            Token token = await auth.ExchangeCode(payload.Code);
            SpotifyWebAPI api = new SpotifyWebAPI
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType
            };
            PrintUsefulData(api);
        }

        private static async void PrintUsefulData(SpotifyWebAPI api)
        {
            PrivateProfile profile = await api.GetPrivateProfileAsync();
            string name = string.IsNullOrEmpty(profile.DisplayName) ? profile.Id : profile.DisplayName;
            string profileId = profile.Id;
            Console.WriteLine($"Hello there, {name}!");

            Console.WriteLine("Will create a new playlist with name \"Test xD\"");
            
            FullPlaylist playlist = api.CreatePlaylist(profileId, "Test xD", false);
            if(!playlist.HasError())
            Console.WriteLine("Playlist-URI: " + playlist.Uri);
        }
        
    }
}