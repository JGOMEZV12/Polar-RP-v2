using System;
using System.Collections.Generic;
using System.Linq;

namespace Polar.Communication.WebSocket
{
    static class EncodeDecode
    {
        internal static byte[] EncodeMessage(byte[] message)
        {
            byte[] bytesRaw = message;
            byte[] frame = new byte[10];

            int indexStartRawData;
            int length = bytesRaw.Length;

            frame[0] = (byte)130; // FIN + Text frame
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (byte)126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (byte)127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            byte[] response = new byte[indexStartRawData + length];

            Int32 i, reponseIdx = 0;

            // Add the frame bytes to the response
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            // Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }

        internal static byte[] DecodeMessage(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 2)
                return null;

            try
            {
                byte secondByte = bytes[1];

                // VERIFICAR SI EL FRAME ESTÁ ENMASCARADO
                bool masked = (secondByte & 0x80) != 0;
                int dataLength = secondByte & 127; // Remover el bit MASK

                int indexFirstMask = 2;

                // Longitud extendida
                if (dataLength == 126)
                {
                    if (bytes.Length < 4) return null;
                    dataLength = (bytes[2] << 8) | bytes[3];
                    indexFirstMask = 4;
                }
                else if (dataLength == 127)
                {
                    if (bytes.Length < 10) return null;
                    // Para 64-bit length, tomamos solo los últimos 4 bytes (32-bit es suficiente)
                    dataLength = (bytes[6] << 24) | (bytes[7] << 16) | (bytes[8] << 8) | bytes[9];
                    indexFirstMask = 10;
                }

                // Verificar que tenemos suficientes datos
                int expectedLength = indexFirstMask + (masked ? 4 : 0) + dataLength;
                if (bytes.Length < expectedLength)
                    return null;

                byte[] decoded;

                if (masked)
                {
                    // Obtener la máscara
                    IEnumerable<byte> keys = bytes.Skip(indexFirstMask).Take(4);
                    int indexFirstDataByte = indexFirstMask + 4;

                    // Aplicar XOR con la máscara
                    decoded = new byte[dataLength];
                    for (int i = indexFirstDataByte, j = 0; i < indexFirstDataByte + dataLength; i++, j++)
                    {
                        decoded[j] = (byte)(bytes[i] ^ keys.ElementAt(j % 4));
                    }
                }
                else
                {
                    // Frame sin máscara - copiar datos directamente
                    int indexFirstDataByte = indexFirstMask;
                    decoded = new byte[dataLength];
                    Buffer.BlockCopy(bytes, indexFirstDataByte, decoded, 0, dataLength);
                }

                return decoded;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EncodeDecode] ERROR decodificando: {ex.Message}");
                return null;
            }
        }
    }
}