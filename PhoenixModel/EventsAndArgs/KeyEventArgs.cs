using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.EventsAndArgs.MapEventArgs;

namespace PhoenixModel.EventsAndArgs {
    public class KeyEventArgs {
        public enum KeyState {
            Down = 0,
            Up = 1
        }

        public KeyState State;
        public int Key;
        public KeyEventArgs(int key, KeyState state) {
            Key = key;
            State = state;
        }
    }
}
