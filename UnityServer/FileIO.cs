using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

public class FileIO
{
	FileStream fs;
	public Hashtable playerData;
	BinaryFormatter bin;

	public FileIO ()
	{
		fs = null;
		bin = new BinaryFormatter ();
		playerData = new Hashtable ();
	}

	public FileIO (string dataPath, FileMode fileMode)
	{
		fs = new FileStream (dataPath, fileMode);
		bin = new BinaryFormatter ();
		if (fs.Length != 0)
			playerData = (Hashtable)bin.Deserialize (fs);
		else
			playerData = new Hashtable ();
	}

	//아이디가 이미 있는지 확인하여 계정을 생성해줍니다.
	//아이디가 생성 되었을 때는 true, 아이디가 이미 있을 경우 false를 반환합니다.
	public bool AddPlayerData (string newID, string newPW)
	{
		PlayerData newPlayer = new PlayerData (newID, newPW);
		try 
		{
			playerData.Add (newPlayer.ID, newPlayer);
		}
		catch
		{
			Console.WriteLine ("FileIO::PlayerData.Add 에러");
			return false;
		}

		fs.Close ();

		try
		{
			fs = new FileStream ("DataFile.data", FileMode.Create);
		}
		catch
		{
			Console.WriteLine ("FileIO::FileMode.Create 에러");
			return false;
		}

		try
		{
			bin.Serialize (fs, playerData);
			fs.Close ();
		}
		catch
		{
			Console.WriteLine ("FileIO::Serialize 에러");
			return false;
		}

		try
		{
			fs = new FileStream ("DataFile.data", FileMode.Open);
		}
		catch
		{
			Console.WriteLine ("FileIO::FileMode.Open 에러");
			return false;
		}

		return true;
	}

	public bool RemovePlayerData (string newID, string newPW)
	{
		PlayerData newPlayer = new PlayerData (newID, newPW);

		try
		{
			if(playerData.Contains(newPlayer.ID))
			{
				if(GetPlayerData(newPlayer.ID).PW == newPlayer.PW)
				{
					playerData.Remove (newPlayer.ID);
				}
				else
				{
					Console.WriteLine("비밀번호가 틀렸습니다.");
					return false;
				}
			}
			else
			{
				Console.WriteLine("아이디가 존재하지 않습니다.");
				return false;
			}
		}
		catch
		{
			Console.WriteLine ("FileIO::playerData.Contains 에러");
			return false;
		}

		try
		{
			fs.Close ();
			fs = new FileStream ("DataFile.data", FileMode.Create);
		}
		catch
		{
			Console.WriteLine ("FileIO::RemovePlayerData.FileMode.Create 에러");
			return false;
		}

		try
		{
			bin.Serialize (fs, playerData);
		}
		catch
		{
			Console.WriteLine ("FileIO::RemovePlayerData.Serialize 에러");
			return false;
		}

		try
		{
			fs.Close ();
			fs = new FileStream ("DataFile.data", FileMode.Open);
		}
		catch
		{
			Console.WriteLine ("FileIO::RemovePlayerData.FileMode.Open 에러");
			return false;
		}

		return true;
	}

	public PlayerData GetPlayerData(string ID)
	{
		try
		{
			fs.Position = 0;
			playerData = (Hashtable)bin.Deserialize (fs);
		}
		catch
		{
			Console.WriteLine ("FileIO::GetPlayerData.Deserialize 에러");
			return null;
		}

		try{
			if (playerData.Contains (ID))
			{
				PlayerData player = (PlayerData)playerData [ID];
				return player;
			}
			else
			{
				Console.WriteLine (ID + "는 존재하지 않습니다.");
				return null;
			}
		}
		catch
		{
			Console.WriteLine ("FileIO::GetPlayerData.Contains 에러");
			return null;
		}
	}

	public Hashtable GetAllPlayerData()
	{
		return playerData;
	}

	public void PrintPlayerData(string ID)
	{
		fs.Position = 0;
		playerData = (Hashtable)bin.Deserialize (fs);

		if (playerData.Contains (ID))
		{
			PlayerData player = (PlayerData)playerData [ID];
			Console.WriteLine ("ID : " + player.ID + "\nPW : " + player.PW);
		}
	}

	public void PrintAllPlayerData()
	{
		fs.Position = 0;
		playerData = (Hashtable) bin.Deserialize (fs);

		foreach (DictionaryEntry player in playerData)
		{
			PlayerData newPlayer = (PlayerData) player.Value;
			Console.WriteLine ("ID : " + newPlayer.ID + "\nPW : " + newPlayer.PW);
		}
	}
}


[Serializable]
public class PlayerData
{
	public string ID;
	public string PW;

	public PlayerData ()
	{
		ID = "???";
		PW = "***";
	}

	public PlayerData (string newID, string newPW)
	{
		ID = newID;
		PW = newPW;
	}
}