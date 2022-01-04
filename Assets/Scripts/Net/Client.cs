using System;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    #region Singleton Implementation
    public static Client Instance { get; set; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public NetworkDriver driver;
    private NetworkConnection connection;
    private bool isActive;
    public Action connectionDropped;

    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endPoint = NetworkEndPoint.Parse(ip, port);
        connection = driver.Connect(endPoint);
        Debug.Log("Attempting to connect to Server on " + endPoint.Address);
        if (connection == null || connection == default(NetworkConnection))
        {
            Debug.Log("connection unsuccessful");
            return;
        }
        isActive = true;
        RegisterToEvent();
    }

    public void Shutdown()
    {
        if (isActive)
        {
            UnregisterToEvent();
            driver.Dispose();
            isActive = false;
            connection = default(NetworkConnection);
        }
    }

    public void OnDestroy()
    {
        Shutdown();
    }

    public void Update()
    {
        if (!isActive)
        {
            return;
        }
        driver.ScheduleUpdate().Complete();
        CheckConnectionToServerAlive();
        UpdateMessagePump();
    }

    private void CheckConnectionToServerAlive()
    {
        if (!connection.IsCreated && isActive)
        {
            Debug.Log("Something went wrong, lost connection to server");
            connectionDropped?.Invoke();
            Shutdown();
        }
    }

    private void UpdateMessagePump()
    {
        DataStreamReader stream;

        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                SendToServer(new NetWelcome());
                Debug.Log("Connected");
            }
            if (cmd == NetworkEvent.Type.Data)
            {
                NetUtility.OnData(stream, default(NetworkConnection));
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client disconnected from server");
                connection = default(NetworkConnection);
                connectionDropped?.Invoke();
                Shutdown(); //Only because this is two-player game.
            }
        }
    }

    //Client specific
    public void SendToServer(NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    //Event parsing
    private void RegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }

    private void UnregisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }

    private void OnKeepAlive(NetMessage nm)
    {
        //Send the keep alive msg backt to server
        SendToServer(nm);
    }
}
