using Godot;

namespace SpaceShooter.Application
{
    public class Explosion : Node2D
    {
        public override void _Ready()
        {
            var animatedSprite = (AnimatedSprite) GetNode("AnimatedSprite");
            animatedSprite.Play();
            animatedSprite.Connect("animation_finished", this, "DestroyItself");
        }

        public void DestroyItself()
        {
            GD.Print("Deleted itself");
            QueueFree();
        }
    }
}