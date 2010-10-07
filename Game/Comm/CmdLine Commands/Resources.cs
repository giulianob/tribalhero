using System;
using Game.Data;
using Game.Setup;
using Game.Util;
using NDesk.Options;

namespace Game.Comm
{
    public partial class CmdLineProcessor
    {        
        public string CmdSendResources(string[] parms)
        {
            Resource resource = new Resource();
            string cityName = string.Empty;
            bool help = false;

            try
            {
                var p = new OptionSet
                            {
                                { "?|help|h", v => help = true }, 
                                { "city=", v => cityName = v.TrimMatchingQuotes() }, 
                                { "crop=", v => resource.Crop = int.Parse(v) },
                                { "wood=", v => resource.Wood = int.Parse(v) },
                                { "labor=", v => resource.Labor = int.Parse(v) },
                                { "iron=", v => resource.Iron = int.Parse(v) },
                                { "gold=", v => resource.Gold = int.Parse(v) },
                            };
                p.Parse(parms);
            }
            catch (Exception)
            {
                help = true;
            }

            if (help || string.IsNullOrEmpty(cityName))
                return "send_resources --city=city [--crop=###] [--wood=###] [--iron=###] [--labor=###]";

            uint cityId;
            if (!Global.World.FindCityId(cityName, out cityId))
                return "City not found";

            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                if (city == null)
                    return "City not found";

                city.BeginUpdate();
                city.Resource.Add(resource);
                city.EndUpdate();
            }

            return "OK!";
        }
    }
}