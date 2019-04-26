using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace yt2spotify{
  class YTRetriever{
    
    private static string description = "";

    static void Main(string[] args){
      RunRetriever();
    }

    static void RunRetriever(){
      Console.WriteLine("YT2Spotify playlist maker");
      Console.WriteLine("========================");
      Console.Write("Please enter the URL of the your chosen YT video: ");
      string videoURL = GetVideoID(Console.ReadLine());
      Console.WriteLine();
      Console.WriteLine("The video id is: " + videoURL);

      try{
        description = new YTRetriever().Run(videoURL).Result;
      }
      catch (AggregateException ex){
        foreach (var e in ex.InnerExceptions){
          Console.WriteLine("Error: " + e.Message);
        }
      }

      if(description.Length == 0){
        //TODO Error handling, for now just exit
        Console.WriteLine("ERROR: Didnt recieve anything from the YT API");
        Environment.Exit(1);
      }

      List<string> trackNames = GetTrackNames(description);

      Console.WriteLine("Press any key to continue...");
      Console.ReadKey();
    }

    // Obtain description from YT REST API
    private async Task<string> Run(string videoID){
      var youtubeService = new YouTubeService(new BaseClientService.Initializer(){
          ApiKey = Credentials.YT_key,
          ApplicationName = this.GetType().ToString()
      });

      var videoDescriptionRequest = youtubeService.Videos.List("snippet");
      videoDescriptionRequest.Id = videoID;

      var videoDescriptionResponse = await videoDescriptionRequest.ExecuteAsync();

      Console.WriteLine("Obtained following description:");
      foreach(var element in videoDescriptionResponse.Items){
          Console.WriteLine(element.Snippet.Description);
      }

      return videoDescriptionResponse.Items[0].Snippet.Description;
    }

    // Handling a very basic filter to retrieve the videoURL
    private static string GetVideoID(string url){
        string output = "";

        if(url.Contains("youtu.be/")){
            int begIndex = url.IndexOf(".be/") + 4;
            output = url.Substring(begIndex);
        }else if(url.Contains("watch?v=")){
            int begIndex = url.IndexOf("?v=") + 3;
            output = url.Substring(begIndex);

            if(output.Contains("&")){
                int endIndex = output.IndexOf("&");
                output = output.Substring(0,endIndex);
            }
        }else{
            //TODO
            // return errorMessage
        }
        
        return output;
    }
  
    // Retrieve individual song names from the description
    private static List<string> GetTrackNames(string description){
      List<string> trackNames = new List<string>();
      var regex = @"(:\d\d)";

      string[] lines = description.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

      Console.WriteLine("All tracks in the description:\n\n");
      foreach(string line in lines){
        var validate = Regex.Match(line, regex, RegexOptions.IgnoreCase);

        if(validate.Success){
          //Just get the track name, which is after the time stamp
          string[] splits = Regex.Split(line, @"(:\d\d) ");

          trackNames.Add(splits[splits.Length-1]);
        }
      }

      return trackNames;
    }
  
  }
}
