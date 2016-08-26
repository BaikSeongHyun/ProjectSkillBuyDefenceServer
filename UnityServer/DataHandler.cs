using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

public class DataHandler
{
	public Queue<TcpPacket> receiveMsgs;
	public Queue<TcpPacket> sendMsgs;
	public RoomManager roomManager;
	public FileIO fileIO;

	public Hashtable LoginUser;

	object receiveLock;
	object sendLock;

	TcpPacket tcpPacket;
	byte[] header;
	byte[] paket = new byte[1024];
	byte[] msg;
	byte[] msgLength = new byte[2];
	byte msgType;

	public delegate ServerPacketId RecvNotifier(byte[] data);
	private Dictionary<int, RecvNotifier> m_notifier = new Dictionary<int, RecvNotifier>();

	public DataHandler (Queue<TcpPacket> receiveQueue, Queue<TcpPacket> sendQueue, object newReceiveLock, object newSendLock, Hashtable newHashtable)
	{
		fileIO = new FileIO ("DataFile.data", FileMode.OpenOrCreate);
		roomManager = new RoomManager ();
		receiveMsgs = receiveQueue;
		sendMsgs = sendQueue;
		receiveLock = newReceiveLock;
		sendLock = newSendLock;
		LoginUser = newHashtable;

		m_notifier.Add((int) ClientPacketId.Create, CreateAccount);
		m_notifier.Add((int) ClientPacketId.Delete, DeleteAccount);
		m_notifier.Add((int) ClientPacketId.Login, Login);
		m_notifier.Add((int) ClientPacketId.RoomData, GetRoomList);
		m_notifier.Add((int) ClientPacketId.RoomCreate, CreateRoom);
		m_notifier.Add((int) ClientPacketId.RoomEnter, EnterRoom);
		m_notifier.Add((int) ClientPacketId.RoomExit, ExitRoom);
		m_notifier.Add((int) ClientPacketId.GameStart, GameStart);
		m_notifier.Add((int) ClientPacketId.Logout, Logout);
		m_notifier.Add((int) ClientPacketId.GameClose, GameClose);
	}

	public void DataHandle ()
	{
		if (receiveMsgs.Count != 0)
		{
			//패킷을 Dequeue 한다 패킷 : 메시지 타입 + 메시지 내용
			tcpPacket = receiveMsgs.Dequeue();

			//타입과 내용을 분리한다
			msgType = tcpPacket.msg [0];
			msg = new byte[tcpPacket.msg.Length - 1];
			Array.Copy (tcpPacket.msg, 1, msg, 0, msg.Length);

			//Dictionary에 등록된 델리게이트형 메소드에서 msg를 반환받는다.
			RecvNotifier recvNotifier;
			HeaderSerializer serializer = new HeaderSerializer ();
			HeaderData headerData = new HeaderData ();

			if (m_notifier.TryGetValue (msgType, out recvNotifier))
			{
				//send 할 id를 반환받음
				headerData.id = (byte) recvNotifier (msg);
			}
			else
			{
				Console.WriteLine ("DataHandler::TryGetValue 에러" + msgType);
				headerData.id = (byte) ServerPacketId.None;
			}

			//상대방에게서 게임종료 패캣이 왔을 때는 따로 Send하지 않기 위해서
			if (headerData.id == (byte)ServerPacketId.None)
				return;

			//send할 메시지의 길이를 받음
			headerData.length = (short) msg.Length;

			//헤더 serialize
			try
			{
				serializer.Serialize (headerData);
				header = serializer.GetSerializedData ();
			}
			catch
			{
				Console.WriteLine ("DataHandler::HeaderSerialize 에러");
			}

			//헤더와 메시지 내용을 합쳐서 Send
			paket = CombineByte (header, msg);
			tcpPacket = new TcpPacket (paket, tcpPacket.client);
			lock (sendLock)
			{
				sendMsgs.Enqueue (tcpPacket);
			}
		}
	}

