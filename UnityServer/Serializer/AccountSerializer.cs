using System.Collections;

public class AccountSerializer : Serializer {

	public bool Serialize(AccountData data)
	{
		bool ret = true;
		ret &= Serialize(data.Id);
		ret &= Serialize(".");
		ret &= Serialize(data.password);
		return ret;
	}

	public bool Deserialize(ref AccountData element)
	{
		if (GetDataSize() == 0)
		{
			// 데이터가 설정되지 않았다.
			return false;
		}

		bool ret = true;
		string total;
		ret &= Deserialize(out total, (int)GetDataSize());

		string[] str = total.Split('.');
		if (str.Length < 2)
		{
			return false;
		}

		element.Id = str[0];
		element.password = str[1];

		return ret;
	}
}
