using System;
using System.IO;

namespace Server.Helper
{
    public static class PathSanitizer
    {
        /// <summary>
        /// Безопасно преобразует относительный путь в полный путь внутри заданной базовой директории.
        /// </summary>
        /// <param name="untrustedPath">Непроверенный относительный путь (может содержать ../, ..\ и т.д.).</param>
        /// <param name="baseDirectory">Абсолютный путь к безопасной директории, из которой нельзя выходить.</param>
        /// <returns>Полный безопасный путь или null, если обнаружена попытка выхода за пределы базовой директории.</returns>
        public static string SanitizeAndResolvePath(string untrustedPath, string baseDirectory)
        {
            if (string.IsNullOrEmpty(untrustedPath))
            {
                return null;
            }

            // 1. Получаем полный путь к базовой директории, чтобы нормализовать его (например, убрать лишние слэши).
            string normalizedBaseDirectory = Path.GetFullPath(baseDirectory);

            // 2. Комбинируем базовый путь с непроверенным. Path.Combine сам позаботится о разделителях.
            string combinedPath = Path.Combine(normalizedBaseDirectory, untrustedPath);

            // 3. Разрешаем все '..' и '.' для получения канонического пути.
            // Например, "C:\Logs\Users\..\Recovery" станет "C:\Logs\Recovery"
            string fullPath = Path.GetFullPath(combinedPath);

            // 4. ГЛАВНАЯ ПРОВЕРКА: Убеждаемся, что итоговый путь находится внутри нашей базовой директории.
            // Используем OrdinalIgnoreCase для кросс-платформенной совместимости и независимости от регистра.
            if (fullPath.StartsWith(normalizedBaseDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath;
            }

            // Попытка выхода за пределы каталога! Возвращаем null или бросаем исключение.
            return null;
        }
    }
}