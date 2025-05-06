using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ErkenGame.Models
{
    public class Portal
    {
        private Animation _animation;
        private Vector2 _position;
        private bool _isActive;
        private Texture2D _errorTexture;

        public Rectangle Bounds => new Rectangle(
            (int)_position.X,
            (int)_position.Y,
            _animation?.CurrentFrameRect.Width ?? 64,
            _animation?.CurrentFrameRect.Height ?? 64);

        public bool IsActive => _isActive;

        public Portal(Vector2 position, GraphicsDevice graphicsDevice)
        {
            _position = position;
            _isActive = false;
        }

        public void LoadContent(ContentManager content)
        {
            Texture2D texture = content.Load<Texture2D>("PortalAnimation");
            _animation = new Animation(texture, 6, 0.1f, true);
        }

        public void Activate()
        {
            _isActive = true;
        }

        public void Deactivate()
        {
            _isActive = false;
            // Дополнительные действия по деактивации, если нужно
        }

        public void Update(GameTime gameTime)
        {
            if (_isActive)
            {
                _animation?.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_isActive && _animation != null)
            {
                spriteBatch.Draw(
                    _animation.Texture,
                    _position,
                    _animation.CurrentFrameRect,
                    Color.White);
            }
        }
    }
}