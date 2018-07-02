using System;
using System.Collections.Generic;
using Godot;
using SpaceShooter.Application.Core;
using static Godot.GD;

namespace SpaceShooter.Application
{
    public class Enemy : Area2D
    {
        [Signal]
        public delegate void Destroyed(int points);
        
        public int Speed = 250;
        public int Health = 40;
        public int CrashDamage = 20;
        public readonly int Points = 10;

        private const string ShipType = "enemy";
        private string _baseFrame = "black";
        private string[] _enemyColors = {"black", "blue", "green", "orange"};

        private float _bulletSpeed = 0.200f;
        private float _bulletTimePassed = 0f;

        private int _bulletsSpawned = 0;
        private int _bulletsMaxSpawn = 2;
        private bool _bulletSpawnerPaused = false;
        private float _bulletSpawnerWaitTime = 2f;
        private float _bulletSpawnerTimePassed = 0f;

        private PackedScene _bulletObject = new PackedScene();
        
        public override void _Ready()
        {
            // set random enemy color on spawn
            var enemyColor = GameScene.Rand.Next(0, _enemyColors.Length);
            var animatedSprite = (AnimatedSprite) GetNode("AnimatedSprite");
            animatedSprite.Animation = _enemyColors[enemyColor];
            
            _bulletObject = (PackedScene) ResourceLoader.Load("res://Scenes/Objects/Bullet.tscn");

            Connect("area_entered", this, "EnemyDamaged");
        }

        public override void _Process(float delta)
        {
            // delete itself
            if(DeletedItself())
            {
                return;
            }

            BulletSpawner(delta);
            
            // we use negative so the ship goes left instead of right
            var direction = new Vector2(-Speed * delta, 0);
            Position += direction;
        }

        public ShipInfo ShipInfo()
        {
            return new ShipInfo(ShipType, Position, GetDimensions());
        }

        private bool DeletedItself()
        {
            // enemies are centered by default so we take the center point + half the width to get the actual 
            // right end side of the point since we're moving the enemy object off the screen to the left
            var position = Position.x + GetDimensions().x / 2;
            if(position < 0)
            {
                QueueFree();
                return true;
            }

            if(Health <= 0)
            {
                QueueFree();
                EmitSignal("Destroyed", Points);
                return true;
            }

            return false;
        }

        public void EnemyDamaged(object obj)
        {
            if(!(obj is Bullet bullet)) return;
            
            // stop hitting yourself
            if(bullet.ShipType() == "enemy") return;
            
            // deal damage
            Health -= bullet.Damage;
                
            // destroy bullet
            bullet.QueueFree();
        }

        private void BulletSpawner(float delta)
        {
            // we need to check if we're chilling for now to space the bullets out
            if(_bulletSpawnerPaused)
            {
                _bulletSpawnerTimePassed += delta;
                
                if(!(_bulletSpawnerTimePassed >= _bulletSpawnerWaitTime)) return;

                _bulletTimePassed = 0;
                _bulletSpawnerPaused = false;
                _bulletSpawnerTimePassed = 0;

                return;
            }
            
            // used to space out our bullets, so they don't stack
            _bulletTimePassed += delta;
            
            if(!(_bulletTimePassed >= _bulletSpeed)) return;
            
            SpawnBullet();
                
            _bulletsSpawned += 1;
            _bulletTimePassed = 0;

            if(_bulletsSpawned == _bulletsMaxSpawn)
            {
                _bulletsSpawned = 0;
                _bulletSpawnerPaused = true;
            }
        } 
       
        private void SpawnBullet()
        {
            var bullet = (Bullet) _bulletObject.Instance();
            bullet.Init(ShipInfo(), "left");
            
            // keep a copy of the instance to make it easier to delete the bullet from memory
            // cool trick we do is when we're in a different instance we can add bullets to another scene child
            // in this case we're in the GameScene and adding the bullets to the bullets container node
            GetNode("../../Bullets").AddChild(bullet);
        }
        
        private Vector2 GetDimensions()
        {
            var animatedSprite = (AnimatedSprite) GetNode("AnimatedSprite");
            var texture = animatedSprite?.Frames?.GetFrame(_baseFrame, 0);

            if(texture == null)
            {
                throw new NullReferenceException("Invalid Texture");
            }

            return texture.GetSize();
        }
    }
}