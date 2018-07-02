using Godot;
using System;
using static Godot.GD;

namespace SpaceShooter.Application
{
    public class Background : Node2D
    {
        protected float Speed = 300;
        protected Camera2D Camera;

        public override void _Ready()
        {
            Camera = (Camera2D) GetNode("Camera2D");
        }

        public override void _Process(float delta)
        {
            var speed = Speed * delta;
            Camera.Position = new Vector2(Camera.Position.x - speed, Camera.Position.y);
        }
    }
}