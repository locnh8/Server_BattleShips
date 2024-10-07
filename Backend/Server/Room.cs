namespace Server
{
    public class Room
    {
        public string RoomID { get; private set; } // ID của phòng

        public Dictionary<string, Player> Players { get; } // Thay đổi tên thành Players cho rõ ràng

        public string CurrentTurn { get; private set; } // Người chơi hiện tại

        public bool IsFull => Players.Count == 2; // Phòng đầy nếu có 2 người chơi

        public List<bool> isPlaying { get; set; }

        public bool IsPrivate { get; private set; }

        public Room(string id, string userName, bool isPrivate)
        {
            this.RoomID = id;
            this.Players = new Dictionary<string, Player>();
            this.IsPrivate = isPrivate; // Gán giá trị cho thuộc tính IsPrivate
            AddPlayer(userName, new Player(userName)); // Thêm người chơi vào phòng

            isPlaying = new List<bool> { false, false }; // Trạng thái chơi ban đầu
            CurrentTurn = userName; // Người chơi đầu tiên là người tạo phòng
        }

        public void AddPlayer(string playerName, Player player)
        {
            if (Players.ContainsKey(playerName))
            {
                Players[playerName] = player; // Cập nhật nếu người chơi đã tồn tại
            }
            else
            {
                Players.Add(playerName, player); // Thêm người chơi mới
            }
        }

        public void RemovePlayer(string playerName)
        {
            if (Players.ContainsKey(playerName))
            {
                Players.Remove(playerName); // Xóa người chơi
            }
        }

        public void ChangePlayerTurn(string lastTurn, int hit)
        {
            // Nếu không có cú đánh, chuyển lượt cho người khác
            if (hit == -1)
            {
                foreach (var playerName in Players.Keys)
                {
                    if (playerName != lastTurn)
                    {
                        CurrentTurn = playerName; // Chuyển lượt cho người chơi còn lại
                        break; // Ra khỏi vòng lặp sau khi tìm thấy người chơi tiếp theo
                    }
                }
            }
            else
            {
                // Nếu có cú đánh, giữ nguyên lượt
                CurrentTurn = lastTurn;
            }
        }
    }
}