	public ServerPacketId CreateAccount (byte[] data)
	{
		Console.WriteLine (tcpPacket.client.RemoteEndPoint.ToString() + " 가입요청");

		AccountPacket accountPacket = new AccountPacket (data);
		AccountData accountData = accountPacket.GetData ();

		Console.WriteLine ("아이디 : " + accountData.Id + "패스워드 : " + accountData.password);

		try
		{
			if (fileIO.AddPlayerData (accountData.Id, accountData.password))
			{
				msg = Encoding.Unicode.GetBytes ("success");
			}
			else
			{
				msg = Encoding.Unicode.GetBytes ("fail");
			}
		}
		catch
		{
			Console.WriteLine ("DataHandler::AddPlayerData 에러");
			msg = Encoding.Unicode.GetBytes ("fail");
		}

		return ServerPacketId.CreateResult;
	}

	public ServerPacketId DeleteAccount (byte[] data)
	{
		Console.WriteLine (tcpPacket.client.RemoteEndPoint.ToString() + " 탈퇴요청");

		AccountPacket accountPacket = new AccountPacket (data);
		AccountData accountData = accountPacket.GetData ();

		Console.WriteLine ("아이디 : " + accountData.Id + "패스워드 : " + accountData.Id);

		try
		{
			if (fileIO.RemovePlayerData (accountData.Id, accountData.password))
			{
				msg = Encoding.Unicode.GetBytes ("success");
			}
			else
			{
				msg = Encoding.Unicode.GetBytes ("fail");
			}
		}
		catch
		{
			Console.WriteLine ("DataHandler::RemovePlayerData 에러");
			msg = Encoding.Unicode.GetBytes ("fail");
		}

		return ServerPacketId.DeleteResult;
	}

	public ServerPacketId Login (byte[] data)
	{
		Console.WriteLine (tcpPacket.client.RemoteEndPoint.ToString() + " 로그인요청");

		AccountPacket accountPacket = new AccountPacket (data);
		AccountData accountData = accountPacket.GetData ();

		Console.WriteLine ("아이디 : " + accountData.Id + "비밀번호 : " + accountData.password);

		try
		{
			if (fileIO.playerData.Contains (accountData.Id))
			{
				if (fileIO.GetPlayerData (accountData.Id).PW == accountData.password)
				{
					if(!LoginUser.Contains(accountData.Id))
					{
						msg = Encoding.Unicode.GetBytes ("success");
						Console.WriteLine ("로그인 성공");
						((TcpClient) LoginUser[tcpPacket.client]).Id = accountData.Id;
					}
					else
					{
						Console.WriteLine ("현재 접속중인 아이디입니다.");
						msg = Encoding.Unicode.GetBytes ("fail");
					}
				}
				else
				{
					Console.WriteLine ("패스워드가 맞지 않습니다.");
					msg = Encoding.Unicode.GetBytes ("fail");
				}
			}
			else
			{
				Console.WriteLine ("존재하지 않는 아이디입니다.");
				msg = Encoding.Unicode.GetBytes ("fail");
			}
		}
		catch
		{
			Console.WriteLine ("DataHandler::PlayerData.Contains 에러");
			msg = Encoding.Unicode.GetBytes ("fail");
		}

		return ServerPacketId.LoginResult;
	}

	public ServerPacketId GetRoomList(byte[] data)
	{
		Console.WriteLine (tcpPacket.client.RemoteEndPoint.ToString() + " 방목록 조회");

		try
		{
			RoomPacket roomPacket = new RoomPacket (roomManager.GetRoomData());
			msg = roomPacket.GetPacketData ();
		}
		catch
		{
			Console.WriteLine ("DataHandler::AddPlayerData 에러");
		}

		return ServerPacketId.RoomList;
	}

	public ServerPacketId CreateRoom(byte[] data)
	{
		Console.WriteLine (tcpPacket.client.RemoteEndPoint.ToString() + " 방 생성");

		CreateRoomPacket createRoomPacket = new CreateRoomPacket (data);
		CreateRoomData createRoomData = createRoomPacket.GetData ();

		Console.WriteLine ("방 타입 : " + (int) createRoomData.mapType + "방 제목 : " + createRoomData.roomName);

		int index = roomManager.CreateRoom (tcpPacket.client, ((TcpClient)LoginUser [tcpPacket.client]).Id, createRoomData.roomName, (int)createRoomData.mapType);

		if (index != 0)
		{
			Console.WriteLine ("방 생성 성공");
		}
		else
		{
			Console.WriteLine ("방 생성 실패");
		}

		msg[0] = (byte) index;

		return ServerPacketId.RoomCreateResult;
	}

