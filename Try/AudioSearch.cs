using System;
using System.Windows;
using System.Xml.Linq;

namespace Try
{
    internal class AudioSearch
    {
        private readonly string _searchText;
        private readonly bool _correctErrors;
        private readonly bool _hasLyrics;
        private readonly bool _searchInOwn;
        private readonly SortingOptions _sortingOption;
        private readonly bool _searchOption;
        private readonly string _offset;
        private readonly string _count;
        private readonly string _accessToken;

        public AudioSearch(string searchText, bool correctErrors, bool hasLyrics, bool searchOption,
            SortingOptions sortingOption, bool searchInOwn, string offset, string count,string accessToken)
        {
            _searchText = searchText;
            _correctErrors = correctErrors;
            _hasLyrics = hasLyrics;
            _searchOption = searchOption;
            _sortingOption = sortingOption;
            _searchInOwn = searchInOwn;
            _offset = offset;
            _count = count;
            _accessToken = accessToken;
        }

        public XDocument Search()
        {
            return XDocument.Load(
                $@"https://api.vk.com/method/audio.search.xml?q={_searchText}&auto_complete={
                    Convert.ToInt32(_correctErrors)}&lyrics={Convert.ToInt32(_hasLyrics)}&performer_only={Convert.ToInt32(_searchOption)
                    }&sort={Convert.ToInt32(_sortingOption)}&search_own={Convert.ToInt32(_searchInOwn)}&offset={_offset
                    }&count={_count}&access_token={_accessToken}");
        }

    }
}