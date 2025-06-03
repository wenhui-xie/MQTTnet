using System;
using System.IO;
using MQTTnet.Fuzz;
using Xunit;

public class MQTTnet_Fuzz_Tests
{
    [Fact]
    public void DecodeEncode_WithCrashBin_DoesNotThrow()
    {
        // Arrange
        var filePath = "../../../crash.bin";
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Test input file not found.", filePath);
        }

        byte[] input = File.ReadAllBytes(filePath);

        // Act & Assert
        Exception ex = Record.Exception(() => MQTTnet_Fuzz.Decode_Encode(input));
        // The method should not throw any unhandled exceptions
        Assert.Null(ex);
    }
}
