﻿using System.Collections;
using System;

public class AccountPacket : IPacket<AccountData>
{
	AccountData m_data;

	public AccountPacket(AccountData data) // 데이터로 초기화(송신용)
	{
		m_data = data;
	}

	public AccountPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
	{
		AccountSerializer serializer = new AccountSerializer();
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_data);
	}

	public byte[] GetPacketData() // 바이트형 패킷(송신용)
	{
		AccountSerializer serializer = new AccountSerializer();
		serializer.Serialize(m_data);
		return serializer.GetSerializedData();
	}

	public AccountData GetData() // 데이터 얻기(수신용)
	{
		return m_data;
	}

	public int GetPacketId()
	{
		return (int) ClientPacketId.Create;
	}
}
