using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Camera
{
    private readonly Viewport _viewport;
    private Vector2 _position;
    private float _zoom = 1.0f;

    // ��������� ������
    private const float FollowSpeed = 5f;
    private const float VerticalOffset = -200f; // �������� ����� (������������� ��������)

    public Matrix Transform { get; private set; }

    public Camera(Viewport viewport)
    {
        _viewport = viewport;
    }

    public void Update(Vector2 targetPosition, float deltaTime)
    {
        // ������������ �������� ������� ������ � ������ ��������
        Vector2 desiredPosition = new Vector2(
            targetPosition.X - _viewport.Width / 2,
            targetPosition.Y + VerticalOffset - _viewport.Height / 2);

        // ������� ���������� �� �������
        _position = Vector2.Lerp(_position, desiredPosition, FollowSpeed * deltaTime);

        // ������������ ������ ��������� ���� (���� �����)
        _position.X = MathHelper.Clamp(_position.X, 0, 10000 - _viewport.Width);
        _position.Y = MathHelper.Clamp(_position.Y, 0, 2000 - _viewport.Height);

        // ������� ������� �������������
        Transform = Matrix.CreateTranslation(-_position.X, -_position.Y, 0) *
                  Matrix.CreateScale(new Vector3(_zoom, _zoom, 1));
    }
}