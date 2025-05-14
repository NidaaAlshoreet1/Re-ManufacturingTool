using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics; 
using OpenTK.WinForms;
using g3;
using System.Collections.Generic;
using ManufacturingTool;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using Swms.BusinessLogic.Additive.CompositerToolings.ManufacturingTool;
// using OpenTK.Graphics.OpenGL4;

namespace ManufacturingTool
{
    public partial class MainForm : Form
    {
        private GLControl glControl;
        private TabControl tabControl;
        private Panel rightPanel;
        private SplitContainer splitContainer;
        private ListBox featureListBox;
        private Button resetButton;
        private Button exportButton;
        private GroupBox instructionsGroup;

        // Shape and features management
        private BaseShape currentShape;
        private List features = new List(); // the Error is from Bib
        private Swms.BusinessLogic.Additive.CompositerToolings.ManufacturingTool.Feature previewFeature;
        private Swms.BusinessLogic.Additive.CompositerToolings.ManufacturingTool.Feature selectedFeature;
        private string addingFeatureType;

        // Camera and view control
        private float xRot = 0;
        private float yRot = 0;
        private float zoom = -15.0f;
        private Point lastMousePos;
        
        // STL import controls
        private Button importStlButton;
        private Label modelInfoLabel;
        private TrackBar scaleTrackBar;
        private Label scaleValueLabel;
        private TrackBar xPosTrackBar, yPosTrackBar, zPosTrackBar;
        private Label xPosLabel, yPosLabel, zPosLabel;


        // Feature controls
        private ComboBox featureTypeComboBox;
        private Panel featurePropertiesPanel;
        private Button placeFeatureButton;
        private Button cancelFeatureButton;
        private Button deleteFeatureButton;

        // Basic shape properties
        private float width = 5.0f;
        private float height = 5.0f;
        private float depth = 5.0f;

        public MainForm()
        {
            InitializeComponent();
            InitializeOpenGL();
            SetupUI();

            // Create initial cube shape
            currentShape = new Cube();

            // Or u Can use other shapes like:
            // currentShape = new Cylinder();
            // currentShape = new Sphere();
            // currentShape = new Cone();
            // currentShape = new Torus();
            // currentShape = new Pyramid();
            // currentShape = new Prism();
            // currentShape = new CustomShape();        
        }

        private void InitializeComponent()
        {
            this.Text = "3D Manufacturing Tool - Simple Version";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
        }
        
        private void InitializeOpenGL()
        {
            glControl = new GLControl();
            glControl.Load += GlControl_Load;
            glControl.Paint += GlControl_Paint;
            glControl.Resize += GlControl_Resize;
            glControl.MouseDown += GlControl_MouseDown;
            glControl.MouseMove += GlControl_MouseMove;
            glControl.MouseWheel += GlControl_MouseWheel;
        }
        
        private void SetupUI()
        {
            // Main layout
            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 900
            };
            this.Controls.Add(splitContainer);
            
            // Add GL control to the left panel
            splitContainer.Panel1.Controls.Add(glControl);
            glControl.Dock = DockStyle.Fill;
            
            // Right panel setup
            rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            splitContainer.Panel2.Controls.Add(rightPanel);
            
            // Tab control for different sections
            tabControl = new TabControl
            {
                Dock = DockStyle.Top,
                Height = 300
            };
            rightPanel.Controls.Add(tabControl);
            
            // Create tabs
            TabPage baseShapeTab = CreateBaseShapeTab();
            TabPage stlImportTab = CreateStlImportTab();
            TabPage featuresTab = CreateFeaturesTab();

            tabControl.TabPages.Add(baseShapeTab);
            tabControl.TabPages.Add(stlImportTab);
            tabControl.TabPages.Add(featuresTab);
            
            // Feature list
           GroupBox featureListGroup = new GroupBox
           {
               Text = "Feature List",
               Dock = DockStyle.Top,
               Height = 150,
               Top = tabControl.Bottom + 10
           };
           rightPanel.Controls.Add(featureListGroup);

           featureListBox = new ListBox
           {
               Dock = DockStyle.Fill
           };
           featureListBox.SelectedIndexChanged += FeatureListBox_SelectedIndexChanged;
           featureListGroup.Controls.Add(featureListBox);

