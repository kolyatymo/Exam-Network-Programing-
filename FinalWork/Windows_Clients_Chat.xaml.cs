using Client_Request;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for Windows_Clients_Chat.xaml
    /// </summary>
    public partial class Windows_Clients_Chat : Window
    {
        private string clientName;
        private string receiver;

        private Func<ClientCommand, Task> sendCommand;

        public Windows_Clients_Chat(
            string name,
            string user,
            Func<ClientCommand, Task> send)
        {
            InitializeComponent();

            clientName = name;
            receiver = user;

            sendCommand = send;

            Title = "Chat with " + receiver;
        }

        public void AddMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ListBoxMessage.Items.Add(message);
            });
        }

        private async void Click_Send(object sender, RoutedEventArgs e)
        {
            if (msgTextBox.Text == "")
                return;

            ClientCommand command = new ClientCommand(msgTextBox.Text, clientName, DateTime.Now, RequestCommand.PrivateMessage);

            command.Receiver = receiver;

            await sendCommand(command);

            ListBoxMessage.Items.Add("You --> " + msgTextBox.Text);

            msgTextBox.Clear();
        }

        private void CLick_Send_Folder(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                ListBoxMessage.Items.Add("Selected file --> " + dialog.FileName);
            }
        }

        private void Click_Send_Image(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                ListBoxMessage.Items.Add("Selected image --> " + dialog.FileName);
            }
        }
    }
}
