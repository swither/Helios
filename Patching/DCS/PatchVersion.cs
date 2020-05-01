using System;
using System.Collections.Generic;
using System.Linq;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public static class PatchVersion
    {
        public static string SortableString(string versionString)
        {
            List<int> numbers;
            try
            {
                numbers = versionString.Split('.')
                    .Select(int.Parse)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new System.Exception($"invalid version format '{versionString}' read from DCS autoupdate file; update Helios to support current DCS version", ex);
            }
            switch (numbers.Count)
            {
                case 3:
                    numbers.Add(0);
                    numbers.Add(0);
                    break;
                case 4:
                    numbers.Add(0);
                    break;
                case 5:
                    break;
                default:
                    throw new System.Exception($"unsupported version number length '{versionString}' read from DCS autoupdate file; update Helios to support current DCS version");
            }
            return $"{numbers[0]:000}_{numbers[1]:000}_{numbers[2]:00000}_{numbers[3]:00000}_{numbers[4]:00000}";
        }
    }
}