using System;
using System.IO;
using System.Collections.Generic;

namespace PasswordKeeper.Core
{
    internal static class FileManager
    {
        // Количество всех существовавших файлов сохранений.
        public static int NumOfFiles { get; private set; } = 0;
        // Номер текущего файла сохранений.
        public static KeyValuePair<string, string> CurrentFilePair { get; private set; } = new KeyValuePair<string, string>();
        // Индексы "unnamed" файлов.
        public static List<int> UnnamedFilesIdx { get; private set; } = new List<int>();
        // Список псевдонимов файлов сохранений.
        public static Dictionary<string, string> FileNamesAndAliases { get; private set; } = new Dictionary<string, string>();

        // Создание файла сохранений.
        public static void CreateFile(bool isDefault)
        {
            NumOfFiles++;

            string str = "";
            if (isDefault)
            {
                str = "Unnamed 1";
                UnnamedFilesIdx.Add(1);

                CurrentFilePair = new KeyValuePair<string, string>(str, $"data{NumOfFiles}");
            }

            FileNamesAndAliases.Add(str, $"data{NumOfFiles}");

            try
            {
                string path = String.Format("{0}\\Data\\data{1}.dat", AppDomain.CurrentDomain.BaseDirectory, NumOfFiles);
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(0);
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindow.Logger(ex);
                throw;
            }
        }

        // Удаляет файл сохранения, его имя и псевдоним.
        public static void DeleteFile(string name)
        {
            // Если файл "unnnamed", удаляем его индекс из соответствующего списка.
            if(IsUnnamed(name))
            {
                int idx = GetIdxOfUnnamed(name);
                UnnamedFilesIdx.Remove(idx);
            }
            
            string path = String.Format("{0}\\Data\\{1}.dat", AppDomain.CurrentDomain.BaseDirectory, FileNamesAndAliases[name]);
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                MainWindow.Logger(ex);
                throw;
            }

            FileNamesAndAliases.Remove(name);
        }

        // Сохранение информации о файлах сохранений.
        public static void SaveGlobalData()
        {
            try
            {
                string path = String.Format("{0}\\Data\\global.dat", AppDomain.CurrentDomain.BaseDirectory);
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(NumOfFiles);
                        sw.WriteLine(CurrentFilePair.ToString());

                        foreach (var item in FileNamesAndAliases)
                        {
                            sw.WriteLine(item.ToString());
                        }
                        sw.WriteLine("");

                        foreach (var item in UnnamedFilesIdx)
                        {
                            sw.WriteLine(item.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindow.Logger(ex);
                throw;
            }
        }

        // Сохранение отсортированной информации пользователя.
        public static void SaveSortedData(List<InfoBox> data)
        {
            try
            {
                string path = String.Format("{0}\\Data\\{1}.dat", AppDomain.CurrentDomain.BaseDirectory, CurrentFilePair.Value);
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        int count = 0;

                        foreach (InfoBox box in data)
                        {
                            string[] info = box.GetInfo();

                            for (int i = 0; i < 4; i++)
                            {
                                if (info[i] == null)
                                {
                                    sw.WriteLine("");
                                }
                                else
                                {
                                    sw.WriteLine(info[i]);
                                }
                            }

                            count++;
                        }

                        sw.WriteLine(count);
                    }
                }
            }
            catch (Exception exc)
            {
                MainWindow.Logger(exc);
                throw;
            }
        }

        // Загрузка информации пользователя из файла сохранений.
        public static List<string[]> LoadInfo(KeyValuePair<string, string> fileName)
        {
            // Строим путь.
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\{fileName.Value}.dat";
            // List для загружаемой информации.
            List<string[]> infoList = new List<string[]>();

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string[] infoArr = new string[4];

                        do
                        {
                            infoArr = new string[4];

                            for (int i = 0; i < 4; i++)
                            {
                                infoArr[i] = sr.ReadLine();
                            }

                            infoList.Add(infoArr);
                        }
                        while (infoArr[1] != null);
                    }
                }
            }
            catch (Exception exc)
            {
                MainWindow.Logger(exc);
                throw;
            }

            // Обновляем значение CurrentFileName в связи со сменой файла.
            CurrentFilePair = fileName;

            return infoList;
        }

        // Загрузка информации о файлах сохранений.
        public static void LoadGlobalData()
        {
            List<string> data = new List<string>();

            try
            {
                string path = String.Format("{0}\\Data\\global.dat", AppDomain.CurrentDomain.BaseDirectory);
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        // Построчное считывание в лист data.
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            data.Add(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindow.Logger(ex);
                throw;
            }

            NumOfFiles = Int32.Parse(data[0]);
            CurrentFilePair = GetKeyValuePair(data[1]);

            int idx = 2;
            while (idx < data.Count && data[idx] != "" && data[idx] != null)
            {
                var pair = GetKeyValuePair(data[idx]);
                FileNamesAndAliases.Add(pair.Key, pair.Value);
                idx++;
            }

            idx++;
            while (idx < data.Count)
            {
                UnnamedFilesIdx.Add(Int32.Parse(data[idx]));
                idx++;
            }
        }

        // Получает новое название с наименьшим номером для "unnamed" файла,
        // исходя из уже существующих "unnamed" файлов.
        // ***
        // Метод нужен для следующего. Например есть "unnamed" файлы с цифрами '1', '2' и '3',
        // мы удаляем файл с цифрой '2'. Потом снова добавляется "безымянный" файл. 
        // Ему будет присвоена цифра '2'. 
        public static string GetUnnamedWithIdx()
        {
            int idx = 0;

            while (idx < UnnamedFilesIdx.Count)
            {
                if ((idx + 1) != UnnamedFilesIdx[idx])
                    break;
                else
                    idx++;
            }

            idx++;
            UnnamedFilesIdx.Add(idx);
            return String.Format("Unnamed {0}", idx);
        }

        // Проверяет является ли файл "unnamed".
        private static bool IsUnnamed(string name)
        {
            name = name.ToLower();
            if (name.Contains("unnamed"))
                return true;

            return false;
        }
        
        // Получает номер "unnamed" файла.
        private static int GetIdxOfUnnamed(string name)
        {
            int idx = name.IndexOf(" ");
            name = name.Substring(idx + 1);

            return Int32.Parse(name);
        }

        // Получает объект KeyValuePair из строки вида "[key, pair]".
        private static KeyValuePair<string, string> GetKeyValuePair(string str)
        {
            if(str != " ")
            {
                string key = str.Substring(1, str.IndexOf(",") - 1);
                string value = str.Substring(str.IndexOf(",") + 2, str.IndexOf("]") - str.IndexOf(",") - 2);

                return new KeyValuePair<string, string>(key, value);
            }

            return new KeyValuePair<string, string>("", "");
        }
    }
}
