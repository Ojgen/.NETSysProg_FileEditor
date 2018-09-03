using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hooks {
    public delegate void KeysEventHandler(object sender, KeysEventArgs e);
    public class KeysEventArgs {
        public KeysEventArgs(bool ctrl, bool alt, bool shift, IEnumerable<Key> keys, bool isDown) {
            Ctrl = ctrl;
            Alt = alt;
            Shift = shift;
            Keys = keys.ToArray();

            IsDown = isDown;
            IsUp = !isDown;
        }

        public bool IsDown { get; private set; }
        public bool IsUp { get; private set; }

        public bool Ctrl { get; private set; }
        public bool Alt { get; private set; }
        public bool Shift { get; private set; }
        public Key[] Keys { get; private set; }
    }
}
