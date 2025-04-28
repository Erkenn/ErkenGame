using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ErkenGame
{
    public class Camera
    {
        private Viewport _viewport;
        public Matrix Transform { get; private set; }
        private Vector2 _desiredPosition; // �������� ������� ������
        private float _maxDistance = 500f; // ������������ ���������� �� ������ �� ������ ������

        public Camera(Viewport viewport)
        {
            _viewport = viewport;
        }

        public void Update(Vector2 position)
        {
            // ��������� �������� ������� ������ (���������� �� ������)
            _desiredPosition.X = position.X - _viewport.Width / 2;
            _desiredPosition.Y = position.Y - _viewport.Height / 2;

            // �������� ������� ������� ������ �� ������� �������������
            Vector2 currentPosition = new Vector2(Transform.Translation.X, Transform.Translation.Y);

            // ��������� ������ ����������� �� ������ � �������� �������
            Vector2 direction = _desiredPosition - currentPosition;

            // ��������� ���������� ����� ������� � �������� ��������
            float distance = direction.Length();

            // ���� ���������� ��������� ������������, ������������ ���
            if (distance > _maxDistance)
            {
                // ����������� ������ ����������� (������ ��� ��������� �����)
                direction.Normalize();

                // ������������� ����� ������� ������ �� ������������ ���������� �� ��������
                currentPosition = _desiredPosition - direction * _maxDistance;
            }

            // ������� ������� �������������
            Transform = Matrix.CreateTranslation(
                -currentPosition.X,
                -currentPosition.Y,
                0);
        }
    }
}
