using System.Collections;
using System;

public class CreateRoomSerializer : Serializer {

	public bool Serialize(CreateRoomData data)
	{
		bool ret = true;
		ret &= Serialize(data.mapType);
		ret &= Serialize(data.roomName);
		return ret;
	}

	public bool Deserialize(ref CreateRoomData element)
	{
		if (GetDataSize() == 0)
		{
			// 데이터가 설정되지 않았다.
			return false;
		}

		bool ret = true;
		byte mapType = 0;
		string total;

		ret &= Deserialize (ref mapType);
		ret &= Deserialize(out total, (int) GetDataSize() - 1);

		element.mapType = mapType;
		element.roomName = total;

		return ret;
	}
}
