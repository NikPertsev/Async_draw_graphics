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
using System.Threading;
using System.Windows.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Асинхронное_рисование_графиков
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            lb.Items.Add($"Инициализация компонентов, поток [{Thread.CurrentThread.ManagedThreadId}]");
            TCPsocket.Bind(TCPendpoint);
            try
            {
                TCPsocket.Connect(TCPserver);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
            ListeningAsync();
        }
        public async void ListeningAsync()
        {
            while (true)
            {
                var buffer = new byte[256];
                var size = 0;
                var data = new StringBuilder();
                await Task.Run(() =>
                {
                    do
                    {
                        size = TCPsocket.Receive(buffer);
                        data.Append(Encoding.UTF8.GetString(buffer), 0, size);
                    }
                    while (TCPsocket.Available > 0);
                    Dispatcher.Invoke(() => { lb.Items.Add(data); });
                }
                );
            }
        }
        private void b1_Click(object sender, RoutedEventArgs e)
        {
            GrafDrawsAsync(500,4030,315, cv, im1);
            GrafDrawsAsync(500, 7030, 615, cv, im1);
            lb.Items.Add($"Конец события клика, поток [{Thread.CurrentThread.ManagedThreadId}]");
        }
        public static double Dist(double time, double draft, double mass)
        {
            return (draft * Math.Pow(time, 2)) / (mass * 2) - 9.81 * Math.Pow(time, 2) / 2;
        }
        public async void GrafDrawsAsync(object time,double dr, double m, Canvas cv, Image im) 
        {
            lb.Items.Add($"Вызов асинхронного метода, поток [{Thread.CurrentThread.ManagedThreadId}]");
            await Task.Run(() => GrafDraws(time,dr,m,cv,im));
            lb.Items.Add($"Конец работы асинхронного метода, поток [{Thread.CurrentThread.ManagedThreadId}]");
        }
        public void GrafDraws(object time, double draf, double mass, Canvas cv, Image im)
        {
            double x1=0, x2=0, y1=0, y2=0; int i1 = 0;
            for (double i = 0; i <= (int)time; i += 1)
            {
                i1++;
                if (i1 % 2 != 0)
                {
                    x1 = i;
                    y1 = Dist(i, draf, mass);
                }
                else
                {
                    x2 = i;
                    y2 = Dist(i, draf, mass);
                    if (y2 > 150000 || y2 < 0 || x2 > 500) break;
                }
                //Отправка сообщения серверу:
                //Локер на запись в txt и на отправку сообщений на сервер
                lock (locker)
                {
                    SendToServer($"Высота полета на {i}-й секунде: {y2}-м");
                    Thread.Sleep(5);
                    using (var dc = new StreamWriter("test.txt", true, Encoding.UTF8))
                    {
                        dc.WriteLine($"Поток [{Thread.CurrentThread.ManagedThreadId}]");
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    LineDraw(x1, x2, y1, y2, cv, im);
                    lb.Items.Add($"Выход из linedraw, поток [{Thread.CurrentThread.ManagedThreadId}]");
                });
            }
        }
        object locker = new object();
        public static void LineDraw(double x1, double x2, double y1, double y2, Canvas cv, Image im)
        {
            FormConv(ref x1, ref x2, ref y1, ref y2, im);
            Line x = new();
            x.Stroke = Brushes.Black; x.X1 = x1; x.X2 = x2; x.Y1 = y1; x.Y2 = y2; x.StrokeThickness = 1;
            cv.Children.Add(x);
        }
        public static void FormConv(ref double x1, ref double x2, ref double y1, ref double y2, Image im)
        {
            x1 = im.Height * 0.05 + 0.58 * x1; x2 = im.Height * 0.05 + 0.58 * x2;
            y1 = im.Height * 0.9417 - y1 / 850; y2 = im.Height * 0.9417 - y2 / 850;
        }

        private void b2_Click(object sender, RoutedEventArgs e)
        {
            lb.Items.Add($"Прочее взаимодействие, поток [{Thread.CurrentThread.ManagedThreadId}]");
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        }
        public void SendToServer(string inf)
        {
            //var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);


            SocketEP.SendTo(Encoding.UTF8.GetBytes(inf), ServerEP);
            //TCPsocket.Send(Encoding.UTF8.GetBytes(inf));
            //Через TCP, слишком много потоков
            //TCPsocket.Send(Encoding.UTF8.GetBytes(inf));
        }
        const string ip = "127.0.0.1";
        const int port2 = 8082;
        Socket SocketEP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        EndPoint ServerEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
        IPEndPoint TCPendpoint = new IPEndPoint(IPAddress.Parse(ip), port2);

        Socket TCPsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        EndPoint TCPserver = new IPEndPoint(IPAddress.Parse(ip), 8088);
        //EndPoint TCP2server = new IPEndPoint(IPAddress.Parse(ip), 8089);
        private void b3_Click(object sender, RoutedEventArgs e)
        {
            TCPsocket.Send(Encoding.UTF8.GetBytes("Тестовая отправка"));
        }
    }
}
