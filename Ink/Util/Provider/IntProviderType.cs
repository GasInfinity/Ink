namespace Ink.Util.Provider;

public enum IntProviderType
{
    Constant,
    Uniform,
    BiasedToBottom,
    Clamped,
    ClampedNormal,
    WeightedList
}


public static class IntProvider
{
    public static string TypeToString(IntProviderType type)
        => type switch
        { 
            IntProviderType.Constant => "constant",
            IntProviderType.Uniform => "uniform",
            IntProviderType.BiasedToBottom => "biased_to_bottom",
            IntProviderType.Clamped => "clamped",
            IntProviderType.ClampedNormal => "clamped_normal",
            IntProviderType.WeightedList => "weighted_list",
            _ => "constant"
        };
}