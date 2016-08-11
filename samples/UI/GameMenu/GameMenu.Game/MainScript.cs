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
        public UILibrary UILibrary;

        private UIPage page;

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
            page = Entity.Get<UIComponent>().Page;

            nameTextBlock = page.RootElement.FindVisualChildOfType<TextBlock>("nameTextBlock");

            // FIXME: UI asset should support multiline text
            var explanationText = page.RootElement.FindVisualChildOfType<TextBlock>("explanationText");
            explanationText.Text = "Pictogram-based alphabets are easily supported.\n日本語も簡単に入れることが出来ます。";

            var quitButton = page.RootElement.FindVisualChildOfType<Button>("quitButton");
            quitButton.Click += delegate { UIGame.Exit(); };

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

            InitializeUpgradeButtons();
            InitializeWelcomePopup();
            CreateShipSelectionPopup();

            // Overlay pop-ups and the main screen
            var overlay = (UniformGrid) page.RootElement;
            overlay.Children.Add(welcomePopup);
            overlay.Children.Add(shipSelectPopup);

            //Script.AddTask(FillLifeBar);
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

        private bool CanPurchase(int requireMoney, int requireBonus)
        {
            return Money >= requireMoney && Bonus >= requireBonus;
        }

        private void CloseShipSelectPopup()
        {
            shipSelectPopup.Visibility = Visibility.Collapsed;
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

        private void InitializeUpgradeButtons()
        {
            var statusUpgradePanel = page.RootElement.FindVisualChildOfType<UniformGrid>("statusUpgradePanel");
            SetupStatusButton((ButtonBase) statusUpgradePanel.VisualChildren[0], 2, 0, () => PowerStatus, () => PowerStatus++);
            SetupStatusButton((ButtonBase) statusUpgradePanel.VisualChildren[1], 2, 0, () => ControlStatus, () => ControlStatus++);
            SetupStatusButton((ButtonBase) statusUpgradePanel.VisualChildren[2], 2, 0, () => SpeedStatus, () => SpeedStatus++);
            SetupStatusButton((ButtonBase) statusUpgradePanel.VisualChildren[3], 1, 1, () => 0, () => LifeStatus++);
        }

        private void InitializeWelcomePopup()
        {
            welcomePopup = UILibrary.UIElements["WelcomePopup"] as ModalElement;
            welcomePopup.SetPanelZIndex(1);

            // FIXME: UI asset should support multiline text
            var welcomeText = welcomePopup.FindVisualChildOfType<TextBlock>("welcomeText");
            welcomeText.Text = "Welcome to xenko UI sample.\nPlease name your character";
            
            var cancelButton = welcomePopup.FindVisualChildOfType<Button>("cancelButton");
            cancelButton.Click += delegate
            {
                nameTextBlock.Text = DefaultName;
                welcomePopup.Visibility = Visibility.Collapsed;
            };

            var nameEditText = welcomePopup.FindVisualChildOfType<EditText>("nameEditText");
            var validateButton = welcomePopup.FindVisualChildOfType<Button>("validateButton");
            validateButton.Click += delegate
            {
                nameTextBlock.Text = nameEditText.Text.Trim();
                welcomePopup.Visibility = Visibility.Collapsed;
            };
        }

        private void SetupStatusButton(ButtonBase button, int moneyCost, int bonuscost, Func<int> getProperty, Action setProperty)
        {
            button.Click += delegate
            {
                if (!CanPurchase(moneyCost, bonuscost) || getProperty() >= MaximumStar)
                    return;

                setProperty();
                PurchaseWithBonus(bonuscost);
                PurchaseWithMoney(moneyCost);
            };
        }

        private void PurchaseWithMoney(int requireMoney)
        {
            Money -= requireMoney;
        }

        private void PurchaseWithBonus(int requireBonus)
        {
            Bonus -= requireBonus;
        }

        private void ShowShipSelectionPopup()
        {
            shipSelectPopup.Visibility = Visibility.Visible;
        }

        public void ShowWelcomePopup()
        {
            welcomePopup.Visibility = Visibility.Visible;
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
