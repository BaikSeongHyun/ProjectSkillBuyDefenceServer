using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

public class RoomManager
{
	public const int maxRoomNum = 20;
	public Room[] rooms;
	private int roomCount;

	public int RoomCount
	{
		get
		{
			int count = 0;

			for (int i = 0; i < maxRoomNum; i++)
			{
				if (rooms [i].userNum != 0)
					count++;
			}

			return count;
		}
		set { roomCount = value; }
	}

	public RoomManager ()
	{
		rooms = new Room[maxRoomNum];

		for (int i = 0; i < maxRoomNum; i++)
		{
			rooms [i] = new Room ();
			rooms [i].roomNum = i + 1;
		}

		RoomCount = 0;
	}

	public void InitializeRoom (int index)
	{
		rooms [index].roomName = "";
		rooms [index].host = null;
		rooms [index].hostId = "";
		rooms [index].mapType = 0;
		rooms [index].roomState = Room.RoomState.empty;
		rooms [index].userNum = 0;
	}

	public int FindEmptyRoom ()
	{
		for (int i = 0; i < maxRoomNum; i++)
		{
			if (rooms [i].roomState == Room.RoomState.empty)
			{
				return i;
			}
		}
		return -1;
	}

	//호스트의 소켓과 방 이름과 새로 추가할 유저이름이 필요하다
	public int CreateRoom(Socket newHost, string newHostId, string newRoomName, int newMapType)
	{
		//빈방을 찾음
		if (RoomCount != maxRoomNum)
		{
			int index = FindEmptyRoom ();

			if(index != -1)
				rooms [index].SetRoom (newHost, newHostId, newRoomName, newMapType, Room.RoomState.waiting);

			return index + 1;
		}
		else
		{
			Console.WriteLine ("방을 더이상 생성할 수 없습니다.");
			return 0;
		}
	}

	//방을 입장한 유저에게 방 호스트의 소켓을 보내주기 위해 호스트 소켓을 반환한다
	public Socket EnterRoom(int index)
	{
		if (rooms [index].roomState == Room.RoomState.waiting)
		{
			if (rooms [index].userNum < Room.maxUserNum)
			{
				rooms [index].userNum++;
				return rooms [index].host;
			}
			else
			{
				Console.WriteLine ("방이 가득찼습니다.");
				return null;
			}
		}
		else
		{
			Console.WriteLine ("이미 시작한 방입니다.");
			return null;
		}
	}

	public bool ExitRoom(int index)
	{
		try
		{
			Console.WriteLine((index+1) + "번방 인원수 : " + rooms[index].userNum);
			rooms [index].userNum--;

			Console.WriteLine((index+1) + "번방 인원수 : " + rooms[index].userNum);

			if (rooms [index].userNum == 0)
			{
				InitializeRoom (index);
			}

			return true;
		}
		catch
		{
			Console.WriteLine ("RoomManager::ExitRoom 에러");
			return false;
		}
	}

	public int FindRoom(Socket newHost)
	{
		for (int i = 0; i < maxRoomNum; i++)
		{
			if (rooms [i].host == newHost)
			{
				return i;
			}
		}
		return -1;
	}

	//방 정보 출력
	public Room[] GetRoomData()
	{
		List<Room> roomData = new List<Room>();

		for (int i = 0; i < maxRoomNum; i++)
		{
			if (rooms [i].roomState == Room.RoomState.waiting)
			{
				roomData.Add (rooms [i]);
			}
		}

		return roomData.ToArray();
	}
}

public class Room
{
	public enum RoomState
	{
		empty,
		waiting,
		playing,
	}

	public int roomNum;
	public string roomName;

	public Socket host;
	public string hostId;

	public int mapType;

	public RoomState roomState;

	public int userNum;
	public const int maxUserNum = 4;

	public void SetRoom (Socket newHost, string newHostId, string newRoomName, int newMapType, RoomState newRoomState)
	{
		roomName = newRoomName;
		host = newHost;
		hostId = newHostId;
		mapType = newMapType;
		roomState = newRoomState;
		userNum = 1;
	}
}
