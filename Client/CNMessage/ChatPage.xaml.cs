using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    /// ChatPage.xaml 的互動邏輯
    /// </summary>
    public partial class ChatPage : UserControl
    {
        public bool IsChatNotEmpty => PreparetoSend.Text.Length != 0;

        public ChatPage()
        {
            InitializeComponent();
            //RetrieveHistory();
        }

        private void RetrieveHistory()
        {
            CNM.SendSock.Send(new byte[1] { 2 });

            byte[] msg = new byte[4];
            ChatBox.Text = "";
            CNM.SendSock.Receive(msg);
            uint recordcount = BitConverter.ToUInt32(msg, 0);

            for (uint i = 0; i < recordcount ; i++)
            {
                msg = new byte[4];
                CNM.SendSock.Receive(msg);
                uint usernamesize = BitConverter.ToUInt32(msg, 0);

                msg = new byte[usernamesize];
                CNM.SendSock.Receive(msg);
                string username = Encoding.ASCII.GetString(msg);

                msg = new byte[4];
                CNM.SendSock.Receive(msg);
                uint messagesize = BitConverter.ToUInt32(msg, 0);

                msg = new byte[messagesize];
                CNM.SendSock.Receive(msg);
                string message = Encoding.ASCII.GetString(msg);

                ChatBox.Text += (username + ": " + message + Environment.NewLine);
            }         
        }

        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            CNM.SendSock.Send(new byte[1] { 3 });
            CNM.SendSock.Send(BitConverter.GetBytes(TargetUser.Text.Length));
            CNM.SendSock.Send(Encoding.ASCII.GetBytes(TargetUser.Text));
            CNM.SendSock.Send(BitConverter.GetBytes(PreparetoSend.Text.Length));
            CNM.SendSock.Send(Encoding.ASCII.GetBytes(PreparetoSend.Text));

            byte[] msg = new byte[1];
            CNM.SendSock.Receive(msg);
            if (msg[0] == BitConverter.GetBytes(0)[0])
            {
                MessageBox.Show("Server error!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            ChatBox.Text += (CNM.MyUsername + ": " + PreparetoSend.Text + Environment.NewLine);
            PreparetoSend.Text = "";
        }

        private void OnFileClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != true)
                return;

            byte[] file = File.ReadAllBytes(ofd.FileName);
            CNM.SendSock.Send(new byte[1] { 4 });
            CNM.SendSock.Send(BitConverter.GetBytes(TargetUser.Text.Length));
            CNM.SendSock.Send(Encoding.ASCII.GetBytes(TargetUser.Text));
            CNM.SendSock.Send(BitConverter.GetBytes(file.Length));
            CNM.SendSock.Send(file);

            byte[] msg = new byte[1];
            CNM.SendSock.Receive(msg);
            if (msg[0] == BitConverter.GetBytes(0)[0])
            {
                MessageBox.Show("Failed to send file!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            ChatBox.Text += (CNM.MyUsername + ": Sent file - \"" + ofd.SafeFileName + '\"' + Environment.NewLine);
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).OnLogout();
        }
    }
}
