using System.Collections.Generic;

namespace SpaceShooter.Application.Core
{
    public class Spawn
    {
        public readonly List<int> Points = new List<int>();

        public Spawn(int col1 = 0, int col2 = 0, int col3 = 0, int col4 = 0)
        {
            Points.Add(col1);
            Points.Add(col2);
            Points.Add(col3);
            Points.Add(col4);
        }
    }
}