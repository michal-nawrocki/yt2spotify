using System;
using System.Collections.Generic;

namespace yt2spotify{
    class Converter{

        public static void Main(string[] args){
            Console.WriteLine("========================");
            Console.Write("Please enter the URL of the your chosen YT video: ");
            string URL = Console.ReadLine();

            List<string> trackNames = YTRetriever.GetTrackList(URL);
                
        }
    }
}