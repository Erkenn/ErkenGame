using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ErkenGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //  Текстуры
        private Texture2D _buttonTexture;
        private Texture2D _backgroundTexture;
        private SpriteFont _font;

        // Переменные для персонажа
        private Texture2D _playerTexture;
        private Vector2 _playerPos;
        private float _playerSpeed = 400f;  // Пикселей в секунду
        private float _gravity = 800f; // Сила гравитации
        private float _jumpSpeed = -700f; // Начальная скорость прыжка (отрицательная, чтобы вверх)
        private bool _isJumping = false;
        private bool _isOnGround = false;  // На земле ли персонаж
        private Vector2 _velocity = Vector2.Zero;

        // Переменные для препятствий
        private Texture2D _obstacleTexture;
        private List<Rectangle> _obstacles = new List<Rectangle>();

        //  Прямоугольник для кнопки
        private Rectangle _buttonRectangle;

        // Переменная для высоты земли
        private int _groundLevel;

        //  Состояние игры
        private enum GameState { Menu, Playing }
        private GameState _gameState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;  // Делает курсор видимым.
        }

        protected override void Initialize()
        {
            //  Размеры окна
            _graphics.PreferredBackBufferWidth = 2880;
            _graphics.PreferredBackBufferHeight = 1620;
            _graphics.ApplyChanges();

            _playerPos = new Vector2(100, 100);// Начальная позиция персонажа
            // Создание препятствий
            _obstacles.Add(new Rectangle(1500, 1300, 500, 300));
            _obstacles.Add(new Rectangle(2000, 1100, 250, 300));

            // Устанавливаем уровень земли
            _groundLevel = 1000;
            // Начальная позиция на земле
            _gameState = GameState.Menu;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //  Загрузка текстур и шрифтов
            _buttonTexture = Content.Load<Texture2D>("button");
            _backgroundTexture = Content.Load<Texture2D>("background");
            _font = Content.Load<SpriteFont>("Font");
            _playerTexture = Content.Load<Texture2D>("Character");
            _obstacleTexture = Content.Load<Texture2D>("Obstacle");
            //  Определение позиции и размера кнопки
            int buttonWidth = 400;
            int buttonHeight = 100;
            int buttonX = (_graphics.PreferredBackBufferWidth - buttonWidth) / 2;
            int buttonY = (_graphics.PreferredBackBufferHeight - buttonHeight) / 2;

            _buttonRectangle = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //  Обработка ввода в меню
            if (_gameState == GameState.Menu)
            {
                MouseState mouseState = Mouse.GetState();
                //  Проверка нажатия на кнопку
                if (mouseState.LeftButton == ButtonState.Pressed && _buttonRectangle.Contains(mouseState.X, mouseState.Y))
                {
                    //  Начинаем игру
                    _gameState = GameState.Playing;
                }
            }

            if (_gameState == GameState.Playing)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                KeyboardState keyboardState = Keyboard.GetState();

                // Горизонтальное движение
                float horizontalMovement = 0f;
                if (keyboardState.IsKeyDown(Keys.Left))
                    horizontalMovement -= 1;
                if (keyboardState.IsKeyDown(Keys.Right))
                    horizontalMovement += 1;

                _velocity.X = horizontalMovement * _playerSpeed;  // Скорость по X

                // Прыжок
                if (keyboardState.IsKeyDown(Keys.Space) && _isOnGround)
                {
                    _isJumping = true;
                    _isOnGround = false;
                    _velocity.Y = _jumpSpeed;  // Начальная скорость прыжка
                }

                // Гравитация
                _velocity.Y += _gravity * deltaTime;

                // Применение скорости
                Vector2 newPosition = _playerPos + _velocity * deltaTime;

                // Обновляем прямоугольник персонажа для проверок столкновений
                Rectangle playerRect = new Rectangle((int)newPosition.X, (int)newPosition.Y, _playerTexture.Width, _playerTexture.Height);

                // Обработка столкновений с препятствиями
                _isOnGround = false; // Сбрасываем, пока не обнаружим, что стоим на чем-то
                // Проверка столкновения с каждым препятствием
                foreach (Rectangle obstacle in _obstacles)
                {
                    // Сначала, проверка сверху
                    if (_velocity.Y >= 0 &&
                        playerRect.Bottom >= obstacle.Top &&
                        _playerPos.Y + _playerTexture.Height <= obstacle.Top &&
                        playerRect.Right > obstacle.Left && // Проверка на то, что правая сторона игрока находится правее левой стороны препятствия
                        playerRect.Left < obstacle.Right) // Проверка на то, что левая сторона игрока находится левее правой стороны препятствия
                    {
                        // Мы приземлились на платформу
                        newPosition.Y = obstacle.Top - _playerTexture.Height;
                        _velocity.Y = 0;
                        _isOnGround = true;
                        _isJumping = false;

                    }
                    // Затем, проверка снизу
                    else if (_velocity.Y <= 0 &&
                             playerRect.Top <= obstacle.Bottom &&
                             _playerPos.Y >= obstacle.Bottom &&
                             playerRect.Right > obstacle.Left &&
                             playerRect.Left < obstacle.Right)
                    {
                        // Ударяемся головой об платформу
                        newPosition.Y = obstacle.Bottom;
                        _velocity.Y = 0;
                    }
                    // Затем, проверка сбоку
                    else if (newPosition.X + _playerTexture.Width > obstacle.Left && _playerPos.X + _playerTexture.Width <= obstacle.Left &&
                             playerRect.Bottom > obstacle.Top &&
                             playerRect.Top < obstacle.Bottom)
                    {
                        // Столкновение сбоку справа
                        newPosition.X = obstacle.Left - _playerTexture.Width;
                        _velocity.X = 0;

                    }
                    else if (newPosition.X < obstacle.Right && _playerPos.X >= obstacle.Right &&
                             playerRect.Bottom > obstacle.Top &&
                             playerRect.Top < obstacle.Bottom)
                    {
                        // Столкновение сбоку слева
                        newPosition.X = obstacle.Right;
                        _velocity.X = 0;

                    }
                }
                if (newPosition.Y > _groundLevel)
                    newPosition.Y = _groundLevel;

                //Применяем новую позицию
                _playerPos = newPosition;

                // Обновляем _isOnGround, если нет столкновений
                if (_playerPos.Y == _groundLevel)
                    _isOnGround = true;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);  //  Цвет фона

            _spriteBatch.Begin();

            //  Рисуем фон
            _spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);

            //  Рисуем меню
            if (_gameState == GameState.Menu)
            {
                //  Рисуем кнопку
                _spriteBatch.Draw(_buttonTexture, _buttonRectangle, Color.White);

                //  Рисуем текст на кнопке
                string buttonText = "Играть";
                Vector2 textSize = _font.MeasureString(buttonText);
                Vector2 textPosition = new Vector2(
                    _buttonRectangle.X + (_buttonRectangle.Width - textSize.X) / 2,
                    _buttonRectangle.Y + (_buttonRectangle.Height - textSize.Y) / 2);

                _spriteBatch.DrawString(_font, buttonText, textPosition, Color.Black); // Цвет текста
            }
            else if (_gameState == GameState.Playing)
            {
                //  Рисуем игровой мир
                _spriteBatch.Draw(_playerTexture, _playerPos, Color.White);
                
                //Рисуем препятствия
                foreach (Rectangle obstacle in _obstacles)
                {
                    _spriteBatch.Draw(_obstacleTexture, obstacle, Color.White);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
