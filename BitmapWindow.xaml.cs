using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.Drawing;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;
using Point = System.Drawing.Point;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for BitmapWindow.xaml
    /// </summary>
    public partial class BitmapWindow : Window
    {
        private const int FieldWidth = 240;
        private const int FieldHeight = 120;
        private ColorCell[] _cells;
        private byte[] _sourceBitmapArray;
        private const int DefaultDesity = 10;
        readonly PixelFormat pixelFormat = PixelFormats.Bgr24;
        const int BytesPerPixel = 3;
        const int Dpi = 96;
      


        public BitmapWindow()
        {
            InitializeComponent();
            Initialize(DefaultDesity);
            DisplayImage.Source = new WriteableBitmap(FieldWidth, FieldHeight, Dpi, Dpi, pixelFormat, null);
            UpdateImage();
            Closing += BitmapWindow_Closing;

        }

        private void BitmapWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AutoThread?.Abort();
        }

        private void Initialize(int liveDensity)
        {
            _cells = new ColorCell[FieldHeight*FieldWidth];
            _sourceBitmapArray = new byte[_cells.Length*BytesPerPixel];
            var random = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i] = new ColorCell(random.Next() % liveDensity != 0? CellState.Dead : CellState.Alive);
            }
            for (int i = 0; i < _cells.Length; i++)
            {
                var cell = _cells[i];
                if (i%FieldWidth != 0)
                {
                    cell.Neightbours.Add(_cells[i-1]);
                    if (i >= FieldWidth)
                    {
                        cell.Neightbours.Add(_cells[i - FieldWidth - 1]);
                    }
                    if (_cells.Length - i > FieldWidth)
                    {
                        cell.Neightbours.Add(_cells[i + FieldWidth - 1]);
                    }
                }
                if ((i+1) % FieldWidth != 0)
                {
                    cell.Neightbours.Add(_cells[i + 1]);
                    if (i >= FieldWidth)
                    {
                        cell.Neightbours.Add(_cells[i - FieldWidth + 1]);
                    }
                    if (_cells.Length - i > FieldWidth)
                    {
                        cell.Neightbours.Add(_cells[i + FieldWidth + 1]);
                    }
                }
                if (i >= FieldWidth)
                {
                    cell.Neightbours.Add(_cells[i - FieldWidth]);
                }
                if (_cells.Length - i > FieldWidth)
                {
                    cell.Neightbours.Add(_cells[i + FieldWidth]);
                }
            }
        }

        private int _generationsCount = 0;

        private void NextRound()
        {
            Parallel.ForEach(_cells, (c) => c.TakeTurn());
            Parallel.ForEach(_cells, (c) => c.NextRound());
            UpdateGenerationCount();
        }

        private void UpdateGenerationCount()
        {
            if (CheckAccess())
            {
                GenerationsTextBox.Text = Interlocked.Increment(ref _generationsCount).ToString();
            }
            else
            {
                Dispatcher.Invoke(UpdateGenerationCount);
            }
        }

        private void UpdateImage(AutoResetEvent redrawComplete = null)
        {
            if (CheckAccess())
            {
                Parallel.For(0, _cells.Length,
                (i =>
                {
                    _sourceBitmapArray[i * BytesPerPixel] = _cells[i].Color.B;
                    _sourceBitmapArray[i * BytesPerPixel + 1] = _cells[i].Color.G;
                    _sourceBitmapArray[i * BytesPerPixel + 2] = _cells[i].Color.R;
                    
                }));
                var wbm = (WriteableBitmap) DisplayImage.Source;
                wbm.WritePixels(new Int32Rect(0, 0, FieldWidth, FieldHeight), _sourceBitmapArray,
                    BytesPerPixel * FieldWidth, 0, 0);
                redrawComplete?.Set();
            }
            else
            {
                Dispatcher.Invoke(() => UpdateImage(redrawComplete), DispatcherPriority.Background);
            }
        }

       

        private void ReinitButton_Click(object sender, RoutedEventArgs e)
        {
            Initialize(DefaultDesity);
            UpdateImage();
        }

        private void NextRoundButton_Click(object sender, RoutedEventArgs e)
        {
            NextRound();
            UpdateImage();
        }

        Thread AutoThread;
        private void AutoButton_Click(object sender, RoutedEventArgs e)
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
                NextRound();
                UpdateImage(redrawComplete);
                redrawComplete.WaitOne();
                //Thread.Sleep(50);
            }

        }

        private void DisplayImage_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);
            var imagePosition = this.TranslatePoint(position, DisplayImage);
            OnCellTouched(imagePosition, e.ChangedButton);
        }

        private void DisplayImage_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(this);
                var imagePosition = this.TranslatePoint(position, DisplayImage);
                OnCellTouched(imagePosition, e.LeftButton == MouseButtonState.Pressed ? MouseButton.Left : MouseButton.Right);
            }
        }

        private void OnCellTouched(System.Windows.Point position, MouseButton changedButton)
        {
            var pixelX = (int)(position.X / DisplayImage.ActualWidth * FieldWidth);
            var pixelY = (int)(position.Y / DisplayImage.ActualHeight * FieldHeight);
            Debug.WriteLine($"X:{pixelX}  Y:{pixelY}");
            var selectedCell = _cells[pixelX + FieldWidth * pixelY];
            selectedCell.Color = Color.Green;
            selectedCell.CurrentState = changedButton == MouseButton.Left ? CellState.Alive : CellState.Dead;
            UpdateImage();
        }
    }
}
