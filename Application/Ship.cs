using Godot;
using System;
using SpaceShooter.Application.Core;
using static Godot.GD;

namespace SpaceShooter.Application
{
    public class Ship : Area2D
    {
        [Signal]
        public delegate void WeaponDischarged();

        public int Speed = 500;
        public int Health = 100;
        public int CrashDamage = 20;

        private const string ShipType = "player";
        private Vector2 _windowSize;
        private AnimatedSprite _animatedSprite;
        
        public override void _Ready()
        {
            _windowSize = GetViewport().Size;
            _animatedSprite = (AnimatedSprite) GetNode("AnimatedSprite");
            
            // setup default position
            // since we rotated our sprite, we need to use the width instead of the height
            // when trying to center our sprite vertically
            Position = new Vector2(20, _windowSize.y / 2 - GetDimensions().x / 2);

            Connect("area_entered", this, "ShipDamaged");
        }

        public override void _Process(float delta)
        {
            HandleShipMovement(delta);
            HandleWeaponDischarge();
        }

        public ShipInfo ShipInfo()
        {
            return new ShipInfo(ShipType, Position, GetDimensions());
        }

        /// <summary>
        /// Get the dimensions of animation texture of the first frame of said animation
        /// </summary>
        /// <returns>Vector2</returns>
        public Vector2 GetDimensions(string animation = "red")
        {
            var currentFrame = _animatedSprite?.Frames?.GetFrame(animation, 0);
            if(currentFrame != null)
            {
                return currentFrame.GetSize();
            }
            
            return new Vector2();
        }

        /// <summary>
        /// Ship takes bullet damage, decrease the health
        /// </summary>
        /// <param name="obj"></param>
        public void ShipDamaged(object obj)
        {
            switch(obj)
            {
                case Bullet bullet:
                    ShipBulletDamage(bullet);
                    break;
                case Enemy enemy:
                    ShipCrashDamage(enemy);
                    break;
            }
            
            Print(Health);
        }

        public void ShipBulletDamage(Bullet bullet)
        {
            if(bullet.ShipType() == "player") return;

            Health -= bullet.Damage;
            
            bullet.QueueFree();
        }

        public void ShipCrashDamage(Enemy enemy)
        {
            Health -= enemy.CrashDamage;
            enemy.Health -= CrashDamage;
        }
        
        /// <summary>
        /// Handle the input for moving our ship
        /// </summary>
        /// <param name="delta"></param>
        private void HandleShipMovement(float delta)
        {
            var direction = new Vector2();

            if(Input.IsKeyPressed((int) KeyList.W))
            {
                direction.y -= Speed * delta;
            }
            
            if(Input.IsKeyPressed((int) KeyList.S))
            {
                direction.y += Speed * delta;
            }
            
            if(Input.IsKeyPressed((int) KeyList.A))
            {
                direction.x -= Speed * delta;
            }
            
            if(Input.IsKeyPressed((int) KeyList.D))
            {
                direction.x += Speed * delta;
            }
            
            //set new position
            Position += direction;
            
            //clamp position so ship isn't going outside viewport
            ClampShipToViewport();
        }

        /// <summary>
        /// Here we set a project input map to the spacebar to capture the input only once because it's simpler than the
        /// alternative which I may show in a future tutorial.
        /// </summary>
        private void HandleWeaponDischarge()
        {
            if(Input.IsActionJustPressed("ui_spacebar")) 
            {
                EmitSignal("WeaponDischarged");
            }
        }

        /// <summary>
        /// This keeps the ship inside the viewport so it won't ever go off the screen
        /// </summary>
        private void ClampShipToViewport()
        {
            var dimensions = GetDimensions();
            var maxXAxis = _windowSize.x - dimensions.y;
            var maxYAxis = _windowSize.y - dimensions.x;
            
            var position = new Vector2(
                Mathf.Clamp(Position.x, 0, maxXAxis),
                Mathf.Clamp(Position.y, 0, maxYAxis)
            );

            Position = position;
        }
    }
}