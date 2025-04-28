using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ErkenGame.Models
{
    public class Zombie
    {
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        private float _zombieSpeed = 200f; // Скорость зомби
        private float _gravity = 800f;
        private float _jumpSpeed = -600f;
        private bool _isJumping = false;
        private bool _isOnGround = false;
        private Vector2 _velocity = Vector2.Zero;
        private int _health = 100; // Здоровье зомби

        public Zombie(Vector2 position, Texture2D texture)
        {
            Position = position;
            Texture = texture;
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        }

        public void Update(GameTime gameTime, Player player, List<Obstacle> obstacles)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _isOnGround = false; // Сбрасываем _isOnGround в начале каждого обновления

            // Следование за игроком
            if (Position.X < player.Position.X)
                _velocity.X = _zombieSpeed;
            else
                _velocity.X = -_zombieSpeed;

            // Гравитация
            _velocity.Y += _gravity * deltaTime;

            // Применение скорости
            Vector2 newPosition = Position + _velocity * deltaTime;
            Rectangle zombieRect = new Rectangle((int)newPosition.X, (int)newPosition.Y, Texture.Width, Texture.Height);
            Rectangle playerRect = new Rectangle((int)player.Position.X, (int)player.Position.Y, player.Texture.Width, player.Texture.Height);

            // Обработка столкновений с препятствиями
            foreach (Obstacle obstacle in obstacles)
            {
                // Проверка столкновений (примерно как у игрока)
                if (_velocity.Y >= 0 &&
                    zombieRect.Bottom >= obstacle.Rectangle.Top &&
                    Position.Y + Texture.Height <= obstacle.Rectangle.Top &&
                    zombieRect.Right > obstacle.Rectangle.Left &&
                    zombieRect.Left < obstacle.Rectangle.Right)
                {
                    // Мы приземлились на платформу
                    newPosition.Y = obstacle.Rectangle.Top - Texture.Height;
                    _velocity.Y = 0;
                    _isOnGround = true;
                    _isJumping = false;
                }
                else if (newPosition.X + Texture.Width > obstacle.Rectangle.Left && Position.X + Texture.Width <= obstacle.Rectangle.Left &&
                         zombieRect.Bottom > obstacle.Rectangle.Top &&
                         zombieRect.Top < obstacle.Rectangle.Bottom)
                {
                    // Столкновение сбоку справа
                    newPosition.X = obstacle.Rectangle.Left - Texture.Width;
                    _velocity.X = 0;
                }
                else if (newPosition.X < obstacle.Rectangle.Right && Position.X >= obstacle.Rectangle.Right &&
                         zombieRect.Bottom > obstacle.Rectangle.Top &&
                         zombieRect.Top < obstacle.Rectangle.Bottom)
                {
                    // Столкновение сбоку слева
                    newPosition.X = obstacle.Rectangle.Right;
                    _velocity.X = 0;
                }

            }

            // Проверка столкновения с игроком снизу
            if (_velocity.Y >= 0 &&
                zombieRect.Bottom >= playerRect.Top &&
                Position.Y + Texture.Height <= playerRect.Top &&
                zombieRect.Right > playerRect.Left &&
                zombieRect.Left < playerRect.Right)
            {
                // Зомби стоит на игроке
                newPosition.Y = playerRect.Top - Texture.Height;
                _velocity.Y = 0;
                _isOnGround = true;
                _isJumping = false;
            }

            // Ограничение падения вниз (замените на высоту земли, если нужно)
            if (_isOnGround == false)
            {
                if (newPosition.Y > 1560 - Texture.Height)
                {
                    newPosition.Y = 1560 - Texture.Height;
                    _velocity.Y = 0;
                    _isOnGround = true;
                    _isJumping = false;
                }
            }

            Position = newPosition;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }

        public void TakeDamage(int damage)
        {
            _health -= damage;
            if (_health <= 0)
            {
                _health = 0; // Убедитесь, что здоровье не становится отрицательным
                             // Зомби умер
                             // TODO: Добавьте логику смерти зомби (например, удаление из списка)
            }
        }

        public int GetHealth()
        {
            return _health;
        }
    }
}
