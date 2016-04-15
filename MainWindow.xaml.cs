using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.PerformanceData;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int FieldHeight = 60;
        private const int FieldWidth = 60;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitCells(3);
            Draw();
        }

      
        readonly Brush _deadBrush = Brushes.White;
        readonly Brush _liveBrush = Brushes.Green;
        const double CellWidth = 10;
        const double CellHeight = 10;

        private void Draw()
        {
            LiveCanvas.Children.Clear();
            for (int i = 0; i < _cells.GetLength(0); i++)
            {
                for (int j = 0; j < _cells.GetLength(1); j++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Width = CellWidth;
                    rect.Height = CellHeight;
                    rect.Stroke = Brushes.Black;
                    rect.StrokeThickness = 0.5;
                    Canvas.SetLeft(rect,CellWidth * j);
                    Canvas.SetTop(rect, CellHeight * i);
                    rect.Fill = _cells[i, j].CurrentState == CellState.Alive ? _liveBrush : _deadBrush;
                    LiveCanvas.Children.Add(rect);

                }
            }
        }

        private void AddToCanvas(Rectangle rect, int i, int j)
        {
            rect.Width = CellWidth;
            rect.Height = CellHeight;
            rect.Stroke = Brushes.Black;
            rect.StrokeThickness = 0.5;
            Canvas.SetLeft(rect, CellWidth * j);
            Canvas.SetTop(rect, CellHeight * i);
            rect.Fill = _cells[i, j].CurrentState == CellState.Alive ? _liveBrush : _deadBrush;
            LiveCanvas.Children.Add(rect);
        }

        private void InitCells(int liveDensity)
        {
            _cells = new Cell[FieldHeight, FieldWidth];
            _random = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < _cells.GetLength(0); i++)
            {
                for (int j = 0; j < _cells.GetLength(1); j++)
                {
                    _cells[i, j] = new Cell(_random.Next() % liveDensity == 0 ? CellState.Alive : CellState.Dead);
                }
            }
            for (int i = 0; i < _cells.GetLength(0); i++)
            {
                for (int j = 0; j < _cells.GetLength(1); j++)
                {
                    var cell = _cells[i, j];
                    if (i > 0)
                    {
                        cell.Neightbours.Add(_cells[i - 1, j]);
                        if (j > 0)
                            cell.Neightbours.Add(_cells[i - 1, j - 1]);
                        if (j < _cells.GetLength(1) - 1)
                            cell.Neightbours.Add(_cells[i - 1, j + 1]);

                    }
                    if (j > 0)
                    {
                        cell.Neightbours.Add(_cells[i, j - 1]);
                    }
                    if (j < _cells.GetLength(1) - 1)
                    {
                        cell.Neightbours.Add(_cells[i, j + 1]);
                    }
                    if (i < _cells.GetLength(0) - 1)
                    {
                        cell.Neightbours.Add(_cells[i + 1, j]);
                        if (j > 0)
                            cell.Neightbours.Add(_cells[i + 1, j - 1]);
                        if (j < _cells.GetLength(1) - 1)
                            cell.Neightbours.Add(_cells[i + 1, j + 1]);

                    }

                }
            }
            

        }

        private Random _random;
        private Cell[,] _cells;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InitCells(2);
            Draw();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var cell in _cells)
            {
               cell.TakeTurn();
            }
            foreach (var cell in _cells)
            {
                cell.NextRound();
            }
            Draw();
        }

        Thread AutoThread;
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (AutoThread == null)
            {
                AutoThread = new Thread(AutoRoutine) {IsBackground = true};
                AutoThread.Start();
            }
            else
            {
                AutoThread.Abort();
                AutoThread = null;
            }
        }

        private void AutoRoutine()
        {
            while (true)
            {
                //foreach (var cell in _cells)
                //{
                //    cell.TakeTurn();
                //}
                //foreach (var cell in _cells)
                //{
                //    cell.NextRound();
                //}

                Parallel.ForEach(_cells.ToEnumerable<Cell>(), (c) => c.TakeTurn());
                Parallel.ForEach(_cells.ToEnumerable<Cell>(), (c) => c.NextRound());

                Dispatcher.Invoke(Draw, DispatcherPriority.ApplicationIdle);
                Thread.Sleep(10);
            }
          
        }


       
    }
    public static class ArrayExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this Array target)
        {
            foreach (var item in target)
                yield return (T)item;
        }
    }
}
