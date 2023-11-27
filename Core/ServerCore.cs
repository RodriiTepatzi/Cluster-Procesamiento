using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cluster_Procesamiento.Models;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;

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

		private Dictionary<(int, int), List<byte[]>> _processedImages = new Dictionary<(int, int), List<byte[]>>();

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

					if (message!.Type == MessageType.Data)
					{
						var jsonContent = message.Content as string;
                    
						message = null;
						GC.Collect();

						var data = JsonConvert.DeserializeObject<FramesData>(jsonContent!);

						message = new Message();
						jsonContent = "";

						jsonContent = data!.Content as string;

						data.Content = null;
						GC.Collect();

						var processedFrame = JsonConvert.DeserializeObject<byte[]>(jsonContent!);

						using var image = SixLabors.ImageSharp.Image.Load(processedFrame);

						image.Mutate(x => x.Grayscale());

						using var ms = new MemoryStream();
						image.SaveAsBmp(ms);
						var grayscaleFrame = ms.ToArray();

						if (!_processedImages.ContainsKey(data.Range))
						{
							_processedImages[data.Range] = new List<byte[]>();
                    }
						_processedImages[data.Range].Add(grayscaleFrame);
                }
					else if (message.Type == MessageType.EndOfData)
					{
						var orderedImages = _processedImages.OrderBy(pair => pair.Key.Item1)
									.SelectMany(pair => pair.Value)
									.ToList();

						for (int i = 0; i < orderedImages.Count; i++)
						{
							var image = orderedImages[i];
							var range = _processedImages.First(pair => pair.Value.Contains(image)).Key;

							var jsonContent = JsonConvert.SerializeObject(image);

							FramesData frameData = new FramesData
                {
								Range = range,
								Content = jsonContent
							};

							message.Content = JsonConvert.SerializeObject(frameData);
							message.Type = MessageType.ProcessedData;

							connection.StreamWriter!.WriteLine(JsonConvert.SerializeObject(message));
							connection.StreamWriter!.Flush();
						}

						connection.StreamWriter!.WriteLine(JsonConvert.SerializeObject(new Message() { Type = MessageType.EndOfData, Content = "Se ha procesado todo el contenido."}));
						connection.StreamWriter!.Flush();
					}
				}
				catch (Exception ex)
				{
					// Handle exception
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