           // Action buttons
           Panel actionsPanel = new Panel
           {
               Dock = DockStyle.Top,
               Height = 40,
               Top = featureListGroup.Bottom + 10
           };
           rightPanel.Controls.Add(actionsPanel);

           resetButton = new Button
           {
               Text = "Reset",
               Width = 100,
               Location = new Point(10, 10)
           };
           resetButton.Click += ResetButton_Click;
           actionsPanel.Controls.Add(resetButton);

           exportButton = new Button
           {
               Text = "Export Model",
               Width = 100,
               Location = new Point(120, 10)
           };
           exportButton.Click += ExportButton_Click;
           actionsPanel.Controls.Add(exportButton);


            // Instructions Box and Text
            GroupBox instructionsGroup = new GroupBox
            {
                Text = "Instructions",
                Dock = DockStyle.Top,
                Height = 150,
                Top = tabControl.Bottom + 10
            };
            rightPanel.Controls.Add(instructionsGroup);
            
            Label instructionsLabel = new Label
            {
                Text = "• Select a base shape or import an STL file\n" +
                       "• Adjust dimensions using sliders\n" +
                       "• Left-click and drag to rotate view\n" +
                       "• Right-click and drag to pan\n" +
                       "• Mouse wheel to zoom in/out",
                Dock = DockStyle.Fill,
                AutoSize = false
            };
            instructionsGroup.Controls.Add(instructionsLabel);
        }
        
