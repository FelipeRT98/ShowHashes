using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;
using System.Diagnostics;
using File = System.IO.File;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


namespace ShowHashes
{

    public class Config
    {
        public string C_languageCode { get; set; } = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        public int C_searchEngineValue { get; set; } = 0;
        public bool C_copyWithMethodInputIsChecked { get; set; } = true;
        public bool C_searchWithMethodInputIsChecked { get; set; } = false;
        public string C_copySeparatorInputText { get; set; } = ": ";
        public string C_searchSeparatorInputText { get; set; } = "";
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // CONFIG FILE
        public readonly string CONFIG_FILE_NAME = "ShowHashes-config.json";
        public readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        public string languageCode;
        public int searchEngineDropdownSelectedIndex;
        public bool copyWithMethodInputIsChecked;
        public bool searchWithMethodInputIsChecked;
        public string copySeparatorInputText;
        public string searchSeparatorInputText;





        public int hashRowQuantity;
        public readonly Dictionary<string, MethodInfo> HASHDICT = new()
        {
            { "CRC32", typeof(MainWindow).GetMethod("GetCRC32") ?? throw new InvalidOperationException("Method 'GetCRC32' not found") },
            { "MD5", typeof(MainWindow).GetMethod("GetMD5")  ?? throw new InvalidOperationException("Method 'GetCRC32' not found") },
            { "SHA1", typeof(MainWindow).GetMethod("GetSHA1")  ?? throw new InvalidOperationException("Method 'GetCRC32' not found") },
            { "SHA256", typeof(MainWindow).GetMethod("GetSHA256")  ?? throw new InvalidOperationException("Method 'GetCRC32' not found") },
        };





        public readonly string[] SEARCH_ENGINES =
        [
            "https://www.google.com/search?q=",
            "https://www.bing.com/search?q=",
            "https://duckduckgo.com/?q=",
            "https://yandex.com/search/?text="
        ];



        public readonly string CONFIG_ICON = "⚙";
        public readonly string RESET_ICON = "↺";
        public readonly string COPY_ICON = "📋";
        public readonly string SEARCH_ICON = "🔎";
        public readonly string MISSING_ICON = "❓";
        public readonly string NOTFOUND_ICON = "❌";
        public readonly string FILE_ICON = "📰";
        public readonly string DIRECTORY_ICON = "📂";
        public readonly string FOUND_ICON = "✔";




        public string _aboutText = "About";
        public string _languageText = "Language";




