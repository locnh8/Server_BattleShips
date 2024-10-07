using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace Server
{
    public static class Game
    {
        public static Dictionary<string, TcpClient> currentTCPs = new Dictionary<string, TcpClient>();
        public static Dictionary<string, Player> currentUsers = new Dictionary<string, Player>();

        public static Dictionary<string, Room> rooms = new Dictionary<string, Room>();

        // Ship lengths.
        public static int[] shipLengths = new int[5] { 2, 3, 3, 4, 5 };

        // Tạo Room ID ngẫu nhiên
        public static string RandomRoomID()
        {
            Random random = new Random();
            int id = random.Next(100, 9999);

            while (rooms.ContainsKey(id.ToString()))
            {
                id = random.Next(100, 9999);
            }

            return id.ToString();
        }

        // Thực hiện đợt tấn công vào người chơi khác
        public static int PerformAttack(int cellX, int cellY, string roomID, string attackerID)
        {
            Player attackedPlayer = GetOpponentPlayer(roomID, attackerID);

            if (attackedPlayer == null)
            {
                return -1;
            }

            // Đánh dấu ô đã bị bắn
            attackedPlayer.RevealedCells[cellX, cellY] = true;

            // Kiểm tra có trúng tàu hay không
            if (attackedPlayer.ShipSet[cellX, cellY] != -1)
            {
                // Giảm số ô còn lại của tàu bị trúng
                attackedPlayer.ShipLeftCells[attackedPlayer.ShipSet[cellX, cellY]]--;

                // Nếu tàu đã bị bắn chìm
                if (attackedPlayer.ShipLeftCells[attackedPlayer.ShipSet[cellX, cellY]] == 0)
                {
                    attackedPlayer.ShipsLeft--; // Giảm số tàu còn lại
                }

                return attackedPlayer.ShipSet[cellX, cellY]; // Trả về độ dài tàu bị tấn công
            }
            else
            {
                return -1; // Không trúng
            }
        }

        // Kiểm tra xem trò chơi đã kết thúc chưa
        public static bool IsEndGame(string roomID, string attackerID)
        {
            Player attackedPlayer = GetOpponentPlayer(roomID, attackerID);

            if (attackedPlayer == null)
            {
                return false;
            }

            // Kiểm tra nếu người chơi còn tàu hay không
            return attackedPlayer.ShipsLeft == 0;
        }

        // Lấy thông tin người chơi đối thủ trong phòng
        private static Player GetOpponentPlayer(string roomID, string attackerID)
        {
            Room room = rooms.ContainsKey(roomID) ? rooms[roomID] : null;

            if (room == null)
            {
                return null;
            }

            // Tìm đối thủ trong phòng (không phải người đang tấn công)
            foreach (var playerID in room.Players.Keys)
            {
                if (playerID != attackerID)
                {
                    return room.Players[playerID]; // Trả về đối tượng Player của đối thủ
                }
            }

            return null;
        }
    }
}
