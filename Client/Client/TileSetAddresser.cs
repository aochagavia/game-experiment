using System;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Client
{
    public class TileSetAddresser
    {
        private readonly Assembly _assembly = typeof(TileSetAddresser).Assembly;

        public readonly Image<Rgba32> TileSet;

        private readonly int _skeletonImageWidth;
        private readonly int _skeletonImageHeight;
        private readonly int _sandImageWidth;
        private readonly int _tileSetHeight;

        public TileSetAddresser()
        {
            using var skeletonTiles = LoadImageFromResources("Client.Tiles.BODY_skeleton.png");
            using var sandTiles = LoadImageFromResources("Client.Tiles.sand.png");

            _skeletonImageWidth = skeletonTiles.Width;
            _skeletonImageHeight = skeletonTiles.Height;
            _sandImageWidth = sandTiles.Width;
            _tileSetHeight = Math.Max(skeletonTiles.Height, sandTiles.Height);

            TileSet = new Image<Rgba32>(skeletonTiles.Width + sandTiles.Width, _tileSetHeight);
            TileSet.Mutate(o => o
                .DrawImage(skeletonTiles, PixelBlenderMode.Add, 1f, new SixLabors.Primitives.Point(0, 0))
                .DrawImage(sandTiles, PixelBlenderMode.Add, 1f, new SixLabors.Primitives.Point(skeletonTiles.Width, 0))
            );
        }

        private Image<Rgba32> LoadImageFromResources(string path)
        {
            var stream = _assembly.GetManifestResourceStream("Client.Tiles.BODY_skeleton.png");
            if (stream == null)
            {
                var x = _assembly.GetManifestResourceNames();
                throw new Exception("Stream is null");
            }

            return Image.Load(stream.ReadAsUtf8Bytes());
        }

        public (float, float) GetSkeletonTileDimensions()
        {
            float totalWidth = _skeletonImageWidth + _sandImageWidth;
            var skeletonRelativeWidth = _skeletonImageWidth / totalWidth;
            var tileWidth = (1f / 9) * skeletonRelativeWidth;

            var skeletonRelativeHeight = _skeletonImageHeight / _tileSetHeight;
            var tileHeight = (1f / 4) * skeletonRelativeHeight;

            return (tileWidth, tileHeight);
        }
    }
}