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
        private float _attackCooldown = 2f; // Âðåìÿ ïåðåçàðÿäêè àòàêè
        private int _attackDamage = 40; // Óðîí îò àòàêè
        private int _health = 100; // Çäîðîâüå èãðîêà
        private float _damageCooldown = 1f; // Âðåìÿ ïåðåçàðÿäêè ïîëó÷åíèÿ óðîíà
        // Äîáàâëÿåì àíèìàöèè
        private Dictionary<string, Animation> _animations;
        private string _currentAnimation;
        // Äîáàâëÿåì îðóæèå
        public Weapon CurrentWeapon { get; private set; }
        private List<Weapon> _availableWeapons = new List<Weapon>();
        // Äîáàâëÿåì ñèñòåìó çäîðîâüÿ
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

            // Îáíîâëÿåì àíèìàöèþ â çàâèñèìîñòè îò ñîñòîÿíèÿ
            if (!_isOnGround) _currentAnimation = "Jump";
            else if (Math.Abs(_velocity.X) > 0.1f) _currentAnimation = "Run";
            else _currentAnimation = "Idle";

            _animations[_currentAnimation].Update(gameTime);

            // Ñèñòåìà âîññòàíîâëåíèÿ çäîðîâüÿ
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

            // Àòàêà
            if (keyboardState.IsKeyDown(Keys.F) && !_isAttacking && _canAttack)
            {
                _isAttacking = true;
                _attackedThisFrame = false; // Ñáðàñûâàåì ôëàã â íà÷àëå íîâîé àòàêè
                _attackTimer = _attackCooldown;
            }

            if (_isAttacking)
            {
                _attackTimer -= deltaTime;
                if (_attackTimer <= 0)
                {
                    _isAttacking = false;
                    _canAttack = false; // Çàïðåùàåì àòàêîâàòü ïîñëå çàâåðøåíèÿ àíèìàöèè àòàêè
                    _postAttackTimer = _postAttackDelay; // Çàïóñêàåì òàéìåð çàäåðæêè
                }
            }

            // Ãîðèçîíòàëüíîå äâèæåíèå
            float horizontalMovement = 0f;
            if (keyboardState.IsKeyDown(Keys.A))
                horizontalMovement -= 1;
            if (keyboardState.IsKeyDown(Keys.D))
                horizontalMovement += 1;

            _velocity.X = horizontalMovement * _playerSpeed;

            // Ïðûæîê
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

            // Ãðàâèòàöèÿ
            _velocity.Y += _gravity * deltaTime;

            // Ïðèìåíåíèå ñêîðîñòè
            Vector2 newPosition = Position + _velocity * deltaTime;

            // Îáíîâëÿåì ïðÿìîóãîëüíèê ïåðñîíàæà äëÿ ïðîâåðîê ñòîëêíîâåíèé
            Rectangle playerRect = new Rectangle((int)newPosition.X, (int)newPosition.Y, Texture.Width, Texture.Height);

            // Îáðàáîòêà ñòîëêíîâåíèé ñ ïðåïÿòñòâèÿìè
            _isOnGround = false;
            foreach (Obstacle obstacle in obstacles)
            {
                // Ñíà÷àëà, ïðîâåðêà ñâåðõó
                if (_velocity.Y >= 0 &&
                    playerRect.Bottom >= obstacle.Rectangle.Top &&
                    Position.Y + Texture.Height <= obstacle.Rectangle.Top &&
                    playerRect.Right > obstacle.Rectangle.Left &&
                    playerRect.Left < obstacle.Rectangle.Right)
                {
                    // Ìû ïðèçåìëèëèñü íà ïëàòôîðìó
                    newPosition.Y = obstacle.Rectangle.Top - Texture.Height;
                    _velocity.Y = 0;
                    _isOnGround = true;
                    _isJumping = false;
                }
                // Çàòåì, ïðîâåðêà ñíèçó
                else if (_velocity.Y <= 0 &&
                         playerRect.Top <= obstacle.Rectangle.Bottom &&
                         Position.Y >= obstacle.Rectangle.Bottom &&
                         playerRect.Right > obstacle.Rectangle.Left &&
                         playerRect.Left < obstacle.Rectangle.Right)
                {
                    // Óäàðÿåìñÿ ãîëîâîé îá ïëàòôîðìó
                    newPosition.Y = obstacle.Rectangle.Bottom;
                    _velocity.Y = 0;
                }
                // Çàòåì, ïðîâåðêà ñáîêó
                else if (newPosition.X + Texture.Width > obstacle.Rectangle.Left && Position.X + Texture.Width <= obstacle.Rectangle.Left &&
                         playerRect.Bottom > obstacle.Rectangle.Top &&
                         playerRect.Top < obstacle.Rectangle.Bottom)
                {
                    // Ñòîëêíîâåíèå ñáîêó ñïðàâà
                    newPosition.X = obstacle.Rectangle.Left - Texture.Width;
                    _velocity.X = 0;
                }
                else if (newPosition.X < obstacle.Rectangle.Right && Position.X >= obstacle.Rectangle.Right &&
                         playerRect.Bottom > obstacle.Rectangle.Top &&
                         playerRect.Top < obstacle.Rectangle.Bottom)
                {
                    // Ñòîëêíîâåíèå ñáîêó ñëåâà
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

            // Ïðîâåðêà ñòîëêíîâåíèé ñ çîìáè
         
            foreach (Zombie zombie in zombies)
            {
                Rectangle zombieRect = zombie.GetRectangle();
                if (playerRect.Intersects(zombieRect))
                {
                    // Åñëè èãðîê àòàêóåò è åùå íå íàíåñ óðîí â ýòîì êàäðå, íàíîñèì óðîí çîìáè
                    if (_isAttacking && !_attackedThisFrame)
                    {
                        zombie.TakeDamage(_attackDamage);
                        _attackedThisFrame = true; // Óñòàíàâëèâàåì ôëàã, ÷òîáû áîëüøå íå íàíîñèòü óðîí â ýòîì êàäðå
                    }
                    // Åñëè èãðîê íå àòàêóåò è òàéìåð ïåðåçàðÿäêè ïðîøåë, ïîëó÷àåì óðîí îò çîìáè
                    else if (_damageTimer <= 0)
                    {
                        TakeDamage(30); 
                        _damageTimer = _damageCooldown; // Çàïóñêàåì òàéìåð ïåðåçàðÿäêè
                    }
                }
            }
            //Îãðàíè÷åíèå ïàäåíèÿ âíèç
            if (newPosition.Y > _groundLevel)
                newPosition.Y = _groundLevel;

            //Ïðèìåíÿåì íîâóþ ïîçèöèþ
            Position = newPosition;

            // Îáíîâëÿåì _isOnGround, åñëè íåò ñòîëêíîâåíèé
            if (Position.Y == _groundLevel)
                _isOnGround = true;
        }

        public void TakeDamage(int damage)
        {
            _health -= damage;
            if (_health <= 0)
            {
                // Èãðîê óìåð
                _health = 0;
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
