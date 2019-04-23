using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace yt2spotify{

  class YTRetriever{

    static void Main(string[] args){

      Console.WriteLine("YT2Spotify playlist maker");
      Console.WriteLine("========================");
      Console.Write("Please enter the URL of the your chosen YT video: ");
      string videoURL = getVideoID(Console.ReadLine());
      Console.WriteLine();
      Console.WriteLine("The video id is: " + videoURL);

      try{
        new YTRetriever().Run(videoURL).Wait();
      }
      catch (AggregateException ex){
        foreach (var e in ex.InnerExceptions){
          Console.WriteLine("Error: " + e.Message);
        }
      }

      Console.WriteLine("Press any key to continue...");
      Console.ReadKey();
    }

    // Obtain description from YT REST API
    private async Task Run(string videoID){
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
    }

    // Handling a very basic filter to retrieve the videoURL
    private static string getVideoID(string url){
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
  }
}
