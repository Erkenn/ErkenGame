using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ErkenGame.Models
{
    public class Zombie
    {
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        private float _zombieSpeed = 200f; // �������� �����
        private float _gravity = 800f;
        private float _jumpSpeed = -600f;
        private bool _isJumping = false;
        private bool _isOnGround = false;
        private Vector2 _velocity = Vector2.Zero;
        private int _health = 100; // �������� �����

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

            _isOnGround = false; // ���������� _isOnGround � ������ ������� ����������

            // ���������� �� �������
            if (Position.X < player.Position.X)
                _velocity.X = _zombieSpeed;
            else
                _velocity.X = -_zombieSpeed;

            // ����������
            _velocity.Y += _gravity * deltaTime;

            // ���������� ��������
            Vector2 newPosition = Position + _velocity * deltaTime;
            Rectangle zombieRect = new Rectangle((int)newPosition.X, (int)newPosition.Y, Texture.Width, Texture.Height);
            Rectangle playerRect = new Rectangle((int)player.Position.X, (int)player.Position.Y, player.Texture.Width, player.Texture.Height);

            // ��������� ������������ � �������������
            foreach (Obstacle obstacle in obstacles)
            {
                // �������� ������������ (�������� ��� � ������)
                if (_velocity.Y >= 0 &&
                    zombieRect.Bottom >= obstacle.Rectangle.Top &&
                    Position.Y + Texture.Height <= obstacle.Rectangle.Top &&
                    zombieRect.Right > obstacle.Rectangle.Left &&
                    zombieRect.Left < obstacle.Rectangle.Right)
                {
                    // �� ������������ �� ���������
                    newPosition.Y = obstacle.Rectangle.Top - Texture.Height;
                    _velocity.Y = 0;
                    _isOnGround = true;
                    _isJumping = false;
                }
                else if (newPosition.X + Texture.Width > obstacle.Rectangle.Left && Position.X + Texture.Width <= obstacle.Rectangle.Left &&
                         zombieRect.Bottom > obstacle.Rectangle.Top &&
                         zombieRect.Top < obstacle.Rectangle.Bottom)
                {
                    // ������������ ����� ������
                    newPosition.X = obstacle.Rectangle.Left - Texture.Width;
                    _velocity.X = 0;
                }
                else if (newPosition.X < obstacle.Rectangle.Right && Position.X >= obstacle.Rectangle.Right &&
                         zombieRect.Bottom > obstacle.Rectangle.Top &&
                         zombieRect.Top < obstacle.Rectangle.Bottom)
                {
                    // ������������ ����� �����
                    newPosition.X = obstacle.Rectangle.Right;
                    _velocity.X = 0;
                }

            }

            // �������� ������������ � ������� �����
            if (_velocity.Y >= 0 &&
                zombieRect.Bottom >= playerRect.Top &&
                Position.Y + Texture.Height <= playerRect.Top &&
                zombieRect.Right > playerRect.Left &&
                zombieRect.Left < playerRect.Right)
            {
                // ����� ����� �� ������
                newPosition.Y = playerRect.Top - Texture.Height;
                _velocity.Y = 0;
                _isOnGround = true;
                _isJumping = false;
            }

            // ����������� ������� ���� (�������� �� ������ �����, ���� �����)
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
                _health = 0; // ���������, ��� �������� �� ���������� �������������
                             // ����� ����
                             // TODO: �������� ������ ������ ����� (��������, �������� �� ������)
            }
        }

        public int GetHealth()
        {
            return _health;
        }
    }
}
