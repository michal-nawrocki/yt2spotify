using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace yt2spotify{
    class SpotifyCreater{

        private static string profileId = "";
        private static string playlistId = "";

        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args){
            Console.WriteLine("####### Spotify API Example #######");
            Console.WriteLine("This example uses AuthorizationCodeAuth.");

            AuthorizationCodeAuth auth =
                new AuthorizationCodeAuth(Credentials.SP_id, Credentials.SP_key, "http://localhost:4002", "http://localhost:4002",
                    Scope.PlaylistModifyPrivate);
            auth.AuthReceived += AuthOnAuthReceived;
            auth.Start();
            auth.OpenBrowser();

            Console.ReadLine();
            auth.Stop(0);
        }

        private static async void AuthOnAuthReceived(object sender, AuthorizationCode payload){
            AuthorizationCodeAuth auth = (AuthorizationCodeAuth) sender;
            auth.Stop();

            Token token = await auth.ExchangeCode(payload.Code);
            SpotifyWebAPI api = new SpotifyWebAPI
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType,

            };
            string playlistid = await CreatePlaylist(api);
            string songid = await GetSongID(api, "Hallelujah");
            await AddSongToPlaylist(api, playlistId, songid);
        }

        private static async Task<string> CreatePlaylist(SpotifyWebAPI api){
            PrivateProfile profile = await api.GetPrivateProfileAsync();
            string name = string.IsNullOrEmpty(profile.DisplayName) ? profile.Id : profile.DisplayName;
            profileId = profile.Id;
            Console.WriteLine($"Hello there, {name}!");

            Console.WriteLine("Will create a new playlist with name \"Test xD\"");
            
            FullPlaylist playlist = api.CreatePlaylist(profileId, "Test xD", false);
            if(!playlist.HasError())
            //Store the ID for future use
            playlistId = playlist.Id; 
            Console.WriteLine("Playlist-URI: " + playlist.Uri);
            Console.WriteLine("Playlist-ID: " + playlist.Id);

            return playlist.Id;
        }

        private static async Task<String> GetSongID(SpotifyWebAPI api, string songName){
            SearchItem item = await api.SearchItemsAsync(songName, SearchType.Track, 10);

            foreach(var result in item.Tracks.Items){
                Console.WriteLine("Name:" + result.Name 
                    + ", Author:" + result.Artists[0].Name + ", ID:" + result.Id);
            }

            return item.Tracks.Items[2].Uri;
        }

        private static async Task AddSongToPlaylist(SpotifyWebAPI api, string playlistID, string songID){
            ErrorResponse response = await api.AddPlaylistTrackAsync(playlistID, songID);
            if(!response.HasError()){
                Console.WriteLine("Success");
            }else{
                Console.WriteLine(response.Error.Message);
            }
        }
    }
}