using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace Server
{
    public class Network
    {
        private readonly int port = 8080; // Lắng nghe trên port 8080
        private TcpListener tcpListener = null;
        public bool IsListening { get; set; }

        public Network()
        {
            this.IsListening = true;
        }

        public void Run()
        {
            try
            {
                // Lắng nghe trên tất cả các địa chỉ IP và cổng 8080
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                Console.WriteLine($"Server đang lắng nghe trên cổng {port}...");

                while (IsListening)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    Thread clientThread = new Thread(() => Listen(client));
                    clientThread.Start();
                    Console.WriteLine($"Chấp nhận kết nối từ {client.Client.RemoteEndPoint}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server - run(): {ex.Message}");
            }
        }

        public void Listen(TcpClient client)
        {
            StreamReader sr = new StreamReader(client.GetStream());

            try
            {
                while (IsListening && client.Connected)
                {
                    string recvMsg = sr.ReadLine();

                    if (string.IsNullOrEmpty(recvMsg)) continue;

                    string[] msgPayload = recvMsg.Split('|');
                    int code = int.Parse(msgPayload[0]);

                    // Đăng nhập hoặc vào hệ thống với tên
                    if (code == 0)
                    {
                        string user = msgPayload[1]; // User name
                        string playerID = Guid.NewGuid().ToString();  // Tạo PlayerID duy nhất cho mỗi người chơi

                        // Lưu kết nối dựa trên TcpClient thay vì tên
                        Game.currentTCPs.Add(playerID, client);
                        Game.currentUsers.Add(playerID, new Player(user));  // Thêm người chơi vào danh sách với ID

                        sendMsg(0, playerID, "success");  // Gửi ID về client để client sử dụng trong các request tiếp theo
                        Console.WriteLine($"User {user} (ID: {playerID}) đã đăng nhập.");
                    }
                    // Tạo hoặc vào phòng
                    else if (code == 1)
                    {
                        string playerID = msgPayload[1]; // Lấy ID người chơi từ client
                        string roomID = msgPayload[2];
                        bool isPrivate = bool.Parse(msgPayload[3]); // 'public' hoặc 'private'

                        if (string.IsNullOrEmpty(roomID))  // Tạo phòng mới
                        {
                            roomID = Game.RandomRoomID();
                            Room room = new Room(roomID, playerID, isPrivate);
                            Game.rooms.Add(roomID, room);
                            Console.WriteLine($"Player {playerID} tạo phòng {roomID} ({isPrivate}).");
                            sendMsg(1, playerID, roomID); // Gửi RoomID về cho client
                        }
                        else  // Vào phòng với RoomID đã tồn tại
                        {
                            if (Game.rooms.ContainsKey(roomID))
                            {
                                Room room = Game.rooms[roomID];
                                if (!room.IsFull)
                                {
                                    room.AddPlayer(playerID, Game.currentUsers[playerID]);  // Thêm người chơi dựa vào ID
                                    Console.WriteLine($"Player {playerID} đã vào phòng {roomID}.");
                                    sendToRoom(1, roomID, $"{Game.currentUsers[playerID].UserName} đã vào phòng.");  // Gửi tên người chơi
                                }
                                else
                                {
                                    sendMsg(1, playerID, "failed"); // Phòng đầy
                                }
                            }
                            else
                            {
                                sendMsg(1, playerID, "failed"); // Phòng không tồn tại
                            }
                        }
                    }
                    // Lấy thông tin bản đồ người chơi
                    else if (code == 2)
                    {
                        string user = msgPayload[1];
                        string roomID = msgPayload[2];

                        getPlayer(user, roomID); // Gọi hàm getPlayer để lấy bản đồ
                        Console.WriteLine($"Player {user} đã sẵn sàng.");

                        // Gửi thông báo tới phòng về lượt người chơi
                        sendToRoom(2, roomID, Game.rooms[roomID].CurrentTurn);
                    }
                    // Xử lý tấn công
                    else if (code == 3)
                    {
                        string roomID = msgPayload[1].Split(':')[0];
                        string attackerID = msgPayload[1].Split(':')[1];  // Dùng PlayerID thay vì tên

                        var coor = msgPayload[2].Split(':');
                        int x = int.Parse(coor[0]);
                        int y = int.Parse(coor[1]);

                        int shipLength = Game.PerformAttack(x, y, roomID, attackerID);  // Tấn công và nhận kết quả

                        sendMove(3, attackerID, roomID, x, y, shipLength);  // Gửi kết quả tấn công cho tất cả người chơi trong phòng

                        Game.rooms[roomID].ChangePlayerTurn(attackerID, shipLength);  // Đổi lượt chơi
                        sendToRoom(2, roomID, Game.rooms[roomID].CurrentTurn);  // Gửi thông tin lượt chơi cho người chơi tiếp theo

                        Console.WriteLine($"Player {Game.currentUsers[attackerID].Name} (ID: {attackerID}) đã tấn công tại {x}:{y}:{shipLength}.");
                    }
                    // Trạng thái người chơi (đã sẵn sàng hay chưa)
                    else if (code == 6)
                    {
                        string roomID = msgPayload[1];
                        string user = msgPayload[2];

                        Room room = Game.rooms[roomID];
                        int index = room.Players.Keys.ToList().IndexOf(user);
                        room.isPlaying[index] = true;

                        if (!room.isPlaying.Contains(false))
                        {
                            sendToRoom(6, roomID); // Gửi thông báo tới phòng rằng tất cả đã sẵn sàng
                        }
                    }
                    // Người chơi rời phòng
                    else if (code == 7)
                    {
                        string roomID = msgPayload[1];
                        string playerID = msgPayload[2];

                        Room room = Game.rooms[roomID];
                        room.RemovePlayer(playerID);
                        Console.WriteLine($"Player {playerID} đã rời phòng {roomID}.");
                        if (room.Players.Count == 0)
                        {
                            Game.rooms.Remove(roomID);
                            Console.WriteLine($"Phòng {roomID} đã bị xóa vì không còn người chơi.");
                        }
                        else
                        {
                            sendToRoom(7, roomID, $"{Game.currentUsers[playerID].Name} đã rời phòng."); // Gửi thông báo tới người chơi khác
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error at: FromClient()");
                client.Close();
                sr.Close();
            }
        }

        // Gửi tin nhắn tới một client
        private void sendMsg(int code, string playerID, string msg, string msg1 = "")
        {
            string formattedMsg = $"{code}|{playerID}|{msg}|{msg1}";

            StreamWriter sw = new StreamWriter(Game.currentTCPs[playerID].GetStream()) { AutoFlush = true };
            if (sw != null)
            {
                sw.WriteLine(formattedMsg);
            }
        }

        // Gửi kết quả tấn công cho cả phòng
        private void sendMove(int code, string from, string roomID, int x, int y, int length)
        {
            string formattedMsg = $"{x}:{y}:{length}";
            sendToRoom(code, $"{roomID}:{from}", formattedMsg);
        }

        // Gửi tin nhắn tới tất cả người chơi trong phòng
        private void sendToRoom(int code, string roomID_And_User, string msg = "")
        {
            string formattedMsg = $"{code}|{roomID_And_User}|{msg}";

            string roomID = roomID_And_User.Split(':')[0];

            foreach (string playerID in Game.rooms[roomID].Players.Keys)
            {
                StreamWriter sw = new StreamWriter(Game.currentTCPs[playerID].GetStream()) { AutoFlush = true };
                if (sw != null)
                {
                    sw.WriteLine(formattedMsg);
                }
            }
        }

        private void getPlayer(string username, string roomID)
        {
            // Tạo StreamReader để đọc dữ liệu từ TCP stream
            StreamReader sr = new StreamReader(Game.currentTCPs[username].GetStream());

            // Đọc chuỗi JSON từ luồng
            string json = sr.ReadLine();

            // Giải mã chuỗi JSON thành mảng 2D
            int[,] playerShipSet = JsonSerializer.Deserialize<int[,]>(json);

            // Cập nhật thông tin người chơi
            Player player = Game.currentUsers[username];
            player.SetShipSet(playerShipSet);

            // Thêm người chơi vào phòng
            Room room = Game.rooms[roomID];
            room.AddPlayer(username, player);
        }

    }
}
