using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public enum CellState
    {
        Dead,
        Alive
    }
    
    public class Cell
    {
        public Cell()
        {
            Neightbours = new List<Cell>();
        }

        public Cell(CellState state) : this()
        {
            CurrentState = state;
        }

        public CellState CurrentState { get; private set; }
        public CellState FutureState { get; private set; }
        public List<Cell> Neightbours { get; private set; }

        public virtual void TakeTurn()
        {
            FutureState = CurrentState;
            var aliveNeghtbours = Neightbours.Count(c => c!=null && c.CurrentState == CellState.Alive);
            if (CurrentState == CellState.Dead)
            {
                if(aliveNeghtbours == 3)
                    FutureState = CellState.Alive;
            }
            else
            {
                if (aliveNeghtbours > 3 || aliveNeghtbours < 2)
                    FutureState = CellState.Dead;
            }
        }

        public virtual void NextRound()
        {
            CurrentState = FutureState;
        }

    }
}
