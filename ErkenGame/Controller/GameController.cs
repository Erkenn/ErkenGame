using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ErkenGame.Models;
using ErkenGame.Views;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;

namespace ErkenGame.Controllers
{
    public class GameController
    {
        private List<Level> _levels = new List<Level>();
        private int _currentLevelIndex = 0;
        private Texture2D _levelButtonTexture;
        private List<Rectangle> _levelButtons = new List<Rectangle>();
        private bool _inLevelSelection = false;
        public ContentManager content;
        private MouseState _lastMouseState;
        private Rectangle _restartButtonRectangle;
        private int _screenWidth;
        private Song _backgroundMusic;
        private int _screenHeight;
        private GameState _gameState;
        private Player _player;
        private List<Obstacle> _obstacles;
        private KeyboardState _lastKeyboardState;
        private GameView _gameView;
        private Texture2D _buttonTexture;
        private SpriteFont _font;
        private SpriteFont _tutorialFont;
        private Texture2D _healthBarTexture;
        private Texture2D _healthBarBackgroundTexture;
        private float _tutorialTimer = 0f;
        private Rectangle _buttonRectangle;
        private Texture2D _backgroundTexture;
        private Camera _camera;
        private Texture2D _tutorialBoxTexture;
        private bool _showTutorial = true;
        private List<Zombie> _zombies;
        private Texture2D _zombieTexture;
        private int _zombiesKilled = 0;
        private int _zombiesToKill = 1; // Сколько зомби нужно убить для победы
        private Portal _portal;
        private GraphicsDevice _graphicsDevice;
        private Texture2D _zombieIconTexture;
        private Rectangle _backButton;
        private Texture2D _heartTexture;
        private Vector2 _heartsPosition = new Vector2(50, 50);

        private enum GameState { Menu, Playing, GameOver, Victory }

        public GameController(int screenWidth, int screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _gameState = GameState.Menu;
            _obstacles = new List<Obstacle>();
            _obstacles.Add(new Obstacle(1500, 1300, 500, 300));
            _obstacles.Add(new Obstacle(2000, 1100, 250, 300));
            _zombies = new List<Zombie>();
            _portal = new Portal(new Vector2(2500, 1300), _graphicsDevice); // Инициализируем здесь
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            this.content = content;
            _backgroundMusic = content.Load<Song>("BackgroundMusic");
            _healthBarTexture = content.Load<Texture2D>("health_bar");
            _healthBarBackgroundTexture = content.Load<Texture2D>("health_bar_background");
            _tutorialFont = content.Load<SpriteFont>("Font");
            _tutorialBoxTexture = new Texture2D(graphicsDevice, 1, 1);
            _tutorialBoxTexture.SetData(new[] { Color.Black });
            _lastKeyboardState = Keyboard.GetState();
            _graphicsDevice = graphicsDevice;
            _levelButtonTexture = content.Load<Texture2D>("level_button");
            _buttonTexture = content.Load<Texture2D>("button");
            _backgroundTexture = content.Load<Texture2D>("background");
            _font = content.Load<SpriteFont>("Font");
            Texture2D playerTexture = content.Load<Texture2D>("Character"); // Загрузка текстуры игрока
            _zombieTexture = content.Load<Texture2D>("Zombie"); // Загрузка текстуры зомби

            _player = new Player(new Vector2(100, 100), playerTexture); // Создание экземпляра игрока
            _zombies.Add(new Zombie(new Vector2(500, 100), _zombieTexture)); // Создание экземпляра зомби
            _player.LoadAnimations(content);
            _gameView = new GameView(content);
            _heartTexture = content.Load<Texture2D>("Heart");

            // Определение позиции и размера кнопки
            int buttonWidth = 400;
            int buttonHeight = 100;
            int buttonX = (_screenWidth - buttonWidth) / 2;
            int buttonY = (_screenHeight - buttonHeight) / 2;

            _restartButtonRectangle = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
            _buttonRectangle = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
            _portal.LoadContent(content);
            _gameView.LoadContent(content); // Загружаем текстуры в GameView
            _zombieIconTexture = content.Load<Texture2D>("ZombieIcon");
            InitializeLevels();
            CreateLevelButtons();
            _camera = new Camera(_graphicsDevice.Viewport); // Инициализируем камеру
        }

        private void UpdateEndGameStates(MouseState mouseState)
        {
            // Обработка кнопки в GameOver/Victory
            if (_restartButtonRectangle.Contains(mouseState.Position) &&
                mouseState.LeftButton == ButtonState.Pressed &&
                _lastMouseState.LeftButton == ButtonState.Released)
            {
                if (_gameState == GameState.GameOver)
                {
                    // Перезапускаем текущий уровень при GameOver
                    StartLevel(_currentLevelIndex);
                }
                else if (_gameState == GameState.Victory)
                {
                    // Возвращаем в меню при Victory
                    ReturnToMainMenu();
                }
            }
        }

