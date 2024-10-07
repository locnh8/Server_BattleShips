namespace Server

{
    public class Player
    {
        public string Name { get; private set; }
        // Lưu vị trí các con tàu của người chơi trên bản đồ 10x10
        public int[,] ShipSet { get; set; }

        // Các ô đã bị bắn và được tiết lộ
        public bool[,] RevealedCells { get; set; }

        // Vị trí ô bị bắn cuối cùng
        public int[] LastRevealedCells { get; set; }

        // Số ô còn lại của từng con tàu (các tàu có chiều dài khác nhau)
        public int[] ShipLeftCells { get; set; }

        // Số lượng tàu còn lại
        public int ShipsLeft { get; set; }

        // Thêm một thuộc tính tùy chọn để lưu trữ tên người chơi
        public string UserName { get; set; }

        // Khởi tạo người chơi với tên
        public Player(string userName)
        {
            this.UserName = userName; // Gán tên người chơi

            // Khởi tạo các tàu với số ô tương ứng còn lại
            ShipLeftCells = new int[] { 2, 3, 3, 4, 5 };
            ShipsLeft = 5;

            // Khởi tạo bản đồ vị trí tàu 10x10 và các ô bị bắn
            ShipSet = new int[10, 10];
            RevealedCells = new bool[10, 10];

            // Đặt tất cả các ô chưa bị bắn (chưa được tiết lộ)
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    RevealedCells[i, j] = false; // Tất cả các ô chưa bị tấn công
                }
            }
        }

        // Thiết lập vị trí các con tàu trên bản đồ (nhận vào mảng 10x10)
        public void SetShipSet(int[,] shipSet)
        {
            this.ShipSet = shipSet;
        }
    }
}
