using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Xml.Linq;

namespace Try
{
    public partial class MainWindow : Window
    {
        readonly Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        private readonly ObservableCollection<User> _accountsList; 

        public MainWindow()
        {
            InitializeComponent();
            var doc= GetUserInformation(_config.AppSettings.Settings.AllKeys);
            _accountsList= new ObservableCollection<User>( GetUserList(doc));
            UsersListBox.ItemsSource = _accountsList;
            

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var lg=new LoginWindow();
            lg.ShowDialog();
            if (lg.Id != null && lg.AccessToken!=null)
            {
                var doc=GetUserInformation(lg.Id);
                User user= GetUserList(doc, lg.AccessToken);

                // ReSharper disable once PossibleInvalidOperationException
                if (lg.checkBox.IsChecked.Value)
                {
                    SaveInConfig(user);
                    stateLabel.Content = "User saved";
                }
                else
                {
                    stateLabel.Content = null;
                }

                _accountsList.Add(user);
                UsersListBox.ItemsSource = _accountsList;

            }
            foreach (var section in _config.AppSettings.Settings.AllKeys)
            {
                MessageBox.Show(_config.AppSettings.Settings[section].Value);
            }


        }

        public void SaveInConfig(User user)
        {
            _config.AppSettings.Settings.Add(user.Id, user.AccessToken);
            _config.Save(ConfigurationSaveMode.Modified);

        }

        public XDocument GetUserInformation(string[] usersId)
        {
            if (usersId.Length == 0)
                stateLabel.Content = "No users found";
            var users=new StringBuilder();
            foreach (var id in usersId)
            {
                users.AppendFormat("{0},", id);
            } 
            return XDocument.Load(
                $"https://api.vk.com/method/users.get.xml?user_ids={users}&fields=photo_50");

        }

        public XDocument GetUserInformation(string userId)
        {
            return XDocument.Load(
                $"https://api.vk.com/method/users.get.xml?user_ids={userId}&fields=photo_50");

        }

        public List<User> GetUserList(XDocument xDoc)
        {
            var users = new List<User>();

            users.AddRange(from XElement child in xDoc.Descendants("user")
                           select new User(child.Element("first_name").Value+" "+
                               child.Element("last_name").Value,
                               child.Element("photo_50").Value,
                               child.Element("uid").Value,_config.AppSettings.Settings[child.Element("uid").Value].Value));

            return users;
        }

        public User GetUserList(XDocument xDoc,string accessToken)
        {
            var users = new List<User>();

            users.AddRange(from XElement child in xDoc.Descendants("user")
                           select new User(child.Element("first_name").Value+" "+
                               child.Element("last_name").Value,
                               child.Element("photo_50").Value,
                               child.Element("uid").Value,accessToken));

            return users[0];
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(@"https://vk.com/id" + e.Uri.ToString());
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListBox.SelectedItem == null)
            {
                stateLabel.Content = "Choose user to proceed";
                return;
            }
            var u= UsersListBox.SelectedItem as User;
            base.Hide();
            var songsDispalayingWindow=new SongsDispalayingWindow(u);
            songsDispalayingWindow.ShowDialog();
            base.Close();
        }
    }
}
