using System;
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

        private readonly VertexPositionColorTexturePosition[] quadVertices =
        {
            new VertexPositionColorTexturePosition(new Vector2(0, 0), RgbaFloat.Red, new Vector2(0, 0)),
            new VertexPositionColorTexturePosition(new Vector2(0, 0), RgbaFloat.Green, new Vector2(0, 0)),
            new VertexPositionColorTexturePosition(new Vector2(0, 0), RgbaFloat.Blue, new Vector2(0, 0)),
            new VertexPositionColorTexturePosition(new Vector2(0, 0), RgbaFloat.Yellow, new Vector2(0, 0)),
        };

        private readonly ushort[] quadIndices = { 0, 1, 2, 3 };

        private readonly GraphicsDevice _graphicsDevice;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private Texture _skeletonTexture;
        private TextureView _skeletonTextureView;
        private Shader[] _shaders;
        private Pipeline _pipeline;
        private ResourceSet _resourceSet;
        private TileSetAddresser _tileSetAddresser;

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
            _vertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(4 * VertexPositionColorTexturePosition.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));

            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, quadVertices);
            _graphicsDevice.UpdateBuffer(_indexBuffer, 0, quadIndices);
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
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                new VertexElementDescription("TexturePosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

            var textureLayout = _graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SkeletonTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));

            _resourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(textureLayout, _skeletonTextureView));

            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
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

            // TODO: use a camera instead of rendering stuff directly
            var (tileWidth, tileHeight) = _tileSetAddresser.GetSkeletonTileDimensions();

            var topOffset = tileHeight * player.Direction switch
            {
                Direction.North => 0,
                Direction.West => 1,
                Direction.South => 2,
                Direction.East => 3,
                _ => 0, // Should never be reached
            };

            var leftOffset = player.WalkingStep * tileWidth;

            var skeletonWidth = tileWidth;
            var skeletonHeight = tileHeight;
            quadVertices[0].Position.X = player.X;
            quadVertices[0].Position.Y = player.Y;
            quadVertices[0].TexturePosition.X = leftOffset;
            quadVertices[0].TexturePosition.Y = topOffset;

            quadVertices[1].Position.X = player.X + 0.25f;
            quadVertices[1].Position.Y = player.Y;
            quadVertices[1].TexturePosition.X = leftOffset + skeletonWidth;
            quadVertices[1].TexturePosition.Y = topOffset;

            quadVertices[2].Position.X = player.X;
            quadVertices[2].Position.Y = player.Y - 0.25f;
            quadVertices[2].TexturePosition.X = leftOffset;
            quadVertices[2].TexturePosition.Y = topOffset + skeletonHeight;

            quadVertices[3].Position.X = player.X + 0.25f;
            quadVertices[3].Position.Y = player.Y - 0.25f;
            quadVertices[3].TexturePosition.X = leftOffset + skeletonWidth;
            quadVertices[3].TexturePosition.Y = topOffset + skeletonHeight;

            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, quadVertices);
        }

        public void Draw(CommandList commandList, World world)
        {
            UpdateVertices(world);

            commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            commandList.ClearColorTarget(0, RgbaFloat.Black);

            commandList.SetVertexBuffer(0, _vertexBuffer);
            commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            commandList.SetPipeline(_pipeline);
            commandList.SetGraphicsResourceSet(0, _resourceSet);
            commandList.DrawIndexed(
                indexCount: 4,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
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
            _indexBuffer.Dispose();
        }
    }
}