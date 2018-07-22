using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Try
{
    public class User
    {
        public string FullName { get; private set; }
        public string ProfilePicture { get; private set; }
        public string Id { get; private set; }
        public string AccessToken { get; private set; }

        public User(string fullName, string imageUri, string uid, string accessToken)
        {
            FullName = fullName;
            ProfilePicture = imageUri;
            Id = uid;
            AccessToken = accessToken;
        }

        public XDocument ExecuteCommand(string command, IEnumerable<Parametr> parametrs)
        {
            var builder = new StringBuilder(@"https://api.vk.com/method/");
            builder.Append(command);
            builder.Append(".xml?");
            builder.Append("access_token=" + AccessToken + "&");
            foreach (var parametr in parametrs)
            {
                builder.Append(parametr.Key + "=" + parametr.Value + "&");
            }

            var xDoc = XDocument.Load(builder.ToString());
            var xElement = xDoc.Root.Element("error_code");
            if (xElement != null)
            {
                MessageBox.Show(((VkErrors) (Convert.ToInt32(xElement.Value))).ToString());
            }
            return xDoc;
        }

        public List<Song> GetSongs(string ownerId, string albumId,string count = "1000")
        {
            if (ownerId == null)
                ownerId = Id;
            var parametrs = new Parametr[]
            {
                new Parametr("owner_id", ownerId),
                new Parametr("album_id", albumId),
                new Parametr("count", count)
            };
            var xDoc = ExecuteCommand("audio.get", parametrs);
            return GetSongList(xDoc);
        }

        public List<Song> GetSongs()
        {
            return GetSongs(Id, null);
        }

        public List<Song> GetSongs(string ownerId)
        {
            return GetSongs(ownerId, null);
        }

        public List<Song> GetSongList(XDocument xDoc)
        {
            var songs = new List<Song>();
            songs.AddRange(from XElement child in xDoc.Descendants("audio")
                           select new Song(child.Element("aid").Value,
                               child.Element("owner_id").Value,
                               child.Element("artist").Value,
                               child.Element("title").Value,
                               child.Element("duration").Value,
                               child.Element("url").Value,
                               child.Element("lyrics_id")?.Value,
                               child.Element("genre")?.Value));
            return songs;
        }

        public List<Song> GetSongList(XDocument xDoc, out string count)
        {
            count = xDoc.Root.Element("count").Value;
            return GetSongList(xDoc);
        }

        public string CreateAlbum(string title, string groupId)
        {
            var parametrs = new Parametr[]
            {
                new Parametr("title", title),
                new Parametr("group_id", groupId)
            };
            var xDoc = ExecuteCommand("audio.addAlbum", parametrs);
            return xDoc.Root.Element("album_id").Value;
            ;
        }

        public string CreateAlbum(string title)
        {
            return CreateAlbum(title, null);
        }

        public string MoveToAlbum(string albumId, Song[] songs, string groupId)
        {
            var sb = new StringBuilder();
            foreach (var song in songs)
            {
                sb.Append(song.Id + ",");
            }

            var parametrs = new Parametr[]
            {
                new Parametr("album_id", albumId),
                new Parametr("group_id", groupId),
                new Parametr("audio_ids", sb.ToString())
            };
            var xDoc = ExecuteCommand("audio.moveToAlbum", parametrs);
            return xDoc.Root.Value;
        }

        public string MoveToAlbum(string albumId, Song[] songs)
        {
            return MoveToAlbum(albumId, songs, null);
        }

        public List<AlbumVk> GetAlbums(string ownerId,string offset="0",string count="100")
        {
            var parametrs = new Parametr[]
            {
                new Parametr("owner_id", ownerId),
                new Parametr("offset", offset),
                new Parametr("count", count)
            };
            var xDoc = ExecuteCommand("audio.getAlbums", parametrs);
            var albums = new List<AlbumVk>();
            albums.AddRange(from XElement child in xDoc.Descendants("album")
                           select new AlbumVk(child.Element("owner_id").Value,
                               child.Element("album_id").Value,
                               child.Element("title").Value));
            return albums;
        }

    }

    public struct Parametr
    {
        public readonly string Key;
        public readonly string Value;

        public Parametr(string key,string value)
        {
            Key = key;
            Value = value;
        }

    }
}
