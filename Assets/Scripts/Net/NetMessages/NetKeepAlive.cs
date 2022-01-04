using System;
using Unity.Networking.Transport;
using UnityEngine;

public class NetKeepAlive : NetMessage
{
    public NetKeepAlive()
    { // Making keepAlive message
        Code = OpCode.KEEP_ALIVE;
    }

    public NetKeepAlive(DataStreamReader reader)
    { //Receving keepAlive message
        Code = OpCode.KEEP_ALIVE;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        //Note that we already read the first byte containing Code
        //at this point, so nothing needed
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_KEEP_ALIVE?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_KEEP_ALIVE?.Invoke(this, cnn);
    }
}
