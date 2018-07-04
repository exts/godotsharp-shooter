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
        private Node _explosions;

        private Node2D _healthbar;
        
        private Ship _ship;
        private Timer _formationTimer;
        private Timer _enemySpawnTimer;
        private Node2D _scoreUi;
        private Vector2 _viewport;
        private StatusPanel _statusPanel;
        private Formations _formations = new Formations();

        private PackedScene _enemyObject;
        private PackedScene _bulletObject;
        private PackedScene _explosionObject;

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
            if(!_running) return;
            
            DespawnBullets();
            SpawnIfNoEnemies();
        }

        private void SetupGameObjects()
        {
            // setup packed scenes used to spawn multiple objects from the same textures
            _enemyObject = (PackedScene) ResourceLoader.Load("res://Scenes/Objects/Enemy.tscn");
            _bulletObject = (PackedScene) ResourceLoader.Load("res://Scenes/Objects/Bullet.tscn");
            _explosionObject = (PackedScene) ResourceLoader.Load("res://Scenes/Objects/Explosion.tscn");

            // get containers
            _bullets = GetNode("GameCanvas/Bullets");
            _enemies = GetNode("GameCanvas/Enemies");
            _explosions = GetNode("GameCanvas/Explosions");
            
            // get ui
            _scoreUi = (Node2D) GetNode("GameCanvas/ScoreUI");
            _healthbar = (Node2D) GetNode("GameCanvas/Healthbar");
            
            _statusPanel = (StatusPanel) GetNode("GameCanvas/StatusPanel");
            _statusPanel.Connect("ResetGame", this, nameof(StartGame));

            // connect the ship signal to a method inside our game scene, this would allow us to spawn bullets
            // into the scene after we press spacebar. Allowing us to keep our code as separated as possible. Also
            // allowing us to have bullets interact with other things like enemies in our case.
            _ship = (Ship) GetNode("GameCanvas/Ship");
            _ship.Connect("Damaged", this, nameof(UpdateHealthBar));
            _ship.Connect("Destroyed", this, nameof(ShipDestroyed));
            _ship.Connect("WeaponDischarged", this, nameof(SpawnBullet));

            // setup monster spawner timer
            _enemySpawnTimer = (Timer) GetNode("GameCanvas/EnemySpawnTimer");
            _enemySpawnTimer.Connect("timeout", this, nameof(SpawnEnemies));
            
            _formationTimer = (Timer) GetNode("GameCanvas/FormationTimer");
            _formationTimer.Connect("timeout", this, nameof(SetNewFormation));
            
            _statusPanel.ShowStartGamePanel();
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
                    // delete bullet when it leaves he screen
                    if(bullet.Position.x > _viewport.x || bullet.Position.x < 0 - bullet.GetSize().x)
                    {
                        bullet.QueueFree();
                    }
                }
            }            
        }

        private void DespawnNodeContainers(Node container)
        {
            foreach(var child in container.GetChildren().ToList())
            {
                if(child is Node baby)
                {
                    baby.QueueFree();
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
            if(_enemies.GetChildCount() == 0 && _enemySpawnTimer.IsStopped() && _formations.IsEndOfColumn(out _))
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
                enemy.Connect("Destroyed", this, nameof(EnemyDestroyed));
                
                GetNode("GameCanvas/Enemies").AddChild(enemy);
            }

            if(_formations.IsEndOfColumn(out _))
            {
                _enemySpawnTimer.Stop();
                _formationTimer.Start();
            }
            else
            {
                _formations.NextColumn();
            }
        }

        public void UpdateHealthBar(int health)
        {
            var progressBar = (TextureProgress) _healthbar.GetNode("progress");
            progressBar.Value = health;
        }

        public void EnemyDestroyed(int points, Vector2 position)
        {
            _score += points;
            
            UpdateScore(_score);
            SpawnExplosion(position);
        }

        public void ShipDestroyed(Vector2 position)
        {
            _ship.Hide();
            SpawnExplosion(position);

            _running = false;
        }

        public void ExplosionAnimationFinished()
        {
            if(!_running)
            {
                GameOver();
            }
        }

        private void UpdateScore(int points)
        {
            var score = (Label) _scoreUi.GetNode("Score");
            score.Text = points.ToString();
        }

        private void SpawnExplosion(Vector2 position)
        {
            //spawn explosion
            var explosion = (Explosion) _explosionObject.Instance();
            explosion.Connect("Destroyed", this, nameof(ExplosionAnimationFinished));
            explosion.Position = position;
            _explosions.AddChild(explosion);
        }

        private void StartGame()
        {
            _score = 0;
            _running = true;

            UpdateScore(_score);
            UpdateHealthBar(100);

            _statusPanel.Hide();
            
            _ship.Reset();
            _ship.Show();

            _enemySpawnTimer.Start();
        }

        private void GameOver()
        {
            _formationTimer.Stop();
            _enemySpawnTimer.Stop();

            _statusPanel.ShowGameOverPanel();

            DespawnNodeContainers(_bullets);
            DespawnNodeContainers(_enemies);
            DespawnNodeContainers(_explosions);
        }
    }
}