        private TabPage CreateBaseShapeTab()
        {
            TabPage tab = new TabPage("Base Shape");
            
            // Shape buttons
            Button cubeButton = new Button
            {
                Text = "Cube",
                Width = 80,
                Location = new Point(10, 10)
            };
            cubeButton.Click += (s, e) => { glControl.Invalidate(); };
            tab.Controls.Add(cubeButton);
            
            Button cylinderButton = new Button
            {
                Text = "Cylinder",
                Width = 80,
                Location = new Point(100, 10)
            };
            cylinderButton.Click += (s, e) => MessageBox.Show("Cylinder shape not implemented in this simplified version.");
            tab.Controls.Add(cylinderButton);
            
            Button sphereButton = new Button
            {
                Text = "Sphere",
                Width = 80,
                Location = new Point(190, 10)
            };
            sphereButton.Click += (s, e) => CreateSphere();
            tab.Controls.Add(sphereButton);
    sphereButton.Click += (s, e) => MessageBox.Show("Sphere shape not implemented in this simplified version.");

            Button coneButton = new Button
            {
                Text = "Cone",
                Width = 80,
                Location = new Point(280, 10)
            };

            coneButton.Click += (s, e) => MessageBox.Show("Cone shape not implemented in this simplified version.");
            tab.Controls.Add(coneButton);
            Button torusButton = new Button
            {
                Text = "Torus",
                Width = 80,
                Location = new Point(370, 10)
            };
            torusButton.Click += (s, e) => MessageBox.Show("Torus shape not implemented in this simplified version.");


            // Dimensions group
            GroupBox dimensionsGroup = new GroupBox
            {
                Text = "Dimensions",
                Location = new Point(10, 50),
                Size = new Size(260, 150)
            };
            tab.Controls.Add(dimensionsGroup);
           
            // Width controls
            Label widthLabel = new Label
            {
                Text = "Width:",
                Location = new Point(10, 30),
                AutoSize = true
            };
            dimensionsGroup.Controls.Add(widthLabel);
            
            TrackBar widthTrackBar = new TrackBar
            {
                Minimum = 10,
                Maximum = 100,
                Value = 50,
                Location = new Point(70, 20),
                Width = 130
            };
            dimensionsGroup.Controls.Add(widthTrackBar);
            
            Label widthValueLabel = new Label
            {
                Text = "5.0",
                Location = new Point(210, 30),
                AutoSize = true
            };
            dimensionsGroup.Controls.Add(widthValueLabel);
            
            // Height controls
            Label heightLabel = new Label
            {
                Text = "Height:",
                Location = new Point(10, 70),
                AutoSize = true
            };
            dimensionsGroup.Controls.Add(heightLabel);
            
            TrackBar heightTrackBar = new TrackBar
            {
                Minimum = 10,
                Maximum = 100,
                Value = 50,
                Location = new Point(70, 60),
                Width = 130
            };
            dimensionsGroup.Controls.Add(heightTrackBar);
            
            Label heightValueLabel = new Label
            {
                Text = "5.0",
                Location = new Point(210, 70),
                AutoSize = true
            };
            dimensionsGroup.Controls.Add(heightValueLabel);
                    
            // Depth controls
            Label depthLabel = new Label
            {
                Text = "Depth:",
                Location = new Point(10, 110),
                AutoSize = true
            };
            dimensionsGroup.Controls.Add(depthLabel);

            TrackBar depthTrackBar = new TrackBar
            {
                Minimum = 10,
                Maximum = 100,
                Value = 50,
                Location = new Point(70, 100),
                Width = 130
            };
            dimensionsGroup.Controls.Add(depthTrackBar);

            Label depthValueLabel = new Label
            {
                Text = "5.0",
                Location = new Point(210, 110),
                AutoSize = true
            };
            dimensionsGroup.Controls.Add(depthValueLabel);

            // Connect dimension sliders
            EventHandler updateDimensions = (s, e) =>
            {
                width = widthTrackBar.Value / 10.0f;
                height = heightTrackBar.Value / 10.0f;
                depth = depthTrackBar.Value / 10.0f;

                widthValueLabel.Text = $"{width:F1}";
                heightValueLabel.Text = $"{height:F1}";
                depthValueLabel.Text = $"{depth:F1}";

                glControl.Invalidate();
                SetShapeDimensions(width, height, depth);
            };
            
            widthTrackBar.ValueChanged += updateDimensions;
            heightTrackBar.ValueChanged += updateDimensions;
            depthTrackBar.ValueChanged += updateDimensions;
            
            return tab;
        }
        private TabPage CreateStlImportTab()
        {
            TabPage tab = new TabPage("STL Import");
            
            // STL Import group
            GroupBox importGroup = new GroupBox
            {
                Text = "STL Import",
                Location = new Point(10, 10),
                Size = new Size(260, 80)
            };
            tab.Controls.Add(importGroup);
            
            Button importStlButton = new Button
            {
                Text = "Import STL File",
                Location = new Point(10, 20),
                Width = 240
            };
            // importStlButton.Click += (s, e) => MessageBox.Show("STL import not implemented in this simplified version.");
            importStlButton.Click += ImportStlButton_Click;
            importGroup.Controls.Add(importStlButton);
            
            modelInfoLabel = new Label
           {
               Text = "No model loaded",
               Location = new Point(10, 50),
               AutoSize = true
           };
           importGroup.Controls.Add(modelInfoLabel);

            // Model Scaling group
            GroupBox scalingGroup = new GroupBox
            {
                Text = "Model Scaling",
                Location = new Point(10, 100),
                Size = new Size(260, 60)
            };
            tab.Controls.Add(scalingGroup);

            Label scaleLabel = new Label
            {
                Text = "Scale:",
                Location = new Point(10, 25),
                AutoSize = true
            };
            scalingGroup.Controls.Add(scaleLabel);

            scaleTrackBar = new TrackBar
            {
                Minimum = 10,
                Maximum = 200,
                Value = 100,
                Location = new Point(60, 15),
                Width = 150
            };
            
            scaleTrackBar.ValueChanged += ScaleTrackBar_ValueChanged;
            scalingGroup.Controls.Add(scaleTrackBar);

            scaleValueLabel = new Label
            {
                Text = "1.0",
                Location = new Point(220, 25),
                AutoSize = true
            };
            scalingGroup.Controls.Add(scaleValueLabel);

            // Model Position group
            GroupBox positionGroup = new GroupBox
            {
                Text = "Model Position",
                Location = new Point(10, 170),
                Size = new Size(260, 120)
            };
            tab.Controls.Add(positionGroup);

            // X position
            Label xLabel = new Label
            {
                Text = "X Position:",
                Location = new Point(10, 25),
                AutoSize = true
            };
            positionGroup.Controls.Add(xLabel);

            xPosTrackBar = new TrackBar
            {
                Minimum = -50,
                Maximum = 50,
                Value = 0,
                Location = new Point(80, 15),
                Width = 130
            };
            xPosTrackBar.ValueChanged += PosTrackBar_ValueChanged;
            positionGroup.Controls.Add(xPosTrackBar);

            xPosLabel = new Label
            {
                Text = "0.0",
                Location = new Point(220, 25),
                AutoSize = true
            };
            positionGroup.Controls.Add(xPosLabel);

            // Y position
            Label yLabel = new Label
            {
                Text = "Y Position:",
                Location = new Point(10, 55),
                AutoSize = true
            };
            positionGroup.Controls.Add(yLabel);

            yPosTrackBar = new TrackBar
            {
                Minimum = -50,
                Maximum = 50,
                Value = 0,
                Location = new Point(80, 45),
                Width = 130
            };
            yPosTrackBar.ValueChanged += PosTrackBar_ValueChanged;
            positionGroup.Controls.Add(yPosTrackBar);

            yPosLabel = new Label
            {
                Text = "0.0",
                Location = new Point(220, 55),
                AutoSize = true
            };
            positionGroup.Controls.Add(yPosLabel);

            // Z position
            Label zLabel = new Label
            {
                Text = "Z Position:",
                Location = new Point(10, 85),
                AutoSize = true
            };
            positionGroup.Controls.Add(zLabel);

            zPosTrackBar = new TrackBar
            {
                Minimum = -50,
                Maximum = 50,
                Value = 0,
                Location = new Point(80, 75),
                Width = 130
            };
            zPosTrackBar.ValueChanged += PosTrackBar_ValueChanged;
            positionGroup.Controls.Add(zPosTrackBar);

            zPosLabel = new Label
            {
                Text = "0.0",
                Location = new Point(220, 85),
                AutoSize = true
            };
            positionGroup.Controls.Add(zPosLabel);
            return tab;
        }
         private TabPage CreateFeaturesTab()
        {
        TabPage tab = new TabPage("Features");

        // Feature type selection
        GroupBox typeGroup = new GroupBox
        {
            Text = "Feature Type",
            Location = new Point(10, 10),
            Size = new Size(260, 60)
        };
            tab.Controls.Add(typeGroup);

            featureTypeComboBox = new ComboBox
            {
                Location = new Point(10, 20),
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            featureTypeComboBox.Items.AddRange(new object[] { "Rib", "Hole", "Fillet", "Boss", "Slot" });
            featureTypeComboBox.SelectedIndexChanged += FeatureTypeComboBox_SelectedIndexChanged;
            typeGroup.Controls.Add(featureTypeComboBox);

            // Feature properties
            GroupBox propertiesGroup = new GroupBox
            {
                Text = "Feature Properties",
                Location = new Point(10, 80),
                Size = new Size(260, 120)
            };
            tab.Controls.Add(propertiesGroup);

            featurePropertiesPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            propertiesGroup.Controls.Add(featurePropertiesPanel);


            // Action buttons
            placeFeatureButton = new Button
            {
                Text = "Place Feature",
                Location = new Point(10, 210),
                Width = 120
            };
            placeFeatureButton.Click += PlaceFeatureButton_Click;
            tab.Controls.Add(placeFeatureButton);

            cancelFeatureButton = new Button
            {
                Text = "Cancel",
                Location = new Point(140, 210),
                Width = 120
            };
            cancelFeatureButton.Click += CancelFeatureButton_Click;
            tab.Controls.Add(cancelFeatureButton);

            // Delete button
            deleteFeatureButton = new Button
            {
                Text = "Delete Selected Feature",
                Location = new Point(10, 240),
                Width = 250,
                Enabled = false
            };
            deleteFeatureButton.Click += DeleteFeatureButton_Click;
            tab.Controls.Add(deleteFeatureButton);

            return tab;
        }

        #region OpenGL Event Handlers
        
        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0.95f, 0.95f, 0.97f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            
            // Set up light
            GL.Light(LightName.Light0, LightParameter.Position, new float[] { 5.0f, 10.0f, 5.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.2f, 0.2f, 0.2f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 0.8f, 0.8f, 0.8f, 1.0f });
            
            // Enable color material mode
            GL.Enable(EnableCap.ColorMaterial);
            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
        }
        
