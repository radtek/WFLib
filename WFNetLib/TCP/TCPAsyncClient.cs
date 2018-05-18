﻿// using System;
// using System.Collections.Generic;
// 
// using System.Text;
// using System.Net;
// using System.Net.Sockets;
// using System.IO;
// using System.Runtime.InteropServices;
// using WFNetLib.PacketProc;
// using System.Threading;
// 
// namespace WFNetLib.TCP
// {
// 	public class TCPAsyncClient
// 	{
// 		public string TCPServerName = "127.0.0.1";
// 		public int TCPServerPort = 8001;
// 		public int myPort = -1;
// 		public string myIP = "";
// 		private ClientContext clientContext;
// 		public SaveDataProcessCallbackDelegate SaveDataProcessCallback = null;
// 		bool isCloseMyself;
// 		public Exception LastException;
// 		public TCPAsyncClient()
// 		{			
// 			if (string.IsNullOrEmpty(myIP))
// 				myIP = "127.0.0.1";
// 			if (string.IsNullOrEmpty(TCPServerName))
// 				TCPServerName = "127.0.0.1";
// 		}
// 		void IO_Completed(object sender, SocketAsyncEventArgs asyncEventArgs)
// 		{
// 			clientContext.ActiveDateTime = DateTime.Now;
// 			try
// 			{
// 				lock (clientContext)
// 				{
// 					if (asyncEventArgs.LastOperation == SocketAsyncOperation.Receive)
// 						ProcessReceive(asyncEventArgs);
// 					else if (asyncEventArgs.LastOperation == SocketAsyncOperation.Send)
// 						ProcessSend(asyncEventArgs);
// 					else
// 						throw new ArgumentException("The last operation completed on the socket was not a receive or send");
// 				}
// 			}
// 			catch (Exception e)
// 			{
// 				//Exception ex = new Exception("IO_completed:" + e.Message, e);
// 				OnErrorClientEvent(new ErrorServerEventArgs(e, null, TCPErrorType.Unkown, "IO_completed:"));
// 				//                 Program.Logger.ErrorFormat("IO_Completed {0} error, message: {1}", userToken.ConnectSocket, E.Message);
// 				//                 Program.Logger.Error(E.StackTrace);
// 			}
// 		}
// // 		public IAsyncResult SendIAsyncResult
// // 		{
// // 			get { return clientContext.SendIAsyncResult; }
// // 		}
// // 		public IAsyncResult ReceiveIAsyncResult
// // 		{
// // 			get { return clientContext.ReceiveIAsyncResult; }
// // 		}
// 		///
// 		/// 与服务器断开连接
// 		///
// 		public void Close()
// 		{
// 			try
// 			{
// 				isCloseMyself = true;
// 				clientContext.ClientSocket.Shutdown(SocketShutdown.Both);
// 				clientContext.ClientSocket.Close();
// 			}
// 			catch (SystemException ex)
// 			{
// 				OnErrorClientEvent(new ErrorServerEventArgs(ex, TCPErrorType.Unkown));
// 				LastException = ex;
// 			}
// 
// 		}
// 		///
// 		/// 连接服务器
// 		///
// 		public Boolean Conn()
// 		{
// 			TcpClient
// 			try
// 			{
// 				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
// 				if (myPort > 0)
// 				{
// 					IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(myIP), myPort);
// 					socket.Bind(localEndPoint);
// 				}
// 				IPAddress ipAddress = IPAddress.Parse(TCPServerName);
// 				IPEndPoint remoteEP = new IPEndPoint(ipAddress, TCPServerPort);
// 				socket.Connect(remoteEP);
// 				clientContext = new ClientContext(socket);
// 				clientContext.ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
// 				clientContext.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
// 				bool willRaiseEvent = clientContext.ClientSocket.ReceiveAsync(clientContext.ReceiveEventArgs); //投递接收请求
// 				if (!willRaiseEvent)
// 				{
// 					ProcessReceive(clientContext.ReceiveEventArgs);
// 				}				
// 				return true;
// 			}
// 			catch (SocketException e)
// 			{
// 				LastException = e;
// 				switch (e.ErrorCode)
// 				{
// 					case 10061:
// 						OnErrorClientEvent(new ErrorServerEventArgs(e, TCPErrorType.CannotConnect));
// 						break;
// 					case 10060:
// 						OnErrorClientEvent(new ErrorServerEventArgs(e, TCPErrorType.CannotConnect));
// 						break;
// 				}
// 			}
// 			catch (Exception e)
// 			{
// 				LastException = e;
// 				OnErrorClientEvent(new ErrorServerEventArgs(e, TCPErrorType.Unkown));
// 			}
// 			return false;
// 		}
// 		private void ProcessReceive(SocketAsyncEventArgs receiveEventArgs)
// 		{
// 			ClientContext client = receiveEventArgs.UserToken as ClientContext;
// 			if (client.ClientSocket == null)
// 				return;
// 			if (client.SaveDataProcessCallback == null && SaveDataProcessCallback == null)
// 				throw (new Exception("没有定义数据的处理回调!"));
// 
// 			client.ActiveDateTime = DateTime.Now;
// 			client.RxDateTime = DateTime.Now;
// 			if (client.ReceiveEventArgs.BytesTransferred > 0 && client.ReceiveEventArgs.SocketError == SocketError.Success)
// 			{
// 				//                 int offset = client.ReceiveEventArgs.Offset;
// 				//                 int count = client.ReceiveEventArgs.BytesTransferred;
// 				//                 while(true)
// 				//                 {
// 				object o = null;
// 				if (client.SaveDataProcessCallback != null)
// 					o = client.SaveDataProcessCallback(client.ReceiveEventArgs.Buffer, ref client.netDataBuffer, ref client.netDataOffset, client.ReceiveEventArgs.BytesTransferred);
// 				else
// 					o = SaveDataProcessCallback(client.ReceiveEventArgs.Buffer, ref client.netDataBuffer, ref client.netDataOffset, client.ReceiveEventArgs.BytesTransferred);
// 				if (o != null)
// 				{
// 					OnReceiveClientEvent(new ReceiveServerEventArgs(o, client));
// 				}
// 				//                     else
// 				//                         break;
// 				//                 }                    
// 				bool willRaiseEvent = client.ClientSocket.ReceiveAsync(client.ReceiveEventArgs); //投递接收请求
// 				if (!willRaiseEvent)
// 					ProcessReceive(client.ReceiveEventArgs);
// 			}
// 			else
// 			{
// 				Close();
// 			}
// 		}
// 		private bool ProcessSend(SocketAsyncEventArgs sendEventArgs)
// 		{
// 			ClientContext client = sendEventArgs.UserToken as ClientContext;
// 			client.ActiveDateTime = DateTime.Now;
// 			if (sendEventArgs.SocketError == SocketError.Success)
// 			{
// 				lock (client)
// 				{
// 					OnSendCompleteServerEvent(new SendCompleteEventArgs(client, client.txBytes));
// 				}
// 				return true;
// 			}
// 			//return client.AsyncSocketInvokeElement.SendCompleted(); //调用子类回调函数
// 			else
// 			{
// 				CloseClientSocket(client);
// 				return false;
// 			}
// 		}
// 		///
// 		/// 连接服务器
// 		///
// 		public Boolean Conn(int timeoutMSec)
// 		{
// 			try
// 			{
// 				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
// 				if (myPort > 0)
// 				{
// 					IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(myIP), myPort);
// 					socket.Bind(localEndPoint);
// 				}
// 				IPAddress ipAddress = IPAddress.Parse(TCPServerName);
// 				IPEndPoint remoteEP = new IPEndPoint(ipAddress, TCPServerPort);
// 				IAsyncResult concAsync = socket.BeginConnect(remoteEP, null, null);
// 
// 				if (concAsync.AsyncWaitHandle.WaitOne(timeoutMSec))
// 				{
// 					socket.EndConnect(concAsync);
// 					clientContext = new ClientContext(socket);
// 					clientContext.ReceiveIAsyncResult = clientContext.netStream.BeginRead(clientContext.tempbuffer, 0, ClientContext.BUFFER_SIZE, new AsyncCallback(AsyncCallbackReadFromNetStream), clientContext);
// 					return true;
// 				}
// 				else
// 				{
// 					socket.Close();
// 					throw new TimeoutException("连接服务器超时");
// 				}
// 			}
// 			catch (Exception e)
// 			{
// 				OnErrorClientEvent(new ErrorServerEventArgs(e, TCPErrorType.Unkown));
// 				LastException = e;
// 				return false;
// 			}
// 		}
// 		/// <summary>
// 		/// 从网络流异步读取数据回调函数
// 		/// </summary>
// 		/// <param name="result"></param>
// 		private void AsyncCallbackReadFromNetStream(IAsyncResult result)
// 		{
// 			try
// 			{
// 				ClientContext Client = (ClientContext)result.AsyncState;
// 				int readLen = Client.netStream.EndRead(result);
// 				if (readLen == 0)
// 				{
// 					if (Client.ClientSocket.Poll(-1, SelectMode.SelectRead))
// 					{
// 						OnDisconnectClientEvent(new DisconnectEventArgs(clientContext));
// 					}
// 				}
// 				else
// 				{
// 					Client.UpdateTime = DateTime.Now;
// 					object o = SaveDataProcessCallback(ref Client.tempbuffer, ref Client.netDataBuffer, ref Client.netDataOffset, readLen);
// 					if (o != null)
// 						OnReceiveClientEvent(new ReceiveServerEventArgs(o, Client));
// 					clientContext.ReceiveIAsyncResult = Client.netStream.BeginRead(Client.tempbuffer, 0, ClientContext.BUFFER_SIZE, new AsyncCallback(AsyncCallbackReadFromNetStream), Client);
// 				}
// 			}
// 			catch (IOException e)
// 			{
// 				if (e.InnerException.GetType() == typeof(SocketException))
// 				{
// 					SocketException ex = (SocketException)e.InnerException;
// 					switch (ex.ErrorCode)
// 					{
// 						case 10054://客户端断开连接
// 							if (isCloseMyself)//自己主动断开连接
// 							{
// 								isCloseMyself = false;
// 							}
// 							else
// 								OnDisconnectClientEvent(new DisconnectEventArgs(clientContext));
// 							break;
// 						default:
// 							OnErrorClientEvent(new ErrorServerEventArgs(e, TCPErrorType.Unkown));
// 							break;
// 					}
// 				}
// 				LastException = e;
// 			}
// 			catch (System.Exception ex)
// 			{
// 				LastException = ex;
// 				OnErrorClientEvent(new ErrorServerEventArgs(ex, TCPErrorType.Unkown));
// 			}
// 		}
// 		public bool Send(MemoryStream mStream)
// 		{
// 			return Send(mStream.GetBuffer());
// 		}
// 		public bool Send(byte[] txBytes)
// 		{
// 			try
// 			{
// 				if (txBytes == null || txBytes.Length == 0)
// 				{
// 					throw (new Exception("不可以发送空信息!"));
// 				}
// 				BeforeSendPacketEventArgs bea = new BeforeSendPacketEventArgs(clientContext, txBytes);
// 				OnBeforeSendPacketClientEvent(bea);
// 				if (bea.isCancel)
// 					return false;
// 				clientContext.SendIAsyncResult = clientContext.netStream.BeginWrite(txBytes, 0, (Int32)txBytes.Length, new AsyncCallback(AsyncCallbackWriteToNetStream), new ClientAndPacket(clientContext, txBytes));
// 
// 			}
// 			catch (Exception e)
// 			{
// 				OnErrorClientEvent(new ErrorServerEventArgs(e, clientContext, TCPErrorType.Unkown));
// 				LastException = e;
// 				return false;
// 			}
// 			return true;
// 		}
// 		/// <summary>
// 		/// 写入网络流异步回调函数
// 		/// </summary>
// 		/// <param name="result"></param>
// 		private void AsyncCallbackWriteToNetStream(IAsyncResult result)
// 		{
// 			try
// 			{
// 				ClientAndPacket p = (ClientAndPacket)result.AsyncState;
// 				p.Client.netStream.EndWrite(result);
// 				OnSendCompleteClientEvent(new SendCompleteEventArgs(p.Client, p.txBytes));
// 			}
// 			catch (ObjectDisposedException e)
// 			{
// 				OnErrorClientEvent(new ErrorServerEventArgs(e, TCPErrorType.NoConnect));
// 				LastException = e;
// 			}
// 			catch (IOException e)
// 			{
// 				if (e.InnerException.GetType() == typeof(SocketException))
// 				{
// 					SocketException ex = (SocketException)e.InnerException;
// 					ClientContext Client = (ClientContext)result.AsyncState;
// 					switch (ex.ErrorCode)
// 					{
// 						case 10054://客户端断开连接                            
// 							OnDisconnectClientEvent(new DisconnectEventArgs((ClientContext)result.AsyncState));
// 							break;
// 						default:
// 							OnErrorClientEvent(new ErrorServerEventArgs(e, TCPErrorType.Unkown));
// 							break;
// 					}
// 				}
// 				else if (e.InnerException.GetType() == typeof(ObjectDisposedException))
// 				{
// 					OnErrorClientEvent(new ErrorServerEventArgs(e, TCPErrorType.SendBreak));
// 				}
// 				LastException = e;
// 			}
// 			catch (SystemException e)
// 			{
// 				OnErrorClientEvent(new ErrorServerEventArgs(e, TCPErrorType.Unkown));
// 				LastException = e;
// 			}
// 		}
// 		///
// 		/// 引发接收事件
// 		///
// 		/// 数据
// 		protected virtual void OnReceiveClientEvent(ReceiveServerEventArgs e)
// 		{
// 			if (ReceiveClientEvent != null)
// 			{
// 				ReceiveClientEvent(this, e);
// 			}
// 		}
// 		///
// 		/// 引发错误事件
// 		///
// 		/// 数据
// 		protected virtual void OnErrorClientEvent(ErrorServerEventArgs e)
// 		{
// 			if (ErrorClientEvent != null)
// 			{
// 				ErrorClientEvent(this, e);
// 			}
// 		}
// 		///
// 		/// 引发发送前事件
// 		///
// 		/// 数据
// 		protected virtual void OnBeforeSendPacketClientEvent(BeforeSendPacketEventArgs e)
// 		{
// 			if (BeforeSendPacketClientEvent != null)
// 			{
// 				BeforeSendPacketClientEvent(this, e);
// 			}
// 		}
// 		///
// 		/// 引发发送完成事件
// 		///
// 		/// 数据
// 		protected virtual void OnSendCompleteClientEvent(SendCompleteEventArgs e)
// 		{
// 			if (SendCompleteClientEvent != null)
// 			{
// 				SendCompleteClientEvent(this, e);
// 			}
// 		}
// 		///
// 		/// 引发离线事件
// 		///
// 		/// 数据
// 		protected virtual void OnDisconnectClientEvent(DisconnectEventArgs e)
// 		{
// 			if (DisconnectClientEvent != null)
// 			{
// 				DisconnectClientEvent(this, e);
// 			}
// 		}
// 
// 		///
// 		/// 接收到数据事件
// 		///
// 		public event TCPReceiveEvent ReceiveClientEvent;
// 		///
// 		/// 发生错误事件
// 		///
// 		public event TCPErrorEvent ErrorClientEvent;
// 		///
// 		/// 发生发送前事件
// 		///
// 		public event TCPBeforeSendPacketEvent BeforeSendPacketClientEvent;
// 		///
// 		/// 发生发送完成事件
// 		///
// 		public event TCPSendCompleteEvent SendCompleteClientEvent;
// 		///
// 		/// 发生离线事件
// 		///
// 		public event TCPDisconnectEvent DisconnectClientEvent;
// 	}
// }
