using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gato.Core
{
    public interface IPlayerMove
    {
        event EventHandler<PlayerArgs> changeGame;

        void Register(string name, int clientId);

    }
}
