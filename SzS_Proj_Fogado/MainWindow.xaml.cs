using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SzS_Proj_Fogado
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket clientSocket;
        private const int PORT = 11111;
        private Thread receiveThread;
        private static readonly byte[] buffer = new byte[1024];

        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Bezáráskor leállítjuk a kapcsolatot
            clientSocket?.Close();
            receiveThread?.Abort();
        }

        private void ConnectToServer()
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, PORT));
                AddMessageToList("Connected to server.");

                // Külön szálon fogadjuk az üzeneteket a szervertől
                receiveThread = new Thread(ReceiveData);
                receiveThread.Start();
            }
            catch (SocketException ex)
            {
                AddMessageToList("Unable to connect to server: " + ex.Message);
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                SendMessage(message);
                MessageTextBox.Clear();
            }
        }

        private void SendMessage(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            clientSocket.Send(data);
            AddMessageToList("You: " + message);
        }

        private void ReceiveData()
        {
            while (true)
            {
                try
                {
                    int received = clientSocket.Receive(buffer);
                    string message = Encoding.ASCII.GetString(buffer, 0, received);
                    Dispatcher.Invoke(() => AddMessageToList("Server: " + message));
                }
                catch (SocketException)
                {
                    Dispatcher.Invoke(() => AddMessageToList("Disconnected from server."));
                    clientSocket.Close();
                    break;
                }
            }
        }

        private void AddMessageToList(string message)
        {
            MessagesListBox.Items.Add(message);
        }
    }
}
