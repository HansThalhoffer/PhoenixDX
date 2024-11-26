using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Structures
{
    enum Direction 
    {
        NW = 0, NO =1, O=2 , SO=3, SW=4, W=5
    }
    public class KleinfeldAdorner
    {
        public KleinfeldAdorner()
        {
        }

        int[] _value = {0,0,0,0,0,0};

        public KleinfeldAdorner(int? NW, int? NO, int? O, int? SO, int? SW, int? W)
        {
            _value[ (int)Direction.NW] = (int)NW;  
            _value[ (int)Direction.NO] = (int)NO;
            _value[ (int)Direction.O] = (int)O;
            _value[ (int)Direction.SO] = (int)SO;
            _value[ (int)Direction.SW] = (int)SW;
            _value[ (int)Direction.W] = (int)W;
        }

        int HasDirection(Direction direction)
        {
            return _value[(int)direction];
        }
        int SetDirection(Direction direction, int? value)
        {
            return _value[(int)direction] = (int)value;
        }
    }
}
