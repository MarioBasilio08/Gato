using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Gato.Core
{
    [Serializable]
    public class ActualizarCasillaEventArgs : EventArgs
    {
        public int Fila { get; set; }
        public int Columna { get; set; }
        public string Contenido { get; set; }

    }
}
