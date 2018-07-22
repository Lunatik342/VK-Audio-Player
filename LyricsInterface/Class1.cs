using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsInterface
{
    public interface ILyricsProvider
    {
        Task<string> GetLyrics(string bandName, string songName);
    }
}
