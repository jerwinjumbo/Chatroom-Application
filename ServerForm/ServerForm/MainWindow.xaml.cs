using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace ServerForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Server : Window
    {
        private readonly BackgroundWorker ui;
        private bool socketStatus = false;

        public static Dictionary<string, TcpClient> allClients = new Dictionary<string, TcpClient>();
        public TcpListener serverSocket;
        public TcpClient clientSocket;
        private Socket currentSocket;

        public int Count = 0;

        private readonly TextBlock _writeText = new TextBlock();

        List<Client> clients = new List<Client>();
        List<TcpClient> sockets = new List<TcpClient>();

        public Server()
        {
            ui = new BackgroundWorker();
            var ip = IPAddress.Parse("127.0.0.1");
            clientSocket = default(TcpClient);
            serverSocket = new TcpListener(ip, 100);
            InitializeComponent();
            ui.DoWork += UIWorker;
            Closing += WindowClosed;
            SvChat.Content = _writeText;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            ui.RunWorkerAsync();
            socketStatus = true;
            BtnStop.IsEnabled = true;
            BtnStart.IsEnabled = false;
        }

        public void UIWorker(object sender, DoWorkEventArgs e)
        {
            serverSocket.Start();
            while (true)
            {
                try
                {
                    clientSocket = serverSocket.AcceptTcpClient();
                    Count += 1;
                    byte[] buffer = new byte[1024];
                    Stream st = clientSocket.GetStream();
                    st.Read(buffer, 0, 1024);
                    string clientData = Encoding.ASCII.GetString(buffer);
                    allClients.Add(clientData, clientSocket);
                    sockets.Add(clientSocket);
                    Client c = new Client(this, sockets);
                    c.StartClient(clientSocket, allClients, clientData);
                    clients.Add(c);
                    MessageBox.Show("Client Connected: " + clientData);

                }
                catch (Exception ex)
                {
                    Environment.Exit(0);
                }
            }
        }

        public void WindowClosed(object sender, CancelEventArgs e)
        {
            foreach (Client c in clients)
            {
                c.StopClient();
            }
        }

        public TextBlock GetText()
        {
            return _writeText;
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;
            socketStatus = false;
            serverSocket.Stop();
        }
    }
}
