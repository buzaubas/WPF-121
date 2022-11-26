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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfBasics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonContainers_Click(object sender, RoutedEventArgs e)
        {
            // отображение окна "Контейнеры"
            new ContainersWindow().ShowDialog();
        }
        private void ButtonMines_Click(object sender, RoutedEventArgs e)
        {
            // отображение окна "Сапер"
            new MinesWindow().ShowDialog();
        }
        private void ButtonCanvas_Click(object sender, RoutedEventArgs e)
        {
            new CanvasWindow().ShowDialog();
        }
        private void ButtonStyles_Click(object sender, RoutedEventArgs e)
        {
            new StylesWindow().ShowDialog();
        }
        private void ButtonTriggers_Click(object sender, RoutedEventArgs e)
        {
            new TriggersWindow().ShowDialog();
        }
        private void ButtonDnd_Click(object sender, RoutedEventArgs e)
        {
            new DndWindow().ShowDialog();
        }
        private void ButtonAdoBasics_Click(object sender, RoutedEventArgs e)
        {
            new ADO.AdoBasicsWindow().ShowDialog();
        }
        private void ButtonAdoSales_Click(object sender, RoutedEventArgs e)
        {
            new ADO.SalesWindow().ShowDialog();
        }
        private void ButtonAdoCrud_Click(object sender, RoutedEventArgs e)
        {
            new ADO.View.AdoCrudWindow().ShowDialog();
        }
        private void ButtonEF_Click(object sender, RoutedEventArgs e)
        {
            new ADO.View.AdoEfWindow().ShowDialog();
        }
        private void ButtonSp1_Click(object sender, RoutedEventArgs e)
        {
            new SystemProgramming.Sp1Window().ShowDialog();
        }
        private void ButtonTask_Click(object sender, RoutedEventArgs e)
        {
            new SystemProgramming.TaskWindow().ShowDialog();
        }
        private void ButtonProcesses_Click(object sender, RoutedEventArgs e)
        {
            new SystemProgramming.ProcessesWindow().ShowDialog();
        }
    }
}