using System.Numerics;

namespace Client
{
    struct VertexPositionTexturePosition
    {
        public Vector2 Position;
        public Vector2 TexturePosition;
        public VertexPositionTexturePosition(Vector2 position, Vector2 texturePosition)
        {
            Position = position;
            TexturePosition = texturePosition;
        }
        public const uint SizeInBytes = 16;
    }
}