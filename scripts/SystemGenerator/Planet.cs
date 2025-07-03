using Godot;
using System;
using System.Linq;

namespace Planets.SystemGenerator
{
    [GlobalClass]
    public partial class Planet : Resource, ICelestialBody
    {
        [Export]
        public string Name { get; set; } = "Earth";
        [Export]
        public string Area3DName { get; private set; }
        [Export]
        public int Area3DExtraSpace { get; set; } = 3000;
        [Export]
        public Mesh Mesh { get; set; }
        [Export]
        public int Scale { get; set; }
        [Export]
        public int Resolution { get; set; }
        [Export]
        public bool Generated { get; private set; }
        [Export]
        public float Gravity { get; set; } = 9.8f;
        [Export]
        public Vector2 Sector { get; private set; } = Vector2.Zero;
        [Export]
        public Vector3 SectorLocation { get; private set; } = Vector3.Zero;

        public Guid Guid { get; private set; } = Guid.Empty;




        public Planet()
        {
            if (Guid == Guid.Empty)
            {
                Guid = Guid.NewGuid();
            }
        }

        public Planet(string name = "Earth", Mesh mesh = null, int scale = 20000, int resolution = 128)
        {
            Name = name;
            Mesh = mesh;
            Scale = scale;
            Resolution = resolution;
            Guid = new Guid();
        }

        public PlanetNode Generate()
        {
            if (Mesh == null)
            {
                CubeSphere cs = new() { MeshName = Name, Scale = Scale, Resolution = Resolution };
                ArrayMesh arrayMesh = cs.Generate();
                Mesh = arrayMesh;
                Cubemap cb = NoiseGenerator.GenerateCubemapTexture();
                ResourceSaver.Save(cb, $"res://textures/{Guid}.tres");
            }
            PlanetNode rootNode = new();
            MeshInstance3D mI = new()
            {
                Mesh = Mesh,
                Name = Name
            };
            RigidBody3D rB = new();

            Area3D area = new()
            {
                GravitySpaceOverride = Area3D.SpaceOverride.Replace,
                GravityPoint = true,
                GravityPointUnitDistance = Scale,
                Gravity = Gravity,
                GravityDirection = new Vector3(0, -1, 0)
            };

            SphereShape3D areaColliderShape = new()
            {
                Radius = Scale + 3000
            };
            CollisionShape3D areaCollider = new()
            {
                Shape = areaColliderShape
            };


            SphereShape3D colliderShape = new()
            {
                Radius = Scale + 8
            };
            CollisionShape3D collider = new()
            {
                Shape = colliderShape
            };

            rootNode.Planet = this;
            rootNode.AddChild(mI);
            rootNode.AddChild(area);
            area.Owner = rootNode;
            area.AddChild(areaCollider);
            areaCollider.Owner = rootNode;

            mI.Owner = rootNode;
            mI.AddChild(rB);

            rB.Owner = rootNode;
            rB.AddChild(collider);

            collider.Owner = rootNode;
            Generated = true;

            Area3DName = area.Name;
            return rootNode;
        }

        public void Save(string path = "res://resources")
        {
            ResourceSaver.Save(this, $"{path}/{Guid}.res", ResourceSaver.SaverFlags.Compress);
        }
    }
}