        private void ReturnToMainMenu()
        {
            _gameState = GameState.Menu;
            _inLevelSelection = false;

            // Сбрасываем игрока без полной перезагрузки уровня
            _player.ResetLives();
            _player.Position = new Vector2(100, 100);

            // Очищаем зомби и препятствия
            _zombies.Clear();
            _obstacles.Clear();

            // Сбрасываем камеру
            _camera = new Camera(_graphicsDevice.Viewport);
        }

        private void UpdateMenu(MouseState mouseState)
        {
            if (_inLevelSelection)
            {
                // Обработка выбора уровня
                for (int i = 0; i < _levelButtons.Count; i++)
                {
                    if (_levelButtons[i].Contains(mouseState.Position) &&
                        mouseState.LeftButton == ButtonState.Pressed &&
                        _lastMouseState.LeftButton == ButtonState.Released)
                    {
                        StartLevel(i);
                        return;
                    }
                }

                // Обработка кнопки "Назад"
                if (_backButton.Contains(mouseState.Position) &&
                    mouseState.LeftButton == ButtonState.Pressed &&
                    _lastMouseState.LeftButton == ButtonState.Released)
                {
                    _inLevelSelection = false;
                }
            }
            else
            {
                // Обработка кнопки "Играть" в главном меню
                if (_buttonRectangle.Contains(mouseState.Position) &&
                    mouseState.LeftButton == ButtonState.Pressed &&
                    _lastMouseState.LeftButton == ButtonState.Released)
                {
                    _inLevelSelection = true; // Переходим к выбору уровня
                }
            }
        }


        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var currentMouseState = Mouse.GetState();
            var currentKeyboardState = Keyboard.GetState();

            // Обработка состояний игры
            switch (_gameState)
            {
                case GameState.Menu:
                    UpdateMenu(currentMouseState);
                    break;

                case GameState.Playing:
                    // Обновление игрового процесса
                    _camera.Update(_player.Position, deltaTime);
                    _player.Update(gameTime, _obstacles, _zombies);

                    // Проверка смерти игрока
                    if (_player.GetLives() <= 0)
                    {
                        _gameState = GameState.GameOver;
                    }

                    // Обновляем зомби
                    for (int i = _zombies.Count - 1; i >= 0; i--)
                    {
                        Zombie zombie = _zombies[i];
                        zombie.Update(gameTime, _player, _obstacles);

                        // Если зомби мертв, удаляем его из списка
                        if (zombie.GetHealth() <= 0)
                        {
                            _zombies.RemoveAt(i);
                            _zombiesKilled++;
                        }
                    }

                    if (_portal.IsActive)
                    {
                        _portal.Update(gameTime);

                        if (_player.GetRectangle().Intersects(_portal.Bounds))
                        {
                            _gameState = GameState.Victory;
                        }
                    }

                    if (_zombiesKilled >= _zombiesToKill && !_portal.IsActive)
                    {
                        _portal.Activate();
                    }

                    // Обновление туториала (если нужно)
                    if (_currentLevelIndex == 0 && _showTutorial)
                    {
                        UpdateTutorial(gameTime);
                    }
                    break;

                case GameState.GameOver:
                case GameState.Victory:
                    UpdateEndGameStates(currentMouseState);
                    break;
            }

            _lastMouseState = currentMouseState;
            _lastKeyboardState = currentKeyboardState;
        }

        private void DrawZombieCounter(SpriteBatch spriteBatch)
        {
            // Позиция в координатах экрана (верхний правый угол)
            Vector2 counterPosition = new Vector2(_screenWidth - 220, 20);

            // Фон для читаемости
            Rectangle bgRect = new Rectangle(
                (int)counterPosition.X - 10,
                (int)counterPosition.Y - 5,
                200,
                30);

            // Текст счетчика
            string counterText = $"Зомби: {_zombiesKilled}/{_zombiesToKill}";
            spriteBatch.DrawString(_font, counterText, counterPosition, Color.White);

            // Иконка зомби (если есть)
            if (_zombieIconTexture != null)
            {
                Rectangle iconRect = new Rectangle(
                    (int)counterPosition.X - 40,
                    (int)counterPosition.Y,
                    32,
                    32);

                spriteBatch.Draw(_zombieIconTexture, iconRect, Color.White);
            }
        }

