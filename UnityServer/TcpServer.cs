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

	AsyncCallback asyncReceiveLengthCallBack;
	AsyncCallback asyncReceiveDataCallBack;

	public TcpServer (Queue<TcpPacket> newQueue, IPAddress newAddress, int newPort, object newLock, Hashtable newHashtable)
	{
		listenSock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		listenSock.Bind (new IPEndPoint (newAddress, newPort));
		listenSock.Listen (10);

		LoginUser = newHashtable;
		msgs = newQueue;
		receiveLock = newLock;

		AsyncCallback asyncAcceptCallback = new AsyncCallback (HandleAsyncAccept);
		asyncReceiveLengthCallBack = new AsyncCallback (HandleAsyncReceiveLength);
		asyncReceiveDataCallBack = new AsyncCallback (HandleAsyncReceiveData);

		listenSock.BeginAccept (asyncAcceptCallback, (Object)listenSock);
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

		listenSock.BeginAccept (asyncAcceptCallback, ob1);
		clientSock.BeginReceive (asyncData.msg, 0, UnityServer.packetLength, SocketFlags.None, asyncReceiveLengthCallBack, (Object)asyncData);
	}

	public void HandleAsyncReceiveLength(IAsyncResult asyncResult)
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
			int msgSize = BitConverter.ToInt16 (asyncData.msg, 0);
			asyncData = new AsyncData(clientSock);
			clientSock.BeginReceive (asyncData.msg, 0, msgSize + UnityServer.packetType, SocketFlags.None, asyncReceiveDataCallBack, (Object)asyncData);
		}
		else
		{
			asyncData = new AsyncData(clientSock);
			clientSock.BeginReceive (asyncData.msg, 0, UnityServer.packetLength, SocketFlags.None, asyncReceiveLengthCallBack, (Object)asyncData);
		}
	}

	public void HandleAsyncReceiveData(IAsyncResult asyncResult)
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
			Array.Resize (ref asyncData.msg, asyncData.msgSize);
			TcpPacket paket = new TcpPacket (asyncData.msg, clientSock);

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

		asyncData = new AsyncData(clientSock);
		clientSock.BeginReceive (asyncData.msg, 0, UnityServer.packetLength, SocketFlags.None, asyncReceiveLengthCallBack, (Object)asyncData);
	}
}


public class AsyncData
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