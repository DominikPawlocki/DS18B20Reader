using System;
using System.IO.Ports;

namespace Ds18B20Reader
{
    public class ComPort : IPort, IDisposable
    {
        private SerialPort _sp = null;
        private readonly string _portName = "COM1";
        private bool _init = false;

        ///<summary>
        /// By default used port is COM1
        ///</summary>
        ///<param name="pName">Port name for example: COM1</param>
        public ComPort(string pName)
        {
            if (pName != string.Empty)
                _portName = pName;
        }

        private SerialPort Port
        {
            get
            {
                if (_sp == null)
                    _sp = new SerialPort(_portName);
                return _sp;
            }
        }

        private bool IsInitialized
        {
            get
            {
                return _init;
            }
        }
        ///<summary>
        /// By default ReadTimeout=3000ms;BaudRate=9600;Dtr=Enable
        ///</summary>
        ///<returns></returns>
        public bool InitializePort()
        {
            try
            {
                if (!Port.IsOpen)
                    Port.Open();
                Port.ReadTimeout = 3000;
                Port.BaudRate = 9600;
                Port.DiscardInBuffer();
                Port.DiscardOutBuffer();
                if (!Port.DtrEnable)
                    Port.DtrEnable = true;
                _init = true;
                return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }
        public bool Reset()
        {
            try
            {
                if (!IsInitialized)
                    this.InitializePort();
                Port.BaudRate = 9600;
                Port.Write(new byte[1] { 0xF0 }, 0, 1);
                while (Port.ReadByte() == 0xF0) ;
            }
            catch (Exception ex)
            {
                Port.Close();
                return false;
            }
            Port.BaudRate = 115200;
            return true;
        }
        public byte[] ByteToBits(byte b)
        {
            byte[] bits = new byte[8];
            byte and = 128;
            for (int i = 7; i >= 0; i--)
            {
                bits[i] = (b & and) == 0x00 ? (byte)0x00 : (byte)0xFF;
                and /= 2;
            }
            return bits;
        }
        public byte[] BitsToBytes(byte[] b)
        {
            byte[] result = new byte[b.Length / 8];
            for (int n = 0; n < result.Length; n++)
            {
                byte and = 128;
                for (int i = 7; i >= 0; i--)
                {
                    result[n] ^= b[n * 8 + i] == (byte)0xFF ? and : (byte)0x00;
                    and /= 2;
                }
            }
            return result;
        }
        public bool Write(params byte[] bytes)
        {
            try
            {
                foreach (byte b in bytes)
                {
                    byte[] bufer = ByteToBits(b);
                    for (int i = 0; i < 8; i++)
                    {
                        this.WriteBit(bufer[i]);
                    }
                }
                return true;
            }
            catch
            {
            }
            return false;
        }
        public bool WriteBit(byte bit)
        {
            try
            {
                byte[] bufer = new byte[1] { bit };
                Port.Write(bufer, 0, 1);
                while (Port.ReadByte() != bufer[0]) ;
                return true;
            }
            catch
            {
            }
            return false;
        }
        public byte[] Read(int length)
        {
            byte[] rbuf = new byte[length * 8];
            for (int n = 0; n < length; n++)
            {
                for (int i = 0; i < 8; i++)
                {
                    rbuf[n * 8 + i] = this.ReadBit();
                }
            }
            return BitsToBytes(rbuf);
        }
        public byte ReadBit()
        {
            byte bit = 0x00;
            Port.Write(new byte[1] { 0xFF }, 0, 1);
            bit = Port.ReadByte() != 0xFF ? (byte)0x00 : (byte)0xFF;
            return bit;
        }
        public void Dispose()
        {
            Port.Close();
            Port.Dispose();
        }
        #region IDisposable Members
        void IDisposable.Dispose()
        {
            this.Dispose();
        }
        #endregion
    }
}