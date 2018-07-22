using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace Try
{
    /// <summary>
    /// Логика взаимодействия для SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        private readonly string  _accessToken;
        public XDocument Result { get; private set; }

        public SearchWindow(string accesstoken)
        {
            _accessToken = accesstoken;
            Loaded += SearchWindow_Loaded;
            InitializeComponent();
        }

        private void SearchWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SortingComboB.ItemsSource = Enum.GetValues(typeof(SortingOptions)).Cast<SortingOptions>();
            SortingComboB.SelectedItem = SortingOptions.Popularity;
            CorrectErrorsCheckB.IsChecked = true;
            OffsetTextB.MaxLength = 3;
            OffsetTextB.Text = "0";
            CountTextB.MaxLength = 3;
            CountTextB.Text = "30";
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            var audioSearch = new AudioSearch(SearchTextB.Text, CorrectErrorsCheckB.IsChecked.Value,
                HasLyricsCheckB.IsChecked.Value,PerformerOnlyCheckB.IsChecked.Value,
                (SortingOptions)SortingComboB.SelectionBoxItem, SearchInOwnComboB.IsChecked.Value,
                OffsetTextB.Text, CountTextB.Text, _accessToken);
            Result = audioSearch.Search();
            Close();
        }


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

}
