using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LyricsInterface;
using HtmlAgilityPack;

namespace GetLyricsFromWikia
{
    public class WikiaLyrics:ILyricsProvider
    {

        private const string ArtistErrorMessage =
            "No artist found.\nPerhaps, the artist name does not correspond to the official,or no artist in database";
        private const string SongErrorMessage =
            "No song found.\nPerhaps, the song name does not correspond to the official,or no song in database";

        public async Task<string> GetLyrics(string bandName, string songName)
        {
            string urlPart = $"{songName.Replace(" ", "-").ToLower()}-lyrics-{bandName.Replace(" ", "-").ToLower()}";
            var httpCl = new HttpClient();
            try
            {
                var htmlCode = await httpCl.GetStringAsync($"http://www.metrolyrics.com/{urlPart}.html");
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlCode);

                var root = htmlDoc.DocumentNode;
                var verses = root
                    .Descendants()
                    .Where(n => n.GetAttributeValue("class", "").Equals("verse"));
                var lyrics = new StringBuilder();
                if (!verses.Any())
                    return SongErrorMessage;
                foreach (var verse in verses)
                {
                    lyrics.Append(verse.InnerText);
                }
                return lyrics.ToString();
            }
            catch (HttpRequestException)
            {
                return ArtistErrorMessage;
            }

        }
    }
}
