using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using Server.Helper;

namespace Server.Connectings;

public class Listner
{
	private Socket Server { get; set; }

	public int port { get; set; }

	public void Stop()
	{
		Server?.Dispose();
		Methods.AppendLogs("Server", "Stop Listner: " + port, Color.Red);
		ConsoleLogger.LogWarning($"Stopped listener on port {port}");
	}

	public Listner(int port)
	{
		this.port = port;
		IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);
		Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		Server.ReceiveBufferSize = 512000;
		Server.SendBufferSize = 512000;
		Server.Bind(localEP);
		Server.Listen(2500);
		Server.BeginAccept(EndAccept, null);
		Methods.AppendLogs("Server", "Start Listner: " + port, Color.Green);
		ConsoleLogger.LogSuccess($"Started listener on port {port} - Ready to accept connections");
	}

	private void EndAccept(IAsyncResult ar)
	{
		try
		{
			Socket clientSocket = Server.EndAccept(ar);
			ConsoleLogger.LogNetwork($"New connection attempt from {clientSocket.RemoteEndPoint}");
			new Clients(clientSocket);
		}
		catch (Exception ex)
		{
			ConsoleLogger.LogError($"Error accepting connection on port {port}: {ex.Message}");
		}
		try
		{
			Server.BeginAccept(EndAccept, null);
		}
		catch (Exception ex)
		{
			ConsoleLogger.LogError($"Error restarting accept on port {port}: {ex.Message}");
		}
	}
}

