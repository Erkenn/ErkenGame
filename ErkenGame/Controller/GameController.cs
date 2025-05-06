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
        private int _zombiesToKill = 1; // ������� ����� ����� ����� ��� ������
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
            _portal = new Portal(new Vector2(2500, 1300), _graphicsDevice); // �������������� �����
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
            Texture2D playerTexture = content.Load<Texture2D>("Character"); // �������� �������� ������
            _zombieTexture = content.Load<Texture2D>("Zombie"); // �������� �������� �����

            _player = new Player(new Vector2(100, 100), playerTexture); // �������� ���������� ������
            _zombies.Add(new Zombie(new Vector2(500, 100), _zombieTexture)); // �������� ���������� �����
            _player.LoadAnimations(content);
            _gameView = new GameView(content);
            _heartTexture = content.Load<Texture2D>("Heart");

            // ����������� ������� � ������� ������
            int buttonWidth = 400;
            int buttonHeight = 100;
            int buttonX = (_screenWidth - buttonWidth) / 2;
            int buttonY = (_screenHeight - buttonHeight) / 2;

            _restartButtonRectangle = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
            _buttonRectangle = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
            _portal.LoadContent(content);
            _gameView.LoadContent(content); // ��������� �������� � GameView
            _zombieIconTexture = content.Load<Texture2D>("ZombieIcon");
            InitializeLevels();
            CreateLevelButtons();
            _camera = new Camera(_graphicsDevice.Viewport); // �������������� ������
        }

        private void UpdateEndGameStates(MouseState mouseState)
        {
            // ��������� ������ � GameOver/Victory
            if (_restartButtonRectangle.Contains(mouseState.Position) &&
                mouseState.LeftButton == ButtonState.Pressed &&
                _lastMouseState.LeftButton == ButtonState.Released)
            {
                if (_gameState == GameState.GameOver)
                {
                    // ������������� ������� ������� ��� GameOver
                    StartLevel(_currentLevelIndex);
                }
                else if (_gameState == GameState.Victory)
                {
                    // ���������� � ���� ��� Victory
                    ReturnToMainMenu();
                }
            }
        }

        private void ReturnToMainMenu()
        {
            _gameState = GameState.Menu;
            _inLevelSelection = false;

            // ���������� ������ ��� ������ ������������ ������
            _player.ResetLives();
            _player.Position = new Vector2(100, 100);

            // ������� ����� � �����������
            _zombies.Clear();
            _obstacles.Clear();

            // ���������� ������
            _camera = new Camera(_graphicsDevice.Viewport);
        }

        private void UpdateMenu(MouseState mouseState)
        {
            if (_inLevelSelection)
            {
                // ��������� ������ ������
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

                // ��������� ������ "�����"
                if (_backButton.Contains(mouseState.Position) &&
                    mouseState.LeftButton == ButtonState.Pressed &&
                    _lastMouseState.LeftButton == ButtonState.Released)
                {
                    _inLevelSelection = false;
                }
            }
            else
            {
                // ��������� ������ "������" � ������� ����
                if (_buttonRectangle.Contains(mouseState.Position) &&
                    mouseState.LeftButton == ButtonState.Pressed &&
                    _lastMouseState.LeftButton == ButtonState.Released)
                {
                    _inLevelSelection = true; // ��������� � ������ ������
                }
            }
        }


        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var currentMouseState = Mouse.GetState();
            var currentKeyboardState = Keyboard.GetState();

            // ��������� ��������� ����
            switch (_gameState)
            {
                case GameState.Menu:
                    UpdateMenu(currentMouseState);
                    break;

                case GameState.Playing:
                    // ���������� �������� ��������
                    _camera.Update(_player.Position, deltaTime);
                    _player.Update(gameTime, _obstacles, _zombies);

                    // �������� ������ ������
                    if (_player.GetLives() <= 0)
                    {
                        _gameState = GameState.GameOver;
                    }

                    // ��������� �����
                    for (int i = _zombies.Count - 1; i >= 0; i--)
                    {
                        Zombie zombie = _zombies[i];
                        zombie.Update(gameTime, _player, _obstacles);

                        // ���� ����� �����, ������� ��� �� ������
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

                    // ���������� ��������� (���� �����)
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
            // ������� � ����������� ������ (������� ������ ����)
            Vector2 counterPosition = new Vector2(_screenWidth - 220, 20);

            // ��� ��� ����������
            Rectangle bgRect = new Rectangle(
                (int)counterPosition.X - 10,
                (int)counterPosition.Y - 5,
                200,
                30);

            // ����� ��������
            string counterText = $"�����: {_zombiesKilled}/{_zombiesToKill}";
            spriteBatch.DrawString(_font, counterText, counterPosition, Color.White);

            // ������ ����� (���� ����)
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

            // ������������ ������� � ������� ������
            Vector2 textSize = _tutorialFont.MeasureString(message);
            Vector2 position = new Vector2(
                (_screenWidth - textSize.X) / 3,  // ������������� �� �����������
                500f);                            // ������������� ������� ������

            // ������� ��� ��� ������
            Rectangle backgroundRect = new Rectangle(
                (int)position.X - 15,
                (int)position.Y - 10,
                (int)textSize.X + 30,
                (int)textSize.Y + 20);

            // ������ �������������� ���
            spriteBatch.Draw(
                _tutorialBoxTexture,
                backgroundRect,
                Color.Black * 0.75f);  // ������������ 75%

            // ������ ����� ���������
            spriteBatch.DrawString(
                _tutorialFont,
                message,
                position,
                Color.White);

            // ���� ��� �������, �� �� �������� - ���������� ���������
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
                case 0: return "����������� ������� AD ��� ��������";
                case 1: return "������� ������ ��� ������";
                case 2: return "������� F ��� ����� �����";
                default: return "��������� ������� �������";
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

            // ���������� ��� ��� ������ ������
            if (!currentStep.WasShown)
            {
                currentStep.OnShow?.Invoke();
                currentStep.WasShown = true;
            }

            // ��������� ���������� �������
            if (!currentStep.IsCompleted && currentStep.CompletionCondition())
            {
                currentStep.IsCompleted = true;

                // ��������� � ���������� ���� ��� ��������� ��������
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
            // ������� ������� ������
            _player.Update(gameTime, _obstacles, _zombies);

            foreach (var zombie in _zombies)
            {
                zombie.Update(gameTime, _player, _obstacles);
            }

            // �������� ������/���������
            if (_player.GetLives() <= 0)
            {
                _gameState = GameState.GameOver;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _gameView.Draw(spriteBatch, _player, _obstacles);
            // ������ ���
            spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, _screenWidth, _screenHeight), Color.White);

            if (_gameState == GameState.Menu)
            {
                if (_inLevelSelection)
                {
                    // ������ ����� ������
                    spriteBatch.DrawString(_font, "�������� �������",
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

                    // ������ "�����"
                    spriteBatch.Draw(_buttonTexture, _backButton, Color.White);
                    spriteBatch.DrawString(_font, "�����",
                        new Vector2(
                            _backButton.X + _backButton.Width / 2 - 30,
                            _backButton.Y + _backButton.Height / 2 - 15),
                        Color.Black);
                }
                else
                {
                    // ������� ����
                    spriteBatch.Draw(_buttonTexture, _buttonRectangle, Color.White);
                    spriteBatch.DrawString(_font, "������",
                        new Vector2(
                            _buttonRectangle.X + _buttonRectangle.Width / 2 - 40,
                            _buttonRectangle.Y + _buttonRectangle.Height / 2 - 15),
                        Color.Black);
                }
            }

            else if (_gameState == GameState.Playing)
            {
                spriteBatch.End(); //��������� ����������
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

                // ���� ������ ������� - ������ ���
                if (_gameState == GameState.Playing && _portal.IsActive)
                {
                    _portal.Draw(spriteBatch);
                }

                // ��������� ���������� � �������������� ������
                spriteBatch.End();

                // �������� ����� ���������� ��� ������������� ������
                spriteBatch.Begin();

                // ������������ ����� ������
                DrawPlayerLives(spriteBatch);
                DrawHealthBar(spriteBatch);


                // ��������� �������� ����� (������ � ����������� ������)
                DrawZombieCounter(spriteBatch);
                // ��������� ���� ����������
                spriteBatch.End();

                // ������������ ���������� � �������������� ������ ��� ��������� ��������
                spriteBatch.Begin(transformMatrix: _camera.Transform);


            }

            else if (_gameState == GameState.GameOver)
            {
                // ������ ����� "���� ��������"
                string gameOverText = "���� ��������";
                Vector2 textSize = _font.MeasureString(gameOverText);
                Vector2 textPosition = new Vector2(
                    (_screenWidth - textSize.X) / 2,
                    (_screenHeight - textSize.Y) / 2 - 150); // �������� ����� ������� �����

                spriteBatch.DrawString(_font, gameOverText, textPosition, Color.Red);

                // ������ ������ "����������� ��� ���"
                spriteBatch.Draw(_buttonTexture, _restartButtonRectangle, Color.White);

                // ������ ����� �� ������
                string buttonText = "����������� ��� ���";
                Vector2 buttonTextSize = _font.MeasureString(buttonText);
                Vector2 buttonTextPosition = new Vector2(
                    _restartButtonRectangle.X + (_restartButtonRectangle.Width - buttonTextSize.X) / 2,
                    _restartButtonRectangle.Y + (_restartButtonRectangle.Height - buttonTextSize.Y) / 2);

                spriteBatch.DrawString(_font, buttonText, buttonTextPosition, Color.Black);
            }

            else if (_gameState == GameState.Victory)
            {
                // ������ ����� ������
                string victoryText = "������!";
                Vector2 textSize = _font.MeasureString(victoryText);
                Vector2 textPosition = new Vector2(
                    (_screenWidth - textSize.X) / 2,
                    (_screenHeight - textSize.Y) / 2 - 100);

                spriteBatch.DrawString(_font, victoryText, textPosition, Color.Gold);

                // ������ "� ����"
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

            // ������������� ���������� �� ������ (50, 50)
            Vector2 basePosition = new Vector2(50, 50);

            // ������ ��������
            for (int i = 0; i < _player.GetLives(); i++)
            {
                spriteBatch.Draw(
                    _heartTexture,
                    basePosition + new Vector2(i * 40, 0), // 40 - ���������� ����� ��������
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    0.5f, // �������
                    SpriteEffects.None,
                    0f);
            }
        }

        private void InitializeLevels()
        {
            // ��������
            var tutorial = new Level
            {
                Name = "��������",
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
                Message = "����������: AD",
                OnShow = () => {
                    _obstacles.Clear();
                    _zombies.Clear();
                    _player.Position = new Vector2(100, 1000);
                },
                CompletionCondition = () => _player.Position.X > 500
            });

            tutorial.TutorialSteps.Add(new TutorialStep
            {
                Message = "��������: ������",
                OnShow = () => {
                    _obstacles.Add(new Obstacle(1000, 1300, 300, 500));
                },
                CompletionCondition = () => _player.Position.Y < 1100
            });


            tutorial.TutorialSteps.Add(new TutorialStep
            {
                Message = "�������� �����: F",
                OnShow = () => {
                    _zombies.Add(new Zombie(new Vector2(500, 100), _zombieTexture));
                },
                CompletionCondition = () => _zombies.Count == 0
            });

            _levels.Add(tutorial);

            // ������� 1
            var level1 = new Level
            {
                Name = "������",
                MapTexture = "background",
                PlayerStartPosition = new Vector2(200, 1390 - 100),  // ����� � ������ ����
                PortalPosition = new Vector2(2500, 1390 - 200),      // ������ � �����
                ZombiesToKill = 3
            };

            level1.Obstacles.Add(new Obstacle(8000, 0, 800, 10000));
            level1.Obstacles.Add(new Obstacle(-500, 1390, 8000, 1000));
            //��� ������
            level1.Obstacles.Add(new Obstacle(500, 1390 - 500, 50, 500));   // ����� ������ ������ 1
            level1.Obstacles.Add(new Obstacle(500, 1390 - 550, 300, 50));  // ���� ������ 1 
            level1.Obstacles.Add(new Obstacle(800, 1390 - 500, 50, 300));   // ������ ������ ������ 1

            level1.Obstacles.Add(new Obstacle(1200, 1390 - 500, 50, 500));  // ������ 2
            level1.Obstacles.Add(new Obstacle(1200, 1390 - 550, 300, 50));
            level1.Obstacles.Add(new Obstacle(1500, 1390 - 500, 50, 300));

            level1.Obstacles.Add(new Obstacle(1900, 1390 - 500, 50, 500));  // ������ 3
            level1.Obstacles.Add(new Obstacle(1900, 1390 - 550, 300, 50));
            level1.Obstacles.Add(new Obstacle(2200, 1390 - 500, 50, 300));

            level1.Obstacles.Add(new Obstacle(400, 1390 - 450, 100, 50));   // ������ � ������ 1
            level1.Obstacles.Add(new Obstacle(1100, 1390 - 450, 100, 50)); // � ������ 2
            level1.Obstacles.Add(new Obstacle(1800, 1390 - 450, 100, 50)); // � ������ 3

            // --- ����� � ������� ---
            level1.ZombieSpawnPoints.Add(new Vector2(650, 1390 - 200));   // ������ 1
            level1.ZombieSpawnPoints.Add(new Vector2(1350, 1390 - 200)); // ������ 2
            level1.ZombieSpawnPoints.Add(new Vector2(2050, 1390 - 200)); // ������ 3

            _levels.Add(level1);
            // ������� 2 - "����"
            var level2 = new Level
            {
                Name = "�� �����",
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

            // ������� 3
            var level3 = new Level
            {
                Name = "��������",
                MapTexture = "background",
                PlayerStartPosition = new Vector2(200, 1390 - 100),
                PortalPosition = new Vector2(4200, 1390 - 1000), 
                ZombiesToKill = 5
            };

            // ���������� �������� � ���������
            level3.Obstacles.Add(new Obstacle(4500, 0, 800, 10000));
            level3.Obstacles.Add(new Obstacle(-500, 1390-100, 5000, 1000));
            level3.Obstacles.Add(new Obstacle(500, 1390 - 300, 50, 300));  // ����� �����
            level3.Obstacles.Add(new Obstacle(500, 1390 - 1100, 1000, 50));  // ������� �����
            level3.Obstacles.Add(new Obstacle(1500, 1390 - 600, 50, 600));  // ����������� ����� (������)
            level3.Obstacles.Add(new Obstacle(2000, 1390 - 300, 50, 300));  // ������ �����
            level3.Obstacles.Add(new Obstacle(1000, 1390 - 800, 500, 50));  // �������������� �����������
            level3.Obstacles.Add(new Obstacle(2500, 1390 - 400, 50, 400));  // ���. ��������
            level3.Obstacles.Add(new Obstacle(3000, 1390 - 900, 800, 50));  // ������� ������

            level3.ZombieSpawnPoints.AddRange(new[] {
            new Vector2(700, 1390 - 400),
            new Vector2(1200, 1390 - 700), 
            new Vector2(1800, 1390 - 500),
            new Vector2(2200, 1390 - 900),
            new Vector2(3500, 1390 - 500)});

            _levels.Add(level3);

            // ������� 4
            var level4 = new Level
            {
                Name = "�����",
                MapTexture = "background",
                PlayerStartPosition = new Vector2(400, 1390 - 100),
                PortalPosition = new Vector2(3800, 1390 - 800),
                ZombiesToKill = 5
            };

            level4.Obstacles.Add(new Obstacle(4500, 0, 800, 10000));
            level4.Obstacles.Add(new Obstacle(-500, 1390 - 100, 5000, 1000));
            // ��������� � ��������� �������
            level4.Obstacles.Add(new Obstacle(600, 1390 - 300, 200, 50));  // �������� ��������� 1
            level4.Obstacles.Add(new Obstacle(550, 1390 - 450, 50, 150));  // �������� ����� �����
            level4.Obstacles.Add(new Obstacle(800, 1390 - 450, 50, 150));  // �������� ����� ������

            level4.Obstacles.Add(new Obstacle(1000, 1390 - 500, 200, 50)); // ��������� 2
            level4.Obstacles.Add(new Obstacle(950, 1390 - 650, 50, 150));
            level4.Obstacles.Add(new Obstacle(1200, 1390 - 650, 50, 150));

            level4.Obstacles.Add(new Obstacle(1400, 1390 - 500, 200, 50)); // ��������� 3
            level4.Obstacles.Add(new Obstacle(1350, 1390 - 650, 50, 150));
            level4.Obstacles.Add(new Obstacle(1600, 1390 - 650, 50, 150));

            level4.Obstacles.Add(new Obstacle(1800, 1390 - 700, 200, 50)); // ��������� 4
            level4.Obstacles.Add(new Obstacle(1750, 1390 - 850, 50, 150));
            level4.Obstacles.Add(new Obstacle(2000, 1390 - 850, 50, 150));
            // ����� ����� � ������ ������
            for (int i = 0; i < 7; i++)
            {
                // ������� �������� (�� ���������)
                level4.ZombieSpawnPoints.Add(new Vector2(800 + i * 400, 1390 - 700));

                // ������ ���������
                level4.ZombieSpawnPoints.Add(new Vector2(500 + i * 300, 1390 - 50));
            }

            // �������� �������
            level4.ZombieSpawnPoints.Add(new Vector2(1800, 1390 - 400));  // �� ����������
            level4.ZombieSpawnPoints.Add(new Vector2(2500, 1390 - 300));  // �� ����������

            _levels.Add(level4);

            // ������� 5 - ������ (������������ ������)
            var level5 = new Level
            {
                Name = "������",
                MapTexture = "background",
                PlayerStartPosition = new Vector2(200, 1390 - 50),
                PortalPosition = new Vector2(3500, 1390 - 1100), 
                ZombiesToKill = 0  // ����� ������ ��������
            };

            // �������� �����������
            level5.Obstacles.Add(new Obstacle(-500, 1390 - 100, 5000, 1000));  // �����
            level5.Obstacles.Add(new Obstacle(4500, 0, 800, 10000));
            // ������-��������� � ����������� ��������
            level5.Obstacles.Add(new Obstacle(500, 1390 - 200, 100, 30));
            level5.Obstacles.Add(new Obstacle(800, 1390 - 300, 100, 30));
            level5.Obstacles.Add(new Obstacle(1100, 1390 - 400, 100, 30));
            level5.Obstacles.Add(new Obstacle(1400, 1390 - 500, 100, 30));
            level5.Obstacles.Add(new Obstacle(1700, 1390 - 600, 100, 30));
            level5.Obstacles.Add(new Obstacle(2000, 1390 - 700, 100, 30));
            level5.Obstacles.Add(new Obstacle(2300, 1390 - 800, 100, 30));
            level5.Obstacles.Add(new Obstacle(2600, 1390 - 900, 100, 30));
            level5.Obstacles.Add(new Obstacle(2900, 1390 - 1000, 100, 30));

            // �������� �������� � ��������
            level5.Obstacles.Add(new Obstacle(3200, 1390 - 1000, 300, 30));  // �������� ����� ��������

            // ������� �����, �������� �������
            level5.ZombieSpawnPoints.AddRange(new[] {
    new Vector2(600, 1390 - 150),
    new Vector2(1000, 1390 - 350),
    new Vector2(1400, 1390 - 550),
    new Vector2(1800, 1390 - 650),
    new Vector2(2200, 1390 - 750),
    new Vector2(2600, 1390 - 850)});

            _levels.Add(level5);

            // ������� 6
            var level6 = new Level
            {
                Name = "������ ���",
                MapTexture = "background",  // ����������� ��������
                PlayerStartPosition = new Vector2(200, 1390 - 100),
                PortalPosition = new Vector2(3200, 1390 - 500),  // ������ �� ������
                ZombiesToKill = 8
            };

            level6.Obstacles.Add(new Obstacle(4500, 0, 800, 10000));
            level6.Obstacles.Add(new Obstacle(-500, 1390 - 100, 5000, 1000));
            // ������� � ���������
            level6.Obstacles.Add(new Obstacle(600, 1390 - 300, 100, 300));
            level6.Obstacles.Add(new Obstacle(1000, 1390 - 500, 100, 500));
            level6.Obstacles.Add(new Obstacle(1500, 1390 - 200, 100, 400));  // ������
            level6.Obstacles.Add(new Obstacle(2000, 1390 - 600, 100, 600));
            level6.Obstacles.Add(new Obstacle(2500, 1390 - 400, 100, 400));  // ����� �����������

            // 8 ����� � �������
            level6.ZombieSpawnPoints.AddRange(new[] {
            new Vector2(650, 1390 - 200),   // �� ������ �������
            new Vector2(1050, 1390 - 400),  // ����������� ������
            new Vector2(1550, 1390 - 100),  // �� ����������
            new Vector2(2050, 1390 - 500),  // ������ �������
            new Vector2(1200, 1390 - 700),  // �����
            new Vector2(1800, 1390 - 300),
            new Vector2(2300, 1390 - 200),
            new Vector2(2800, 1390 - 400)});

            _levels.Add(level6);

            // ������� 7

            var level7 = new Level
            {
                Name = "�����",
                MapTexture = "background",
                PlayerStartPosition = new Vector2(400, 1390 - 100),
                PortalPosition = new Vector2(2500, 1390 - 1200),  // ������ �� ����������
                ZombiesToKill = 15  // �������������� ���
            };

            level7.Obstacles.Add(new Obstacle(4500, 0, 800, 10000));
            level7.Obstacles.Add(new Obstacle(-500, 1390 - 100, 5000, 1000));
            // �������� ����� � ���������
            level7.Obstacles.Add(new Obstacle(800, 1390 - 200, 200, 50));   // ����������� ���������
            level7.Obstacles.Add(new Obstacle(1200, 1390 - 500, 50, 300)); // ������� 1
            level7.Obstacles.Add(new Obstacle(1800, 1390 - 300, 50, 300)); // ������� 2
            level7.Obstacles.Add(new Obstacle(2200, 1390 - 600, 50, 300)); // ������� 3

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

            // ������� � ������� ����� ��������
            Vector2 healthBarPosition = new Vector2(50, 20);
            int healthBarWidth = 200;
            int healthBarHeight = 20;

            // ������ ��� ����� ��������
            spriteBatch.Draw(_healthBarBackgroundTexture,
                new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y,
                             healthBarWidth, healthBarHeight),
                Color.White);

            // ������������ ������� ������ ����������� �����
            int currentHealthWidth = (int)(healthBarWidth * (_player.GetHealth() / 100f));

            // ������ ����������� ����� ����� ��������
            if (currentHealthWidth > 0)
            {
                spriteBatch.Draw(_healthBarTexture,
                    new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y,
                                 currentHealthWidth, healthBarHeight),
                    Color.White);
            }

            // ����� � �������� ��������� ��������
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

            // ������ ������� ����� ��������� ������
            _obstacles.Clear();
            _zombies.Clear();

            // ��������� �������
            _backgroundTexture = content.Load<Texture2D>(level.MapTexture);
            _player.Position = level.PlayerStartPosition;
            _player.ResetLives(); // ����� ������ ������

            // ����������� ������
            _portal = new Portal(level.PortalPosition, _graphicsDevice);
            _portal.LoadContent(content);
            _portal.Deactivate(); // ������������ �� ������ ������

            // ��������� ����������� �� ������
            _obstacles.AddRange(level.Obstacles);

            // ������� �����
            foreach (var spawnPoint in level.ZombieSpawnPoints)
            {
                _zombies.Add(new Zombie(spawnPoint, _zombieTexture));
            }

            // ����� ���������
            _zombiesKilled = 0;
            _zombiesToKill = level.ZombiesToKill;

            // ����� ��������� ����
            _gameState = GameState.Playing;

            // ���� ��� �������� - �������� ��� ���������
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