	public ServerPacketId EnterRoom(byte[] data)
	{
		Console.WriteLine (tcpPacket.client.RemoteEndPoint.ToString() + " 방 입장");

		int roomNum = data[0] - 1;
		Socket host = roomManager.EnterRoom (roomNum);

		byte[] result = new byte[1];

		if (host != null)
		{
			result[0] = 1;
			string ip = host.RemoteEndPoint.ToString ();
			Console.WriteLine (ip.Substring(0, ip.IndexOf(":")));

			msg = CombineByte(result, Encoding.Unicode.GetBytes(ip.Substring(0, ip.IndexOf(":"))));

			Console.WriteLine ("방 입장 성공");
		}
		else
		{
			result[0] = 0;
			msg = result;
			Console.WriteLine ("방 입장 실패");
		}

		return ServerPacketId.EnterResult;
	}

	public ServerPacketId ExitRoom(byte[] data)
	{
		Console.WriteLine (tcpPacket.client.RemoteEndPoint.ToString() + " 방 퇴장");

		int roomNum = data[0] - 1;

		if (roomManager.ExitRoom (roomNum))
		{
			if (tcpPacket.client == roomManager.rooms [roomNum].host)
			{
				roomManager.InitializeRoom (roomNum);
			}
			msg = Encoding.Unicode.GetBytes ("success");
			Console.WriteLine ("방 퇴장 성공");
		}
		else
		{
			msg = Encoding.Unicode.GetBytes ("fail");
			Console.WriteLine ("방 퇴장 실패");
		}

		return ServerPacketId.ExitResult;
	}

	public ServerPacketId GameStart(byte[] data)
	{
		Console.WriteLine (tcpPacket.client.RemoteEndPoint.ToString() + " 게임시작");

		int roomNum = BitConverter.ToInt32 (data, 0);

		try
		{
			roomManager.rooms[roomNum].roomState = Room.RoomState.playing;
			msg = Encoding.Unicode.GetBytes ("success");
			Console.WriteLine (roomNum + "번 방 게임 시작");
		}
		catch
		{
			msg = Encoding.Unicode.GetBytes ("fail");
			Console.WriteLine (roomNum + "번 방 게임 시작 실패");
		}

		return ServerPacketId.GameStartResult;
	}

	public ServerPacketId Logout (byte[] data)
	{
		Console.WriteLine (tcpPacket.client.RemoteEndPoint.ToString() + " 로그아웃요청");

		string id = Encoding.Unicode.GetString(data);

		try
		{
			if (LoginUser.Contains (id))
			{
				LoginUser.Remove(id);
				Console.WriteLine(id + "로그아웃");
				msg = Encoding.Unicode.GetBytes ("success");
			}
			else
			{
				Console.WriteLine ("로그인되어있지 않은 아이디입니다.");
				msg = Encoding.Unicode.GetBytes ("fail");
			}
		}
		catch
		{
			Console.WriteLine ("DataHandler::PlayerData.Contains 에러");
			msg = Encoding.Unicode.GetBytes ("fail");
		}

		return ServerPacketId.LogoutResult;
	}

	public ServerPacketId GameClose (byte[] data)
	{
		Console.WriteLine (tcpPacket.client.RemoteEndPoint.ToString() + " 가 게임을 종료했습니다.");

		try
		{
			int roomNum = roomManager.FindRoom(tcpPacket.client);

			if(roomNum != -1)
			{
				roomManager.InitializeRoom(roomNum);
			}

			LoginUser.Remove(tcpPacket.client);
			tcpPacket.client.Close();
		}
		catch
		{
			Console.WriteLine ("DataHandler::LoginUser.Remove 에러");
		}

		return ServerPacketId.None;
	}


	public static byte[] CombineByte (byte[] array1, byte[] array2)
	{
		byte[] array3 = new byte[array1.Length + array2.Length];
		Array.Copy (array1, 0, array3, 0, array1.Length);
		Array.Copy (array2, 0, array3, array1.Length, array2.Length);
		return array3;
	}

	public static byte[] CombineByte (byte[] array1, byte[] array2, byte[] array3)
	{
		byte[] array4 = CombineByte (CombineByte (array1, array2), array3);;
		return array4;
	}
}

[Serializable]
public class TcpClient
{
	public Socket client;
	public string Id;

	public TcpClient (Socket newClient)
	{
		client = newClient;
		Id = "";
	}
}