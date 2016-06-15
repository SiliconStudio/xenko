using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SimpleTerrain
{
    /// <summary>
    /// This script rotates around Oy the entity it is attached to.
    /// </summary>
    public class TerrainScript : StartupScript
    {
        public Entity TerrainEntity;        // Manipulate the entity components: Transformation component
        public Entity UIEntity;
        public Entity CameraEntity;
        public Entity DirectionalLight0;
        public Entity DirectionalLight1;

        public SpriteFont Font;

        public Material TerrainMaterial;
        public Texture WaterTexture;
        public Texture GrassTexture;
        public Texture MountainTexture;

        // Terrain Parameters
        private const int MinTerrainSizePowerFactor = 2;
        private const int MaxTerrainSizePowerFactor = 8;

        private Mesh terrainMesh;           // Update a number of element to draw
        private Buffer terrainVertexBuffer; // Set Vertex Buffer on the fly
        private Buffer terrainIndexBuffer;  // Set Index Buffer on the fly

        // Fault formation algorithm Parameters
        private int terrainSizePowerFactor = MaxTerrainSizePowerFactor;
        private int iterationPowerFactor = 5;
        private float filterHeightBandStrength = 0.5f;
        private float terrainHeightScale = 100f;

        // Camera Parameters
        private Vector3 cameraStartPosition;
        private float zoomFactor = 1;

        // UI Parameters
        private ModalElement loadingModal;
        private TextBlock loadingTextBlock;

        #region Fault formation properties
        private int TerrainSizePowerFactor
        {
            get { return terrainSizePowerFactor; }
            set
            {
                if (value < MinTerrainSizePowerFactor) value = MinTerrainSizePowerFactor;
                if (value > MaxTerrainSizePowerFactor) value = MaxTerrainSizePowerFactor;
                terrainSizePowerFactor = value;
            }
        }

        private int IterationPowerFactor
        {
            get { return iterationPowerFactor; }
            set
            {
                if (value < 0) value = 0;
                if (value > 7) value = 7;
                iterationPowerFactor = value;
            }
        }

        private float FilterHeightBandStrength
        {
            get { return filterHeightBandStrength; }
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                filterHeightBandStrength = value;
            }
        }

        private float TerrainHeightScale
        {
            get { return terrainHeightScale; }
            set
            {
                if (value < 1) value = 1;
                terrainHeightScale = value;
            }
        }
        #endregion Fault formation terrain generator properties

        /// <summary>
        /// Creates and Initializes Pipeline, UI, Camera, and a terrain model
        /// </summary>
        /// <returns></returns>
        public override void Start()
        {
            CreateUI();

            cameraStartPosition = CameraEntity.Transform.Position;
            UpdateCamera();

            var maxTerrainSize = (int)Math.Pow(2, MaxTerrainSizePowerFactor);
            var maxVerticesCount = maxTerrainSize * maxTerrainSize;
            var maxIndicesCount = 2 * maxTerrainSize * maxTerrainSize; // each index appear on average twice since the mesh is rendered as triangle strips
            CreateTerrainModelEntity(maxVerticesCount, maxIndicesCount);

            GenerateTerrain();

            Script.AddTask(UpdateInput);
        }

        /// <summary>
        /// Creates UI showing parameters of Fault formation algorithm
        /// </summary>
        private void CreateUI()
        {
            var virtualResolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height, 1);

            loadingModal = new ModalElement { Visibility = Visibility.Collapsed };

            loadingTextBlock = new TextBlock { Font = Font, Text = "Loading a model...", Visibility = Visibility.Collapsed, TextColor = Color.White, TextSize = 22 };

            loadingTextBlock.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 1f));
            loadingTextBlock.SetCanvasRelativePosition(new Vector3(0.5f, 0.5f, 0));

            // Parameters Grid
            var parametersGrid = new Grid();
            parametersGrid.LayerDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.ColumnDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.ColumnDefinitions.Add(new StripDefinition(StripType.Star));
            parametersGrid.ColumnDefinitions.Add(new StripDefinition(StripType.Fixed, 30));
            parametersGrid.ColumnDefinitions.Add(new StripDefinition(StripType.Fixed, 30));

            // Terrain Size
            var terrainSizeText = new TextBlock
            {
                Font = Font,
                Text = "" + (int)Math.Pow(2, terrainSizePowerFactor),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                MinimumWidth = 60
            };
            terrainSizeText.SetGridRow(0);
            terrainSizeText.SetGridColumn(1);

            var terrainSizeIncButton = new Button { Content = new TextBlock { Text = "+", Font = Font, TextAlignment = TextAlignment.Center } };
            terrainSizeIncButton.SetGridRow(0);
            terrainSizeIncButton.SetGridColumn(3);

            var terrainSizeDecButton = new Button { Content = new TextBlock { Text = "-", Font = Font, TextAlignment = TextAlignment.Center } };
            terrainSizeDecButton.SetGridRow(0);
            terrainSizeDecButton.SetGridColumn(2);

            terrainSizeIncButton.Click += (s, e) =>
            {
                TerrainSizePowerFactor++;
                terrainSizeText.Text = "" + (int)Math.Pow(2, TerrainSizePowerFactor);
            };

            terrainSizeDecButton.Click += (s, e) =>
            {
                TerrainSizePowerFactor--;
                terrainSizeText.Text = "" + (int)Math.Pow(2, TerrainSizePowerFactor);
            };

            var terrainSizeDescription = new TextBlock
            {
                Font = Font,
                Text = "Terrain Size:",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            terrainSizeDescription.SetGridRow(0);
            terrainSizeDescription.SetGridColumn(0);

            parametersGrid.Children.Add(terrainSizeDescription);
            parametersGrid.Children.Add(terrainSizeText);
            parametersGrid.Children.Add(terrainSizeDecButton);
            parametersGrid.Children.Add(terrainSizeIncButton);

            // Iteration
            var iterationText = new TextBlock
            {
                Font = Font,
                Text = "" + (int)Math.Pow(2, IterationPowerFactor),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            iterationText.SetGridRow(1);
            iterationText.SetGridColumn(1);

            var iterationIncButton = new Button { Content = new TextBlock { Text = "+", Font = Font, TextAlignment = TextAlignment.Center } };
            iterationIncButton.SetGridRow(1);
            iterationIncButton.SetGridColumn(3);

            var iterationDecButton = new Button { Content = new TextBlock { Text = "-", Font = Font, TextAlignment = TextAlignment.Center } };
            iterationDecButton.SetGridRow(1);
            iterationDecButton.SetGridColumn(2);

            iterationIncButton.Click += (s, e) =>
            {
                IterationPowerFactor++;
                iterationText.Text = "" + (int)Math.Pow(2, IterationPowerFactor);
            };

            iterationDecButton.Click += (s, e) =>
            {
                IterationPowerFactor--;
                iterationText.Text = "" + (int)Math.Pow(2, IterationPowerFactor);
            };

            var iterationDescription = new TextBlock
            {
                Font = Font,
                Text = "Iteration:",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            iterationDescription.SetGridRow(1);
            iterationDescription.SetGridColumn(0);

            parametersGrid.Children.Add(iterationDescription);
            parametersGrid.Children.Add(iterationText);
            parametersGrid.Children.Add(iterationDecButton);
            parametersGrid.Children.Add(iterationIncButton);

            // Filter Intensity
            var filterIntensityText = new TextBlock
            {
                Font = Font,
                Text = "" + FilterHeightBandStrength,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            filterIntensityText.SetGridRow(2);
            filterIntensityText.SetGridColumn(1);

            var filterIncButton = new Button { Content = new TextBlock { Text = "+", Font = Font, TextAlignment = TextAlignment.Center } };
            filterIncButton.SetGridRow(2);
            filterIncButton.SetGridColumn(3);

            var filterDecButton = new Button { Content = new TextBlock { Text = "-", Font = Font, TextAlignment = TextAlignment.Center } };
            filterDecButton.SetGridRow(2);
            filterDecButton.SetGridColumn(2);

            filterIncButton.Click += (s, e) =>
            {
                FilterHeightBandStrength += 0.1f;
                filterIntensityText.Text = "" + FilterHeightBandStrength;
            };

            filterDecButton.Click += (s, e) =>
            {
                FilterHeightBandStrength -= 0.1f;
                filterIntensityText.Text = "" + FilterHeightBandStrength;
            };

            var filterIntensityDescription = new TextBlock
            {
                Font = Font,
                Text = "Filter Intensity:",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            filterIntensityDescription.SetGridRow(2);
            filterIntensityDescription.SetGridColumn(0);

            parametersGrid.Children.Add(filterIntensityDescription);
            parametersGrid.Children.Add(filterIntensityText);
            parametersGrid.Children.Add(filterDecButton);
            parametersGrid.Children.Add(filterIncButton);

            // Height Scale
            var heightScaleText = new TextBlock
            {
                Font = Font,
                Text = "" + TerrainHeightScale,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            heightScaleText.SetGridRow(3);
            heightScaleText.SetGridColumn(1);

            var heightScaleIncButton = new Button { Content = new TextBlock { Text = "+", Font = Font, TextAlignment = TextAlignment.Center } };
            heightScaleIncButton.SetGridRow(3);
            heightScaleIncButton.SetGridColumn(3);

            var heightScaleDecButton = new Button { Content = new TextBlock { Text = "-", Font = Font, TextAlignment = TextAlignment.Center } };
            heightScaleDecButton.SetGridRow(3);
            heightScaleDecButton.SetGridColumn(2);

            heightScaleIncButton.Click += (s, e) =>
            {
                TerrainHeightScale++;
                heightScaleText.Text = "" + TerrainHeightScale;
            };

            heightScaleDecButton.Click += (s, e) =>
            {
                TerrainHeightScale--;
                heightScaleText.Text = "" + TerrainHeightScale;
            };

            var heightScaleDescription = new TextBlock
            {
                Font = Font,
                Text = "Height Scale:",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            heightScaleDescription.SetGridRow(3);
            heightScaleDescription.SetGridColumn(0);

            parametersGrid.Children.Add(heightScaleDescription);
            parametersGrid.Children.Add(heightScaleText);
            parametersGrid.Children.Add(heightScaleDecButton);
            parametersGrid.Children.Add(heightScaleIncButton);

            // Zoom
            var zoomFactorIncButton = new Button { Content = new TextBlock { Text = "+", Font = Font, TextAlignment = TextAlignment.Center } };
            zoomFactorIncButton.SetGridRow(4);
            zoomFactorIncButton.SetGridColumn(3);

            var zoomFactorDecButton = new Button { Content = new TextBlock { Text = "-", Font = Font, TextAlignment = TextAlignment.Center } };
            zoomFactorDecButton.SetGridRow(4);
            zoomFactorDecButton.SetGridColumn(2);

            zoomFactorIncButton.Click += (s, e) =>
            {
                zoomFactor -= 0.1f;
                UpdateCamera();
            };

            zoomFactorDecButton.Click += (s, e) =>
            {
                zoomFactor += 0.1f;
                UpdateCamera();
            };

            var zoomDescription = new TextBlock
            {
                Font = Font,
                Text = "Zoom",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            zoomDescription.SetGridRow(4);
            zoomDescription.SetGridColumn(0);

            parametersGrid.Children.Add(zoomDescription);
            parametersGrid.Children.Add(zoomFactorDecButton);
            parametersGrid.Children.Add(zoomFactorIncButton);

            // Light toggle button
            var lightToggleButton = new Button { Content = new TextBlock { Text = "Directional Light Off", Font = Font }, HorizontalAlignment = HorizontalAlignment.Left };

            lightToggleButton.Click += (s, e) =>
            {
                var ligh0 = DirectionalLight0.Get<LightComponent>();
                var ligh1 = DirectionalLight1.Get<LightComponent>();

                ligh0.Enabled = !ligh0.Enabled;
                ligh1.Enabled = !ligh1.Enabled;
                ((TextBlock)lightToggleButton.Content).Text = ligh0.Enabled ? "Directional Light Off" : "Directional Light On";
            };

            // Re-create terrain
            var reCreateTerrainButton = new Button { Content = new TextBlock { Text = "Recreate terrain", Font = Font }, HorizontalAlignment = HorizontalAlignment.Left };

            reCreateTerrainButton.Click += (s, e) => GenerateTerrain();

            var descriptionCanvas = new StackPanel
            {
                Children =
                {
                    new TextBlock { Font = Font, Text = "Fault formation parameters", TextSize = 19},
                    parametersGrid,
                    lightToggleButton,
                    reCreateTerrainButton
                }
            };

            var activeButton = new Button
            {
                Content = new TextBlock { Text = "Description Off", Font = Font },
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var isDescriptionOn = true;

            activeButton.Click += (s, e) =>
            {
                isDescriptionOn = !isDescriptionOn;
                ((TextBlock)activeButton.Content).Text = isDescriptionOn ? "Description Off" : "Description On";
                descriptionCanvas.Visibility = isDescriptionOn ? Visibility.Visible : Visibility.Collapsed;
            };

            var buttonDescription = new StackPanel { Orientation = Orientation.Vertical, Children = { activeButton, descriptionCanvas } };

            var uiComponent = UIEntity.Get<UIComponent>();
            uiComponent.RootElement = new Canvas { Children = { buttonDescription, loadingModal, loadingTextBlock } };
            uiComponent.Resolution = virtualResolution;
        }

        private void UpdateCamera()
        {
            CameraEntity.Transform.Position = Vector3.Transform(cameraStartPosition * zoomFactor, CameraEntity.Transform.Rotation);
        }

        /// <summary>
        /// Creates an Entity that contains our dynamic Vertex and Index buffers.
        /// This Entity will be rendered by the model renderer.
        /// </summary>
        /// <param name="verticesCount"></param>
        /// <param name="indicesCount"></param>
        private void CreateTerrainModelEntity(int verticesCount, int indicesCount)
        {
            // Compute sizes
            var vertexDeclaration = VertexNormalTexture.VertexDeclaration;
            var vertexBufferSize = verticesCount * vertexDeclaration.CalculateSize();
            var indexBufferSize = indicesCount * sizeof(short);

            // Create Vertex and Index buffers
            terrainVertexBuffer = Buffer.Vertex.New(GraphicsDevice, vertexBufferSize, GraphicsResourceUsage.Dynamic);
            terrainIndexBuffer = Buffer.New(GraphicsDevice, indexBufferSize, BufferFlags.IndexBuffer, GraphicsResourceUsage.Dynamic);

            // Prepare mesh and entity
            var meshDraw = new MeshDraw
            {
                PrimitiveType = PrimitiveType.TriangleStrip,
                VertexBuffers = new[] { new VertexBufferBinding(terrainVertexBuffer, vertexDeclaration, verticesCount) },
                IndexBuffer = new IndexBufferBinding(terrainIndexBuffer, false, indicesCount),
            };

            // Load the material and set parameters
            TerrainMaterial.Parameters.Set(VertexTextureTerrainKeys.MeshTexture0, WaterTexture);
            TerrainMaterial.Parameters.Set(VertexTextureTerrainKeys.MeshTexture1, GrassTexture);
            TerrainMaterial.Parameters.Set(VertexTextureTerrainKeys.MeshTexture2, MountainTexture);

            // Set up material regions
            TerrainMaterial.Parameters.Set(VertexTextureTerrainKeys.MinimumHeight0, -10);
            TerrainMaterial.Parameters.Set(VertexTextureTerrainKeys.OptimalHeight0, 40);
            TerrainMaterial.Parameters.Set(VertexTextureTerrainKeys.MaximumHeight0, 70);

            TerrainMaterial.Parameters.Set(VertexTextureTerrainKeys.MinimumHeight1, 60);
            TerrainMaterial.Parameters.Set(VertexTextureTerrainKeys.OptimalHeight1, 80);
            TerrainMaterial.Parameters.Set(VertexTextureTerrainKeys.MaximumHeight1, 90);

            TerrainMaterial.Parameters.Set(VertexTextureTerrainKeys.MinimumHeight2, 85);
            TerrainMaterial.Parameters.Set(VertexTextureTerrainKeys.OptimalHeight2, 95);
            TerrainMaterial.Parameters.Set(VertexTextureTerrainKeys.MaximumHeight2, 125);

            terrainMesh = new Mesh { Draw = meshDraw, MaterialIndex = 0 };
            TerrainEntity.GetOrCreate<ModelComponent>().Model = new Model { terrainMesh, TerrainMaterial };
        }

        /// <summary>
        /// Updates touch input for controlling the camera by polling to check pointer events
        /// </summary>
        /// <returns></returns>
        public async Task UpdateInput()
        {
            var rotY = 0f;
            var rotX = MathUtil.Pi / 5f;

            while (Game.IsRunning)
            {
                await Script.NextFrame();

                if (Input.PointerEvents.Count > 0)
                {
                    var sumDelta = MathUtil.Pi * Input.PointerEvents.Aggregate(Vector2.Zero, (current, pointerEvent) => current + pointerEvent.DeltaPosition);
                    rotY += 1.5f * sumDelta.X;
                    rotX += 0.75f * sumDelta.Y;
                }
                // Rotate the terrain
                rotY += 0.25f * (float)Game.UpdateTime.Elapsed.TotalSeconds;

                Entity.Transform.Rotation = Quaternion.RotationY(rotY) * Quaternion.RotationX(rotX); // rotate the whole world
            }
        }

        /// <summary>
        /// Generates new terrain and initializes it in vertex and index buffer asynchronously.
        /// </summary>
        /// <returns></returns>
        private void GenerateTerrain()
        {
            Script.AddTask(async () =>
            {
                // Show loading modal and text
                loadingModal.Visibility = Visibility.Visible;
                loadingTextBlock.Visibility = Visibility.Visible;

                var heightMap = await Task.Run(() =>
                {
                    return HeightMap.GenerateFaultFormation((int)Math.Pow(2, terrainSizePowerFactor),
                        (int)Math.Pow(2, iterationPowerFactor), 0, 256, TerrainHeightScale, filterHeightBandStrength);
                });

                InitializeBuffersFromTerrain(heightMap);
                TerrainEntity.Transform.Position = new Vector3(0, -heightMap.MedianHeight, 0);

                // Dismiss loading modal and text
                loadingModal.Visibility = Visibility.Collapsed;
                loadingTextBlock.Visibility = Visibility.Collapsed;
            });
        }

        /// <summary>
        /// Initializes Vertex and Index buffers with a given height map
        /// </summary>
        /// <param name="heightMap"></param>
        private void InitializeBuffersFromTerrain(HeightMap heightMap)
        {
            var commandList = Game.GraphicsContext.CommandList;

            // Set data in VertexBuffer
            var mappedSubResource = commandList.MapSubresource(terrainVertexBuffer, 0, MapMode.WriteDiscard);
            SetVertexDataFromHeightMap(heightMap, mappedSubResource.DataBox.DataPointer);
            commandList.UnmapSubresource(mappedSubResource);

            // Set data in IndexBuffer
            mappedSubResource = commandList.MapSubresource(terrainIndexBuffer, 0, MapMode.WriteDiscard);
            var elementCount = SetIndexDataForTerrain(heightMap.Size, mappedSubResource.DataBox.DataPointer);
            commandList.UnmapSubresource(mappedSubResource);

            terrainMesh.Draw.DrawCount = elementCount;
        }

        /// <summary>
        /// Initializes Index buffer data from the given size of terrain for a square Triangle Strip (Brute force) rendering
        /// </summary>
        /// <param name="size"></param>
        /// <param name="indexBuffer"></param>
        /// <returns></returns>
        private static unsafe int SetIndexDataForTerrain(int size, IntPtr indexBuffer)
        {
            var ib = (short*)indexBuffer;
            var currentIndex = 0;

            for (var iZ = 0; iZ < size - 1; ++iZ)
            {
                for (var iX = 0; iX < size; ++iX)
                {
                    ib[currentIndex++] = (short)(size * iZ + iX);
                    ib[currentIndex++] = (short)(size * (iZ + 1) + iX);
                }

                ib[currentIndex] = ib[currentIndex - 1];
                ++currentIndex;
                ib[currentIndex++] = (short)(size * (iZ + 1));
            }
            return currentIndex - 1;
        }

        /// <summary>
        /// Initializes Vertex buffer data by a given height map
        /// </summary>
        /// <param name="heightMap"></param>
        /// <param name="vertexBuffer"></param>
        private static unsafe void SetVertexDataFromHeightMap(HeightMap heightMap, IntPtr vertexBuffer)
        {
            var vb = (VertexNormalTexture*)vertexBuffer;

            var halfSize = heightMap.Size * 0.5f;

            for (var iZ = 0; iZ < heightMap.Size; ++iZ)
                for (var iX = 0; iX < heightMap.Size; ++iX)
                {
                    vb[iZ * heightMap.Size + iX] = new VertexNormalTexture
                    {
                        Position = new Vector4(iX - halfSize, heightMap.GetHeight(iX, iZ), -iZ + halfSize, 1),
                        Normal = GetNormalVector(heightMap, iX, iZ),
                        TextureCoordinate = new Vector2((float)iX / heightMap.Size, (float)iZ / heightMap.Size)
                    };
                }
        }

        /// <summary>
        /// Gets a normal vector for a given x, z coordinate and the corresponding heightmap
        /// </summary>
        /// <param name="heightMap"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static Vector4 GetNormalVector(HeightMap heightMap, int x, int z)
        {
            var currentP = new Vector3(x, heightMap.GetHeight(x, z), z);
            Vector3 p1;
            Vector3 p2;

            if (x == heightMap.Size - 1 && z == heightMap.Size - 1) // Bottom right pixel
            {
                p1 = new Vector3(x, heightMap.GetHeight(x, z - 1), z - 1);
                p2 = new Vector3(x - 1, heightMap.GetHeight(x - 1, z), z);
            }
            else if (x == heightMap.Size - 1) // Right border
            {
                p1 = new Vector3(x - 1, heightMap.GetHeight(x - 1, z), z);
                p2 = new Vector3(x, heightMap.GetHeight(x, z + 1), z + 1);
            }
            else if (z == heightMap.Size - 1) // Bottom border
            {
                p1 = new Vector3(x + 1, heightMap.GetHeight(x + 1, z), z);
                p2 = new Vector3(x, heightMap.GetHeight(x, z - 1), z - 1);
            }
            else // The rest of pixels
            {
                p1 = new Vector3(x, heightMap.GetHeight(x, z + 1), z + 1);
                p2 = new Vector3(x + 1, heightMap.GetHeight(x + 1, z), z);
            }
            return new Vector4(Vector3.Normalize(Vector3.Cross(p1 - currentP, p2 - currentP)), 1);
        }
    }

    /// <summary>
    /// Vertex attribute uses in VertexTextureTerrain shader
    /// </summary>
    struct VertexNormalTexture
    {
        /// <summary>
        /// Gets a declaration of Vertex attribute which consists of Position::Vector4, Normal::Vector4, TextureCoordinate::Vector2
        /// </summary>
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElement.Position<Vector4>(),
            VertexElement.Normal<Vector4>(), VertexElement.TextureCoordinate<Vector2>());

        /// <summary>
        /// Gets or sets a Position of a vertex
        /// </summary>
        public Vector4 Position;

        /// <summary>
        /// Gets or sets a Normal vector of a vertex 
        /// </summary>
        public Vector4 Normal;

        /// <summary>
        /// Gets or sets a texture coordinate of a vertex
        /// </summary>
        public Vector2 TextureCoordinate;
    }
}

