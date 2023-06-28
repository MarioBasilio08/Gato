using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Tcp;
using System.Xml.Linq;
using System.Net.Mime;
using System.Windows.Controls;
using System.Security.Policy;

namespace Gato.Core
{
    public class PlayerMove : MarshalByRefObject, IPlayerMove, IJuegoDelGatoRemote, IGameWinner
    {
        private TcpServerChannel serverChannel;
        private ObjRef internalRef;
        private bool serverActive = false;
        private static int tcpPort = 9096;
        private static string serverUri = "GatoGame";

        public event EventHandler<PlayerArgs> changeGame;
        //private Random _random = new Random();


        public event EventHandler<ActualizarCasillaEventArgs> casillaActualizada;


        public static PlayerArgs[] users = new PlayerArgs[2];

        public static string[,] point = new string[3, 3];

        public event EventHandler<PlayerArgs> playerWinner;

        public static bool winnerFlag = false;
        public PlayerMove()
        {

        }

        public void Initialize()
        {
            if (serverActive)
                return;

            Hashtable props = new Hashtable();
            props["port"] = tcpPort;
            props["name"] = serverUri;

            BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
            serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

            serverChannel = new TcpServerChannel(props, serverProv);


            try
            {
                ChannelServices.RegisterChannel(serverChannel, false);
                internalRef = RemotingServices.Marshal(this, props["name"].ToString());
                serverActive = true;
                Console.WriteLine("Manager initialized...");
                Console.WriteLine($"In tcp://localhost:{tcpPort}/{serverUri}");

            }
            catch (RemotingException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error could not start the server {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error could not start the server {ex.Message}");
            }

        }

        public void Uninitialize()
        {
            if (!serverActive) return;

            RemotingServices.Unmarshal(internalRef);

            try
            {
                ChannelServices.UnregisterChannel(serverChannel);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error to uninitialize {ex.Message}");
            }
        }

        public void Register(string name, int clientId)
        {
            Console.WriteLine($"El jugador: {name} se unio al juego");
            //Random random = new Random();

            if (users[0] == null || users[1] == null)
            {
                
                if (users[0] == null )
                {
                    users[0] = new PlayerArgs() { Id = clientId, Figu = "X", Name = name };
                    changeGame?.Invoke(this, users[0]);
                }
                else if (users[1] == null)
                {
                    users[1] = new PlayerArgs() { Id = clientId, Figu = "O", Name = name };
                    changeGame?.Invoke(this, users[1]);
                }
            }
        }


        public void changeCasilla(int columna, int fila, int id)
        {
            if (!winnerFlag)
            {
                Console.WriteLine($"Movimiento en: {columna} {fila}");
                string contenido = null;

                if (id == users[0].Id)
                {
                    contenido = users[0].Figu;
                }
                else if (id == users[1].Id)
                {
                    contenido = users[1].Figu;
                }

                ActualizarCasillaEventArgs args;
                if (point[columna, fila] == null)
                {
                    point[columna, fila] = contenido;
                    args = new ActualizarCasillaEventArgs()
                    {
                        Columna = columna,
                        Fila = fila,
                        Contenido = contenido
                    };
                }
                else
                {
                    args = new ActualizarCasillaEventArgs()
                    {
                        Columna = columna,
                        Fila = fila,
                        Contenido = point[columna, fila]
                    };
                }


                casillaActualizada?.Invoke(new object() { }, args);

                Winner();
            }
        }

        public void Winner()
        {
            // Comprobar tres en línea horizontal
            for (int fila = 0; fila < 3; fila++)
            {

                if (point[fila, 0] != null && point[fila, 0] == point[fila, 1] && point[fila, 0] == point[fila, 2])
                {
                    if (point[fila,0] == users[0].Figu)
                    {
                        playerWinner?.Invoke(new object() { }, users[0]);
                        Console.WriteLine($"¡Jugador { users[0].Name}  es el ganador!");

                    }else if(point[fila, 0] == users[1].Figu)
                    {
                        playerWinner?.Invoke(new object() { }, users[1]);
                        Console.WriteLine($"¡Jugador {users[1].Name}  es el ganador!");
                    }

                    winnerFlag = true;
                    return;
                }
            }

            // Comprobar tres en línea vertical
            for (int columna = 0; columna < 3; columna++)
            {
                if (point[0, columna] != null && point[0, columna] == point[1, columna] && point[0, columna] == point[2, columna])
                {
                     if (point[0,columna] == users[0].Figu)
                    {
                        playerWinner?.Invoke(new object() { }, users[0]);
                        Console.WriteLine($"¡Jugador { users[0].Name}  es el ganador!");

                    }
                    else if(point[0, columna] == users[1].Figu)
                    {
                        playerWinner?.Invoke(new object() { }, users[1]);
                        Console.WriteLine($"¡Jugador {users[1].Name}  es el ganador!");

                    }
                    winnerFlag = true;
                    return;
                }
            }

            // Comprobar tres en línea diagonal (de izquierda a derecha)
            if (point[0, 0] != null && point[0, 0] == point[1, 1] && point[0, 0] == point[2, 2])
            {
                if (point[0, 0] == users[0].Figu)
                {
                    playerWinner?.Invoke(new object() { }, users[0]);
                    Console.WriteLine($"¡Jugador {users[0].Name}  es el ganador!");

                }
                else if (point[0, 0] == users[1].Figu)
                {
                    playerWinner?.Invoke(new object() { }, users[1]);
                    Console.WriteLine($"¡Jugador {users[1].Name}  es el ganador!");
                }
                winnerFlag = true;
                return;
            }

            // Comprobar tres en línea diagonal (de derecha a izquierda)
            if (point[0, 2] != null && point[0, 2] == point[1, 1] && point[0, 2] == point[2, 0])
            {
                if (point[0, 2] == users[0].Figu)
                {
                    playerWinner?.Invoke(new object() { }, users[0]);
                    Console.WriteLine($"¡Jugador {users[0].Name}  es el ganador!");

                }
                else if (point[0, 2] == users[1].Figu)
                {
                    playerWinner?.Invoke(new object() { }, users[1]);
                    Console.WriteLine($"¡Jugador {users[1].Name}  es el ganador!");

                }
                winnerFlag = true;
                return;
            }

        }

    }
}
