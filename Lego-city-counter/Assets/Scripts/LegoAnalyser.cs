using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class LegoAnalyser : MonoBehaviour
{
    [Space(5)]
    [Header("    Analyzer attributes")]
    [Tooltip("Desired lego map size (in lego amount).")]
    public Vector2Int legoMapSize = new Vector2Int(100, 100);
    [Tooltip("Maximum height for the analyzer.")]
    public float analyserHeight = 10;
    [Tooltip("Size of one lego in unity units. (Impacts lenght and width of the analyzer in editor. Be sure to adjust it's size and position accordingly.)")]
    public float scale = 1;
    [Tooltip("Multiplies all heights computed for the map.")]
    public float verticalScaleMultiplicator = 1;
    [Tooltip("Set the layers detected by the analyzer.")]
    public LayerMask collisionMask;
    [Tooltip("Sets the lowest heigth to be 0 in the final results. adjusts other heigths accordingly.")]
    public bool setGroundAt0;
    [Tooltip("Analyzer color in editor")]
    public Color analyzerColor = new Color(1, 0, 0, 0.5f);
    [Tooltip("single lego color displayed in the middle of the analyzer")]
    public Color singleLegoColor = new Color(0, 1, 1, 0.5f);

    [Space(5)]
    [Header("    Files configuration")]
    [Tooltip("Name of the JSON and CSV files output")]
    public string fileName = "lego_map";
    [Tooltip("Check if you need CSV export for building instructions")]
    public bool exportToCSV;
    [Tooltip("Name of the folder for the CSV files")]
    public string folderName = "lego_map";
    [Tooltip("Sets the delimiter used in the CSV files")]
    public string delimiter = ",";
    [Tooltip("Size of the tile used for your model to properly adjust instructions format")]
    public int legoTileSize = 32;


    public void ExportLegoMap()
    {
        LegoMap legoMap = ComputeLegoMap();
        File.WriteAllText("Assets/JSON/" + fileName + ".json", JsonUtility.ToJson(legoMap));
        if (exportToCSV)
        {
            ExportToCSV(legoMap);
        }
    }

    private LegoMap ComputeLegoMap()
    {
        //Initialisation
        int count = 0;
        LegoMap legoMap = new LegoMap();

        legoMap.mapSize = legoMapSize;
        legoMap.mapScale = scale;
        legoMap.columns = new List<Column>();

        Vector3 rayOrigin = transform.position - new Vector3(
            legoMapSize.x * scale / 2f,
            0,
            legoMapSize.y * scale / 2f
        );

        float minHeight = analyserHeight;

        //Map computation
        for (int x = 0; x < legoMapSize.x; x++)
        {
            for (int z = 0; z < legoMapSize.y; z++)
            {
                Vector3 rayPosition = rayOrigin + new Vector3(
                    (x + 0.5f) * scale,
                    0,
                    (z + 0.5f) * scale
                );

                RaycastHit hit;
                float height = 0;
                Column.Type type = Column.Type.Default;
                if (Physics.Raycast(rayPosition, Vector3.down, out hit, analyserHeight, collisionMask))
                {
                    height = analyserHeight - hit.distance;
                    if (height < minHeight) minHeight = height;

                    if (hit.collider.tag == "Ground") type = Column.Type.Ground;
                    if (hit.collider.tag == "Building") type = Column.Type.Building;
                }
                Column column = new Column(height, type);
                legoMap.columns.Add(column);
            }
        }
        
        // Conversion to legos
        for (int i = 0; i < legoMap.columns.Count; i++)
        {

            // Lowest height brought to 0
            if (setGroundAt0)
            {
                if (legoMap.columns[i].height < minHeight)
                    legoMap.columns[i].height = 0;
                else
                    legoMap.columns[i].height -= minHeight;
            }

            legoMap.columns[i].height = Mathf.Round(legoMap.columns[i].height * verticalScaleMultiplicator);
            count += (int)legoMap.columns[i].height;
        }

        legoMap.legoCount = count;

        return legoMap;
    }

    // Parameters for the analyzer display in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = analyzerColor;
        Gizmos.DrawCube(
            transform.position - new Vector3(0, analyserHeight / 2f, 0),
            new Vector3(legoMapSize.x * scale, analyserHeight, legoMapSize.y * scale)
        );

        Debug.Log(analyzerColor);

        singleLegoColor = new Color(1-analyzerColor.r , 1-analyzerColor.g , 1-analyzerColor.b, 1);
        Gizmos.color = singleLegoColor;
        Gizmos.DrawCube(
            transform.position - new Vector3(0, analyserHeight / 2f, 0),
            new Vector3(scale, 1, scale)
        );
    }

    void ExportToCSV(LegoMap legoMap)
    {
        // Output initialisation
        int nbHorizontalTiles = Mathf.CeilToInt((float)legoMapSize.x / (float)legoTileSize);
        int nbVerticalTiles = Mathf.CeilToInt((float)legoMapSize.y / (float)legoTileSize);

        int outputHorizontalSize = nbHorizontalTiles * legoTileSize;
        int outputVerticalSize = nbVerticalTiles * legoTileSize;

        int[] output = new int[outputHorizontalSize * outputVerticalSize];

        for (int i = 0 ; i < legoMapSize.y ; i++)
        {
            for(int j = 0 ; j < legoMapSize.x ; j++)
            {
                output [ i * outputHorizontalSize + j ] = Mathf.RoundToInt(legoMap.columns[i* legoMapSize.x + j].height);
            }
        }


        //Folder and files creation
        AssetDatabase.CreateFolder("Assets/CSV", folderName);

        for (int i = 0; i < nbVerticalTiles;i++)
        {
            for(int j = 0; j < nbHorizontalTiles;j++)
            {
                string filePath = "Assets/CSV/" + folderName + fileName + "_" + i + "_" + j + ".csv";
                StringBuilder sb = new StringBuilder();
                
                for( int u = 0; u < legoTileSize ; u++)
                {
                    string tempLine = "";
                    for(int v = 0; v < legoTileSize ; v++)
                    {
                        tempLine += output[(i * outputHorizontalSize + j) * legoTileSize + u * outputHorizontalSize + v] + delimiter;
                    }
                    sb.AppendLine(tempLine);
                }

                StreamWriter outStream = System.IO.File.CreateText(filePath);
                outStream.WriteLine(sb);
                outStream.Close();
            }
        }
    }
}
