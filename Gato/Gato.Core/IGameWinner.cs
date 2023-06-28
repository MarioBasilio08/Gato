using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gato.Core
{
    public interface IGameWinner
    {
        event EventHandler<PlayerArgs> playerWinner;
        void Winner();
    }
}
