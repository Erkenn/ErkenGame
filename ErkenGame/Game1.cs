using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ErkenGame.Models;
using ErkenGame.Views;
using ErkenGame.Controllers;

namespace ErkenGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Rectangle _restartButtonRectangle;

        //  Controllers
        private GameController _gameController;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 2880;
            _graphics.PreferredBackBufferHeight = 1620;
            _graphics.ApplyChanges();

            _gameController = new GameController(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight); // Инициализируем контроллер
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameController.LoadContent(Content, GraphicsDevice); // Передаем Content и GraphicsDevice контроллеру
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _gameController.Update(gameTime); // Обновляем состояние игры через контроллер

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(); // Начинаем SpriteBatch только здесь

            _gameController.Draw(_spriteBatch); // Передаем SpriteBatch для отрисовки

            _spriteBatch.End(); // Заканчиваем SpriteBatch только здесь

            base.Draw(gameTime);
        }
    }
}
