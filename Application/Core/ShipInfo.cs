using Godot;
using static Godot.GD;

namespace SpaceShooter.Application.Core
{
    public class ShipInfo
    {
        public string Type;
        public Vector2 Position;
        public Vector2 Dimensions;

        public ShipInfo(string type, Vector2 position, Vector2 dimensions)
        {
            Type = type;
            Position = position;
            Dimensions = dimensions;
        }

        public Vector2 BulletPosition(Vector2 offset = new Vector2())
        {
            var bulletPosition = new Vector2();
            
            switch(Type)
            {
                case "player":
                    bulletPosition = new Vector2(
                        Position.x + Dimensions.y,
                        Position.y + Dimensions.x / 2 - offset.x / 2
                    );
                    break;
                case "enemy":
                    bulletPosition = new Vector2(
                        Position.x - Dimensions.x / 2 - offset.y,
                        Position.y - offset.x / 2
                    );
                    break;
            }

            return bulletPosition;
        }
    }
}