using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System;
public abstract class Controller : MonoBehaviour
{
    [Tooltip("Desired speed of vehicle in MPH")]
    public float speed;
    public bool exportData = true;
    public bool setDirectionToCurrentFacing = true;
    public new Rigidbody rigidbody;
    public Vector3 directionMove;

    protected float realTime;
    protected Transform initialTransform;
    protected float startSpeed;
    protected bool shouldRunThread;
    Thread encoderThread;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (Application.isPlaying)
        {
            Transform transformUse = transform;

            startSpeed = speed;

            initialTransform = new GameObject("Initial Transform").transform;
            initialTransform.parent = transformUse.parent;
            initialTransform.position = transformUse.position;
            initialTransform.rotation = transformUse.rotation;
            initialTransform.localScale = transformUse.localScale;

            if (setDirectionToCurrentFacing)
                directionMove = transformUse.TransformDirection(new Vector3(0f, 0f, 1f));
            if (exportData)
                ExportData();
        }
    }


    public void IncrementSpeed(float speedIncrement)
    {
     //   Debug.Log("Incrementing speed!");
        speed += speedIncrement;
    }

    protected virtual void OnDestroy()
    {
        shouldRunThread = false;
        if (initialTransform)
            Destroy(initialTransform.gameObject);
    }


    protected virtual void Update() 
    {
        float realTimeNow = Time.realtimeSinceStartup;
        float deltaRealTime = realTimeNow - realTime;
        realTime = realTimeNow;

        Vector3 speedVelocity = directionMove * speed;
        if (rigidbody && !rigidbody.gameObject.name.Contains("Car"))
        {
            rigidbody.velocity = speedVelocity;
        }
        else
            transform.position += speedVelocity * deltaRealTime;
    }

    private void OnTriggerEnter(Collider collision)
    {
        Controller c = collision.gameObject.GetComponentInParent<Controller>();
        if (c)
            CollidedWith(c);
    }

    protected virtual void CollidedWith(Controller otherController)
    {
        
    }

    #region Data Export 
    public virtual void ExportData()
    {
        string fileName = FileName();
        fileName = Path.Combine(Utils.GetOrCreateDataFolder(), fileName);

        if (!File.Exists(fileName))
        {
            string heading = Headers();
            using (StreamWriter dataWriter = new StreamWriter(File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write)))
            {
                dataWriter.Write(heading);
            }
        }

        shouldRunThread = true;

        encoderThread = new Thread(WriteData);
        encoderThread.Priority = System.Threading.ThreadPriority.BelowNormal;
        encoderThread.Start(new object[] { fileName, this, new List<string>() });
    }


    public void WriteData(object data)
    {
        while (true)
        {
            Thread.Sleep(50);
            object[] dataParsed = (object[])data;
            string fileName = dataParsed[0].ToString();
            Controller controller = dataParsed[1] as Controller;
            List<string> queue = dataParsed[2] as List<string>;

            if (controller == null || !controller.shouldRunThread)
            {
                return;
            }
            string strWrite = controller.WriteString();
            try
            {
                foreach (string str in queue)
                    strWrite = str + strWrite;
                File.AppendAllText(fileName, strWrite);
                queue.Clear();
            }
            catch (IOException)
            {
                queue.Add(strWrite);
            }
        }
    }

    protected virtual string WriteString()
    {
        return "";
    }

    protected virtual string FileName()
    {
        return "";
    }

    protected virtual string Headers()
    {
        return "";
    }

    #endregion

}
