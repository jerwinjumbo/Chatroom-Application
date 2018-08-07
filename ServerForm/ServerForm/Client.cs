using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ServerForm
{
    class Client
    {
        private bool running = true;
        private TcpClient client;
        private Dictionary<string, TcpClient> clientDct;
        private string name;
        private Server UI;
        private readonly Regex _rgx = new Regex("[^a-zA-Z0-9 -/+=]");
        public delegate void UpdateTextCallback(string message, string userid);
        public delegate void Broadcasting(string message, string id);
        List<TcpClient> clients;

        public Client(TcpClient tcp, Dictionary<string, TcpClient> users, string id)
        {
            client = tcp;
            clientDct = users;
            name = id;
            var chat = new Thread(ReceiveChat);
            chat.Start();
        }

        public Client(Server gui)
        {
            UI = gui;
        }

        public Client(Server gui, List<TcpClient> clientList)
        {
            UI = gui;
            clients = clientList;
        }

        public void StartClient(TcpClient tcp, Dictionary<string, TcpClient> users, string id)
        {
            client = tcp;
            clientDct = users;
            name = id;
            var chat = new Thread(ReceiveChat);
            chat.Start();
        }

        public void StopClient()
        {
            running = false;
        }

        private void UpdateText(string message, string username)
        {
            var msg = _rgx.Replace(message, "");
            var replaceName = _rgx.Replace(username, "");
            if (UI != null)
            {
                UI.GetText().Text += replaceName + ": " + msg + Environment.NewLine;
                //if (username.Equals("yes"))
                //{
                    
                //}
                //else if (username.Equals("no"))
                //{
                //    UI.GetText().Text += msg + Environment.NewLine;
                //}
            }
        }

        public void BroadCast(string message, string name)
        {
            var msg = _rgx.Replace(message, "");
            var user = "";
            if (!string.IsNullOrEmpty(this.name))
            {
                user = this.name;
            }
            else
            {
                user = "Anonymous";
            }
            try
            {
                foreach (var item in this.clientDct)
                {
                    TcpClient client = item.Value;
                    Stream broadcastStream = client.GetStream();
                    byte[] broacastBytes = new byte[1024];
                    broacastBytes = Encoding.UTF8.GetBytes(user + ": " + msg);
                    broadcastStream.Write(broacastBytes, 0, broacastBytes.Length);
                    broadcastStream.Flush();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex);
            }
        }

        public void ReceiveChat()
        {
            while (running)
            {
                try
                {
                    byte[] bytesFrom = new byte[1024];
                    string clientData = null;
                    Stream st = client.GetStream();

                    st.Read(bytesFrom, 0, 1024);
                    clientData = Encoding.UTF8.GetString(bytesFrom);
                    UI.GetText().Dispatcher.Invoke(new UpdateTextCallback(UpdateText), new Object[] { clientData, name});

                    BroadCast(clientData, name);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error: " + e);
                }
            }

        }
    }
}
