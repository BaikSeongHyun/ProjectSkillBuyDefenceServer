using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;

public class UnityServer
{
	public const short packetType = 1;
	public const short packetLength = 2;

	public static void Main (string[] args)
	{
		Queue<TcpPacket> inData = new Queue<TcpPacket> ();
		Queue<TcpPacket> outData = new Queue<TcpPacket> ();

		object receiveLock = new object ();
		object sendLock = new object ();

		Hashtable LoginUser = new Hashtable ();

		TcpServer TcpServer = new TcpServer (inData, IPAddress.Parse ("192.168.94.88"), 9800, receiveLock, LoginUser);
		DataHandler dataHandler = new DataHandler (inData, outData, receiveLock, sendLock, LoginUser);
		DataSender dataSender = new DataSender (outData, sendLock);

		while (true)
		{
			if (Console.KeyAvailable)
			{
				string message = Console.ReadLine ();

				if (message == "Print PlayerData" || message == "print playerdata")
				{
					dataHandler.fileIO.PrintAllPlayerData ();
				}
			}

			dataHandler.DataHandle ();
			dataSender.DataSend ();
//			TcpServer.CheckClients ();
		}
	}
}
