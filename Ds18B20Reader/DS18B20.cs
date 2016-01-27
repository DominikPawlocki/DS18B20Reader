using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ds18B20Reader
{
    public class Ds18B20 : IDisposable
    {
        private byte[] _id;
        private IPort _port;
        private TemperatureResolution _tres = TemperatureResolution.T12Bit;
        private float _min = 130;
        private float _max = 130;

        public TemperatureResolution TemperResolution
        {
            get
            {
                return _tres;
            }
            set
            {
                _tres = value;
            }
        }

        public string IdString
        {
            get
            {
                StringBuilder strBld = new StringBuilder(8);
                foreach (byte byteVal in _id)
                {
                    strBld.Append(byteVal.ToString());
                }
                return strBld.ToString();
            }
        }

        public byte[] Id
        {
            get
            {
                return _id;
            }
        }
        public float Min
        {
            get
            {
                return _min;
            }
        }
        public float Max
        {
            get
            {
                return _max;
            }
        }

        public Ds18B20(IPort port)
        {
            _port = port;
            this.ConvertT();
        }

        public Ds18B20(byte[] id, IPort port)
        {
            _id = id.Take(8).ToArray();
            _port = port;
            this.ConvertT();
        }

        ///<summary>
        /// Only when there is one slave on the bus
        ///</summary>
        public void ReadId()
        {
            lock (_port)
            {
                if (_port.InitializePort())
                {
                    if (_port.Reset())
                    {
                        this.WriteCommand(Command.ReadRom);
                        _id = this.Read(8);
                    }
                }
            }
        }

        private float ConvertToTemperature(byte ms, byte ls)
        {
            float temp = 0;
            bool negative = false;
            if ((ms & 128) > 0)
            {
                ms = (byte)(0xFF - ms);
                ls = (byte)(0x00 - ls);
                negative = true;
            }
            temp += (ms & 4) > 0 ? 64 : 0;
            temp += (ms & 2) > 0 ? 32 : 0;
            temp += (ms & 1) > 0 ? 16 : 0;
            temp += (ls & 128) > 0 ? 8 : 0;
            temp += (ls & 64) > 0 ? 4 : 0;
            temp += (ls & 32) > 0 ? 2 : 0;
            temp += (ls & 16) > 0 ? 1 : 0;
            temp += (ls & 8) > 0 ? 0.5f : 0;
            if (_tres >= TemperatureResolution.T10Bit)
            {
                temp += (ls & 4) > 0 ? 0.25f : 0;
                if (_tres >= TemperatureResolution.T11Bit)
                {
                    temp += (ls & 2) > 0 ? 0.125f : 0;
                    if (_tres >= TemperatureResolution.T12Bit)
                        temp += (ls & 1) > 0 ? 0.0625f : 0;
                }
            }
            temp = negative ? -temp : temp;
            return temp;
        }
        public bool WriteCommand(Command cmd)
        {
            return this.Write((byte)cmd);
        }
        public bool Write(params byte[] b)
        {
            lock (_port)
            {
                return _port.Write(b);
            }
        }
        public byte ReadByte()
        {
            return this.Read(1)[0];
        }
        public byte[] Read(int num)
        {
            lock (_port)
            {
                return _port.Read(num);
            }
        }

        private bool ConvertT()
        {
            bool flag = true;
            lock (_port)
            {
                if (_port.Reset())
                {
                    if (this.WriteCommand(Command.SkipRom))
                    {
                        flag = this.WriteCommand(Command.ConvertTemperatureFun);
                    }
                }
            }
            if (flag)
            {
                switch (_tres)
                {
                    case TemperatureResolution.T12Bit:
                        Thread.Sleep(750);
                        break;
                    case TemperatureResolution.T11Bit:
                        Thread.Sleep(375);
                        break;
                    case TemperatureResolution.T10Bit:
                        Thread.Sleep(188);
                        break;
                    case TemperatureResolution.T9Bit:
                        Thread.Sleep(94);
                        break;
                }
                return true;
            }
            return false;
        }
        ///<summary>
        /// If Resolution is x then conversion time is y:
        ///<example>12Bits = 750ms; 11Bits = 375ms; 10Bits = 188ms; 9Bits = 94ms
        ///</example>
        ///</summary>
        ///<returns></returns>
        public async Task<float> ReadTemperature()
        {
            Task<float> tempRead = new Task<float>(() =>
            {
                lock (_port)
                {
                    if (this.ConvertT())
                    {
                        if (this.AddressById())
                        {
                            CRC8 ccc = new CRC8();
                            byte dupa = 0x00;
                            int xCRC = 0x00;

                            bool resul = ccc.CheckCrc(Id);

                            this.WriteCommand(Command.ReadScratchPadFun);
                            byte[] buf = this.Read(9);

                             resul = ccc.CheckCrc(buf);

                            float tmp = this.ConvertToTemperature(buf[1], buf[0]);
                            if (_min == _max && _min == 130)
                                _min = _max = tmp;
                            if (_min > tmp)
                                _min = tmp;
                            if (_max < tmp)
                                _max = tmp;
                            return tmp;
                        }
                    }
                }
                return -999.999f;
            });
            tempRead.Start();
            return await tempRead;
        }
        public bool AddressById()
        {
            lock (_port)
            {
                if (_port.Reset())
                {
                    if (this.WriteCommand(Command.MatchRom))
                    {
                        return this.Write(_id);
                    }
                }
            }
            return false;
        }

        public async Task<bool> SetTemperatureResolution(TemperatureResolution tr)
        {
            Task<bool> tempSet = new Task<bool>(() =>
            {
                lock (_port)
                {
                    _tres = tr;
                    if (this.AddressById())
                    {
                        if (this.WriteCommand(Command.WriteScratchPadFun))
                            if (this.Write(0xFF, 0xFF, (byte)_tres))
                                return true;
                    }
                }
                return false;
            });
            tempSet.Start();
            return await tempSet;
        }

        #region IDisposable Members
        void IDisposable.Dispose()
        {
            _port = null;
        }
        #endregion
    }
}