using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ErkenGame.Models;
using ErkenGame.Views;
using System.Collections.Generic;

namespace ErkenGame.Controllers
{
    public class GameController
    {
        public ContentManager content;
        private Rectangle _restartButtonRectangle;
        private int _screenWidth;
        private int _screenHeight;
        private GameState _gameState;
        private Player _player;
        private List<Obstacle> _obstacles;
        private GameView _gameView;
        private Texture2D _buttonTexture;
        private SpriteFont _font;
        private Rectangle _buttonRectangle;
        private Texture2D _backgroundTexture;
        private Camera _camera;
        private List<Zombie> _zombies;
        private Texture2D _zombieTexture;


        private enum GameState { Menu, Playing, GameOver }

        public GameController(int screenWidth, int screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _gameState = GameState.Menu;
            _obstacles = new List<Obstacle>();

            // �������� ����������� (������ ����� ������)
            _obstacles.Add(new Obstacle(1500, 1300, 500, 300));
            _obstacles.Add(new Obstacle(2000, 1100, 250, 300));
            _zombies = new List<Zombie>();
        }

        private GraphicsDevice _graphicsDevice;

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            this.content = content;
            _graphicsDevice = graphicsDevice;
            _buttonTexture = content.Load<Texture2D>("button");
            _backgroundTexture = content.Load<Texture2D>("background");
            _font = content.Load<SpriteFont>("Font");
            Texture2D playerTexture = content.Load<Texture2D>("Character"); // �������� �������� ������
            _zombieTexture = content.Load<Texture2D>("Zombie"); // �������� �������� �����

            _player = new Player(new Vector2(100, 100), playerTexture); // �������� ���������� ������
            _zombies.Add(new Zombie(new Vector2(500, 100), _zombieTexture)); // �������� ���������� �����
            _player.LoadAnimations(content);
            _gameView = new GameView(content);

            // ����������� ������� � ������� ������
            int buttonWidth = 400;
            int buttonHeight = 100;
            int buttonX = (_screenWidth - buttonWidth) / 2;
            int buttonY = (_screenHeight - buttonHeight) / 2;

            _restartButtonRectangle = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
            _buttonRectangle = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);

            _gameView.LoadContent(content); // ��������� �������� � GameView

            _camera = new Camera(_graphicsDevice.Viewport); // �������������� ������
        }


        public void Update(GameTime gameTime)
        {
            if (_gameState == GameState.Menu)
            {
                MouseState mouseState = Mouse.GetState();
                if (mouseState.LeftButton == ButtonState.Pressed && _buttonRectangle.Contains(mouseState.X, mouseState.Y))
                {
                    _gameState = GameState.Playing;
                }
            }
            else if (_gameState == GameState.Playing)
            {
                _camera.Update(_player.Position);
                _player.Update(gameTime, _obstacles, _zombies);

                // �������� ������ ������
                if (_player.GetHealth() <= 0)
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
                    }
                }
            }
            else if (_gameState == GameState.GameOver)
            {
                // TODO: �������� ������ ��� ������ "����������� ��� ���"
                MouseState mouseState = Mouse.GetState();
                if (mouseState.LeftButton == ButtonState.Pressed && _restartButtonRectangle.Contains(mouseState.X, mouseState.Y))
                {
                    // ���������� ����
                    ResetGame();
                    _gameState = GameState.Playing;
                }
            }

        }
        private void ResetGame()
        {
            // ���������� �������� ������
            _player = new Player(new Vector2(100, 100), content.Load<Texture2D>("Character"));

            // ������� ������ ����� � ������� ������ �����
            _zombies.Clear();
            _zombies.Add(new Zombie(new Vector2(500, 100), content.Load<Texture2D>("Zombie")));

            // TODO: �������� ������ ��������� ���� (��������, ������� ������, �����������)

            // ���������� ������ � ��������� ��������� (��������)
            _camera = new Camera(_graphicsDevice.Viewport);
        }





        public void Draw(SpriteBatch spriteBatch)
        {
            // � GameController
            _gameView.Draw(spriteBatch, _player, _obstacles);
            // ������ ���
            spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, _screenWidth, _screenHeight), Color.White);

            if (_gameState == GameState.Menu)
            {
                // ������ ������
                spriteBatch.Draw(_buttonTexture, _buttonRectangle, Color.White);

                // ������ ����� �� ������
                string buttonText = "������";
                Vector2 textSize = _font.MeasureString(buttonText);
                Vector2 textPosition = new Vector2(
                    _buttonRectangle.X + (_buttonRectangle.Width - textSize.X) / 2,
                    _buttonRectangle.Y + (_buttonRectangle.Height - textSize.Y) / 2);

                spriteBatch.DrawString(_font, buttonText, textPosition, Color.Black);
            }
            else if (_gameState == GameState.Playing)
            {
                spriteBatch.End(); //��������� ����������
                spriteBatch.Begin(transformMatrix: _camera.Transform);

                _gameView.Draw(spriteBatch, _player, _obstacles);

                foreach (Zombie zombie in _zombies)
                {
                    zombie.Draw(spriteBatch);
                }

                spriteBatch.End();// ��������� ���������� � �������
                spriteBatch.Begin(); //��������� ���������� ��� ������
            }

            else if (_gameState == GameState.GameOver)
            {
                // ������ ����� "���� ��������"
                string gameOverText = "���� ��������";
                Vector2 textSize = _font.MeasureString(gameOverText);
                Vector2 textPosition = new Vector2(
                    (_screenWidth - textSize.X) / 2,
                    (_screenHeight - textSize.Y) / 2 - 50); // �������� ����� ������� �����

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
        }
    }

}
