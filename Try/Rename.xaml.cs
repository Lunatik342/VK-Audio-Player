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

namespace Try
{
    /// <summary>
    /// Логика взаимодействия для Rename.xaml
    /// </summary>
    public partial class Rename : Window
    {
        public new string Title { get; private set; }
        public bool IsConfurmed { get; private set; }

        public Rename(string title)
        {
            InitializeComponent();
            Title = title;
            NameTextBox.Text = title;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ConfurmButton_OnClick(object sender, RoutedEventArgs e)
        {
            IsConfurmed = true;
            Title = NameTextBox.Text;
            Close();
        }
    }
}
