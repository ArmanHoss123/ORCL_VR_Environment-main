using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using System.Threading;
using System;
using UnityEngine.SceneManagement;

public class CarSpawner : MonoBehaviour
{
    [Header("Car Spawner Asset")]
    public string localFilePath = "Assets/Text Files/CarSpawner.csv";

    [Header("Car Models and Scales")]
    public List<CarController> cars;
    public float carScale;

    public bool constantSpeed = true;
    public Transform nodes90;
    public bool nodes90Reversed;
    public Transform nodesNeg90;
    public bool nodesNeg90Reversed;
    public Transform nodesParkingLot;


    string sceneName;
    Scene m_Scene;
    string FILE_NAME;


    List<CarController> carsSpawned = new List<CarController>();
    SortedDictionary<float, List<Dictionary<string, string>>> spawnedCarTimes = new SortedDictionary<float, List<Dictionary<string, string>>>();

    int carsOnScreen;
    int carId = 0;
    int routinesRunning = 0;
    string folderLocation;

    // Start is called before the first frame update
    void Start()
    {
        m_Scene = SceneManager.GetActiveScene();
        sceneName = m_Scene.name;

        FILE_NAME = "cars_compiled_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + "_" + sceneName.ToString() + ".csv";
    }

    void ReadSpawnedCars()
    {
        var records = Utils.GetCSVRecords(localFilePath);
        int id = 0;
        foreach(var record in records)
        {
            float time = float.Parse(record["SpawnTime"]);
            if (!spawnedCarTimes.ContainsKey(time))
                spawnedCarTimes.Add(time, new List<Dictionary<string, string>>());
            record["Id"] = id.ToString();
            spawnedCarTimes[time].Add(record);
            id++;
        }
    }


    
    private void OnApplicationQuit()
    {
        Thread t = new Thread(CompileFiles);
        t.Start();
    }

    public void CompileFiles()
    {
        string carData = Path.Combine(Utils.GetOrCreateDataFolder(), "Car Data");
        //string newFile = Path.Combine(Utils.GetOrCreateDataFolder(), "cars_compiled.csv");
        string newFile = Path.Combine(Utils.GetOrCreateDataFolder(), FILE_NAME);

        using (StreamWriter dataWriter = new StreamWriter(File.Open(newFile, FileMode.OpenOrCreate, FileAccess.Write)))
        {
            dataWriter.Write("Id,Timestamp,DateTime,Speed,LocationX,LocationY,LocationZ,Model");
            //Go through all files 
            string[] filePaths = Directory.GetFiles(carData);
            List<Dictionary<string, string>> records = new List<Dictionary<string, string>>();
            int id = 0;
            foreach (string file in filePaths)
            {
                records.Clear();
                records = Utils.GetCSVRecords(file);
                foreach (var record in records)
                {
                    dataWriter.Write(string.Format("\n{0},{1},{2},{3},{4},{5},{6},{7}",
                        id, record["Timestamp"], record["DateTime"], record["Speed"], record["LocationX"], record["LocationY"], record["LocationZ"], record["Model"]));
                }
                id++;
            }
        }
    }

    

    private void OnEnable()
    {
        folderLocation = Utils.GetOrCreateDataFolder();
        StartCoroutine(Spawn());
        VehicleParkingLotState.exitedParkingLot += onDriveState;
    }

    private void OnDisable() {
        VehicleParkingLotState.exitedParkingLot -= onDriveState;
    }

