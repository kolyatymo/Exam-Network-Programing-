using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using Client_Request;
internal class Program
{
    private const int port = 4040;
    static TcpListener server;

    private static async Task Main(string[] args)
    {
        server = new TcpListener(IPAddress.Any, port);

        try
        {
            server.Start();
            Console.WriteLine("Server started!");

            Chat chat = new Chat();

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();

                Task.Run(() => chat.HandleClient(client));
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Server error: " + ex.Message);
        }
        finally
        {
            server.Stop();
        }
    }
}

class Chat
{
    private Dictionary<string, TcpClient> members = new();

    private Dictionary<string, StreamWriter> writers = new();

    private Dictionary<string, List<string>> groups = new();

    private object locker = new object();

    public async Task HandleClient(TcpClient client)
    {
        string name = "";

        try
        {
            NetworkStream ns = client.GetStream();

            StreamReader reader = new StreamReader(ns);
            StreamWriter writer = new StreamWriter(ns);

            while (true)
            {
                string? json = await reader.ReadLineAsync();

                if (json == null)
                    break;

                ClientCommand command =
                    JsonSerializer.Deserialize<ClientCommand>(json)!;

                switch (command.Command)
                {
                    case RequestCommand.Join:

                        name = command.Name;

                        lock (locker)
                        {
                            members[name] = client;
                            writers[name] = writer;
                        }

                        Console.WriteLine(name + " joined");

                        await SendOnlineUsers();
                        break;

                    case RequestCommand.Leave:
                        RemoveMember(name);
                        return;

                    case RequestCommand.OnlineUsersRequest:
                        await SendOnlineUsers();
                        break;

                    case RequestCommand.Message:
                        await SendAll(command);
                        break;

                    case RequestCommand.PrivateMessage:
                        await SendToUser(command.Receiver, command);
                        break;

                    case RequestCommand.GroupMessage:

                        if (command.Users != null)
                        {
                            foreach (string user in command.Users)
                            {
                                if (user != name)
                                    await SendToUser(user, command);
                            }
                        }
                        break;

                    case RequestCommand.CreateGroup:

                        foreach (string user in command.Users)
                        {
                            await SendToUser(user, command);
                        }
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (name != "")
                RemoveMember(name);

            client.Close();
            await SendOnlineUsers();
        }
    }

    private void RemoveMember(string name)
    {
        lock (locker)
        {
            members.Remove(name);
            writers.Remove(name);
        }

        Console.WriteLine(name + " left");
    }

    private async Task SendOnlineUsers()
    {
        ClientCommand command = new ClientCommand(RequestCommand.OnlineUsers);

        lock (locker)
        {
            command.Users = members.Keys.ToList();
        }

        await SendAll(command);
    }

    private async Task SendAll(ClientCommand command)
    {
        List<string> users;

        lock (locker)
        {
            users = members.Keys.ToList();
        }

        foreach (string user in users)
            await SendToUser(user, command);
    }

    private async Task SendToUser(string name, ClientCommand command)
    {
        StreamWriter? writer;

        lock (locker)
        {
            writers.TryGetValue(name, out writer);
        }

        if (writer != null)
        {
            try
            {
                string json = JsonSerializer.Serialize(command);

                await writer.WriteLineAsync(json);
                await writer.FlushAsync();
            }
            catch
            {
                Console.WriteLine("Cannot send message to " + name);
            }
        }
    }
}

