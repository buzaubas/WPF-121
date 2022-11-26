using System;
using System.Collections.Generic;
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

namespace WpfBasics
{
    /// <summary>
    /// Interaction logic for MinesWindow.xaml
    /// </summary>
    public partial class MinesWindow : Window
    {
        private const String MINE_SYMBOL = "\x2622";
        private const String FREE_SYMBOL = "\x0DF4";
        private const String FLAG_SYMBOL = "\x2691";

        private Random random = new();

        public MinesWindow()
        {
            InitializeComponent();
            for (int y = 0; y < App.FIELD_SIZE_Y; y++)
            {
                for (int x = 0; x < App.FIELD_SIZE_X; x++)
                {
                    FieldLabel label = new()
                    {
                        X = x,
                        Y = y,
                        IsMine = random.Next(3) == 0
                    };
                    label.Content = FREE_SYMBOL; // label.IsMine ? MINE_SYMBOL : FREE_SYMBOL; 
                    label.FontSize = 20;

                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    label.VerticalContentAlignment = VerticalAlignment.Center;

                    label.Background = Brushes.Beige;
                    label.Margin = new Thickness(1);

                    // Подключаем обработчик события
                    label.MouseLeftButtonUp += LabelClick;
                    label.MouseRightButtonDown += LabelRightClick;

                    // Регистрируем имя для элемента, по этому имени его
                    // можно будет найти (в другом коде)
                    this.RegisterName($"label_{x}_{y}", label);

                    Field.Children.Add(label);
                }
            }
        }
        // Задание: по нажатию правой кнопки мыши устанавливается "флажок"
        // при этом ячека не нажимается левой. Повторное нажатие снимает
        // "флажок".

        // Задание: выигрыш - проверять условия, что все свободные ячейки
        // открыты.
        private bool IsWin()
        {
            foreach (var child in Field.Children)
            {
                if(child is FieldLabel label)
                {
                    if( ! label.IsMine && 
                        (label.Content.Equals(FREE_SYMBOL) ||
                         label.Content.Equals(FLAG_SYMBOL)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        // обработчик события нажатия ПКМ
        private void LabelRightClick(object sender, RoutedEventArgs e)
        {
            if (sender is FieldLabel label)
            {
                // Если контент - не закрытая ячейка, то не обрабатываем нажатие
                if ( ! label.Content.Equals(FREE_SYMBOL)
                  && ! label.Content.Equals(FLAG_SYMBOL)) return;

                label.Content =
                    label.Content.Equals(FLAG_SYMBOL)
                    ? FREE_SYMBOL
                    : FLAG_SYMBOL;
            }
        }

        // обработчик события нажатия ЛКМ
        private void LabelClick(object sender, RoutedEventArgs e)
        {
            if(sender is FieldLabel label)
            {
                // Если контент - флажок, то не обрабатываем нажатие
                if (label.Content.Equals(FLAG_SYMBOL)) return;
                
                // Если мина - сообщение Game Over, иначе кол-во мин отображаем на самой ячейке
                if (label.IsMine)
                {
                    label.Content = MINE_SYMBOL;

                    // MessageBox.Show("Game Over");  // Переделать на "еще раз? (да/нет)"
                    if (MessageBoxResult.No ==
                        MessageBox.Show("Play again?", "Game Over", MessageBoxButton.YesNo))
                    {
                        this.Close();
                    }
                    else
                    {
                        foreach(var child in Field.Children)
                        {
                            if(child is FieldLabel cell)
                            {
                                cell.Content = FREE_SYMBOL;
                                cell.IsMine = random.Next(3) == 0;
                            }
                        }
                    }
                    return;
                }
                
                // Задание: определить имена всех возможных соседей 
                String[] names =  // массив имен соседей
                {
                    $"label_{label.X - 1}_{label.Y - 1}",
                    $"label_{label.X    }_{label.Y - 1}",
                    $"label_{label.X + 1}_{label.Y - 1}",
                    $"label_{label.X - 1}_{label.Y}",
                    $"label_{label.X + 1}_{label.Y}",
                    $"label_{label.X - 1}_{label.Y + 1}",
                    $"label_{label.X    }_{label.Y + 1}",
                    $"label_{label.X + 1}_{label.Y + 1}",
                };
                int mines = 0;
                foreach (String name in names)
                {
                    // Поиск элемента по имени, преобразование типа
                    // var neighbour = this.FindName(name) as FieldLabel;
                    // if (neighbour != null)
                    if (this.FindName(name) is FieldLabel neighbour)
                    {
                        if(neighbour.IsMine) mines += 1;
                    }
                }
                label.Content = mines.ToString();

                // Состояние поля изменилось - проверяем условие победы
                if (IsWin())
                {
                    // Вывести сообщение и предложить повторную игру
                }
            }
        }
    }
    /* Д.З. Обеспечить отображение ячеек с разным кол-вом мин 
     * разным цветом.
     * ** Если в ячейке получается ноль, то открывать все соседние ячейки
     * Добавить элемент, отображающий кол-во мин на поле, уменьшать его
     *  после установки "флажков"
     * * Добавить таймер игры (время от начала игры)
     * Не забыть - новая игра отменяет цвета и сбрасывает время.
     */

    // Класс, расширяющий Label дополнительными полями
    class FieldLabel : Label
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsMine { get; set; }

    }
}
/* Взаимодействие элементов.
На примере игры "Сапер"
Главная идея - по щелчку на ячейке поля отображается кол-во
 "мин", находящихся в соседних ячейках
 
1. Объявляем константы, которые будут доступны во всем приложении
   (и в коде, и в разметке): в файле приложения App.xaml.cs
     public const int FIELD_SIZE_X = 8;
2. Разметка. 
   <UniformGrid 
     x:Name="Field"   - имя, по которому UniformGrid будет доступен в коде
     Columns="{x:Static local:App.FIELD_SIZE_X}"   - "обратная" связь - 
                                                используем данные из кода
3. Код.
    - организовываем циклы по введенным в Арр константам
    - создаем элементы Label и добавляем их в коллекцию 
       Field.Children - дочерние элементы контейнера Field (см. UniformGrid)
-----------------
Задание1: сохранить в Label дополнительную информацию (признак мины,
 координаты на поле)
Задание2: иметь возможность определить соседей - ячейки с заданными
 координатами

Решение1: а) у каждого элемента есть "свободное" поле Tag, в котором можно
 сохранять произвольные данные; б) есть возможность создавать наследников
 (от Label) и добавлять данные
Решение2: а) перебирать коллекцию Field.Children и искать элементы с
 заданными координатами; б) зарегистрировать имена для ячеек и искать 
 их по известным именам.
 */
