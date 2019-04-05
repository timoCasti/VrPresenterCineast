using System;
using UnityEngine;

namespace Unibas.DBIS.DynamicModelling.Objects
{
    /// <summary>
    ///     Custom cuboid model.
    ///     This model's size is actually the one specified, in unity units.<br />
    ///     In contrast, the vanilla unity cube object has to be resized, which this one doesn't, to get other shapes than a
    ///     cube.<br />
    ///     <b>Note</b> The material is copied, so modifications on the material are not reflected at runtime.
    ///     UV Mapping is based on the smallest dimension, e.g. depending on the texture, further adjustments are required.
    ///     <br />
    ///     This can be achieved by accessing the MeshRenderer via GetComponent.<br />
    ///     If this object's dimensions are changed during runtime, the caller has to call GenerateModel() afterwards,
    ///     to reflect the changes on the model.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(BoxCollider))]
    public class CuboidObject : MonoBehaviour, IObject
    {
        /// <summary>
        ///     The depth in untiy units. Depth is along the z axis.
        /// </summary>
        public float Depth;

        /// <summary>
        ///     The height in unity units. Height is along the y axis.
        /// </summary>
        public float Height;

        /// <summary>
        ///     The material to use for texturing this cuboid.
        ///     Be aware that while drag'n'drop a material to this gameobject works in the scene view,
        ///     it is not the same and this material will always override the one from the inspector.
        ///     Use null if you want the default object material.
        /// </summary>
        public Material Material;

        /// <summary>
        ///     The width in unity units. Width is along the x axis.
        /// </summary>
        public float Width;

