using Godot;
using System;
using System.Diagnostics;

namespace Planets.SystemGenerator
{
    public partial class NoiseGenerator : RefCounted
    {
        public static int Seed { get; set; } = 42;

        private static FastNoiseLite _noise => new FastNoiseLite()
        {
            Seed = Seed,
            NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
        };

        public static float GenerateNoise(float x, float y, float z)
        {
            return _noise.GetNoise3D(x, y, z);
        }

        public static float GenerateNoise(Vector3 position)
        {
            return _noise.GetNoise3Dv(position);
        }

        public static Cubemap GenerateCubemapTexture(int size = 256)
        {
            var images = _noise.GetImage3D(256, 256, 6, normalize: false);
            Cubemap cubemap = new Cubemap();
            cubemap.CreateFromImages(images);
            return cubemap;
        }

    }
}