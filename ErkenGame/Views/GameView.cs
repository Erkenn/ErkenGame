using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ErkenGame.Models;
using System;

namespace ErkenGame.Views
{
    public class GameView
    {
        private Texture2D _playerTexture;
        private Texture2D _obstacleTexture;

        public GameView(ContentManager content)
        {
            LoadContent(content);
        }

        public void LoadContent(ContentManager content)
        {
            _playerTexture = content.Load<Texture2D>("Character");
            _obstacleTexture = content.Load<Texture2D>("Obstacle");
        }

        public void Draw(SpriteBatch spriteBatch, Player player, List<Obstacle> obstacles)
        {
            // Отрисовка игрока
            player.Draw(spriteBatch);

            // Отрисовка препятствий с правильным масштабированием
            foreach (Obstacle obstacle in obstacles)
            {
                // Рассчитываем нужное количество повторений текстуры
                int repeatX = (int)Math.Ceiling((float)obstacle.Rectangle.Width / _obstacleTexture.Width);
                int repeatY = (int)Math.Ceiling((float)obstacle.Rectangle.Height / _obstacleTexture.Height);

                for (int x = 0; x < repeatX; x++)
                {
                    for (int y = 0; y < repeatY; y++)
                    {
                        Rectangle destRect = new Rectangle(
                            obstacle.Rectangle.X + x * _obstacleTexture.Width,
                            obstacle.Rectangle.Y + y * _obstacleTexture.Height,
                            Math.Min(_obstacleTexture.Width, obstacle.Rectangle.Width - x * _obstacleTexture.Width),
                            Math.Min(_obstacleTexture.Height, obstacle.Rectangle.Height - y * _obstacleTexture.Height));

                        spriteBatch.Draw(
                            _obstacleTexture,
                            destRect,
                            null,
                            Color.White);
                    }
                }
            }
        }

        public void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            Texture2D dummyTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            dummyTexture.SetData(new Color[] { Color.White }); // Заполняем текстуру белым цветом

            spriteBatch.Draw(dummyTexture, rectangle, color);

            dummyTexture.Dispose();
        }
    }
}
