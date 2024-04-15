using System.Numerics;
using OpenCvSharp;

namespace Mandelbrot;

/// <summary>
/// The class, which displays the fractal and handles user input. It is a subclass of &lt;see cref="Window"/&gt;.
/// </summary>
public class FractalWindow : Window
{
    /// <summary>
    /// Demonstrating the usage of the <see cref="FractalWindow"/> class by displaying the Mandelbrot fractal.
    /// </summary>
    public static void Main()
    {
        /*
         * Controls:
         *
         * Left click and drag: Move the image. (The image is re-rendered when the mouse is released.)
         * Ctrl + left click: Open a new window of the Julia set, with the clicked point as the constant.
         * Ctrl + scroll: Zoom in and out of the fractal at the mouse position.
         * F5: Reset the fractal to its default state.
         * + (Numpad): Zoom in at the center.
         * - (Numpad): Zoom out at the center.
         * Esc: Close the window.
         */
        new FractalWindow("Mandelbrot set", Fractals.Mandelbrot, new Size(800, 700), new Complex(-0.75, 0.0), 1.3)
            .StartListening();
    }

    /// <summary>
    /// The default center of the fractal, used when resetting the fractal to its default state.
    /// </summary>
    private Complex DefaultCenter { get; }

    /// <summary>
    /// The center of the fractal.
    /// </summary>
    private Complex Center { get; set; }

    /// <summary>
    /// The default zoom of the fractal, used when resetting the fractal to its default state.
    /// </summary>
    private double DefaultZoom { get; }

    /// <summary>
    /// The zoom multiplier of the fractal.
    /// </summary>
    private double Zoom { get; set; }

    /// <summary>
    /// The fractal delegate, which is used to calculate the iterations, and thus the color of each pixel.
    /// </summary>
    private Fractals.FractalDelegate FractalDelegate { get; }

    /// <summary>
    /// The size of the window.
    /// </summary>
    private Size Size { get; }

    /// <summary>
    /// The position of the mouse when the user started dragging the image.
    /// </summary>
    private Point? MouseDragStart { get; set; }

    /// <summary>
    /// The last rendered image.
    /// </summary>
    private Mat? LastRender { get; set; }

    /// <summary>
    /// Creates a new <see cref="FractalWindow"/> instance.
    /// </summary>
    /// <param name="name">The name of the window.</param>
    /// <param name="fractalDelegate">The fractal delegate, which is used to calculate the iterations of each pixel.</param>
    /// <param name="size">The size of the window.</param>
    /// <param name="center">The center of the fractal.</param>
    /// <param name="zoom">The zoom multiplier of the fractal.</param>
    /// <returns>The created <see cref="FractalWindow"/> instance.</returns>
    private FractalWindow(
        string name,
        Fractals.FractalDelegate fractalDelegate,
        Size size,
        Complex center = default,
        double zoom = 1.0
    ) : base(name)
    {
        FractalDelegate = fractalDelegate;
        Size = size;
        Center = center;
        DefaultCenter = center;
        Zoom = zoom;
        DefaultZoom = zoom;
        Render();
    }

    /// <summary>
    /// Rendering the fractal and displaying it in the window.
    /// </summary>
    private void Render()
    {
        LastRender?.Dispose();
        LastRender = Fractals.Render(FractalDelegate, Size, Center, Zoom);
        ShowImage(LastRender);
    }

    /// <summary>
    /// Handling mouse input.
    /// </summary>
    /// <param name="e">The type of the mouse event.</param>
    /// <param name="x">The x coordinate of the mouse.</param>
    /// <param name="y">The y coordinate of the mouse.</param>
    /// <param name="flags">The flags of the mouse event.</param>
    /// <param name="_">The user data. (Unused)</param>
    private void OnMouse(MouseEventTypes e, int x, int y, MouseEventFlags flags, IntPtr _)
    {
        // Holding the left mouse button and moving the mouse moves the image without re-rendering it.
        if (e == MouseEventTypes.MouseMove && flags.HasFlag(MouseEventFlags.LButton))
        {
            var img = LastRender!.Clone();
            var vector = new Point(x - MouseDragStart!.Value.X, y - MouseDragStart.Value.Y);
            var black = new Mat(img.Size(), MatType.CV_8UC3, Scalar.Black);

            var matrix = Cv2.GetRotationMatrix2D(default, 0, 1);
            matrix.At<double>(0, 2) = vector.X;
            matrix.At<double>(1, 2) = vector.Y;
            Cv2.WarpAffine(img, black, matrix, img.Size(), borderMode: BorderTypes.Constant);

            ShowImage(black);
            return;
        }

        switch (e)
        {
            // Saving the mouse position when the user starts dragging the image.
            case MouseEventTypes.LButtonDown:
                MouseDragStart = new Point(x, y);
                break;
            case MouseEventTypes.LButtonUp:
            {
                // Ctrl + left clicking on the Mandelbrot fractal opens a new window of the Julia fractal,
                // with the clicked point as the constant.
                if (FractalDelegate == Fractals.Mandelbrot && flags.HasFlag(MouseEventFlags.CtrlKey))
                {
                    new FractalWindow("Julia set",
                        z => Fractals.Julia(z, Fractals.PixelToComplex(Size, Center, Zoom, x, y)),
                        Size).StartListening();
                    break;
                }

                // When the user stops dragging the image, the center of the fractal is adjusted sand the image is re-rendered.
                Center += new Complex(
                    MouseDragStart!.Value.X - x,
                    MouseDragStart.Value.Y - y
                ) / Size.Width * 4 / Zoom;
                Render();
                break;
            }

            // Ctrl + scrolling zooms in and out of the fractal at the mouse position.
            case MouseEventTypes.MouseWheel:
                if (!flags.HasFlag(MouseEventFlags.CtrlKey))
                    break;
                Zoom *= flags > 0 ? 1.25 : 0.8;
                Center += new Complex((double)x / Size.Width - 0.5, (double)y / Size.Height - 0.5) / Zoom;
                Render();
                break;
        }
    }

    /// <summary>
    /// Handling key presses.
    /// </summary>
    /// <param name="key">The key code of the pressed key.</param>
    private void OnKeyPressed(int key)
    {
        switch (key)
        {
            // Esc closes the window.
            case Keys.Escape:
                Close();
                break;
            // F5 resets the fractal to its default state.
            case Keys.F5:
                Zoom = DefaultZoom;
                Center = DefaultCenter;
                Render();
                break;
            // + zooms in the fractal.
            case Keys.Plus:
                Zoom *= 1.25;
                Render();
                break;
            // - zooms out of the fractal.
            case Keys.Minus:
                Zoom *= 0.8;
                Render();
                break;
        }
    }

    /// <summary>
    /// Start listening for user input.
    /// </summary>
    /// <remarks>
    /// This method blocks the thread until the window is closed.
    /// </remarks>
    private void StartListening()
    {
        SetMouseCallback(OnMouse);
        while (!IsDisposed)
        {
            // Wait for a key press, blocking the thread.
            var key = Cv2.WaitKeyEx();
            if (IsDisposed)
                break;
            OnKeyPressed(key);
        }
    }

    /// <summary>
    /// The key codes of the keys, which are used in the &lt;see cref="OnKeyPressed"/&gt; method.
    /// </summary>
    private static class Keys
    {
        public const int Escape = 27;
        public const int F5 = 7602176;
        public const int Plus = 43;
        public const int Minus = 45;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        LastRender?.Dispose();
    }
}