        public readonly char[] INVALID_CHARACTERS =
        [
            '"', '<', '>', '|','*', '?', '\0', (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9,
            (char)10, (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
            (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30, (char)31
        ];




        public Dictionary<SolidColorBrush, SolidColorBrush> customColorsHalfDict;

        public readonly SolidColorBrush BORDER_COLOR = new(Color.FromRgb(22, 27, 172));
        public readonly SolidColorBrush TEXTBOX_COLOR = new(Color.FromRgb(204, 251, 234));
        public readonly SolidColorBrush BUTTON_COLOR = new(Color.FromRgb(93, 177, 222));

        public readonly SolidColorBrush CUSTOM_RED = new(Color.FromRgb(187, 12, 9));
        public readonly SolidColorBrush CUSTOOM_ORANGE = new(Color.FromRgb(207, 107, 29));
        public readonly SolidColorBrush CUSTOM_GREEN = new(Color.FromRgb(65, 238, 157));

        public readonly SolidColorBrush CUSTOM_HALFRED = new(Color.FromArgb(128, 187, 12, 9));
        public readonly SolidColorBrush CUSTOM_HALFORANGE = new(Color.FromArgb(128, 207, 107, 29));
        public readonly SolidColorBrush CUSTOM_HALFGREEN = new(Color.FromArgb(128, 65, 238, 157));




        // TOP
        public Grid topGrid;

        public Button configButton;
        public ComboBox searchEngineDropdown;
        public Button copyAllButton;
        public Button searchAllButton;

        // CONFIG
        public Grid configGrid;

        public string? copyWithMethodDescriptionTranslation;
        public string? searchWithMethodDescriptionTranslation;
        public string? copySeparatorDescriptionTranslation;
        public string? searchSeparatorDescriptionTranslation;

        public TextBox copyWithMethodDescription;
        public CheckBox copyWithMethodInput;

        public TextBox searchWithMethodDescription;
        public CheckBox searchWithMethodInput;

        public TextBox copySeparatorDescription;
        public TextBox copySeparatorInput;

        public TextBox searchSeparatorDescription;
        public TextBox searchSeparatorInput;


        // FILE
        public Grid fileGrid;

        public TextBox fileTextBox;
        public TextBox fileStatusTextBox;
        public Button searchFileButton;
        public Button copyFileButton;

        // HASHES
        public Grid hashesGrid;

        public CheckBox[] hashCheckBoxArray;
        public Button[] hashBlackListButtonArray;
        public Button[] hashWhiteListButtonArray;
        public TextBox[] hashMethodTextBoxArray;
        public TextBox[] hashValueTextBoxArray;
        public TextBox[] hashStatusTextBoxArray;
        public Button[] copyHashValueButtonArray;
        public Button[] searchHashValueButtonArray;




        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Closing += MainWindow_Closing;

            Config config = new();

            try
            {
                if (File.Exists(CONFIG_FILE_NAME))
                {
                    string json = File.ReadAllText(CONFIG_FILE_NAME);
                    config = JsonSerializer.Deserialize<Config>(json, JSON_SERIALIZER_OPTIONS) ?? new Config();

                    config.C_searchEngineValue = (config.C_searchEngineValue >= SEARCH_ENGINES.Length) ? 0 : config.C_searchEngineValue;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex.Message, "⚠", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            languageCode = config.C_languageCode;
            searchEngineDropdownSelectedIndex = config.C_searchEngineValue;
            copyWithMethodInputIsChecked = config.C_copyWithMethodInputIsChecked;
            searchWithMethodInputIsChecked = config.C_searchWithMethodInputIsChecked;
            copySeparatorInputText = config.C_copySeparatorInputText;
            searchSeparatorInputText = config.C_searchSeparatorInputText;




            SetLanguage(languageCode);

            hashRowQuantity = HASHDICT.Keys.Count;

            customColorsHalfDict = new()
            {
                [CUSTOM_RED] = CUSTOM_HALFRED,
                [CUSTOOM_ORANGE] = CUSTOM_HALFORANGE,
                [CUSTOM_GREEN] = CUSTOM_HALFGREEN
            };




            content.ShowGridLines = false;
            for (int i = 0; i < 3; i++)
            {
                SetGridRowDefinition(content, "Auto");
            }
            SetGridColumnDefinition(content, "*");




            #region TOP


            topGrid = new() { ShowGridLines = false, };
            for (int i = 0; i < 2; i++)
            {
                SetGridRowDefinition(topGrid, "Auto");
                SetGridColumnDefinition(topGrid, "0.5*");
            }
            GridSet(topGrid, content.Children.Count, 0, content);
            topGrid.Margin = new Thickness(0, 0, 0, 12);

            configButton = CreateControlElement<Button>(CONFIG_ICON);
            searchEngineDropdown = CreateControlElement<ComboBox>("");
            copyAllButton = CreateControlElement<Button>(COPY_ICON);
            searchAllButton = CreateControlElement<Button>(SEARCH_ICON);

            configButton.Click += OpenConfig;
            searchEngineDropdown.SelectionChanged += SearchEngineDropdown_SelectionChanged;
            searchEngineDropdown.KeyDown += SearchEngineDropdown_KeyDown;
            copyAllButton.Click += CopyAllButton_Click;
            searchAllButton.Click += SearchAllButton_Click;

            GridSet(configButton, 0, 0, topGrid);
            GridSet(searchEngineDropdown, 0, 1, topGrid);
            GridSet(copyAllButton, 1, 0, topGrid);
            GridSet(searchAllButton, 1, 1, topGrid);



            searchEngineDropdown.HorizontalContentAlignment = HorizontalAlignment.Left;
            foreach (string url in SEARCH_ENGINES)
            {
                searchEngineDropdown.Items.Add(url);
            }
            searchEngineDropdown.SelectedIndex = searchEngineDropdownSelectedIndex;


            #endregion




            #region CONFIG

            configGrid = new() { ShowGridLines = false, };
            for (int i = 0; i < 4; i++)
            {
                SetGridRowDefinition(configGrid, "Auto");
            }
            SetGridColumnDefinition(configGrid, "0.9*");
            SetGridColumnDefinition(configGrid, "0.1*");

            configGrid.Margin = new Thickness(16, 16, 16, 12);




            copyWithMethodDescription = CreateControlElement<TextBox>("");
            copyWithMethodInput = CreateControlElement<CheckBox>("");
            searchWithMethodDescription = CreateControlElement<TextBox>("");
            searchWithMethodInput = CreateControlElement<CheckBox>("");
            copySeparatorDescription = CreateControlElement<TextBox>("");
            copySeparatorInput = CreateControlElement<TextBox>("");
            searchSeparatorDescription = CreateControlElement<TextBox>("");
            searchSeparatorInput = CreateControlElement<TextBox>("");




            GridSet(copyWithMethodDescription, 0, 0, configGrid);
            GridSet(copyWithMethodInput, 0, 1, configGrid);
            GridSet(searchWithMethodDescription, 1, 0, configGrid);
            GridSet(searchWithMethodInput, 1, 1, configGrid);
            GridSet(copySeparatorDescription, 2, 0, configGrid);
            GridSet(searchSeparatorDescription, 3, 0, configGrid);

            Button copySeparatorBG = CreateControlElement<Button>("");
            copySeparatorBG.Background = new SolidColorBrush(Color.FromRgb(190, 230, 253));
            copySeparatorBG.BorderThickness = new Thickness(0);
            copySeparatorBG.IsHitTestVisible = true;
            copySeparatorBG.Focusable = false;
            GridSet(copySeparatorInput, 2, 1, configGrid);
            GridSet(copySeparatorBG, 2, 1, configGrid);
            Panel.SetZIndex(copySeparatorBG, 0);
            Panel.SetZIndex(copySeparatorInput, 1);
            copySeparatorBG.Click += FocusCopySeparator;
            copySeparatorInput.Background = Brushes.Black;
            copySeparatorInput.Foreground = Brushes.White;
            copySeparatorInput.SelectionBrush = Brushes.Red;

            Button searchSeparatorBG = CreateControlElement<Button>("");
            searchSeparatorBG.Background = new SolidColorBrush(Color.FromRgb(190, 230, 253));
            searchSeparatorBG.BorderThickness = new Thickness(0);
            searchSeparatorBG.IsHitTestVisible = true;
            searchSeparatorBG.Focusable = false;
            GridSet(searchSeparatorInput, 3, 1, configGrid);
            GridSet(searchSeparatorBG, 3, 1, configGrid);
            Panel.SetZIndex(searchSeparatorBG, 0);
            Panel.SetZIndex(searchSeparatorInput, 1);
            searchSeparatorBG.Click += FocusSearchSeparator;
            searchSeparatorInput.Background = Brushes.Black;
            searchSeparatorInput.Foreground = Brushes.White;
            searchSeparatorInput.SelectionBrush = Brushes.Red;




            copyWithMethodInput.Checked += CopyWithMethodInput_CheckChanged;
            copyWithMethodInput.Unchecked += CopyWithMethodInput_CheckChanged;
            searchWithMethodInput.Checked += SearchWithMethodInput_CheckChanged;
            searchWithMethodInput.Unchecked += SearchWithMethodInput_CheckChanged;

            copySeparatorInput.TextChanged += CopySeparatorInput_TextChanged;
            searchSeparatorInput.TextChanged += SearchSeparatorInput_TextChanged;




            copyWithMethodDescription.Text = copyWithMethodDescriptionTranslation;
            searchWithMethodDescription.Text = searchWithMethodDescriptionTranslation;
            copySeparatorDescription.Text = copySeparatorDescriptionTranslation;
            searchSeparatorDescription.Text = searchSeparatorDescriptionTranslation;

            copyWithMethodDescription.IsReadOnly = true;
            searchWithMethodDescription.IsReadOnly = true;
            copySeparatorDescription.IsReadOnly = true;
            searchSeparatorDescription.IsReadOnly = true;

            copyWithMethodDescription.IsReadOnlyCaretVisible = true;
            searchWithMethodDescription.IsReadOnlyCaretVisible = true;
            copySeparatorDescription.IsReadOnlyCaretVisible = true;
            searchSeparatorDescription.IsReadOnlyCaretVisible = true;

            copyWithMethodDescription.BorderThickness = new Thickness(0);
            searchWithMethodDescription.BorderThickness = new Thickness(0);
            copySeparatorDescription.BorderThickness = new Thickness(0);
            searchSeparatorDescription.BorderThickness = new Thickness(0);

            copyWithMethodInput.HorizontalAlignment = HorizontalAlignment.Center;
            searchWithMethodInput.HorizontalAlignment = HorizontalAlignment.Center;
            copySeparatorInput.HorizontalAlignment = HorizontalAlignment.Center;
            searchSeparatorInput.HorizontalAlignment = HorizontalAlignment.Center;

            copyWithMethodDescription.FontSize = 22;
            searchWithMethodDescription.FontSize = 22;
            copySeparatorDescription.FontSize = 22;
            copySeparatorInput.FontSize = 22;
            searchSeparatorDescription.FontSize = 22;
            searchSeparatorInput.FontSize = 22;

            copyWithMethodDescription.TextWrapping = TextWrapping.Wrap;
            searchWithMethodDescription.TextWrapping = TextWrapping.Wrap;
            copySeparatorDescription.TextWrapping = TextWrapping.Wrap;
            copySeparatorInput.TextWrapping = TextWrapping.Wrap;
            searchSeparatorDescription.TextWrapping = TextWrapping.Wrap;
            searchSeparatorInput.TextWrapping = TextWrapping.Wrap;


            copyWithMethodInput.IsChecked = copyWithMethodInputIsChecked;
            searchWithMethodInput.IsChecked = searchWithMethodInputIsChecked;
            copySeparatorInput.Text = copySeparatorInputText;
            searchSeparatorInput.Text = searchSeparatorInputText;





            #endregion



            #region FILE


            fileGrid = new() { ShowGridLines = false, };
            SetGridRowDefinition(fileGrid, "Auto");
            for (int i = 0; i < 4; i++)
            {
                SetGridColumnDefinition(fileGrid, "Auto");
            }
            fileGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            GridSet(fileGrid, content.Children.Count, 0, content);
            fileGrid.Margin = new Thickness(0, 0, 0, 12);



            fileTextBox = CreateControlElement<TextBox>("");
            fileStatusTextBox = CreateControlElement<TextBox>("");
            copyFileButton = CreateControlElement<Button>(COPY_ICON);
            searchFileButton = CreateControlElement<Button>(SEARCH_ICON);

            fileTextBox.TextChanged += FileTextBox_TextChanged;
            fileTextBox.PreviewTextInput += FileTextBox_PreviewTextInput;
            copyFileButton.Click += CopyFile;
            searchFileButton.Click += SearchFile;

            GridSet(fileTextBox, 0, fileGrid.Children.Count, fileGrid);
            GridSet(fileStatusTextBox, 0, fileGrid.Children.Count, fileGrid);
            GridSet(copyFileButton, 0, fileGrid.Children.Count, fileGrid);
            GridSet(searchFileButton, 0, fileGrid.Children.Count, fileGrid);

            fileStatusTextBox.BorderThickness = new Thickness(0);
            fileStatusTextBox.IsReadOnly = true;
            fileStatusTextBox.IsReadOnlyCaretVisible = true;
            fileStatusTextBox.Text = DIRECTORY_ICON + MISSING_ICON;

            fileStatusTextBox.Background = CUSTOM_RED;
            fileTextBox.Background = customColorsHalfDict[CUSTOM_RED];


            #endregion




            #region HASHES


            hashesGrid = new() { ShowGridLines = false, };
            for (int i = 0; i < hashRowQuantity; i++)
            {
                SetGridRowDefinition(hashesGrid, "Auto");
            }
            for (int i = 0; i < 8; i++)
            {
                SetGridColumnDefinition(hashesGrid, "Auto");
            }
            hashesGrid.ColumnDefinitions[4].Width = new GridLength(1, GridUnitType.Star);
            GridSet(hashesGrid, content.Children.Count, 0, content);



            hashCheckBoxArray = new CheckBox[hashRowQuantity];
            hashBlackListButtonArray = new Button[hashRowQuantity];
            hashWhiteListButtonArray = new Button[hashRowQuantity];
            hashMethodTextBoxArray = new TextBox[hashRowQuantity];
            hashValueTextBoxArray = new TextBox[hashRowQuantity];
            hashStatusTextBoxArray = new TextBox[hashRowQuantity];
            copyHashValueButtonArray = new Button[hashRowQuantity];
            searchHashValueButtonArray = new Button[hashRowQuantity];


            for (int i = 0; i < HASHDICT.Keys.Count; i++)
            {
                string hashMethod = HASHDICT.Keys.ToArray()[i];


                hashCheckBoxArray[i] = CreateControlElement<CheckBox>("");
                hashBlackListButtonArray[i] = CreateControlElement<Button>("➖");
                hashWhiteListButtonArray[i] = CreateControlElement<Button>("➕");
                hashMethodTextBoxArray[i] = CreateControlElement<TextBox>("");
                hashValueTextBoxArray[i] = CreateControlElement<TextBox>("");
                hashStatusTextBoxArray[i] = CreateControlElement<TextBox>(MISSING_ICON);
                copyHashValueButtonArray[i] = CreateControlElement<Button>(COPY_ICON);
                searchHashValueButtonArray[i] = CreateControlElement<Button>(SEARCH_ICON);




                GridSet(hashCheckBoxArray[i], i, hashesGrid.Children.Count % 8, hashesGrid);
                GridSet(hashBlackListButtonArray[i], i, hashesGrid.Children.Count % 8, hashesGrid);
                GridSet(hashWhiteListButtonArray[i], i, hashesGrid.Children.Count % 8, hashesGrid);
                GridSet(hashMethodTextBoxArray[i], i, hashesGrid.Children.Count % 8, hashesGrid);
                GridSet(hashValueTextBoxArray[i], i, hashesGrid.Children.Count % 8, hashesGrid);
                GridSet(hashStatusTextBoxArray[i], i, hashesGrid.Children.Count % 8, hashesGrid);
                GridSet(copyHashValueButtonArray[i], i, hashesGrid.Children.Count % 8, hashesGrid);
                GridSet(searchHashValueButtonArray[i], i, hashesGrid.Children.Count % 8, hashesGrid);




                copyHashValueButtonArray[i].Click += CopyHash;
                searchHashValueButtonArray[i].Click += SearchHash;
                hashCheckBoxArray[i].Checked += HashCheckBox_CheckToggle;
                hashCheckBoxArray[i].Unchecked += HashCheckBox_CheckToggle;
                hashBlackListButtonArray[i].Click += BlackList;
                hashWhiteListButtonArray[i].Click += WhiteList;




                hashMethodTextBoxArray[i].Text = hashMethod;

                hashCheckBoxArray[i].IsChecked = true;

                hashMethodTextBoxArray[i].HorizontalContentAlignment = HorizontalAlignment.Right;
                hashMethodTextBoxArray[i].BorderThickness = new Thickness(0);
                hashMethodTextBoxArray[i].IsReadOnly = true;
                hashMethodTextBoxArray[i].IsReadOnlyCaretVisible = true;

                hashValueTextBoxArray[i].HorizontalContentAlignment = HorizontalAlignment.Left;
                hashValueTextBoxArray[i].BorderThickness = new Thickness(0);
                hashValueTextBoxArray[i].IsReadOnly = true;
                hashValueTextBoxArray[i].IsReadOnlyCaretVisible = true;

                hashStatusTextBoxArray[i].BorderThickness = new Thickness(0);
                hashStatusTextBoxArray[i].IsReadOnly = true;
                hashStatusTextBoxArray[i].IsReadOnlyCaretVisible = true;
                hashStatusTextBoxArray[i].Background = Brushes.Gray;

                hashStatusTextBoxArray[i].Text = MISSING_ICON;
                hashStatusTextBoxArray[i].Background = Brushes.Gray;
                hashValueTextBoxArray[i].Text = new string('-', 64);

            }

            hashesGrid.IsEnabled = false;

            #endregion





            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                fileTextBox.Text = args[1];
            }
            fileTextBox.Focus();


        }

        private void FocusCopySeparator(object sender, RoutedEventArgs e)
        {
            copySeparatorInput.Focus();
        }
        private void FocusSearchSeparator(object sender, RoutedEventArgs e)
        {
            searchSeparatorInput.Focus();
        }

        private void CopyWithMethodInput_CheckChanged(object sender, RoutedEventArgs e)
        {
            copyWithMethodInputIsChecked = copyWithMethodInput.IsChecked == true;
        }
        private void SearchWithMethodInput_CheckChanged(object sender, RoutedEventArgs e)
        {
            searchWithMethodInputIsChecked = searchWithMethodInput.IsChecked == true;
        }
        private void CopySeparatorInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            copySeparatorInputText = copySeparatorInput.Text;
        }
        private void SearchSeparatorInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchSeparatorInputText = searchSeparatorInput.Text;
        }



        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            SaveJson();
        }

