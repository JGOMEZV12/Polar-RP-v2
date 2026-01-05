using Polar.Communication.Packets.Incoming;
using Polar.Communication.WebSocket;
using Polar.HabboHotel.GameClients;
using Polar.Utilities;
using SharedPacketLib;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Polar.Net
{
    public class GamePacketParser : IDataParser, IDisposable, ICloneable
    {
        private GameClient currentClient;

        public event HandlePacket OnNewPacket;

        private bool _halfDataRecieved = false;
        private byte[] _halfData = null;
        private bool _isWebSocket = false;
        private byte[] _webSocketBuffer = null;

        public GamePacketParser(GameClient me)
        {
            this.currentClient = me;
            this.OnNewPacket = null;
            this._webSocketBuffer = new byte[0];
        }

        public void handlePacketData(byte[] Data, bool deciphered = false)
        {
            try
            {
                if (this.OnNewPacket == null || Data == null || Data.Length == 0)
                    return;

                // DEBUG: Solo mostrar paquetes pequeños
                if (Data.Length <= 32)
                {
                    //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Recibidos {Data.Length} bytes: {BitConverter.ToString(Data)}");
                }

                // 1. Detectar handshake WebSocket
                if (Data[0] == 71 && Data[1] == 69) // "GE" de "GET"
                {
                    PolicyRequest(Data);
                    this._isWebSocket = true;
                    this.currentClient.GetConnection().IsWebSocket = true;
                    return;
                }

                // 2. Detectar política Flash
                if (Data.Length == 23 || Data.Length == 22)
                {
                    if (Data[0] == 60 && Data[1] == 112) // "<p"
                    {
                        this.currentClient.GetConnection().SendData(Encoding.Default.GetBytes(GetXmlPolicy()));
                        return;
                    }
                }

                // 3. Para WebSocket: usar buffer acumulativo
                byte[] dataToProcess = Data;

                if (this._isWebSocket)
                {
                    // Acumular datos en buffer WebSocket
                    byte[] newBuffer = new byte[_webSocketBuffer.Length + Data.Length];
                    Buffer.BlockCopy(_webSocketBuffer, 0, newBuffer, 0, _webSocketBuffer.Length);
                    Buffer.BlockCopy(Data, 0, newBuffer, _webSocketBuffer.Length, Data.Length);
                    _webSocketBuffer = newBuffer;

                    // Intentar decodificar frames completos
                    int totalProcessed = 0;
                    int bufferPosition = 0;

                    while (bufferPosition < _webSocketBuffer.Length)
                    {
                        // Intentar decodificar un frame
                        byte[] decodedFrame = DecodeSingleWebSocketFrame(_webSocketBuffer, bufferPosition, out int frameLength);

                        if (decodedFrame != null && frameLength > 0)
                        {
                            // Procesar el frame decodificado
                            ProcessDecodedData(decodedFrame);
                            totalProcessed++;
                            bufferPosition += frameLength;
                        }
                        else
                        {
                            // Frame incompleto, salir del loop
                            break;
                        }
                    }

                    // Guardar datos no procesados
                    if (bufferPosition > 0)
                    {
                        if (bufferPosition >= _webSocketBuffer.Length)
                        {
                            _webSocketBuffer = new byte[0];
                        }
                        else
                        {
                            byte[] remaining = new byte[_webSocketBuffer.Length - bufferPosition];
                            Buffer.BlockCopy(_webSocketBuffer, bufferPosition, remaining, 0, remaining.Length);
                            _webSocketBuffer = remaining;
                        }
                    }

                    if (totalProcessed > 0)
                        return;
                    else
                        return; // Esperar más datos
                }

                // 4. Para TCP normal: procesar normalmente
                ProcessNormalData(Data, deciphered);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR en handlePacketData: {e.Message}");
                Console.WriteLine(e.StackTrace);

                // Reset en caso de error
                this._halfDataRecieved = false;
                this._halfData = null;
                this._webSocketBuffer = new byte[0];
            }
        }

        private byte[] DecodeSingleWebSocketFrame(byte[] data, int startPosition, out int frameLength)
        {
            frameLength = 0;

            if (data.Length - startPosition < 2)
                return null;

            try
            {
                byte secondByte = data[startPosition + 1];
                bool masked = (secondByte & 0x80) != 0;
                int dataLength = secondByte & 127;

                int indexFirstMask = 2;

                // Longitud extendida
                if (dataLength == 126)
                {
                    if (data.Length - startPosition < 4) return null;
                    dataLength = (data[startPosition + 2] << 8) | data[startPosition + 3];
                    indexFirstMask = 4;
                }
                else if (dataLength == 127)
                {
                    if (data.Length - startPosition < 10) return null;
                    // Solo necesitamos 32 bits
                    dataLength = (data[startPosition + 6] << 24) |
                                 (data[startPosition + 7] << 16) |
                                 (data[startPosition + 8] << 8) |
                                 data[startPosition + 9];
                    indexFirstMask = 10;
                }

                // Calcular longitud total del frame
                frameLength = indexFirstMask + (masked ? 4 : 0) + dataLength;

                if (data.Length - startPosition < frameLength)
                    return null;

                byte[] decoded;

                if (masked)
                {
                    // Obtener máscara
                    int maskStart = startPosition + indexFirstMask;
                    byte[] mask = new byte[4];
                    Buffer.BlockCopy(data, maskStart, mask, 0, 4);

                    // Decodificar aplicando XOR
                    decoded = new byte[dataLength];
                    int dataStart = maskStart + 4;

                    for (int i = 0; i < dataLength; i++)
                    {
                        decoded[i] = (byte)(data[dataStart + i] ^ mask[i % 4]);
                    }
                }
                else
                {
                    // Sin máscara
                    decoded = new byte[dataLength];
                    Buffer.BlockCopy(data, startPosition + indexFirstMask, decoded, 0, dataLength);
                }

                return decoded;
            }
            catch
            {
                return null;
            }
        }

        private void ProcessDecodedData(byte[] decodedData)
        {
            //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] WebSocket decodificado: {decodedData.Length} bytes");

            // DEBUG: Mostrar primeros bytes
            if (decodedData.Length >= 8)
            {
                //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Primeros bytes: {BitConverter.ToString(decodedData, 0, Math.Min(16, decodedData.Length))}");
            }

            // Procesar como datos normales
            ProcessNormalData(decodedData, true);
        }

        private void ProcessNormalData(byte[] Data, bool deciphered)
        {
            // Manejar datos parciales
            if (this._halfDataRecieved)
            {
                byte[] FullDataRcv = new byte[this._halfData.Length + Data.Length];
                Buffer.BlockCopy(this._halfData, 0, FullDataRcv, 0, this._halfData.Length);
                Buffer.BlockCopy(Data, 0, FullDataRcv, this._halfData.Length, Data.Length);

                this._halfDataRecieved = false;
                this._halfData = null;

                // Procesar datos combinados
                ProcessNormalData(FullDataRcv, true);
                return;
            }

            // Aplicar RC4 si es necesario (solo para TCP, no WebSocket)
            if (currentClient is { RC4Client: { } } && !deciphered && !_isWebSocket)
            {
                currentClient.RC4Client.Decrypt(ref Data);
                deciphered = true;
            }

            int position = 0;

            while (position < Data.Length)
            {
                // Necesitamos al menos 4 bytes para la longitud
                if (Data.Length - position < 4)
                {
                    // Guardar datos parciales
                    this._halfData = new byte[Data.Length - position];
                    Buffer.BlockCopy(Data, position, this._halfData, 0, this._halfData.Length);
                    this._halfDataRecieved = true;
                    break;
                }

                // LEER LONGITUD - Big Endian (según HabboEncoding)
                uint packetLength = (uint)HabboEncoding.DecodeInt32(new byte[] {
                    Data[position], Data[position + 1], Data[position + 2], Data[position + 3]
                });

                // Validar longitud
                if (packetLength < 2 || packetLength > 1000000)
                {
                    // Intentar Little Endian como fallback
                    packetLength = BitConverter.ToUInt32(Data, position);

                    if (packetLength < 2 || packetLength > 1000000)
                    {
                        // DEBUG: Mostrar bytes problemáticos
                        //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Bytes longitud inválida: " +
                        //                 $"{Data[position]:X2} {Data[position + 1]:X2} " +
                         //                $"{Data[position + 2]:X2} {Data[position + 3]:X2}");

                        // Buscar próximo paquete
                        position++;
                        continue;
                    }
                }

                // Verificar paquete completo
                if (Data.Length - position - 4 < packetLength)
                {
                    // Paquete incompleto
                    this._halfData = new byte[Data.Length - position];
                    Buffer.BlockCopy(Data, position, this._halfData, 0, this._halfData.Length);
                    this._halfDataRecieved = true;
                    break;
                }

                // Extraer paquete
                byte[] Packet = new byte[packetLength];
                Buffer.BlockCopy(Data, position + 4, Packet, 0, (int)packetLength);

                // Parsear header
                if (Packet.Length >= 2)
                {
                    ushort Header = (ushort)HabboEncoding.DecodeInt16(Packet);

                    byte[] Content = new byte[Packet.Length - 2];
                    if (Content.Length > 0)
                    {
                        Buffer.BlockCopy(Packet, 2, Content, 0, Content.Length);
                    }

                    // Mostrar paquetes importantes
                    if (Header != 2596 && Header != 21) // Filtrar pings/keep-alive
                    {
                        string username = currentClient.GetHabbo()?.Username ?? "nuevo";
                        //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ [{username}] Header={Header}, Longitud={packetLength}");
                    }

                    ClientPacket Message = new ClientPacket(Header, Content);
                    OnNewPacket.Invoke(Message);
                }

                // Avanzar posición
                position += 4 + (int)packetLength;
            }
        }

        private static string GetXmlPolicy()
        {
            return "<?xml version=\"1.0\"?>\r\n" +
                   "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n" +
                   "<cross-domain-policy>\r\n" +
                   "<allow-access-from domain=\"*\" to-ports=\"1-31111\" />\r\n" +
                   "</cross-domain-policy>\x0";
        }

        private void PolicyRequest(byte[] packet)
        {
            string data = Encoding.UTF8.GetString(packet);

            if (!data.Contains("ey:"))
                return;

            string key = data.Replace("ey:", "`")
                      .Split('`')[1]
                      .Replace("\r", "").Split('\n')[0]
                      .Trim();

            string longKey = key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            SHA1 sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(Encoding.ASCII.GetBytes(longKey));
            string acceptKey = Convert.ToBase64String(hashBytes);

            string response = "HTTP/1.1 101 Switching Protocols\r\n" +
                             "Upgrade: websocket\r\n" +
                             "Connection: Upgrade\r\n" +
                             "Sec-WebSocket-Accept: " + acceptKey + "\r\n\r\n";

            currentClient.GetConnection().SendData(Encoding.UTF8.GetBytes(response));

            //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Handshake WebSocket completado");
        }

        public void Dispose()
        {
            this.OnNewPacket = null;
            GC.SuppressFinalize(this);
        }

        public object Clone()
        {
            return new GamePacketParser(this.currentClient);
        }

        public delegate void HandlePacket(ClientPacket message);
    }
}