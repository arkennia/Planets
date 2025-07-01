using Godot;
using System;

namespace Planets.SystemGenerator
{
    [GlobalClass]
    public partial class Planet : Resource
    {
        [Export]
        public string Name { get; private set; }
        [Export]
        public Mesh Mesh { get; private set; }
        [Export]
        public int Scale { get; private set; }
        [Export]
        public int Resolution { get; private set; }
        [Export]
        public bool Generated { get; private set; }
        [Export]
        public string Area3DName { get; private set; }

        public Planet()
        {

        }

        public Planet(string name = "Earth", Mesh mesh = null, int scale = 20000, int resolution = 128)
        {
            Name = name;
            Mesh = mesh;
            Scale = scale;
            Resolution = resolution;
        }

        public PlanetNode Generate()
        {
            var cs = new CubeSphere { MeshName = Name, Scale = 1000, Resolution = 128 };
            var arrayMesh = cs.Generate(false);
            PlanetNode rootNode = new();
            Mesh = arrayMesh;
            MeshInstance3D mI = new()
            {
                Mesh = Mesh,
                Name = Name
            };
            var sB = new RigidBody3D();
            var collider = new CollisionShape3D();
            var colliderShape = new SphereShape3D();

            var area = new Area3D();
            area.GravitySpaceOverride = Area3D.SpaceOverride.Replace;
            area.GravityPoint = true;
            area.GravityPointUnitDistance = Scale;

            var areaCollider = new CollisionShape3D();
            var areaColliderShape = new SphereShape3D();
            areaColliderShape.Radius = Scale + 3000;
            areaCollider.Shape = areaColliderShape;
            area.Gravity = 10f;
            area.GravityDirection = new Vector3(0, -1, 0);

            colliderShape.Radius = cs.Scale + 25;
            collider.Shape = colliderShape;
            rootNode.Planet = this;
            rootNode.AddChild(mI);
            rootNode.AddChild(area);
            area.Owner = rootNode;
            area.AddChild(areaCollider);
            areaCollider.Owner = rootNode;
            mI.Owner = rootNode;
            mI.AddChild(sB);
            sB.Owner = rootNode;
            sB.AddChild(collider);
            collider.Owner = rootNode;
            Generated = true;
            Area3DName = area.Name;
            return rootNode;
        }

        public void Save(string path = "res://resources")
        {
            ResourceSaver.Save(this, $"{path}/{Name}.res", ResourceSaver.SaverFlags.Compress);
        }
    }
}
