using Godot;
using System;

namespace Planets.SystemGenerator
{
    [GlobalClass]
    public partial class Planet(string name = "Earth", Mesh mesh = null, int scale = 20000, int resolution = 128) : Resource
    {

        public string Name { get; private set; } = name;

        public Mesh Mesh { get; private set; } = mesh;

        public int Scale { get; private set; } = scale;
        public int Resolution { get; private set; } = resolution;

        public PlanetNode Generate()
        {
            var cs = new CubeSphere { MeshName = Name, Scale = 20000, Resolution = 128 };
            var arrayMesh = cs.Generate(false);
            PlanetNode rootNode = new();
            Mesh = arrayMesh;
            MeshInstance3D mI = new()
            {
                Mesh = Mesh,
                Name = Name
            };
            var sB = new StaticBody3D();
            var collider = new CollisionShape3D();
            var colliderShape = new SphereShape3D();

            colliderShape.Radius = cs.Scale + 25;
            collider.Shape = colliderShape;
            rootNode.Planet = this;
            rootNode.AddChild(mI);
            mI.Owner = rootNode;
            mI.AddChild(sB);
            sB.Owner = rootNode;
            sB.AddChild(collider);
            collider.Owner = rootNode;
            return rootNode;
        }

        public void Save(string path = "res://resources")
        {
            ResourceSaver.Save(this, $"{path}/{Name}.res", ResourceSaver.SaverFlags.Compress);
        }
    }
}
