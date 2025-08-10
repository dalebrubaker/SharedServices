using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using BruSoftware.SharedServices;
using Xunit;

namespace BruSoftware.SharedServicesTests;

public class ZipCodecTests
{
    [Fact]
    public void RoundTripBytes()
    {
        var input = Enumerable.Repeat((byte)1, 1000).ToArray();
        var compressed = ZipCodec.Compress(input);
        var decompressed = ZipCodec.Decompress(compressed);
        Assert.Equal(input, decompressed);
    }

    [Fact]
    public async Task RoundTripBytesAsync()
    {
        var input = Enumerable.Repeat((byte)1, 1000).ToArray();
        var compressed = await ZipCodec.CompressAsync(input);
        var decompressed = await ZipCodec.DecompressAsync(compressed);
        Assert.Equal(input, decompressed);
    }

    [Fact]
    public void RoundTripBytesToFile()
    {
        var filePath = Path.GetTempFileName();
        var input = Enumerable.Repeat((byte)1, 1000).ToArray();
        ZipCodec.CompressToFile(input, filePath, CompressionLevel.Fastest);
        var decompressed = ZipCodec.DecompressFromFile(filePath);
        Assert.Equal(input, decompressed);
        File.Delete(filePath);
    }

    [Fact]
    public async Task RoundTripBytesToFileAsync()
    {
        var filePath = Path.GetTempFileName();
        var input = Enumerable.Repeat((byte)1, 1000).ToArray();
        await ZipCodec.CompressToFileAsync(input, filePath, CompressionLevel.Fastest);
        var decompressed = await ZipCodec.DecompressFromFileAsync(filePath);
        Assert.Equal(input, decompressed);
        File.Delete(filePath);
    }
}