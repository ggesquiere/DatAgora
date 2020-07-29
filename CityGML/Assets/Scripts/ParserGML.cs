using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Globalization;

public class ParserGML
{
    public static List<Building> GetBuildingsFromGML(string path)
    {
        List<Building> city = new List<Building>();
        List<string> buildindsPoints = new List<string>();
        foreach (string line in File.ReadAllLines(path))
        {
            if (line.Contains("<gml:posList srsDimension="))
            {
                string buffer = line.Remove(line.Length - 14);
                city.Add(CreateBuilding(buffer.Remove(0, 52))); //create building
            }    
        }
        return city;
    }


    private static Building CreateBuilding(string points)
    {

        List<Vector3> coords = new List<Vector3>();
        char[] separators = {' '};
        string[] strValues = points.Split(separators);

        List<float> floatValues = new List<float>();
        foreach (string str in strValues)
            floatValues.Add(float.Parse(str, CultureInfo.InvariantCulture.NumberFormat));

        for (int i = 0; i < floatValues.Count; i += 3)
            coords.Add(new Vector3(floatValues[i], floatValues[i + 2], floatValues[i + 1]));

        return new Building(coords);
    }
}
