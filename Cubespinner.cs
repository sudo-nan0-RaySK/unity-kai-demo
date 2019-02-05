using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;



public class Cubespinner : MonoBehaviour
{
    //Globals Declaration
    public WebSocket ws; 
    public float SPEED = 263.6f;
    public bool PINCHED = false;
    public bool GRABBED = false;

    public class PYRData
    {
        public float yaw, pitch , roll;

        public PYRData(float Yaw, float Pitch, float Roll){
            yaw=Yaw;
            pitch=Pitch;
            roll=Roll;
        }
    }
    public PYRData pyrData;
    public bool blue = true;

    public Quaternion quat = new Quaternion();
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("I am Alive!");
        Debug.Log("Client is starting....");
        ws = new WebSocket("ws://localhost:2203");
        pyrData = new PYRData(1,1,1);
        ws.OnMessage += (sender, e) =>
        {
            //Debug.Log(e.Data);

            var response = JObject.Parse(e.Data);
            var type = response["type"]?.ToObject<string>();

            switch (type)
            {
                case "authentication":
                    Debug.Log(" " + response["type"] + " " + response["success"]);
                    break;
                case "incomingData":
                    JArray dataArray = response["data"]?.ToObject<JArray>();

                    foreach (JObject dataObject in dataArray)
                    {
                        JObject dataObj = dataObject.ToObject<JObject>();
                        ;
                        switch (dataObj["type"]?.ToObject<string>())
                        {
                            case "pyrData":
                                HandlePyrData(dataObj);
                                break;
                            case "quaternionData":
                                HandleQuaternionData(dataObj);
                                break;
                            case "gestureData":
                                HandleGestureData(dataObj);
                                break;
                            case "fingerShortcutData":
                                HandleFingerShortcutData(dataObj);
                                break;
                        }
                    }

                    break;
            }

        };
        ws.Connect();
        ws.Send("{\"type\":\"authentication\" , \"moduleId\":\"uiid\" ,  \"moduleSecret\":\"qwerty\" }");
        ws.Send("{\"type\":\"setCapabilities\" , \"kaiId\":\"default\" ,  \"quaternionData\":true  , \"gestureData\":true , \"fingerShortcutData\":true , \"pyrData\":true }");
    }

    void HandlePyrData(JObject pyrObj)
    {
        var yaw = pyrObj["yaw"].ToObject<float>();
        var pitch = pyrObj["pitch"].ToObject<float>();
        var roll = pyrObj["roll"].ToObject<float>();
        Debug.Log(" "+yaw+" "+pitch+" "+roll);
        pyrData.yaw=yaw;
        pyrData.roll=roll;
        pyrData.pitch=pitch;
        Debug.Log("obj : "+pyrData.yaw+" "+pyrData.pitch+" "+pyrData.roll);
        //transform.Rotate(new Vector3(yaw,pitch,roll));
        // Do something with pyr here
    }

    void HandleQuaternionData(JObject quatObj)
    {
        var quaternion = quatObj["quaternion"]?.ToObject<JObject>();
        var w = quaternion["w"].ToObject<float>();
        var x = quaternion["x"].ToObject<float>();
        var y = quaternion["y"].ToObject<float>();
        var z = quaternion["z"].ToObject<float>();
        quat.Set(x, y, z, w);
        Debug.Log(" "+w+" "+x+" "+y+" "+z);
        // Do something with w, x, y, z here
    }

    void HandleGestureData(JObject gestureObj)
    {
        var gesture = gestureObj["gesture"]?.ToObject<string>();

        if(gesture=="pinch3Begin")
        {
            PINCHED=true;
        }
        else if(gesture=="pinch3End")
        {
            PINCHED=false;
        }
        // Do something with gesture here
    }

    void HandleFingerShortcutData(JObject fingerShortcutObj){

        JArray fingers = fingerShortcutObj["fingers"].ToObject<JArray>();

        foreach(bool finger in fingers){
            if(finger==false){
                GRABBED=false;
                return;
            }
        }
        GRABBED = true;
    }

    // Update is called once per frame
    void Update()
    {    
        if(GRABBED){
            transform.rotation=quat;
        }
        //Debug.Log("render : "+pyrData.yaw+" "+pyrData.pitch+" "+pyrData.roll);
        //transform.localScale = new Vector3(pyrData.pitch, pyrData.pitch, pyrData.pitch);
        if(PINCHED){
            Debug.Log("render : "+pyrData.yaw+" "+pyrData.pitch+" "+pyrData.roll);   
            transform.localScale = new Vector3(pyrData.pitch, pyrData.pitch, pyrData.pitch);
        }
        if (Input.GetKeyDown(KeyCode.Space)){
            if (blue)
            {
                GetComponent<Renderer>().material.color = new Color(255, 0, 0);
                blue = false;
            }
       
            else if (Input.GetKeyDown(KeyCode.Space))
       
            if (!blue)
            {
                GetComponent<Renderer>().material.color = new Color(0, 0, 255);
                blue = true;
            }
       }
    }
    //On Quit
    void OnApplicationQuit()
    {
        ws.Close();
    }
}
