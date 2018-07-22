using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using MessageBox = System.Windows.Forms.MessageBox;

namespace Try
{
    /// <summary>
    /// Логика взаимодействия для EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private readonly string _accessToken;
        private Song _song;
        public EditWindow(Song song, string accessToken)
        {
            InitializeComponent();
            _accessToken = accessToken;
            _song = song;
            GenresComboB.ItemsSource = Enum.GetValues(typeof(SongGenres)).Cast<SongGenres>();
            VkRadioButton.IsChecked = true;
            var b = new Binding
            {
                Source = song,
                Mode = BindingMode.OneWay
            };
            AudioEditGrid.SetBinding(DataContextProperty, b);


        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            _song.Edit(_accessToken, ArtistTextBox.Text, TitleTextBox.Text, LyricsTextBox.Text,
                (SongGenres) GenresComboB.SelectedItem, IsSearchableCheckBox.IsChecked);
            Close();
        }

        private void VkToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            OkButton.IsEnabled = false;
            LyricsTextBox.Text = _song.GetLyricsFromVk(_accessToken);
            OkButton.IsEnabled = true;
        }

        private async void  SuggestedToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            OkButton.IsEnabled = false;
            LyricsTextBox.Text = "Loading";
            LyricsTextBox.Text = await _song.GetLyricsFromWikia();
            OkButton.IsEnabled = true;
        }
    }
}
