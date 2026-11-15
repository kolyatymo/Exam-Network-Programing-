using Client_db;
using Client_Request;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace FinalWork
{
    /// <summary>
    /// Interaction logic for Window_Clients.xaml
    /// </summary>
    public partial class Window_Clients : Window
    {
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;

        private string remoteIP = "127.0.0.1";
        private int port = 4040;
        TcpClient client = new TcpClient();
        ObservableCollection<string> messages;
        private string clientName = "";

        private Windows_Clients_Chat activeChat;

        private Window_Group_Chat activeGroupChat;

        private int clientId;

        public Window_Clients(string login)
        {
            InitializeComponent();

            using (Clients_db db = new Clients_db())
            {
                var user = db.Clients.FirstOrDefault(x => x.Login == login);

                if (user != null)
                {
                    clientId = user.Id;
                    clientName = user.Name;
                    NameTextBox.Text = user.Name;
                }
            }

            messages = new ObservableCollection<string>();
            this.DataContext = messages;
        }

        private async void Button_Okay(object sender, RoutedEventArgs e)
        {
            if (client.Connected)
                return;

            try
            {
                await client.ConnectAsync(
                    IPAddress.Parse(remoteIP), port);

                stream = client.GetStream();

                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                await SendRequest(new ClientCommand(RequestCommand.Join)
                {
                    Name = clientName
                });

                ServerListener();

                MessageBox.Show("Connected!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task SendRequest(ClientCommand command)
        {
            if (writer == null)
                return;

            string json = JsonSerializer.Serialize(command);

            await writer.WriteLineAsync(json);
            await writer.FlushAsync();
        }

        private async Task ServerListener()
        {
            try
            {
                while (client.Connected)
                {
                    string response = await reader.ReadLineAsync();

                    if (response == null)
                        break;

                    ClientCommand command = JsonSerializer.Deserialize<ClientCommand>(response);

                    Dispatcher.Invoke(() =>
                    {
                        if (command.Command == RequestCommand.OnlineUsers)
                        {
                            Checking_users.Items.Clear();

                            foreach (string user in command.Users)
                            {
                                if (user != clientName)
                                    Checking_users.Items.Add(user);
                            }
                        }

                        if (command.Command == RequestCommand.PrivateMessage)
                        {
                            if (activeChat != null)
                            {
                                activeChat.AddMessage(command.Name + " --> " + command.Message);
                            }
                        }

                        if (command.Command == RequestCommand.GroupMessage)
                        {
                            if (activeGroupChat != null)
                            {
                                activeGroupChat.AddMessage(command.Name + " --> " + command.Message);
                            }
                        }
                    });
                }
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Connection lost");
                });
            }
        }

        private Task<ClientCommand> GetResponse()
        {
            return Task.Run(() =>
            {
                NetworkStream ns = client.GetStream();
                StreamReader sr = new StreamReader(ns);
                string resp = sr.ReadLine()!;

                return JsonSerializer.Deserialize<ClientCommand>(resp)!;
            });
        }

        private async void Button_Check_User(object sender, RoutedEventArgs e)
        {
            await SendRequest(new ClientCommand(RequestCommand.OnlineUsersRequest));
        }

        private void Button_Contact(object sender, RoutedEventArgs e)
        {
            if (Checking_users.SelectedItem == null)
            {
                MessageBox.Show("Choose user!");
                return;
            }

            string selectedName = Checking_users.SelectedItem.ToString();

            using (Clients_db db = new Clients_db())
            {
                var selectedUser = db.Clients.FirstOrDefault(
                    x => x.Name == selectedName);

                if (selectedUser == null)
                    return;

                var contact = db.Contacts.FirstOrDefault(
                    x => x.ClientId == clientId &&
                         x.ContactClientId == selectedUser.Id);

                if (contact != null)
                {
                    MessageBox.Show("Contact already exists!");
                    return;
                }

                db.Contacts.Add(new Contact()
                {
                    ClientId = clientId,
                    ContactClientId = selectedUser.Id
                });

                db.SaveChanges();
            }

            MessageBox.Show("Contact added!");
        }
        private void Button_Group_Chat(object sender, RoutedEventArgs e)
        {
            List<string> users = Checking_users.SelectedItems.Cast<string>().ToList();

            if (users.Count == 0)
            {
                MessageBox.Show("Choose users!");
                return;
            }

            users.Add(clientName);

            activeGroupChat = new Window_Group_Chat(clientName,users,SendRequest);

            ClientCommand command =
                new ClientCommand(RequestCommand.CreateGroup);

            command.Name = clientName;
            command.Users = users;
            command.GroupId = Guid.NewGuid().ToString();

            SendRequest(command);

            activeGroupChat.Show();
        }
        private void Button_Go_Chat(object sender, RoutedEventArgs e)
        {
            if (Checking_users.SelectedItem == null)
            {
                MessageBox.Show("Choose user!");
                return;
            }

            string receiver = Checking_users.SelectedItem.ToString();

            activeChat = new Windows_Clients_Chat(clientName, receiver, SendRequest);

            activeChat.Show();
        }

        private void Button_Checking_Contacts(object sender, RoutedEventArgs e)
        {
            ShowContacts();
        }

        public void ShowContacts()
        {
            Checking_contacts.Items.Clear();

            using (Clients_db db = new Clients_db())
            {
                var contacts = db.Contacts
                    .Where(x => x.ClientId == clientId)
                    .Select(x => x.ContactClientId)
                    .ToList();

                var users = db.Clients
                    .Where(x => contacts.Contains(x.Id))
                    .ToList();

                foreach (var user in users)
                {
                    Checking_contacts.Items.Add(user.Name);
                }
            }
        }
    }
}
