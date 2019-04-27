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

    private static string videoTitle = "";

    public static string GetVideoTitle(){
      return videoTitle;
    }

    // Obtain List of track name's of the given YT video
    public static List<string> GetTrackList(string url){
      string description = "";
      string videoURL = GetVideoID(url);

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

      return GetTrackNames(description);
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

      videoTitle = videoDescriptionResponse.Items[0].Snippet.Title;

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

      foreach(string line in lines){
        var validate = Regex.Match(line, regex, RegexOptions.IgnoreCase);

        if(validate.Success){
          //Just get the track name, which is after the time stamp
          string[] splits = Regex.Split(line, @"(:\d\d) ");

          trackNames.Add(splits[splits.Length-1]);
        }
      }

      Console.WriteLine("Number of track names: " + trackNames.Count);
      return trackNames;
    }
  
  }
}
