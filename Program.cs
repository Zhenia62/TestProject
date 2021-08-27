using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestProject
{
    class Program
    {
        static bool statusError = false; // Статус при неправильном вводе пути к файлу
        static Stopwatch programTime = new Stopwatch(); // Указывает время работы программы в целом
        static Stopwatch searchTriplets = new Stopwatch(); // Указывает время обработки текста

        static string path = "";

        [STAThread]
        static async Task Main(string[] args)
        {
            programTime.Start(); //Старт таймера работы программы

            Thread thread = new Thread(() => SelectFile()); //Выделяем отдельный поток для запуска формы выбора файла из файловой системы
            thread.SetApartmentState(ApartmentState.STA);


            thread.Start(); thread.Join();
            

            Console.WriteLine("\n");

            searchTriplets.Start();//Старт таймера обработки текста
            string result = await GetText(path);

            Console.WriteLine($"\n{result}");
            programTime.Stop(); searchTriplets.Stop();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Общее время выполнения программы: {programTime.Elapsed.TotalSeconds}; Время обработки текста: {searchTriplets.Elapsed.TotalSeconds}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        //Выбор текстового файла
        static string SelectFile()
        {            
            do //Пока не будет введено валидное значение программа будет выполнятся
            {
                Console.Clear();
                statusError = false;
                Console.WriteLine($"{ConsoleLines.Title}\n");

                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.I: //Input - Ввод пути к файлу вручную
                        path = Console.ReadLine();
                        CheckingForNullAndEmpty(path); 
                        break;
                    case ConsoleKey.C: //Choose - Выбор файла через файловый менеджер
                        OpenFileDialog OPF = new OpenFileDialog();

                        OPF.Title = "Выберите текстовый файл";
                        OPF.Filter = "txt files (*.txt)|*.txt";

                        if (OPF.ShowDialog() == DialogResult.OK)
                        {
                            path = OPF.FileName;
                            CheckingForNullAndEmpty(path);
                        }
                        break;
                    case ConsoleKey.Escape: //Exit - просто выход из программы
                        Environment.Exit(0);
                        break;
                    default: //Other - нажатие какой-то иной клавиши
                        statusError = true;
                        break;
                }

            } while (statusError);

            return path;

        }

        //Проверка полученого пути на валидность
        static void CheckingForNullAndEmpty(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                statusError = true;
            }
        }

        //Чтение текстового файла
        static async ValueTask<string> GetText(string path)
        {
            var line = "";


            if (File.Exists(path))
            {
                using (var sr = new StreamReader(path, Encoding.Default))
                {
                    line = await sr.ReadToEndAsync();

                }
            }

            return await Task.Run(() => GetResult(line));

        }

        //Получение конечного результата
        static string GetResult(string text)
        {
            //Убираем из результата триплеты с пробелами и вешаем группировку
            var triplets = GetTriplets(text)
                 .Where(str => str.All(ch => char.IsLetter(ch)))
                 .GroupBy(str => str);

            //Объединяем элементы через разделитель согласно условию и выбираем 10 элементов. 
            return string.Join(',',
                    triplets.OrderByDescending(gr => gr.Count()).Take(10).Select(gr => $"{gr.Key}")
                    );
        }

        //Получаем подстроки, состоящие из 3 букв из главного текста
        //Возвращаем коллекцию данных триплетов
        public static List<string> GetTriplets(string text)
        {
            var collectionsParts = new List<string>();
            for (int i = 3; i <= text.Length; i++)
                collectionsParts.Add(text.Substring(i - 3, 3));
            return collectionsParts;

        }

    }
}
