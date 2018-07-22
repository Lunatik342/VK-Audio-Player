using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AudioInformationSourceLibrary;
using WikiaLibrary;

namespace Try
{
    /// <summary>
    /// Логика взаимодействия для AlbumSelectionWindow.xaml
    /// </summary>
    public partial class AlbumSelectionWindow : Window
    {
        public Album Result { get;private set; }

        public AlbumSelectionWindow()
        {
            InitializeComponent();
        }

        private void SelectArtistButton_OnClick(object sender, RoutedEventArgs e)
        {
            AlbumsTreeView.Items.Clear();
            var wl = new WikiaLyrics();
            if (BandNameTextBox.Text == null)
                MessageBox.Show("Enter band name into a textbox");
            var albums = wl.GetLyricsFromWikiApi(BandNameTextBox.Text);
            if (!albums.Any())
            {
                MessageBox.Show("Nothing found:check the correctness of the entered band name");
            }

            foreach (var album in albums)
            {
                TreeViewItem item = new TreeViewItem();
                item.Tag = album;
                item.Header = album.AlbumName;
                foreach (var song in album.Songs)
                {
                    item.Items.Add(new TreeViewItem() {Tag = album,Header = song});
                }
                AlbumsTreeView.Items.Add(item);
            }
        }

        private void AddAlbumTabButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (AlbumsTreeView.SelectedItem == null) return;
            Result = (Album) ((TreeViewItem) AlbumsTreeView.SelectedItem).Tag;
            Close();
        }
    }
}
