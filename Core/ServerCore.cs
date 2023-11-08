using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cluster_Procesamiento.Models;
using Newtonsoft.Json;

namespace Cluster_Procesamiento.Core
{
    public  class ServerCore
    {
        private readonly static ServerCore _instance = new ServerCore();
        private string? _serverIP;
        
        private int _maxConnections;
        private TcpListener? _server;
        private List<Connection> _connections = new List<Connection>();

        // datos conexiones 
        private TcpClient? _client;
        private Connection _newConnection = new Connection();

        private bool _isRunning = false;

        private const int _SERVERPORT = 6969;

        private ServerCore()
        {
            _serverIP = "";
        }

        public static ServerCore Instance
        {
            get { return _instance; }
        }


        // PARA INICIAR SERVIDOR
        public async void InitializeLocalServer(string ip)
        {
            _serverIP = ip;


            _server = new TcpListener(IPAddress.Parse(_serverIP), _SERVERPORT);
            _server.Start(1);

            Console.WriteLine($"Server listo y esperando en: {_serverIP}:{_SERVERPORT}");

            while (true)
            {
                _client = await _server.AcceptTcpClientAsync();
                // guardamos la conexión con sus datos

                _newConnection = new Connection();
                _newConnection.Stream = _client.GetStream();
                _newConnection.StreamWriter = new StreamWriter(_newConnection.Stream); // stream para enviar
                _newConnection.StreamReader = new StreamReader(_newConnection.Stream); // stream para recibir


                // confirmamos el nombre

                var dataReceived = _newConnection.StreamReader!.ReadLine();
                var message = JsonConvert.DeserializeObject<Message>(dataReceived!);

                Thread thread = new Thread(ListenToConnection);
                thread.Start();

            }
        }

        public async void ListenToConnection()
        {

            Connection connection = _newConnection;
            var connectionOpen = true;

            while (connectionOpen)
            {
                try
                {

                    var dataReceived = await connection.StreamReader!.ReadLineAsync();
                    var message = JsonConvert.DeserializeObject<Message>(dataReceived!);

                    if (message!.Type == MessageType.Processor)
                    {

                    }
                }
                catch
                {

                    // Desconexion

                    

                    connectionOpen = false;
                }
            }
        }


        public void StopServer()
        {
            if (_isRunning)
            {
                _server!.Stop();
                _isRunning = false;
            }
        }
    }
}
