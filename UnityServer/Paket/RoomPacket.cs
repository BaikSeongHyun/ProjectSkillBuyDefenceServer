using System.Collections;
using System;

public class RoomPacket : IPacket<Room[]>
{
	Room[] m_data;

	public RoomPacket(Room[] data) // 데이터로 초기화(송신용)
	{
		m_data = data;
	}

	public RoomPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
	{
		RoomSerializer serializer = new RoomSerializer();
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_data);
	}

	public byte[] GetPacketData() // 바이트형 패킷(송신용)
	{
		RoomSerializer serializer = new RoomSerializer();
		serializer.Serialize(m_data);
		return serializer.GetSerializedData();
	}

	public Room[] GetData() // 데이터 얻기(수신용)
	{
		return m_data;
	}

	public int GetPacketId()
	{
		return (int) ServerPacketId.RoomList;
	}
}
