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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        const string AppId = "5504055";
        const int Scope = (int)(VkontakteScopeList.Audio | VkontakteScopeList.Offline);
        public string Id { get; private set; }
        public string AccessToken { get; private set; }

        public string String { get; set; }

        public LoginWindow()
        {
            InitializeComponent();
            Connect();
        }

        public void Connect()
        {
            MyWebBrowser.Source = new Uri(
                $"https://oauth.vk.com/authorize?client_id={AppId}&scope={Scope}&display=popup&response_type=token");
            MyWebBrowser.AddressChanged += UrlChangedEventHandler;
        }

        private void UrlChangedEventHandler(object sender, Awesomium.Core.UrlEventArgs e)
        {
            var uri = MyWebBrowser.Source.OriginalString;

            var firstStr = (uri.IndexOf("access_token=", StringComparison.Ordinal) + "access_token=".Length);
            var secondStr = (uri.IndexOf("&expires_in", StringComparison.Ordinal));

            if ((secondStr - firstStr) <= 0)
            {
                return;
            }
            AccessToken = uri.Substring(firstStr, secondStr - firstStr);

            Id = uri.Substring((uri.IndexOf("&user_id=", StringComparison.Ordinal) + "&user_id=".Length),
                uri.Length- (uri.IndexOf("&user_id=", StringComparison.Ordinal) + "&user_id=".Length));
            this.Close();

        }
    }
}
