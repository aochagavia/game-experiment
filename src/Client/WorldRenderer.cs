using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Client.Simulation;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;

namespace Client
{
    public class WorldRenderer : IDisposable
    {
        private readonly Assembly _assembly = typeof(WorldRenderer).Assembly;

        private readonly GraphicsDevice _graphicsDevice;
        private DeviceBuffer _vertexBuffer;
        private Texture _skeletonTexture;
        private TextureView _skeletonTextureView;
        private Shader[] _shaders;
        private Pipeline _pipeline;
        private ResourceSet _resourceSet;
        private TileSetAddresser _tileSetAddresser;
        private ArrayList<VertexPositionTexturePosition> _vertices = new ArrayList<VertexPositionTexturePosition>();

        public WorldRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            InitBuffers();
            InitTextures();
            InitShaders();
            InitPipeline();
        }

        private void InitBuffers()
        {
            _vertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(_vertices.Capacity * VertexPositionTexturePosition.SizeInBytes, BufferUsage.VertexBuffer));
        }

        private void InitTextures()
        {
            _tileSetAddresser = new TileSetAddresser();
            var imageSharpTexture = new ImageSharpTexture(_tileSetAddresser.TileSet);
            _skeletonTexture = imageSharpTexture.CreateDeviceTexture(_graphicsDevice, _graphicsDevice.ResourceFactory);
            _skeletonTextureView = _graphicsDevice.ResourceFactory.CreateTextureView(_skeletonTexture);
        }

        private void InitShaders()
        {
            var vertexShaderCode = _assembly.GetManifestResourceStream("Client.Shader.vert").ReadAsUtf8Bytes();
            var fragmentShaderCode = _assembly.GetManifestResourceStream("Client.Shader.frag").ReadAsUtf8Bytes();

            var vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                vertexShaderCode,
                "main");
            var fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                fragmentShaderCode,
                "main");

            _shaders = _graphicsDevice.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
        }

        private void InitPipeline()
        {
            var pipelineDescription = new GraphicsPipelineDescription();

            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("TexturePosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

            var textureLayout = _graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SkeletonTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));

            _resourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(textureLayout, _skeletonTextureView));

            pipelineDescription.BlendState = BlendStateDescription.SingleAlphaBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.None,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = new [] { textureLayout };
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new [] { vertexLayout },
                shaders: _shaders);
            pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
            _pipeline = _graphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);
        }

        private void UpdateVertices(World world)
        {
            // TODO: allow multiple players, somehow
            var player = world.Players.FirstOrDefault();


            _vertices.Clear();

            DrawSand();
            DrawSkeleton(player);

            var span = _vertices.Elements.AsSpan();

            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, ref span[0], _vertices.Count * VertexPositionTexturePosition.SizeInBytes);
        }

        private void DrawSkeleton(Player player)
        {
            // TODO: use a camera instead of rendering stuff directly
            var (tileWidth, tileHeight) = _tileSetAddresser.GetSkeletonTileDimensions();

            var topOffset = player.Direction switch
            {
                Direction.North => 0,
                Direction.West => 1,
                Direction.South => 2,
                Direction.East => 3,
                _ => 0, // Should never be reached
            };

            DrawTile(new Vector2(player.X, player.Y), player.WalkingStep, topOffset, 0, tileWidth, tileHeight);
        }

        private void DrawSand()
        {
            var (tileWidth, tileHeight) = _tileSetAddresser.GetSandTileDimensions();
            var topLeftX = _tileSetAddresser.GetSandTopLeftCornerX();

            var worldWidthTiles = 8;

            for (var y = 0; y < worldWidthTiles; y++) {
                var currentY = ((float)y) * 0.25f - 0.75f;
                DrawTile(new Vector2(0, currentY), 2, 11, topLeftX, tileWidth, tileHeight);
                DrawTile(new Vector2(0.25f, currentY), 2, 11, topLeftX, tileWidth, tileHeight);
                DrawTile(new Vector2(0.50f, currentY), 2, 11, topLeftX, tileWidth, tileHeight);

                DrawTile(new Vector2(0, currentY), 2, 3, topLeftX, tileWidth, tileHeight);
                DrawTile(new Vector2(0.50f, currentY), 0, 3, topLeftX, tileWidth, tileHeight);
                DrawTile(new Vector2(0.75f, currentY), 5, 3, topLeftX, tileWidth, tileHeight);

                for (var x = 0; x < 4; x++) {
                    DrawTile(new Vector2((x + 1) * -0.25f, currentY), 1, 3, topLeftX, tileWidth, tileHeight);
                }
            }
        }

        private void DrawTile(Vector2 screenPosition, int tileIndexX, int tileIndexY, float tileSetXOffset, float tileWidth, float tileHeight)
        {
            Vector2 tilePosition = new Vector2(tileSetXOffset + tileIndexX * tileWidth, tileIndexY * tileHeight);

            // Triangle 1
            _vertices.Add(new VertexPositionTexturePosition(new Vector2(screenPosition.X, screenPosition.Y), new Vector2(tilePosition.X, tilePosition.Y)));
            _vertices.Add(new VertexPositionTexturePosition(new Vector2(screenPosition.X + 0.25f, screenPosition.Y), new Vector2(tilePosition.X + tileWidth, tilePosition.Y)));
            _vertices.Add(new VertexPositionTexturePosition(new Vector2(screenPosition.X, screenPosition.Y - 0.25f), new Vector2(tilePosition.X, tilePosition.Y + tileHeight)));

            // Triangle 2
            _vertices.Add(new VertexPositionTexturePosition(new Vector2(screenPosition.X + 0.25f, screenPosition.Y), new Vector2(tilePosition.X + tileWidth, tilePosition.Y)));
            _vertices.Add(new VertexPositionTexturePosition(new Vector2(screenPosition.X, screenPosition.Y - 0.25f), new Vector2(tilePosition.X, tilePosition.Y + tileHeight)));
            _vertices.Add(new VertexPositionTexturePosition(new Vector2(screenPosition.X + 0.25f, screenPosition.Y - 0.25f), new Vector2(tilePosition.X + tileWidth, tilePosition.Y + tileHeight)));
        }

        public void Draw(CommandList commandList, World world)
        {
            UpdateVertices(world);

            commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            commandList.ClearColorTarget(0, RgbaFloat.Black);

            commandList.SetVertexBuffer(0, _vertexBuffer);
            commandList.SetPipeline(_pipeline);
            commandList.SetGraphicsResourceSet(0, _resourceSet);
            commandList.Draw(_vertices.Count);
        }

        public void Dispose()
        {
            _pipeline.Dispose();

            foreach (var shader in _shaders)
            {
                shader.Dispose();
            }

            _resourceSet.Dispose();
            _skeletonTexture.Dispose();
            _skeletonTextureView.Dispose();

            _vertexBuffer.Dispose();
        }
    }
}
