namespace Ds18B20Reader
{
    public enum Command : byte
    {
        ///<summary>
        /// 0x44
        ///</summary>
        ConvertTemperatureFun = 0x44,

        ///<summary>
        /// 0x4E
        ///</summary>
        WriteScratchPadFun = 0x4E,

        ///<summary>
        /// 0xBE
        ///</summary>
        ReadScratchPadFun = 0xBE,

        ///<summary>
        /// 0x48
        ///</summary>
        CopyScratchPadFun = 0x48,

        ///<summary>
        /// 0xB8
        ///</summary>
        RecallE2Fun = 0xB8,

        ///<summary>
        /// 0xB4
        ///</summary>
        ReadPowerSupplyFun = 0xB4,

        ///<summary>
        /// 0xF0
        ///</summary>
        SearchRom = 0xF0,

        ///<summary>
        /// 0x33
        ///</summary>
        ReadRom = 0x33,

        ///<summary>
        /// 0x55
        ///</summary>
        MatchRom = 0x55,

        ///<summary>
        /// 0xCC
        ///</summary>
        SkipRom = 0xCC,

        ///<summary>
        /// 0xEC
        ///</summary>
        AlarmSearchRom = 0xEC
    }

    public enum TemperatureResolution : byte
    {
        T12Bit = 0x60,
        T11Bit = 0x40,
        T10Bit = 0x20,
        T9Bit = 0x00
    }
}
