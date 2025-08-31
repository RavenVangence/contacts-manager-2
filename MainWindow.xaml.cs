using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using ContactsManager.ViewModels;
using ContactsManager.Pages;
using System.IO;
using System.Windows.Media.Imaging;

namespace ContactsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;
        private ContactsPage? _contactsPageInstance;
        private bool _isTitleDragInitiated;
        private Point _titleMouseDownPos;
        private const double DragThreshold = 4; // pixels
        private const double DragVerticalCompensation = 6; // pixels to nudge window up on restore-drag

        public MainWindow()
        {
            InitializeComponent();

            // Set the DataContext to the new MainWindowViewModel
            DataContext = new MainWindowViewModel();

            // Handle window closing event
            Closing += MainWindow_Closing;

            // Navigate to home page on startup
            Loaded += (_, __) =>
            {
                // Try load window icon: prefer .ico; fall back to rendering biome.svg
                TryLoadWindowIcon();

                NavigateToHome();
            };
        }

        private void TryLoadWindowIcon()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var icoPath = Path.Combine(baseDir, "Assets", "app.ico");
                if (File.Exists(icoPath))
                {
                    Icon = BitmapFrame.Create(new Uri(icoPath, UriKind.Absolute));
                    return;
                }

                // Try SVG fallback
                var svgPath = Path.Combine(baseDir, "Assets", "biome.svg");
                if (File.Exists(svgPath))
                {
                    // Use the app's primary/success green for tint
                    var tint = GetPrimaryColor();
                    var bmp = RenderSvgToBitmap(svgPath, 256, 256, tint);
                    if (bmp != null)
                    {
                        Icon = bmp;
                    }
                }
            }
            catch { /* ignore icon load errors */ }
        }

        private static System.Windows.Media.Color GetPrimaryColor()
        {
            try
            {
                if (Application.Current.Resources["PrimaryBrush"] is SolidColorBrush b)
                {
                    return b.Color;
                }
            }
            catch { }
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0C8900");
        }

        private static BitmapSource? RenderSvgToBitmap(string svgFile, int width, int height, System.Windows.Media.Color? tintColor = null)
        {
            try
            {
                var skSvg = new Svg.Skia.SKSvg();
                using (var ms = new MemoryStream(File.ReadAllBytes(svgFile)))
                {
                    skSvg.Load(ms);
                }

                var picture = skSvg.Picture;
                if (picture == null)
                    return null;

                var info = new SkiaSharp.SKImageInfo(width, height);
                using var surface = SkiaSharp.SKSurface.Create(info);
                var canvas = surface.Canvas;
                canvas.Clear(SkiaSharp.SKColors.Transparent);

                // Scale to fit while preserving aspect
                var picBounds = picture.CullRect;
                float scale = Math.Min(width / picBounds.Width, height / picBounds.Height);
                canvas.Translate(width / 2f, height / 2f);
                canvas.Scale(scale);
                canvas.Translate(-picBounds.MidX, -picBounds.MidY);
                // Optional tint to match Success/Primary green
                if (tintColor.HasValue)
                {
                    var c = tintColor.Value;
                    var skc = new SkiaSharp.SKColor(c.R, c.G, c.B, c.A);
                    using var paint = new SkiaSharp.SKPaint
                    {
                        IsAntialias = true,
                        ColorFilter = SkiaSharp.SKColorFilter.CreateBlendMode(skc, SkiaSharp.SKBlendMode.SrcIn)
                    };
                    canvas.SaveLayer(paint);
                    canvas.DrawPicture(picture);
                    canvas.Restore();
                }
                else
                {
                    canvas.DrawPicture(picture);
                }
                canvas.Flush();

                using var image = surface.Snapshot();
                using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
                using var pngStream = new MemoryStream(data.ToArray());
                var decoder = new PngBitmapDecoder(pngStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                var frame = decoder.Frames[0];
                frame.Freeze();
                return frame;
            }
            catch
            {
                return null;
            }
        }

        public void NavigateToHome()
        {
            ViewModel.NavigateToHome();
            MainFrame.Navigate(new HomePage());
        }

        public void NavigateToContacts()
        {
            ViewModel.NavigateToContacts();

            // Create ContactsPage instance only when needed (lazy loading)
            if (_contactsPageInstance == null)
            {
                _contactsPageInstance = new ContactsPage();
            }

            MainFrame.Navigate(_contactsPageInstance);
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            // Check if we can close based on the contacts page state (if it exists and has been loaded)
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM && !contactsVM.CanClose())
            {
                e.Cancel = true; // Cancel the closing
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.ImportCommand.Execute(null);
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.ExportCommand.Execute(null);
            }
        }

        private void AddContact_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.AddCommand.Execute(null);
            }
        }

        private void EditContact_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.EditCommand.Execute(null);
            }
        }

        private void DeleteContact_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.DeleteCommand.Execute(null);
            }
        }

        private void SortByName_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.SortByNameCommand.Execute(null);
            }
        }

        private void SortByUsed_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.SortByUsedCommand.Execute(null);
            }
        }

        private void OpenContactsManager_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
        }

        private void ContactsNavButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
        }

        private void HomeNavButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToHome();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                return;
            }

            if (e.ButtonState == MouseButtonState.Pressed)
            {
                _isTitleDragInitiated = true;
                _titleMouseDownPos = e.GetPosition(this);
            }
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isTitleDragInitiated || e.LeftButton != MouseButtonState.Pressed)
                return;

            var currentPos = e.GetPosition(this);
            if (Math.Abs(currentPos.X - _titleMouseDownPos.X) < DragThreshold &&
                Math.Abs(currentPos.Y - _titleMouseDownPos.Y) < DragThreshold)
            {
                return; // not enough movement yet
            }

            // Movement exceeded threshold => start actual drag
            _isTitleDragInitiated = false;

            if (WindowState == WindowState.Maximized)
            {
                // Translate mouse position on window to screen, compute relative percent
                var mousePos = e.GetPosition(this);
                double percentX = mousePos.X / ActualWidth;
                double targetWidth = RestoreBounds.Width;
                if (double.IsNaN(targetWidth) || targetWidth <= 0)
                {
                    targetWidth = 1000; // fallback width
                }
                // Convert screen point to WPF (device-independent units) to avoid DPI offsets
                var mouseScreen = PointToScreen(mousePos);
                Point mouseDiu = mouseScreen;
                var src = PresentationSource.FromVisual(this);
                if (src?.CompositionTarget != null)
                {
                    mouseDiu = src.CompositionTarget.TransformFromDevice.Transform(mouseScreen);
                }

                WindowState = WindowState.Normal;
                // Keep cursor over the same relative X, and align Y exactly under cursor
                Left = mouseDiu.X - targetWidth * percentX;
                Top = mouseDiu.Y - mousePos.Y - DragVerticalCompensation;
            }

            try
            {
                DragMove();
            }
            catch { }
        }

        private void TitleBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isTitleDragInitiated)
            {
                _isTitleDragInitiated = false;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}