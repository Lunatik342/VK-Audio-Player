using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using WikiaLibrary;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace Try
{
    /// <summary>
    /// Логика взаимодействия для SongsDispalayingWindow.xaml
    /// </summary>
    public partial class SongsDispalayingWindow
    {
        private User User { get; }
        private CancellationTokenSource _tokenSource;
        private readonly DispatcherTimer _audioTimeUpdateTimer;
        private ObservableCollection<Song> _selectedSongs;
        private readonly ObservableCollection<SongTab> _tabsCollection = new ObservableCollection<SongTab>();
        private int _index;
        private bool _isPlaying;
        private ObservableCollection<AlbumVk> _albumsVk;

        public SongsDispalayingWindow(User user)
        {
            InitializeComponent();
            User = user;
            MyTabControl.ItemsSource = _tabsCollection;
            DislpayMyAudioTab();
            NechtoUZasnoe();
            CancelButton.IsEnabled = false;
            _audioTimeUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.0) };
            _audioTimeUpdateTimer.Tick += AudioTimeUpdateTimerTick;
            SongProgressSlider.AddHandler(Slider.PreviewMouseDownEvent, new MouseButtonEventHandler(SongProgressSlider_OnPreviewMouseDown), true);
            SongProgressBar.Maximum = 0;
            SongProgressSlider.Maximum = 0;
            TabsComboBox.ItemsSource = MyTabControl.ItemsSource;
            _albumsVk = new ObservableCollection<AlbumVk>(User.GetAlbums(User.Id));
            AlbumsVkComboBox.ItemsSource = _albumsVk;
        }

        public void NechtoUZasnoe()
        {
            HLink.NavigateUri = new Uri("https://vk.com/id" + User.Id);
            ProfileImage.Source = new BitmapImage(new Uri(User.ProfilePicture));
            NameTextBlock.Text = User.FullName;
        }

        public void DislpayMyAudioTab()
        {
            var songsCollection = new ObservableCollection<Song>(User.GetSongs());
            var sTab = new SongTab(songsCollection, "Songs");
            _tabsCollection.Add(sTab);
            MyTabControl.SelectedItem = sTab;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void DownloadButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var songs = MyTabControl.GetChildOfType<ListView>().SelectedItems.Cast<Song>();
            var songsList = songs as Song[] ?? songs.ToArray();
            if (!songsList.Any()) return;
            _tokenSource = new CancellationTokenSource();
            CancelButton.IsEnabled = true;
            if (songsList.Length == 1)
            {
                songsList.First().DownloadAudio(_tokenSource.Token, dialog.SelectedPath, this);
                return;
            }
            CancelButton.IsEnabled = true;
            Song.DownloadAudio(songsList, _tokenSource.Token, dialog.SelectedPath, this);
        }

        private void CancellButton_Click(object sender, RoutedEventArgs e)
        {
            _tokenSource.Cancel();
        }

        private void Lyrics_OnExpanded(object sender, RoutedEventArgs e)
        {
            var song = ((FrameworkElement)sender).DataContext as Song;
            ((TextBox)((Expander)e.Source).Content).Text = song.GetLyricsFromVk(User.AccessToken) ?? "No lyrics found";

        }

        private async void SugestedLyrics_OnExpanded(object sender, RoutedEventArgs e)
        {
            var lyrTextBox = ((TextBox)((Expander)e.Source).Content);
            var song = ((FrameworkElement)sender).DataContext as Song;
            lyrTextBox.Text = "Loading";
            lyrTextBox.Text = await song.GetLyricsFromWikia() ?? "No lyrics found";

        }

        private void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            var cs = new SearchWindow(User.AccessToken);
            cs.ShowDialog();
            if (cs.Result == null) return;
            var sTab = new SongTab(new ObservableCollection<Song>(User.GetSongList(cs.Result)), cs.SearchTextB.Text);
            _tabsCollection.Add(sTab);
            MyTabControl.SelectedItem = sTab;
        }

        void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            var tab = (SongTab)(((Button)sender).DataContext);
            var index = MyTabControl.Items.IndexOf(tab);
            int i = MyTabControl.Items.IndexOf(tab);
            ((TabItem)MyTabControl.ItemContainerGenerator.ContainerFromItem(tab)).ToString();
            if ((index == MyTabControl.Items.Count - 2) && index != 0)
                MyTabControl.SelectedItem = MyTabControl.Items[index - 1];
            _tabsCollection.Remove(tab);
        }

        private void ChangeButton_OnClick(object sender, RoutedEventArgs e)
        {
            int count = MyTabControl.GetChildOfType<ListView>().SelectedItems.Count;
            if (count > 1)
            {
                StateLabel.Text = "Choose only one song";
                return;
            }
            if (count == 0)
            {
                StateLabel.Text = "Choose song";
                return;
            }
            var eW = new EditWindow((Song)MyTabControl.GetChildOfType<ListView>().SelectedItem, User.AccessToken);
            eW.ShowDialog();
        }

        private void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!MyTabControl.GetChildOfType<ListView>().SelectedItems.Cast<Song>().ToList().Any())
            {
                _selectedSongs =
                    new ObservableCollection<Song>(
                        MyTabControl.GetChildOfType<ListView>().ItemsSource.Cast<Song>().ToList());
            }
            else
            {
                _selectedSongs =
                    new ObservableCollection<Song>(
                        MyTabControl.GetChildOfType<ListView>().SelectedItems.Cast<Song>().ToList());
            }
            _index = -1;
            _audioTimeUpdateTimer.Start();
            _isPlaying = true;
            CurrentSongsListBox.ItemsSource = _selectedSongs;
            PlayNextSong();
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (SongPlayingMediaElement.Source == null)
                return;
            StopPlay();
        }

        private void PauseButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (SongPlayingMediaElement.Source == null)
                return;
            if (_isPlaying)
                PausePlay();
            else
                RessumePlay();
        }

        private void SongProgressSlider_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SongPlayingMediaElement.Position = TimeSpan.FromSeconds(SongProgressSlider.Value);
            UpdateProgressBar();
        }


        private void AudioTimeUpdateTimerTick(object sender, EventArgs e)
        {
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            SongProgressBar.Value = SongPlayingMediaElement.Position.TotalSeconds;
            CurrentPosition.Text = SongPlayingMediaElement.Position.ToString(@"hh\:mm\:ss");
        }

        private void SongPlayingMediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            PlayNextSong();
        }

        private void PlayNextSong()
        {
            ++_index;
            if (_index >= _selectedSongs.Count)
            {
                _index = 0;
                SetNextSong();
                return;
            }
            SetNextSong();
            SongPlayingMediaElement.Play();
        }

        private void PlayPreviousSong()
        {
            --_index;
            if (_index < 0)
            {
                _index = _selectedSongs.Count - 1;
                SetNextSong();
                return;
            }
            SetNextSong();
            SongPlayingMediaElement.Play();
        }

        private void SetNextSong()
        {
            PauseButton.Content = "Pause";
            SongProgressSlider.Maximum = _selectedSongs[_index].Duration.TotalSeconds;
            SongProgressBar.Maximum = _selectedSongs[_index].Duration.TotalSeconds;
            SongPlayingMediaElement.Source = new Uri(_selectedSongs[_index].Url);
            SongDuration.Text = @"/" + _selectedSongs[_index].Duration;
            CurrentPosition.Text = SongPlayingMediaElement.Position.ToString(@"hh\:mm\:ss");
            SongNameTextBox.Text = _selectedSongs[_index].ToString();
        }

        private void StopPlay()
        {
            SongPlayingMediaElement.Stop();
            _audioTimeUpdateTimer.Stop();
            UpdateProgressBar();
            _isPlaying = false;
        }

        private void RessumePlay()
        {
            SongPlayingMediaElement.Play();
            _audioTimeUpdateTimer.Start();
            _isPlaying = true;
            PauseButton.Content = "Pause";
        }

        private void PausePlay()
        {
            SongPlayingMediaElement.Pause();
            _audioTimeUpdateTimer.Stop();
            _isPlaying = false;
            PauseButton.Content = "Ressume";
        }

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            PlayPreviousSong();
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            PlayNextSong();
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedSongs = MyTabControl.GetChildOfType<ListView>();
            if (selectedSongs.SelectedItems.Count < 0)
                return;
            var items = selectedSongs.SelectedItems;
            for (var i = 0; i < items.Count; i++)
            {
                ((Collection<Song>)selectedSongs.ItemsSource).Remove((Song)items[i]);
            }
        }

        private void AddNewTabButton_OnClick(object sender, RoutedEventArgs e)
        {
            var sTAb = AddNewTab();
            if (sTAb != null)
                MyTabControl.SelectedItem = sTAb;
        }

        public SongTab AddNewTab()
        {
            var renameWindow = new Rename("New Tab");
            renameWindow.ShowDialog();
            if (!renameWindow.IsConfurmed)
                return null;
            SongTab sTab = new SongTab(new ObservableCollection<Song>(), renameWindow.Title);
            _tabsCollection.Add(sTab);
            return sTab;
        }

        private void SongsListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lV = ((ListView)sender);
            if (lV.SelectedItem != null)
                ((ListViewItem)(lV).ItemContainerGenerator.ContainerFromItem((lV.SelectedItem))).Background = Brushes.Gold;
        }

        private void TitleTextBlock_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2) return;
            var textBlock = ((TextBlock)sender);
            var renameWindow = new Rename(textBlock.Text);
            renameWindow.ShowDialog();
            textBlock.Text = renameWindow.Title;
        }

        private void CopyButton_OnClick(object sender, RoutedEventArgs e)
        {
            SongTab sTab;
            ObservableCollection<Song> targetCollection;
            if (TabsComboBox.SelectedItem == null)
            {
                sTab = AddNewTab();
                if (sTab == null)
                    return;
                targetCollection = sTab.SongCollection;

            }
            else
            {
                sTab = ((SongTab)TabsComboBox.SelectedItem);
                targetCollection = sTab.SongCollection;
            }
            var items = MyTabControl.GetChildOfType<ListView>().SelectedItems;
            foreach (var item in items)
            {
                targetCollection.Add((Song)((Song)item).Clone());
            }
            MyTabControl.SelectedItem = sTab;

        }

        public void SavePlaylist(object obj, Stream stream)
        {
            try
            {
                var formater = new BinaryFormatter();
                formater.Serialize(stream, obj);
            }
            catch (SerializationException ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var sTab = (SongTab)MyTabControl.SelectedItem;
            string path = @"Playlists\" + sTab.Title + ".pllst";
            try
            {
                using (var fs = new FileStream(path, FileMode.CreateNew))
                {
                    SavePlaylist(sTab, fs);
                }
            }
            catch (IOException)
            {
                var result = MessageBox.Show("The file already exists. Do you want to overwrite it?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    using (var fs = new FileStream(path, FileMode.Create))
                    {
                        SavePlaylist(sTab, fs);
                    }

                    ShowMessage("Playlist " + Path.GetFullPath(path) + " saved");
                }
                else
                {
                    MessageBox.Show("You can rename playlist by doubleclicking on tab's header");
                }

            }
        }

        private void SaveAsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var sTab = MyTabControl.SelectedItem as SongTab;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Playlist file|*.pllst";
            dialog.Title = @"Choose path";
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;
            using (var fs = new FileStream(dialog.FileName, FileMode.Create))
            {
                SavePlaylist(sTab, fs);
            }

            ShowMessage("Playlist " + dialog.FileName + " saved");
        }

        //save in vk as playlist ConfurmBandNameButton click
        private void SaveInVkButton_OnClick(object sender, RoutedEventArgs e)
        {
            var songaTab = (SongTab)MyTabControl.SelectedItem;
            var albumId = User.CreateAlbum(songaTab.Title);
            var songs = songaTab.SongCollection.ToArray();
            User.MoveToAlbum(albumId, songs);
        }

        private void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            var of = new OpenFileDialog { Filter = "Playlist file|*.pllst" };
            var result = of.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                using (var fs = new FileStream(of.FileName, FileMode.Open))
                {
                    var formater = new BinaryFormatter();
                    var sTab = (SongTab)(formater.Deserialize(fs));
                    _tabsCollection.Add(sTab);
                }
            }
        }

        public void ShowMessage(string message)
        {
            StateLabel.Text = message;
        }

        private void MyAudioButton_OnClick(object sender, RoutedEventArgs e)
        {
            DislpayMyAudioTab();
        }

        private void GetPlaylistButton_OnClick(object sender, RoutedEventArgs e)
        {
            var album = (AlbumVk)AlbumsVkComboBox.SelectedItem;
            if (album == null)
                return;
            var sTab=new SongTab(new ObservableCollection<Song>(User.GetSongs(album.OwnerId,album.AlbumId)),album.Title);
            _tabsCollection.Add(sTab);
            MyTabControl.SelectedItem = sTab;

        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            AlbumSelectionWindow albumSelection = new AlbumSelectionWindow();
            albumSelection.ShowDialog();
            if (albumSelection.Result == null)
                return;
            var album = albumSelection.Result;
            var albumSongs=new List<Song>();
            foreach (var song in album.Songs)
            {
                var audioSearch=new AudioSearch(album.ArtistName + " – " + song,false,false,false,SortingOptions.Popularity,false,"0","20",User.AccessToken);
                var s= User.GetSongList(audioSearch.Search());
                if(s.Capacity==0)
                    break;
                albumSongs.Add(s[0]);
                await Task.Delay(300);
            }
            SongTab sTab=new SongTab(new ObservableCollection<Song>(albumSongs),album.AlbumName);
            _tabsCollection.Add(sTab);
            
        }
    }

}