    IEnumerator Spawn()
    {
        ReadSpawnedCars();
        while(true)
        {
            float delay = 0;

            foreach(var kv in spawnedCarTimes)
            {
                delay = kv.Key - delay;
                yield return new WaitForSeconds(delay);
                for (int i=0;i<kv.Value.Count;i++) {
                    // spawn cars at this time index
                    try {
                        SpawnCar(kv.Value[i]);
                    }
                    catch (ArgumentOutOfRangeException e) {
                        Debug.Log("Spawn Car Index (" + i + ") out of Range: " + e);
                    }
                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield return new WaitForSeconds(5);
        }
    }

    public void RemoveCar(CarController controller)
    {
        if (carsSpawned.Contains(controller))
        {
            carsOnScreen--;
            carsSpawned.Remove(controller);
        }
    }

    Transform[] GetChildren(Transform parent, bool reverse)
    {
        Transform[] children = new Transform[parent.childCount];
        int i = reverse ? children.Length - 1 : 0;
        int actualI = 0;

        while(actualI < children.Length)
        {
            children[actualI] = parent.GetChild(i);

            if(reverse)
            {
                i--;
            }
            else
            {
                i++;
            }
            actualI++;
        }
        return children;
    }

    private void onDriveState(GameObject carObject) {
        CarController spawnCar = carObject.GetComponent<CarController>();
        //spawnCar.Nodes = GetChildren(nodes90, nodes90Reversed);
    } 

    bool SpawnCar(Dictionary<string, string> carSpawned)
    {
        Vector3 scale = transform.localScale;
        Vector3 carSize = cars[0].sizeCollider.size * carScale * 2.5f;
        Vector3 positionSpawn = transform.position;

        //string[] positionStr = carSpawned["Position"].Split(";"[0]);
        float rotation = float.Parse(carSpawned["Rotation"]);
        //positionSpawn = new Vector3(float.Parse(positionStr[0]), float.Parse(positionStr[1]), float.Parse(positionStr[2]));
        positionSpawn = new Vector3(float.Parse(carSpawned["PosX"]), float.Parse(carSpawned["PosY"]), float.Parse(carSpawned["PosZ"]));
        var overlap = Physics.OverlapBox(positionSpawn, carSize, Quaternion.Euler(0f, rotation, 0f), 1 << cars[0].carLayer);
        foreach (Collider c in overlap)
        {
            if (c.GetComponentInParent<CarController>())
            {
                return false;
            }
        }

        Debug.Log("Spawning car");
        RaycastHit hit;
        if (Physics.Raycast(positionSpawn + new Vector3(0f, 10f, 0f), Vector2.down, out hit, Mathf.Infinity))
        {
            positionSpawn = hit.point + new Vector3(0f, carSize.y / 4f, 0f);
        }

        int model = int.Parse(carSpawned["VehicleModel"]);


        CarController spawnCar = Instantiate(cars[model], positionSpawn, Quaternion.Euler(0f, rotation, 0f)).GetComponent<CarController>();
        spawnCar.name = "Car-"+ spawnCar.GetInstanceID();
        spawnCar.Nodes = rotation == 90f ? GetChildren(nodes90, nodes90Reversed) : GetChildren(nodesNeg90, nodesNeg90Reversed);
        spawnCar.transform.localScale *= carScale;     
        spawnCar.speed = float.Parse(carSpawned["Speed"]);
        spawnCar.brakeDistance = float.Parse(carSpawned["BrakeDistance"]);
        spawnCar.CarId = carId;
        spawnCar.brakeSpeed = float.Parse(carSpawned["BrakeSpeed"]);
        spawnCar.FolderLocation = folderLocation;
        spawnCar.Model = model;
        spawnCar.constantSpeed = constantSpeed;
        switch(int.Parse(carSpawned["State"])) {
            case 0:
                spawnCar.SwitchState(spawnCar.vehicleParkedState);
                break;
            case 1:
                spawnCar.SwitchState(spawnCar.vehicleBrakeState);
                break;
            case 2:
                spawnCar.Nodes = GetChildren(nodesParkingLot, false);
                spawnCar.SwitchState(spawnCar.vehicleParkingLotState);
                break;

        }
        spawnCar.Destroyed = () => {
            if(carsSpawned.Contains(spawnCar))
                carsOnScreen--;
        };
        carsSpawned.Add(spawnCar);
        carsOnScreen++;
        carId++;
        spawnCar.initNodes();
        return true;
    }

    bool SpawnParkedCar(Dictionary<string, string> carSpawned) {
        Vector3 scale = transform.localScale;
        Vector3 carSize = cars[0].sizeCollider.size * carScale * 2.5f;
        Vector3 positionSpawn = transform.position;

        string[] positionStr = carSpawned["Position"].Split(";"[0]);
        float rotation = float.Parse(carSpawned["Rotation"]);
        positionSpawn = new Vector3(float.Parse(positionStr[0]), float.Parse(positionStr[1]), 461.99f);
        //positionSpawn = new Vector3(174f,224f,229f);
        var overlap = Physics.OverlapBox(positionSpawn, carSize, Quaternion.Euler(0f, rotation, 0f), 1 << cars[0].carLayer);
        foreach (Collider c in overlap)
        {
            if (c.GetComponentInParent<CarController>())
            {
                return false;
            }
        }

        Debug.Log("Spawning car");
        RaycastHit hit;
        if (Physics.Raycast(positionSpawn + new Vector3(0f, 10f, 0f), Vector2.down, out hit, Mathf.Infinity))
        {
            positionSpawn = hit.point + new Vector3(0f, carSize.y / 4f, 0f);
        }

        int model = int.Parse(carSpawned["VehicleModel"]);


        CarController spawnCar = Instantiate(cars[model], positionSpawn, Quaternion.Euler(0f, rotation, 0f)).GetComponent<CarController>();
        Debug.Log(carSpawned["Position"]);
        spawnCar.Nodes = rotation == 90f ? GetChildren(nodes90, nodes90Reversed) : GetChildren(nodesNeg90, nodesNeg90Reversed);
        spawnCar.transform.localScale *= carScale;     
        spawnCar.speed = float.Parse(carSpawned["Speed"]);
        spawnCar.brakeDistance = float.Parse(carSpawned["BrakeDistance"]);
        spawnCar.CarId = carId;
        spawnCar.brakeSpeed = float.Parse(carSpawned["BrakeSpeed"]);
        spawnCar.FolderLocation = folderLocation;
        spawnCar.Model = model;
        spawnCar.constantSpeed = constantSpeed;
        spawnCar.SwitchState(spawnCar.vehicleParkedState);
        spawnCar.Destroyed = () => {
            if(carsSpawned.Contains(spawnCar))
                carsOnScreen--;
        };
        carsSpawned.Add(spawnCar);
        spawnCar.SwitchState(spawnCar.vehicleParkedState);
        carsOnScreen++;
        carId++;
        return true;
    }
}
