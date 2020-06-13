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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading;
using AdonisUI.Controls;
using System.IO;

namespace ipmessenger_client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : AdonisWindow
    {

        //private List<string> recievedMessages = new List<string>();
        private Thread socketThread;
        private string publicIPAddress;

        public MainWindow()
        {
            InitializeComponent();
            publicIPAddress = GetLocalIPAddress();



        }

        static string GetLocalIPAddress()
        {
            String address = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                address = stream.ReadToEnd();
            }

            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);

            return address;
        }

        public void Socketing()
        {
            while (true)
            {
                if (SocketConnected(clientSocket))
                {
                    Recieve();
                }
                else
                {
                    if (clientSocket != null)
                    {
                        clientSocket.Close();
                    }
                    CreateClient();
                }

            }
        }

        Socket clientSocket;
        private int port = 1998;
        private string LocalIP = "127.0.0.1";


        bool SocketConnected(Socket s)
        {
            if (s == null)
            {
                return false;
            }
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        void CreateClient()
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(LocalIP), port);
                clientSocket.Connect(endPoint);
            }
            catch
            {
                Console.WriteLine("Connection failed");
            }
        }
        bool Send(string message)
        {

            if (clientSocket?.Connected == true)
            {
                string msg = message;
                byte[] msgBuffer = Encoding.Default.GetBytes(msg);
                clientSocket.Send(msgBuffer, 0, msgBuffer.Length, 0);
                return true;
            }
            return false;
        }

        void Recieve()
        {
            if (clientSocket.Connected)
            {
                byte[] buffer = new byte[255];
                int recieved = clientSocket.Receive(buffer, 0, buffer.Length, 0);
                Array.Resize(ref buffer, recieved);
                string recievedMessage = Encoding.Default.GetString(buffer);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    chatbox.Items.Add(recievedMessage);
                });


            }
            else
            {
                Console.WriteLine("Socket is not connected");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (socketThread != null)
                socketThread.Abort();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LocalIP = ipAddressBox.Text;
            chatbox.Items.Add("Now connecting to ip: " + LocalIP);
            if (socketThread != null)
            {
                socketThread.Abort();
                clientSocket = null;
            }
            socketThread = new Thread(new ThreadStart(Socketing));
            socketThread.Start();



        }

        private void Button_Send(object sender, RoutedEventArgs e)
        {
            if (!Send(publicIPAddress + ": " + messageBox.Text ))
            {
                chatbox.Items.Add("Socket is not connected");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            chatbox.Items.Clear();
        }
    }
}
