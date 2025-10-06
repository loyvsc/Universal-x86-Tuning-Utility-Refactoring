using ApplicationCore.Enums;
using ApplicationCore.Utilities;

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
            var sb = StringBuilderPool.Rent();

            if (_parameters.TctlTemp != null)
            {
                sb.Append("--tctl-temp=");
                sb.Append(_parameters.TctlTemp);
                sb.Append(' ');
            }

            if (_parameters.CHTCTemp != null)
            {
                sb.Append("--cHTC-temp=");
                sb.Append(_parameters.CHTCTemp);
                sb.Append(' ');
            }

            if (_parameters.ApuSkinTemp != null)
            {
                sb.Append("--apu-skin-temp=");
                sb.Append(_parameters.ApuSkinTemp);
                sb.Append(' ');
            }

            if (_parameters.StampLimit != null)
            {
                sb.Append("--stapm-limit=");
                sb.Append(_parameters.StampLimit);
                sb.Append(' ');
            }

            if (_parameters.FastLimit != null)
            {
                sb.Append("--fast-limit=");
                sb.Append(_parameters.FastLimit);
                sb.Append(' ');
            }

            if (_parameters.SlowLimit != null)
            {
                sb.Append("--slow-limit=");
                sb.Append(_parameters.SlowLimit);
                sb.Append(' ');
            }

            if (_parameters.VrmCurrent != null)
            {
                sb.Append("--vrm-current=");
                sb.Append(_parameters.VrmCurrent);
                sb.Append(' ');
            }

            if (_parameters.VrmMaxCurrent != null)
            {
                sb.Append("--vrmmax-current=");
                sb.Append(_parameters.VrmMaxCurrent);
                sb.Append(' ');
            }

            if (_parameters.VrmSocCurrent != null)
            {
                sb.Append("--vrmsoc-current=");
                sb.Append(_parameters.VrmSocCurrent);
                sb.Append(' ');
            }

            if (_parameters.VrmSocMaxCurrent != null)
            {
                sb.Append("--vrmsocmax-current=");
                sb.Append(_parameters.VrmSocMaxCurrent);
                sb.Append(' ');
            }

            if (_parameters.VrmGfxCurrent != null)
            {
                sb.Append("--vrmgfx-current=");
                sb.Append(_parameters.VrmGfxCurrent);
                sb.Append(' ');
            }

            if (_parameters.WinPower != null)
            {
                sb.Append("--Power-Plan=");
                sb.Append(_parameters.WinPower);
                sb.Append(' ');
            }

            if (_parameters.PptLimit != null)
            {
                sb.Append("--ppt-limit=");
                sb.Append(_parameters.PptLimit);
                sb.Append(' ');
            }

            if (_parameters.EdcLimit != null)
            {
                sb.Append("--edc-limit=");
                sb.Append(_parameters.EdcLimit);
                sb.Append(' ');
            }

            if (_parameters.TdcLimit != null)
            {
                sb.Append("--tdc-limit=");
                sb.Append(_parameters.TdcLimit);
            }
            
            var paramStr = sb.ToString();
            
            StringBuilderPool.Return(sb);
            
            return paramStr;
        }
    }
}