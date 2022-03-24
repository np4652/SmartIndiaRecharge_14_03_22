using Fintech.AppCode.StaticModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.StaticModel
{
    public static class ColorSets
    {
        public const int DefaultColorSet = 1;
        public const int ColorSet2 = 2;
        public const int ColorSet3 = 3;

        public static int GetCurrentThemeSet(int ThemeID, int wId = 0)
        {
            try
            {
                var path = DOCType.ThemeWithColorJsonFilePath;
                if (System.IO.File.Exists(path))
                {
                    var jsonData = System.IO.File.ReadAllText(path);
                    var res = JsonConvert.DeserializeObject<List<ThemeWithColor>>(jsonData);
                    int? colorSetID = res.Where(x => x.ThemeID == ThemeID && x.WId == wId).FirstOrDefault()?.ColorID;
                    if (colorSetID == null || colorSetID == 0)
                        colorSetID = res.Where(x => x.ThemeID == ThemeID && x.WId == 0).FirstOrDefault()?.ColorID;
                    return colorSetID != null ? (int)colorSetID : 1;
                }
                else
                {
                    return DefaultColorSet;
                }
            }
            catch
            {
                return DefaultColorSet;
            }
        }
        public static bool UpdateThemColor(int themeId, int colorId, int wId)
        {
            try
            {
                var jsonFile = DOCType.ThemeWithColorJsonFilePath;
                if (File.Exists(jsonFile))
                {
                    var json = System.IO.File.ReadAllText(jsonFile);
                    var jObjectList = JsonConvert.DeserializeObject<List<ThemeWithColor>>(json);
                    if (jObjectList.Any(x => x.ThemeID == themeId && x.WId == wId))
                    {
                        foreach (var obj in jObjectList.Where(x => x.ThemeID == themeId && x.WId == wId))
                        {
                            obj.ColorID = colorId;
                        }
                    }
                    else
                    {
                        jObjectList.Add(new ThemeWithColor
                        {
                            WId = wId,
                            ThemeID = themeId,
                            ColorID = colorId
                        });
                    }
                    string output = JsonConvert.SerializeObject(jObjectList, Formatting.Indented);
                    File.WriteAllText(jsonFile, output);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
