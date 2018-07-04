using Godot;

namespace SpaceShooter.Application
{
    public class Explosion : Node2D
    {
        [Signal]
        public delegate void Destroyed();
        
        public override void _Ready()
        {
            var animatedSprite = (AnimatedSprite) GetNode("AnimatedSprite");
            animatedSprite.Play();
            animatedSprite.Connect("animation_finished", this, nameof(DestroyItself));
        }

        public void DestroyItself()
        {
            EmitSignal("Destroyed");
            QueueFree();
        }
    }
}