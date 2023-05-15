using System;
using System.Text;

namespace P1SmartMeter
{
    public class MessageBuffer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly NLog.Logger LoggerP1Stream = NLog.LogManager.GetLogger("p1stream");
        private static readonly NLog.Logger LoggerP1Messages = NLog.LogManager.GetLogger("p1messages");

        private const char StartChar = '/';
        private const char EndChar = '!';

        public const int BufferCapacity = 3072;

        public event EventHandler<DataErrorEventArgs> DataError = delegate { };

        private readonly char[] _buffer = new char[BufferCapacity];
        private int _tailPosition;

        public int BufferUsed { get => _tailPosition; private set => _tailPosition = value; }
        public bool IsEmpty { get => _tailPosition == 0; }

        public int Add(ReadOnlySpan<char> span)
        {
            while (span.Length > 0)
            {
                var chrs = span.Length < 1024 ? span.Length : 1024; // should normaly be enough for one complete message, looping for in the case there is more data
                AddInternal(span.Slice(0, chrs));
                span = span.Slice(chrs);
            }
            return _tailPosition;
        }

        public bool TryTake(out string? msg)
        {
            return RetrieveMessageFromBuffer(out msg);
        }

        /// <summary>
        /// Internal function to add chunks of data to the buffer. The length of the string
        /// should never exceed the maximum length the buffer can contain
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected int AddInternal(ReadOnlySpan<char> data)
        {
            if (LoggerP1Stream.IsTraceEnabled)
            {
                LoggerP1Stream.Trace("---------- start ----------{NewLine}{Data}", Environment.NewLine, new String(data));
                LoggerP1Stream.Trace("---------- end   ----------");
            }

            if (data.Length == 0) return _tailPosition;
            if (data.Length > BufferCapacity) throw new ArgumentOutOfRangeException(nameof(data));

            while (BufferCapacity - _tailPosition < data.Length)
            {
                if (!TryTake(out _))
                {
                    _tailPosition = 0;
                }
                OnDataError(new DataErrorEventArgs("buffer overflow, data purged from buffer"));
            }

            // add the data to the current tail position in the buffer
            var bspan = _buffer.AsSpan();
            data.CopyTo(bspan.Slice(_tailPosition));
            _tailPosition += data.Length;

            RemovePartialMessage();

            return _tailPosition;
        }

        private bool RetrieveMessageFromBuffer(out string? msg)
        {
            msg = null;
            bool bufferChanged;
            do
            {
                if (_tailPosition == 0) return false;
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

            // it can either end with the EndChar or the StartChar of a new message
            while (_buffer[s] != EndChar && ((_buffer[s] != StartChar && s > 0) || s == 0) && s < _tailPosition)
                s++;

            // Normal End
            // <!><#><#><#><#><CR><LF> -> 7 bytes
            if (_buffer[s] == EndChar && s + 7 <= _tailPosition)
                return s + 7;

            // End with the start of a new message
            if (_buffer[s] == StartChar && s > 0 && s < _tailPosition)
                return s;

            // no end found 
            return -1;
        }

        private string? HandleMessageInBuffer(int msgLength)
        {
            /* /xxx/ */
            if (msgLength <= 7)
            {
                // There is more data in the buffer to process. Move it to the beginning of the buffer
                _buffer.AsSpan(msgLength, _tailPosition - msgLength).CopyTo(_buffer.AsSpan());
                _tailPosition -= msgLength;
                RemovePartialMessage();
                return string.Empty;
            }

            var msg = new String(_buffer.AsSpan(0, msgLength));             // get message

            // TODO: validate if checksumbytes contain valid checksum characters
            Span<char> checksumbytes = stackalloc char[4];                  // get checksum
            checksumbytes[0] = _buffer[msgLength - 6];
            checksumbytes[1] = _buffer[msgLength - 5];
            checksumbytes[2] = _buffer[msgLength - 4];
            checksumbytes[3] = _buffer[msgLength - 3];

            if (_tailPosition > msgLength)
            {
                // There is more data in the buffer to process. Move it to the beginning of the buffer
                _buffer.AsSpan(msgLength, _tailPosition - msgLength).CopyTo(_buffer.AsSpan());
                _tailPosition -= msgLength;
                RemovePartialMessage();
            }
            else
            {
                // buffer is now empty
                _tailPosition = 0;
            }

            var calculatedChecksum = CRC16.ComputeChecksumAsString(msg.AsSpan().Slice(0, msg.Length - 6));
            string? retvalMsg = null;

            if (calculatedChecksum.AsSpan().CompareTo(checksumbytes, StringComparison.OrdinalIgnoreCase) == 0)
            {
                retvalMsg = msg;
                if (LoggerP1Messages.IsTraceEnabled)
                {
                    LoggerP1Messages.Trace($"---------- start ----------{Environment.NewLine}{msg}");
                    LoggerP1Messages.Trace($"---------- end   ----------");
                }
            }
            else
            {
                if (LoggerP1Messages.IsErrorEnabled)
                {
                    LoggerP1Messages.Error($"---------- start ---------- crc {checksumbytes} != {calculatedChecksum.AsSpan()}{Environment.NewLine}{retvalMsg}");
                    LoggerP1Messages.Error($"---------- end   ----------");
                }
                // crc error
                OnDataError(new DataErrorEventArgs($"crc error. Expected 0x{checksumbytes} and calculated 0x{calculatedChecksum.AsSpan()}") { Data = msg });
            }

            return retvalMsg;
        }

        private void RemovePartialMessage()
        {
            // check if buffer is empty
            if (_tailPosition == 0) return;

            int start = 0;
            while (_buffer[start] != StartChar && start < _tailPosition)
                start++;

            if (_buffer[start] == StartChar && start > 0)
            {
                if (Logger.IsDebugEnabled) Logger.Debug("discarding {Start} bytes", start);
                var discardedData = new String(_buffer.AsSpan(0, _tailPosition));

                _buffer.AsSpan(start, _tailPosition - start).CopyTo(_buffer.AsSpan());
                _tailPosition -= start;
                if (Logger.IsDebugEnabled) Logger.Debug("bytes in buffer {Position}, ", _tailPosition);
                OnDataError(new DataErrorEventArgs("Partial message removed from buffer") { Data = discardedData });
            }
            else if (start == _tailPosition)
            {
                if (Logger.IsDebugEnabled) Logger.Debug("discarding all bytes");
                var discardedData = new String(_buffer.AsSpan(0, _tailPosition));
                _tailPosition = 0;
                OnDataError(new DataErrorEventArgs("Partial message removed from buffer") { Data = discardedData });
            }
        }

        private void OnDataError(DataErrorEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);
            Logger.Error("Data error {Message}", e.Message);
            if (!string.IsNullOrWhiteSpace(e.Data))
                Logger.Error("{NewLine}{Data}{NewLine}", Environment.NewLine, e.Data, Environment.NewLine);

            DataError.Invoke(this, e);
        }
    }

    public class DataErrorEventArgs : EventArgs
    {
        public string Message { get; init; }
        public string? Data { get; init; }
        public DataErrorEventArgs(string message)
        {
            Message = message;
        }
    }
}