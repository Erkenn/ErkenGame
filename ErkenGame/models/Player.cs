using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ErkenGame.Models
{
    public class Player
    {
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        private float _playerSpeed = 400f;
        private float _gravity = 800f;
        private float _jumpSpeed = -700f;
        private bool _isAttacking = false;
        private bool _attackedThisFrame = false;
        private bool _canAttack = true;
        private float _postAttackDelay =  10f; 
        private float _postAttackTimer = 0f; 
        private float _attackTimer = 0f;
        private float _attackCooldown = 2f; // Время перезарядки атаки
        private int _attackDamage = 40; // Урон от атаки
        private int _health = 100; // Здоровье игрока
        private float _damageCooldown = 1f; // Время перезарядки получения урона
        // Добавляем анимации
        private Dictionary<string, Animation> _animations;
        private string _currentAnimation;
        // Добавляем оружие
        public Weapon CurrentWeapon { get; private set; }
        private List<Weapon> _availableWeapons = new List<Weapon>();
        // Добавляем систему здоровья
        private float _healCooldown = 5f;
        private float _healTimer = 0f;
        private bool _canHeal = true;
        private float _damageTimer = 0f;
        private bool _isJumping = false;
        private bool _isOnGround = false;
        private Vector2 _velocity = Vector2.Zero;
        private int _groundLevel = 1000;

        public Player(Vector2 position, Texture2D texture)
        {
            Position = position;
            Texture = texture;
        }

        public void LoadAnimations(ContentManager content)
        {
            _animations = new Dictionary<string, Animation>()
        {
            {"Idle", new Animation(content.Load<Texture2D>("PlayerIdle"), 4, 0.2f)},
            {"Run", new Animation(content.Load<Texture2D>("PlayerRun"), 6, 0.1f)},
            {"Jump", new Animation(content.Load<Texture2D>("PlayerJump"), 3, 0.15f)},
            {"Attack", new Animation(content.Load<Texture2D>("PlayerAttack"), 4, 0.1f)}
        };
            _currentAnimation = "Idle";
        }

        public void Update(GameTime gameTime, List<Obstacle> obstacles, List<Zombie> zombies)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboardState = Keyboard.GetState();

            // Обновляем анимацию в зависимости от состояния
            if (!_isOnGround) _currentAnimation = "Jump";
            else if (Math.Abs(_velocity.X) > 0.1f) _currentAnimation = "Run";
            else _currentAnimation = "Idle";

            _animations[_currentAnimation].Update(gameTime);

            // Система восстановления здоровья
            if (!_canHeal)
            {
                _healTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_healTimer <= 0) _canHeal = true;
            }
            else if (_health < 100)
            {
                _health = Math.Min(_health + 1, 100);
                _healTimer = _healCooldown;
                _canHeal = false;
            }

            if (!_canAttack)
            {
                _postAttackTimer -= deltaTime;
                if (_postAttackTimer <= 0)
                {
                    _canAttack = true;
                }
            }

            // Атака
            if (keyboardState.IsKeyDown(Keys.F) && !_isAttacking && _canAttack)
            {
                _isAttacking = true;
                _attackedThisFrame = false; // Сбрасываем флаг в начале новой атаки
                _attackTimer = _attackCooldown;
            }

            if (_isAttacking)
            {
                _attackTimer -= deltaTime;
                if (_attackTimer <= 0)
                {
                    _isAttacking = false;
                    _canAttack = false; // Запрещаем атаковать после завершения анимации атаки
                    _postAttackTimer = _postAttackDelay; // Запускаем таймер задержки
                }
            }

            // Горизонтальное движение
            float horizontalMovement = 0f;
            if (keyboardState.IsKeyDown(Keys.A))
                horizontalMovement -= 1;
            if (keyboardState.IsKeyDown(Keys.D))
                horizontalMovement += 1;

            _velocity.X = horizontalMovement * _playerSpeed;

            // Прыжок
            if (keyboardState.IsKeyDown(Keys.Space) && _isOnGround)
            {
                _velocity.Y = _jumpSpeed;
                _isJumping = true;
                _isOnGround = false;
            }

            if (_damageTimer > 0)
            {
                _damageTimer -= deltaTime;
            }

            // Гравитация
            _velocity.Y += _gravity * deltaTime;

            // Применение скорости
            Vector2 newPosition = Position + _velocity * deltaTime;

            // Обновляем прямоугольник персонажа для проверок столкновений
            Rectangle playerRect = new Rectangle((int)newPosition.X, (int)newPosition.Y, Texture.Width, Texture.Height);

            // Обработка столкновений с препятствиями
            _isOnGround = false;
            foreach (Obstacle obstacle in obstacles)
            {
                // Сначала, проверка сверху
                if (_velocity.Y >= 0 &&
                    playerRect.Bottom >= obstacle.Rectangle.Top &&
                    Position.Y + Texture.Height <= obstacle.Rectangle.Top &&
                    playerRect.Right > obstacle.Rectangle.Left &&
                    playerRect.Left < obstacle.Rectangle.Right)
                {
                    // Мы приземлились на платформу
                    newPosition.Y = obstacle.Rectangle.Top - Texture.Height;
                    _velocity.Y = 0;
                    _isOnGround = true;
                    _isJumping = false;
                }
                // Затем, проверка снизу
                else if (_velocity.Y <= 0 &&
                         playerRect.Top <= obstacle.Rectangle.Bottom &&
                         Position.Y >= obstacle.Rectangle.Bottom &&
                         playerRect.Right > obstacle.Rectangle.Left &&
                         playerRect.Left < obstacle.Rectangle.Right)
                {
                    // Ударяемся головой об платформу
                    newPosition.Y = obstacle.Rectangle.Bottom;
                    _velocity.Y = 0;
                }
                // Затем, проверка сбоку
                else if (newPosition.X + Texture.Width > obstacle.Rectangle.Left && Position.X + Texture.Width <= obstacle.Rectangle.Left &&
                         playerRect.Bottom > obstacle.Rectangle.Top &&
                         playerRect.Top < obstacle.Rectangle.Bottom)
                {
                    // Столкновение сбоку справа
                    newPosition.X = obstacle.Rectangle.Left - Texture.Width;
                    _velocity.X = 0;
                }
                else if (newPosition.X < obstacle.Rectangle.Right && Position.X >= obstacle.Rectangle.Right &&
                         playerRect.Bottom > obstacle.Rectangle.Top &&
                         playerRect.Top < obstacle.Rectangle.Bottom)
                {
                    // Столкновение сбоку слева
                    newPosition.X = obstacle.Rectangle.Right;
                    _velocity.X = 0;
                }
            }

            if (keyboardState.IsKeyDown(Keys.F) && !_isAttacking)
            {
                _isAttacking = true;
                _attackTimer = _attackCooldown;
            }

            if (_isAttacking)
            {
                _attackTimer -= deltaTime;
                if (_attackTimer <= 0)
                {
                    _isAttacking = false;
                }
            }

            // Проверка столкновений с зомби
         
            foreach (Zombie zombie in zombies)
            {
                Rectangle zombieRect = zombie.GetRectangle();
                if (playerRect.Intersects(zombieRect))
                {
                    // Если игрок атакует и еще не нанес урон в этом кадре, наносим урон зомби
                    if (_isAttacking && !_attackedThisFrame)
                    {
                        zombie.TakeDamage(_attackDamage);
                        _attackedThisFrame = true; // Устанавливаем флаг, чтобы больше не наносить урон в этом кадре
                    }
                    // Если игрок не атакует и таймер перезарядки прошел, получаем урон от зомби
                    else if (_damageTimer <= 0)
                    {
                        TakeDamage(30); 
                        _damageTimer = _damageCooldown; // Запускаем таймер перезарядки
                    }
                }
            }
            //Ограничение падения вниз
            if (newPosition.Y > _groundLevel)
                newPosition.Y = _groundLevel;

            //Применяем новую позицию
            Position = newPosition;

            // Обновляем _isOnGround, если нет столкновений
            if (Position.Y == _groundLevel)
                _isOnGround = true;
        }

        public void TakeDamage(int damage)
        {
            _health -= damage;
            if (_health <= 0)
            {
                // Игрок умер
                _health = 0;
                // TODO: Добавьте логику смерти игрока (например, перезапуск уровня)
            }
        }
        public int GetHealth()
        {
            return _health;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animations[_currentAnimation].Texture, Position, _animations[_currentAnimation].CurrentFrameRect, Color.White);
        }





        public void PickUpWeapon(Weapon weapon)
        {
            CurrentWeapon = weapon;
            if (!_availableWeapons.Contains(weapon))
                _availableWeapons.Add(weapon);
        }
    }
}
