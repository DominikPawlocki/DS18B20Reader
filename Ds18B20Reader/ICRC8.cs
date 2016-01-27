namespace Ds18B20Reader
{
    public interface ICrc8
    {
        bool CheckCrc(byte[] frame);
        byte Crc8 { get; }
    }
}