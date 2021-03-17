using System;
using System.Collections.Concurrent;
using System.Text;

namespace P1SmartMeter
{
    public class MessageBuffer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly NLog.Logger LoggerP1Stream = NLog.LogManager.GetLogger("p1stream");
        private static readonly NLog.Logger LoggerP1Messages = NLog.LogManager.GetLogger("p1messages");

        private readonly CRC16 crc16 = new CRC16();

        public event EventHandler<DataErrorEventArgs> DataError;

        private static int bufferCapicity = 2048;
        private byte[] buffer = new byte[bufferCapicity];
        private int position = 0;             

        public int Add(string data)
        {
            LoggerP1Stream.Info($"---------- start ----------{Environment.NewLine}{data}");
            LoggerP1Stream.Info($"---------- end   ----------");

            Encoding ascii = Encoding.ASCII;
            int spaceAvailable = bufferCapicity - position;
            int nrToCopy = spaceAvailable > data.Length ? data.Length : spaceAvailable; // todo check buffer overflowing!
            if (nrToCopy > 0)
            {
                Buffer.BlockCopy(ascii.GetBytes(data), 0, buffer, position, nrToCopy);
                position += nrToCopy;
            }
            Logger.Debug($"bytes in buffer after adding {position}, ");
            RemovePartialMessage();

            //while (RetrieveMessageFromBuffer()) ;

            return position;
        }

        public bool TryTake(out string msg)
        {
            while (RetrieveMessageFromBuffer(out msg) && string.IsNullOrWhiteSpace(msg));
            return !string.IsNullOrWhiteSpace(msg);
        }

        private bool RetrieveMessageFromBuffer(out string msg)
        {
            msg = null;
            bool bufferChanged = false;                   
            do
            {
                if (position == 0) return false;
                RemovePartialMessage();

                bufferChanged = false;
                int s = 0;
                while (buffer[s] != '!' && (s == 0 || (buffer[s] != '/' && s > 0)) && s < position)
                    s++;

                // <!><#><#><#><#><CR><LF> -> 7 bytes
                if (s + 7 <= position)
                {
                    if (buffer[s] == '!')
                    {
                        var mbytes = new byte[s + 7];
                        var checksumbytes = new byte[4];
                        Buffer.BlockCopy(buffer, 0, mbytes, 0, s + 7);
                        Buffer.BlockCopy(buffer, s + 1, checksumbytes, 0, 4);

                        if (position > s + 7)
                        {
                            Buffer.BlockCopy(buffer, s + 7, buffer, 0, position - s - 7);
                            position -= s + 7;
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
                            LoggerP1Messages.Debug($"---------- start ----------{Environment.NewLine}{msg}");
                            LoggerP1Messages.Debug($"---------- end   ----------");
                        }
                        else
                        {
                            var logmsg = Encoding.ASCII.GetString(mbytes);
                            LoggerP1Messages.Error($"---------- start ---------- crc {msgChecksum} != {calculatedChecksum}{Environment.NewLine}{logmsg}");
                            LoggerP1Messages.Error($"---------- end   ----------");
                            // crc error
                            OnDataError(new DataErrorEventArgs() { Message = $"crc {msgChecksum} != {calculatedChecksum}" });
                        }
                        bufferChanged = true;
                    }
                    else
                    if (buffer[s] == '/' && s > 0)
                    {
                        // we have a partial start of a message, followed with a new start of a message
                        // get rid of first partial message
                        Buffer.BlockCopy(buffer, s, buffer, 0, position - s);
                        position -= s;
                        // partial removed error
                        OnDataError(new DataErrorEventArgs() { Message = "partial" });
                        bufferChanged = true;
                    }
                }
            } while (bufferChanged && string.IsNullOrWhiteSpace(msg));
            return !string.IsNullOrWhiteSpace(msg);
        }

        private bool RemovePartialMessage()
        {
            // check if buffer is empty
            if (position == 0) return false;

            bool bufferChanged = false;
            int start = 0;
            while (buffer[start] != '/' && start < position)
                start++;

            Logger.Debug($"check partial message {position}, {start}");

            if ((buffer[start] == '/' && start > 0))
            {
                // discarding data
                Logger.Debug(@"discarding {start} bytes");
                Buffer.BlockCopy(buffer, start, buffer, 0, position - start);
                position -= start;
                Logger.Debug($"bytes in buffer {position}, ");
                OnDataError(new DataErrorEventArgs() { Message = "partial" });
                bufferChanged = true;
            }
            else if (start == position)
            {
                Logger.Debug(@"discarding all bytes");
                var tmp = new byte[position];
                Buffer.BlockCopy(buffer, 0, tmp, 0, position);
                OnDataError(new DataErrorEventArgs() { Message = "partial", Data = Encoding.ASCII.GetString(tmp)});
                position = 0;
                bufferChanged = true;
            }

            return bufferChanged;
        }

        protected void OnDataError(DataErrorEventArgs e)
        {
            Logger.Error($"data error {e.Message}{Environment.NewLine}{e.Data}");

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