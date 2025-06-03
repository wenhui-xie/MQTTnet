namespace MQTTnet.Fuzz;

using MQTTnet.Formatter;
using MQTTnet.Exceptions;
using MQTTnet.Packets;
using MQTTnet.Adapter;
using System;

public static class MQTTnet_Fuzz
{
    public static void Decode_Encode(ReadOnlySpan<byte> input)
    {
        try
        {
            if (input.Length == 0) return;

            MqttProtocolVersion protocolVersion;

            if (input[0] > 0xAA)
            {
                protocolVersion = MqttProtocolVersion.V500;
            }
            else if (input[0] > 0x55)
            {
                protocolVersion = MqttProtocolVersion.V311;
            }
            else if (input[0] > 0x1)
            {
                protocolVersion = MqttProtocolVersion.V310;
            }
            else
            {
                protocolVersion = MqttProtocolVersion.Unknown;
            }

            ReadOnlySpan<byte> trimmedInput = input.Slice(1);
            var array = trimmedInput.ToArray();

            int totalLength = array.Length;
            byte fixedHeader;
            ArraySegment<byte> body;

            if (totalLength > 0)
            {
                fixedHeader = array[0];
                body = new ArraySegment<byte>(array, 1, totalLength - 1);
            }
            else
            {
                fixedHeader = 0;
                body = Array.Empty<byte>();
            }
            ReceivedMqttPacket receivedPacket = new ReceivedMqttPacket(fixedHeader, body, totalLength);

            MqttPacketFormatterAdapter serializer;

            if (protocolVersion == MqttProtocolVersion.Unknown)
            {
                serializer = new MqttPacketFormatterAdapter(new MqttBufferWriter(4096, 65535));
                serializer.DetectProtocolVersion(receivedPacket);
            }
            else
            {
                serializer = new MqttPacketFormatterAdapter(protocolVersion, new MqttBufferWriter(4096, 65535));
            }

            MqttPacket packet = serializer.Decode(receivedPacket);
            if (packet == null)  return;

            var packetBuffer = serializer.Encode(packet);
            packetBuffer.Join();
            packetBuffer.ToArray();
        }
        catch (Exception ex) when (ex is MqttProtocolViolationException)
        { // filter out expected exception from your code here
        }
    }
}
