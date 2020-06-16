namespace ipmessenger_client
{
    using AdonisUI.Controls;
    using System;

    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public static class command
    {
        /// <summary>
        /// Defines the setUserName, clear, setIp, help..
        /// </summary>
        public const string
            setUserName = "/setusername",
            clear = "/clear",
            setIp = "/setip",
            help = "/help";
    }

    /// <summary>
    /// Defines the <see cref="MainWindow" />.
    /// </summary>
    public partial class MainWindow : AdonisWindow
    {
        /// <summary>
        /// Defines the socketThread.
        /// </summary>
        private Thread socketThread;

        /// <summary>
        /// Defines the update.
        /// </summary>
        private Thread update;

        /// <summary>
        /// Defines the publicIPAddress.
        /// </summary>
        private string publicIPAddress;

        /// <summary>
        /// Defines the messages.
        /// </summary>
        private string[] messages;

        private string messageType = "str";
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            update = new Thread(new ThreadStart(update_frame));
            update.Start();
        }

        /// <summary>
        /// The update_frame.
        /// </summary>
        internal void update_frame()
        {
            while (true)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {

                });

                System.Threading.Thread.Sleep(100);


            }
        }

        /// <summary>
        /// The GetLocalIPAddress.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        internal static string GetLocalIPAddress()
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

        /// <summary>
        /// The Socketing.
        /// </summary>
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

        /// <summary>
        /// Defines the clientSocket.
        /// </summary>
        internal Socket clientSocket;

        /// <summary>
        /// Defines the port.
        /// </summary>
        private int port = 1998;

        /// <summary>
        /// Defines the ServerIP.
        /// </summary>
        //private string ServerIP = "2.56.212.56";
        private string ServerIP = "127.0.0.1";
        private MemoryStream ms;

        /// <summary>
        /// The SocketConnected.
        /// </summary>
        /// <param name="s">The s<see cref="Socket"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool SocketConnected(Socket s)
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

        /// <summary>
        /// The CreateClient.
        /// </summary>
        internal void CreateClient()
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ServerIP), port);
                clientSocket.Connect(endPoint);
            }
            catch
            {
                Console.WriteLine("Connection failed");
            }
        }

        /// <summary>
        /// The Send.
        /// </summary>
        /// <param name="message">The message<see cref="string"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool Send(string message)
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

        internal bool Send(byte[] buffer)
        {
            if (clientSocket?.Connected == true)
            {
       
                clientSocket.Send(buffer, 0, buffer.Length, 0);
                return true;
            }
            return false;
        }
        /// <summary>
        /// The Recieve.
        /// </summary>
        internal void Recieve()
        {
            try
            {



                if (clientSocket.Connected)
                {
                    byte[] buffer = new byte[1000000];
                    int recieved = clientSocket.Receive(buffer, buffer.Length, SocketFlags.None);
                    Array.Resize(ref buffer, recieved);
                    if (buffer.Length == 0)
                    {
                        return;
                    }
                    if (buffer[0] == 's')
                    {
                        string recievedMessage = Encoding.Default.GetString(buffer);
                        string str = recievedMessage.Substring(0, 3);
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            chatbox.Items.Insert(0, recievedMessage.Replace("str", ""));
                        });

                    }
                    else
                    {

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            BitmapImage bitmap = LoadImage(buffer);
                            Image newImage = new Image();
                            newImage.Source = bitmap;
                            chatbox.Items.Insert(0, newImage);
                        });
                    }
                }
                else
                {
                    chatbox.Items.Insert(0, "Not connected");
                }
            }
            catch
            {
                Console.WriteLine("Error");
            }
        }

        /// <summary>
        /// The Window_Closed.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="EventArgs"/>.</param>
        private void Window_Closed(object sender, EventArgs e)
        {
            if (socketThread != null)
                socketThread.Abort();
            if (update != null)
                update.Abort();
        }

        /// <summary>
        /// The Connect.
        /// </summary>
        private void Connect()
        {


            if (socketThread != null)
            {

                socketThread.Abort();
                clientSocket = null;
            }
            socketThread = new Thread(new ThreadStart(Socketing));
            socketThread.Start();
        }


        /// <summary>
        /// The checkCommand.
        /// </summary>
        private void checkCommand()
        {
            try
            {
                if (messageBox.Text[0] == '/')
                {
                    string[] tokens = messageBox.Text.Split(' ');
                    switch (tokens[0])
                    {
                        case (string)command.setUserName:
                            publicIPAddress = tokens[1];
                            chatbox.Items.Insert(0, "New username: " + publicIPAddress);
                            break;

                        case (string)command.clear:
                            chatbox.Items.Clear();
                            break;

                        case (string)command.setIp:
                            ServerIP = tokens[1];
                            chatbox.Items.Insert(0, "New ip address: " + ServerIP);
                            break;

                        case (string)command.help:
                            foreach (var foo in Enum.GetValues(typeof(command)))
                            {
                                Console.WriteLine(foo);
                            }
                            break;

                        default:
                            chatbox.Items.Insert(0, "unknown command: " + tokens[0]);
                            break;
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// The setUpConnection.
        /// </summary>
        private void setUpConnection()
        {
            while (clientSocket == null || clientSocket.Connected != true)
            {
                if (socketThread != null)
                {
                    socketThread.Abort();
                }
                Connect();
                System.Threading.Thread.Sleep(20);
            }
        }

        /// <summary>
        /// The Button_Send.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void Button_Send(object sender, RoutedEventArgs e)
        {

            setUpConnection();
            checkCommand();

            if (!Send(messageType + publicIPAddress + ": " + messageBox.Text))
            {
                chatbox.Items.Insert(0, "Socket is not connected");

            }

            messageBox.Text = "";
        }

        /// <summary>
        /// The MenuItem_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            chatbox.Items.Clear();
        }

        /// <summary>
        /// The Button_KeyDown.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.Windows.Input.KeyEventArgs"/>.</param>
        private void Button_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && messageBox.Text != "")
            {
                //execute go button method
                Button_Send(null, null);

            }
        }

        private void chatbox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                Image test = new Image();
                BitmapImage source = new BitmapImage();

                source.BeginInit();
                source.UriSource = new Uri(files[0]);
                source.DecodePixelWidth = 250;
                source.EndInit();

                byte[] byteimage = ImageToByte(source);
                Send(byteimage);

            }
            
        }

        public static Image recieve_image(byte[] buffer)
        {
            System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();
            return (Image)converter.ConvertTo(buffer, typeof(Image));

        }

        //void send_image(System.Windows.Controls.Image image)
        //{
        //    byte[] buffer = ImageToByte(image);            
        //    clientSocket.Send(buffer, buffer.Length, SocketFlags.None);
        //    Console.WriteLine("send succes");


        //}

        private static BitmapImage LoadImage(byte[] imageData)
        {
            Console.WriteLine("hello");
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static byte[] ImageToByte(BitmapImage img)
        {
            byte[] data;
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            return data;
        }

    }

}





