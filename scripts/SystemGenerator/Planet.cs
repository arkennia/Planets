using Godot;
using System;

namespace Planets.SystemGenerator
{
    [GlobalClass]
    public partial class Planet : Resource
    {
        [Export]
        public string Name { get; set; }
        [Export]
        public Mesh Mesh { get; set; }

        public Planet() { }

        public Planet(string name = "Earth", Mesh mesh = null)
        {
            Name = name;
            Mesh = mesh;
        }

        public PlanetNode Generate()
        {
            var cs = new CubeSphere();
            var arrayMesh = cs.Generate(true);
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

            colliderShape.Radius = cs.Scale + 10;
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
