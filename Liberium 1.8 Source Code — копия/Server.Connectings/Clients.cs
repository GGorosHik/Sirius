// --- Õ¿◊¿ÀŒ ‘¿…À¿ Clients.cs ---

using System;
using System.Drawing;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Windows.Forms;
using Leb128;
using Server.Connectings.Events;
using Server.Data;
using Server.Helper;
using Server.Messages;

namespace Server.Connectings;

public class Clients
{
    public delegate void Delegate1();

    public delegate void Delegate(long bytes);

    public Delegate Sents;

    public Delegate Recevied;

    private byte[] ClientBuffer { get; set; }

    private int HeaderSize { get; set; }

    private int Offset { get; set; }

    private bool ClientBufferRecevied { get; set; }

    private object SendSync { get; set; }

    private SslStream SslClient { get; set; }

    public LastPing lastPing { get; set; }

    public Socket Tcp { get; private set; }

    public string IP { get; private set; }

    public bool itsConnect { get; private set; }

    public bool Auth { get; private set; }

    public string Hwid { get; set; }

    public string UserMachine { get; set; }

    public object Tag { get; set; }

    public Clients ReportWindow { get; set; }

    public bool HasCompletedHandshake { get; set; }

    public event EventHandler<EventDisconnect> eventDisconnect;

    public Clients(Socket Tcp)
    {
        itsConnect = true;
        this.Tcp = Tcp;
        SslClient = new SslStream(new NetworkStream(Tcp, ownsSocket: true), leaveInnerStreamOpen: false);
        SslClient.BeginAuthenticateAsServer(Certificate.certificate, clientCertificateRequired: false, SslProtocols.Tls, checkCertificateRevocation: false, EndAuthenticate, null);
        IP = this.Tcp.RemoteEndPoint.ToString().Split(':')[0];
        SendSync = new object();
        lastPing = new LastPing(this);
        this.HasCompletedHandshake = false;

        ConsoleLogger.LogClient($"New client connection from {IP} - Starting SSL authentication");
    }

