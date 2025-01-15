using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Net;

[EnumExtensions]
public enum MinecraftProtocol : int
{
    [Display(Name = "All")]
    Base = -1,

    [Display(Name = "1.7.2-1.7.4")]
    V1_7_2 = 4,
    [Display(Name = "1.7.6-1.7.10")]
    V1_7_6 = 5,

    [Display(Name = "1.8-1.8.9")]
    V1_8 = 47,

    [Display(Name = "1.9")]
    V1_9 = 107,
    [Display(Name = "1.9.1")]
    V1_9_1 = 108,
    [Display(Name = "1.9.2")]
    V1_9_2 = 109,
    [Display(Name = "1.9.3-1.10.2")]
    V1_9_3 = 110,

    [Display(Name = "1.11")]
    V1_11 = 315,
    [Display(Name = "1.11.1-1.11.2")]
    V1_11_2 = 316,

    [Display(Name = "1.12")]
    V1_12 = 335,
    [Display(Name = "1.12.1")]
    V1_12_1 = 338,
    [Display(Name = "1.12.2")]
    V1_12_2 = 340,

    [Display(Name = "1.13")]
    V1_13 = 393,
    [Display(Name = "1.13.1")]
    V1_13_1 = 401,
    [Display(Name = "1.13.2")]
    V1_13_2 = 404,

    [Display(Name = "1.14")]
    V1_14 = 477,
    [Display(Name = "1.14.1")]
    V1_14_1 = 480,
    [Display(Name = "1.14.2")]
    V1_14_2 = 485,
    [Display(Name = "1.14.3")]
    V1_14_3 = 490,
    [Display(Name = "1.14.4")]
    V1_14_4 = 498,

    [Display(Name = "1.15")]
    V1_15 = 573,
    [Display(Name = "1.15.1")]
    V1_15_1 = 575,
    [Display(Name = "1.15.2")]
    V1_15_2 = 578,

    [Display(Name = "1.16")]
    V1_16 = 735,
    [Display(Name = "1.16.1")]
    V1_16_1 = 736,
    [Display(Name = "1.16.2")]
    V1_16_2 = 751,
    [Display(Name = "1.16.3")]
    V1_16_3 = 753,
    [Display(Name = "1.16.4-1.16.5")]
    V1_16_4 = 754,

    [Display(Name = "1.17")]
    V1_17 = 755,
    [Display(Name = "1.17.1")]
    V1_17_1 = 756,

    [Display(Name = "1.18-1.18.1")]
    V1_18 = 757,
    [Display(Name = "1.18.2")]
    V1_18_2 = 758,

    [Display(Name = "1.19")]
    V1_19 = 759,
    [Display(Name = "1.19.1-1.19.2")]
    V1_19_1 = 760,
    [Display(Name = "1.19.3")]
    V1_19_3 = 761,
    [Display(Name = "1.19.4")]
    V1_19_4 = 762,

    [Display(Name = "1.20-1.20.1")]
    V1_20 = 763,
    [Display(Name = "1.20.2")]
    V1_20_2 = 764,
    [Display(Name = "1.20.3-1.20.4")]
    V1_20_3 = 765,
    [Display(Name = "1.20.5-1.20.6")]
    V1_20_5 = 766,

    [Display(Name = "1.21-1.21.1")]
    V1_21 = 767,
    [Display(Name = "1.21.2-1.21.3")]
    V1_21_2 = 768,
    [Display(Name = "1.21.4")]
    V1_21_4 = 769,
}