        //         private void GlControl_Load(object sender, EventArgs e)
        // {
        //     GL.ClearColor(0.95f, 0.95f, 0.97f, 1.0f);
        //     GL.Enable(EnableCap.DepthTest);
        //     GL.Enable(EnableCap.Lighting);
        //     GL.Enable(EnableCap.Light0);
            
        //     // Set up light
        //     GL.Light(LightName.Light0, LightParameter.Position, new float[] { 5.0f, 10.0f, 5.0f, 1.0f });
        //     GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.2f, 0.2f, 0.2f, 1.0f });
        //     GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 0.8f, 0.8f, 0.8f, 1.0f });
            
        //     // Enable color material mode
        //     GL.Enable(EnableCap.ColorMaterial);
        //     GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
        // }
        private void GlControl_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            
            float aspect = (float)glControl.Width / glControl.Height;
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45.0f),
                aspect,
                0.1f,
                100.0f);
            GL.LoadMatrix(ref perspective);
            
            //GL.MatrixMode(MatrixMode.Modelview);
        }
        
        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.LoadIdentity();
            
            // Apply view transformations
            GL.Translate(0.0f, 0.0f, zoom);
            GL.Rotate(xRot, 1.0f, 0.0f, 0.0f);
            GL.Rotate(yRot, 0.0f, 1.0f, 0.0f);
            
            // Draw grid
            DrawGrid();
            
            // Draw axes
            DrawAxes();
            
            // // Draw cube
            // DrawCube();
            
            // Draw current shape
            currentShape?.Draw();

            // Draw features
            foreach (var feature in features)
            {
                if (feature == selectedFeature)
                {
                    // Highlight selected feature
                    float[] originalColor = feature.Color;
                    feature.Color = new float[] { 1.0f, 1.0f, 0.0f, 1.0f }; // Yellow highlight
                    feature.Draw();
                    feature.Color = originalColor;
                }
                else
                {
                    feature.Draw();
                }
            }

            // Draw preview feature
            previewFeature?.Draw();
            glControl.SwapBuffers();
        }
        
        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePos = e.Location;
        }
        
        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.X - lastMousePos.X;
                int dy = e.Y - lastMousePos.Y;
                
                yRot += dx;
                xRot += dy;
                
                glControl.Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                int dy = e.Y - lastMousePos.Y;
                zoom += dy / 10.0f;
                
                glControl.Invalidate();
            }
            
            lastMousePos = e.Location;
        }
        
        private void GlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            zoom += e.Delta / 120.0f;
            glControl.Invalidate();
        }
        
        #endregion
        
        #region Drawing Methods
        
        private void DrawGrid()
        {
            GL.Disable(EnableCap.Lighting);
            GL.Color3(0.7f, 0.7f, 0.7f);
            
            GL.Begin(PrimitiveType.Lines);
            for (int i = -10; i <= 10; i++)
            {
                GL.Vertex3(i, 0, -10);
                GL.Vertex3(i, 0, 10);
                GL.Vertex3(-10, 0, i);
                GL.Vertex3(10, 0, i);
            }
            GL.End();
            
            GL.Enable(EnableCap.Lighting);
        }
        
        private void DrawAxes()
        {
            GL.Disable(EnableCap.Lighting);
            
            GL.Begin(PrimitiveType.Lines);
            // X axis (red)
            GL.Color3(1.0f, 0.0f, 0.0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(5, 0, 0);
            
            // Y axis (green)
            GL.Color3(0.0f, 1.0f, 0.0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 5, 0);
            
            // Z axis (blue)
            GL.Color3(0.0f, 0.0f, 1.0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 5);
            GL.End();
            
            GL.Enable(EnableCap.Lighting);
        }
        
        private void DrawCube()
        {
            GL.Color4(0.2f, 0.5f, 0.8f, 1.0f);
            
            // Define the vertices of the cube
            float w = width / 2;
            float h = height / 2;
            float d = depth / 2;
            
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
        }
        
        #endregion
    
        #region Shape Management
        

        private void CreateCube()
        {
            currentShape = new Cube();
            glControl.Invalidate();
        }

        private void CreateCylinder()
        {
            // In a full implementation, this would create a cylinder shape
            MessageBox.Show("Cylinder creation not implemented in this demo.");
        }

        private void CreateSphere()
        {
            // In a full implementation, this would create a sphere shape
            MessageBox.Show("Sphere creation not implemented in this demo.");
        }

        private void SetShapeDimensions(float width, float height, float depth)
        {
            if (currentShape != null)
            {
                currentShape.Width = width;
                currentShape.Height = height;
                currentShape.Depth = depth;
                glControl.Invalidate();
            }
        }

        #endregion

        #region STL Import
                private void ImportStlButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "STL Files (*.stl)|*.stl|All Files (*.*)|*.*";
                dialog.Title = "Open STL File";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadStlFile(dialog.FileName);
                }
            }
        }

        private void LoadStlFile(string filename)
        {
            try
            {
                // Create a new STL model
                STLModel stlModel = new STLModel();

                if (stlModel.LoadStl(filename))
                {
                    currentShape = stlModel;
                    modelInfoLabel.Text = $"Loaded: {Path.GetFileName(filename)}";
                    glControl.Invalidate();
                }
                else
                {
                    MessageBox.Show("Failed to load STL file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CreateCube(); // Fallback to cube
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading STL file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CreateCube(); // Fallback to cube
            }
        }

        private void ScaleTrackBar_ValueChanged(object sender, EventArgs e)
        {
            float scale = scaleTrackBar.Value / 100.0f;
            scaleValueLabel.Text = $"{scale:F1}";

            if (currentShape is STLModel stlModel)
            {
                stlModel.ScaleFactor = scale;
                glControl.Invalidate();
            }
        }

        private void PosTrackBar_ValueChanged(object sender, EventArgs e)
        {
            float x = xPosTrackBar.Value / 10.0f;
            float y = yPosTrackBar.Value / 10.0f;
            float z = zPosTrackBar.Value / 10.0f;

            xPosLabel.Text = $"{x:F1}";
            yPosLabel.Text = $"{y:F1}";
            zPosLabel.Text = $"{z:F1}";

            if (currentShape != null)
            {
                currentShape.Position = new float[] { x, y, z };
                glControl.Invalidate();
            }
        }


        #endregion

        # region Feature Management
        private void FeatureTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (featureTypeComboBox.SelectedItem != null)
            {
                string featureType = featureTypeComboBox.SelectedItem.ToString().ToLower();
                StartAddingFeature(featureType);
            }
        }

        private void StartAddingFeature(string featureType)
        {
            addingFeatureType = featureType;

            switch (featureType)
            {
                case "rib":
                    previewFeature = new Rib(new float[] { 0, 0, 0 }, new float[] { 0, 1, 0 });
                    break;
                case "hole":
                    previewFeature = new Hole(new float[] { 0, 0, 0 }, new float[] { 0, 1, 0 });
                    break;
                case "fillet":
                    previewFeature = new Fillet(new float[] { 0, 0, 0 }, new float[] { 0, 1, 0 });
                    break;
                case "boss":
                    previewFeature = new Boss(new float[] { 0, 0, 0 }, new float[] { 0, 1, 0 });
                    break;
                case "slot":
                    previewFeature = new Slot(new float[] { 0, 0, 0 }, new float[] { 0, 1, 0 });
                    break;
            }

            UpdateFeaturePropertiesPanel(previewFeature);
            glControl.Invalidate();
        }

        private void UpdateFeaturePropertiesPanel(Swms.BusinessLogic.Additive.CompositerToolings.ManufacturingTool.Feature feature)
        {
            featurePropertiesPanel.Controls.Clear();

            if (feature == null)
            {
                Label noFeatureLabel = new Label
                {
                    Text = "No feature selected",
                    AutoSize = true,
                    Location = new Point(10, 10)
                };
                featurePropertiesPanel.Controls.Add(noFeatureLabel);
                return;
            }

            // Get properties
            Dictionary properties = feature.GetProperties();
            int y = 10;

            foreach (var prop in properties)
            {
                Label propLabel = new Label
                {
                    Text = $"{char.ToUpper(prop.Key[0]) + prop.Key.Substring(1)}:",
                    AutoSize = true,
                    Location = new Point(10, y + 5)
                };
                featurePropertiesPanel.Controls.Add(propLabel);

                NumericUpDown propNumeric = new NumericUpDown
                {
                    Minimum = 0.1m,
                    Maximum = 20.0m,
                    DecimalPlaces = 1,
                    Increment = 0.1m,
                    Value = (decimal)prop.Value,
                    Location = new Point(100, y),
                    Width = 80,
                    Tag = prop.Key // Store property name in Tag
                };
                propNumeric.ValueChanged += PropNumeric_ValueChanged;
                featurePropertiesPanel.Controls.Add(propNumeric);

                y += 30;
            }
        }

        private void PropNumeric_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown control = (NumericUpDown)sender;
            string propertyName = (string)control.Tag;
            float value = (float)control.Value;

            Dictionary properties = new Dictionary
            {
                { propertyName, value }
            };

            if (previewFeature != null)
            {
                previewFeature.SetProperties(properties);
                glControl.Invalidate();
            }
            else if (selectedFeature != null)
            {
                selectedFeature.SetProperties(properties);
                glControl.Invalidate();
            }
        }

        private void PlaceFeatureButton_Click(object sender, EventArgs e)
        {
            if (previewFeature != null)
            {
                // Adjust position based on shape dimensions
                if (currentShape != null)
                {
                    // Place on top of the shape by default
                    previewFeature.Position = new float[]
                    {
                        0,
                        currentShape.Height / 2,
                        0
                    };
                }

                // Add feature to the list
                features.Add(previewFeature);

                // Add to feature list box
                featureListBox.Items.Add($"{char.ToUpper(previewFeature.FeatureType[0]) + previewFeature.FeatureType.Substring(1)} {previewFeature.Id % 1000}");

                // Create a new preview feature of the same type
                Swms.BusinessLogic.Additive.CompositerToolings.ManufacturingTool.Feature newFeature = previewFeature.Clone();
                previewFeature = newFeature;

                glControl.Invalidate();
            }
        }

        private void CancelFeatureButton_Click(object sender, EventArgs e)
        {
            addingFeatureType = null;
            previewFeature = null;
            glControl.Invalidate();
        }

        private void FeatureListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = featureListBox.SelectedIndex;
            if (index >= 0 && index < features.Count)
            {
                selectedFeature = features[index];
                UpdateFeaturePropertiesPanel(selectedFeature);
                deleteFeatureButton.Enabled = true;
                glControl.Invalidate();
            }
        }

        private void DeleteFeatureButton_Click(object sender, EventArgs e)
        {
            if (selectedFeature != null && featureListBox.SelectedIndex >= 0)
            {
                int index = featureListBox.SelectedIndex;
                features.RemoveAt(index);
                featureListBox.Items.RemoveAt(index);

                selectedFeature = null;
                UpdateFeaturePropertiesPanel(null);
                deleteFeatureButton.Enabled = false;

                glControl.Invalidate();
            }
        }
        #endregion

        #region Other UI Event Handlers
        private void ResetButton_Click(object sender, EventArgs e)
        {
            // Reset everything
            currentShape = new Cube();
            features.Clear();
            featureListBox.Items.Clear();
            previewFeature = null;
            selectedFeature = null;
            addingFeatureType = null;

            // Reset UI
            modelInfoLabel.Text = "No model loaded";
            scaleTrackBar.Value = 100;
            xPosTrackBar.Value = 0;
            yPosTrackBar.Value = 0;
            zPosTrackBar.Value = 0;

            UpdateFeaturePropertiesPanel(null);
            deleteFeatureButton.Enabled = false;

            glControl.Invalidate();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("In a full implementation, this would export your model in STL format for 3D printing.",
                "Export Model", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
               
        #endregion

    }
}


