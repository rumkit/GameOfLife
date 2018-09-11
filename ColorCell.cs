using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GameOfLife
{
    public class ColorCell : Cell
    {
        private Color _color;
        private static Random Random = new Random(DateTime.Now.Millisecond);

        public Color Color
        {
            get => CurrentState == CellState.Alive ? _color : Color.Black;
            set => _color = value;
        }

        public ColorCell() : this(CellState.Dead)
        {
            
        }

        public ColorCell(CellState state) : base(state)
        {
//            var bytes = new byte[3];
//            Random.NextBytes(bytes);
//            _color = Color.FromArgb(0xFF, bytes[0], bytes[1], bytes[2]);
            _color = ColorFromHSV(Random.Next(1,255), 1,1);
        }

        private Color GetNewColor(IEnumerable<Color> colors)
        {
            var colorsArray = colors as Color[] ?? colors.ToArray();
//            int red = colorsArray.Sum(c => c.R) / colorsArray.Length;
//            int green = colorsArray.Sum(c => c.G) / colorsArray.Length;
//            int blue = colorsArray.Sum(c => c.B) / colorsArray.Length;
//            return Color.FromArgb(0xFF, red, green, blue);
            var hue = colorsArray.Sum(c => c.GetHue())/ colorsArray.Length;
            return ColorFromHSV(hue, 1, 1);
        }
        
        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
        
       

        public override void TakeTurn()
        {
            base.TakeTurn();
            if (CurrentState == CellState.Dead && FutureState == CellState.Alive)
            {
                Color = GetNewColor(Neightbours.Where(n => n.CurrentState == CellState.Alive).
                    Select(n => (ColorCell) n).
                    Select(n => n.Color));
            }
        }
    }
}