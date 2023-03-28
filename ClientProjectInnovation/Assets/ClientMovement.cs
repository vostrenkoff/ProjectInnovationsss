using shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

/**
 * The main ChatLobbyClient where you will have to do most of your work.
 * 
 * @author J.C. Wichman
 */
public class ClientMovement : MonoBehaviour
{
    //reference to the helper class that hides all the avatar management behind a blackbox
    /*private AvatarAreaManager _avatarAreaManager;
    private AvatarAreaManagerTester _avatarAreaManagerTester;*/
    //reference to the helper class that wraps the chat interface
    //private PanelWrapper _panelWrapper;

    [SerializeField] private string _server = "localhost";
    [SerializeField] private int _port = 54321;

    private TcpClient _client;
    private int id;
    private int x;
    private int y;
    private int z;
    private float speed = 0.1f;
    private void Start()
    {
        connectToServer();

        /*_avatarAreaManager = FindObjectOfType<AvatarAreaManager>();
        _avatarAreaManagerTester = FindObjectOfType<AvatarAreaManagerTester>();
        _avatarAreaManager.OnAvatarAreaClicked += onAvatarAreaClicked;

        _panelWrapper = FindObjectOfType<PanelWrapper>();
        _panelWrapper.OnChatTextEntered += onChatTextEntered;*/
    }

    private void connectToServer()
    {

        try
        {
            _client = new TcpClient();
            _client.Connect(_server, _port);
            Debug.Log("Connected to server. :0");
            //onClientsAvatarRequested();
        }
        catch (Exception e)
        {
            Debug.Log("Could not connect to server:");
            Debug.Log(e.Message);
        }
    }
    public void MoveLeft()
    {
        Packet outPacket = new Packet();
        outPacket.Write("sendMovement");
        outPacket.Write(speed);
        sendPacket(outPacket);
    }
    private void onAvatarAreaClicked(Vector3 pClickPosition)
    {
        Debug.Log("ChatLobbyClient: you clicked on " + pClickPosition);
        //TODO pass data to the server so that the server can send a position update to all clients (if the position is valid!!)

    }

    private void onChatTextEntered(string pText)
    {
        //_panelWrapper.ClearInput();
        sendString(pText);
    }

    private void sendString(string pOutString)
    {
        try
        {
            Packet outPacket = new Packet();
            outPacket.Write("sendMessage");
            outPacket.Write(pOutString);
            sendPacket(outPacket);
        }
        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            //_client.Close();
            //connectToServer();
        }
    }

    // RECEIVING CODE

    private void Update()
    {
        try
        {
            if (_client.Available > 0)
            {
                Packet inPacket = new Packet(StreamUtil.Read(_client.GetStream()));

                string command = inPacket.ReadString();

                if (command == "yourID") createAvatar(inPacket);
                //else if (command == "otherAvatars") createOtherAvatars(inPacket);
                else if (command == "newPlayerConnected") newPlayerConnected(inPacket);
                else if (command == "disconnectAvatar") disconnectAvatar(inPacket);
                else if (command == "getMessage") getMessage(inPacket);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            //_client.Close();
            //connectToServer();
        }
    }

    /*private void showMessage(string pText, int clientID)
    {

        //This is a stub for what should actually happen
        //What should actually happen is use an ID that you got from the server, to get the correct avatar
        //and show the text message through that
        List<int> allAvatarIds = _avatarAreaManager.GetAllAvatarIds();

        if (allAvatarIds.Count == 0)
        {
            Debug.Log("No avatars available to show text through:" + pText);
            return;
        }

        AvatarView avatarView = _avatarAreaManager.GetAvatarView(clientID);
        avatarView.Say(pText);
    }*/
    private void createAvatar(Packet pInPacket)
    {
        int clientID = pInPacket.ReadInt();

        //AvatarView avatarView = _avatarAreaManager.AddAvatarView(clientID);
        //Vector3 pos = _avatarAreaManagerTester.getRandomPosition();
        //x = (int)pos.x;
        //y = (int)pos.y;
        //z = (int)pos.z;

        //avatarView.transform.localPosition = new Vector3(x, y, z);
        //avatarView.SetSkin(clientID);
        id = clientID;
        Debug.Log("Your ID is " + id);

        //updatePlayersPosition(pos);
        //onOtherAvatarsRequested();
        //informOtherAvatarsRequest();
    }
    /*private void createOtherAvatars(Packet pInPacket)
    {
        int avatarsCount = pInPacket.ReadInt();
        for (int i = 0; i < avatarsCount; i++)
        {
            int clientID = pInPacket.ReadInt();
            int x = pInPacket.ReadInt();
            int y = pInPacket.ReadInt();
            int z = pInPacket.ReadInt();
            if (clientID != id)
            {
                AvatarView avatarView2 = _avatarAreaManager.AddAvatarView(clientID);
                avatarView2.transform.localPosition = new Vector3(x, y, z);
                avatarView2.SetSkin(clientID);
            }
        }
    }*/
    private void newPlayerConnected(Packet pInPacket)
    {
        int clientID = pInPacket.ReadInt();
        Debug.Log("New player connected with ID " + clientID);
        int x = pInPacket.ReadInt();
        int y = pInPacket.ReadInt();
        int z = pInPacket.ReadInt();
        if (clientID != id)
        {
            //AvatarView avatarView2 = _avatarAreaManager.AddAvatarView(clientID);
            //avatarView2.transform.localPosition = new Vector3(x, y, z);
            //avatarView2.SetSkin(clientID);
        }
    }
    private void disconnectAvatar(Packet pInPacket)
    {
        int clientID = pInPacket.ReadInt();
        Debug.Log("player disconnected with ID " + clientID);
        // _avatarAreaManager.RemoveAvatarView(clientID);
    }
    private void getMessage(Packet pInPacket)
    {
        int clientID = pInPacket.ReadInt();
        string message = pInPacket.ReadString();
        Debug.Log("got a message " + message);
        // showMessage(message, clientID);
    }

    private void onClientsAvatarRequested()
    {
        Packet outPacket = new Packet();
        outPacket.Write("getclientavatar");
        sendPacket(outPacket);
    }
    private void informOtherAvatarsRequest()
    {
        Packet outPacket = new Packet();
        outPacket.Write("somebodyjoined");
        sendPacket(outPacket);
    }
    private void onOtherAvatarsRequested()
    {
        Packet outPacket = new Packet();
        outPacket.Write("getavatars");
        sendPacket(outPacket);
    }
    private void updatePlayersPosition(Vector3 pos)
    {
        Packet outPacket = new Packet();
        outPacket.Write("updatePos");
        outPacket.Write((int)pos.x);
        outPacket.Write((int)pos.y);
        outPacket.Write((int)pos.z);
        sendPacket(outPacket);
    }
    private void sendPacket(Packet pOutPacket)
    {
        try
        {
            Debug.Log("Sending:" + pOutPacket);
            StreamUtil.Write(_client.GetStream(), pOutPacket.GetBytes());
        }

        catch (Exception e)
        {
            Debug.Log(e.Message);
            //_client.Close();
            //connectToServer();
        }
    }
}
