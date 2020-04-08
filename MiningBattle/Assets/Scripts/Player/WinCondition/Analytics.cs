using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;

public struct Analytics
{
    public bool hasWon;
    public int attackBuilt;
    public int minerBuilt;
    public int defenceBuilt;
    public int moneyEarned;
    public int spaceshipDestroyed;
    public int spaceshipGotDestroyed;
}

public static class SerializedAnalytics
{
    private static readonly int SizeOfAnalytics = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Analytics));

    public static readonly byte[] MemCustomTile = new byte[SizeOfAnalytics];

    public static byte[] Serialize(object customObj)
    {
        Analytics analytics = (Analytics)customObj;
        byte[] bytes;
        int hasWon;

        lock (MemCustomTile)
        {
            bytes = MemCustomTile;
            int index = 0;
            hasWon = (analytics.hasWon) ? (1) : (0);
            Protocol.Serialize(hasWon, bytes, ref index);
            Protocol.Serialize(analytics.attackBuilt, bytes, ref index);
            Protocol.Serialize(analytics.minerBuilt, bytes, ref index);
            Protocol.Serialize(analytics.defenceBuilt, bytes, ref index);
            Protocol.Serialize(analytics.moneyEarned, bytes, ref index);
            Protocol.Serialize(analytics.spaceshipDestroyed, bytes, ref index);
            Protocol.Serialize(analytics.spaceshipGotDestroyed, bytes, ref index);
        }
        return bytes;
    }

    public static object Deserialize(byte[] dataStream)
    {
        Analytics analytics = new Analytics();
        int hasWon;

        lock (MemCustomTile)
        {
            int index = 0;
            Protocol.Deserialize(out hasWon, dataStream, ref index);
            Protocol.Deserialize(out analytics.attackBuilt, dataStream, ref index);
            Protocol.Deserialize(out analytics.minerBuilt, dataStream, ref index);
            Protocol.Deserialize(out analytics.defenceBuilt, dataStream, ref index);
            Protocol.Deserialize(out analytics.moneyEarned, dataStream, ref index);
            Protocol.Deserialize(out analytics.spaceshipDestroyed, dataStream, ref index);
            Protocol.Deserialize(out analytics.spaceshipGotDestroyed, dataStream, ref index);

            analytics.hasWon = (hasWon == 1) ? true : false;
        }

        return analytics;
    }
}