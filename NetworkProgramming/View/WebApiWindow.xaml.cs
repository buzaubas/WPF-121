﻿using NetworkProgramming.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkProgramming.View
{
    /// <summary>
    /// Interaction logic for WebApiWindow.xaml
    /// </summary>
    public partial class WebApiWindow : Window
    {
        public ObservableCollection<AssetModel> Assets { get; set; }
        private System.Windows.Media.Color graphColor;
        private Random random = new();

        public WebApiWindow()
        {
            InitializeComponent();
            Assets = new ObservableCollection<AssetModel>();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // DrawLine(10, 10, Graph.ActualWidth-10, Graph.ActualHeight-10);
            
            Task.Run(GetAssets);
            graphColor = System.Windows.Media.Color.FromArgb(
                255, 
                (byte) random.Next(50, 250),
                (byte) random.Next(50, 250),
                (byte) random.Next(50, 250));
        }

        private async void GetAssets()
        {
            using var client = new HttpClient { BaseAddress = new Uri("https://api.coincap.io/") };
            String assets = await client.GetStringAsync("/v2/assets/");
            ProcessAssets(assets);
        }
        private void ProcessAssets(String assets)
        {
            var json = JsonSerializer.Deserialize<Models.AssetData>(assets);
            if (json is null) return;
            
            // Получив и разобрав ассеты запускам историю по первому из них
            Task.Run(() => GetHistory(json.data[0].id));   // старт задачи (в новом потоке)

            Dispatcher.Invoke(() => {  // делегируем работу с UI главному потоку
                // Выводим название валюты, запущенной на историю
                coinTitle.Content = ": " + json.data[0].name;

                // отображаем полученные ассеты путем помещения ссылок на них в 
                // наблюдаемую коллекцию Assets, связанную с ListView
                json.data.ForEach(Assets.Add);
            });
        }

        /// <summary>
        /// Получает историю курсов валюты
        /// </summary>
        private async void GetHistory(String assetId)
        {
            using var client = new HttpClient { BaseAddress = new Uri("https://api.coincap.io/") };
            String history = await client.GetStringAsync($"/v2/assets/{assetId}/history?interval=d1");
            ProcessHistory(history);
        }

        /// <summary>
        /// Обрабатывает полученные с сайта данные, отображает график
        /// </summary>
        /// <param name="history">Загруженные данные с историей курсов</param>
        private void ProcessHistory(String history)
        {
            var json = JsonSerializer.Deserialize<Models.HistoryData>(history);
            if (json is null) return;
            // json.data - массив элементов HistoryModel с годовыми курсами валюты

            /* Работаем над графиком:
             * по Х время (json.data[].time)
             * по Y курс (json.data[].priceUsd)
             * Данные нужно масштабировать, т.к. они явно не соответствуют
             * размерам холста. Для этого определяем максимальное и минимальное
             * значение по каждой координате
             */
            Int64  minTime,  maxTime;
            Double minPrice, maxPrice;
            minTime  = maxTime  = json.data[0].time;
            minPrice = maxPrice = json.data[0].price;
            foreach (HistoryModel item in json.data)
            {
                if (item.time < minTime) minTime = item.time;
                if (item.time > maxTime) maxTime = item.time;
                if (item.price < minPrice) minPrice = item.price;
                if (item.price > maxPrice) maxPrice = item.price;
            }
            /* Еще один цикл - масштабирует
             * точка на графике X: (time-minTime) - от нуля до (maxTime-minTime)
             *   ноль нас устраивает, но нужно чтобы максимальное значение 
             *   было шириной холста (Graph.ActualWidth):
             *   (time-minTime) / (maxTime-minTime) * Graph.ActualWidth
             * по Y аналогично, только с price и Graph.ActualHeight, но еще и "перевернуть",
             *  т.к. ось Y на холсте направлена вниз:
             *  y = Graph.ActualHeight - y
             * 
             * Для того чтобы проводить линии нужно помнить предыдущую точку и 
             *  соединять ее с текущей.
             */
            Double x1 = -1, y1 = -1;

            foreach (HistoryModel item in json.data)
            {                
                Double x2 = (item.time - minTime) * Graph.ActualWidth / (maxTime - minTime);
                Double y2 = (item.price - minPrice) * Graph.ActualHeight / (maxPrice - minPrice);
                y2 = Graph.ActualHeight - y2;   // инверсия по Y (вверх ногами)

                if (x1 != -1)   // не первая точка
                {
                    Dispatcher.Invoke(() => DrawLine(x1, y1, x2, y2));
                }
                
                x1 = x2;
                y1 = y2;
            }
        }

        /// <summary>
        /// Рисует линию на холсте Graph из точки (х1, у1) в точку (х2, у2)
        /// </summary>
        private void DrawLine(double x1, double y1, double x2, double y2)
        {
            Graph.Children.Add( new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(graphColor),
                StrokeThickness = 2
            });
        }
    }
}
/* Д.З. Реализовать загрузку и отображение истории той валюты, которую
 * в списке выберет пользователь двойным щелчком мыши.
 * Расширить список доступных валют (вывести сведения о цене priceUsd и кол-ве supply)
 * ** добавить флажок "Стирать график" состояние которого будет учитываться при
 *    выводе нового графика: очищать или нет холст перед построением нового графика
 */
