using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace OsuGameAvalonia;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}

public class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new GameWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

public class GameWindow : Window
{
    public GameWindow()
    {
        Title = "OSU Mini Game on C#";
        Width = 800;
        Height = 500;
        MinWidth = 640;
        MinHeight = 420;

        Content = new GameCanvas();
    }
}

public class GameCanvas : Control
{
    private readonly DispatcherTimer timer;
    private readonly Random random = new Random();

    private double targetX = 350;
    private double targetY = 220;
    private double mouseX = 100;
    private double mouseY = 100;
    private int score = 0;

    private int dx = 1;
    private int dy = 1;

    public GameCanvas()
    {
        Focusable = true;

        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(35)
        };

        timer.Tick += (_, _) =>
        {
            MoveTarget();
            InvalidateVisual();
        };

        timer.Start();

        PointerMoved += OnPointerMoved;
        PointerPressed += OnPointerPressed;
        KeyDown += OnKeyDown;

        AttachedToVisualTree += (_, _) => Focus();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        mouseX = position.X;
        mouseY = position.Y;

        CheckHit();
        InvalidateVisual();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        CheckHit();
        InvalidateVisual();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            score = 0;
        }
    }

    private void MoveTarget()
    {
        targetX += dx * 6;
        targetY += dy * 6;

        if (targetX < 60 || targetX > Bounds.Width - 60)
        {
            dx *= -1;
            dy = random.Next(-1, 2);
            if (dy == 0) dy = 1;
        }

        if (targetY < 80 || targetY > Bounds.Height - 60)
        {
            dy *= -1;
            dx = random.Next(-1, 2);
            if (dx == 0) dx = 1;
        }
    }

    private void CheckHit()
    {
        double distance = Math.Sqrt(
            Math.Pow(mouseX - targetX, 2) +
            Math.Pow(mouseY - targetY, 2)
        );

        if (distance < 45)
        {
            score++;
            targetX = random.Next(80, Math.Max(90, (int)Bounds.Width - 80));
            targetY = random.Next(100, Math.Max(110, (int)Bounds.Height - 80));

            dx = random.Next(-1, 2);
            dy = random.Next(-1, 2);

            if (dx == 0) dx = 1;
            if (dy == 0) dy = 1;
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        Rect area = new Rect(Bounds.Size);

        context.FillRectangle(
            new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.FromRgb(22, 22, 35), 0),
                    new GradientStop(Color.FromRgb(42, 42, 70), 1)
                }
            },
            area
        );

        DrawTarget(context);
        DrawPlayer(context);
        DrawText(context);
    }

    private void DrawTarget(DrawingContext context)
    {
        context.DrawEllipse(
            Brushes.Transparent,
            new Pen(Brushes.White, 7),
            new Avalonia.Point(targetX, targetY),
            48,
            48
        );

        context.DrawEllipse(
            Brushes.Transparent,
            new Pen(Brushes.Red, 4),
            new Avalonia.Point(targetX, targetY),
            28,
            28
        );

        context.DrawEllipse(
            Brushes.Red,
            null,
            new Avalonia.Point(targetX, targetY),
            7,
            7
        );
    }

    private void DrawPlayer(DrawingContext context)
    {
        context.DrawEllipse(
            Brushes.Gold,
            new Pen(Brushes.Black, 3),
            new Avalonia.Point(mouseX, mouseY),
            25,
            25
        );
    }

    private void DrawText(DrawingContext context)
    {
        var scoreText = new FormattedText(
            $"Score: {score}",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial", FontStyle.Normal, FontWeight.Bold),
            32,
            Brushes.Yellow
        );

        context.DrawText(scoreText, new Avalonia.Point(20, 20));

        var helpText = new FormattedText(
            "Move your mouse/touchpad in a circle. Space — reset the score.",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            16,
            Brushes.White
        );

        context.DrawText(helpText, new Avalonia.Point(20, 62));
    }
}