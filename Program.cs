using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MobaCompanion;

/// <summary>
/// Лёгкий автономный клиент игр. Показывает меню выбора игры (2D MOBA / D&D),
/// затем открывает лобби выбранной игры. Передавайте напарнику MobaCompanion.exe из publish.
/// </summary>
internal static class Program
{
    private static readonly string ErrorLogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MobaCompanion", "startup_error.txt");

    [STAThread]
    private static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            ShowFatal(e.ExceptionObject as Exception, "Необработанная ошибка");

        try
        {
            var app = new Application { ShutdownMode = ShutdownMode.OnLastWindowClose };
            app.DispatcherUnhandledException += (_, e) =>
            {
                ShowFatal(e.Exception, "Ошибка в приложении");
                e.Handled = true;
            };
            app.Run(BuildChooser());
        }
        catch (Exception ex)
        {
            ShowFatal(ex, "Не удалось запустить компаньон");
        }
    }

    private static void ShowFatal(Exception? ex, string title)
    {
        var msg = ex?.ToString() ?? "Неизвестная ошибка.";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ErrorLogPath)!);
            File.WriteAllText(ErrorLogPath, DateTime.Now + Environment.NewLine + msg);
        }
        catch { /* ignore */ }

        try
        {
            MessageBox.Show(
                msg + Environment.NewLine + Environment.NewLine +
                "Подробности сохранены в:" + Environment.NewLine + ErrorLogPath,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch
        {
            Console.Error.WriteLine(msg);
        }
    }

    private static Window BuildChooser()
    {
        var chooser = new Window
        {
            Title = "Игровой компаньон — выбор игры",
            Width = 480,
            Height = 440,
            ResizeMode = ResizeMode.CanMinimize,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = new SolidColorBrush(Color.FromRgb(12, 12, 16)),
        };

        var panel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        panel.Children.Add(new TextBlock
        {
            Text = "ВЫБЕРИТЕ ИГРУ",
            Foreground = Brushes.White,
            FontSize = 30,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12),
        });
        panel.Children.Add(new TextBlock
        {
            Text = "Первый запуск может занять до 1 мин.\nЕсли Windows спрашивает — нажмите «Подробнее» → «Выполнить».",
            Foreground = new SolidColorBrush(Color.FromRgb(160, 170, 190)),
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Width = 380,
            Margin = new Thickness(0, 0, 0, 16),
        });

        panel.Children.Add(MenuButton("🎮  2D MOBA (в духе DOTA)", () =>
        {
            new AiConsole.Game.GameWindow { Title = "2D MOBA — компаньон" }.Show();
            chooser.Close();
        }));

        panel.Children.Add(MenuButton("🎲  D&D — кооп приключение", () =>
        {
            new AiConsole.Game.DndWindow { Title = "D&D — компаньон" }.Show();
            chooser.Close();
        }));

        panel.Children.Add(MenuButton("💼  Офисный спринт (1 раб. день)", () =>
        {
            new AiConsole.Game.DaySimWindow { Title = "Офисный спринт — компаньон" }.Show();
            chooser.Close();
        }));

        chooser.Content = panel;
        return chooser;
    }

    private static Button MenuButton(string text, Action onClick)
    {
        var b = new Button
        {
            Content = text,
            FontSize = 17,
            Width = 340,
            Padding = new Thickness(12),
            Margin = new Thickness(6),
            Cursor = System.Windows.Input.Cursors.Hand,
            Background = new SolidColorBrush(Color.FromRgb(30, 42, 54)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(79, 168, 255)),
        };
        b.Click += (_, _) => onClick();
        return b;
    }
}
