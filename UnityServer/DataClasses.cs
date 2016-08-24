public class HeaderData  {
	// 헤더 == [2바이트 - 패킷길이][1바이트 - ID]
	public short length; // 패킷의 길이
	public byte id; // 패킷 ID
}

public enum MapType
{
	Basic = 1,
}

public enum ClientPacketId
{
	None = 0,
	Create,
	Delete,
	Login,
	RoomData,
	RoomCreate,
	RoomEnter,
	RoomExit,
	GameStart,
	Logout,
	GameClose,
}

public enum ServerPacketId
{
	None = 0,
	CreateResult,
	DeleteResult,
	LoginResult,
	RoomList,
	RoomCreateResult,
	EnterResult,
	ExitResult,
	GameStartResult,
	LogoutResult,
}

// 클라이언트-To-서버 가입데이터
public struct AccountData
{
	public string Id;
	public string password;
}

public struct ResultData
{
	public string result;
	const string Fail = "fail";
	const string Success = "success";
}

public struct CreateRoomData
{
	public byte mapType;
	public string roomName;
}