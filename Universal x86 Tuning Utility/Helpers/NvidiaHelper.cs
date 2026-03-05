using System;

namespace Universal_x86_Tuning_Utility.Helpers;

public static class NvidiaHelper
{
    public static int CalculateTMU(string shortName, int cudaCores)
    {
        if (string.IsNullOrWhiteSpace(shortName))
            throw new ArgumentException("ShortName is empty");

        string prefix = shortName.Substring(0, 2).ToUpperInvariant();

        int cudaPerSM;
        int tmuPerSM;

        switch (prefix)
        {
            case "GF": // Fermi
                cudaPerSM = 32;
                tmuPerSM  = 4;
                break;

            case "GK": // Kepler
                cudaPerSM = 192;
                tmuPerSM  = 16;
                break;

            case "GM": // Maxwell
            case "GP": // Pascal
                cudaPerSM = 128;
                tmuPerSM  = 8;
                break;

            case "TU": // Turing
                cudaPerSM = 64;
                tmuPerSM  = 4;
                break;

            case "GA": // Ampere
                cudaPerSM = 128;
                tmuPerSM  = 4;
                break;

            case "AD": // Ada Lovelace
                cudaPerSM = 128;
                tmuPerSM  = 4;
                break;

            default:
                throw new NotSupportedException($"Unknown GPU architecture prefix: {prefix}");
        }

        if (cudaCores % cudaPerSM != 0)
            throw new InvalidOperationException(
                $"CUDA cores ({cudaCores}) not divisible by CUDA per SM ({cudaPerSM})");

        int smCount = cudaCores / cudaPerSM;
        return smCount * tmuPerSM;
    }

}