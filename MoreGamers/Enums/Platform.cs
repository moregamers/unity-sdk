using System.ComponentModel;

namespace MG.Enums
{
    public enum Platform
    {
        [Description("development")]
        None       = 0,

        [Description("ios")]
        Itunes     = 1,

        [Description("amazon")]
        Amazon     = 2,

        [Description("android")]
        GooglePlay = 3,

        [Description("windows")]
        Windows    = 4,

        [Description("blackberry")]
        Blackberry = 5
    }
}
