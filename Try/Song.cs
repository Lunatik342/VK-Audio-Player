using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Try
{
    [Serializable]
    public class Song : INotifyPropertyChanged, ICloneable
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public string Id { get; }
        public string OwnerId { get; }
        public string Artist { get; private set; }
        public string Title { get; private set; }
        public TimeSpan Duration { get; private set; }
        public string Url { get; private set; }
        public string LyricsId { get; private set; }
        public SongGenres Genre { get; private set; }

        [NonSerialized]
        private string _vkLyrics;
        [NonSerialized]
        private bool _isVkLyricsLoaded;

        [NonSerialized]
        private string _suggestedLyrics;
        [NonSerialized]
        private bool _isSuggestedLyricsLoaded;

        public Song(string id, string ownerId, string artist, string title, string duration,
            string url, string lyricsId, string genreId)
        {
            Id = id;
            OwnerId = ownerId;
            Artist = artist;
            Title = title;
            Duration = TimeSpan.FromSeconds(Convert.ToUInt32(duration));
            Url = url;
            LyricsId = lyricsId;
            if (genreId != null)
                Genre = (SongGenres)Enum.Parse(typeof(SongGenres), genreId);
            else
            {
                Genre = SongGenres.Other;
            }
        }

        public override string ToString()
        {
            return Artist + " - " + Title;
        }

        public async Task<string> GetLyricsFromWikia()
        {
            if (_isSuggestedLyricsLoaded)
                return _suggestedLyrics;
            string urlPart = $"{Artist.Replace(" ", "_")}:{Title.Replace(" ", "_")}";
            using (var httpCl = new HttpClient())
            {
                try
                {
                    var htmlCode = await httpCl.GetStringAsync($"http://lyrics.wikia.com/wiki/{urlPart}");
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlCode);

                    var root = htmlDoc.DocumentNode;
                    var lyrics = root
                        .Descendants().Single(n => n.GetAttributeValue("class", "").Equals("lyricbox"));

                    if (lyrics.SelectSingleNode("//span[@style='padding:1em']") != null)
                        return "Intrumental";

                    lyrics.ChildNodes.OfType<HtmlCommentNode>().Single().Remove();
                    var d = WebUtility.HtmlDecode(lyrics.FancyInnerText().TrimEnd('\r', '\n', ' '));
                    _suggestedLyrics = d;
                    _isSuggestedLyricsLoaded = true;
                    return d;

                }
                catch (HttpRequestException)
                {
                    return null;
                }
                catch (InvalidOperationException)
                {
                    return null;
                }

            }

        }
        private async Task<string> GetLyricsFromMetrolyrics()
        {
            string urlPart = $"{Title.Replace(" ", "-").ToLower()}-lyrics-{Artist.Replace(" ", "-").ToLower()}";
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
                    return null;

                foreach (var verse in verses)
                {
                    lyrics.Append(verse.InnerText);
                }
                return lyrics.ToString();
            }
            catch (HttpRequestException)
            {
                return null;
            }

        }

        public string GetLyricsFromVk(string accessToken)
        {
            if (LyricsId == null)
            {
                return null;
            }
            if (_isVkLyricsLoaded)
                return _vkLyrics;
            var xDoc = XDocument.Load(
                    $"https://api.vk.com/method/audio.getLyrics.xml?lyrics_id={LyricsId}&access_token={accessToken}");
            _vkLyrics = xDoc.Root.Element("lyrics").Element("text").Value;
            _isVkLyricsLoaded = true;
            return _vkLyrics;
        }

        public async static void DownloadAudio(IEnumerable<Song> songs, CancellationToken cancellationToken, string path, SongsDispalayingWindow w)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    foreach (var song in songs)
                    {
                        var st = await client.GetAsync(song.Url, cancellationToken);

                        using (
                            var stream = new FileStream(path + @"\" + song.Artist + " - " + song.Title + ".mp3",
                                FileMode.OpenOrCreate,
                                FileAccess.Write))
                            await st.Content.CopyToAsync(stream);
                        w.StateLabel.Text = path + @"\" + song.Artist + " - " + song.Title + ".mp3" + " loaded";
                    }
                }

                catch (OperationCanceledException)
                {
                    MessageBox.Show(@"Downloading cancelled");
                }
                finally
                {
                    w.CancelButton.IsEnabled = false;
                }

            }

        }
        public async void DownloadAudio(CancellationToken cancellationToken, string path, SongsDispalayingWindow w)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var st = await client.GetAsync(Url, cancellationToken);
                    using (var stream = new FileStream(path + @"\" + Artist + " - " + Title + ".mp3",
                            FileMode.OpenOrCreate,
                            FileAccess.Write))
                        await st.Content.CopyToAsync(stream);
                    w.StateLabel.Text = path + @"\" + Artist + " - " + Title + ".mp3" + " loaded";
                }

                catch (OperationCanceledException)
                {
                    MessageBox.Show(@"Downloading cancelled");
                }
                finally
                {
                    w.CancelButton.IsEnabled = false;
                }

            }

        }
        public async void Edit(string accessToken, string artist, string title, string lyrics, SongGenres songGenre, bool? isVisible)
        {
            if (accessToken == null)
            {
                throw new NullReferenceException("Access token should not be null");
            }

            var xDoc = await Task.Run(() => XDocument.Load(
                $"https://api.vk.com/method/audio.edit.xml?" +
                $"owner_id={OwnerId}&" +
                $"audio_id={Id}&" +
                $"artist={artist}&" +
                $"title={title}&" +
                $"text={lyrics}&" +
                $"genre_id={(int)songGenre}&" +
                $"no_search={Convert.ToInt32(isVisible)}&" +
                $"access_token={accessToken}"));

            var xElement = xDoc.Root.Element("error_code");
            if (xElement != null)
            {
                MessageBox.Show(((VkErrors)(Convert.ToInt32(xElement.Value))).ToString());
                return;
            }
            System.Windows.MessageBox.Show(xDoc.ToString());
            Artist = artist;
            Title = title;
            _vkLyrics = lyrics;
            Genre = songGenre;
            LyricsId = xDoc.Root.Value;
            _isSuggestedLyricsLoaded = false;
            _isVkLyricsLoaded = false;
            OnPropertyChanged(null);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    [Serializable]
    public class SongTab
    {
        public ObservableCollection<Song> SongCollection { get; private set; }
        public string Title { get; set; }

        public SongTab(ObservableCollection<Song> songsCollection, string title)
        {
            SongCollection = songsCollection;
            Title = title;
        }

        public override string ToString()
        {
            return Title;
        }
    }

    public class AlbumVk
    {
        public string OwnerId { get; set; }
        public string AlbumId { get; set; }
        public string Title { get; set; }

        public AlbumVk(string ownerId,string albumId,string title)
        {
            OwnerId = ownerId;
            AlbumId = albumId;
            Title = title;
        }
    }

}