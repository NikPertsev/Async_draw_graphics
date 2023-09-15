using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace СерверДанныеГрфиков
{
    class Program
    {
        static void Main(string[] args)
        {
            const string ip = "127.0.0.1";
            const int port = 8080;
            const int porttcp = 8088;
            #region UDP
            var endpoint = new IPEndPoint(IPAddress.Parse(ip),port);
            var SocketEP = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
            SocketEP.Bind(endpoint);
            Console.WriteLine("UDP Сервер на прослушивании данных:");
            Thread t1 = new(()=>
            {
                    while (true)
                    {
                        EndPoint SenderEP = new IPEndPoint(IPAddress.Any, 0);
                        var buffer = new byte[256];
                        var size = 0;
                        var data = new StringBuilder();
                        do
                        {
                            size = SocketEP.ReceiveFrom(buffer, ref SenderEP);
                            data.Append(Encoding.UTF8.GetString(buffer), 0, size);
                        }
                        while (SocketEP.Available > 0);
                        WriteData(data);
                    };
            });
            t1.Start();
            #endregion
            #region TCP тестовая отправка
            var TCPendpoint = new IPEndPoint(IPAddress.Parse(ip), porttcp);

            var TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TCPSocket.Bind(TCPendpoint);
            TCPSocket.Listen(3);
            Console.WriteLine("TCP сервер на прослушивании:");
            var lsn = TCPSocket.Accept();
            while (true)
            {
                var buffer = new byte[256];
                var size = 0;
                var data1 = new StringBuilder();
                do
                {
                    size = lsn.Receive(buffer);
                    data1.Append(Encoding.UTF8.GetString(buffer), 0, size);
                }
                while (TCPSocket.Available > 0);
                WriteData(data1);
                lsn.Send(Encoding.UTF8.GetBytes("Данные сервером получены"));
            }
            #endregion
            void WriteData(StringBuilder data)
                {
                    Console.WriteLine(data);
                }
            
        }
    }
}
