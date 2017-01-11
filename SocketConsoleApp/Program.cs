using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SocketConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("App started...");

            // get PORT assigned to app from PCF env var
            var port = Environment.GetEnvironmentVariable("PORT");
            if (string.IsNullOrEmpty(port))
            {
                port = "11000";
            }
            var localip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            if (localip == null)
            {
                Console.Error.WriteLine("Couldn't find IP address of container");
                Environment.Exit(1);
            }

            TcpListener server = new TcpListener(localip, int.Parse(port));

            server.Start();
            Console.WriteLine($"Server has started on {localip}:{port}.\r\nWaiting for a connection...");

            // loop forever and handle any connections
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                Console.WriteLine("A client connected.");

                NetworkStream stream = client.GetStream();

                while (!stream.DataAvailable) ;

                Byte[] bytes = new Byte[client.Available];

                stream.Read(bytes, 0, bytes.Length);

                //translate bytes of request to string
                String data = Encoding.UTF8.GetString(bytes);

                if (new Regex("^GET").IsMatch(data))
                {
                    Console.WriteLine("GET request");
                    var res = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
                    stream.Write(res, 0, res.Length);
                    stream.Close();
                }
                else
                {
                    Console.WriteLine("??? request");
                }
            }
        }
    }
}