        /// <summary>
        ///     Generates the mesh based on the object's configuration
        /// </summary>
        public void GenerateModel()
        {
            var meshFilter = GetComponent<MeshFilter>(); // No null value due to RequireComponent statements
            var meshRenderer = GetComponent<MeshRenderer>();
            var mesh = meshFilter.mesh;

            // The naming is always from the front and downwards looking! e.g. From the back, left and right is swapped
            var frontLeftDown = Vector3.zero;
            var frontRightDown = new Vector3(Width, 0, 0);
            var frontLeftUp = new Vector3(0, Height, 0);
            var frontRightUp = new Vector3(Width, Height, 0);

            var backLeftDown = new Vector3(0, 0, Depth);
            var backRightDown = new Vector3(Width, 0, Depth);
            var backLeftUp = new Vector3(0, Height, Depth);
            var backRightUp = new Vector3(Width, Height, Depth);

            Vector3[] vertices =
            {
                // Front
                frontLeftDown, frontRightDown, frontLeftUp, frontRightUp,
                // Back
                backLeftDown, backRightDown, backLeftUp, backRightUp,
                // Left
                backLeftDown, frontLeftDown, backLeftUp, frontLeftUp,
                // Right
                frontRightDown, backRightDown, frontRightUp, backRightUp,
                // Up
                frontLeftUp, frontRightUp, backLeftUp, backRightUp,
                // Down
                frontLeftDown, frontRightDown, backLeftDown, backRightDown
            };
            mesh.vertices = vertices;

            int[] triangles =
            {
                // Front
                0, 2, 1, 2, 3, 1,
                // Back
                5, 7, 4, 7, 6, 4,
                // Left
                8, 10, 9, 10, 11, 9,
                // Right
                12, 14, 13, 14, 15, 13,
                // Up
                16, 18, 17, 18, 19, 17,
                // Down
                21, 23, 20, 23, 22, 20
            };
            mesh.triangles = triangles;

            Vector3[] normals =
            {
                // Front
                -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward,
                // Back
                -Vector3.back, -Vector3.back, -Vector3.back, -Vector3.back,
                // Left
                -Vector3.left, -Vector3.left, -Vector3.left, -Vector3.left,
                // Right
                -Vector3.right, -Vector3.right, -Vector3.right, -Vector3.right,
                // Up
                -Vector3.up, -Vector3.up, -Vector3.up, -Vector3.up,
                // Down
                -Vector3.down, -Vector3.down, -Vector3.down, -Vector3.down
            };
            mesh.normals = normals;


            /*
             * Unwrapping of mesh for uf like following
             *  U
             * LFRB
             *  D
            */

            var u = Math.Min(Math.Min(Width, Height), Depth);
            var w = Width / u;
            var h = Height / u;
            var d = Depth / u;

            var uvUnits = new Vector2(u, u);
            var fOff = uvUnits.x * Depth;
            var rOff = uvUnits.x * Width + fOff;
            var bOff = uvUnits.x * Depth + rOff;
            var uOff = uvUnits.y * Depth + uvUnits.y * Height;
            Vector2[] uv =
            {
                // Front
                new Vector2(fOff, uvUnits.y * Depth), new Vector2(fOff + uvUnits.x * Width, uvUnits.y * Depth),
                new Vector2(fOff, uvUnits.y * Depth + uvUnits.y * Height),
                new Vector2(fOff + uvUnits.x * Width, uvUnits.y * Depth + uvUnits.y * Height),

                // Back
                new Vector2(bOff, uvUnits.y * Depth), new Vector2(bOff + uvUnits.x * Width, uvUnits.y * Depth),
                new Vector2(bOff, uvUnits.y * Depth + uvUnits.y * Height),
                new Vector2(bOff + uvUnits.x * Width, uvUnits.y * Depth + uvUnits.y * Height),

                // Left
                new Vector2(0, uvUnits.y * Depth), new Vector2(uvUnits.x * Depth, uvUnits.y * Depth),
                new Vector2(0, uvUnits.y * Depth + uvUnits.y * Height),
                new Vector2(uvUnits.x * Depth, uvUnits.y * Depth + uvUnits.y * Height),
                // Right
                new Vector2(rOff, uvUnits.y * Depth), new Vector2(rOff + uvUnits.x * Depth, uvUnits.y * Depth),
                new Vector2(rOff, uvUnits.y * Depth + uvUnits.y * Height),
                new Vector2(rOff + uvUnits.x * Depth, uvUnits.y * Depth + uvUnits.y * Height),
                // Up
                new Vector2(fOff, uOff), new Vector2(fOff + uvUnits.x * Width, uOff),
                new Vector2(fOff, uOff + uvUnits.y * Depth),
                new Vector2(fOff + uvUnits.x * Width, uOff + uvUnits.y * Depth),

                // Down
                new Vector2(fOff, 0), new Vector2(fOff + uvUnits.x * Width, 0), new Vector2(fOff, uvUnits.y * Depth),
                new Vector2(fOff + uvUnits.x * Width, uvUnits.y * Depth)
            };

            mesh.uv = uv;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            if (Material == null)
            {
                meshRenderer.material = new Material(Shader.Find("Standard"));
                meshRenderer.material.name = "Default";
                meshRenderer.material.color = Color.green;
            }
            else
            {
                meshRenderer.material.CopyPropertiesFromMaterial(Material);
                meshRenderer.material.name = Material.name + " (Copy)";
            }

            var col = GetComponent<BoxCollider>();
            col.center = new Vector3(Width / 2, Height / 2, Depth / 2);
            col.size = new Vector3(Width, Height, Depth);
        }

        /// <summary>
        ///     Creates a cuboid object with the specified dimensions and an optional material parameter
        /// </summary>
        /// <param name="width">The width in unity units. Width is along the x axis.</param>
        /// <param name="height">The height in unity units. Height is along the y axis.</param>
        /// <param name="depth">The depth in untiy units. Depth is along the z axis.</param>
        /// <returns>A gameobject whose mesh and collider represent a cuboid with the specified dimensions.</returns>
        public static CuboidObject Create(float width, float height, float depth, Material material = null)
        {
            var go = new GameObject("CuboidObject");
            var co = go.AddComponent<CuboidObject>();
            co.Width = width;
            co.Height = height;
            co.Depth = depth;
            co.Material = material;
            return co;
        }

        private void OnValidate()
        {
            GenerateModel();
        }

        private void Start()
        {
            GenerateModel();
        }
    }
}