        private void DrawTutorial(SpriteBatch spriteBatch)
        {
            if (!_showTutorial || _currentLevelIndex >= _levels.Count)
                return;

            var tutorial = _levels[_currentLevelIndex];
            if (tutorial.TutorialCompleted || tutorial.CurrentTutorialStep >= tutorial.TutorialSteps.Count)
                return;

            var currentStep = tutorial.TutorialSteps[tutorial.CurrentTutorialStep];
            string message = currentStep.Message;

            // Рассчитываем размеры и позицию текста
            Vector2 textSize = _tutorialFont.MeasureString(message);
            Vector2 position = new Vector2(
                (_screenWidth - textSize.X) / 3,  // Центрирование по горизонтали
                500f);                            // Фиксированная позиция сверху

            // Создаем фон для текста
            Rectangle backgroundRect = new Rectangle(
                (int)position.X - 15,
                (int)position.Y - 10,
                (int)textSize.X + 30,
                (int)textSize.Y + 20);

            // Рисуем полупрозрачный фон
            spriteBatch.Draw(
                _tutorialBoxTexture,
                backgroundRect,
                Color.Black * 0.75f);  // Прозрачность 75%

            // Рисуем текст сообщения
            spriteBatch.DrawString(
                _tutorialFont,
                message,
                position,
                Color.White);

            // Если шаг активен, но не завершен - показываем подсказку
            if (currentStep.WasShown && !currentStep.IsCompleted)
            {
                string hint = GetCurrentHint(tutorial.CurrentTutorialStep);
                Vector2 hintSize = _tutorialFont.MeasureString(hint);
                Vector2 hintPosition = new Vector2(
                    (_screenWidth - hintSize.X) / 3,
                    position.Y + textSize.Y + 15);

                spriteBatch.DrawString(
                    _tutorialFont,
                    hint,
                    hintPosition,
                    Color.Gold);
            }
        }

        private string GetCurrentHint(int stepIndex)
        {
            switch (stepIndex)
            {
                case 0: return "Используйте клавиши AD для движения";
                case 1: return "Нажмите ПРОБЕЛ для прыжка";
                case 2: return "Нажмите F для атаки зомби";
                default: return "Выполните текущее задание";
            }
        }

        private void UpdateTutorial(GameTime gameTime)
        {
            if (_currentLevelIndex != 0 || !_levels[0].IsTutorialLevel) return;

            var tutorial = _levels[0];
            if (tutorial.TutorialCompleted)
            {
                UpdateGameplay(gameTime);
                return;
            }

            var currentStep = tutorial.TutorialSteps[tutorial.CurrentTutorialStep];

            // Активируем шаг при первом показе
            if (!currentStep.WasShown)
            {
                currentStep.OnShow?.Invoke();
                currentStep.WasShown = true;
            }

            // Проверяем выполнение условия
            if (!currentStep.IsCompleted && currentStep.CompletionCondition())
            {
                currentStep.IsCompleted = true;

                // Переходим к следующему шагу или завершаем туториал
                if (tutorial.CurrentTutorialStep < tutorial.TutorialSteps.Count - 1)
                {
                    tutorial.CurrentTutorialStep++;
                }
                else
                {
                    tutorial.TutorialCompleted = true;
                    _showTutorial = false;
                }
            }
        }

        private void UpdateGameplay(GameTime gameTime)
        {
            // Обычная игровая логика
            _player.Update(gameTime, _obstacles, _zombies);

            foreach (var zombie in _zombies)
            {
                zombie.Update(gameTime, _player, _obstacles);
            }

            // Проверка победы/поражения
            if (_player.GetLives() <= 0)
            {
                _gameState = GameState.GameOver;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _gameView.Draw(spriteBatch, _player, _obstacles);
            // Рисуем фон
            spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, _screenWidth, _screenHeight), Color.White);

            if (_gameState == GameState.Menu)
            {
                if (_inLevelSelection)
                {
                    // Рисуем выбор уровня
                    spriteBatch.DrawString(_font, "Выберите уровень",
                        new Vector2(_screenWidth / 2 - 100, 100), Color.White);

                    for (int i = 0; i < _levelButtons.Count; i++)
                    {
                        var button = _levelButtons[i];
                        spriteBatch.Draw(_levelButtonTexture, button, Color.White);

                        string levelName = $"{i + 1}. {_levels[i].Name}";
                        Vector2 textSize = _font.MeasureString(levelName);
                        spriteBatch.DrawString(_font, levelName,
                            new Vector2(
                                button.X + button.Width / 2 - textSize.X / 2,
                                button.Y + button.Height / 2 - textSize.Y / 2),
                            Color.Black);
                    }

                    // Кнопка "Назад"
                    spriteBatch.Draw(_buttonTexture, _backButton, Color.White);
                    spriteBatch.DrawString(_font, "Назад",
                        new Vector2(
                            _backButton.X + _backButton.Width / 2 - 30,
                            _backButton.Y + _backButton.Height / 2 - 15),
                        Color.Black);
                }
                else
                {
                    // Главное меню
                    spriteBatch.Draw(_buttonTexture, _buttonRectangle, Color.White);
                    spriteBatch.DrawString(_font, "Играть",
                        new Vector2(
                            _buttonRectangle.X + _buttonRectangle.Width / 2 - 40,
                            _buttonRectangle.Y + _buttonRectangle.Height / 2 - 15),
                        Color.Black);
                }
            }

