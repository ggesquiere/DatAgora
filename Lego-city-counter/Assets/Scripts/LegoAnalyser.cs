using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class LegoAnalyser : MonoBehaviour
{
    public Vector2Int legoMapSize = new Vector2Int(100, 100);
    public float analyserHeight = 10;
    public float scale = 1;
    public float verticalScaleMultiplicator = 1;
    public LayerMask collisionMask;
    public bool setGroundAt0;
    public string fileName = "lego_map";
    public string folderName = "lego_map";

    public bool exportToCSV;
    public string delimiter = ",";

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
        
        
        for (int i = 0; i < legoMap.columns.Count; i++)
        {

            // Position la plus basse ramenée à hauteur 0
            if (setGroundAt0)
            {
                if (legoMap.columns[i].height < minHeight)
                    legoMap.columns[i].height = 0;
                else
                    legoMap.columns[i].height -= minHeight;
            }

            // Normalisation en lego et count avec multiplication de la taille par l'echelle verticale
            legoMap.columns[i].height = Mathf.Round(legoMap.columns[i].height * verticalScaleMultiplicator);
            count += (int)legoMap.columns[i].height;
        }

        legoMap.legoCount = count;

        return legoMap;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(
            transform.position - new Vector3(0, analyserHeight / 2f, 0),
            new Vector3(legoMapSize.x * scale, analyserHeight, legoMapSize.y * scale)
        );
    }

    void ExportToCSV(LegoMap legoMap)
    {
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

        AssetDatabase.CreateFolder("Assets/CSV", folderName + "_Instructions");

        for (int i = 0; i < nbVerticalTiles;i++)
        {
            for(int j = 0; j < nbHorizontalTiles;j++)
            {
                string filePath = "Assets/CSV/" + folderName + "_Instructions/" + fileName + "_" + i + "_" + j + ".csv";
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
