using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameOfLife
{
    class VisualCell : Cell
    {
        public Brush DeadBrush = Brushes.White;
        public Brush LiveBrush = Brushes.Green;

        public VisualCell(CellState state) : base(state)
        {
            Rectangle = new Rectangle();
            Rectangle.Fill = CurrentState == CellState.Alive ? LiveBrush : DeadBrush;
           
        }

        public override void NextRound()
        {
            base.NextRound();
            Rectangle.Fill = CurrentState == CellState.Alive ? LiveBrush : DeadBrush;
        }

        public Rectangle Rectangle { get; set; }
    }
}
