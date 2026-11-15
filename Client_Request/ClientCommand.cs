using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_Request
{
    public enum RequestCommand
    {
        Join, Leave, Message, PrivateMessage, File, Image, OnlineUsersRequest, OnlineUsers, CreateGroup, GroupMessage
    }

    public class ClientCommand
    {
        public RequestCommand Command { get; set; }

        public string Message { get; set; }
        public string Name { get; set; }
        public string Receiver { get; set; }
        public string GroupId { get; set; }
        public List<string> Users { get; set; } = new List<string>();
        public DateTime Time { get; set; }

        public ClientCommand() { }

        public ClientCommand(RequestCommand command)
        {
            Command = command;
            Time = DateTime.Now;
        }

        public ClientCommand(string message, string name, DateTime time, RequestCommand command = RequestCommand.Message)
        {
            Message = message;
            Name = name;
            Time = time;
            Command = command;
        }
    }
}
