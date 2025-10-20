using ApplicationCore.Enums;

namespace ApplicationCore.Extensions;

public static class ChassisTypeExtensions
{
    public static bool IsLaptop(this ChassisType chassisType)
    {
        return chassisType is ChassisType.Laptop
            or ChassisType.Notebook
            or ChassisType.SubNotebook
            or ChassisType.Portable;
    }
}