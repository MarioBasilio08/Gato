using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gato.Core
{
    [Serializable]
    public class PlayerArgs : EventArgs
    {
        public int Id { get; set; }
        public string Figu { get; set; }

        public string Name { get; set; }
    }
}
