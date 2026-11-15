using Client_db;
using Client_Request;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Interaction logic for Window_Group_Chat.xaml
    /// </summary>
    public partial class Window_Group_Chat : Window
    {
        private string clientName;

        private List<string> groupUsers;

        private Func<ClientCommand, Task> sendCommand;

        public Window_Group_Chat(string name, List<string> users, Func<ClientCommand, Task> send)
        {
            InitializeComponent();

            clientName = name;
            groupUsers = users;
            sendCommand = send;

            Title = "Group Chat";
        }

        public void AddMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ListBoxGroupMessage.Items.Add(message);
            });
        }

        private async void Click_Send(object sender, RoutedEventArgs e)
        {
            if (msgTextBoxGroup.Text == "")
                return;

            ClientCommand command = new ClientCommand(msgTextBoxGroup.Text, clientName, DateTime.Now, RequestCommand.GroupMessage);

            command.Users = groupUsers;

            await sendCommand(command);

            ListBoxGroupMessage.Items.Add("You --> " + msgTextBoxGroup.Text);

            msgTextBoxGroup.Clear();
        }

        private void Click_Disconnect(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CLick_Send_Folder(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                ListBoxGroupMessage.Items.Add("Selected file --> " + dialog.FileName);
            }
        }

        private void Click_Send_Image(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                ListBoxGroupMessage.Items.Add("Selected image --> " + dialog.FileName);
            }
        }
    }
}
