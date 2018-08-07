using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace ClientForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool clientStatus = true;
        private bool _connected;
        private string _data;
        private bool msg = true;
        private string name;
        private int counter = 0;

        private Thread t;
        private readonly string ip = "127.0.0.1";
        private readonly int port = 100;

        private readonly TextBlock _writeText = new TextBlock();

        private readonly TcpClient _clientSocket = new TcpClient();
        NetworkStream ns = default(NetworkStream);
        public delegate void UpdateHistory(string data);
        private readonly Regex _rgx = new Regex("[^a-zA-Z0-9 -/+=>]");

        public MainWindow()
        {
            InitializeComponent();
            Closing += OnWindowClosed;
            SvClient.Content = _writeText;
        }

        private void OnWindowClosed(object sender, CancelEventArgs e)
        {
            StopChat();
        }

        private void StopChat()
        {
            clientStatus = false;
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!_connected)
            {
                var username = TxtName.Text;
                if (string.IsNullOrEmpty(username))
                {
                    username = "Anonymous";
                }

                ConnectingServer(ip, username);
                _connected = true;
            }
        }

        private void ConnectingServer(string serverIP, string name)
        {
            _data = "Connecting to server...";
            DisplayMessage(_data);
            try
            {
                _clientSocket.Connect(serverIP, 100);
                ns = _clientSocket.GetStream();
                byte[] buffer = Encoding.ASCII.GetBytes(name);
                ns.Write(buffer, 0, buffer.Length);
                ns.Flush();
                _data = "Connected to server.";
                DisplayMessage(_data);
                BtnSend.IsEnabled = true;
                BtnConnect.IsEnabled = false;
                TxtName.IsEnabled = false;
                t = new Thread(ReceiveMessage);
                t.Start();
            }
            catch (SocketException se)
            {
                MessageBox.Show("Failed to connect: " + se);
            }
        }

        private void ReceiveMessage()
        {
            try
            {
                while (clientStatus)
                {
                    Stream s = _clientSocket.GetStream();
                    byte[] buffer = new byte[1024];
                    s.Read(buffer, 0, 1024);
                    string sendData = Encoding.ASCII.GetString(buffer);
                    sendData = _rgx.Replace(sendData, "");
                    if (counter == 1)
                    {
                        msg = false;
                        Dispatcher.Invoke(new UpdateHistory(DisplayMessage), sendData);
                        counter--;
                    }
                    else
                    {
                        name = sendData;
                        counter++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No message received: " + ex);
            }
        }

        private void DisplayMessage(string data)
        {
            if (msg != true)
            {
                _writeText.Text += name + ": " + data + Environment.NewLine;
            }
            else
            {
                _writeText.Text += data + Environment.NewLine;
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_clientSocket.Connected)
                {
                    Stream s = _clientSocket.GetStream();
                    byte[] buffer = Encoding.ASCII.GetBytes(TxtInput.Text);
                    s.Write(buffer, 0, buffer.Length);
                    s.Flush();
                    TxtInput.Clear();
                }
                else
                {
                    MessageBox.Show("Connected.");
                }
            }
            catch (SocketException se)
            {
                MessageBox.Show("Message not sent: " + se);
            }
        }
    }
}
