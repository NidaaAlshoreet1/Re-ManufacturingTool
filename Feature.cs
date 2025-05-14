using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Swms.BusinessLogic.Additive.CompositerToolings.ManufacturingTool
{
    public abstract class Feature
    {
        public float[] Position { get; set; } 
        public float[] Normal { get; set; }
        public string FeatureType { get; set; }
        public float[] Color { get; set; }
        public int Id { get; }
    
        protected Feature(float[] position, float[] normal, string featureType, float[] color, int id)
        {
            Position = (float[])position.Clone();
            Normal = (float[])normal.Clone(); ;
            FeatureType = featureType;
            Color = new float[] { 0.2f, 0.8f, 0.2f, 1.0f};
            Id = GetHashCode;
        }

        public abstract void Draw();

        public abstract Directory GetDirectories();

        public abstract void SetProperties(Directory directory);

        public abstract Feature Clone();

        public void ApplyRotationToNormal ()
        {
            // Create a rotation matrix to align with the normal
            float[] up = new float[] { 0, 1, 0 };  // Default up vector

            // Calculate dot product
            float dot = up[0] * Normal[0] + up[1] * Normal[1] + up[2] * Normal[2];

            if (Math.Abs(dot + 1.0f) < 0.000001f)  // If normal is opposite to up
            {
                GL.Rotate(180, 1.0f, 0.0f, 0.0f);
            }
            else if (Math.Abs(dot - 1.0f) > 0.000001f)  // If normal is not parallel to up
            {
                // Calculate cross product for rotation axis
                float[] axis = new float[]
                {
                    up[1] * Normal[2] - up[2] * Normal[1],
                    up[2] * Normal[0] - up[0] * Normal[2],
                    up[0] * Normal[1] - up[1] * Normal[0]
                };

                // Calculate angle in degrees
                float angle = (float)(Math.Acos(dot) * 180.0 / Math.PI);

                GL.Rotate(angle, axis[0], axis[1], axis[2]);
            }
        }
    }

    public class Rib : Feature
    {

        public float Height { get; set; } = 1.0f;
        public float Thickness { get; set; } = 0.5f;
        public float Width { get; set; } = 2.0f;

        public Rib(float[] position, float[] normal) : base(position, normal, "rib")
        {
            Color = new float[] { 0.1f, 0.7f, 0.3f, 1.0f };
        }
        public override void Draw()
        {
            GL.Color4(Color);

            GL.PushMatrix();
            GL.Translate(Position[0], Position[1], Position[2]);

            // Apply rotation to align with normal
            ApplyRotationToNormal();

            // Draw the rib as a box
            float w = Width / 2;
            float h = Height / 2;
            float d = Thickness / 2;

            float[][] vertices = new float[][]
            {
                new float[] { -w, 0, -d }, new float[] { w, 0, -d }, new float[] { w, 2*h, -d }, new float[] { -w, 2*h, -d },
                new float[] { -w, 0, d }, new float[] { w, 0, d }, new float[] { w, 2*h, d }, new float[] { -w, 2*h, d }
            };

            int[][] faces = new int[][]
            {
                new int[] { 0, 1, 2, 3 },  // Back face
                new int[] { 4, 5, 6, 7 },  // Front face
                new int[] { 0, 4, 7, 3 },  // Left face
                new int[] { 1, 5, 6, 2 },  // Right face
                new int[] { 3, 2, 6, 7 },  // Top face
                new int[] { 0, 1, 5, 4 }   // Bottom face
            };

            GL.Begin(PrimitiveType.Quads);
            foreach (int[] face in faces)
            {
                foreach (int idx in face)
                {
                    GL.Vertex3(vertices[idx][0], vertices[idx][1], vertices[idx][2]);
                }
            }
            GL.End();

            GL.PopMatrix();
        }

        public override Dictionary GetProperties()
        {
            return new Dictionary
            {
                { "height", Height },
                { "thickness", Thickness },
                { "width", Width }
            };
        }

        public override void SetProperties(Dictionary properties)
        {
            if (properties.ContainsKey("height"))
                Height = properties["height"];
            if (properties.ContainsKey("thickness"))
                Thickness = properties["thickness"];
            if (properties.ContainsKey("width"))
                Width = properties["width"];
        }

        public override Feature Clone()
        {
            Rib newRib = new Rib(Position, Normal);
            newRib.Height = Height;
            newRib.Thickness = Thickness;
            newRib.Width = Width;
            return newRib;
        }
    }
    public class Hole : Feature
    {
        public float Diameter { get; set; } = 1.0f;
        public float Depth { get; set; } = 2.0f;

        public Hole(float[] position, float[] normal) : base(position, normal, "hole")
        {
            Color = new float[] { 0.8f, 0.2f, 0.2f, 0.7f };  // Semi-transparent red
        }

        public override void Draw()
        {
            GL.Color4(Color);

            GL.PushMatrix();
            GL.Translate(Position[0], Position[1], Position[2]);

            // Apply rotation to align with normal
            ApplyRotationToNormal();

            // Draw the hole as a cylinder
            float radius = Diameter / 2;
            float height = Depth;
            int slices = 32;
            int stacks = 1;

            // Enable blending for transparency
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Draw cylinder using GLU quadric
            DrawCylinder(radius, height, slices, stacks);

            GL.Disable(EnableCap.Blend);
            GL.PopMatrix();
        }

        private void DrawCylinder(float radius, float height, int slices, int stacks)
        {
            // Draw the cylinder sides
            GL.Begin(PrimitiveType.Quad);
            for (int i = 0; i < slices; i++)
            {
                float angle1 = (float)(i * 2.0 * Math.PI / slices);
                float angle2 = (float)((i + 1) * 2.0 * Math.PI / slices);

                float x1 = (float)Math.Cos(angle1) * radius;
                float z1 = (float)Math.Sin(angle1) * radius;
                float x2 = (float)Math.Cos(angle2) * radius;
                float z2 = (float)Math.Sin(angle2) * radius;

                // Bottom circle
                GL.Normal3(0.0f, -1.0f, 0.0f);
                GL.Vertex3(0.0f, 0.0f, 0.0f);
                GL.Vertex3(x1, 0.0f, z1);
                GL.Vertex3(x2, 0.0f, z2);

                // Top circle
                GL.Normal3(0.0f, 1.0f, 0.0f);
                GL.Vertex3(0.0f, height, 0.0f);
                GL.Vertex3(x1, height, z1);
                GL.Vertex3(x2, height, z2);

                // Side
                GL.Normal3(x1, 0.0f, z1);
                GL.Vertex3(x1, 0.0f, z1);
                GL.Vertex3(x1, height, z1);
                GL.Vertex3(x2, height, z2);
                GL.Vertex3(x2, 0.0f, z2);
            }
            GL.End();
        }

        public override Dictionary GetProperties()
        {
            return new Dictionary
            {
                { "diameter", Diameter },
                { "depth", Depth }
            };
        }

        public override void SetProperties(Dictionary properties)
        {
            if (properties.ContainsKey("diameter"))
                Diameter = properties["diameter"];
            if (properties.ContainsKey("depth"))
                Depth = properties["depth"];
        }

        public override Feature Clone()
        {
            Hole newHole = new Hole(Position, Normal);
            newHole.Diameter = Diameter;
            newHole.Depth = Depth;
            return newHole;
        }
    }
    public class Fillet : Feature
    {
        public float Radius { get; set; } = 0.5f;
        public float Length { get; set; } = 2.0f;

        public Fillet(float[] position, float[] normal) : base(position, normal, "fillet")
        {
            Color = new float[] { 0.2f, 0.6f, 0.8f, 1.0f };
        }

        public override void Draw()
        {
            GL.Color4(Color);

            GL.PushMatrix();
            GL.Translate(Position[0], Position[1], Position[2]);

            // Apply rotation to align with normal
            ApplyRotationToNormal();

            // Draw the fillet as a partial cylinder
            float radius = Radius;
            float length = Length;
            int slices = 32;

            // Rotate to position
            GL.Rotate(90, 0.0f, 0.0f, 1.0f);
            GL.Translate(0, 0, -length / 2);

            // Draw the quarter cylinder
            GL.PushMatrix();
            GL.Rotate(-90, 1.0f, 0.0f, 0.0f);

            // Draw the curved part
            GL.Begin(PrimitiveType.QuadStrip);
            for (int i = 0; i <= slices / 4; i++)
            {
                float angle = (float)(i * (Math.PI / 2) / (slices / 4));
                float x = (float)(radius * Math.Cos(angle));
                float y = (float)(radius * Math.Sin(angle));
                GL.Vertex3(x, y, 0);
                GL.Vertex3(x, y, length);
            }
            GL.End();

            // Draw the end caps
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Vertex3(0, 0, 0);
            for (int i = 0; i <= slices / 4; i++)
            {
                float angle = (float)(i * (Math.PI / 2) / (slices / 4));
                float x = (float)(radius * Math.Cos(angle));
                float y = (float)(radius * Math.Sin(angle));
                GL.Vertex3(x, y, 0);
            }
            GL.End();

            GL.Begin(PrimitiveType.TriangleFan);
            GL.Vertex3(0, 0, length);
            for (int i = 0; i <= slices / 4; i++)
            {
                float angle = (float)((slices / 4 - i) * (Math.PI / 2) / (slices / 4));
                float x = (float)(radius * Math.Cos(angle));
                float y = (float)(radius * Math.Sin(angle));
                GL.Vertex3(x, y, length);
            }
            GL.End();

            GL.PopMatrix();
            GL.PopMatrix();
        }

        public override Dictionary GetProperties()
        {
            return new Dictionary
            {
                { "radius", Radius },
                { "length", Length }
            };
        }

        public override void SetProperties(Dictionary properties)
        {
            if (properties.ContainsKey("radius"))
                Radius = properties["radius"];
            if (properties.ContainsKey("length"))
                Length = properties["length"];
        }

        public override Feature Clone()
        {
            Fillet newFillet = new Fillet(Position, Normal);
            newFillet.Radius = Radius;
            newFillet.Length = Length;
            return newFillet;
        }
    }
    public class Boss : Feature
    {
        public float Diameter { get; set; } = 1.5f;
        public float Height { get; set; } = 1.0f;

        public Boss(float[] position, float[] normal) : base(position, normal, "boss")
        {
            Color = new float[] { 0.8f, 0.6f, 0.2f, 1.0f };
        }

        public override void Draw()
        {
            GL.Color4(Color);

            GL.PushMatrix();
            GL.Translate(Position[0], Position[1], Position[2]);

            // Apply rotation to align with normal
            ApplyRotationToNormal();

            // Draw the boss as a cylinder
            float radius = Diameter / 2;
            float height = Height;
            int slices = 32;

            // Draw cylinder
            GL.Rotate(90, 1.0f, 0.0f, 0.0f);  // Rotate to stand upright

            // Draw the cylinder sides
            GL.Begin(PrimitiveType.QuadStrip);
            for (int i = 0; i <= slices; i++)
            {
                float angle = (float)(i * 2.0 * Math.PI / slices);
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;

                GL.Normal3(x, 0, z);
                GL.Vertex3(x, 0, z);
                GL.Vertex3(x, height, z);
            }
            GL.End();

            // Draw the top and bottom caps
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Normal3(0, -1, 0);
            GL.Vertex3(0, 0, 0);
            for (int i = 0; i <= slices; i++)
            {
                float angle = (float)(i * 2.0 * Math.PI / slices);
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;
                GL.Vertex3(x, 0, z);
            }
            GL.End();

            GL.Begin(PrimitiveType.TriangleFan);
            GL.Normal3(0, 1, 0);
            GL.Vertex3(0, height, 0);
            for (int i = slices; i >= 0; i--)
            {
                float angle = (float)(i * 2.0 * Math.PI / slices);
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;
                GL.Vertex3(x, height, z);
            }
            GL.End();

            GL.PopMatrix();
        }

        public override Dictionary GetProperties()
        {
            return new Dictionary
            {
                { "diameter", Diameter },
                { "height", Height }
            };
        }

        public override void SetProperties(Dictionary properties)
        {
            if (properties.ContainsKey("diameter"))
                Diameter = properties["diameter"];
            if (properties.ContainsKey("height"))
                Height = properties["height"];
        }

        public override Feature Clone()
        {
            Boss newBoss = new Boss(Position, Normal);
            newBoss.Diameter = Diameter;
            newBoss.Height = Height;
            return newBoss;
        }
    }
    public class Slot : Feature
    {
        public float Width { get; set; } = 0.8f;
        public float Length { get; set; } = 3.0f;
        public float Depth { get; set; } = 1.0f;

        public Slot(float[] position, float[] normal) : base(position, normal, "slot")
        {
            Color = new float[] { 0.5f, 0.3f, 0.7f, 1.0f };
        }

        public override void Draw()
        {
            GL.Color4(Color);

            GL.PushMatrix();
            GL.Translate(Position[0], Position[1], Position[2]);

            // Apply rotation to align with normal
            ApplyRotationToNormal();

            // Draw the slot as a box with rounded ends
            float w = Width / 2;
            float l = Length / 2;
            float d = Depth;

            // Draw the main rectangular part
            GL.Begin(PrimitiveType.Quads);
            // Bottom face
            GL.Normal3(0, -1, 0);
            GL.Vertex3(-w, 0, -l + w);
            GL.Vertex3(w, 0, -l + w);
            GL.Vertex3(w, 0, l - w);
            GL.Vertex3(-w, 0, l - w);

            // Top face
            GL.Normal3(0, 1, 0);
            GL.Vertex3(-w, d, -l + w);
            GL.Vertex3(w, d, -l + w);
            GL.Vertex3(w, d, l - w);
            GL.Vertex3(-w, d, l - w);

            // Side faces
            GL.Normal3(-1, 0, 0);
            GL.Vertex3(-w, 0, -l + w);
            GL.Vertex3(-w, d, -l + w);
            GL.Vertex3(-w, d, l - w);
            GL.Vertex3(-w, 0, l - w);

            GL.Normal3(1, 0, 0);
            GL.Vertex3(w, 0, -l + w);
            GL.Vertex3(w, d, -l + w);
            GL.Vertex3(w, d, l - w);
            GL.Vertex3(w, 0, l - w);

            // End faces
            GL.Normal3(0, 0, -1);
            GL.Vertex3(-w, 0, -l + w);
            GL.Vertex3(w, 0, -l + w);
            GL.Vertex3(w, d, -l + w);
            GL.Vertex3(-w, d, -l + w);

            GL.Normal3(0, 0, 1);
            GL.Vertex3(-w, 0, l - w);
            GL.Vertex3(w, 0, l - w);
            GL.Vertex3(w, d, l - w);
            GL.Vertex3(-w, d, l - w);
            GL.End();

            // Draw the rounded ends
            // Negative end cap
            GL.PushMatrix();
            GL.Translate(0, d / 2, -l + w);
            GL.Rotate(90, 1, 0, 0);
            DrawCylinder(w, d, 16);
            GL.PopMatrix();

            // Positive end cap
            GL.PushMatrix();
            GL.Translate(0, d / 2, l - w);
            GL.Rotate(90, 1, 0, 0);
            DrawCylinder(w, d, 16);
            GL.PopMatrix();

            GL.PopMatrix();
        }

        private void DrawCylinder(float radius, float height, int slices)
        {
            // Draw the cylinder sides
            GL.Begin(PrimitiveType.QuadStrip);
            for (int i = 0; i <= slices; i++)
            {
                float angle = (float)(i * 2.0 * Math.PI / slices);
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;

                GL.Normal3(x, 0, z);
                GL.Vertex3(x, 0, z);
                GL.Vertex3(x, height, z);
            }
            GL.End();

            // Draw the top and bottom caps
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Normal3(0, -1, 0);
            GL.Vertex3(0, 0, 0);
            for (int i = 0; i <= slices; i++)
            {
                float angle = (float)(i * 2.0 * Math.PI / slices);
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;
                GL.Vertex3(x, 0, z);
            }
            GL.End();

            GL.Begin(PrimitiveType.TriangleFan);
            GL.Normal3(0, 1, 0);
            GL.Vertex3(0, height, 0);
            for (int i = slices; i >= 0; i--)
            {
                float angle = (float)(i * 2.0 * Math.PI / slices);
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;
                GL.Vertex3(x, height, z);
            }
            GL.End();
        }

        public override Dictionary GetProperties()
        {
            return new Dictionary
            {
                { "width", Width },
                { "length", Length },
                { "depth", Depth }
            };
        }

        public override void SetProperties(Dictionary properties)
        {
            if (properties.ContainsKey("width"))
                Width = properties["width"];
            if (properties.ContainsKey("length"))
                Length = properties["length"];
            if (properties.ContainsKey("depth"))
                Depth = properties["depth"];
        }

        public override Feature Clone()
        {
            Slot newSlot = new Slot(Position, Normal);
            newSlot.Width = Width;
            newSlot.Length = Length;
            newSlot.Depth = Depth;
            return newSlot;
        }
    }
}

