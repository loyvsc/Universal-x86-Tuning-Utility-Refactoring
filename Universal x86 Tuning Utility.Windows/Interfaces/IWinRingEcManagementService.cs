namespace Universal_x86_Tuning_Utility.Windows.Interfaces;

public interface IWinRingEcManagementService
{
    public ushort RegAddress { get; set; }
    public ushort RegData { get; set; }
    
    public void InitECWin4();
    public byte ECRamReadWin4(ushort address);
    public void ECRamWriteWin4(ushort address, byte data);
    public void ECRamWrite(ushort address, byte data);
    public byte ECRamRead(ushort address);
    
    public void Initialize();
    public void Free();
}