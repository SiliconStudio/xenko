using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace GameMenu
{
    public class MainScript : UISceneBase
    {
        private const string DefaultName = "John Doe";
        private const int MaximumStar = 3;

        private static readonly List<string> ShipNameList = new List<string>
        {
            "red_ship", "green_ship", "blue_ship", "blue_ship", "yellow_ship", "yellow_ship", "cyan_ship"
        };

        public SpriteFont WesternFont;
        public SpriteSheet MainSceneImages;
        public SpriteFont JapaneseFont;

        private readonly List<SpaceShip> shipList = new List<SpaceShip>();
        private int money = 30;
        private int bonus = 30;

        private int lifeStatus;
        private int powerStatus;
        private int controlStatus;
        private int speedStatus;

        private ModalElement shipSelectPopup; // Root of SpaceShip select popup
        private ModalElement welcomePopup; // Root of welcome popup

        private ISpriteProvider popupWindowImage; // Window frame for popup which is shared between SpaceShip select and welcome popups
        private TextBlock nameTextBlock; // Name of the character
        private ImageElement currentShipImage; // Current SpaceShip of the character
        private int activeShipIndex;

        private readonly List<Sprite> starSprites = new List<Sprite>();
        private readonly List<Sprite> borderStarImages = new List<Sprite>();

        // Life gauge
        private RectangleF gaugeBarRegion;
        private Grid lifeBarGrid;
        private Sprite lifeBarGaugeImage;

        private TextBlock moneyCounter;
        private int Money
        {
            set
            {
                money = value;
                moneyCounter.Text = CreateMoneyCountText();
            }
            get { return money; }
        }

        private TextBlock bonusCounter;
        private int Bonus
        {
            set
            {
                bonus = value;
                bonusCounter.Text = CreateBonusCountText();
            }
            get { return bonus; }
        }

        private TextBlock lifeCounter;
        private int LifeStatus
        {
            set
            {
                lifeStatus = value;
                lifeCounter.Text = CreateLifeCountText();
            }
            get { return lifeStatus; }
        }


        private readonly ImageElement powerStatusStar = new ImageElement();
        private int PowerStatus
        {
            set
            {
                if (value > MaximumStar) return;
                powerStatus = value;
                powerStatusStar.Source = (SpriteFromTexture)starSprites[powerStatus];
                shipList[activeShipIndex].Power = powerStatus;
            }
            get { return powerStatus; }
        }

        private readonly ImageElement controlStatusStar = new ImageElement();
        private int ControlStatus
        {
            set
            {
                if (value > MaximumStar) return;
                controlStatus = value;
                controlStatusStar.Source = (SpriteFromTexture)starSprites[controlStatus];
                shipList[activeShipIndex].Control = controlStatus;
            }
            get { return controlStatus; }
        }

        private readonly ImageElement speedStatusStar = new ImageElement();

        private int SpeedStatus
        {
            set
            {
                if (value > MaximumStar) return;
                speedStatus = value;
                speedStatusStar.Source = (SpriteFromTexture)starSprites[speedStatus];
                shipList[activeShipIndex].Speed = speedStatus;
            }
            get { return speedStatus; }
        }

        public override void Start()
        {
            base.Start();
            ShowWelcomePopup();
        }

        protected override void LoadScene()
        {
            popupWindowImage = SpriteFromSheet.Create(MainSceneImages, "popup_window");

            // Preload stars
            starSprites.Add(MainSceneImages["star0"]);
            starSprites.Add(MainSceneImages["star1"]);
            starSprites.Add(MainSceneImages["star2"]);
            starSprites.Add(MainSceneImages["star3"]);
            borderStarImages.Add(MainSceneImages["bstar0"]);
            borderStarImages.Add(MainSceneImages["bstar1"]);
            borderStarImages.Add(MainSceneImages["bstar2"]);
            borderStarImages.Add(MainSceneImages["bstar3"]);

            // Create space ships
            var random = new Random();
            for (var i = 0; i < ShipNameList.Count; i++)
            {
                shipList.Add(new SpaceShip
                {
                    Name = ShipNameList[i],
                    Power = random.Next(4),
                    Control = random.Next(4),
                    Speed = random.Next(4),
                    IsLocked = (i % 3) == 2,
                });
            }

            var mainStackPanel = CreateMainScene();
            CreateWelcomePopup();
            CreateShipSelectionPopup();

            // Create the background
            var background = new ImageElement { Source = SpriteFromSheet.Create(MainSceneImages, "background_uiimage"), StretchType = StretchType.Fill };
            background.SetPanelZIndex(-1);

            // Overlay pop-ups and the main screen
            var overlay = new UniformGrid();
            overlay.Children.Add(background);
            overlay.Children.Add(mainStackPanel);
            overlay.Children.Add(welcomePopup);
            overlay.Children.Add(shipSelectPopup);

            // Set the root element to the overall overlay
            var uiComponent = Entity.Get<UIComponent>();
            uiComponent.RootElement = overlay;

            Script.AddTask(FillLifeBar);
        }
        private async Task FillLifeBar()
        {
            var gaugePercentage = 0.15f;

            while (gaugePercentage < 1f)
            {
                await Script.NextFrame();

                gaugePercentage = Math.Min(1f, gaugePercentage + (float)Game.UpdateTime.Elapsed.TotalSeconds * 0.02f);

                var gaugeCurrentRegion = lifeBarGaugeImage.Region;
                gaugeCurrentRegion.Width = gaugePercentage * gaugeBarRegion.Width;
                lifeBarGaugeImage.Region = gaugeCurrentRegion;

                lifeBarGrid.ColumnDefinitions[1].SizeValue = gaugeCurrentRegion.Width / gaugeBarRegion.Width;
                lifeBarGrid.ColumnDefinitions[2].SizeValue = 1 - lifeBarGrid.ColumnDefinitions[1].SizeValue;
            }
        }

        private void CreateShipSelectionPopup()
        {
            // Create "Please select your SpaceShip" text
            var pleaseSelectText = new TextBlock
            {
                Font = WesternFont,
                TextSize = 48,
                TextColor = Color.White,
                Text = "Please select your ship",
                TextAlignment = TextAlignment.Center,
                WrapText = true
            };

            // Layout elements in vertical StackPanel
            var contentStackpanel = new StackPanel { Orientation = Orientation.Vertical };

            // Create and Add SpaceShip to the stack layout
            foreach (var ship in shipList)
                contentStackpanel.Children.Add(CreateShipButtonElement(ship));

            // Uncomment those lines to have an example of stack panel item virtualization
            //var shipInitialCount = shipList.Count;
            //contentStackpanel.ItemVirtualizationEnabled = true;
            //for (int i = 0; i < 200; i++)
            //{
            //    shipList.Add(new SpaceShip { Name = shipList[i % shipInitialCount].Name });
            //    contentStackpanel.Children.Add(CreateShipButtonElement(shipList[shipList.Count - 1]));
            //}

            UpdateShipStatus();

            var contentScrollView = new ScrollViewer
            {
                MaximumHeight = 425,
                Content = contentStackpanel,
                ScrollMode = ScrollingMode.Vertical,
                Margin = new Thickness(12, 10, 7, 10),
                Padding = new Thickness(0, 0, 6, 0),
                ScrollBarColor = Color.Orange
            };

            var scrollViewerDecorator = new ContentDecorator { BackgroundImage = SpriteFromSheet.Create(MainSceneImages, "scroll_background"), Content = contentScrollView, };
            scrollViewerDecorator.SetGridRow(2);

            var layoutGrid = new Grid();
            layoutGrid.ColumnDefinitions.Add(new StripDefinition());
            layoutGrid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            layoutGrid.RowDefinitions.Add(new StripDefinition(StripType.Fixed, 10)); // white space
            layoutGrid.RowDefinitions.Add(new StripDefinition(StripType.Star));
            layoutGrid.LayerDefinitions.Add(new StripDefinition());
            layoutGrid.Children.Add(pleaseSelectText);
            layoutGrid.Children.Add(scrollViewerDecorator);

            var shipSelectPopupContent = new ContentDecorator
            {
                BackgroundImage = popupWindowImage,
                Content = layoutGrid,
                Padding = new Thickness(110, 120, 100, 140)
            };

            // Create SpaceShip selection popup
            shipSelectPopup = new ModalElement
            {
                Visibility = Visibility.Collapsed,
                Content = shipSelectPopupContent
            };

            shipSelectPopup.SetPanelZIndex(1);
        }

        private void UpdateShipStatus()
        {
            foreach (var ship in shipList)
            {
                ship.PowerImageElement.Source = (SpriteFromTexture)borderStarImages[ship.Power];
                ship.ControlImageElement.Source = (SpriteFromTexture)borderStarImages[ship.Control];
                ship.SpeedImageElement.Source = (SpriteFromTexture)borderStarImages[ship.Speed];
            }
        }

        private UniformGrid CreateShipButtonElement(SpaceShip spaceShip)
        {
            // Put the stat text block in a vertical uniform grid
            var statusTextGrid = new UniformGrid { Rows = 3, Margin = new Thickness(5f, -6f, 0, 0)};
            statusTextGrid.Children.Add(CreateShipStatusTextBlock("Power", 0));
            statusTextGrid.Children.Add(CreateShipStatusTextBlock("Control", 1));
            statusTextGrid.Children.Add(CreateShipStatusTextBlock("Speed", 2));

            // Put the stat stars in a vertical uniform grid
            spaceShip.PowerImageElement = CreateShipStatusStar(0);
            spaceShip.ControlImageElement = CreateShipStatusStar(1);
            spaceShip.SpeedImageElement = CreateShipStatusStar(2);

            var starGrid = new UniformGrid { Rows = 3 };
            starGrid.Children.Add(spaceShip.PowerImageElement);
            starGrid.Children.Add(spaceShip.ControlImageElement);
            starGrid.Children.Add(spaceShip.SpeedImageElement);
            starGrid.SetGridColumn(2);

            // Ship image
            var shipSprite = SpriteFromSheet.Create(MainSceneImages, spaceShip.Name);
            var shipImageElement = new ImageElement { Source = shipSprite };
            shipImageElement.SetGridColumn(4);

            // Create the horizontal grid with two blank stretchable columns and add the text blocks, the starts and the ship image
            var shipContent = new Grid();
            shipContent.ColumnDefinitions.Add(new StripDefinition(StripType.Auto));
            shipContent.ColumnDefinitions.Add(new StripDefinition(StripType.Star));
            shipContent.ColumnDefinitions.Add(new StripDefinition(StripType.Auto));
            shipContent.ColumnDefinitions.Add(new StripDefinition(StripType.Star));
            shipContent.ColumnDefinitions.Add(new StripDefinition(StripType.Auto));
            shipContent.RowDefinitions.Add(new StripDefinition());
            shipContent.LayerDefinitions.Add(new StripDefinition());

            shipContent.Children.Add(statusTextGrid);
            shipContent.Children.Add(starGrid);
            shipContent.Children.Add(shipImageElement);

            //
            var shipSelectFrameSprite = SpriteFromSheet.Create(MainSceneImages, "weapon_select_frame");

            var shipButton = new Button
            {
                Name = spaceShip.Name,
                Content = shipContent,
                PressedImage = shipSelectFrameSprite,
                NotPressedImage = shipSelectFrameSprite,
                MouseOverImage = shipSelectFrameSprite,
                Padding = new Thickness(60, 20, 20, 20)
            };

            shipButton.Click += delegate
            {
                currentShipImage.Source = shipSprite;

                activeShipIndex = shipList.FindIndex(w => w.Name == spaceShip.Name);

                PowerStatus = spaceShip.Power;
                ControlStatus = spaceShip.Control;
                SpeedStatus = spaceShip.Speed;

                CloseShipSelectPopup();
            };

            shipButton.IsEnabled = !spaceShip.IsLocked;
            shipButton.SetCanvasRelativeSize(new Vector3(1f, 1f, 1f));

            var buttonGrid = new UniformGrid { MaximumHeight = 100 };
            buttonGrid.Children.Add(shipButton);

            if (spaceShip.IsLocked)
            {
                var lockIconElement = new ImageElement { Source = SpriteFromSheet.Create(MainSceneImages, "lock_icon"), StretchType = StretchType.Fill, };
                lockIconElement.SetPanelZIndex(1);
                buttonGrid.Children.Add(lockIconElement);
            }

            return buttonGrid;
        }

        private UIElement CreateShipStatusTextBlock(string statusName, int elementIndex)
        {
            var textBlock = new TextBlock
            {
                TextSize = 19,
                Font = WesternFont,
                Text = statusName,
                TextColor = Color.Black,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            textBlock.SetGridRow(elementIndex);

            return textBlock;
        }

        private ImageElement CreateShipStatusStar(int elementIndex)
        {
            var starImage = new ImageElement { VerticalAlignment = VerticalAlignment.Center };
            starImage.SetGridRow(elementIndex);

            return starImage;
        }

        private void CreateWelcomePopup()
        {
            // Create welcome text
            var welcomeText = new TextBlock
            {
                Font = WesternFont,
                TextSize = 42,
                TextColor = Color.White,
                Text = "Welcome to xenko UI sample.\n" + "Please name your character",
                TextAlignment = TextAlignment.Center,
                WrapText = true,
                Margin = new Thickness(20, 0, 20, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Create Edit text
            var nameEditText = new EditText()
            {
                Font = WesternFont,
                TextSize = 32,
                TextColor = Color.White,
                Text = DefaultName,
                MaxLength = 15,
                TextAlignment = TextAlignment.Center,
                ActiveImage = SpriteFromSheet.Create(MainSceneImages, "tex_edit_activated_background"),
                InactiveImage = SpriteFromSheet.Create(MainSceneImages, "tex_edit_inactivated_background"),
                MouseOverImage = SpriteFromSheet.Create(MainSceneImages, "tex_edit_inactivated_background"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MinimumWidth = 340,
                Padding = new Thickness(30, 30, 30, 40), // Pad text (Content inside),
                Margin = new Thickness(0, 80, 0, 80), // Space around the edit
            };
            nameEditText.SetGridRow(1);

            // Create cancel and validate button
            var cancelButton = CreateTextButton("Cancel");
            cancelButton.SetGridColumn(1);

            cancelButton.Click += delegate
            {
                nameTextBlock.Text = DefaultName;
                welcomePopup.Visibility = Visibility.Collapsed;
            };

            var validateButton = CreateTextButton("Validate");
            validateButton.SetGridColumn(3);

            validateButton.Click += delegate
            {
                nameTextBlock.Text = nameEditText.Text.Trim();
                welcomePopup.Visibility = Visibility.Collapsed;
            };

            // Put cancel and validate buttons in stack panel (Left-right orientation placement)
            var buttonPanel = new Grid();
            buttonPanel.ColumnDefinitions.Add(new StripDefinition(StripType.Star));
            buttonPanel.ColumnDefinitions.Add(new StripDefinition(StripType.Auto));
            buttonPanel.ColumnDefinitions.Add(new StripDefinition(StripType.Star));
            buttonPanel.ColumnDefinitions.Add(new StripDefinition(StripType.Auto));
            buttonPanel.ColumnDefinitions.Add(new StripDefinition(StripType.Star));
            buttonPanel.RowDefinitions.Add(new StripDefinition());
            buttonPanel.LayerDefinitions.Add(new StripDefinition());

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(validateButton);
            buttonPanel.SetGridRow(2);

            // Create a stack panel to store items (Top-down orientation placement)
            var popupContentPanel = new Grid
            {
                MaximumWidth = 580,
                MaximumHeight = 900,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            popupContentPanel.ColumnDefinitions.Add(new StripDefinition());
            popupContentPanel.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            popupContentPanel.RowDefinitions.Add(new StripDefinition(StripType.Star));
            popupContentPanel.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            popupContentPanel.LayerDefinitions.Add(new StripDefinition());

            popupContentPanel.Children.Add(welcomeText);
            popupContentPanel.Children.Add(nameEditText);
            popupContentPanel.Children.Add(buttonPanel);

            var welcomePopupContent = new ContentDecorator
            {
                BackgroundImage = popupWindowImage,
                Content = popupContentPanel,
                Padding = new Thickness(85, 130, 85, 110)
            };
			
            welcomePopup = new ModalElement
            {
                Visibility = Visibility.Collapsed,
                Content = welcomePopupContent,
            };
            welcomePopup.SetPanelZIndex(1);
			
        }

        private UIElement CreateMainScreneTopBar()
        {
            // Create Life bar
            lifeBarGaugeImage = MainSceneImages["life_bar"];
            gaugeBarRegion = lifeBarGaugeImage.Region;

            var lifebarGauge = new ImageElement
            {
                Name = "LifeBarBackground",
                Source = (SpriteFromTexture)lifeBarGaugeImage,
                StretchType = StretchType.Fill,
            };
            lifebarGauge.SetGridColumn(1);

            lifeBarGrid = new Grid();
            lifeBarGrid.Children.Add(lifebarGauge);
            lifeBarGrid.ColumnDefinitions.Add(new StripDefinition(StripType.Fixed, 128));
            lifeBarGrid.ColumnDefinitions.Add(new StripDefinition(StripType.Star, 0));
            lifeBarGrid.ColumnDefinitions.Add(new StripDefinition(StripType.Star, 100));
            lifeBarGrid.ColumnDefinitions.Add(new StripDefinition(StripType.Fixed, 85));
            lifeBarGrid.RowDefinitions.Add(new StripDefinition());
            lifeBarGrid.LayerDefinitions.Add(new StripDefinition());
            lifeBarGrid.SetCanvasRelativePosition(new Vector3(0f, 0.185f, 0f));
            lifeBarGrid.SetCanvasRelativeSize(new Vector3(1f, 0.25f, 1f));
            lifeBarGrid.SetPanelZIndex(-1);

            var lifebarForeground = new ImageElement
            {
                Name = "LifeBarForeGround",
                Source = SpriteFromSheet.Create(MainSceneImages, "character_frame"),
                StretchType = StretchType.Fill,
            };
            lifebarForeground.SetGridColumnSpan(3);
            lifebarForeground.SetGridRowSpan(3);
            lifebarForeground.SetCanvasRelativeSize(new Vector3(1f, 1f, 1f));

            // Life count
            lifeCounter = new TextBlock
            {
                Text = CreateLifeCountText(),
                TextColor = Color.Gold,
                Font = WesternFont,
                TextSize = 32,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            lifeCounter.SetCanvasAbsolutePosition(new Vector3(128, float.NaN, float.NaN));
            lifeCounter.SetCanvasRelativePosition(new Vector3(float.NaN, 0.44f, 0f));
            lifeCounter.SetPanelZIndex(1);
            LifeStatus = 3;

            // Bonus items
            var bonusIcon = new ImageElement
            {
                Source = SpriteFromSheet.Create(MainSceneImages, "gold_icon"),
                Name = "bonus Icon",
                VerticalAlignment = VerticalAlignment.Center
            };
            bonusCounter = new TextBlock
            {
                Text = CreateBonusCountText(),
                TextColor = Color.White,
                TextSize = 27,
                Font = WesternFont,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            // Money 
            var moneyIcon = new ImageElement
            {
                Source = SpriteFromSheet.Create(MainSceneImages, "money_icon"),
                Name = "money Icon",
                Margin = new Thickness(20, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            moneyCounter = new TextBlock
            {
                Text = CreateMoneyCountText(),
                TextColor = Color.White,
                TextSize = 27,
                Font = WesternFont,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            // Stack panel containing the bonus and money counters
            var moneyBonusStackPanel = new StackPanel
            {
                Name = "MoneyBonusStackPanel",
                Orientation = Orientation.Horizontal,
            };
            moneyBonusStackPanel.Children.Add(bonusIcon);
            moneyBonusStackPanel.Children.Add(bonusCounter);
            moneyBonusStackPanel.Children.Add(moneyIcon);
            moneyBonusStackPanel.Children.Add(moneyCounter);
            moneyBonusStackPanel.SetCanvasRelativePosition(new Vector3(0.93f, 0.44f, 0f));
            moneyBonusStackPanel.SetCanvasRelativeSize(new Vector3(float.NaN, 0.4f, 1f));
            moneyBonusStackPanel.SetCanvasPinOrigin(new Vector3(1f, 0f, 0f));
            moneyBonusStackPanel.SetPanelZIndex(1);

            // the main grid of the top bar
            var mainLayer = new Canvas
            {
                VerticalAlignment = VerticalAlignment.Top,
                MaximumHeight = 150
            };

            mainLayer.Children.Add(lifeBarGrid);
            mainLayer.Children.Add(lifebarForeground);
            mainLayer.Children.Add(lifeCounter);
            mainLayer.Children.Add(moneyBonusStackPanel);

            return mainLayer;
        }

        private Button CreateTextButton(string text)
        {
            var buttonImage = SpriteFromSheet.Create(MainSceneImages, "button0");

            return new Button
            {
                NotPressedImage = buttonImage,
                PressedImage = buttonImage,
                MouseOverImage = buttonImage,
                Content = new TextBlock
                {
                    Font = WesternFont,
                    TextColor = Color.White,
                    Text = text,
                },
                Padding = new Thickness(90, 30, 25, 35),
            };
        }

        private UIElement CreateMainScene()
        {
            // the top life bar
            var topBar = CreateMainScreneTopBar();

            // Create Name label
            nameTextBlock = new TextBlock
            {
                Font = WesternFont,
                TextSize = 52,
                TextColor = Color.LightGreen,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(50, 0, 50, 0)
            };
            var nameLabel = new ContentDecorator
            {
                BackgroundImage = SpriteFromSheet.Create(MainSceneImages, "tex_edit_inactivated_background"),
                Content = nameTextBlock,
                Padding = new Thickness(20, 15, 20, 20),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            nameLabel.SetGridRow(1);

            // Create Character image
            var characterImage = new ImageElement
            {
                Name = "HeroImage",
                Source = SpriteFromSheet.Create(MainSceneImages, "character"),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            characterImage.SetGridRow(2);

            // Create Explanation TextBlock
            var explanationLabel = new ContentDecorator
            {
                BackgroundImage = SpriteFromSheet.Create(MainSceneImages, "description_frame"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Content = new TextBlock
                {
                    Font = JapaneseFont,
                    TextSize = 28,
                    TextColor = Color.White,
                    Text = "Pictogram-based alphabets are easily supported.\n日本語も簡単に入れることが出来ます。",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    WrapText = true
                },
                Padding = new Thickness(50, 30, 50, 40),
            };
            explanationLabel.SetGridRow(3);

            // The ship status panel
            var shipStatusPanel = CreateMainSceneShipStatusPanel();
            shipStatusPanel.SetGridRow(4);

            // Create quit button (Last element)
            var quitButton = CreateTextButton("Quit Sample");
            quitButton.HorizontalAlignment = HorizontalAlignment.Center;
            quitButton.VerticalAlignment = VerticalAlignment.Center;
            quitButton.Click += delegate { UIGame.Exit(); };
            quitButton.SetGridRow(5);

            // Put region together in the main grid
            var mainLayout = new Grid();
            mainLayout.ColumnDefinitions.Add(new StripDefinition());
            mainLayout.RowDefinitions.Add(new StripDefinition(StripType.Star, 15));
            mainLayout.RowDefinitions.Add(new StripDefinition(StripType.Star, 10));
            mainLayout.RowDefinitions.Add(new StripDefinition(StripType.Star, 20));
            mainLayout.RowDefinitions.Add(new StripDefinition(StripType.Star, 20));
            mainLayout.RowDefinitions.Add(new StripDefinition(StripType.Star, 25));
            mainLayout.RowDefinitions.Add(new StripDefinition(StripType.Star, 10));
            mainLayout.LayerDefinitions.Add(new StripDefinition());

            // set minimal and maximal size of rows
            mainLayout.RowDefinitions[0].MaximumSize = topBar.MaximumHeight;
            mainLayout.RowDefinitions[1].MinimumSize = 100;
            mainLayout.RowDefinitions[3].MinimumSize = 120;
            mainLayout.RowDefinitions[5].MinimumSize = 90;

            mainLayout.Children.Add(topBar);
            mainLayout.Children.Add(nameLabel);
            mainLayout.Children.Add(characterImage);
            mainLayout.Children.Add(explanationLabel);
            mainLayout.Children.Add(shipStatusPanel);
            mainLayout.Children.Add(quitButton);


            return mainLayout;
        }

        private UIElement CreateMainSceneShipStatusStars(string imageName, UIElement content, int rowIndex)
        {
            var item = new ContentDecorator
            {
                Content = content,
                BackgroundImage = SpriteFromSheet.Create(MainSceneImages, imageName),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            item.SetGridRow(rowIndex);

            return item;
        }

        private UIElement CreateMainSceneShipStatusPanel()
        {
            // Status star bars {Power, Life, Speed}
            var powerStatusDecorator = CreateMainSceneShipStatusStars("power", powerStatusStar, 0);
            var controlStatusDecorator = CreateMainSceneShipStatusStars("control", controlStatusStar, 1);
            var speedStatusDecorator = CreateMainSceneShipStatusStars("speed", speedStatusStar, 2);
            PowerStatus = shipList[activeShipIndex].Power;
            ControlStatus = shipList[activeShipIndex].Control;
            SpeedStatus = shipList[activeShipIndex].Speed;

            // Put the stats (Stars) in 3x1 uniform grid
            var statusPanel = new UniformGrid { Rows = 3 };
            statusPanel.Children.Add(powerStatusDecorator);
            statusPanel.Children.Add(controlStatusDecorator);
            statusPanel.Children.Add(speedStatusDecorator);
            statusPanel.SetGridColumn(1);

            // SpaceShip Button
            currentShipImage = new ImageElement { Source = SpriteFromSheet.Create(MainSceneImages, shipList[activeShipIndex].Name) };
            currentShipImage.SetGridRow(1);

            var shipImageSpacerGrid = new Grid { HorizontalAlignment = HorizontalAlignment.Center };
            shipImageSpacerGrid.Children.Add(currentShipImage);
            shipImageSpacerGrid.RowDefinitions.Add(new StripDefinition(StripType.Star, 2));
            shipImageSpacerGrid.RowDefinitions.Add(new StripDefinition(StripType.Star, 6));
            shipImageSpacerGrid.RowDefinitions.Add(new StripDefinition(StripType.Star, 2));
            shipImageSpacerGrid.ColumnDefinitions.Add(new StripDefinition());
            shipImageSpacerGrid.LayerDefinitions.Add(new StripDefinition());

            var shipButtonDesign = SpriteFromSheet.Create(MainSceneImages, "display_element");
            var currentShipButton = new Button
            {
                NotPressedImage = shipButtonDesign,
                PressedImage = shipButtonDesign,
                MouseOverImage = shipButtonDesign,
                Content = shipImageSpacerGrid,
                Padding = new Thickness(45, 20, 10, 25),
                VerticalAlignment = VerticalAlignment.Center
            };
            currentShipButton.Click += delegate
            {
                // Once click, update the SpaceShip status pop-up and show it.
                UpdateShipStatus();
                ShowShipSelectionPopup();
            };
            currentShipButton.SetGridColumn(3);

            // Status upgrade buttons
            var powerUpgradeButton = CreateIncreaseStatusButton("P", 0, 0, 2, 0, () => PowerStatus, () => PowerStatus++);
            var controlUpgradeButton = CreateIncreaseStatusButton("C", 0, 1, 2, 0, () => ControlStatus, () => ControlStatus++);
            var speedUpgradeButton = CreateIncreaseStatusButton("S", 1, 0, 2, 0, () => SpeedStatus, () => SpeedStatus++);
            var lifeUpgradeButton = CreateIncreaseStatusButton("L", 1, 1, 1, 1, () => 0, () => LifeStatus++);

            // Arrange the status up buttons in a 2x2 Uniform grid.
            var statusUpgradeGridPanel = new UniformGrid
            {
                Rows = 2,
                Columns = 2,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            statusUpgradeGridPanel.Children.Add(powerUpgradeButton);
            statusUpgradeGridPanel.Children.Add(controlUpgradeButton);
            statusUpgradeGridPanel.Children.Add(speedUpgradeButton);
            statusUpgradeGridPanel.Children.Add(lifeUpgradeButton);
            statusUpgradeGridPanel.SetGridColumn(5);

            // Put together bottom region in horizontal Stack panel, arranging it from left to right
            var mainPanel = new Grid();
            mainPanel.RowDefinitions.Add(new StripDefinition());
            mainPanel.ColumnDefinitions.Add(new StripDefinition(StripType.Fixed, 10)); // space
            mainPanel.ColumnDefinitions.Add(new StripDefinition(StripType.Star, 4));
            mainPanel.ColumnDefinitions.Add(new StripDefinition(StripType.Fixed, 25)); // space
            mainPanel.ColumnDefinitions.Add(new StripDefinition(StripType.Star, 4));
            mainPanel.ColumnDefinitions.Add(new StripDefinition(StripType.Fixed, 25)); // space
            mainPanel.ColumnDefinitions.Add(new StripDefinition(StripType.Star, 3));
            mainPanel.ColumnDefinitions.Add(new StripDefinition(StripType.Fixed, 10)); // space
            mainPanel.LayerDefinitions.Add(new StripDefinition());

            mainPanel.Children.Add(statusPanel);
            mainPanel.Children.Add(currentShipButton);
            mainPanel.Children.Add(statusUpgradeGridPanel);

            return mainPanel;
        }

        private Button CreateIncreaseStatusButton(string text, int rowIndex, int columnIndex, int moneyCost, int bonuscost, Func<int> getProperty, Action setProperty)
        {
            var button = new Button
            {
                NotPressedImage = SpriteFromSheet.Create(MainSceneImages, "small_display_element"),
                MouseOverImage = SpriteFromSheet.Create(MainSceneImages, "small_display_element"),
                PressedImage = SpriteFromSheet.Create(MainSceneImages, "small_display_element_pressed"),
                MinimumWidth = 80,
                Name = text,
                Content = new TextBlock
                {
                    Font = WesternFont,
                    TextColor = Color.Black,
                    Text = text,
                    TextSize = 54,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Padding = new Thickness(22, 11, 22, 15)
            };

            button.SetGridColumn(columnIndex);
            button.SetGridRow(rowIndex);

            button.Click += delegate
            {
                if (!CanPurchase(moneyCost, bonuscost) || getProperty() >= MaximumStar)
                    return;

                setProperty();
                PurchaseWithBonus(bonuscost);
                PurchaseWithMoney(moneyCost);
            };

            return button;
        }

        private bool CanPurchase(int requireMoney, int requireBonus)
        {
            return Money >= requireMoney && Bonus >= requireBonus;
        }

        private void PurchaseWithMoney(int requireMoney)
        {
            Money -= requireMoney;
        }

        private void PurchaseWithBonus(int requireBonus)
        {
            Bonus -= requireBonus;
        }

        public void ShowWelcomePopup()
        {
            welcomePopup.Visibility = Visibility.Visible;
        }

        private void ShowShipSelectionPopup()
        {
            shipSelectPopup.Visibility = Visibility.Visible;
        }

        private void CloseShipSelectPopup()
        {
            shipSelectPopup.Visibility = Visibility.Collapsed;
        }

        private string CreateMoneyCountText()
        {
            return money.ToString("D3");
        }

        private string CreateBonusCountText()
        {
            return bonus.ToString("D3");
        }

        private string CreateLifeCountText()
        {
            return "x" + lifeStatus;
        }

        private class SpaceShip
        {
            public string Name;
            public int Power;
            public int Control;
            public int Speed;
            public bool IsLocked;
            public ImageElement PowerImageElement;
            public ImageElement ControlImageElement;
            public ImageElement SpeedImageElement;
        }
    }
}
