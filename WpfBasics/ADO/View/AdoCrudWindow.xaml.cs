using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfBasics.ADO.View
{
    /// <summary>
    /// Interaction logic for AdoCrudWindow.xaml
    /// </summary>
    public partial class AdoCrudWindow : Window
    {
        // ObservableCollection - коллекция, уведомлящая контейнер о своих изменениях
        public ObservableCollection<Entities.Department> Departments { get; set; }
        private readonly SqlConnection _connection;
        private readonly DAL.Departments _departments;
        private DispatcherTimer timer = new DispatcherTimer();
        int n=0;

        public AdoCrudWindow()
        {
            InitializeComponent();
            _connection = new SqlConnection(App.ConnectionString);
            ConnectDb();
            _departments = new DAL.Departments(_connection);
            Departments = new(_departments.GetList()) {
                new Entities.Department
                {
                    Id = Guid.Empty,
                    Name = "Добавить новый отдел"
                }
            };
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            // Связывание данных (1) указываем контекст - откуда берутся
            //  имена ресурсов
            this.DataContext = this;  
        }
        private void ConnectDb()
        {
            try
            {
                _connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connection error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            Departments.Add(_departments.GetList()[n]);
            n++;
            if(n == _departments.GetList().Count)
            {
                timer.Stop();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // timer.Start();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            _connection?.Close();
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // двойной щелчек мыши на элементе (строке) списка
            if( sender is ListViewItem item)
            {
                // В тело заходим если sender - это ListViewItem
                // извлекаем ссылку на объект данных (Department)
                if( item.Content is Entities.Department department )
                {
                    // department - ссылка на элемент коллекции Departments,
                    //  на котором сработало событие
                    // MessageBox.Show(department.ToString());
                    var editWindow = new View.Models.DepartmentWindow()
                    {
                        Department = department
                    };
                    if (editWindow.ShowDialog() == true)
                    {
                        if (department.Id == Guid.Empty)  // Добавление
                        {
                            Guid id = _departments.Create(department);
                            Departments.Remove(department);
                            department.Id = id;
                            Departments.Add(department);
                            Departments.Add(new Entities.Department
                            {
                                Id = Guid.Empty,
                                Name = "Добавить новый отдел"
                            });
                        }
                        else  // Изменение, Удаление
                        {
                            if (department.Name == String.Empty)  // Удаление
                            {
                                Departments.Remove(department);
                                _departments.Delete(department);
                            }
                            else  // Изменение
                            {
                                // Коллекция не отслеживает изменения внутри элементов
                                // поэтому создаем эффект изменения состава коллекции
                                int index = Departments.IndexOf(department);
                                Departments.Remove(department);
                                Departments.Insert(index, department);
                                // Пока обновлен только список, вносим изменения в БД
                                _departments.Update(department);
                            }
                            
                        }
                    }
                }
            }
        }
    }
}
/*
 CRUD - (Create Read Update Delete) - концепция, согласно которой
 информационная система должна обеспечить эти 4 операции по отношению
 ко всем своим данным.
Create - добавление данных (Add, Insert) - создание новых инфо-единиц
Read   - отображение, извлечение данных из БД
Update - внесение изменений в уже существующие данные
Delete - удаление данных из БД. Особенность БД еще и в том, что
          удаление нельзя откатить (отменить). Поэтому одной из традиций
          является замена настоящего удаления введением дополнительного
          поля "deleted" (либо признак, либо дата удаления)
         Как вариант, ведется отдельная таблица удалений, в которой
          кроме даты отмечается кто удалил, причина удаления и т.п.

Задание: ограничить возможность введения пустого названия для
нового отдела
Д.З. Реализовать концепцию CRUD для работы с таблицей товаров (Products)
По аналогии с рассмотренными задачами с таблицей отделов (Departments)
 */
