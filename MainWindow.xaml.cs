using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        private const int FieldHeight = 50;
        private const int FieldWidth = 50;
        const double CellWidth = 7;
        const double CellHeight = 7;
        
        private Random _random;
        private VisualCell[,] _cells;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitCells(3);
        }
       
        private void AddToCanvas(Rectangle rect, int i, int j)
        {
            rect.Width = CellWidth;
            rect.Height = CellHeight;
            rect.Stroke = Brushes.Black;
            rect.StrokeThickness = 0.5;
            Canvas.SetLeft(rect, CellWidth * j);
            Canvas.SetTop(rect, CellHeight * i);
            LiveCanvas.Children.Add(rect);
        }

        private void InitCells(int liveDensity)
        {
            _cells = new VisualCell[FieldHeight, FieldWidth];
            _random = new Random(DateTime.Now.Millisecond);

            // Init array of cells
            for (int i = 0; i < _cells.GetLength(0); i++)
            {
                for (int j = 0; j < _cells.GetLength(1); j++)
                {
                    _cells[i, j] = new VisualCell(_random.Next() % liveDensity == 0 ? CellState.Dead : CellState.Alive);
                    // Add cell's rectangle to canvas
                    AddToCanvas(_cells[i, j].Rectangle, i, j);
                }
            }

            // Count neightbours and add references
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



        private void ReinitButton_Click(object sender, RoutedEventArgs e)
        {
            InitCells(2);
        }

        private void NextRoundButton_Click(object sender, RoutedEventArgs e)
        {
            NextRound();
        }

        private void NextRound(AutoResetEvent redrawComplete = null)
        {
            Parallel.ForEach(_cells.ToEnumerable<Cell>(), (c) => c.TakeTurn());
            try
            {
                Dispatcher.Invoke(() =>
                {
                    foreach (var cell in _cells)
                    {
                        cell.NextRound();
                    }
                    redrawComplete?.Set();
                }, DispatcherPriority.ApplicationIdle);
            }
            catch (TaskCanceledException exception)
            {
                Trace.WriteLine("Rude shutdown");
            }
        }

        Thread AutoThread;
        private void AUtoButton_Click(object sender, RoutedEventArgs e)
        {
            if (AutoThread == null)
            {
                AutoThread = new Thread(AutoRoutine) { IsBackground = true };
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
            AutoResetEvent redrawComplete = new AutoResetEvent(false);
            while (true)
            {
                NextRound(redrawComplete);
                redrawComplete.WaitOne();
                Thread.Sleep(50);
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
