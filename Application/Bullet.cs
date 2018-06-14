using Godot;
using SpaceShooter.Application.Core;

namespace SpaceShooter.Application
{
    public class Bullet : Area2D
    {
        public int Damage = 5;
        public int Speed = 600;

        private ShipInfo _shipInfo;
        private Sprite _sprite;

        private string _direction = "right";

        /// <summary>
        /// When we instance the bullet the bullet isn't ready until it's added to the scene so we have to
        /// so we can't get access to the bullet until it's ran in the scene so it's better that we pass the ship
        /// to the bullet and when the bullet gets added to the scene we properly set it up the position. That's just
        /// how instanced scenes work
        /// </summary>
        /// <param name="shipInfo"></param>
        /// <param name="direction"></param>
        public void Init(ShipInfo shipInfo, string direction = "right")
        {
            _shipInfo = shipInfo;
            _direction = direction;
        }

        public override void _Ready()
        {
            _sprite = (Sprite) GetNode("Sprite");
            SetupPosition();
        }

        public override void _Process(float delta)
        {
            var speed = Speed * delta;
            
            //move bullet across the screen
            switch(_direction)
            {
                case "right":
                    Position += new Vector2(speed, 0);
                    break;
                case "left":
                    Position += new Vector2(-speed, 0);
                    break;
            }
        }

        /// <summary>
        /// We setup the bullet position to match the ship's center point, since we rotated the bullet we use the normal
        /// width instead of the rotated width because that's how it's saved.
        /// </summary>
        private void SetupPosition()
        {
            var bulletPosition = _shipInfo.BulletPosition(GetSize());
            
            Position = new Vector2(
                bulletPosition.x,
                bulletPosition.y
            );
        }
        
        /// <summary>
        /// Returns the sprite texture size
        /// </summary>
        /// <returns>Vector2</returns>
        public Vector2 GetSize()
        {
            return _sprite.GetTexture().GetSize();
        }

        public string ShipType()
        {
            return _shipInfo.Type;
        }
    }
}