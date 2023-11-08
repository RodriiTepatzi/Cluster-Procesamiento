using System.Text.RegularExpressions;

namespace Cluster_Procesamiento
{
	internal class Program
	{
        private static string? _ip = "";
		private static bool _ipIsValid = false;
        private static int _port = 6969;

        static void Main(string[] args)
		{
			
			Regex regex = new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");

			while (!_ipIsValid)
			{
                Console.WriteLine("Ingrese la IPv4 a usar");
                _ip = Console.ReadLine();

                if (string.IsNullOrEmpty(_ip))
                {
                    Console.Clear();
                    Console.WriteLine("Error: La IP esta vacia.");
                }
                else
                {
                    if (regex.IsMatch(_ip))
                    {
                        _ipIsValid = true;  
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Error: La IP tiene un formato incorrecto.");
                    }
                }
            }

            if (_ipIsValid)
            {
                Console.WriteLine($"Iniciando servidor de procesamiento en: {_ip}:6969");
            }
        }
	}
}