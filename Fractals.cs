using System.Numerics;
using OpenCvSharp;

namespace Mandelbrot;

/// <summary>
/// The class containing the Mandelbrot and Julia functions as well as the fractal image rendering function.
/// </summary>
public abstract class Fractals
{
    /// <summary>
    /// The maximum number of iterations. The higher the number, the more detailed the image.
    /// </summary>
    private const uint MaxIterations = 256U;

    /// <summary>
    /// The maximum magnitude of the complex number.
    /// If the magnitude of the complex number is greater than this value,
    /// the fractal function will return <see cref="MaxIterations"/>.
    /// </summary>
    private const uint MaxMagnitude = 65536U;

    /// <summary>
    /// The gradient used to color the fractal image.
    /// </summary>
    private static Gradient Gradient => Gradient.Default;

    /// <summary>
    /// The fractal function delegate.
    /// </summary>
    /// <param name="c">The coordinate in the complex plane.</param>
    /// <returns>The number of iterations it took for the complex number to escape.</returns>
    public delegate double FractalDelegate(Complex c);

    /// <summary>
    /// The Mandelbrot fractal function.
    /// </summary>
    /// <param name="c">The coordinate in the complex plane.</param>
    /// <returns>The number of iterations it took for the complex number to escape.</returns>
    public static double Mandelbrot(Complex c)
    {
        Complex z = default;
        double iterations = 0;

        while (z.Magnitude <= MaxMagnitude && iterations < MaxIterations)
        {
            z = z * z + c;
            iterations++;
        }

        if (iterations >= MaxIterations)
            return iterations;

        // Interpolation
        return iterations + 1 - Math.Log(Math.Log(z.Magnitude) / 2 / Math.Log(2)) / Math.Log(2);
    }

    /// <summary>
    /// The Julia fractal function.
    /// </summary>
    /// <param name="z">The coordinate in the complex plane.</param>
    /// <param name="c">The "seed" of the fractal.</param>
    /// <returns>The number of iterations it took for the complex number to escape.</returns>
    public static double Julia(Complex z, Complex c)
    {
        double iterations = 0;

        while (z.Magnitude <= MaxMagnitude && iterations < MaxIterations)
        {
            z = z * z + c;
            iterations++;
        }

        if (iterations >= MaxIterations)
            return iterations;

        // Interpolation
        return iterations + 1 - Math.Log(Math.Log(z.Magnitude) / 2 / Math.Log(2)) / Math.Log(2);
    }

    /// <summary>
    /// Parallelized rendering of the fractal image.
    /// </summary>
    /// <param name="getIterations">The fractal function, which returns the number of iterations.</param>
    /// <param name="size">The size of the image.</param>
    /// <param name="center">The center of the image in the complex plane.</param>
    /// <param name="zoom">The zoom multiplier of the image.</param>
    /// <returns>The fractal image.</returns>
    public static Mat Render(
        FractalDelegate getIterations,
        Size size,
        Complex center = default,
        double zoom = 1.0
    )
    {
        var img = new Mat<Vec3b>(size);
        var indexer = img.GetIndexer();

        Parallel.For(0, size.Width, px =>
        {
            Parallel.For(0, size.Height, py =>
            {
                var c = PixelToComplex(size, center, zoom, px, py);

                indexer[py, px] = Gradient.GetColor(getIterations(c) / MaxIterations).ToVec3b();
            });
        });

        return img;
    }

    /// <summary>
    /// Finding the complex number corresponding to a pixel.
    /// </summary>
    /// <param name="size">The size of the image.</param>
    /// <param name="center">The center of the image in the complex plane.</param>
    /// <param name="zoom">The zoom multiplier of the image.</param>
    /// <param name="px">The x coordinate of the pixel.</param>
    /// <param name="py">The y coordinate of the pixel.</param>
    /// <returns>The complex number corresponding to the pixel.</returns>
    public static Complex PixelToComplex(Size size, Complex center, double zoom, int px, int py)
    {
        return new Complex(
            px - size.Width / 2.0,
            py - size.Height / 2.0
        ) / size.Width * 4 / zoom + center;
    }
}