            else if (_gameState == GameState.Playing)
            {
                spriteBatch.End(); //закрываем спрайтбатч
                spriteBatch.Begin(transformMatrix: _camera.Transform);
                _gameView.Draw(spriteBatch, _player, _obstacles);

                if (_currentLevelIndex == 0 && _levels[0].IsTutorialLevel && !_levels[0].TutorialCompleted)
                {
                    DrawTutorial(spriteBatch);
                }

                foreach (Zombie zombie in _zombies)
                {
                    zombie.Draw(spriteBatch);
                }

                // Если портал активен - рисуем его
                if (_gameState == GameState.Playing && _portal.IsActive)
                {
                    _portal.Draw(spriteBatch);
                }

                // Завершаем спрайтбатч с трансформацией камеры
                spriteBatch.End();

                // Начинаем новый спрайтбатч БЕЗ трансформации камеры
                spriteBatch.Begin();

                // Отрисовываем жизни игрока
                DrawPlayerLives(spriteBatch);
                DrawHealthBar(spriteBatch);


                // Отрисовка счетчика зомби (теперь в координатах экрана)
                DrawZombieCounter(spriteBatch);
                // Завершаем этот спрайтбатч
                spriteBatch.End();

                // Возобновляем спрайтбатч с трансформацией камеры для остальных объектов
                spriteBatch.Begin(transformMatrix: _camera.Transform);


            }

            else if (_gameState == GameState.GameOver)
            {
                // Рисуем экран "Игра окончена"
                string gameOverText = "Игра окончена";
                Vector2 textSize = _font.MeasureString(gameOverText);
                Vector2 textPosition = new Vector2(
                    (_screenWidth - textSize.X) / 2,
                    (_screenHeight - textSize.Y) / 2 - 150); // Сдвигаем текст немного вверх

                spriteBatch.DrawString(_font, gameOverText, textPosition, Color.Red);

                // Рисуем кнопку "Попробовать еще раз"
                spriteBatch.Draw(_buttonTexture, _restartButtonRectangle, Color.White);

                // Рисуем текст на кнопке
                string buttonText = "Попробовать еще раз";
                Vector2 buttonTextSize = _font.MeasureString(buttonText);
                Vector2 buttonTextPosition = new Vector2(
                    _restartButtonRectangle.X + (_restartButtonRectangle.Width - buttonTextSize.X) / 2,
                    _restartButtonRectangle.Y + (_restartButtonRectangle.Height - buttonTextSize.Y) / 2);

                spriteBatch.DrawString(_font, buttonText, buttonTextPosition, Color.Black);
            }

