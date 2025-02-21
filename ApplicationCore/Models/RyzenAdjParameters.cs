using System.Text;
using ApplicationCore.Enums;

namespace ApplicationCore.Models;

public class RyzenAdjParameters
{
    public int? TctlTemp { get; set; }
    public int? CHTCTemp { get; set; }
    public int? ApuSkinTemp { get; set; }
    
    public int? StampLimit { get; set; }
    public int? StampTime { get; set; }
    public int? SlowLimit { get; set; }
    public int? SlowTime { get; set; }
    public int? FastLimit { get; set; }
    
    public int? VrmCurrent { get; set; }
    public int? VrmMaxCurrent { get; set; }
    public int? VrmSocCurrent { get; set; }
    public int? VrmSocMaxCurrent { get; set; }
    public int? VrmGfxCurrent { get; set; }
    public int? PptLimit { get; set; }
    public int? EdcLimit { get; set; }
    public int? TdcLimit { get; set; }
    public PowerPlan? WinPower { get; set; }

    private RyzenAdjParameters()
    {
        
    }

    public class RyzenAdjParametersBuilder
    {
        private readonly RyzenAdjParameters _parameters;

        public RyzenAdjParametersBuilder()
        {
            _parameters = new RyzenAdjParameters();
        }

        public RyzenAdjParametersBuilder WithStampLimit(int stampLimit, int stampTime = 0)
        {
            _parameters.StampLimit = stampLimit;
            if (stampTime != 0)
            {
                _parameters.StampTime = stampTime;
            }
            
            return this;
        }

        public RyzenAdjParametersBuilder WithSlowLimit(int slowLimit, int slowTime = 0)
        {
            _parameters.SlowLimit = slowLimit;
            if (slowTime != 0)
            {
                _parameters.SlowTime = slowTime;
            }
            
            return this;
        }

        public RyzenAdjParametersBuilder WithFastLimit(int fastLimit)
        {
            _parameters.FastLimit = fastLimit;
            
            return this;
        }

        public RyzenAdjParametersBuilder WithTctlTemp(int tctlTemp)
        {
            _parameters.TctlTemp = tctlTemp;

            return this;
        }

        public RyzenAdjParametersBuilder WithCHTCTemp(int cHtcTemp)
        {
            _parameters.CHTCTemp = cHtcTemp;

            return this;
        }

        public RyzenAdjParametersBuilder WithApuSkinTemp(int apuSkinTemp)
        {
            _parameters.ApuSkinTemp = apuSkinTemp;

            return this;
        }
        
        public RyzenAdjParametersBuilder WithVrm(int vrm, int vrmMax = 0)
        {
            _parameters.VrmCurrent = vrm;
            if (vrmMax != 0)
            {
                _parameters.VrmMaxCurrent = vrmMax;
            }
            
            return this;
        }
        
        public RyzenAdjParametersBuilder WithVrmSoc(int vrmSoc, int vrmSocMax = 0)
        {
            _parameters.VrmSocCurrent = vrmSoc;
            if (vrmSocMax != 0)
            {
                _parameters.VrmSocMaxCurrent = vrmSocMax;
            }
            
            return this;
        }
        
        public RyzenAdjParametersBuilder WithVrmGfx(int vrmGfx)
        {
            _parameters.VrmGfxCurrent = vrmGfx;
            
            return this;
        }

        public RyzenAdjParametersBuilder WithWinPower(PowerPlan winPower)
        {
            _parameters.WinPower = winPower;
            
            return this;
        }

        public RyzenAdjParametersBuilder WithPptLimit(int pptLimit)
        {
            _parameters.PptLimit = pptLimit;
            
            return this;
        }

        public RyzenAdjParametersBuilder WithEdcLimit(int edcLimit)
        {
            _parameters.EdcLimit = edcLimit;
            
            return this;
        }

        public RyzenAdjParametersBuilder WithTdcLimit(int tdcLimit)
        {
            _parameters.TdcLimit = tdcLimit;
            
            return this;
        }

        public string BuildParamtersString()
        {
            var parametersList = new List<string>();

            if (_parameters.TctlTemp != null)
            {
                parametersList.Add($"--tctl-temp={_parameters.TctlTemp}");
            }

            if (_parameters.CHTCTemp != null)
            {
                parametersList.Add($"--cHTC-temp={_parameters.CHTCTemp}");
            }

            if (_parameters.ApuSkinTemp != null)
            {
                parametersList.Add($"--apu-skin-temp={_parameters.ApuSkinTemp}");
            }

            if (_parameters.StampLimit != null)
            {
                parametersList.Add($"--stapm-limit={_parameters.StampLimit}");
            }

            if (_parameters.FastLimit != null)
            {
                parametersList.Add($"--fast-limit={_parameters.FastLimit}");
            }

            if (_parameters.SlowLimit != null)
            {
                parametersList.Add($"--slow-limit={_parameters.SlowLimit}");
            }

            if (_parameters.VrmCurrent != null)
            {
                parametersList.Add($"--vrm-current={_parameters.VrmCurrent}");
            }

            if (_parameters.VrmMaxCurrent != null)
            {
                parametersList.Add($"--vrmmax-current={_parameters.VrmMaxCurrent}");
            }

            if (_parameters.VrmSocCurrent != null)
            {
                parametersList.Add($"--vrmsoc-current={_parameters.VrmSocCurrent}");
            }

            if (_parameters.VrmSocMaxCurrent != null)
            {
                parametersList.Add($"--vrmsocmax-current={_parameters.VrmSocMaxCurrent}");
            }

            if (_parameters.VrmGfxCurrent != null)
            {
                parametersList.Add($"--vrmgfx-current={_parameters.VrmGfxCurrent}");
            }

            if (_parameters.WinPower != null)
            {
                parametersList.Add($"--Power-Plan={_parameters.WinPower}");
            }

            if (_parameters.PptLimit != null)
            {
                parametersList.Add($"--ppt-limit={_parameters.PptLimit}");
            }

            if (_parameters.EdcLimit != null)
            {
                parametersList.Add($"--edc-limit={_parameters.EdcLimit}");
            }

            if (_parameters.TdcLimit != null)
            {
                parametersList.Add($"--tdc-limit={_parameters.TdcLimit}");
            }
            
            return string.Join(" ", parametersList);
        }
    }
}