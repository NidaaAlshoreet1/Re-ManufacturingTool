using g3;
using OpenTK.Graphics.OpenGL;

namespace ManufacturingTool
{
    public abstract class BaseShape
    {
        public float Width { get; set; } = 5.0f;
        public float Height { get; set; } = 5.0f;
        public float Depth { get; set; } = 5.0f;
        public float[] Position { get; set; } = new float[] { 0, 0, 0 };
        public float[] Color { get; set; } = new float[] { 0.2f, 0.5f, 0.8f, 1.0f };  // RGBA
        
        public abstract void Draw();
    }
    
    public class Cube : BaseShape
    {
        public override void Draw()
        {
            GL.Color4(Color);
            
            // Define the vertices of the cube
            float w = Width / 2;
            float h = Height / 2;
            float d = Depth / 2;
            
            float[][] vertices = new float[][]
            {
                new float[] { -w, -h, -d }, new float[] { w, -h, -d }, new float[] { w, h, -d }, new float[] { -w, h, -d },
                new float[] { -w, -h, d }, new float[] { w, -h, d }, new float[] { w, h, d }, new float[] { -w, h, d }
            };
            
            // Define the faces using indices
            int[][] faces = new int[][]
            {
                new int[] { 0, 1, 2, 3 },  // Back face
                new int[] { 4, 5, 6, 7 },  // Front face
                new int[] { 0, 4, 7, 3 },  // Left face
                new int[] { 1, 5, 6, 2 },  // Right face
                new int[] { 3, 2, 6, 7 },  // Top face
                new int[] { 0, 1, 5, 4 }   // Bottom face
            };
            
            GL.PushMatrix();
            GL.Translate(Position[0], Position[1], Position[2]);
            
            // Draw each face as a quad
            GL.Begin(PrimitiveType.Quads);
            foreach (int[] face in faces)
            {
                foreach (int idx in face)
                {
                    GL.Vertex3(vertices[idx][0], vertices[idx][1], vertices[idx][2]);
                }
            }
            GL.End();
            
            // Draw edges in black
            GL.Color4(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Begin(PrimitiveType.Lines);
            int[][] edges = new int[][]
            {
                new int[] { 0, 1 }, new int[] { 1, 2 }, new int[] { 2, 3 }, new int[] { 3, 0 },
                new int[] { 4, 5 }, new int[] { 5, 6 }, new int[] { 6, 7 }, new int[] { 7, 4 },
                new int[] { 0, 4 }, new int[] { 1, 5 }, new int[] { 2, 6 }, new int[] { 3, 7 }
            };
            
            foreach (int[] edge in edges)
            {
                foreach (int idx in edge)
                {
                    GL.Vertex3(vertices[idx][0], vertices[idx][1], vertices[idx][2]);
                }
            }
            GL.End();
            
            GL.PopMatrix();
        }
    }
    
    public class STLModel : BaseShape
    {
        private DMesh3 mesh;
        private int displayList;
        public string Filename { get; private set; } = "";
        public float ScaleFactor { get; set; } = 1.0f;
        public float[] CenterOffset { get; set; } = new float[] { 0, 0, 0 };
        
        public bool LoadStl(string filename)
        {
            try
            {
                // Load the STL file using geometry3Sharp
                StandardMeshReader reader = new StandardMeshReader();
                reader.MeshBuilder = new DMesh3Builder();
                reader.Read(filename, ReadOptions.Defaults);
                mesh = ((DMesh3Builder)reader.MeshBuilder).Meshes[0];
                Filename = Path.GetFileName(filename);
                
                // Calculate bounding box for scaling
                AxisAlignedBox3d bounds = mesh.CachedBounds;
                Vector3d centroid = mesh.CachedBounds.Center;
                CenterOffset = new float[] { -(float)centroid.x, -(float)centroid.y, -(float)centroid.z };
                
                // Calculate dimensions
                Vector3d dimensions = bounds.Max - bounds.Min;
                double maxDim = Math.Max(Math.Max(dimensions.x, dimensions.y), dimensions.z);
                
                // Scale to fit in a 5x5x5 box by default
                ScaleFactor = maxDim > 0 ? 5.0f / (float)maxDim : 1.0f;
                
                // Update shape dimensions based on the mesh
                Width = (float)dimensions.x * ScaleFactor;
                Height = (float)dimensions.y * ScaleFactor;
                Depth = (float)dimensions.z * ScaleFactor;
                
                // Create display list for faster rendering
                if (displayList != 0)
                {
                    GL.DeleteLists(displayList, 1);
                }
                
                displayList = GL.GenLists(1);
                GL.NewList(displayList, ListMode.Compile);
                
                // Draw the mesh
                GL.Begin(PrimitiveType.Triangles);
                
                foreach (int tid in mesh.TriangleIndices())
                {
                    Index3i tri = mesh.GetTriangle(tid);
                    
                    // Get vertices for this face
                    Vector3d v1 = mesh.GetVertex(tri.a);
                    Vector3d v2 = mesh.GetVertex(tri.b);
                    Vector3d v3 = mesh.GetVertex(tri.c);
                    
                    // Calculate face normal
                    Vector3d edge1 = v2 - v1;
                    Vector3d edge2 = v3 - v1;
                    Vector3d normal = Vector3d.Cross(edge1, edge2).Normalized;
                    
                    // Set normal and draw vertices
                    GL.Normal3(normal.x, normal.y, normal.z);
                    GL.Vertex3(v1.x, v1.y, v1.z);
                    GL.Vertex3(v2.x, v2.y, v2.z);
                    GL.Vertex3(v3.x, v3.y, v3.z);
                }
                
                GL.End();
                GL.EndList();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading STL file: {ex.Message}");
                return false;
            }
        }
        
        public override void Draw()
        {
            if (mesh == null || displayList == 0)
                return;
            
            GL.Color4(Color);
            
            GL.PushMatrix();
            GL.Translate(Position[0], Position[1], Position[2]);
            
            // Apply scaling and centering
            GL.Translate(CenterOffset[0], CenterOffset[1], CenterOffset[2]);
            GL.Scale(ScaleFactor, ScaleFactor, ScaleFactor);
            
            // Call the display list
            GL.CallList(displayList);
            
            GL.PopMatrix();
        }
    }
}