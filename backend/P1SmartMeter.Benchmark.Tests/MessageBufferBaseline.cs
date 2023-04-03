using System;
using System.Text;

namespace P1SmartMeter
{
    public class MessageBufferBaseline
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly NLog.Logger LoggerP1Stream = NLog.LogManager.GetLogger("p1stream");
        private static readonly NLog.Logger LoggerP1Messages = NLog.LogManager.GetLogger("p1messages");

        public const int BufferCapacity = 2560;

        public event EventHandler<DataErrorEventArgs> DataError = delegate { };

        private readonly byte[] _buffer = new byte[BufferCapacity];
        private int _position;

        public int BufferUsed { get => _position; private set => _position = value; }
        public bool IsEmpty { get => _position == 0; }

        public int Add(string data)
        {

            var sb = new StringBuilder(data);
            while (sb.Length > 0)
            {
                var chrs = sb.Length < 512 ? sb.Length : 512;
                AddInternal(sb.ToString().Substring(0, chrs));
                sb.Remove(0, chrs);
            }
            return _position;
        }

        /// <summary>
        /// Internal function to add chunks of data to the buffer. The length of the string
        /// should never exceed the maximum length the buffer can contain
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected int AddInternal(string data)
        {
            ArgumentNullException.ThrowIfNull(data);

            LoggerP1Stream.Trace($"---------- start ----------{Environment.NewLine}{data}");
            LoggerP1Stream.Trace($"---------- end   ----------");


            if (data.Length == 0) return _position;
            if (data.Length > BufferCapacity) throw new ArgumentOutOfRangeException(nameof(data));

            Encoding ascii = Encoding.ASCII;

            while (BufferCapacity - _position < data.Length)
            {
                if (!TryTake(out _))
                {
                    _position = 0;
                }
                OnDataError(new DataErrorEventArgs("Buffer overflow, data purged from buffer"));
            }

            Buffer.BlockCopy(ascii.GetBytes(data), 0, _buffer, _position, data.Length);
            _position += data.Length;

            Logger.Debug($"bytes in buffer after adding {_position}, ");
            RemovePartialMessage();

            return _position;
        }

        public bool TryTake(out string? msg)
        {
            return RetrieveMessageFromBuffer(out msg);
        }

        private bool RetrieveMessageFromBuffer(out string? msg)
        {
            msg = null;
            bool bufferChanged;
            do
            {
                if (_position == 0) return false;
                RemovePartialMessage();

                bufferChanged = false;
                int s = FindEndOfMessage();

                if (s > 0)
                {
                    msg = HandleMessageInBuffer(s);
                    bufferChanged = true;
                }
            } while (bufferChanged && string.IsNullOrWhiteSpace(msg));
            return !string.IsNullOrWhiteSpace(msg);
        }

        private int FindEndOfMessage()
        {
            int s = 0;
            while (_buffer[s] != '!' && (s == 0 || (_buffer[s] != '/' && s > 0)) && s < _position)
                s++;

            // <!><#><#><#><#><CR><LF> -> 7 bytes
            if (_buffer[s] == '!' && s + 7 <= _position)
                return s + 7;
            if (_buffer[s] == '/' && s > 0 && s < _position)
                return s;

            return -1;
        }

        private string? HandleMessageInBuffer(int msgLength)
        {
            var mbytes = new byte[msgLength];
            var checksumbytes = new byte[4];
            Buffer.BlockCopy(_buffer, 0, mbytes, 0, msgLength);             // get message
            Buffer.BlockCopy(_buffer, msgLength - 6, checksumbytes, 0, 4);   // get checksum

            if (_position > msgLength)
            {
                Buffer.BlockCopy(_buffer, msgLength, _buffer, 0, _position - msgLength);
                _position -= msgLength;
                RemovePartialMessage();
            }
            else
            {
                // buffer is now empty
                _position = 0;
            }

            var msgChecksum = Encoding.ASCII.GetString(checksumbytes);
            var calculatedChecksum = CRC16.ComputeChecksumAsString(mbytes.AsSpan().Slice(0, mbytes.Length - 6));
            string? msg = null;

            if (string.Equals(msgChecksum, calculatedChecksum, StringComparison.OrdinalIgnoreCase))
            {
                msg = Encoding.ASCII.GetString(mbytes);

                LoggerP1Messages.Trace($"---------- start ----------{Environment.NewLine}{msg}");
                LoggerP1Messages.Trace($"---------- end   ----------");
            }
            else
            {
                var logmsg = Encoding.ASCII.GetString(mbytes);
                LoggerP1Messages.Error($"---------- start ---------- crc {msgChecksum} != {calculatedChecksum}{Environment.NewLine}{logmsg}");
                LoggerP1Messages.Error($"---------- end   ----------");
                // crc error
                OnDataError(new DataErrorEventArgs($"crc {msgChecksum} != {calculatedChecksum}"));
            }

            return msg;
        }

        private void RemovePartialMessage()
        {
            // check if buffer is empty
            if (_position == 0) return;

            int start = 0;
            while (_buffer[start] != '/' && start < _position)
                start++;

            if ((_buffer[start] == '/' && start > 0))
            {
                // discarding data
                Logger.Debug(@"discarding {start} bytes");
                Buffer.BlockCopy(_buffer, start, _buffer, 0, _position - start);
                _position -= start;
                Logger.Debug($"bytes in buffer {_position}, ");
                OnDataError(new DataErrorEventArgs("partial") );
            }
            else if (start == _position)
            {
                Logger.Debug(@"discarding all bytes");
                var tmp = new byte[_position];
                Buffer.BlockCopy(_buffer, 0, tmp, 0, _position);
                OnDataError(new DataErrorEventArgs("partial") { Data = Encoding.ASCII.GetString(tmp) });
                _position = 0;
            }
        }

        protected void OnDataError(DataErrorEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);
            Logger.Error($"data error {e.Message}");
            if (!string.IsNullOrWhiteSpace(e.Data))
                Logger.Error($"{Environment.NewLine}{e.Data}");

            EventHandler<DataErrorEventArgs> handler = DataError;

            handler?.Invoke(this, e);
        }
    }
}