            else if (_gameState == GameState.Victory)
            {
                // Рисуем экран победы
                string victoryText = "Победа!";
                Vector2 textSize = _font.MeasureString(victoryText);
                Vector2 textPosition = new Vector2(
                    (_screenWidth - textSize.X) / 2,
                    (_screenHeight - textSize.Y) / 2 - 100);

                spriteBatch.DrawString(_font, victoryText, textPosition, Color.Gold);

                // Кнопка "В меню"
                spriteBatch.Draw(_buttonTexture, _restartButtonRectangle, Color.White);
                string buttonText = "Back to Menu";
                Vector2 buttonTextSize = _font.MeasureString(buttonText);
                Vector2 buttonTextPosition = new Vector2(
                    _restartButtonRectangle.X + (_restartButtonRectangle.Width - buttonTextSize.X) / 2,
                    _restartButtonRectangle.Y + (_restartButtonRectangle.Height - buttonTextSize.Y) / 2);

                spriteBatch.DrawString(_font, buttonText, buttonTextPosition, Color.Black);
            }

        }

        private void DrawPlayerLives(SpriteBatch spriteBatch)
        {
            if (_heartTexture == null || _player == null)
                return;

            // Фиксированные координаты на экране (50, 50)
            Vector2 basePosition = new Vector2(50, 50);

            // Рисуем сердечки
            for (int i = 0; i < _player.GetLives(); i++)
            {
                spriteBatch.Draw(
                    _heartTexture,
                    basePosition + new Vector2(i * 40, 0), // 40 - расстояние между сердцами
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    0.5f, // Масштаб
                    SpriteEffects.None,
                    0f);
            }
        }

        private void InitializeLevels()
        {
            // Туториал
            var tutorial = new Level
            {
                Name = "Обучение",
                IsTutorialLevel = true,
                MapTexture = "background",
                PlayerStartPosition = new Vector2(100, 1000),
                PortalPosition = new Vector2(2000, 1300),
                ZombiesToKill = 1
            };

            _obstacles.Add(new Obstacle(4500, 0, 800, 10000));
            _obstacles.Add(new Obstacle(-500, 1390 - 100, 5000, 1000));
            tutorial.TutorialSteps.Add(new TutorialStep
            {
                Message = "Двигайтесь: AD",
                OnShow = () => {
                    _obstacles.Clear();
                    _zombies.Clear();
                    _player.Position = new Vector2(100, 1000);
                },
                CompletionCondition = () => _player.Position.X > 500
            });

            tutorial.TutorialSteps.Add(new TutorialStep
            {
                Message = "Прыгайте: ПРОБЕЛ",
                OnShow = () => {
                    _obstacles.Add(new Obstacle(1000, 1300, 300, 500));
                },
                CompletionCondition = () => _player.Position.Y < 1100
            });


            tutorial.TutorialSteps.Add(new TutorialStep
            {
                Message = "Атакуйте зомби: F",
                OnShow = () => {
                    _zombies.Add(new Zombie(new Vector2(500, 100), _zombieTexture));
                },
                CompletionCondition = () => _zombies.Count == 0
            });

            _levels.Add(tutorial);

            // Уровень 1
            var level1 = new Level
            {
                Name = "Начало",
                MapTexture = "background",
                PlayerStartPosition = new Vector2(200, 1390 - 100),  // Старт у левого края
                PortalPosition = new Vector2(2500, 1390 - 200),      // Портал в конце
                ZombiesToKill = 3
            };

            level1.Obstacles.Add(new Obstacle(8000, 0, 800, 10000));
            level1.Obstacles.Add(new Obstacle(-500, 1390, 8000, 1000));
            //Три клетки
            level1.Obstacles.Add(new Obstacle(500, 1390 - 500, 50, 500));   // Левая стенка клетки 1
            level1.Obstacles.Add(new Obstacle(500, 1390 - 550, 300, 50));  // Верх клетки 1 
            level1.Obstacles.Add(new Obstacle(800, 1390 - 500, 50, 300));   // Правая стенка клетки 1

            level1.Obstacles.Add(new Obstacle(1200, 1390 - 500, 50, 500));  // Клетка 2
            level1.Obstacles.Add(new Obstacle(1200, 1390 - 550, 300, 50));
            level1.Obstacles.Add(new Obstacle(1500, 1390 - 500, 50, 300));

            level1.Obstacles.Add(new Obstacle(1900, 1390 - 500, 50, 500));  // Клетка 3
            level1.Obstacles.Add(new Obstacle(1900, 1390 - 550, 300, 50));
            level1.Obstacles.Add(new Obstacle(2200, 1390 - 500, 50, 300));

            level1.Obstacles.Add(new Obstacle(400, 1390 - 450, 100, 50));   // Подход к клетке 1
            level1.Obstacles.Add(new Obstacle(1100, 1390 - 450, 100, 50)); // К клетке 2
            level1.Obstacles.Add(new Obstacle(1800, 1390 - 450, 100, 50)); // К клетке 3

            // --- ЗОМБИ В КЛЕТКАХ ---
            level1.ZombieSpawnPoints.Add(new Vector2(650, 1390 - 200));   // Клетка 1
            level1.ZombieSpawnPoints.Add(new Vector2(1350, 1390 - 200)); // Клетка 2
            level1.ZombieSpawnPoints.Add(new Vector2(2050, 1390 - 200)); // Клетка 3

            _levels.Add(level1);
            // Уровень 2 - "БЕГИ"
            var level2 = new Level
            {
                Name = "Не упади",
                MapTexture = "background",
                PlayerStartPosition = new Vector2(200, 1390 - 100), 
                PortalPosition = new Vector2(6200, 1390 - 300),     
                ZombiesToKill = 0                               
            };

            level2.Obstacles.Add(new Obstacle(8000, 0, 800, 10000));
            level2.Obstacles.Add(new Obstacle(-500, 1390-150, 8500, 1000));
            level2.Obstacles.Add(new Obstacle(800, 1390 - 350, 50, 200));
            level2.Obstacles.Add(new Obstacle(1500, 1390 - 450, 50, 300));
            level2.Obstacles.Add(new Obstacle(2200, 1390 - 550, 50, 400)); 
            level2.Obstacles.Add(new Obstacle(3000, 1390 - 650, 50, 500));  
            level2.Obstacles.Add(new Obstacle(3700, 1390 - 750, 50, 600)); 
            level2.Obstacles.Add(new Obstacle(4400, 1390 - 850, 50, 700)); 
            level2.Obstacles.Add(new Obstacle(5100, 1390 - 950, 50, 800));  
            level2.Obstacles.Add(new Obstacle(5800, 1390 - 1050, 50, 900));  

            for (int i = 0; i < 40; i++)
            {
                level2.ZombieSpawnPoints.Add(new Vector2(100 + i * 200, 1390 - 200));
            }
            _levels.Add(level2);

            // Уровень 3
            var level3 = new Level
            {
                Name = "Лабиринт",
                MapTexture = "background",
                PlayerStartPosition = new Vector2(200, 1390 - 100),
                PortalPosition = new Vector2(4200, 1390 - 1000), 
                ZombiesToKill = 5
            };

            // Улучшенный лабиринт с проходами
            level3.Obstacles.Add(new Obstacle(4500, 0, 800, 10000));
            level3.Obstacles.Add(new Obstacle(-500, 1390-100, 5000, 1000));
            level3.Obstacles.Add(new Obstacle(500, 1390 - 300, 50, 300));  // Левая стена
            level3.Obstacles.Add(new Obstacle(500, 1390 - 1100, 1000, 50));  // Верхняя стена
            level3.Obstacles.Add(new Obstacle(1500, 1390 - 600, 50, 600));  // Центральная стена (короче)
            level3.Obstacles.Add(new Obstacle(2000, 1390 - 300, 50, 300));  // Правая стена
            level3.Obstacles.Add(new Obstacle(1000, 1390 - 800, 500, 50));  // Горизонтальная перегородка
            level3.Obstacles.Add(new Obstacle(2500, 1390 - 400, 50, 400));  // Доп. преграда
            level3.Obstacles.Add(new Obstacle(3000, 1390 - 900, 800, 50));  // Верхний проход

            level3.ZombieSpawnPoints.AddRange(new[] {
            new Vector2(700, 1390 - 400),
            new Vector2(1200, 1390 - 700), 
            new Vector2(1800, 1390 - 500),
            new Vector2(2200, 1390 - 900),
            new Vector2(3500, 1390 - 500)});

            _levels.Add(level3);

            // Уровень 4
            var level4 = new Level
            {
                Name = "Осада",
                MapTexture = "background",
                PlayerStartPosition = new Vector2(400, 1390 - 100),
                PortalPosition = new Vector2(3800, 1390 - 800),
                ZombiesToKill = 5
            };

            level4.Obstacles.Add(new Obstacle(4500, 0, 800, 10000));
            level4.Obstacles.Add(new Obstacle(-500, 1390 - 100, 5000, 1000));
            // Платформы с защитными стенами
            level4.Obstacles.Add(new Obstacle(600, 1390 - 300, 200, 50));  // Основная платформа 1
            level4.Obstacles.Add(new Obstacle(550, 1390 - 450, 50, 150));  // Защитная стена слева
            level4.Obstacles.Add(new Obstacle(800, 1390 - 450, 50, 150));  // Защитная стена справа

            level4.Obstacles.Add(new Obstacle(1000, 1390 - 500, 200, 50)); // Платформа 2
            level4.Obstacles.Add(new Obstacle(950, 1390 - 650, 50, 150));
            level4.Obstacles.Add(new Obstacle(1200, 1390 - 650, 50, 150));

            level4.Obstacles.Add(new Obstacle(1400, 1390 - 500, 200, 50)); // Платформа 3
            level4.Obstacles.Add(new Obstacle(1350, 1390 - 650, 50, 150));
            level4.Obstacles.Add(new Obstacle(1600, 1390 - 650, 50, 150));

            level4.Obstacles.Add(new Obstacle(1800, 1390 - 700, 200, 50)); // Платформа 4
            level4.Obstacles.Add(new Obstacle(1750, 1390 - 850, 50, 150));
            level4.Obstacles.Add(new Obstacle(2000, 1390 - 850, 50, 150));
            // Волны зомби с разных сторон
            for (int i = 0; i < 7; i++)
            {
                // Верхние снайперы (не двигаются)
                level4.ZombieSpawnPoints.Add(new Vector2(800 + i * 400, 1390 - 700));

                // Нижние атакующие
                level4.ZombieSpawnPoints.Add(new Vector2(500 + i * 300, 1390 - 50));
            }

            // Защитные позиции
            level4.ZombieSpawnPoints.Add(new Vector2(1800, 1390 - 400));  // На возвышении
            level4.ZombieSpawnPoints.Add(new Vector2(2500, 1390 - 300));  // За баррикадой

            _levels.Add(level4);

            // Уровень 5 - Паркур (исправленная версия)
            var level5 = new Level
            {
                Name = "Паркур",
                MapTexture = "background",
                PlayerStartPosition = new Vector2(200, 1390 - 50),
                PortalPosition = new Vector2(3500, 1390 - 1100), 
                ZombiesToKill = 0  // Нужно только добежать
            };

            // Основные препятствия
            level5.Obstacles.Add(new Obstacle(-500, 1390 - 100, 5000, 1000));  // Земля
            level5.Obstacles.Add(new Obstacle(4500, 0, 800, 10000));
            // Паркур-платформы с постепенным подъемом
            level5.Obstacles.Add(new Obstacle(500, 1390 - 200, 100, 30));
            level5.Obstacles.Add(new Obstacle(800, 1390 - 300, 100, 30));
            level5.Obstacles.Add(new Obstacle(1100, 1390 - 400, 100, 30));
            level5.Obstacles.Add(new Obstacle(1400, 1390 - 500, 100, 30));
            level5.Obstacles.Add(new Obstacle(1700, 1390 - 600, 100, 30));
            level5.Obstacles.Add(new Obstacle(2000, 1390 - 700, 100, 30));
            level5.Obstacles.Add(new Obstacle(2300, 1390 - 800, 100, 30));
            level5.Obstacles.Add(new Obstacle(2600, 1390 - 900, 100, 30));
            level5.Obstacles.Add(new Obstacle(2900, 1390 - 1000, 100, 30));

            // Финишная площадка с порталом
            level5.Obstacles.Add(new Obstacle(3200, 1390 - 1000, 300, 30));  // Площадка перед порталом

            // Быстрые зомби, мешающие прыгать
            level5.ZombieSpawnPoints.AddRange(new[] {
    new Vector2(600, 1390 - 150),
    new Vector2(1000, 1390 - 350),
    new Vector2(1400, 1390 - 550),
    new Vector2(1800, 1390 - 650),
    new Vector2(2200, 1390 - 750),
    new Vector2(2600, 1390 - 850)});

            _levels.Add(level5);

            // Уровень 6
            var level6 = new Level
            {
                Name = "Темный лес",
                MapTexture = "background",  // Специальная текстура
                PlayerStartPosition = new Vector2(200, 1390 - 100),
                PortalPosition = new Vector2(3200, 1390 - 500),  // Портал на дереве
                ZombiesToKill = 8
            };

            level6.Obstacles.Add(new Obstacle(4500, 0, 800, 10000));
            level6.Obstacles.Add(new Obstacle(-500, 1390 - 100, 5000, 1000));
            // Деревья с проходами
            level6.Obstacles.Add(new Obstacle(600, 1390 - 300, 100, 300));
            level6.Obstacles.Add(new Obstacle(1000, 1390 - 500, 100, 500));
            level6.Obstacles.Add(new Obstacle(1500, 1390 - 200, 100, 400));  // Короче
            level6.Obstacles.Add(new Obstacle(2000, 1390 - 600, 100, 600));
            level6.Obstacles.Add(new Obstacle(2500, 1390 - 400, 100, 400));  // Новое препятствие

            // 8 зомби в засадах
            level6.ZombieSpawnPoints.AddRange(new[] {
            new Vector2(650, 1390 - 200),   // За первым деревом
            new Vector2(1050, 1390 - 400),  // Центральная засада
            new Vector2(1550, 1390 - 100),  // На возвышении
            new Vector2(2050, 1390 - 500),  // Правая сторона
            new Vector2(1200, 1390 - 700),  // Новые
            new Vector2(1800, 1390 - 300),
            new Vector2(2300, 1390 - 200),
            new Vector2(2800, 1390 - 400)});

            _levels.Add(level6);

            // Уровень 7

            var level7 = new Level
            {
                Name = "Арена",
                MapTexture = "background",
                PlayerStartPosition = new Vector2(400, 1390 - 100),
                PortalPosition = new Vector2(2500, 1390 - 1200),  // Портал на пьедестале
                ZombiesToKill = 15  // Стратегический бой
            };

            level7.Obstacles.Add(new Obstacle(4500, 0, 800, 10000));
            level7.Obstacles.Add(new Obstacle(-500, 1390 - 100, 5000, 1000));
            // Круговая арена с колоннами
            level7.Obstacles.Add(new Obstacle(800, 1390 - 200, 200, 50));   // Центральная платформа
            level7.Obstacles.Add(new Obstacle(1200, 1390 - 500, 50, 300)); // Колонна 1
            level7.Obstacles.Add(new Obstacle(1800, 1390 - 300, 50, 300)); // Колонна 2
            level7.Obstacles.Add(new Obstacle(2200, 1390 - 600, 50, 300)); // Колонна 3

            for (int i = 0; i < 5; i++)
            {
                level7.ZombieSpawnPoints.Add(new Vector2(1000 + i * 150, 1390 - 50));
                level7.ZombieSpawnPoints.Add(new Vector2(1500 + i * 100, 1390 - 800));
                level7.ZombieSpawnPoints.Add(new Vector2(2000 + i * 120, 1390 - 400));
            }

            _levels.Add(level7);
        }

        private void CreateLevelButtons()
        {
            _levelButtons.Clear();
            int buttonWidth = 300;
            int buttonHeight = 80;
            int startX = (_screenWidth - buttonWidth) / 2;
            int startY = 200;
            int spacing = 30;

            for (int i = 0; i < _levels.Count; i++)
            {
                _levelButtons.Add(new Rectangle(
                    startX,
                    startY + i * (buttonHeight + spacing),
                    buttonWidth,
                    buttonHeight));
            }
            _backButton = new Rectangle(
        (_screenWidth - 200) / 2,
        _screenHeight - 100,
        200,
        50);
        }

        private void DrawHealthBar(SpriteBatch spriteBatch)
        {
            if (_player == null || _healthBarTexture == null || _healthBarBackgroundTexture == null)
                return;

            // Позиция и размеры шкалы здоровья
            Vector2 healthBarPosition = new Vector2(50, 20);
            int healthBarWidth = 200;
            int healthBarHeight = 20;

            // Рисуем фон шкалы здоровья
            spriteBatch.Draw(_healthBarBackgroundTexture,
                new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y,
                             healthBarWidth, healthBarHeight),
                Color.White);

            // Рассчитываем текущую ширину заполненной части
            int currentHealthWidth = (int)(healthBarWidth * (_player.GetHealth() / 100f));

            // Рисуем заполненную часть шкалы здоровья
            if (currentHealthWidth > 0)
            {
                spriteBatch.Draw(_healthBarTexture,
                    new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y,
                                 currentHealthWidth, healthBarHeight),
                    Color.White);
            }

            // Текст с числовым значением здоровья
            string healthText = $"{_player.GetHealth()}%";
            Vector2 textPosition = new Vector2(
                healthBarPosition.X + healthBarWidth + 10,
                healthBarPosition.Y);

            spriteBatch.DrawString(_font, healthText, textPosition, Color.White);
        }

        private void StartLevel(int levelIndex)
        {
            _currentLevelIndex = levelIndex;
            Level level = _levels[levelIndex];
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(_backgroundMusic);

            // Полная очистка перед загрузкой уровня
            _obstacles.Clear();
            _zombies.Clear();

            // Загружаем уровень
            _backgroundTexture = content.Load<Texture2D>(level.MapTexture);
            _player.Position = level.PlayerStartPosition;
            _player.ResetLives(); // Сброс жизней игрока

            // Пересоздаем портал
            _portal = new Portal(level.PortalPosition, _graphicsDevice);
            _portal.LoadContent(content);
            _portal.Deactivate(); // Деактивируем на старте уровня

            // Загружаем препятствия из уровня
            _obstacles.AddRange(level.Obstacles);

            // Спавним зомби
            foreach (var spawnPoint in level.ZombieSpawnPoints)
            {
                _zombies.Add(new Zombie(spawnPoint, _zombieTexture));
            }

            // Сброс счетчиков
            _zombiesKilled = 0;
            _zombiesToKill = level.ZombiesToKill;

            // Сброс состояния игры
            _gameState = GameState.Playing;

            // Если это туториал - сбросить его состояние
            if (levelIndex == 0)
            {
                _showTutorial = true;
                _tutorialTimer = 0f;
                level.CurrentTutorialStep = 0;
                foreach (var step in level.TutorialSteps)
                {
                    step.WasShown = false;
                    step.IsCompleted = false;
                }
            }

            _camera = new Camera(new Viewport(0, 0, _screenWidth, _screenHeight));
        }
    }

}
