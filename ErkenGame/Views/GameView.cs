using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ErkenGame.Models;

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
            player.Draw(spriteBatch); // Рисуем игрока, вызывая Draw из Player
                                      // Рисуем препятствия
            foreach (Obstacle obstacle in obstacles)
            {
                spriteBatch.Draw(_obstacleTexture, obstacle.Rectangle, Color.White);
            }
        }


    }
}