        public void SaveJson()
        {
            Config config = new()
            {
                C_languageCode = languageCode,
                C_searchEngineValue = searchEngineDropdownSelectedIndex,
                C_copyWithMethodInputIsChecked = copyWithMethodInputIsChecked,
                C_searchWithMethodInputIsChecked = searchWithMethodInputIsChecked,
                C_copySeparatorInputText = copySeparatorInputText,
                C_searchSeparatorInputText = searchSeparatorInputText,
            };
            string jsonString = JsonSerializer.Serialize(config, JSON_SERIALIZER_OPTIONS);
            File.WriteAllText(CONFIG_FILE_NAME, jsonString);
        }

        private void OpenConfig(object sender, RoutedEventArgs e)
        {
            Window window = new()
            {
                Title = CONFIG_ICON,
                Width = Width / 1.1,
                Height = Height / 1.1,
                Background = Brushes.DarkGray,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Content = configGrid
            };
            window.ShowDialog();
        }




        #region GRID

        static void GridSet(UIElement gridElement, int row, int column, Grid targetGrid)
        {
            Grid.SetRow(gridElement, row);
            Grid.SetColumn(gridElement, column);
            targetGrid.Children.Add(gridElement);
        }

        static void SetGridRowDefinition(Grid grid, string height)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = ParseGridLength(height) });
        }

        static void SetGridColumnDefinition(Grid grid, string width)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = ParseGridLength(width) });
        }

        static GridLength ParseGridLength(string value)
        {
            if (value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
                return GridLength.Auto;

            if (value.EndsWith('*'))
            {
                string starValue = value.TrimEnd('*');
                if (string.IsNullOrEmpty(starValue))
                    return new GridLength(1, GridUnitType.Star);

                if (double.TryParse(starValue, out double factor))
                    return new GridLength(factor, GridUnitType.Star);
            }

            if (double.TryParse(value, out double pixels))
                return new GridLength(pixels, GridUnitType.Pixel);

            throw new ArgumentException($"Invalid GridLength value: {value}");
        }

        #endregion




        #region ROW_TOGGLE

        void HashCheckBox_CheckToggle(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox targetCheckBox)
            {
                int row = Grid.GetRow(targetCheckBox);
                if (targetCheckBox.IsChecked == false)
                {
                    DisableHashRow(row);
                }
                else
                {
                    EnableHashRow(row);
                }
            }

        }

        void BlackList(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                int row = Grid.GetRow(clickedButton);

                for (int i = 0; i < HASHDICT.Keys.Count; i++)
                {
                    EnableHashRow(i);
                }
                DisableHashRow(row);
            }
        }

        void WhiteList(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                int row = Grid.GetRow(clickedButton);

                for (int i = 0; i < HASHDICT.Keys.Count; i++)
                {
                    DisableHashRow(i);
                }
                EnableHashRow(row);
            }
        }

        void DisableHashRow(int row)
        {
            hashCheckBoxArray[row].IsChecked = false;

            hashMethodTextBoxArray[row].IsEnabled = false;
            hashValueTextBoxArray[row].IsEnabled = false;
            copyHashValueButtonArray[row].IsEnabled = false;
            searchHashValueButtonArray[row].IsEnabled = false;


            hashValueTextBoxArray[row].Visibility = Visibility.Hidden;
            hashStatusTextBoxArray[row].Visibility = Visibility.Hidden;
            copyHashValueButtonArray[row].Visibility = Visibility.Hidden;
            searchHashValueButtonArray[row].Visibility = Visibility.Hidden;
        }

        void EnableHashRow(int row)
        {
            hashCheckBoxArray[row].IsChecked = true;

            hashMethodTextBoxArray[row].IsEnabled = true;
            hashValueTextBoxArray[row].IsEnabled = true;
            copyHashValueButtonArray[row].IsEnabled = true;
            searchHashValueButtonArray[row].IsEnabled = true;

            hashValueTextBoxArray[row].Visibility = Visibility.Visible;
            hashStatusTextBoxArray[row].Visibility = Visibility.Visible;
            copyHashValueButtonArray[row].Visibility = Visibility.Visible;
            searchHashValueButtonArray[row].Visibility = Visibility.Visible;
        }

        #endregion




        #region HASH_CALCULATION

        public static string GetCRC32(string filePath)
        {
            uint[] Table = new uint[256];
            const uint polynomial = 0xEDB88320;

            for (uint i = 0; i < Table.Length; ++i)
            {
                uint entry = i;
                for (int j = 0; j < 8; ++j)
                {
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry >>= 1;
                }
                Table[i] = entry;
            }

            uint crc = 0xFFFFFFFF;
            const int bufferSize = 8192;
            byte[] buffer = new byte[bufferSize];

            using FileStream stream = File.OpenRead(filePath);
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    byte tableIndex = (byte)((crc ^ buffer[i]) & 0xFF);
                    crc = (crc >> 8) ^ Table[tableIndex];
                }
            }

            return $"{~crc:X8}";
        }

        public static string GetMD5(string filePath)
        {
            using MD5 md5 = MD5.Create();
            using FileStream stream = File.OpenRead(filePath);
            StringBuilder stringBuilder = new();

            byte[] hash = md5.ComputeHash(stream);

            foreach (byte @byte in hash)
            {
                stringBuilder.Append(@byte.ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        public static string GetSHA1(string filePath)
        {
            using SHA1 sha1 = SHA1.Create();
            using FileStream stream = File.OpenRead(filePath);
            StringBuilder stringBuilder = new();

            byte[] hash = sha1.ComputeHash(stream);

            foreach (byte @byte in hash)
            {
                stringBuilder.Append(@byte.ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        public static string GetSHA256(string filePath)
        {
            using SHA256 sha256 = SHA256.Create();
            using FileStream stream = File.OpenRead(filePath);
            StringBuilder stringBuilder = new();

            byte[] hash = sha256.ComputeHash(stream);

            foreach (byte @byte in hash)
            {
                stringBuilder.Append(@byte.ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        #endregion




        #region ALT_BAR

        public string AboutText
        {
            get => _aboutText;
            set { _aboutText = value; OnPropertyChanged(nameof(AboutText)); }
        }

        public string LanguageText
        {
            get => _languageText;
            set { _languageText = value; OnPropertyChanged(nameof(LanguageText)); }
        }

        static void DisplayAbout()
        {
            string messageBoxText =
                "Copyright (C) 2025 Felipe R.T.\r\n\r\n" +
                "This program is free software: you can redistribute it and/or modify it\r\n" +
                "under the terms of the GNU General Public License as published by\r\n" +
                "the Free Software Foundation, either version 3 of the License, or\r\n" +
                "(at your option) any later version.\r\n\r\n" +
                "This program is distributed in the hope that it will be useful,\r\n" +
                "but WITHOUT ANY WARRANTY; without even the implied warranty of\r\n" +
                "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.\r\n" +
                "See the GNU General Public License for more details.\r\n\r\n" +
                "You should have received a copy of the GNU General Public License along with this program. " +
                "If not, see <https://www.gnu.org/licenses/>." +
                "\r\n\r\n" +
                "==========" +
                "\r\n\r\n" +
                "https://github.com/FelipeRT98/ShowHashes";


            string caption = Assembly.GetExecutingAssembly().GetName()?.Version?.ToString() ?? "Show Hashes";
            MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public void SetLanguage(string languageCode)
        {
            switch (languageCode)
            {
                case "en":
                    AboutText = "_About";
                    LanguageText = "_Language";

                    copyWithMethodDescriptionTranslation = "When copying, the hash method is included with the hash value";
                    searchWithMethodDescriptionTranslation = "When searching, the hash method is included with the hash value";
                    copySeparatorDescriptionTranslation = "When copying, this value is placed before the hash value";
                    searchSeparatorDescriptionTranslation = "When searching, this value is placed before the hash value";
                    break;
                case "es":
                    AboutText = "_Acerca de";
                    LanguageText = "_Idioma";

                    copyWithMethodDescriptionTranslation = "Al copiar, el método hash se incluye con el valor hash";
                    searchWithMethodDescriptionTranslation = "Al buscar, el método hash se incluye con el valor hash";
                    copySeparatorDescriptionTranslation = "Al copiar, este valor se coloca antes del valor hash";
                    searchSeparatorDescriptionTranslation = "Al buscar, este valor se coloca antes del valor hash";
                    break;
                case "de":
                    AboutText = "Ü_ber";
                    LanguageText = "_Sprache";

                    copyWithMethodDescriptionTranslation = "Beim Kopieren wird die Hash-Methode mit dem Hash-Wert eingeschlossen";
                    searchWithMethodDescriptionTranslation = "Beim Suchen wird die Hash-Methode mit dem Hash-Wert eingeschlossen";
                    copySeparatorDescriptionTranslation = "Beim Kopieren wird dieser Wert vor dem Hashwert eingefügt";
                    searchSeparatorDescriptionTranslation = "Beim Suchen wird dieser Wert vor dem Hashwert eingefügt";
                    break;
                case "pt":
                    AboutText = "_Sobre";
                    LanguageText = "_Idioma";

                    copyWithMethodDescriptionTranslation = "Ao copiar, o método de hash é incluído com o valor hash";
                    searchWithMethodDescriptionTranslation = "Ao pesquisar, o método de hash é incluído com o valor hash";
                    copySeparatorDescriptionTranslation = "Ao copiar, este valor é colocado antes do valor do hash";
                    searchSeparatorDescriptionTranslation = "Ao pesquisar, este valor é colocado antes do valor do hash";
                    break;
                case "fr":
                    AboutText = "À pr_opos";
                    LanguageText = "_Langue";

                    copyWithMethodDescriptionTranslation = "Lors de la copie, la méthode de hachage est incluse avec la valeur de hachage";
                    searchWithMethodDescriptionTranslation = "Lors de la recherche, la méthode de hachage est incluse avec la valeur de hachage";
                    copySeparatorDescriptionTranslation = "Lors de la copie, cette valeur est placée avant la valeur de hachage";
                    searchSeparatorDescriptionTranslation = "Lors de la recherche, cette valeur est placée avant la valeur de hachage";
                    break;
                case "it":
                    AboutText = "_Informazioni";
                    LanguageText = "_Lingua";

                    copyWithMethodDescriptionTranslation = "Durante la copia, il metodo di hash è incluso con il valore hash";
                    searchWithMethodDescriptionTranslation = "Durante la ricerca, il metodo di hash è incluso con il valore hash";
                    copySeparatorDescriptionTranslation = "Durante la copia, questo valore viene inserito prima del valore hash";
                    searchSeparatorDescriptionTranslation = "Durante la ricerca, questo valore viene inserito prima del valore hash";
                    break;
                case "ja":
                    AboutText = "情報";
                    LanguageText = "言語";

                    copyWithMethodDescriptionTranslation = "コピー時に、ハッシュ方式がハッシュ値と一緒に含まれます";
                    searchWithMethodDescriptionTranslation = "検索時に、ハッシュ方式がハッシュ値と一緒に含まれます";
                    copySeparatorDescriptionTranslation = "コピー時に、この値がハッシュ値の前に配置されます";
                    searchSeparatorDescriptionTranslation = "検索時に、この値がハッシュ値の前に配置されます";
                    break;
                case "ko":
                    AboutText = "정보";
                    LanguageText = "언어";

                    copyWithMethodDescriptionTranslation = "복사 시, 해시 방법이 해시 값과 함께 포함됩니다";
                    searchWithMethodDescriptionTranslation = "검색 시, 해시 방법이 해시 값과 함께 포함됩니다";
                    copySeparatorDescriptionTranslation = "복사할 때 이 값이 해시 값 앞에 배치됩니다";
                    searchSeparatorDescriptionTranslation = "검색할 때 이 값이 해시 값 앞에 배치됩니다";
                    break;
                case "zh":
                    AboutText = "关于";
                    LanguageText = "语言";

                    copyWithMethodDescriptionTranslation = "复制时，哈希方法与哈希值一起包含";
                    searchWithMethodDescriptionTranslation = "搜索时，哈希方法与哈希值一起包含";
                    copySeparatorDescriptionTranslation = "复制时，此值会放在哈希值之前";
                    searchSeparatorDescriptionTranslation = "搜索时，此值会放在哈希值之前";
                    break;
                case "hi":
                    AboutText = "के बारे में";
                    LanguageText = "भाषा";

                    copyWithMethodDescriptionTranslation = "कॉपी करते समय, हैश विधि हैश मान के साथ शामिल होती है";
                    searchWithMethodDescriptionTranslation = "खोजते समय, हैश विधि हैश मान के साथ शामिल होती है";
                    copySeparatorDescriptionTranslation = "कॉपी करते समय, यह मान हैश मान से पहले रखा जाता है";
                    searchSeparatorDescriptionTranslation = "खोजते समय, यह मान हैश मान से पहले रखा जाता है";
                    break;
                case "ru":
                    AboutText = "О программе";
                    LanguageText = "Язык";

                    copyWithMethodDescriptionTranslation = "При копировании метод хеширования включается вместе со значением хеша";
                    searchWithMethodDescriptionTranslation = "При поиске метод хеширования включается вместе со значением хеша";
                    copySeparatorDescriptionTranslation = "При копировании это значение помещается перед значением хеша";
                    searchSeparatorDescriptionTranslation = "При поиске это значение помещается перед значением хеша";
                    break;
                default:
                    AboutText = "_About";
                    LanguageText = "_Language";

                    copyWithMethodDescriptionTranslation = "When copying, the hash method is included with the hash value";
                    searchWithMethodDescriptionTranslation = "When searching, the hash method is included with the hash value";
                    copySeparatorDescriptionTranslation = "When copying, this value is placed before the hash value";
                    searchSeparatorDescriptionTranslation = "When searching, this value is placed before the hash value";
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        void About_Click(object sender, RoutedEventArgs e)
        {
            DisplayAbout();
        }

        void SetLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                languageCode = menuItem.Tag.ToString() ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                SetLanguage(languageCode);

                copyWithMethodDescription.Text = copyWithMethodDescriptionTranslation;
                searchWithMethodDescription.Text = searchWithMethodDescriptionTranslation;
                copySeparatorDescription.Text = copySeparatorDescriptionTranslation;
                searchSeparatorDescription.Text = searchSeparatorDescriptionTranslation;
            }
        }

        #endregion




        #region SEARCH

        void SearchEngineDropdown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                searchEngineDropdown.IsDropDownOpen = true;
                e.Handled = true;
            }
        }

        void SearchEngineDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            searchEngineDropdownSelectedIndex = searchEngineDropdown.SelectedIndex;
        }

        void SearchFile(object sender, RoutedEventArgs e)
        {
            string searchUrl;

            searchUrl = SEARCH_ENGINES[searchEngineDropdownSelectedIndex] + Uri.EscapeDataString(Path.GetFileName(fileTextBox.Text));
            Process.Start(new ProcessStartInfo
            {
                FileName = searchUrl,
                UseShellExecute = true
            });
        }

        void SearchHash(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement clickedElement)
            {
                int row = Grid.GetRow(clickedElement);

                string hashMethod = (searchWithMethodInput.IsChecked == true) ? hashMethodTextBoxArray[row].Text : "";
                string hashValue = hashValueTextBoxArray[row].Text;

                string searchUrl = SEARCH_ENGINES[searchEngineDropdownSelectedIndex] + Uri.EscapeDataString(hashMethod + searchSeparatorInput.Text + hashValue);
                Process.Start(new ProcessStartInfo
                {
                    FileName = searchUrl,
                    UseShellExecute = true
                });
            }
        }

        void SearchAllButton_Click(object sender, RoutedEventArgs e)
        {
            string searchUrl;

            searchUrl = SEARCH_ENGINES[searchEngineDropdownSelectedIndex] + Uri.EscapeDataString(Path.GetFileName(fileTextBox.Text));
            Process.Start(new ProcessStartInfo
            {
                FileName = searchUrl,
                UseShellExecute = true
            });


            for (int row = 0; row < hashRowQuantity; row++)
            {
                if (hashValueTextBoxArray[row].IsEnabled)
                {
                    string hashMethod = (searchWithMethodInput.IsChecked == true) ? hashMethodTextBoxArray[row].Text : ""; ;
                    string hashValue = hashValueTextBoxArray[row].Text;

                    searchUrl = SEARCH_ENGINES[searchEngineDropdownSelectedIndex] + Uri.EscapeDataString(hashMethod + searchSeparatorInput.Text + hashValue);
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = searchUrl,
                        UseShellExecute = true
                    });
                }
            }

        }

        #endregion




        #region COPY

        void CopyFile(object sender, RoutedEventArgs e)
        {
            TrySetClipboardText(fileTextBox.Text);
        }

        void CopyHash(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement clickedElement)
            {
                int row = Grid.GetRow(clickedElement);

                string hashMethod = (copyWithMethodInput.IsChecked == true) ? hashMethodTextBoxArray[row].Text : "";
                string hashValue = hashValueTextBoxArray[row].Text;

                TrySetClipboardText(hashMethod + copySeparatorInput.Text + hashValue);
            }
        }

        void CopyAllButton_Click(object sender, RoutedEventArgs e)
        {
            string allText = string.Empty;

            if (fileStatusTextBox.Text == FILE_ICON + FOUND_ICON)
            {
                allText += Path.GetFileName(fileTextBox.Text) + "\n";
            }
            else
            {
                allText += fileTextBox.Text + "\n";
            }

            for (int row = 0; row < hashRowQuantity; row++)
            {
                if (hashValueTextBoxArray[row].IsEnabled)
                {
                    string hashMethod = (copyWithMethodInput.IsChecked == true) ? hashMethodTextBoxArray[row].Text : "";
                    string hashValue = hashValueTextBoxArray[row].Text;

                    allText += hashMethod + copySeparatorInput.Text + hashValue + "\n";
                }
            }

            TrySetClipboardText(allText);

        }

        void TrySetClipboardText(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Clipboard.SetText(text);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Clipboard error: " + ex.Message);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);

        }

        #endregion




        #region FILE_TEXTBOX

        async void FileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (fileStatusTextBox == null)
            {
                return;
            }

            string fileTextBoxText = fileTextBox.Text.Trim();

            string cleanedText = new(fileTextBoxText
                                            .Where(c => !INVALID_CHARACTERS.Contains(c))
                                            .ToArray());

            if (fileTextBoxText != cleanedText)
            {
                // Temporarily disconnect the event handler to prevent recursion
                fileTextBox.TextChanged -= FileTextBox_TextChanged;

                fileTextBox.Text = cleanedText;

                fileTextBox.TextChanged += FileTextBox_TextChanged;
            }

            if (string.IsNullOrEmpty(cleanedText))
            {
                fileTextBox.Background = CUSTOM_HALFRED;
                fileStatusTextBox.Text = DIRECTORY_ICON + MISSING_ICON;
                fileStatusTextBox.Background = CUSTOM_RED;
                return;
            }

            string directory = Path.GetDirectoryName(cleanedText) ?? string.Empty;

            bool endsWithDirSeparator = cleanedText.EndsWith('/') || cleanedText.EndsWith('\\');
            string statusText;
            Brush backgroundBrush;
            ;

            if (hashesGrid != null)
            {
                hashesGrid.IsEnabled = false;
                for (int i = 0; i < hashRowQuantity; i++)
                {
                    hashStatusTextBoxArray[i].Text = MISSING_ICON;
                    hashStatusTextBoxArray[i].Background = Brushes.Gray;
                    hashValueTextBoxArray[i].Text = new string('-', 64);
                }

            }



            if (!Directory.Exists(directory))
            {
                statusText = endsWithDirSeparator ? DIRECTORY_ICON + NOTFOUND_ICON : DIRECTORY_ICON + MISSING_ICON;
                backgroundBrush = CUSTOM_RED;
            }
            else if (endsWithDirSeparator)
            {
                statusText = FILE_ICON + MISSING_ICON;
                backgroundBrush = CUSTOOM_ORANGE;
            }
            else if (!File.Exists(cleanedText))
            {
                statusText = FILE_ICON + NOTFOUND_ICON;
                backgroundBrush = CUSTOOM_ORANGE;
            }
            else
            {
                statusText = FILE_ICON + FOUND_ICON;
                backgroundBrush = CUSTOM_GREEN;

                searchAllButton.IsEnabled = true;
                searchFileButton.IsEnabled = true;
            }

            fileTextBox.Background = customColorsHalfDict[(SolidColorBrush)backgroundBrush];
            fileStatusTextBox.Text = statusText;
            fileStatusTextBox.Background = backgroundBrush;

            if (hashesGrid != null && statusText == FILE_ICON + FOUND_ICON)
            {
                string filePath = fileTextBox.Text;

                await Task.Run(() =>
                {
                    for (int i = 0; i < hashRowQuantity; i++)
                    {
                        string hashMethod = HASHDICT.Keys.ToArray()[i];

                        // Create CancellationTokenSource per textbox
                        var cts = new CancellationTokenSource();
                        var token = cts.Token;

                        // Start spinner
                        var spinnerTask = Dispatcher.Invoke(() => RunSpinnerAsync(hashStatusTextBoxArray[i], token));

                        // Run hash calculation in background
                        object? invokeResult = HASHDICT[hashMethod].Invoke(null, [filePath]);
                        string result = invokeResult as string ?? string.Empty;

                        // Stop spinner and show result
                        Dispatcher.Invoke(() =>
                        {
                            cts.Cancel(); // Tell spinner to stop
                            hashValueTextBoxArray[i].Text = result;

                            hashStatusTextBoxArray[i].Text = FOUND_ICON;
                            hashStatusTextBoxArray[i].Background = CUSTOM_GREEN;
                        });
                    }
                });

                hashesGrid.IsEnabled = true;

            }
        }

        void FileTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Text) && e.Text.Any(c => INVALID_CHARACTERS.Contains(c)))
            {
                e.Handled = true;
            }

        }

        #endregion




        static async Task RunSpinnerAsync(TextBox textBox, CancellationToken token)
        {
            string[] spinnerFrames = ["|", "/", "-", "\\"];
            int frame = 0;

            while (!token.IsCancellationRequested)
            {
                textBox.Text = spinnerFrames[frame % spinnerFrames.Length];
                frame++;

                await Task.Delay(64, token); // Adjust for speed
            }
        }

        T CreateControlElement<T>(object content) where T : Control, new()
        {
            var element = new T
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                BorderBrush = BORDER_COLOR,
                Margin = new Thickness(0, 0, 4, 4),
            };

            var type = typeof(T);

            if (type == typeof(Button))
            {
                element.Background = BUTTON_COLOR;
            }
            else if (type == typeof(TextBox))
            {
                element.Background = TEXTBOX_COLOR;
            }


            if (content != null && element is ContentControl contentControl)
            {
                contentControl.Content = content;
            }

            return element;
        }


    }


}
