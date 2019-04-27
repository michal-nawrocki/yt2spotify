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
        private static string playlistName = "";
        private static List<string> tracks;
        
        public static void Run(List<string> trackNames){

            tracks = trackNames;

            AuthorizationCodeAuth auth = new AuthorizationCodeAuth(Credentials.SP_id, 
                Credentials.SP_key, "http://localhost:4002", "http://localhost:4002", Scope.PlaylistModifyPrivate);
            auth.AuthReceived += AuthOnAuthReceived;
            auth.Start();
            auth.OpenBrowser();
            
            Console.ReadLine();
            auth.Stop(0);
        }


        public static void SetPlaylistName(string name){
            playlistName = name;
        }

        private static async void AuthOnAuthReceived(object sender, AuthorizationCode payload){
            AuthorizationCodeAuth auth = (AuthorizationCodeAuth) sender;
            auth.Stop();

            Token token = await auth.ExchangeCode(payload.Code);
            SpotifyWebAPI api = new SpotifyWebAPI{
                AccessToken = token.AccessToken,
                TokenType = token.TokenType,

            };

            string playlistid = await CreatePlaylist(api, playlistName);
            List<string> songIDs = await GetSongIDs(api, tracks);
            await AddSongsToPlaylist(api, playlistId, songIDs);
        }

        private static async Task<string> CreatePlaylist(SpotifyWebAPI api, string playlistName){
            PrivateProfile profile = await api.GetPrivateProfileAsync();
            string name = string.IsNullOrEmpty(profile.DisplayName) ? profile.Id : profile.DisplayName;
            profileId = profile.Id;
            Console.WriteLine($"Hello there, {name}!");

            Console.WriteLine("Will create a new playlist with name: " + playlistName);
            
            FullPlaylist playlist = api.CreatePlaylist(profileId, playlistName, false);
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

                return result.Uri;
            }

            return "";
        }

        private static async Task<List<string>> GetSongIDs(SpotifyWebAPI api, List<string> trackNames){
            List<string> trackIDs = new List<string>();

            foreach(string track in trackNames){
                string temp = await GetSongID(api, track);
                if(temp.Length != 0) trackIDs.Add(temp);
            }

            return trackIDs;
        }

        private static async Task AddSongToPlaylist(SpotifyWebAPI api, string playlistID, string songID){
            ErrorResponse response = await api.AddPlaylistTrackAsync(playlistID, songID);
            if(!response.HasError()){
                Console.WriteLine("Success");
            }else{
                Console.WriteLine(response.Error.Message);
            }
        }

        private static async Task AddSongsToPlaylist(SpotifyWebAPI api, string playlistID, List<string> songIDs){
            ErrorResponse response = await api.AddPlaylistTracksAsync(playlistID, songIDs);

            if(!response.HasError()){
                Console.WriteLine("Success");
            }else{
                Console.WriteLine(response.Error.Message);
            }
        }
    }
}