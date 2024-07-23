using System;
using System.Collections.Generic;

namespace NS.Bot.Shared.Enums
{
    public static class ColorsDeclinaton
    {
        public static Dictionary<DeclensionsEnum, string> Red = new Dictionary<DeclensionsEnum, string>()
        {
            [DeclensionsEnum.Im] = "красная",
            [DeclensionsEnum.Rod] = "красного",
            [DeclensionsEnum.Dat] = "красному",
            [DeclensionsEnum.Vin] = "красного",
            [DeclensionsEnum.Twor] = "красным",
            [DeclensionsEnum.Pred] = "красном"
        };

        public static Dictionary<DeclensionsEnum, string> Blue = new Dictionary<DeclensionsEnum, string>()
        {
            [DeclensionsEnum.Im] = "синий",
            [DeclensionsEnum.Rod] = "синего",
            [DeclensionsEnum.Dat] = "синему",
            [DeclensionsEnum.Vin] = "синего",
            [DeclensionsEnum.Twor] = "синим",
            [DeclensionsEnum.Pred] = "синем"
        };

        public static Dictionary<DeclensionsEnum, string> Green = new Dictionary<DeclensionsEnum, string>()
        {
            [DeclensionsEnum.Im] = "зелёный",
            [DeclensionsEnum.Rod] = "зелёного",
            [DeclensionsEnum.Dat] = "зелёному",
            [DeclensionsEnum.Vin] = "зелёного",
            [DeclensionsEnum.Twor] = "зелёным",
            [DeclensionsEnum.Pred] = "зелёном"
        };

        public static Dictionary<DeclensionsEnum, string> White = new Dictionary<DeclensionsEnum, string>()
        {
            [DeclensionsEnum.Im] = "белый",
            [DeclensionsEnum.Rod] = "белого",
            [DeclensionsEnum.Dat] = "белому",
            [DeclensionsEnum.Vin] = "белый",
            [DeclensionsEnum.Twor] = "белым",
            [DeclensionsEnum.Pred] = "белом"
        };

        public static Dictionary<DeclensionsEnum, string> Pink = new Dictionary<DeclensionsEnum, string>()
        {
            [DeclensionsEnum.Im] = "розовый",
            [DeclensionsEnum.Rod] = "розового",
            [DeclensionsEnum.Dat] = "розовому",
            [DeclensionsEnum.Vin] = "розовый",
            [DeclensionsEnum.Twor] = "розовым",
            [DeclensionsEnum.Pred] = "розовом"
        };

        public static Dictionary<DeclensionsEnum, string> Yellow = new Dictionary<DeclensionsEnum, string>()
        {
            [DeclensionsEnum.Im] = "жёлтый",
            [DeclensionsEnum.Rod] = "жёлтого",
            [DeclensionsEnum.Dat] = "жёлтому",
            [DeclensionsEnum.Vin] = "жёлтый",
            [DeclensionsEnum.Twor] = "жёлтым",
            [DeclensionsEnum.Pred] = "жёлтом"
        };

        public static Dictionary<DeclensionsEnum, string> GetColorDict(ColorsEnum color)
        {
            switch (color)
            {
                case ColorsEnum.Red:
                    return Red;
                case ColorsEnum.Blue:
                    return Blue;
                case ColorsEnum.Green:
                    return Green;
                case ColorsEnum.White:
                    return White;
                case ColorsEnum.Pink:
                    return Pink;
                case ColorsEnum.Yellow:
                    return Yellow;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
