using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections;
using Gato.Core;
using System.Data.Common;

namespace Gato.Client
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    /// 
    public class Client
    {
        private int ClientId { get;  set; }

        public int ObtenerId()
        {
            return ClientId;
        }

        public void ReceiveClientId(int clientId)
        {
            ClientId = clientId;
        }
    }
    public partial class MainWindow : Window
    {
        static IPlayerMove playerMove;
        static TcpChannel channel;

        static RemoteEvent<PlayerArgs> changeGame;
        static BinaryClientFormatterSinkProvider clientProv;
        static BinaryServerFormatterSinkProvider serverProv;

        private static string serverUri = "tcp://192.168.0.5:9096/GatoGame";
        private static bool connected = false;

        static IJuegoDelGatoRemote gatoRemote;
        static RemoteEvent<ActualizarCasillaEventArgs> changeInterface;

        static IGameWinner winnerPla;
        static RemoteEvent<PlayerArgs> winnerPlayer;

        private Client client = new Client();

        public string[,] casillas = new string[3, 3];

        private static MainWindow instance;
        public static MainWindow Instance { get { return instance; } }
        private Random _random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            instance = this;
        }

        private void ActualizarTextBlocks()
        {
            // Asignar los valores de las casillas a los TextBlock correspondientes
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Casilla00.Text = casillas[0, 0];
                Casilla01.Text = casillas[0, 1];
                Casilla02.Text = casillas[0, 2];
                Casilla10.Text = casillas[1, 0];
                Casilla11.Text = casillas[1, 1];
                Casilla12.Text = casillas[1, 2];
                Casilla20.Text = casillas[2, 0];
                Casilla21.Text = casillas[2, 1];
                Casilla22.Text = casillas[2, 2];
            }));
        }

        private void Button_Click_Conect(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextName.Text))
            {
                MessageBox.Show("Por favor ingrese su nombre de jugador.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                ////Manager mas completo
                clientProv = new BinaryClientFormatterSinkProvider();
                serverProv = new BinaryServerFormatterSinkProvider();
                serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

                //Manejo una clase que encapsula un evento para poder integrar logs de disparo o error
                changeGame = new RemoteEvent<PlayerArgs>();
                changeGame.eventToHandle += new EventHandler<PlayerArgs>(changeGameEvent);

                changeInterface = new RemoteEvent<ActualizarCasillaEventArgs>();
                changeInterface.eventToHandle += new EventHandler<ActualizarCasillaEventArgs>(changeCasillaEvent);

                winnerPlayer = new RemoteEvent<PlayerArgs>();
                winnerPlayer.eventToHandle += new EventHandler<PlayerArgs>(WinnerPlayerEvent);


                Hashtable props = new Hashtable();
                props["name"] = "remotingClient";
                props["port"] = 0;

                channel = new TcpChannel(props, clientProv, serverProv);
                ChannelServices.RegisterChannel(channel);

                RemotingConfiguration.RegisterWellKnownClientType(new WellKnownClientTypeEntry(typeof(IPlayerMove), serverUri));

                if (connected)
                    return;

                try
                {

                    playerMove = (IPlayerMove)Activator.GetObject(typeof(IPlayerMove), serverUri);
                    //Para encapsular el evento en una clase para manejar logs
                    playerMove.changeGame += new EventHandler<PlayerArgs>(changeGame.Notify);
                    //accountMove.changeAmount += changeAmountEvent;

                    int clientId = _random.Next();
                    client.ReceiveClientId(clientId);

                    playerMove.Register(TextName.Text, client.ObtenerId());

                    TextStatus.Content = $"Conectado {client.ObtenerId()}";
                    connected = true;
                    
                    gatoRemote = (IJuegoDelGatoRemote)Activator.GetObject(typeof(IJuegoDelGatoRemote), serverUri);
                    gatoRemote.casillaActualizada += new EventHandler<ActualizarCasillaEventArgs>(changeInterface.Notify);

                    winnerPla = (IGameWinner)Activator.GetObject(typeof(IGameWinner), serverUri);
                    winnerPla.playerWinner += new EventHandler<PlayerArgs>(winnerPlayer.Notify);

                }
                catch (Exception ex)
                {
                    connected = false;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Could not connect {ex.Message}");
                    TextStatus.Content = "Error";
                }
            }

        }

        private static void changeGameEvent(object sender, PlayerArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            
            Console.WriteLine("ChangeAmountEvent Fired");
        }

        private static void changeCasillaEvent(object sender, ActualizarCasillaEventArgs e)
        {
            string textBlockKey = $"Casilla{e.Columna}{e.Fila}"; // Identificador del TextBlock que deseas acceder
            MainWindow mainWindow = MainWindow.Instance;
            mainWindow.casillas[e.Columna, e.Fila] = e.Contenido;
            mainWindow.ActualizarTextBlocks();
        }


        private static bool Disconnect()
        {
            if (!connected) return true;
            playerMove.changeGame -= new EventHandler<PlayerArgs>(changeGame.Notify);
            ChannelServices.UnregisterChannel(channel);
            return false;
        }

        private void Button_Click_Diconect(object sender, RoutedEventArgs e)
        {
            if (!Disconnect())
                TextStatus.Content = "Desconectado";
        }


        private void Grid_Click(object sender, MouseButtonEventArgs e)
        {
            if (connected)
            {
                Grid grid = (Grid)sender;
                TextBlock textBlock = (TextBlock)grid.Children[0];
                string name = textBlock.Name.Substring(7,2);
                gatoRemote.changeCasilla(Int32.Parse(name.Substring(0, 1)), Int32.Parse(name.Substring(1, 1)), client.ObtenerId());
                // También puedes realizar otras acciones necesarias en respuesta al clic en el grid
            }
            else
            {
                MessageBox.Show("Por favor conectate al servidor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private static void WinnerPlayerEvent(object sender, PlayerArgs e)
        {
            MainWindow mainWindow = MainWindow.Instance;
            mainWindow.ActualizarGanador(e.Name);
        }

        private void ActualizarGanador(string name)
        {
            // Asignar los valores de las casillas a los TextBlock correspondientes
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TextStatus.Content = $"El ganador es {name}";
            }));
        }

    }
}
