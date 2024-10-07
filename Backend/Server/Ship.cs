namespace Server
{
    [Serializable]
    public class Ship
    {
        public int cellX;
        public int cellY;

        public int Length;

        private bool isHorizontal;

        public Ship(int x, int y, int l, bool _is)
        {
            this.cellX = x;
            this.cellY = y;

            this.Length = Game.shipLengths[l];
            this.isHorizontal = _is;
        }
    }
}
