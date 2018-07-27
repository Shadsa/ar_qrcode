using System;
using UnityEngine;
using UnityEngine.EventSystems;

//Class used for store interface and manipulation object definition across differents scripts.



///<summary>
///Interface to implement to react to focus enter/exit
///</summary>

public interface IFocusable : IEventSystemHandler
{
    void OnFocusEnter();
    void OnFocusExit();
}

public enum UIStatus
{
    MainSelectorView,
    SensorView,
    GraphView
}





/// <summary>
/// Sensor definition documention :
/// All the code below is a definition of structure concerning the Sensor object.
/// </summary>


public struct Measure
{
    public DateTime date;
    public float value;
}

public struct SensorMapResults
{
    public string sensor_title;
    public string measure_title;
    public Measure[] measures;
}

public class Sensor
{

    public string device_SP_assetTag;
    public string device_SP_sysID;
    public string device_SP_context;
    public string device_SP_description;
    public string device_SP_altitude;
    public string device_SP_localization;
    public string device_SP_active;
    public string device_assetTag;
    public string device_sysId;
    public string device_dev_EUI;
    public string device_context;
    public string device_last_uplink_time;
    public string device_type_name;
    public string device_type_category;
    public string device_type_code;
    public string device_type_context;
    public string sensor_mapping_size;
    public string dtStart;
    public string dtEnd;
    public SensorMapResults[] result;




    public static Sensor SensorFromJson(string s)
    {
        return JsonUtility.FromJson<Sensor>(s);

    }


}

