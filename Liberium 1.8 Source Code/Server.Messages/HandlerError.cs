using System;
using System.Drawing;
using Server.Connectings;
using Server.Helper;
using System.Collections.Concurrent;
using System.Threading;

namespace Server.Messages;

internal class HandlerError
{
    private static ConcurrentDictionary<string, int> clientErrorCounts = new ConcurrentDictionary<string, int>();
    private const int MAX_ERRORS_PER_CLIENT_PER_MINUTE = 50; // Allow 50 error messages per client per minute

    static HandlerError()
    {
        // Start a timer to periodically reset error counts for all clients
        // This timer runs every minute to reset the counts, implementing a sliding window for rate limiting.
        Timer timer = new Timer((e) =>
        {
            // Decrement counts for all clients, remove if zero or less
            foreach (var entry in clientErrorCounts.ToArray())
            {
                clientErrorCounts.AddOrUpdate(entry.Key, 0, (key, count) => Math.Max(0, count - (MAX_ERRORS_PER_CLIENT_PER_MINUTE / 60))); // Adjust decrement based on interval
                if (clientErrorCounts[entry.Key] <= 0)
                {
                    clientErrorCounts.TryRemove(entry.Key, out _);
                }
            }
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)); // Run every second for more granular control
    }

	public static void Read(Clients client, object[] objects)
	{
        string clientHwid = client.Hwid; // Use HWID for client identification

        // Increment error count for the client
        clientErrorCounts.AddOrUpdate(clientHwid, 1, (key, count) => count + 1);

        // Check if client exceeded error message limit
        if (clientErrorCounts[clientHwid] > MAX_ERRORS_PER_CLIENT_PER_MINUTE)
        {
            // Instead of disconnecting, just ignore the excessive error messages
            Methods.AppendLogs(client.IP, $"Ignoring excessive error messages from {client.IP} ({clientHwid}). Rate limit exceeded.", Color.Orange);
            return; // Do not process this error message further
        }

        string errorMessage = (string)objects[1];
        // Sanitize and truncate error message to prevent excessively long logs
        if (errorMessage.Length > 500) // Limit message length to 500 characters
        {
            errorMessage = errorMessage.Substring(0, 500) + "...";
        }

		Console.WriteLine("Error: " + errorMessage);
		Methods.AppendLogs(client.IP, "Error: " + errorMessage, Color.Red);
	}
}


