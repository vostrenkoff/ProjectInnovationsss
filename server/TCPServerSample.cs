using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using shared;
using System.Threading;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Collections;
using System.Collections.Concurrent;

/**
 * This class implements a simple tcp echo server.
 * Read carefully through the comments below.
 * Note that the server does not contain any sort of error handling.
 */
class TCPServerSample
{
    public static void Main(string[] args)
    {
        TCPServerSample server = new TCPServerSample();
        server.run();
    }
    private int _port = 54321;
    private TcpListener _listener;
    private List<TcpClient> _clients = new List<TcpClient>();
    private List<ClientInfo> _clientsList = new List<ClientInfo>();
    private Dictionary<TcpClient, ClientInfo> _clientsDictionary = new Dictionary<TcpClient, ClientInfo>();
    private int _clientId = 0;

    private int x;
    private int y;
    private int z;

    private string message;
    private void run()
    {
        Console.WriteLine("Server started on port " + _port);

        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();

        while (true)
        {
            processNewClients();
            processExistingClients();

            Thread.Sleep(100);

            foreach (TcpClient client in _clients)
            {
                try
                {
                    Packet outPacket = new Packet();
                    outPacket.Write("h");
                    sendPacket(client, outPacket);
                }
                catch (Exception ex)
                {
                    int index = _clients.FindIndex(a => a == client);
                    _clients.Remove(client);
                    disconnectAvatar(client);
                    _clientsDictionary.Remove(client);
                    client.Close();
                    break;
                }
            }
        }
    }


    private void processNewClients()
    {
        while (_listener.Pending())
        {
            TcpClient client = _listener.AcceptTcpClient();
            Console.WriteLine("Accepted new client with ID " + _clientId);
            _clients.Add(client);
        }
    }

    private void processExistingClients()
    {
        foreach (TcpClient client in _clients)
        {
            try
            {

                if (client.Available == 0)
                {

                    continue;
                }

                byte[] inBytes = StreamUtil.Read(client.GetStream());
                Packet inPacket = new Packet(inBytes);
                string command = inPacket.ReadString();

                Console.WriteLine("Received command:" + command);
                if (command == "getavatars")
                {
                    handleSendAvatars(client);
                }
                else if (command == "getclientavatar")
                {
                    handleCreateAvatar(client);
                }
                else if (command == "somebodyjoined")
                {
                    handleCreateNewPlayerAvatar(client);
                }
                else if (command == "sendMessage")
                {
                    message = inPacket.ReadString();
                    handleSendMessage(client);
                }
                else if (command == "updatePos")
                {
                    handleUpdatePos(client, inPacket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exeption.");
            }
        }
    }
    private void handleCreateAvatar(TcpClient pClient)
    {
        _clientId++;
        ClientInfo clientInfo = new ClientInfo(_clientId, x, y, z);
        _clientsDictionary.Add(pClient, clientInfo);

        Packet outPacket = new Packet();
        outPacket.Write("yourID");
        outPacket.Write(_clientId);
        sendPacket(pClient, outPacket);

    }
    private void handleSendMessage(TcpClient pClient)
    {
        if (_clients.Count >= 1)
        {
            ClientInfo info;
            bool success = _clientsDictionary.TryGetValue(pClient, out info);
            if (success)
            {
                Packet outPacket = new Packet();
                outPacket.Write("getMessage");
                outPacket.Write(info.Id);
                outPacket.Write(message);
                foreach (TcpClient oldClient in _clients)
                {
                    sendPacket(oldClient, outPacket);
                }
            }
        }
    }
    private void disconnectAvatar(TcpClient pClient)
    {
        ClientInfo info;
        bool success = _clientsDictionary.TryGetValue(pClient, out info);
        if (success)
        {
            Debug.WriteLine("disconnecting.");
            Packet outPacket = new Packet();
            outPacket.Write("disconnectAvatar");
            outPacket.Write(info.Id);
            foreach (TcpClient oldClient in _clients)
            {
                sendPacket(oldClient, outPacket);
            }
        }
    }
    private void handleUpdatePos(TcpClient pClient, Packet inPacket)
    {
        ClientInfo info;
        bool success = _clientsDictionary.TryGetValue(pClient, out info);
        if (success)
        {
            info.x = inPacket.ReadInt();
            info.y = inPacket.ReadInt();
            info.z = inPacket.ReadInt();
        }
    }
    private void handleCreateNewPlayerAvatar(TcpClient pClient)
    {
        if (_clients.Count >= 1)
        {
            ClientInfo info;
            bool success = _clientsDictionary.TryGetValue(pClient, out info);
            if (success)
            {
                Packet outPacket = new Packet();
                outPacket.Write("newPlayerConnected");
                outPacket.Write(info.Id);
                outPacket.Write(info.x);
                outPacket.Write(info.y);
                outPacket.Write(info.z);
                    Console.WriteLine("Client joined with ID " + info.Id);
                foreach (TcpClient oldClient in _clients)
                {
                    sendPacket(oldClient, outPacket);
                }
            }
        }
    }

    private void handleSendAvatars(TcpClient pClient)
    {
        Packet outPacket = new Packet();
        outPacket.Write("otherAvatars");
        outPacket.Write(_clientsDictionary.Count);


        foreach (var entry in _clientsDictionary)
        {
            outPacket.Write(entry.Value.Id);
            outPacket.Write(entry.Value.x);
            outPacket.Write(entry.Value.y);
            outPacket.Write(entry.Value.z);
        }

        sendPacket(pClient, outPacket);
    }

    private void sendPacket(TcpClient pClient, Packet pOutPacket)
    {
        StreamUtil.Write(pClient.GetStream(), pOutPacket.GetBytes());
    }
}

public class ClientInfo
{
    public int Id { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public int z { get; set; }

    public ClientInfo(int id, int X, int Y, int Z)
    {
        Id = id;
        x = X;
        y = Y;
        z = Z;
    }
}