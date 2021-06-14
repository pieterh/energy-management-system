using System;
using System.Text;

namespace P1SmartMeter
{
    public class MessageBuffer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly NLog.Logger LoggerP1Stream = NLog.LogManager.GetLogger("p1stream");
        private static readonly NLog.Logger LoggerP1Messages = NLog.LogManager.GetLogger("p1messages");

        private static readonly CRC16 crc16 = new();

        public event EventHandler<DataErrorEventArgs> DataError;

        private readonly static int bufferCapacity = 2560; 
        private readonly byte[] buffer = new byte[BufferCapacity];
        private int position = 0;

        public static int BufferCapacity => bufferCapacity;

        public int BufferUsed { get => position; private set => position = value; }

        public bool IsEmpty {  get => position == 0; }

        public int Add(string data)
        {
            
            var sb = new StringBuilder(data);
            while(sb.Length > 0)
            {
                var chrs = sb.Length < 512 ? sb.Length : 512;
                AddInternal(sb.ToString().Substring(0, chrs));
                sb.Remove(0, chrs);
            }
            return position;
        }

        /// <summary>
        /// Internal function to add chunks of data to the buffer. The length of the string
        /// should never exceed the maximum length the buffer can contain
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected int AddInternal(string data)
        {
            if (LoggerP1Stream.IsTraceEnabled)
            {
                LoggerP1Stream.Trace($"---------- start ----------{Environment.NewLine}{data}");
                LoggerP1Stream.Trace($"---------- end   ----------");
            }

            if (data.Length == 0) return position;
            if (data.Length > BufferCapacity) throw new ArgumentOutOfRangeException(nameof(data));

            Encoding ascii = Encoding.ASCII;

            while (BufferCapacity - position < data.Length)
            {
                if (!TryTake(out string removedMessage))
                {
                    position = 0;
                }
                OnDataError(new DataErrorEventArgs() { Message = "buffer overflow, data purged from buffer" });
            }

            Buffer.BlockCopy(ascii.GetBytes(data), 0, buffer, position, data.Length);
            position += data.Length;

            Logger.Debug($"bytes in buffer after adding {position}, ");
            RemovePartialMessage();
            
            return position;
        }

        public bool TryTake(out string msg)
        {           
            return RetrieveMessageFromBuffer(out msg);
        }

        private bool RetrieveMessageFromBuffer(out string msg)
        {
            msg = null;
            bool bufferChanged;                   
            do
            {
                if (position == 0) return false;
                RemovePartialMessage();

                bufferChanged = false;
                int s = FindEndOfMessage();
                
                if (s > 0)
                {
                    if (buffer[s-1] == '/')
                    {
                        // we have a partial start of a message, followed with a new start of a message
                        // get rid of first partial message
                        Buffer.BlockCopy(buffer, s, buffer, 0, position - s);
                        position -= s;
                        // partial removed error
                        OnDataError(new DataErrorEventArgs() { Message = "partial" });
                        bufferChanged = true;
                    }
                    else { 
                        msg = HandleMessageInBuffer(msg, s);
                        bufferChanged = true;
                    }
                }
            } while (bufferChanged && string.IsNullOrWhiteSpace(msg));
            return !string.IsNullOrWhiteSpace(msg);
        }

        private int FindEndOfMessage()
        {
            int s = 0;
            while (buffer[s] != '!' && (s == 0 || (buffer[s] != '/' && s > 0)) && s < position)
                s++;

            // <!><#><#><#><#><CR><LF> -> 7 bytes
            if (buffer[s] == '!' && s + 7 <= position)
                return s+7;   
            if (buffer[s] == '/' && s > 0 && s <position)
                return s;

             return -1;
        }

        private string HandleMessageInBuffer(string msg, int msgLength)
        {
            var mbytes = new byte[msgLength ];
            var checksumbytes = new byte[4];
            Buffer.BlockCopy(buffer, 0, mbytes, 0, msgLength );             // get message
            Buffer.BlockCopy(buffer, msgLength - 6, checksumbytes, 0, 4);   // get checksum

            if (position > msgLength )
            {
                Buffer.BlockCopy(buffer, msgLength , buffer, 0, position - msgLength );
                position -= msgLength ;
                RemovePartialMessage();
            }
            else
            {
                // buffer is now empty
                position = 0;
            }

            var msgChecksum = Encoding.ASCII.GetString(checksumbytes);
            var calculatedChecksum = crc16.ComputeChecksumAsString(mbytes, mbytes.Length - 6);

            if (string.Equals(msgChecksum, calculatedChecksum, StringComparison.OrdinalIgnoreCase))
            {
                msg = Encoding.ASCII.GetString(mbytes);
                if (LoggerP1Messages.IsTraceEnabled)
                {
                    LoggerP1Messages.Trace($"---------- start ----------{Environment.NewLine}{msg}");
                    LoggerP1Messages.Trace($"---------- end   ----------");
                }
            }
            else
            {
                var logmsg = Encoding.ASCII.GetString(mbytes);
                LoggerP1Messages.Error($"---------- start ---------- crc {msgChecksum} != {calculatedChecksum}{Environment.NewLine}{logmsg}");
                LoggerP1Messages.Error($"---------- end   ----------");
                // crc error
                OnDataError(new DataErrorEventArgs() { Message = $"crc {msgChecksum} != {calculatedChecksum}" });
            }

            return msg;
        }

        private void RemovePartialMessage()
        {
            // check if buffer is empty
            if (position == 0) return;

            int start = 0;
            while (buffer[start] != '/' && start < position)
                start++;

            if ((buffer[start] == '/' && start > 0))
            {
                // discarding data
                Logger.Debug(@"discarding {start} bytes");
                Buffer.BlockCopy(buffer, start, buffer, 0, position - start);
                position -= start;
                Logger.Debug($"bytes in buffer {position}, ");
                OnDataError(new DataErrorEventArgs() { Message = "partial" });
            }
            else if (start == position)
            {
                Logger.Debug(@"discarding all bytes");
                var tmp = new byte[position];
                Buffer.BlockCopy(buffer, 0, tmp, 0, position);
                OnDataError(new DataErrorEventArgs() { Message = "partial", Data = Encoding.ASCII.GetString(tmp)});
                position = 0;
            }            
        }

        protected void OnDataError(DataErrorEventArgs e)
        {
            Logger.Error($"data error {e.Message}");
            if (!string.IsNullOrWhiteSpace(e.Data))
                Logger.Error($"{Environment.NewLine}{e.Data}");

            EventHandler<DataErrorEventArgs> handler = DataError;

            handler?.Invoke(this, e);
        }
    }

    public class DataErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string Data { get; set; }
    }
}