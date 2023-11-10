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

        private List<Connection> _connections = new List<Connection>();
        private TcpClient _client = new TcpClient();
        private Connection _connection = new Connection();
        private Connection _localConnection = new Connection();

        private string? _serverIP;
        private int _maxConnections;
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
        public void InitializeLocalServer(string ip)
        {
            _serverIP = ip;

            try
            {
                var entry = Dns.GetHostEntry(Dns.GetHostName());
                var ips = new List<string>();

                foreach (IPAddress ipvalue in entry.AddressList)
                    if (ipvalue.AddressFamily == AddressFamily.InterNetwork)
                        ips.Add(ipvalue.ToString());

                var localEndPoint = new IPEndPoint(IPAddress.Parse(ips[0]), 0);

                _localConnection.Port = 0;
                _localConnection.IpAddress = localEndPoint.Address.ToString();

                _client.Connect(_serverIP, _SERVERPORT);

                if (_client.Connected) Console.WriteLine($"Server listo y esperando en: {_serverIP}:{_SERVERPORT}");

                var message = new Message();
                message.Type = MessageType.Processor;
                message.Content = JsonConvert.SerializeObject(_localConnection);


                _connection = new Connection();
                _connection.Stream = _client.GetStream();
                _connection.StreamWriter = new StreamWriter(_connection.Stream); // stream para enviar
                _connection.StreamReader = new StreamReader(_connection.Stream); // stream para recibir

                _connection.StreamWriter.WriteLine(JsonConvert.SerializeObject(message));
                _connection.StreamWriter.Flush();

                Thread thread = new Thread(ListenToServer);
                thread.Start();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ListenToServer()
        {
            Connection connection = _connection;

            while (_client.Connected)
            {
                try
                {
                    var dataReceived = connection.StreamReader!.ReadLine();
                    var message = JsonConvert.DeserializeObject<Message>(dataReceived!);

                    

                    if (message!.Type == MessageType.Processor)
                    {

                    }
                }
                catch
                {


                }
            }
        }

        public void StopServer()
        {
            if (_isRunning)
            {
                _client!.Close();
                _isRunning = false;
            }
        }
    }
}
