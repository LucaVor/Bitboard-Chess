using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void UDPTest(Packet _packet)
    {
        string _msg = _packet.ReadString();

        Debug.Log($"Received packet via UDP. Contains message: {_msg}");
        ClientSend.UDPTestReceived();
    }

    public static void AssignSide (Packet _packet)
    {
        bool isWhite = _packet.ReadBool();
        Debug.Log ($"Got side ({isWhite}) from server.");

        QuickChess.GameManager.instance.Init (isWhite);
    }

    public static void OnOpponentMove (Packet _packet)
    {
        int from = _packet.ReadInt ();
        int to = _packet.ReadInt ();

        Debug.Log ($"Got {from} {to}");

        QuickChess.GameManager.instance.Move (from, to, true);
    }
}
