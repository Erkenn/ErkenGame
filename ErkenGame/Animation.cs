using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ErkenGame
{
    public class Animation
    {
        public Texture2D Texture { get; }
        public int FrameCount { get; }
        public float FrameTime { get; }
        public bool IsLooping { get; }

        private int _currentFrame;
        private float _timer;

        public Animation(Texture2D texture, int frameCount, float frameTime, bool isLooping = true)
        {
            Texture = texture;
            FrameCount = frameCount;
            FrameTime = frameTime;
            IsLooping = isLooping;
        }

        public void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timer >= FrameTime)
            {
                _timer = 0f;
                _currentFrame++;

                if (_currentFrame >= FrameCount && IsLooping)
                    _currentFrame = 0;
            }
        }

        public Rectangle CurrentFrameRect
        {
            get
            {
                int frameWidth = Texture.Width / FrameCount;
                return new Rectangle(_currentFrame * frameWidth, 0, frameWidth, Texture.Height);
            }
        }
    }
}
