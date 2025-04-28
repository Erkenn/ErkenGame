using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ErkenGame
{
    public class Camera
    {
        private Viewport _viewport;
        public Matrix Transform { get; private set; }
        private Vector2 _desiredPosition; // ∆елаема€ позици€ камеры
        private float _maxDistance = 500f; // ћаксимальное рассто€ние от игрока до центра экрана

        public Camera(Viewport viewport)
        {
            _viewport = viewport;
        }

        public void Update(Vector2 position)
        {
            // ¬ычисл€ем желаемую позицию камеры (центрируем на игроке)
            _desiredPosition.X = position.X - _viewport.Width / 2;
            _desiredPosition.Y = position.Y - _viewport.Height / 2;

            // ѕолучаем текущую позицию камеры из матрицы трансформации
            Vector2 currentPosition = new Vector2(Transform.Translation.X, Transform.Translation.Y);

            // ¬ычисл€ем вектор направлени€ от камеры к желаемой позиции
            Vector2 direction = _desiredPosition - currentPosition;

            // ¬ычисл€ем рассто€ние между камерой и желаемой позицией
            float distance = direction.Length();

            // ≈сли рассто€ние превышает максимальное, ограничиваем его
            if (distance > _maxDistance)
            {
                // Ќормализуем вектор направлени€ (делаем его единичной длины)
                direction.Normalize();

                // ”станавливаем новую позицию камеры на максимальном рассто€нии от желаемой
                currentPosition = _desiredPosition - direction * _maxDistance;
            }

            // —оздаем матрицу трансформации
            Transform = Matrix.CreateTranslation(
                -currentPosition.X,
                -currentPosition.Y,
                0);
        }
    }
}
