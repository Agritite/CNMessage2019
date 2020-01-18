using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    ///

    public static class CNM
    {
        static CNM()
        {
            IsLogin = false;
            MyUsername = "";
            serverAddress = "127.0.0.1";
            sendport = 35645;
            sock = null;
            SendSock = null;
        }

        public static bool IsLogin;
        public static string MyUsername;

        static readonly string serverAddress;
        static readonly int sendport;

        public static SslStream SendSock;
        static TcpClient sock;

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static void SendSockConnect()
        {
            sock = new TcpClient(serverAddress, sendport);
            SendSock = new SslStream(sock.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            try
            {
                SendSock.AuthenticateAsClient("cnmessage");
            }
            catch(AuthenticationException)
            {
                Reset();
                MessageBox.Show("Server error!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }

        public static void Reset()
        {
            SendSock?.Close();
            SendSock = null;
            sock?.Close();
            sock = null;
            IsLogin = false;
            MyUsername = "";
        }

        public static void ReceiveAll(byte[] msg)
        {
            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }
            
            int size = msg.Length;
            for (int bytesLeft = size, receivedBytes = 0; bytesLeft > 0; bytesLeft -= receivedBytes)
                receivedBytes = SendSock.Read(msg, size - bytesLeft, bytesLeft);
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            contentControl.Content = new LoginPage();
        }

        public void OnLogin()
        {
            contentControl.Content = new ChatPage();
            Title = "CNMessage";
        }

        public void OnNewAcc()
        {
            contentControl.Content = new RegisterPage();
            Title = "Register";
        }

        public void OnRegister()
        {
            contentControl.Content = new LoginPage();
            Title = "Login";
        }

        public void OnLogout()
        {
            if (CNM.IsLogin)
                CNM.SendSock.Write(new byte[1] { 5 });
            CNM.Reset();
            contentControl.Content = new LoginPage();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OnLogout();
        }
    }
}
