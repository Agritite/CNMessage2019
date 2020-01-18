using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace CNMessage
{
    /// <summary>
    /// LoginPage.xaml 的互動邏輯
    /// </summary>
    public partial class LoginPage : UserControl
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CNM.SendSockConnect();

                CNM.SendSock.Write(new byte[1] { 0 });
                CNM.SendSock.Write(BitConverter.GetBytes(User.Text.Length));
                CNM.SendSock.Write(Encoding.ASCII.GetBytes(User.Text));
                CNM.SendSock.Write(BitConverter.GetBytes(Pwd.Password.Length));
                CNM.SendSock.Write(Encoding.ASCII.GetBytes(Pwd.Password));

                byte[] msg = new byte[1];
                CNM.ReceiveAll(msg);
                if (msg[0] == BitConverter.GetBytes(0)[0])
                {
                    CNM.Reset();
                    MessageBox.Show("Username does not exist or wrong password!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                else
                {
                    CNM.IsLogin = true;
                    CNM.MyUsername = User.Text;
                    ((MainWindow)Application.Current.MainWindow).OnLogin();
                }
            }
            catch(SocketException Ex)
            {

            }
        }

        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).OnNewAcc();
        }
    }
}
