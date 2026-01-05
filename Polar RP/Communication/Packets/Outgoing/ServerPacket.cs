using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Polar.Communication.Interfaces;

namespace Polar.Communication.Packets.Outgoing
{
    public class ServerPacket : IServerPacket, IDisposable
    {
        private readonly Encoding _encoding;
        private byte[] _buffer;
        private int _position;
        private bool _disposed;
        private readonly int _initialCapacity;

        public int Id { get; private set; }
        public int Length => _position;
        public int Capacity => _buffer.Length;

        private const int DefaultInitialCapacity = 64;
        private const int MaxPacketSize = 10000000; // 10MB máximo

        public ServerPacket(int id) : this(id, DefaultInitialCapacity)
        {
        }

        public ServerPacket(int id, int initialCapacity)
        {
            if (initialCapacity <= 0)
                initialCapacity = DefaultInitialCapacity;

            _encoding = Encoding.UTF8;
            _initialCapacity = initialCapacity;
            _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
            _position = 0;
            Id = id;

            // Escribir el ID al inicio (2 bytes, BIG-ENDIAN)
            WriteShort(id);
        }

        #region Core Write Methods - Optimizadas

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int additionalBytes)
        {
            int requiredCapacity = _position + additionalBytes;

            if (requiredCapacity > MaxPacketSize)
                throw new InvalidOperationException($"Packet size exceeds maximum allowed ({MaxPacketSize} bytes)");

            if (requiredCapacity <= _buffer.Length)
                return;

            // Duplicar tamaño o alcanzar capacidad requerida
            int newCapacity = Math.Max(_buffer.Length * 2, requiredCapacity);
            byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newCapacity);

            Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _position);
            ArrayPool<byte>.Shared.Return(_buffer);

            _buffer = newBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            ValidateNotDisposed();
            EnsureCapacity(1);
            _buffer[_position++] = value;
        }

        public void WriteByte(int value)
        {
            WriteByte((byte)value);
        }

        public void WriteBytes(byte[] bytes, bool isBigEndian = false)
        {
            ValidateNotDisposed();

            if (bytes == null || bytes.Length == 0)
                return;

            EnsureCapacity(bytes.Length);

            if (isBigEndian)
            {
                // Escribir en orden BIG-ENDIAN (byte más significativo primero)
                for (int i = bytes.Length - 1; i >= 0; i--)
                {
                    _buffer[_position++] = bytes[i];
                }
            }
            else
            {
                // Escribir en orden normal
                Buffer.BlockCopy(bytes, 0, _buffer, _position, bytes.Length);
                _position += bytes.Length;
            }
        }

        public void WriteBytes(ReadOnlySpan<byte> bytes, bool isBigEndian = false)
        {
            ValidateNotDisposed();

            if (bytes.Length == 0)
                return;

            EnsureCapacity(bytes.Length);

            if (isBigEndian)
            {
                for (int i = bytes.Length - 1; i >= 0; i--)
                {
                    _buffer[_position++] = bytes[i];
                }
            }
            else
            {
                bytes.CopyTo(new Span<byte>(_buffer, _position, bytes.Length));
                _position += bytes.Length;
            }
        }

        #endregion

        #region Data Type Methods

        public void WriteString(string value)
        {
            ValidateNotDisposed();

            if (string.IsNullOrEmpty(value))
            {
                WriteShort(0);
                return;
            }

            int byteCount = _encoding.GetByteCount(value);
            WriteShort(byteCount);

            if (byteCount == 0)
                return;

            EnsureCapacity(byteCount);

            // Codificar directamente al buffer
            int encodedBytes = _encoding.GetBytes(value, 0, value.Length, _buffer, _position);
            _position += encodedBytes;
        }

        public void WriteString(string value, int fixedLength)
        {
            ValidateNotDisposed();

            if (fixedLength <= 0)
                return;

            if (string.IsNullOrEmpty(value))
            {
                WriteBytes(new byte[fixedLength], false);
                return;
            }

            byte[] bytes = _encoding.GetBytes(value);
            int bytesToWrite = Math.Min(bytes.Length, fixedLength);

            EnsureCapacity(fixedLength);

            // Escribir bytes del string
            if (bytesToWrite > 0)
            {
                Buffer.BlockCopy(bytes, 0, _buffer, _position, bytesToWrite);
            }

            // Rellenar con ceros si es necesario
            int padding = fixedLength - bytesToWrite;
            if (padding > 0)
            {
                Array.Clear(_buffer, _position + bytesToWrite, padding);
            }

            _position += fixedLength;
        }

        public void WriteShort(int value)
        {
            ValidateNotDisposed();
            WriteShort((short)value);
        }

        public void WriteShort(short value)
        {
            ValidateNotDisposed();
            EnsureCapacity(2);

            // BIG-ENDIAN: byte más significativo primero
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)value;
        }

        public void WriteUShort(ushort value)
        {
            WriteShort((short)value);
        }

        public void WriteInteger(int value)
        {
            ValidateNotDisposed();
            EnsureCapacity(4);

            // BIG-ENDIAN: byte más significativo primero
            _buffer[_position++] = (byte)(value >> 24);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)value;
        }

        public void WriteUInteger(uint value)
        {
            WriteInteger((int)value);
        }

        public void WriteLong(long value)
        {
            ValidateNotDisposed();
            EnsureCapacity(8);

            // BIG-ENDIAN: byte más significativo primero
            _buffer[_position++] = (byte)(value >> 56);
            _buffer[_position++] = (byte)(value >> 48);
            _buffer[_position++] = (byte)(value >> 40);
            _buffer[_position++] = (byte)(value >> 32);
            _buffer[_position++] = (byte)(value >> 24);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)value;
        }

        public void WriteULong(ulong value)
        {
            WriteLong((long)value);
        }

        public void WriteBoolean(bool value)
        {
            WriteByte(value ? (byte)1 : (byte)0);
        }

        public void WriteDouble(double value, string format = "0.0")
        {
            ValidateNotDisposed();

            string formatted = value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
            WriteString(formatted);
        }

        public void WriteFloat(float value, string format = "0.0")
        {
            WriteDouble(value, format);
        }

        #endregion

        #region Array and Collection Methods

        public void WriteIntArray(int[] array)
        {
            ValidateNotDisposed();

            if (array == null)
            {
                WriteInteger(0);
                return;
            }

            WriteInteger(array.Length);
            foreach (int value in array)
            {
                WriteInteger(value);
            }
        }

        public void WriteStringArray(string[] array)
        {
            ValidateNotDisposed();

            if (array == null)
            {
                WriteInteger(0);
                return;
            }

            WriteInteger(array.Length);
            foreach (string value in array)
            {
                WriteString(value);
            }
        }

        public void WriteByteArray(byte[] array)
        {
            ValidateNotDisposed();

            if (array == null)
            {
                WriteInteger(0);
                return;
            }

            WriteInteger(array.Length);
            WriteBytes(array, false);
        }

        #endregion

        #region Packet Assembly

        public byte[] GetBytes()
        {
            ValidateNotDisposed();

            // Calcular tamaño total: 4 bytes (longitud) + cuerpo
            int totalLength = 4 + _position;

            if (totalLength > MaxPacketSize)
                throw new InvalidOperationException($"Packet too large: {totalLength} bytes");

            byte[] finalPacket = new byte[totalLength];

            // 1. Escribir longitud (4 bytes, BIG-ENDIAN)
            int bodyLength = _position;
            finalPacket[0] = (byte)(bodyLength >> 24);
            finalPacket[1] = (byte)(bodyLength >> 16);
            finalPacket[2] = (byte)(bodyLength >> 8);
            finalPacket[3] = (byte)bodyLength;

            // 2. Copiar cuerpo del paquete
            if (_position > 0)
            {
                Buffer.BlockCopy(_buffer, 0, finalPacket, 4, _position);
            }

            return finalPacket;
        }

        public byte[] GetBytesWithoutLength()
        {
            ValidateNotDisposed();

            // Versión para WebSocket que no necesita longitud
            byte[] packet = new byte[_position];
            Buffer.BlockCopy(_buffer, 0, packet, 0, _position);
            return packet;
        }

        public ReadOnlySpan<byte> GetBody()
        {
            ValidateNotDisposed();
            return new ReadOnlySpan<byte>(_buffer, 0, _position);
        }

        #endregion

        #region Utility Methods

        public void Clear()
        {
            ValidateNotDisposed();

            _position = 0;
            Id = 0;

            // Reescribir el ID
            WriteShort(Id);
        }

        public void Reset(int newId)
        {
            ValidateNotDisposed();

            _position = 0;
            Id = newId;

            // Reescribir el nuevo ID
            WriteShort(newId);
        }

        public void TrimExcess()
        {
            ValidateNotDisposed();

            if (_buffer.Length > _position * 2 && _buffer.Length > _initialCapacity)
            {
                byte[] newBuffer = ArrayPool<byte>.Shared.Rent(_position);
                Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _position);
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = newBuffer;
            }
        }

        public string ToDebugString()
        {
            if (_disposed)
                return $"[DISPOSED]";

            string hex = BitConverter.ToString(_buffer, 0, Math.Min(_position, 32));
            if (_position > 32)
                hex += "...";

            return $"[{Id}] Length: {_position} bytes | Hex: {hex}";
        }

        public string GetBodyAsString()
        {
            ValidateNotDisposed();

            if (_position == 0)
                return string.Empty;

            return _encoding.GetString(_buffer, 0, _position);
        }

        #endregion

        #region IDisposable Implementation

        private void ValidateNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ServerPacket));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = null;
            }

            _position = 0;
            Id = 0;
            _disposed = true;

            GC.SuppressFinalize(this);
        }

        ~ServerPacket()
        {
            Dispose();
        }

        #endregion

        #region Static Helper Methods

        public static byte[] CreateQuickPacket(int id, Action<ServerPacket> writer)
        {
            using var packet = new ServerPacket(id);
            writer?.Invoke(packet);
            return packet.GetBytes();
        }

        public static byte[] CreateQuickStringPacket(int id, string message)
        {
            using var packet = new ServerPacket(id);
            packet.WriteString(message);
            return packet.GetBytes();
        }

        public static byte[] EncodeShort(short value)
        {
            return new byte[]
            {
                (byte)(value >> 8),
                (byte)value
            };
        }

        public static byte[] EncodeInt(int value)
        {
            return new byte[]
            {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            };
        }

        public static byte[] EncodeLong(long value)
        {
            return new byte[]
            {
                (byte)(value >> 56),
                (byte)(value >> 48),
                (byte)(value >> 40),
                (byte)(value >> 32),
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            };
        }

        #endregion
    }
}