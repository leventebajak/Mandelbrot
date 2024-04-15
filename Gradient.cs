using OpenCvSharp;

namespace Mandelbrot;

/// <summary>
/// The gradient class, used to define a color palette for the fractal.
/// </summary>
public class Gradient
{
    /// <summary>
    /// The default gradient.
    /// </summary>
    public static Gradient Default { get; } = new()
    {
        Colors =
        {
            // Black
            { 0.0, new Scalar(0, 0, 0) },
            // Blue
            { 0.03, new Scalar(0, 48, 160) },
            // Light blue
            { 0.06, new Scalar(0, 128, 255) },
            // White
            { 0.09, new Scalar(255, 255, 255) },
            // Yellow
            { 0.14, new Scalar(255, 255, 0) },
            // Orange
            { 0.19, new Scalar(255, 128, 0) },
            // Red
            { 0.5, new Scalar(255, 0, 0) },
            // Black
            { 1.0, new Scalar(0, 0, 0) }
        }
    };

    /// <summary>
    /// The colors in the gradient sorted by their value.
    /// </summary>
    private SortedDictionary<double, Scalar> Colors { get; } = new();


    /// <summary>
    /// Adds a color to the gradient.
    /// </summary>
    /// <param name="value">The value associated with the color.</param>
    /// <param name="color">The color to add.</param>
    public void AddColor(double value, Scalar color) => Colors[value] = color;

    /// <summary>
    /// Gets a color by linearly interpolating between the two closest colors.
    /// </summary>
    /// <param name="value">The value used to get the color.</param>
    /// <returns>The color in BGR format.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the palette has no colors.</exception>
    public Scalar GetColor(double value)
    {
        if (Colors.Count == 0)
            throw new InvalidOperationException("The palette has no colors.");
        if (value >= Colors.Last().Key)
            return Colors.Last().Value;
        if (value <= Colors.First().Key)
            return Colors.First().Value;

        var previous = Colors.First();
        foreach (var color in Colors)
        {
            if (color.Key >= value)
            {
                var alpha = (value - previous.Key) / (color.Key - previous.Key);
                // The result is returned in BGR format.
                return new Scalar(
                    (1d - alpha) * previous.Value[2] + alpha * color.Value[2],
                    (1d - alpha) * previous.Value[1] + alpha * color.Value[1],
                    (1d - alpha) * previous.Value[0] + alpha * color.Value[0]
                );
            }

            previous = color;
        }

        throw new InvalidOperationException("No color was found.");
    }
}