using System.Buffers.Binary;
using System.IO.Compression;

namespace SumireCraft.Maui.UITests;

/// <summary>
/// The pixels of a screenshot, read from the PNG the drivers send.
/// </summary>
/// <remarks>
/// Enough of PNG for what the drivers produce -- eight bits a channel, RGB or RGBA, not interlaced --
/// so that a test can look at what was drawn without an imaging package. Anything else is refused
/// rather than misread.
/// </remarks>
internal sealed class Png
{
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];

    private readonly byte[] _pixels;
    private readonly int _channels;

    private Png(int width, int height, int channels, byte[] pixels)
    {
        Width = width;
        Height = height;
        _channels = channels;
        _pixels = pixels;
    }

    public int Width { get; }

    public int Height { get; }

    public static Png Read(byte[] file)
    {
        if (!file.AsSpan(0, Signature.Length).SequenceEqual(Signature))
        {
            throw new InvalidDataException("Not a PNG.");
        }

        int width = 0, height = 0, channels = 0;
        using var compressed = new MemoryStream();

        for (var at = Signature.Length; at < file.Length;)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(file.AsSpan(at));
            var type = System.Text.Encoding.ASCII.GetString(file, at + 4, 4);
            var data = file.AsSpan(at + 8, length);

            if (type == "IHDR")
            {
                width = BinaryPrimitives.ReadInt32BigEndian(data);
                height = BinaryPrimitives.ReadInt32BigEndian(data[4..]);
                var (bitDepth, colourType, interlace) = (data[8], data[9], data[12]);
                if (bitDepth != 8 || interlace != 0)
                {
                    throw new InvalidDataException($"A PNG of {bitDepth} bits a channel, interlace {interlace}, is not read here.");
                }

                channels = colourType switch
                {
                    2 => 3,
                    6 => 4,
                    _ => throw new InvalidDataException($"PNG colour type {colourType} is not read here."),
                };
            }
            else if (type == "IDAT")
            {
                compressed.Write(data);
            }

            at += 12 + length;
        }

        compressed.Position = 0;
        using var inflated = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionMode.Decompress))
        {
            zlib.CopyTo(inflated);
        }

        return new Png(width, height, channels, Unfilter(inflated.ToArray(), width, height, channels));
    }

    /// <summary>
    /// How far across the picture its light pixels lie on average, from 0 at the left edge to 1 at
    /// the right: where white text on a coloured badge has most of its ink.
    /// </summary>
    /// <remarks>
    /// Weighed by the green channel above its middle, which white text has in full and the red, blue
    /// or dark fills a badge is drawn in barely have.
    /// </remarks>
    public double LightCentre()
    {
        double sum = 0, weight = 0;

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var green = _pixels[((y * Width) + x) * _channels + 1];
                var w = Math.Max(0, green - 128);
                sum += w * (x + 0.5);
                weight += w;
            }
        }

        return weight == 0 ? double.NaN : sum / weight / Width;
    }

    /// <summary>
    /// The colour of one pixel.
    /// </summary>
    public (byte Red, byte Green, byte Blue) ColourAt(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(x),
                $"({x}, {y}) is outside the picture, which is {Width} by {Height}.");
        }

        var at = ((y * Width) + x) * _channels;

        return (_pixels[at], _pixels[at + 1], _pixels[at + 2]);
    }

    private static byte[] Unfilter(byte[] data, int width, int height, int channels)
    {
        var stride = width * channels;
        var pixels = new byte[stride * height];

        for (var y = 0; y < height; y++)
        {
            var filter = data[y * (stride + 1)];
            var source = (y * (stride + 1)) + 1;
            var row = y * stride;

            for (var i = 0; i < stride; i++)
            {
                var left = i >= channels ? pixels[row + i - channels] : 0;
                var up = y > 0 ? pixels[row - stride + i] : 0;
                var upLeft = i >= channels && y > 0 ? pixels[row - stride + i - channels] : 0;

                var predictor = filter switch
                {
                    0 => 0,
                    1 => left,
                    2 => up,
                    3 => (left + up) / 2,
                    4 => Paeth(left, up, upLeft),
                    _ => throw new InvalidDataException($"PNG filter {filter} does not exist."),
                };

                pixels[row + i] = (byte)(data[source + i] + predictor);
            }
        }

        return pixels;
    }

    private static int Paeth(int a, int b, int c)
    {
        var p = a + b - c;
        var pa = Math.Abs(p - a);
        var pb = Math.Abs(p - b);
        var pc = Math.Abs(p - c);

        return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
    }
}
