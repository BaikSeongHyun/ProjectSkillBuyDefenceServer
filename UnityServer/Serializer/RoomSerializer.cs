using System.Collections;
using System.Text;

public class RoomSerializer : Serializer 
{
	public bool Serialize(Room[] data)
	{
		bool ret = true;
		byte roomDataLength;

		ret &= Serialize ((byte) data.Length);

		for (int i = 0; i < data.Length; i++)
		{
			ret &= Serialize ((byte) data [i].roomNum);
			ret &= Serialize ((byte) data [i].mapType);

			int length = Encoding.Unicode.GetBytes(data [i].hostId).Length + Encoding.Unicode.GetBytes(data [i].roomName).Length;

			roomDataLength = (byte) (length + 2);

			ret &= Serialize (roomDataLength);
			ret &= Serialize (data [i].hostId);
			ret &= Serialize (".");
			ret &= Serialize (data [i].roomName);
		}

		return ret;
	}

	public bool Deserialize(ref Room[] element)
	{
		if (GetDataSize() == 0)
		{
			return false;
		}

		bool ret = true;

		byte roomCount = 0;
		byte roomNum = 0;
		byte mapType = 0;
		byte roomDataLength = 0;
		string data = "";
		string[] str;

		ret &= Deserialize (ref roomCount);

		element = new Room[roomCount];

		for (int i = 0; i < roomCount; i++)
		{
			ret &= Deserialize (ref roomNum);
			ret &= Deserialize (ref mapType);
			ret &= Deserialize (ref roomDataLength);
			ret &= Deserialize (out data, (int) roomDataLength);
			str = data.Split('.');

			element [i] = new Room ();
			element [i].roomNum = roomNum;
			element [i].mapType = mapType;
			element [i].hostId = str[0];
			element [i].roomName = str[1];
		}

		return ret;
	}
}
