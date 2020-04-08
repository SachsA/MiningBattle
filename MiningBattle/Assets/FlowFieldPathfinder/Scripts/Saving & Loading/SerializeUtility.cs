using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class SerializeUtility 
{
    #region PublicMethods

    public static byte[] SerializeObject(object obj)
    {
        try
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            formatter.Serialize(memoryStream, obj);

            return memoryStream.ToArray();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        return null;
    }

    public static object DeserializeObject(byte[] data)
    {
        try
        {
            if(data == null || data.Length == 0)
                return null;

            IFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(data);

            return formatter.Deserialize(memoryStream);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        return null;
    }

    #endregion
}
