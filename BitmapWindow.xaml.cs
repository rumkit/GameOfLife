using System;
using System.Collections.Generic;
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

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for BitmapWindow.xaml
    /// </summary>
    public partial class BitmapWindow : Window
    {
        private const int FieldWidth = 640;
        private const int FieldHeight = 480;
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

        private uint _generationsCount = 0;

        private void NextRound()
        {
            Parallel.ForEach(_cells, (c) => c.TakeTurn());
            Parallel.ForEach(_cells, (c) => c.NextRound());
           
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
                GenerationsTextBox.Text = (++_generationsCount).ToString();
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
    }
}
