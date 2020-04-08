using ExitGames.Client.Photon;
using System;

[Serializable]
public struct CustomTile
{
    public int life;
    public int amount;
    public World.BlockType type;
    public bool isMineable;
    public bool isMined;
    public bool isDiscovered;

    public CustomTile(int _life, World.BlockType _type, int _amount)
    {
        life = _life;
        type = _type;
        amount = _amount;
        isMineable = false;
        isMined = false;
        isDiscovered = false;
    }
}

public static class SerializedCustomTile
{
    private static readonly int SizeOfCT = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CustomTile)) + 4;

    public static readonly byte[] MemCustomTile = new byte[SizeOfCT];

    public static byte[] Serialize(object customObj)
    {
        CustomTile tile = (CustomTile)customObj;
        byte[] bytes;

        lock (MemCustomTile)
        {
            bytes = MemCustomTile;
            int index = 0;
            Protocol.Serialize(tile.life, bytes, ref index);
            int type = (int)tile.type;
            Protocol.Serialize(tile.amount, bytes, ref index);
            Protocol.Serialize(type, bytes, ref index);
        }
        return bytes;
    }

    public static object Deserialize(byte[] dataStream)
    {
        CustomTile tile = new CustomTile();

        lock (MemCustomTile)
        {
            int index = 0;
            Protocol.Deserialize(out tile.life, dataStream, ref index);
            Protocol.Deserialize(out tile.amount, dataStream, ref index);
            int type = 0;
            Protocol.Deserialize(out type, dataStream, ref index);
            tile.type = (World.BlockType)type;
        }

        return tile;
    }
}