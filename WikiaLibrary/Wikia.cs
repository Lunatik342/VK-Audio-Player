using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using AudioInformationSourceLibrary;
using HtmlAgilityPack;
using WikiaLibrary.LyricsWiki;

namespace WikiaLibrary
{
    public class WikiaLyrics
    {


        public List<Album> GetLyricsFromWikiApi(string artist)
        {
            using (var httpCl = new HttpClient())
            {
                var xDoc =XDocument.Load($"http://lyrics.wikia.com/api.php?func=getArtist&artist={artist}&fmt=xml&fixXML");
                var songs = new List<Album>();
                songs.AddRange(from XElement child in xDoc.Descendants("albumResult")
                    let xElement = child.Element("year")
                    where xElement != null
                    select new Album($"{child.Element("album").Value}({xElement.Value})",
                                   new List<string>(child.Descendants("item").Select(element=>element.Value).ToList()),artist));

                return songs;
            }

        }

        
        public async Task<List<string>> GetAlbums(string bandName)
        {
            string urlPart = $"{bandName.Replace(" ", "_")}";
            using (var httpCl = new HttpClient())
            {
                var htmlCode = await httpCl.GetStringAsync($"http://lyrics.wikia.com/wiki/{urlPart}");

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlCode);

                var root = htmlDoc.DocumentNode;

                var lyrics = root
                    .Descendants("span").Where(n => n.GetAttributeValue("class", "").Equals("mw-headline"));
                var albumsNames = lyrics.Select(htmlNode => htmlNode.InnerText).ToList();

                return albumsNames;

            }

        }

        public async Task<List<string>> GetSongsFromAlbum(string bandname, string albumname)
        {
            string urlPart = $"{bandname.Replace(" ", "_")}:{albumname.Replace(" ", "_")}";
            using (var httpCl = new HttpClient())
            {
                var htmlCode = await httpCl.GetStringAsync($"http://lyrics.wikia.com/wiki/{urlPart}");
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlCode);
                var root = htmlDoc.DocumentNode;
                var lyricsNode = root
                    .Descendants("div").Single(n => n.GetAttributeValue("id", "").Equals("mw-content-text"));
                var songnames = new List<string>();
                foreach (var li in lyricsNode.Descendants("li"))
                {
                    songnames.Add(li.InnerText);
                }

                return songnames;
            }

        }

    }
}