    private void EndAuthenticate(IAsyncResult ar)
    {
        try
        {
            SslClient.EndAuthenticateAsServer(ar);
            Offset = 0;
            HeaderSize = 4;
            ClientBuffer = new byte[HeaderSize];
            Auth = true;
            SocketData.ConnectsPluse();
            SslClient.BeginRead(ClientBuffer, Offset, HeaderSize, ReadData, null);
            ConsoleLogger.LogSuccess($"SSL authentication completed for client {IP}");
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogError($"SSL authentication failed for client {IP}: {ex.Message}");
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if (!itsConnect)
        {
            return;
        }
        if (Auth)
        {
            SocketData.ConnectsMinuse();
        }
        if (this.eventDisconnect != null)
        {
            this.eventDisconnect(this, new EventDisconnect
            {
                clients = this
            });
        }
        itsConnect = false;
        ClientBuffer = null;
        HeaderSize = 0;
        Offset = 0;
        Tcp?.Dispose();
        SslClient?.Dispose();
        lastPing?.Disconnect();
        if (Tag != null && Tag is DataGridViewRow)
        {
            DataGridViewRow row = (DataGridViewRow)Tag;
            DataGridView datagrid = row.DataGridView;
            if (row.Cells.Count > 12)
            {
                Methods.AppendLogs("Client " + IP + " " + UserMachine + " " + Hwid, "Disconnect", Color.Red);
                ConsoleLogger.LogClient($"Client disconnected: {IP} ({UserMachine}) HWID: {Hwid}");
            }
            else
            {
                ConsoleLogger.LogClient($"Client disconnected: {IP}");
            }
            if (datagrid != null)
            {
                datagrid.Invoke((MethodInvoker)delegate
                {
                    datagrid.Rows.Remove(row);
                });
            }
            row.Dispose();
        }
        else
        {
            ConsoleLogger.LogClient($"Client disconnected: {IP}");
        }
        if (ReportWindow != null)
        {
            ReportWindow.Disconnect();
        }
    }

    private void ReadData(IAsyncResult ar)
    {
        if (!itsConnect)
        {
            return;
        }
        try
        {
            int num = SslClient.EndRead(ar);
            if (num > 0)
            {
                if (Recevied != null)
                {
                    Recevied(num);
                }
                SocketData.ReciveData(num);
                HeaderSize -= num;
                Offset += num;
                if (lastPing != null)
                {
                    lastPing.Last();
                }
                if (!ClientBufferRecevied)
                {
                    if (HeaderSize == 0)
                    {
                        HeaderSize = BitConverter.ToInt32(ClientBuffer, 0);
                        if (HeaderSize > 0)
                        {
                            ClientBuffer = new byte[HeaderSize];
                            Offset = 0;
                            ClientBufferRecevied = true;
                        }
                    }
                    else if (HeaderSize < 0)
                    {
                        ConsoleLogger.LogError($"Invalid header size received from {IP}: {HeaderSize}");
                        Disconnect();
                        return;
                    }
                }
                else if (HeaderSize == 0)
                {
                    new Packet().Read(this, ClientBuffer);
                    Offset = 0;
                    HeaderSize = 4;
                    ClientBuffer = new byte[HeaderSize];
                    ClientBufferRecevied = false;
                }
                else if (HeaderSize < 0)
                {
                    ConsoleLogger.LogError($"Invalid data size received from {IP}: {HeaderSize}");
                    Disconnect();
                    return;
                }
                SslClient.BeginRead(ClientBuffer, Offset, HeaderSize, ReadData, null);
            }
            else
            {
                ConsoleLogger.LogWarning($"Client {IP} closed connection");
                Disconnect();
            }
        }
        catch (Exception ex) // ¬Œ“ »«Ã≈Õ≈ÕÕ€… ¡ÀŒ 
        {
            ConsoleLogger.LogError($"Error with client {IP} ({ex.GetType().Name}). Connection was likely closed unexpectedly.");
            Disconnect();
        }
    }

    public void Send(object[] Data)
    {
        Send(LEB128.Write(Data));
    }

    public void Send(byte[] Data)
    {
        if (!itsConnect)
        {
            return;
        }
        lock (SendSync)
        {
            try
            {
                byte[] bytes = BitConverter.GetBytes(Data.Length);
                byte[] array = new byte[4 + Data.Length];
                Array.Copy(bytes, 0, array, 0, bytes.Length);
                Array.Copy(Data, 0, array, 4, Data.Length);
                Tcp.Poll(-1, SelectMode.SelectWrite);
                SslClient.Write(array, 0, array.Length);
                SslClient.Flush();
                if (Sents != null)
                {
                    Sents(array.Length);
                }
                SocketData.SentData(array.Length);

                if (array.Length > 1024)
                {
                    ConsoleLogger.LogNetwork($"Sent {array.Length} bytes to {IP}");
                }
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"Error sending data to {IP}: {ex.Message}");
                Disconnect();
            }
        }
    }

    public void SendChunk(object[] Data)
    {
        Send(LEB128.Write(Data));
    }

    public void SendChunk(byte[] Data)
    {
        if (!itsConnect)
        {
            return;
        }
        lock (SendSync)
        {
            try
            {
                if (Data.Length < 16384)
                {
                    Send(Data);
                    return;
                }
                byte[] bytes = BitConverter.GetBytes(Data.Length);
                byte[] array = new byte[4 + Data.Length];
                Array.Copy(bytes, 0, array, 0, bytes.Length);
                Array.Copy(Data, 0, array, 4, Data.Length);
                using MemoryStream memoryStream = new MemoryStream(array);
                memoryStream.Position = 0L;
                byte[] array2 = new byte[16384];
                int num;
                int totalSent = 0;
                while ((num = memoryStream.Read(array2, 0, array2.Length)) > 0)
                {
                    Tcp.Poll(-1, SelectMode.SelectWrite);
                    SslClient.Write(array2, 0, num);
                    SslClient.Flush();
                    if (Sents != null)
                    {
                        Sents(num);
                    }
                    totalSent += num;
                }
                ConsoleLogger.LogNetwork($"Sent chunked data {totalSent} bytes to {IP}");
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"Error sending chunked data to {IP}: {ex.Message}");
                Disconnect();
            }
        }
    }
}

// ---  ŒÕ≈÷ ‘¿…À¿ Clients.cs ---