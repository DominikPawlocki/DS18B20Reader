using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ds18B20Reader
{
    public class SearchSlaves : IDisposable
    {
        private readonly List<byte[]> _ds = new List<byte[]>();
        private IPort _port = null;

        private int _lastDiscrepancy = 0;
        private byte _lastSelectedBit = 0x00;
        private bool _done = false;
        private readonly byte[] _currentId = new byte[512];

        public List<byte[]> IdList
        {
            get
            {
                return _ds;
            }
        }

        public async Task<bool> Search(IPort p)
        {
            _port = p;

            Task<bool> run = new Task<bool>(() =>
            {
                lock (_port)
                {
                    if (!_port.InitializePort())
                        return false;
                    _lastDiscrepancy = -1;
                    _lastSelectedBit = 0x00;
                    _done = true;
                    do
                    {
                        this.Run(_lastSelectedBit);
                        _ds.Add(_port.BitsToBytes(_currentId));
                    } while (!_done);
                }
                return true;
            });
            run.Start();
            return await run;
        }


        private void Run(byte selectedBit)
        {
            if (!_port.Reset())
                return;
            if (!_port.Write((byte)Command.SearchRom))
                return;
            byte ab = 0x00;
            for (int i = 0; i < 512; i++)
            {
                ab = (byte)(_port.ReadBit() == 0xFF ? 0x01 : 0x00);
                ab ^= (byte)(_port.ReadBit() == 0xFF ? 0x02 : 0x00);
                /*if (ab == 0x03)
                throw new Exception("No any slaves found");*/
                if (ab != 0x00)
                {
                    if (ab == 0x01)
                        _currentId[i] = 0xFF;
                    if (ab == 0x02)
                        _currentId[i] = 0x00;
                    if (!_port.WriteBit(_currentId[i]))
                        return;
                }
                else
                {
                    if (_lastDiscrepancy != i)
                        _done = false;
                    else
                        _done = true;
                    if (_lastDiscrepancy == i)
                    {
                        if (_lastSelectedBit == 0x00)
                            selectedBit = 0xFF;
                        else if (_lastSelectedBit == 0xFF)
                            selectedBit = 0x00;
                    }
                    _currentId[i] = selectedBit;
                    _lastDiscrepancy = i;
                    _lastSelectedBit = selectedBit;
                    if (!_port.WriteBit(selectedBit))
                        return;
                }
            }
        }

        #region IDisposable Members
        void IDisposable.Dispose()
        {
            _port = null;
        }
        #endregion
    }
}