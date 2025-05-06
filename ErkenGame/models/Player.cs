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
        private SpriteEffects _spriteEffect = SpriteEffects.None; // �� ��������� �� ��������
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        private float _playerSpeed = 400f;
        private Vector2 _startPosition;
        private float _gravity = 800f;
        private float _jumpSpeed = -900f;
        private bool _isAttacking = false;
        private bool _attackedThisFrame = false;
        private bool _canAttack = true;
        private float _postAttackDelay =  10f; 
        private float _postAttackTimer = 0f; 
        private float _attackTimer = 0f;
        private float _attackCooldown = 2f; // ����� ����������� �����
        private int _attackDamage = 40; // ���� �� �����
        private int _health = 100; // �������� ������
        private int _lives = 3; // ��������� ���������� ������
        private Vector2 _respawnPoint = new Vector2(100, 100); // ����� �����������
        public int Lives => _lives;

        public Player(Vector2 position, Texture2D texture)
        {
            Position = position;
            _startPosition = position; // ��������� ��������� �������
            Texture = texture;
        }

        public void ResetLives()
        {
            _lives = 3;
            _health = 100;
            Position = _startPosition;
            _velocity = Vector2.Zero;
            _isAttacking = false;
            _canAttack = true;
            _damageTimer = 0f;
        }

        public void Reset()
        {
            ResetLives();
            // ��� ������(���� ����� ����� � �������)
        }

        private float _damageCooldown = 1f; // ����� ����������� ��������� �����
        // ��������� ��������
        private Dictionary<string, Animation> _animations;
        private string _currentAnimation;
        // ��������� ������
        public Weapon CurrentWeapon { get; private set; }
        private List<Weapon> _availableWeapons = new List<Weapon>();
        // ��������� ������� ��������
        private float _healCooldown = 0.1f;
        private float _healTimer = 0f;
        private bool _canHeal = true;

        private float _damageTimer = 0f;
        private bool _isJumping = false;
        private bool _isOnGround = false;
        private Vector2 _velocity = Vector2.Zero;
        private int _groundLevel = 1300;

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

            // ��������� �������� � ����������� �� ���������
            if (!_isOnGround) _currentAnimation = "Jump";
            else if (Math.Abs(_velocity.X) > 0.1f) _currentAnimation = "Run";
            else _currentAnimation = "Idle";

            _animations[_currentAnimation].Update(gameTime);

            // ������� �������������� ��������
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

            // �����
            if (keyboardState.IsKeyDown(Keys.F) && !_isAttacking && _canAttack)
            {
                _isAttacking = true;
                _attackedThisFrame = false; // ���������� ���� � ������ ����� �����
                _currentAnimation = "Attack";
                _attackTimer = _attackCooldown;
            }

            if (_isAttacking)
            {
                _attackTimer -= deltaTime;
                if (_attackTimer <= 0)
                {
                    _isAttacking = false;
                    _canAttack = false; // ��������� ��������� ����� ���������� �������� �����
                    _postAttackTimer = _postAttackDelay; // ��������� ������ ��������
                }
            }

            // �������������� ��������
            float horizontalMovement = 0f;
            if (keyboardState.IsKeyDown(Keys.A))
            {
                horizontalMovement -= 1;
                _spriteEffect = SpriteEffects.FlipHorizontally;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                horizontalMovement += 1;
                _spriteEffect = SpriteEffects.None;
            }

            _velocity.X = horizontalMovement * _playerSpeed;

            // ������
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

            // ����������
            _velocity.Y += _gravity * deltaTime;

            Vector2 newPosition = Position + _velocity * deltaTime;

            // ��������� ������������� ��������� ��� �������� ������������
            Rectangle playerRect = new Rectangle(
                (int)newPosition.X,
                (int)newPosition.Y,
                CurrentFrameRect.Width,
                CurrentFrameRect.Height
            );

            // ��������� ������������ � �������������
            foreach (Obstacle obstacle in obstacles)
            {
                Rectangle obstacleRect = obstacle.Rectangle;

                if (playerRect.Intersects(obstacleRect))
                {
                    // ��������� �������� ���������� ���������������
                    float overlapX = Math.Min(playerRect.Right, obstacleRect.Right) - Math.Max(playerRect.Left, obstacleRect.Left);
                    float overlapY = Math.Min(playerRect.Bottom, obstacleRect.Bottom) - Math.Max(playerRect.Top, obstacleRect.Top);

                    // ������������ ������� �� ��� ���, ��� ���������� ������
                    if (overlapX < overlapY)
                    {
                        if (playerRect.Center.X < obstacleRect.Center.X)
                        {
                            newPosition.X = obstacleRect.Left - CurrentFrameRect.Width;
                            _velocity.X = 0;
                        }
                        else
                        {
                            newPosition.X = obstacleRect.Right;
                            _velocity.X = 0;
                        }
                    }
                    else
                    {
                        //���� ��������� ���� � ��������� � ������� ���������
                        if (_velocity.Y > 0 && playerRect.Center.Y < obstacleRect.Center.Y)
                        {
                            newPosition.Y = obstacleRect.Top - CurrentFrameRect.Height;
                            _velocity.Y = 0;
                            _isOnGround = true;
                            _isJumping = false;
                        }
                        else
                        {
                            if (playerRect.Center.Y < obstacleRect.Center.Y)
                            {
                                newPosition.Y = obstacleRect.Top - CurrentFrameRect.Height;
                                _velocity.Y = 0;
                                _isOnGround = true;
                                _isJumping = false;
                            }
                            else
                            {
                                newPosition.Y = obstacleRect.Bottom;
                                _velocity.Y = 0;
                            }
                        }
                    }
                }
            }

            if (newPosition.Y > _groundLevel)
            {
                //���� ��������� ���� � �������� �����
                if (_velocity.Y > 0)
                {
                    newPosition.Y = _groundLevel;
                    _velocity.Y = 0;
                    _isOnGround = true;
                    _isJumping = false;
                }
                else
                {
                    newPosition.Y = _groundLevel;
                }
            }
            //��������� ����� �������
            Position = newPosition;

            if (!_isOnGround) _currentAnimation = "Jump";
            else if (Math.Abs(_velocity.X) > 0.1f) _currentAnimation = "Run";
            else _currentAnimation = "Idle";

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
        _canAttack = true; // ��������� ��������� �����
        _currentAnimation = _isOnGround ? (_velocity.X != 0 ? "Run" : "Idle") : "Jump"; //���������� �������� � ����������� �� ���������
    }
    else
    {
        _currentAnimation = "Attack";
    }
}





            // �������� ������������ � �����

            foreach (Zombie zombie in zombies)
            {
                Rectangle zombieRect = zombie.GetRectangle();
                if (playerRect.Intersects(zombieRect))
                {
                    // ���� ����� ������� � ��� �� ����� ���� � ���� �����, ������� ���� �����
                    if (_isAttacking && !_attackedThisFrame)
                    {
                        zombie.TakeDamage(_attackDamage);
                        _attackedThisFrame = true; // ������������� ����, ����� ������ �� �������� ���� � ���� �����
                    }
                    // ���� ����� �� ������� � ������ ����������� ������, �������� ���� �� �����
                    else if (_damageTimer <= 0)
                    {
                        TakeDamage(30); 
                        _damageTimer = _damageCooldown; // ��������� ������ �����������
                    }
                }
            }
        }

        public void TakeDamage(int damage)
        {
            if (_damageTimer > 0) return;

            _health -= damage;
            _damageTimer = _damageCooldown;

            if (_health <= 0)
            {
                Respawn();
            }
        }

        public void Respawn()
        {
            _health = 100;
            Position = _startPosition;
            _lives--;
        }

        public int GetHealth() => _health;
        public int GetLives() => _lives;
        public bool IsAlive => _lives > 0;


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animations[_currentAnimation].Texture,
                           Position,
                           _animations[_currentAnimation].CurrentFrameRect,
                           Color.White,
                           0f,
                           Vector2.Zero,
                           1f,
                           _spriteEffect,
                           0f);
        }

        public void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            Texture2D dummyTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            dummyTexture.SetData(new Color[] { Color.White }); // ��������� �������� ����� ������

            spriteBatch.Draw(dummyTexture, rectangle, color);

            dummyTexture.Dispose();
        }

        public Rectangle CurrentFrameRect =>
            _animations != null && _animations.ContainsKey(_currentAnimation)
                ? _animations[_currentAnimation].CurrentFrameRect
                : new Rectangle(0, 0, Texture?.Width ?? 0, Texture?.Height ?? 0);

        public void PickUpWeapon(Weapon weapon)
        {
            CurrentWeapon = weapon;
            if (!_availableWeapons.Contains(weapon))
                _availableWeapons.Add(weapon);
        }

        public Rectangle GetRectangle() =>
            new Rectangle((int)Position.X, (int)Position.Y, CurrentFrameRect.Width, CurrentFrameRect.Height);
    }
}
