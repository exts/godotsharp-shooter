using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using SpaceShooter.Application.Core;
using static Godot.GD;

namespace SpaceShooter.Application
{
    public class GameScene : Node2D
    {
        public static Random Rand = new Random();

        private Node _bullets;
        private Node _enemies;
        private Ship _ship;
        private Timer _formationTimer;
        private Timer _enemySpawnTimer;
        private Vector2 _viewport;
        private Formations _formations = new Formations();
        private PackedScene _enemyObject;
        private PackedScene _bulletObject;

        private int _score = 0;

        private bool _running = true;

        public override void _Ready()
        {
            _viewport = GetViewport().Size;
            
            SetupGameObjects();
            
            // select random monster spawn formation
            _formations.SelectRandomFormation();
        }

        public override void _Process(float delta)
        {
            DespawnBullets();
            SpawnIfNoEnemies();
        }

        private void SetupGameObjects()
        {
            // setup packed scenes used to spawn multiple objects from the same textures
            _enemyObject = (PackedScene) ResourceLoader.Load("res://Scenes/Objects/Enemy.tscn");
            _bulletObject = (PackedScene) ResourceLoader.Load("res://Scenes/Objects/Bullet.tscn");

            // get containers
            _bullets = GetNode("GameCanvas/Bullets");
            _enemies = GetNode("GameCanvas/Enemies");

            // connect the ship signal to a method inside our game scene, this would allow us to spawn bullets
            // into the scene after we press spacebar. Allowing us to keep our code as separated as possible. Also
            // allowing us to have bullets interact with other things like enemies in our case.
            _ship = (Ship) GetNode("GameCanvas/Ship");
            _ship.Connect("WeaponDischarged", this, "SpawnBullet");
            
            // setup monster spawner timer
            _enemySpawnTimer = (Timer) GetNode("GameCanvas/EnemySpawnTimer");
            _enemySpawnTimer.Connect("timeout", this, "SpawnEnemies");
            
            _formationTimer = (Timer) GetNode("GameCanvas/FormationTimer");
            _formationTimer.Connect("timeout", this, "SetNewFormation");
        }

        /// <summary>
        /// Signal callback when we press the spacebar inside the ship script which triggers
        /// bullet spawning.
        /// </summary>
        private void SpawnBullet()
        {
            var bullet = (Bullet) _bulletObject.Instance();
            
            bullet.Init(_ship.ShipInfo());
            
            //keep a copy of the instance to make it easier to delete the bullet from memory
            GetNode("GameCanvas/Bullets").AddChild(bullet);
        }

        private void DespawnBullets()
        {
            foreach(var child in _bullets.GetChildren().ToList())
            {
                if(child is Bullet bullet)
                {
                    if(bullet.Position.x > _viewport.x)
                    {
                        bullet.QueueFree();
                    }
                }
            }            
        }

        public void SetNewFormation()
        {
            // set next column
            _formations.NextColumn();
            
            // stop & start appropriate timers
            _formationTimer.Stop();
            _enemySpawnTimer.Start();
        }

        private void SpawnIfNoEnemies()
        {
            if(_enemies.GetChildCount() == 0 && _enemySpawnTimer.IsStopped() && _formations.IsEndOfColumn())
            {
                _formations.NextColumn();
                _formationTimer.Stop();
                _enemySpawnTimer.Start();
            }
        }

        public void SpawnEnemies()
        {
            var spawns = _formations.Spawns();
            for(var spawn = 0; spawn < spawns.Length; spawn++)
            {
                if(spawns[spawn] == 0) continue;
                
                var enemy = (Enemy) _enemyObject.Instance();
                enemy.Position = new Vector2(_formations.XPosition, _formations.Positions[spawn]);
                enemy.Connect("Destroyed", this, "EnemyDestroyed");
                
                GetNode("GameCanvas/Enemies").AddChild(enemy);
            }

            if(_formations.IsEndOfColumn())
            {
                _enemySpawnTimer.Stop();
                _formationTimer.Start();
            }
            else
            {
                _formations.NextColumn();
            }
        }

        public void EnemyDestroyed(int points)
        {
            _score += points;
            
            Print(_score);
        }
    }
}
