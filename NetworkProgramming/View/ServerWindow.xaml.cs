using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
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
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        private Models.NetworkConfig? networkConfig;  // из стартового окна

        private Socket? listenSocket;   // постоянно активный, слушающий
        private Thread? listenThread;   // поток с сервером
        public ServerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // в Tag - данные о конфигурации
            if(this.Tag is Models.NetworkConfig config)
            {
                networkConfig = config;           // сохраняем полученную конфигурацию
                listenSocket = new Socket(        // один на окно
                    AddressFamily.InterNetwork,   // IPv4 адресация
                    SocketType.Stream,            // двусторонний (чтение/запись)
                    ProtocolType.Tcp              // протокол TCP
                );
                // запуск сервера - обязательно в отдельном потоке
                listenThread = new Thread(StartServer);
                listenThread.Start();
            }
            else
            {
                MessageBox.Show("Configuration error");
                Close();
            }
        }

        private void StartServer()
        {
            if (listenSocket is null || networkConfig is null) return;

            Socket? requestSocket = null;  // обменный сокет - новый для каждого клиента
            try
            {
                listenSocket.Bind(networkConfig.EndPoint);   // привязка сокета к EndPoint
                listenSocket.Listen(10);   // 10 - допустимая очередь

                // если к этой строке нет исключений, то сервер запущен
                // msg = "сервер запущен\n";
                Dispatcher.Invoke(() => { Log.Text += "сервер запущен\n"; });

                #region Прием и обработка запросов
                byte[] buffer = new byte[2048];               // буфер приема данных
                String str;                                   // перевод буфера в строку
                int n;                                        // кол-во принятых символов
                while (true)                                  // постоянное "слушание"
                {                                            
                    requestSocket =                           // ожидание запроса и открытие
                        listenSocket.Accept();                // сокета обмена данными
                    str = "";                                 // Новый сеанс приема
                    do
                    {
                        n = requestSocket.Receive(buffer);    // получаем данные в буфер, n - кол-во реально полученных байт
                        str +=                                // Переводим байты в строку
                            networkConfig.Encoding            // согласно принятой кодировке
                            .GetString(buffer, 0, n);         // Ограничивам работу n байтами

                    } while (requestSocket.Available > 0);    // пока есть данные для приема

                    // Данные получены и переведены в строку str
                    // преобразуем их в команду (запрос) и проанализируем
                    var request = JsonSerializer
                        .Deserialize<Models.ClientRequest>(str);
                    String response;
                    switch (request?.Command)
                    {
                        case "CREATE":
                            response = "Новое сообщение: " + request.Data;
                            break;
                        default:
                            response = "Команда не распознана";
                            break;
                    }
                    // Отвечаем клиенту (обратный процесс: строка-байты-отправка)
                    buffer = networkConfig.Encoding.GetBytes(response);
                    requestSocket.Send(buffer);

                    // Закрываем соединение (обменный сокет)
                    requestSocket.Shutdown(SocketShutdown.Both);
                    requestSocket.Close();

                    // логгируем полученное сообщение
                    Dispatcher.Invoke(() => { Log.Text += str + "\n"; });
                }
                #endregion
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => { Log.Text += ex.Message + "\nсервер остановлен\n"; });
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if(listenThread != null)     // сервер запущен - надо останавливать
            {
                listenSocket?.Close();   // закрываем сокет - это создаст исключение, разрушающее поток
            }
            // this.Close();
        }
    }
}
/* Сервер находится в постоянной активности - "слушает порт"
 * Поэтому серверная активность всегда запускается в 
 * отдельном потоке (иначе он полностью заблокирует интерфейс)
 * ! В серверной части существуют два типа сокетов
 *   - слушающий сокет: постоянно активная компонента
 *   - сокет обмена данными: создается для каждого запроса
 *       от клиента, обеспечивает обмен данными и закрывается
 *       после этого
 */
/* Д.З. Создать модель для ответа сервера
 * ServerResponse {
 *    Status - успешно или неуспешно (можно взять HTTP Status Codes)
 *    Data - результат работы
 * }
 * Со стороны клиента реализовать разбор данных и их анализ:
 * если статус успешный, то выводить сообщение с результатами 
 * от сервера, если нет, то формировать сообщение об ошибке
 * обработки запроса.
 */