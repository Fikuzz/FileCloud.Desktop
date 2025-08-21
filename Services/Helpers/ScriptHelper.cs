using MoonSharp.Interpreter;
using System;
using System.IO;

namespace FileCloud.Desktop.Helpers
{
    public static class ScriptHelper
    {
        public static string Rename(string fileName)
        {
            string renameScriptPath = Path.Combine(ScriptManager.ScriptsFolder, "rename.lua");

            if (!File.Exists(renameScriptPath))
            {
                Console.WriteLine("Не удалось найти файл скрипта: rename.lua");
                return fileName;
            }

            try
            {
                Script script = new Script();

                // Загружаем скрипт
                script.DoFile(renameScriptPath);

                // Ищем функцию rename_file
                DynValue function = script.Globals.Get("rename_file");
                if (function.Type != DataType.Function)
                    return fileName;

                // Вызываем функцию, передавая имя файла аргументом
                DynValue result = script.Call(function, DynValue.NewString(fileName));

                if (result.Type == DataType.String && !string.IsNullOrWhiteSpace(result.String))
                {
                    return result.String;
                }
            }
            catch (InterpreterException ex)
            {
                // Ошибка в Lua-скрипте
                Console.WriteLine($"Lua error: {ex.DecoratedMessage}");
            }
            catch (Exception ex)
            {
                // Любая другая ошибка
                Console.WriteLine($"Error executing rename script: {ex.Message}");
            }

            return fileName;
        }
    }
}
