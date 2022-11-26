using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfBasics.SystemProgramming
{
    /// <summary>
    /// Interaction logic for TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        public TaskWindow()
        {
            InitializeComponent();
        }

        #region Пул потоков

        CancellationTokenSource cts;
        private void Start1_Click(object sender, RoutedEventArgs e)
        {
            Log1.Text = "Start";
            cts = new();
            for (int i = 0; i < 40; i++)
            {
                ThreadPool.QueueUserWorkItem(PoolWorker, new PoolWorkerData
                {
                    Num = i,
                    CancellationToken = cts.Token
                });
            }
        }

        private void Stop1_Click(object sender, RoutedEventArgs e)
        {
            Log1.Text += "Stop";
            cts?.Cancel();
        }

        private void PoolWorker(object? pars)
        {
            if (pars is PoolWorkerData data)
            {
                if (data.CancellationToken.IsCancellationRequested) 
                { 
                    return; 
                }

                Thread.Sleep(1000);
                Dispatcher.Invoke(() => Log1.Text += $"..{data.Num}\t");
                // Log1.Text += $"..{pars}\t"; - как способ проверить, что это другой поток
            }
        }
        class PoolWorkerData
        {
            public int Num { get; set; }
            public CancellationToken CancellationToken { get; set; }
        }
        #endregion

        #region Многозадачность

        private void Start2_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(Starter);
        }
        
        private void Stop2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Starter()
        {
            Task t1 = new Task(() => TaskMethod0(20));  // Создание задачи t1 
            t1.Start();                                 // запуск t1 
                                                        // 
            Task t2 = Task.Run(() => TaskMethod0(10));  // Создание и запуск t2
                                                        // 
            t1.Wait();                                  // Ожидание окончания t1 
                                                        // 
            Task.Run(() => TaskMethod0(30));            // запуск t3 после t1 
        }

        private void TaskMethod0(int num)
        {
            Thread.Sleep(1000);
            Dispatcher.Invoke(() => Log2.Text += $"..{num}\t");
        }




        private void Start3_Click(object sender, RoutedEventArgs e)
        {
            Log2.Text = "Start3 ";
            Task.Run(Starter3_Good);
        }
        private void Starter3_Good()
        {
            Task<String> t1 = Task.Run(() => TaskMethod1(40));
            Task<String> t2 = Task.Run(() => TaskMethod1(50));
            Task<String> t3 = Task.Run(() => TaskMethod1(60));

            Dispatcher.Invoke(() => Log2.Text += t1.Result);            
            Dispatcher.Invoke(() => Log2.Text += t2.Result);
            Dispatcher.Invoke(() => Log2.Text += t3.Result);
        }
        private void Starter3()
        {
            Task<String> t = Task.Run(() => TaskMethod1(40));
            String res = t.Result;
            Dispatcher.Invoke(() => Log2.Text += res);     // Такая схема
                                                           // приводит к
            t = Task.Run(() => TaskMethod1(50));           // последовательному
            res = t.Result;                                // выполнению всех
            Dispatcher.Invoke(() => Log2.Text += res);     // задач.
                                                           // res = t.Result; значит
            t = Task.Run(() => TaskMethod1(60));           // ожидание завершения
            res = t.Result;                                // задачи
            Dispatcher.Invoke(() => Log2.Text += res);     // 
        }
        private String TaskMethod1(int num)
        {
            Thread.Sleep(1000);
            return $"tm1-{num}\t";
        }

        #endregion

        #region async/await
        /* Задача: есть файл, в нем "конфигурация" - в каждой строке 
         * записана "пара"  имя=значение
         * Необходимо: считать файл, разобрать его содержимое в Dictionary
         * ------
         * Анализ: считывание файла процесс достаточно длительный, поэтому
         * логично отделить эту подзадачу в самостоятельный метод и запускать
         * асинхронно.
         * Разбор строки на словать также логично реализовать в виде
         * самостоятельной задачи, это позволит ее использовать более
         * универсально (например, получать текст не из файла, а из Инета)
         */
        private async void StartAsyncButton_Click(object sender, RoutedEventArgs e)
        {
            /* LogAsync.Text = "ini loading...\n";  // Последовательное 
            String fileContent =                    // выполнение
                await FileContentAsync("SystemProgramming/TaskWindow.ini");
            LogAsync.Text += fileContent; */

            /*
            var loader = FileContentAsync("SystemProgramming/TaskWindow.ini");
            LogAsync.Text = "ini loading...\n";  // более эффективно:
            // .....                             // пока задача работает, 
            LogAsync.Text += await loader;       // выполняются парал. действия
            */
            /*  // последовательный вызов, организуемый "здесь"
            var loader = FileContentAsync("SystemProgramming/TaskWindow.ini");
            LogAsync.Text = "ini loading...\n";
            String fileContent = await loader;
            var dic = await ParseIniAsync(fileContent);
            LogDicAsync(dic);
            */
            // Создание "нити" кода - последовательности выполнения задач
            var task =
                FileContentAsync("SystemProgramming/TaskWindow.ini")  // первая - FileContent
                .ContinueWith(res => ParseIniAsync(res.Result))       // по окончанию - ParseIni
                .Unwrap()                                             // убрать один Task - техническое действие
                .ContinueWith(res => LogDicAsync(res.Result));        // по окончанию - LogDic
            // пока нить выполняется - параллельно выполняем действия
            LogAsync.Text = "ini loading...\n";
            // если нужно, можно ожидать завершение нити
            // await task;
        }

        // private async Task<String> FileContentAsync(String filename)
        private async                      // ключевое слово async - асинхронный режим
            Task<String>                   // все async возвращают Task, String - Result
            FileContentAsync(              // имена методов следует заканчивать Async
                String filename)           // перечень параметров - произвольный
        {                                  // 
            return await Task.Run( () =>   // "преобразование" синхронного кода в async
            {
                var sb = new StringBuilder();
                using(StreamReader reader = new(filename))
                {
                    while (!reader.EndOfStream)
                    {
                        sb.AppendLine(reader.ReadLine());
                    }
                }
                // Task.Delay(3000).Wait();
                return sb.ToString();
            });
        }
        private async Task<Dictionary<String, String>> ParseIniAsync(String ini)
        {
            return await Task.Run(() =>
            {
                Dictionary<String, String> res = new();
                String[] lines = ini.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach(String line in lines)
                {
                    if (line.StartsWith("#"))
                    {
                        continue;   // пропускаем строки - комментарии
                    }
                    String[] pair = line.Split('=');   // [0]-key [1]-value
                    res[pair[0]] = pair[1];
                    // res["ip"] = "127.0.0.1";
                }
                return res;
            });
        }
        private async void LogDicAsync(Dictionary<String, String> dic)
        {
            // вывести словарь dic в LogAsync (TextBlock)
            await Task.Run(() =>
            {
                foreach (var pair in dic)   // итератор: pair.Key  / pair.Value
                {
                    this.Dispatcher.Invoke(() =>
                        LogAsync.Text += $"{pair.Key,-10}= {pair.Value}"
                    );
                }
            });
        }
        #endregion

    }

}
