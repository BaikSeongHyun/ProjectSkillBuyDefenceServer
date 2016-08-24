using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class TcpServer
{
	public Socket listenSock;
	public Hashtable LoginUser;
	Queue<TcpPacket> msgs;

	object receiveLock;

	public TcpServer (Queue<TcpPacket> newQueue, IPAddress newAddress, int newPort, object newLock, Hashtable newHashtable)
	{
		listenSock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		listenSock.Bind (new IPEndPoint (newAddress, newPort));
		listenSock.Listen (10);

		LoginUser = newHashtable;
		msgs = newQueue;
		receiveLock = newLock;

		AsyncCallback asyncAcceptCallback = new AsyncCallback (HandleAsyncAccept);
		Object ob1 = (Object)listenSock;
		listenSock.BeginAccept (asyncAcceptCallback, ob1);
	}

	public void HandleAsyncAccept(IAsyncResult asyncResult)
	{
		Socket listenSock = (Socket) asyncResult.AsyncState;
		Socket clientSock = listenSock.EndAccept (asyncResult);

		TcpClient tcpClient = new TcpClient (clientSock);
		LoginUser.Add (clientSock, tcpClient);

		Console.WriteLine("접속 아이피 : " + clientSock.RemoteEndPoint.ToString());

		AsyncCallback asyncAcceptCallback = new AsyncCallback (HandleAsyncAccept);
		Object ob1 = (Object)listenSock;

		AsyncData asyncData = new AsyncData (clientSock);
		AsyncCallback asyncReceiveCallBack = new AsyncCallback (HandleAsyncReceive);
		Object ob2 = (Object)asyncData;

		listenSock.BeginAccept (asyncAcceptCallback, ob1);
		clientSock.BeginReceive (asyncData.msg, 0, AsyncData.msgMaxLength, SocketFlags.None, asyncReceiveCallBack, ob2);
	}

	public void HandleAsyncReceive(IAsyncResult asyncResult)
	{
		AsyncData asyncData = (AsyncData) asyncResult.AsyncState;
		Socket clientSock = asyncData.clientSock;

		try
		{
			asyncData.msgSize = (short) clientSock.EndReceive (asyncResult);
		}
		catch
		{
			Console.WriteLine ("클라이언트가 접속을 종료했습니다.");
			LoginUser.Remove (clientSock);
			clientSock.Close ();
			return;
		}

		if (asyncData.msgSize > 0)
		{
			HeaderData header = new HeaderData();
			HeaderSerializer serializer = new HeaderSerializer();

			serializer.SetDeserializedData(asyncData.msg);
			serializer.Deserialize (ref header);

			byte[] data = new byte[header.length + UnityServer.packetType];
			Array.Copy (asyncData.msg, UnityServer.packetLength, data, 0, header.length + UnityServer.packetType);

			TcpPacket paket = new TcpPacket (data, clientSock);

			lock (receiveLock)
			{
				try
				{
					msgs.Enqueue (paket);
				}
				catch (Exception e)
				{
					Console.WriteLine (e.Message);
				}
			}
		}

		AsyncCallback asyncReceiveCallBack = new AsyncCallback (HandleAsyncReceive);
		Object ob2 = (Object)asyncData;

		clientSock.BeginReceive (asyncData.msg, 0, AsyncData.msgMaxLength, SocketFlags.None, asyncReceiveCallBack, ob2);
	}
}


class AsyncData
{
	public Socket clientSock;
	public byte[] msg;
	public short msgSize;
	public const int msgMaxLength = 1024;

	public AsyncData (Socket newClient)
	{
		msg = new byte[msgMaxLength];
		clientSock = newClient;
	}
}

public class TcpPacket
{
	public byte[] msg;
	public Socket client;

	public TcpPacket (byte[] newMsg, Socket newclient)
	{
		msg = newMsg;
		client = newclient;
	}
}