using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioInformationSourceLibrary
{
    public class Album
    {
        public string AlbumName { get; private set; }
        public string ArtistName { get; private set; }
        public List<string> Songs { get; private set; }

        public Album(string albumName,List<string> songs,string artistName )
        {
            AlbumName = albumName;
            ArtistName = artistName;
            Songs = songs;
        }
    }
}
