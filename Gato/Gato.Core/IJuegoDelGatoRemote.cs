using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Gato.Core
{
    public interface IJuegoDelGatoRemote
    {
        event EventHandler<ActualizarCasillaEventArgs> casillaActualizada;

        void changeCasilla(int columna, int fila, int id);

    }
}
