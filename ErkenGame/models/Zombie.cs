using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ErkenGame.Models
{
    public class Zombie
    {
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        public bool IsAlive { get; private set; } = true;
        public int Health { get; private set; } = 100; // Добавлено свойство Health

        // Физические параметры
        private float _speed = 150f;
        private float _gravity = 800f;
        private Vector2 _velocity = Vector2.Zero;
        private bool _isOnGround = false;

        // Анимация
        private SpriteEffects _spriteEffect = SpriteEffects.None;
        private int _currentFrame = 0;
        private int _frameCount = 4;
        private float _frameTime = 0.2f;
        private float _animationTimer = 0f;
        private int _frameWidth;
        private int _collisionWidth = 40;

        // Механика прыжков
        private float _jumpDecisionDistance = 200f; // Дистанция для принятия решения о прыжке
        private float _jumpPreparationTime = 0.2f; // Время подготовки к прыжку
        private float _jumpPreparationTimer = 0f;
        private bool _isPreparingToJump = false;
        private float _jumpPower = -650f;
        private float _jumpCooldown = 1.5f;
        private float _jumpTimer = 0f;
        private bool _shouldJump = false;
        private float _obstacleCheckRange = 150f;
        private float _maxJumpHeight = 400f;
        private float _obstacleCheckCooldown = 0.3f;
        private float _obstacleCheckTimer = 0f;
        private bool _facingRight = true;

        public Zombie(Vector2 position, Texture2D texture)
        {
            Position = position;
            Texture = texture;
            _frameWidth = texture.Width / _frameCount;
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle(
                (int)Position.X + (_frameWidth - _collisionWidth) / 2,
                (int)Position.Y,
                _collisionWidth,
                Texture.Height);
        }

        public int GetHealth() // Добавлен метод GetHealth
        {
            return Health;
        }

        public void Update(GameTime gameTime, Player player, List<Obstacle> obstacles)
        {
            if (!IsAlive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновление анимации
            UpdateAnimation(deltaTime);

            // Определение направления движения
            _facingRight = Position.X < player.Position.X;
            _spriteEffect = _facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Горизонтальное движение
            _velocity.X = _facingRight ? _speed : -_speed;

            // Обработка прыжков
            HandleJumping(deltaTime, obstacles);

            // Применение гравитации
            _velocity.Y += _gravity * deltaTime;

            // Обработка коллизий и перемещение
            HandleMovement(deltaTime, obstacles);
        }

        private void UpdateAnimation(float deltaTime)
        {
            _animationTimer += deltaTime;
            if (_animationTimer >= _frameTime)
            {
                _animationTimer = 0f;
                _currentFrame = (_currentFrame + 1) % _frameCount;
            }
        }

        private void UpdateObstacleCheck(float deltaTime, List<Obstacle> obstacles)
        {
            _obstacleCheckTimer -= deltaTime;
            if (_obstacleCheckTimer <= 0)
            {
                _shouldJump = CheckForObstacle(obstacles);
                _obstacleCheckTimer = _obstacleCheckCooldown;
            }
        }

        private void UpdateMovementDirection(Player player)
        {
            _facingRight = Position.X < player.Position.X;
            _spriteEffect = _facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        }

        private bool CheckForObstacle(List<Obstacle> obstacles)
        {
            Rectangle checkArea = new Rectangle(
                (int)Position.X + (_facingRight ? _collisionWidth : -30),
                (int)Position.Y - 50,
                _facingRight ? (int)_obstacleCheckRange : -(int)_obstacleCheckRange,
                100);

            foreach (var obstacle in obstacles)
            {
                if (checkArea.Intersects(obstacle.Rectangle))
                {
                    bool isJumpable = obstacle.Rectangle.Height <= _maxJumpHeight;
                    bool isInFront = _facingRight ?
                        obstacle.Rectangle.Left > Position.X :
                        obstacle.Rectangle.Right < Position.X;

                    return isJumpable && isInFront;
                }
            }
            return false;
        }

        private void HandleJumping(float deltaTime, List<Obstacle> obstacles)
        {
            if (_jumpTimer > 0)
            {
                _jumpTimer -= deltaTime;
                return;
            }

            if (_isPreparingToJump)
            {
                _jumpPreparationTimer -= deltaTime;
                if (_jumpPreparationTimer <= 0)
                {
                    PerformJump();
                }
                return;
            }

            if (_isOnGround)
            {
                Obstacle obstacleToJump = FindObstacleToJump(obstacles);
                if (obstacleToJump != null)
                {
                    PrepareForJump();
                }
            }
        }

        private void PrepareForJump()
        {
            _isPreparingToJump = true;
            _jumpPreparationTimer = _jumpPreparationTime;
            _velocity.X = 0; // Останавливаемся перед прыжком
        }

        private void HandleMovement(float deltaTime, List<Obstacle> obstacles)
        {
            Vector2 newPosition = Position + _velocity * deltaTime;
            HandleCollisions(ref newPosition, obstacles);
            Position = newPosition;
            CheckGroundStatus(obstacles);
        }

        private void HandleCollisions(ref Vector2 newPosition, List<Obstacle> obstacles)
        {
            Rectangle zombieRect = GetCollisionRect(newPosition);

            foreach (var obstacle in obstacles)
            {
                if (zombieRect.Intersects(obstacle.Rectangle))
                {
                    ResolveCollision(ref newPosition, zombieRect, obstacle.Rectangle);
                }
            }
        }

        private Obstacle FindObstacleToJump(List<Obstacle> obstacles)
        {
            // Явное преобразование всех координат в int
            int detectionX = (int)Position.X + (_facingRight ? _collisionWidth : -(int)_jumpDecisionDistance);
            int detectionY = (int)Position.Y - 100;
            int detectionWidth = (int)_jumpDecisionDistance;
            int detectionHeight = 200;

            Rectangle detectionArea = new Rectangle(
                detectionX,
                detectionY,
                detectionWidth,
                detectionHeight);

            foreach (var obstacle in obstacles)
            {
                if (detectionArea.Intersects(obstacle.Rectangle))
                {
                    bool shouldJump = obstacle.Rectangle.Top < Position.Y &&
                                    obstacle.Rectangle.Height <= _maxJumpHeight &&
                                    ((_facingRight && obstacle.Rectangle.Left > Position.X) ||
                                    (!_facingRight && obstacle.Rectangle.Right < Position.X));

                    if (shouldJump)
                    {
                        return obstacle;
                    }
                }
            }
            return null;
        }

        private Rectangle GetCollisionRect(Vector2 position)
        {
            return new Rectangle(
                (int)position.X + (_frameWidth - _collisionWidth) / 2,
                (int)position.Y,
                _collisionWidth,
                Texture.Height);
        }

        private void ResolveCollision(ref Vector2 position, Rectangle zombieRect, Rectangle obstacleRect)
        {
            float overlapLeft = zombieRect.Right - obstacleRect.Left;
            float overlapRight = obstacleRect.Right - zombieRect.Left;
            float overlapTop = zombieRect.Bottom - obstacleRect.Top;
            float overlapBottom = obstacleRect.Bottom - zombieRect.Top;

            float minOverlap = MathHelper.Min(
                MathHelper.Min(overlapLeft, overlapRight),
                MathHelper.Min(overlapTop, overlapBottom));

            if (minOverlap == overlapTop)
            {
                position.Y = obstacleRect.Top - Texture.Height;
                _velocity.Y = 0;
                _isOnGround = true;
            }
            else if (minOverlap == overlapLeft)
            {
                position.X = obstacleRect.Left - _collisionWidth - (_frameWidth - _collisionWidth) / 2;
                _velocity.X = 0;
            }
            else if (minOverlap == overlapRight)
            {
                position.X = obstacleRect.Right - (_frameWidth - _collisionWidth) / 2;
                _velocity.X = 0;
            }
        }

        private void CheckGroundStatus(List<Obstacle> obstacles)
        {
            _isOnGround = false;

            Rectangle feetCheck = GetCollisionRect(new Vector2(Position.X, Position.Y + 5));

            foreach (var obstacle in obstacles)
            {
                if (feetCheck.Intersects(obstacle.Rectangle))
                {
                    _isOnGround = true;
                    break;
                }
            }

            if (Position.Y >= 1390 - Texture.Height)
            {
                _isOnGround = true;
                Position = new Vector2(Position.X, 1390 - Texture.Height);
                _velocity.Y = 0;
            }
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                IsAlive = false;
                Health = 0;
            }
        }

        private void PerformJump()
        {
            _velocity.Y = _jumpPower;
            _isOnGround = false;
            _isPreparingToJump = false;
            _jumpTimer = _jumpCooldown;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRect = new Rectangle(
                _currentFrame * _frameWidth,
                0,
                _frameWidth,
                Texture.Height);

            spriteBatch.Draw(
                Texture,
                Position,
                sourceRect,
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                _spriteEffect,
                0f);
        }
    }
}