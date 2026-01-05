using System;
using System.Text;

namespace Polar.Communication.Packets.Incoming
{
    public class ClientPacket : IDisposable
    {
        private byte[] _body;
        private int _pointer;
        private readonly Encoding _encoding;
        private bool _disposed;

        public int Id { get; private set; }
        public int Header => Id;
        public int RemainingLength => _body.Length - _pointer;
        public int TotalLength => _body.Length;
        public int Position => _pointer;

        public ClientPacket(int messageId, byte[] body)
        {
            _encoding = Encoding.UTF8;
            Init(messageId, body);
        }

        public void Init(int messageId, byte[] body)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ClientPacket));

            Id = messageId;
            _body = body ?? Array.Empty<byte>();
            _pointer = 0;
        }

        #region Read Methods - Optimizadas

        public byte[] ReadBytes(int bytes)
        {
            ValidateNotDisposed();

            if (bytes <= 0 || bytes > RemainingLength)
                bytes = RemainingLength;

            if (bytes == 0)
                return Array.Empty<byte>();

            byte[] data = new byte[bytes];

            // Usar Buffer.BlockCopy para mejor performance en arrays grandes
            if (bytes > 8) // Threshold para usar BlockCopy vs loop manual
            {
                Buffer.BlockCopy(_body, _pointer, data, 0, bytes);
                _pointer += bytes;
            }
            else
            {
                // Loop manual para pocos bytes (más rápido)
                for (int i = 0; i < bytes; i++)
                    data[i] = _body[_pointer++];
            }

            return data;
        }

        public byte[] ReadFixedValue()
        {
            ValidateNotDisposed();

            if (RemainingLength < 2)
                return Array.Empty<byte>();

            short len = PopShort();
            return ReadBytes(len);
        }

        #endregion

        #region Data Type Methods

        public string PopString()
        {
            ValidateNotDisposed();

            byte[] bytes = ReadFixedValue();
            return bytes.Length > 0 ? _encoding.GetString(bytes) : string.Empty;
        }

        public string PopString(int length)
        {
            ValidateNotDisposed();

            if (length <= 0 || length > RemainingLength)
                return string.Empty;

            byte[] bytes = ReadBytes(length);
            return _encoding.GetString(bytes);
        }

        public bool PopBoolean()
        {
            ValidateNotDisposed();

            if (RemainingLength == 0)
                return false;

            // Leer y avanzar
            return _body[_pointer++] == 1;
        }

        public bool PopBoolean(int defaultValue)
        {
            ValidateNotDisposed();

            if (RemainingLength == 0)
                return defaultValue == 1;

            return _body[_pointer++] == 1;
        }

        public int PopInt()
        {
            ValidateNotDisposed();

            if (RemainingLength < 4)
                return 0;

            // Leer los 4 bytes directamente desde el array
            int result = (_body[_pointer] << 24) |
                         (_body[_pointer + 1] << 16) |
                         (_body[_pointer + 2] << 8) |
                         _body[_pointer + 3];

            _pointer += 4;
            return result;
        }

        public int PopInt(int defaultValue)
        {
            ValidateNotDisposed();

            if (RemainingLength < 4)
                return defaultValue;

            return PopInt();
        }

        public short PopShort()
        {
            ValidateNotDisposed();

            if (RemainingLength < 2)
                return 0;

            // Leer los 2 bytes directamente
            short result = (short)((_body[_pointer] << 8) | _body[_pointer + 1]);
            _pointer += 2;
            return result;
        }

        public short PopShort(short defaultValue)
        {
            ValidateNotDisposed();

            if (RemainingLength < 2)
                return defaultValue;

            return PopShort();
        }

        public byte PopByte()
        {
            ValidateNotDisposed();

            if (RemainingLength == 0)
                return 0;

            return _body[_pointer++];
        }

        public byte PopByte(byte defaultValue)
        {
            ValidateNotDisposed();

            if (RemainingLength == 0)
                return defaultValue;

            return _body[_pointer++];
        }

        public ushort PopUShort()
        {
            return (ushort)PopShort();
        }

        public uint PopUInt()
        {
            return (uint)PopInt();
        }

        #endregion

        #region Advanced Methods

        public byte[] ReadAllRemainingBytes()
        {
            ValidateNotDisposed();
            return ReadBytes(RemainingLength);
        }

        public void SkipBytes(int bytes)
        {
            ValidateNotDisposed();

            if (bytes <= 0)
                return;

            if (bytes > RemainingLength)
                bytes = RemainingLength;

            _pointer += bytes;
        }

        public void ResetPointer()
        {
            ValidateNotDisposed();
            _pointer = 0;
        }

        public void Seek(int position)
        {
            ValidateNotDisposed();

            if (position < 0)
                position = 0;
            else if (position > _body.Length)
                position = _body.Length;

            _pointer = position;
        }

        public byte PeekByte()
        {
            ValidateNotDisposed();

            if (RemainingLength == 0)
                return 0;

            return _body[_pointer];
        }

        public int PeekInt()
        {
            ValidateNotDisposed();

            if (RemainingLength < 4)
                return 0;

            return (_body[_pointer] << 24) |
                   (_body[_pointer + 1] << 16) |
                   (_body[_pointer + 2] << 8) |
                   _body[_pointer + 3];
        }

        public bool HasRemainingData(int requiredBytes)
        {
            ValidateNotDisposed();
            return RemainingLength >= requiredBytes;
        }

        #endregion

        #region Debug and Utility Methods

        public string GetBodyAsHex()
        {
            ValidateNotDisposed();
            return BitConverter.ToString(_body);
        }

        public string GetBodyAsString()
        {
            ValidateNotDisposed();
            return _encoding.GetString(_body);
        }

        public byte[] GetBodyCopy()
        {
            ValidateNotDisposed();
            byte[] copy = new byte[_body.Length];
            Buffer.BlockCopy(_body, 0, copy, 0, _body.Length);
            return copy;
        }

        public override string ToString()
        {
            if (_disposed)
                return $"[{Id}] DISPOSED";

            string bodyStr = _encoding.GetString(_body)
                .Replace("\0", "[0]")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");

            return $"[{Id}] Remaining: {RemainingLength}/{TotalLength} | Body: {bodyStr}";
        }

        public string ToDebugString()
        {
            if (_disposed)
                return $"[{Id}] DISPOSED";

            return $"[{Id}] Pos: {_pointer}/{_body.Length} | Hex: {BitConverter.ToString(_body, 0, Math.Min(32, _body.Length))}";
        }

        #endregion

        #region Static Helper Methods

        public static int DecodeInt32(byte[] v)
        {
            if (v == null || v.Length < 4)
                return 0;

            // BIG-ENDIAN
            return (v[0] << 24) | (v[1] << 16) | (v[2] << 8) | v[3];
        }

        public static short DecodeInt16(byte[] v)
        {
            if (v == null || v.Length < 2)
                return 0;

            // BIG-ENDIAN
            return (short)((v[0] << 8) | v[1]);
        }

        public static byte[] EncodeInt32(int value)
        {
            return new byte[]
            {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            };
        }

        public static byte[] EncodeInt16(short value)
        {
            return new byte[]
            {
                (byte)(value >> 8),
                (byte)value
            };
        }

        #endregion

        #region IDisposable Implementation

        private void ValidateNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ClientPacket), "Cannot access a disposed ClientPacket");
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _body = null;
            _pointer = 0;
            Id = 0;
            _disposed = true;

            GC.SuppressFinalize(this);
        }

        ~ClientPacket()
        {
            Dispose();
        }

        #endregion
    }
}