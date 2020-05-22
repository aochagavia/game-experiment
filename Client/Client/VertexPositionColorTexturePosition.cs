using System.Numerics;
using Veldrid;

namespace Client
{
    struct VertexPositionColorTexturePosition
    {
        public Vector2 Position; // This is the position, in normalized device coordinates.
        public RgbaFloat Color; // This is the color of the vertex.
        public Vector2 TexturePosition;
        public VertexPositionColorTexturePosition(Vector2 position, RgbaFloat color, Vector2 texturePosition)
        {
            Position = position;
            Color = color;
            TexturePosition = texturePosition;
        }
        public const uint SizeInBytes = 32; // TODO: shouldn't this be the same as sizeof(T)? It is a struct...
    }
}