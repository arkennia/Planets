using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public partial class ArrayMeshGen : MeshInstance3D
{
    const int SIDES = 6;
    struct Side
    {
        public int id;
        public Vector3 uvOrigin, uVector, vVector;
        public Vector3 normal;
        public Vector4 tangent;
    }
    [Export]
    public int Resolution { get; set; }
    public int VertexCount => SIDES * 4 * Resolution * Resolution;
    public int IndexCount => SIDES * 6 * Resolution * Resolution;
    public int JobLength => SIDES * Resolution;
    public override void _Ready()
    {
        // New Array for Surfaces
        Godot.Collections.Array surfaceArray = [];
        //Holds all the arrays of data that the surface needs.
        // Godot will expect it to be of size Mesh.ARRAY_MAX
        surfaceArray.Resize((int)Mesh.ArrayType.Max);

        // Create lists(arrays) for each type you need for the mesh.
        // Non exhaustive list.
        List<Vector3> verts = [.. new Vector3[VertexCount]];
        List<Vector2> uvs = [];
        List<Vector3> normals = [];
        List<float> tangents = [];
        List<int> indices = [.. new int[IndexCount]];

        /*
            Code to generate geometry goes here.

        */
        for (int i = 0; i < JobLength; i++)
        {

            int u = i / SIDES;
            Side side = GetSide(i % SIDES);

            int vi = 4 * Resolution * (Resolution * side.id + u);
            int ti = 2 * Resolution * (Resolution * side.id + u);
            Vector3 uA = side.uvOrigin + side.uVector * u / Resolution;
            Vector3 uB = side.uvOrigin + side.uVector * (u + 1) / Resolution;
            Vector3 pA = uA;
            Vector3 pB = uB;

            for (int v = 1; v <= Resolution; v++, vi += 4, ti += 2)
            {
                Vector3 pC = uA + side.vVector * (v) / Resolution;
                Vector3 pD = uB + side.vVector * (v) / Resolution;

                // Vector3 vertex = new();
                // Vector3 normal = side.normal;
                Vector4 tangent = new();
                var norm = (pB - pA).Normalized();
                tangent.X = norm.X;
                tangent.Y = norm.Y;
                tangent.Z = norm.Z;
                tangent.W = -1f;
                // verts.Add(pA);
                verts[vi] = pA;
                uvs.Add(Vector2.Zero);
                normals.Add((pC - pA).Cross(new Vector3(tangent.X, tangent.Y, tangent.Z)).Normalized());
                tangents.AddRange([tangent.X, tangent.Y, tangent.Z, tangent.W]);

                //verts.Add(pB);
                verts[vi + 1] = pB;
                uvs.Add(new Vector2(1f, 0f));
                normals.Add((pD - pB).Cross(new Vector3(tangent.X, tangent.Y, tangent.Z)).Normalized());
                tangents.AddRange([tangent.X, tangent.Y, tangent.Z, tangent.W]);

                //verts.Add(pC);
                norm = (pD - pC).Normalized();
                tangent.X = norm.X;
                tangent.Y = norm.Y;
                tangent.Z = norm.Z;
                verts[vi + 2] = pC;
                uvs.Add(new Vector2(0f, 1f));
                normals.Add((pC - pA).Cross(new Vector3(tangent.X, tangent.Y, tangent.Z)).Normalized());
                tangents.AddRange([tangent.X, tangent.Y, tangent.Z, tangent.W]);

                verts[vi + 3] = pD;
                uvs.Add(Vector2.One);
                normals.Add((pD - pB).Cross(new Vector3(tangent.X, tangent.Y, tangent.Z)).Normalized());
                tangents.AddRange([tangent.X, tangent.Y, tangent.Z, tangent.W]);

                indices.InsertRange(ti, [vi, vi + 2, vi + 3, vi, vi + 1, vi + 2]);
                //indices.InsertRange(ti, [vi + 1, vi + 2, vi, vi + 3, vi + 2, vi + 1]);
                pA = pC;
                pB = pD;
            }

        }

        // Once you have filled your data arrays with your geometry you can create
        // a mesh by adding each array to surface_array and then committing to the mesh.
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
        surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Tangent] = tangents.ToArray();

        var arrMesh = Mesh as ArrayMesh;
        // No blendshapes, lods, or compression used.
        arrMesh?.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

        ResourceSaver.Save(arrMesh, "res://sphere.tres", ResourceSaver.SaverFlags.Compress);
    }

    static Side GetSide(int id) => id switch
    {
        0 => new Side
        {
            id = id,
            uvOrigin = new Vector3(-1f, -1f, -1f),
            uVector = Vector3.Right * 2f,
            vVector = Vector3.Up * 2f,
            normal = Vector3.Back,
            tangent = new Vector4(1f, 0f, 0f, -1f)
        },
        1 => new Side
        {
            id = id,
            uvOrigin = new Vector3(1f, -1f, -1f),
            uVector = Vector3.Forward * 2f,
            vVector = Vector3.Up * 2f,
            normal = Vector3.Right,
            tangent = new Vector4(0f, 0f, 1f, -1f)
        },
        2 => new Side
        {
            id = id,
            uvOrigin = new Vector3(-1f, -1f, -1f),
            uVector = Vector3.Forward * 2f,
            vVector = Vector3.Right * 2f,
            normal = Vector3.Down,
            tangent = new Vector4(0f, 0f, 1f, -1f)
        },
        3 => new Side
        {
            id = id,
            uvOrigin = new Vector3(-1f, -1f, 1f),
            uVector = Vector3.Up * 2f,
            vVector = Vector3.Right * 2f,
            normal = Vector3.Forward,
            tangent = new Vector4(0f, 1f, 0f, -1f)
        },
        4 => new Side
        {
            id = id,
            uvOrigin = new Vector3(-1f, -1f, -1f),
            uVector = Vector3.Up * 2f,
            vVector = Vector3.Forward * 2f,
            normal = Vector3.Left,
            tangent = new Vector4(0f, 1f, 0f, -1f)
        },
        _ => new Side
        {
            id = id,
            uvOrigin = new Vector3(-1f, 1f, -1f),
            uVector = Vector3.Right * 2f,
            vVector = Vector3.Forward * 2f,
            normal = Vector3.Up,
            tangent = new Vector4(1f, 0f, 0f, -1f)
        }
    };
}