using System;
using System.Collections.Generic;
using EMS.Library;

namespace EMS.Engine
{
    public class Measurements
    {
        private readonly Queue<MeasurementBase> _measurements = new(60);

        public MeasurementBase[] Get()
        {
            lock (_measurements)
            {
                var m = _measurements.ToArray();
                return m;
            }
        }

        public void Add(MeasurementBase measurement)
        {
            lock (_measurements)
            {
                _measurements.Enqueue(measurement);

                bool done = false;
                do
                {
                    if (_measurements.TryPeek(out MeasurementBase m))
                    {
                        var t = DateTime.Now - m.Received;
                        if (t.Seconds > 30)
                        {
                            _measurements.Dequeue();
                        }
                        else
                            done = true;
                    }
                    else
                        done = true;
                } while (!done);
            }
        }
    }
}
