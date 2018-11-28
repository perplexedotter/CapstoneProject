using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

//will save and load data
public class UnitSave : MonoBehaviour {

    public UnitData unitData;
    const string folder = "BinaryUnitData";
    const string extension = ".dat";

    //if no files exist in save directory then create 9 default saves
    private void Awake()
    {
        string[] filePaths = GetFilePaths();
        if (filePaths.Length <= 0)
        {
            for(int i = 1; i <= 9; i++)
            {
                save(new UnitData("Unit" + i, UnitType.fighter, ModuleType.shortRange, ModuleType.longRange));

            }
        }
    }
    void Update()
    {

    }

    //Pass unit to save data
    //File Format: Unit# ie Unit1, Unit2...Unit9  (based on Unit name)
    public void save(UnitData unit)
    {
        unitData = unit;
        string folderPath = Path.Combine(Application.persistentDataPath, folder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string dataPath = Path.Combine(folderPath, unitData.unitName + extension);
        SaveUnit(unitData, dataPath);
    }

    //load data based on passed in variable
    //subtract 1 from unit number to get index i
    public UnitData load(int i)
    {
        string[] filePaths = GetFilePaths();
        if (filePaths.Length > 0)
            unitData = LoadUnit(filePaths[i]);
        return unitData;
    }


    static void SaveUnit(UnitData data, string fileP)
    {
        BinaryFormatter bf = new BinaryFormatter();

        using (FileStream fs = File.Open(fileP, FileMode.OpenOrCreate))
        {
            bf.Serialize(fs, data);
        }
    }

    static UnitData LoadUnit(string fileP)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream fs = File.Open(fileP, FileMode.Open))
        {
            return (UnitData)bf.Deserialize(fs);
        }
    }

    //returns files in selected folder
    static string[] GetFilePaths()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, folder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        return Directory.GetFiles(folderPath, "*.*");
    }
}
