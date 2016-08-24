using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

	public class DataSender
	{
		Queue<TcpPacket> msgs;
		TcpPacket tcpPacket;
		Socket client;
		byte[] msg;

		object sendLock;

		public DataSender (Queue<TcpPacket> newQueue, object newSendLock)
		{
			msgs = newQueue;
			sendLock = newSendLock;
		}

		public void DataSend()
		{
			if (msgs.Count != 0)
			{
				lock (sendLock)
				{
					tcpPacket = msgs.Dequeue ();
				}

				client = tcpPacket.client;
				msg = tcpPacket.msg;



				client.Send (msg);
			}
		}
	}
