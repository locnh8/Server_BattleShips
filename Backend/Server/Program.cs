using System;
using System.Threading;

namespace Server
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Tạo đối tượng Network (server)
            Network server = new Network();

            // Tạo một luồng riêng để chạy server
            Thread serverThread = new Thread(new ThreadStart(server.Run));
            serverThread.Start();

            // Hiển thị thông báo cho người dùng biết server đã khởi động
            Console.WriteLine("Server đang chạy. Nhấn phím bất kỳ để dừng...");

        }
    }
}
