using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoloToolkit.Unity;

using Newtonsoft.Json.Linq;

using UnityEngine;
using UnityEngine.UI;

public class UIDisplayerManager : Singleton<UIDisplayerManager>
{


    /// <summary>
    /// UI DOCUMENTATION :
    /// 
    /// The UI here is also the data controller due to the absence of real database. 
    /// 
    /// For the UI workflow : there is 3 state : 
    ///     - MainSelectorView which handle the sensor selection. Basicly it's the main menu of the application.
    ///     - MainSensor view which display information about only one sensor.
    ///     - MainGraph view which is in fact a zoom on the graph display in CaptorLastValue list.
    ///     
    /// It's a top-down workflow, so you can't access a graph panel from a selection view for exemple.
    /// 
    /// The UI also spread a lot of handler on object in order to capture gazing and taping gesture on different parts. 
    /// 
    /// Finally, the UI also handle API call due to the lack of a proper Monitor. The class could be rework in order to separate the two aspect.
    /// Basicly, some part of the code handle data while the other handle graphical render.
    /// 
    /// </summary>
    /// 

    //Global UI variable
    #region
    //Panel
    public UIStatus UIStatus = UIStatus.MainSelectorView;
    private int ActualSensorSelected = 0;
    private bool Scanning = false;
    private bool FoldUI = false;
    private GameObject UIGlobalContainer;
    private GameObject UIMainPanel;
    private GameObject UIMainSelectorView;
    private GameObject UIMainSensorView;
    private GameObject UIButtonsPanel;
    private GameObject UINotification;
    private GameObject UIConsole;
    private GameObject UIMainGraphView;
    private GameObject Graph;

    //Buttons
    private List<GameObject> Buttons = new List<GameObject>();
    private GameObject Fold;
    private GameObject Unfold;
    private GameObject Scan;
    private GameObject NoScan;
    private GameObject BackToMainSelectorView;
    private GameObject UpCaptorsList;
    private GameObject DownCaptorsList;

    //Specific fields in the sensor view
    private static int MAXLISTCAPTORSIZE = 3; //Max number of element that can be display in the Sub UI part of list
    private GameObject Title;
    private GameObject ServicePoint;
    private GameObject Uplink;
    private GameObject Status;
    private GameObject Description;
    private List<GameObject> CodeList = new List<GameObject>(); //Not implemented in UI!
    private List<GameObject> CaptorsTitle = new List<GameObject>();
    private List<GameObject> CaptorsLastValue = new List<GameObject>();
    private int IndexOfFirstCaptorLoadedInView = 0;
    private List<GameObject> MainSensorViewArea = new List<GameObject>();

    //Specific fields in the selection view
    public int MAXSENSOR = 4 * 4; // Done this way because it depend of how many sensor you could display in heigh and in widht. In this case, 4 by 7.   
    private List<GameObject> MainSelectorViewArea = new List<GameObject>();

    //Interactible
    private List<GameObject> Interactible = new List<GameObject>();

    //Dynamic UI variable   
    public int NotificationTimerTime = 3; // Time for a notification to be shown  
    public Color OveringColor = new Color(120, 233, 255, 255);
    public Color StandardColor = new Color(255, 255, 255, 255);
    private static string ABSENTVALUE = "_____";
    #endregion

    //Data Structures
    #region
    public List<Sensor> Sensors = new List<Sensor>(); //Contain all scanned Sensor, up to the last MAXSENSOR one.
    private Dictionary<GameObject, Texture2D> TextureStack = new Dictionary<GameObject, Texture2D>(); //Contain texture for graph, indexed by gameObject. Avoid unecessary call to API.
    #endregion

    //Haxs
    private Vector3 visible = new Vector3(1, 1, 1);
    private Vector3 hidden = new Vector3(0, 0, 0);
    public Texture2D test;
    public GameObject cube;

    //API communication
    public static string jsonTest = "{ \"device_SP_assetTag\": \"SP000301\", \"device_SP_sysID\": \"850706c761de2f160161de30a11222e6\", \"device_SP_context\": \"CTXCAF\", \"device_SP_description\": \"Gouter - tension batterie\", \"device_SP_altitude\": null, \"device_SP_localization\": null, \"device_SP_active\": null, \"device_assetTag\": \"TAG000000486\", \"device_sysId\": \"850706c75d564e1b015d56696abb3cf6\", \"device_dev_EUI\": \"A81758FFFE0309C1\", \"device_context\": \"CTXCAF\", \"device_last_uplink_time\": null, \"device_type_name\": \"Elsys ELT1 UBat\", \"device_type_category\": null, \"device_type_code\": \"ELT1STD\", \"device_type_context\": \"CTXCAF\", \"sensor_mapping_size\": 4, \"dtStart\": \"2018-06-06T05:00:00.000Z\", \"dtEnd\": \"2018-06-06T06:00:00.000Z\", \"result\": [ { \"sensor_title\": \"U batterie\", \"measure_title\": \"Electric potential (V)\", \"measures\": [ { \"date\": \"2018-06-06T05:00:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:01:00.000Z\", \"value\": 55.9 }, { \"date\": \"2018-06-06T05:02:00.000Z\", \"value\": 55.9 }, { \"date\": \"2018-06-06T05:03:00.000Z\", \"value\": 50.2 }, { \"date\": \"2018-06-06T05:04:00.000Z\", \"value\": 51.9 }, { \"date\": \"2018-06-06T05:05:00.000Z\", \"value\": 52.9 }, { \"date\": \"2018-06-06T05:06:00.000Z\", \"value\": 53.7 }, { \"date\": \"2018-06-06T05:07:00.000Z\", \"value\": 56.6 }, { \"date\": \"2018-06-06T05:08:00.000Z\", \"value\": 57.5 }, { \"date\": \"2018-06-06T05:09:00.000Z\", \"value\": 57.4 }, { \"date\": \"2018-06-06T05:10:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:11:00.000Z\", \"value\": 55.9 }, { \"date\": \"2018-06-06T05:12:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:13:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:14:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:15:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:16:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:17:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:18:00.000Z\", \"value\": 55.9 }, { \"date\": \"2018-06-06T05:19:00.000Z\", \"value\": 55.8 }, { \"date\": \"2018-06-06T05:20:00.000Z\", \"value\": 57.6 }, { \"date\": \"2018-06-06T05:21:00.000Z\", \"value\": 57.5 }, { \"date\": \"2018-06-06T05:22:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:23:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:24:00.000Z\", \"value\": 56.4 }, { \"date\": \"2018-06-06T05:25:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:26:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:27:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:28:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:29:00.000Z\", \"value\": 57.2 }, { \"date\": \"2018-06-06T05:30:00.000Z\", \"value\": 55.7 }, { \"date\": \"2018-06-06T05:31:00.000Z\", \"value\": 57.6 }, { \"date\": \"2018-06-06T05:32:00.000Z\", \"value\": 57.5 }, { \"date\": \"2018-06-06T05:33:00.000Z\", \"value\": 55.9 }, { \"date\": \"2018-06-06T05:34:00.000Z\", \"value\": 57.5 }, { \"date\": \"2018-06-06T05:35:00.000Z\", \"value\": 57 }, { \"date\": \"2018-06-06T05:36:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:37:00.000Z\", \"value\": 57.2 }, { \"date\": \"2018-06-06T05:38:00.000Z\", \"value\": 56.6 }, { \"date\": \"2018-06-06T05:39:00.000Z\", \"value\": 57.5 }, { \"date\": \"2018-06-06T05:40:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:41:00.000Z\", \"value\": 56.9 }, { \"date\": \"2018-06-06T05:42:00.000Z\", \"value\": 56 }, { \"date\": \"2018-06-06T05:43:00.000Z\", \"value\": 56.5 }, { \"date\": \"2018-06-06T05:44:00.000Z\", \"value\": 57.2 }, { \"date\": \"2018-06-06T05:45:00.000Z\", \"value\": 57.2 }, { \"date\": \"2018-06-06T05:46:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:47:00.000Z\", \"value\": 55.9 }, { \"date\": \"2018-06-06T05:48:00.000Z\", \"value\": 55.8 }, { \"date\": \"2018-06-06T05:49:00.000Z\", \"value\": 55.9 }, { \"date\": \"2018-06-06T05:50:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:51:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:52:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:53:00.000Z\", \"value\": 56.2 }, { \"date\": \"2018-06-06T05:54:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:55:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:56:00.000Z\", \"value\": 57.2 }, { \"date\": \"2018-06-06T05:57:00.000Z\", \"value\": 57.1 }, { \"date\": \"2018-06-06T05:58:00.000Z\", \"value\": 56.2 }, { \"date\": \"2018-06-06T05:59:00.000Z\", \"value\": 57.1 } ] }, { \"sensor_title\": \"Temp Local \", \"measure_title\": \"Temperature (°C)\", \"measures\": [ { \"date\": \"2018-06-06T05:00:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:01:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:02:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:03:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:04:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:05:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:06:00.000Z\", \"value\": 17.6 }, { \"date\": \"2018-06-06T05:07:00.000Z\", \"value\": 17.6 }, { \"date\": \"2018-06-06T05:08:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:09:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:10:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:11:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:12:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:13:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:14:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:15:00.000Z\", \"value\": 17.8 }, { \"date\": \"2018-06-06T05:16:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:17:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:18:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:19:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:20:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:21:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:22:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:23:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:24:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:25:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:26:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:27:00.000Z\", \"value\": 17.6 }, { \"date\": \"2018-06-06T05:28:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:29:00.000Z\", \"value\": 17.6 }, { \"date\": \"2018-06-06T05:30:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:31:00.000Z\", \"value\": 17.6 }, { \"date\": \"2018-06-06T05:32:00.000Z\", \"value\": 17.6 }, { \"date\": \"2018-06-06T05:33:00.000Z\", \"value\": 17.6 }, { \"date\": \"2018-06-06T05:34:00.000Z\", \"value\": 17.6 }, { \"date\": \"2018-06-06T05:35:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:36:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:37:00.000Z\", \"value\": 17.6 }, { \"date\": \"2018-06-06T05:38:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:39:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:40:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:41:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:42:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:43:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:44:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:45:00.000Z\", \"value\": 17.7 }, { \"date\": \"2018-06-06T05:46:00.000Z\", \"value\": 17.8 }, { \"date\": \"2018-06-06T05:47:00.000Z\", \"value\": 17.8 }, { \"date\": \"2018-06-06T05:48:00.000Z\", \"value\": 17.8 }, { \"date\": \"2018-06-06T05:49:00.000Z\", \"value\": 17.8 }, { \"date\": \"2018-06-06T05:50:00.000Z\", \"value\": 17.8 }, { \"date\": \"2018-06-06T05:51:00.000Z\", \"value\": 17.8 }, { \"date\": \"2018-06-06T05:52:00.000Z\", \"value\": 17.8 }, { \"date\": \"2018-06-06T05:53:00.000Z\", \"value\": 17.8 }, { \"date\": \"2018-06-06T05:54:00.000Z\", \"value\": 17.9 }, { \"date\": \"2018-06-06T05:55:00.000Z\", \"value\": 17.9 }, { \"date\": \"2018-06-06T05:56:00.000Z\", \"value\": 17.9 }, { \"date\": \"2018-06-06T05:57:00.000Z\", \"value\": 17.9 }, { \"date\": \"2018-06-06T05:58:00.000Z\", \"value\": 17.9 }, { \"date\": \"2018-06-06T05:59:00.000Z\", \"value\": 18 } ] }, { \"sensor_title\": \"Humidity Local \", \"measure_title\": \"Humidity (%)\", \"measures\": [ { \"date\": \"2018-06-06T05:00:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:01:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:02:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:03:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:04:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:05:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:06:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:07:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:08:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:09:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:10:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:11:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:12:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:13:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:14:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:15:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:16:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:17:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:18:00.000Z\", \"value\": 26 }, { \"date\": \"2018-06-06T05:19:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:20:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:21:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:22:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:23:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:24:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:25:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:26:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:27:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:28:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:29:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:30:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:31:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:32:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:33:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:34:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:35:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:36:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:37:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:38:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:39:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:40:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:41:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:42:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:43:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:44:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:45:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:46:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:47:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:48:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:49:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:50:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:51:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:52:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:53:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:54:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:55:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:56:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:57:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:58:00.000Z\", \"value\": 27 }, { \"date\": \"2018-06-06T05:59:00.000Z\", \"value\": 27 } ] }, { \"sensor_title\": \"Sensor bat \", \"measure_title\": \"Electric potential (V)\", \"measures\": [ { \"date\": \"2018-06-06T05:00:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:01:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:02:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:03:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:04:00.000Z\", \"value\": 3.569 }, { \"date\": \"2018-06-06T05:05:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:06:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:07:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:08:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:09:00.000Z\", \"value\": 3.567 }, { \"date\": \"2018-06-06T05:10:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:11:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:12:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:13:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:14:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:15:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:16:00.000Z\", \"value\": 3.567 }, { \"date\": \"2018-06-06T05:17:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:18:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:19:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:20:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:21:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:22:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:23:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:24:00.000Z\", \"value\": 3.567 }, { \"date\": \"2018-06-06T05:25:00.000Z\", \"value\": 3.567 }, { \"date\": \"2018-06-06T05:26:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:27:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:28:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:29:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:30:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:31:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:32:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:33:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:34:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:35:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:36:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:37:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:38:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:39:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:40:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:41:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:42:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:43:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:44:00.000Z\", \"value\": 3.569 }, { \"date\": \"2018-06-06T05:45:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:46:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:47:00.000Z\", \"value\": 3.567 }, { \"date\": \"2018-06-06T05:48:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:49:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:50:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:51:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:52:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:53:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:54:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:55:00.000Z\", \"value\": 3.567 }, { \"date\": \"2018-06-06T05:56:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:57:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:58:00.000Z\", \"value\": 3.564 }, { \"date\": \"2018-06-06T05:59:00.000Z\", \"value\": 3.561 } ] } ] }";
#if !UNITY_EDITOR
    private REQUEA_LIB api = new REQUEA_LIB();
#endif
    //Event throw by UI
    #region
    public delegate void UIEvent();
    public event UIEvent StopScanEvent;
    public event UIEvent StartScanEvent;
    #endregion

    //UI Functions
    #region
    private void UpdateUI()
    {
        if (FoldUI)
        {
            UIMainPanel.SetActive(false);
            Fold.SetActive(false);
            Unfold.SetActive(true);

        }
        else
        {
            UIMainPanel.SetActive(true);
            Fold.SetActive(true);
            Unfold.SetActive(false);
        }

        if (Scanning)
        {
            Scan.SetActive(false);
            NoScan.SetActive(true);
        }
        else
        {
            Scan.SetActive(true);
            NoScan.SetActive(false);
        }

        if (UIStatus == UIStatus.MainSelectorView)
        {
            UIMainSensorView.SetActive(false);
            UIMainSelectorView.SetActive(true);
            UIMainGraphView.SetActive(false);
        }
        else if (UIStatus == UIStatus.SensorView)
        {
            UIMainSensorView.SetActive(true);
            UIMainSelectorView.SetActive(false);
            UIMainGraphView.SetActive(false);

        }
        else
        {
            UIMainSensorView.SetActive(false);
            UIMainSelectorView.SetActive(false);
            UIMainGraphView.SetActive(true);
        }


    }

    private void LoadUI()
    {

    }

    private void HideUI(GameObject go)
    {
        FoldUI = true;
        UpdateUI();
    }

    private void ShowUI(GameObject go)
    {
        FoldUI = false;
        UpdateUI();
    }

    //Interesting code for handle a timer in UNity for UI element.
    private void PopNotification(string s)
    {
        StartCoroutine(ShowMessage(s));
    }
    IEnumerator ShowMessage(string s)
    {

        UINotification.GetComponent<Text>().text = s;
        UINotification.GetComponent<Text>().enabled = true;
        yield return new WaitForSeconds(NotificationTimerTime);
        UINotification.GetComponent<Text>().enabled = false;

    }

    //For scan function, don't forget to send a message to the ScannerManager in order to stop the scanner.
    private void StartScan(GameObject go)
    {
        PopNotification("Scan démarré");
        Scanning = true;
        if (StartScanEvent != null)
        {
            StartScanEvent();
        }
    }

    private void EndScan(GameObject go)
    {
        PopNotification("Scan arreté");
        Scanning = false;
        if (StopScanEvent != null)
        {
            StopScanEvent();
        }
    }

    private  void ScrollUpCaptors(GameObject go)
    {
         DataFiller(IndexOfFirstCaptorLoadedInView - MAXLISTCAPTORSIZE);
    }

    private  void ScrollDownCaptors(GameObject go)
    {
         DataFiller(IndexOfFirstCaptorLoadedInView + MAXLISTCAPTORSIZE);
    }

    #endregion

    //Navigation Functions
    #region
    private void SwitchToMainSelectorView(GameObject go)//go useless in the main selector
    {
        UIStatus = UIStatus.MainSelectorView;
        UIMainSensorView.SetActive(false);
        UIMainGraphView.SetActive(false);
        UIMainSelectorView.SetActive(true);

        //NO data filler because we don't want to overcompute at each flow. The scan must be done in the main frame. If you want to change it, uncomment the datafiller
        //await DataFiller();
    }

    private  void SwitchToMainSensorView(GameObject go)//go usefull here, will say from wich we retrieve data
    {
        ActualSensorSelected = MainSelectorViewArea.IndexOf(go);
        print(ActualSensorSelected);
        UIStatus = UIStatus.SensorView;
        
        UIMainSensorView.SetActive(true);
        UIMainGraphView.SetActive(false);
        UIMainSelectorView.SetActive(false);
         DataFiller();


    }

    private void SwitchToMainGraphView(GameObject go)//go usefull here, pass the captor last value object, wich contain the texture
    {
        print(go.name);
        UIStatus = UIStatus.GraphView;
        UIMainSensorView.SetActive(false);
        try
        {
            UIMainGraphView.GetComponentInChildren<RawImage>().texture = TextureStack[go]; // Texture stack avoid call to .find, so it's a small buffer that hel pperformance.
        }
        catch
        {
            UIMainGraphView.GetComponentInChildren<RawImage>().texture = null;
        }
        UIMainGraphView.SetActive(true);
        UIMainSelectorView.SetActive(false);

    }

    private void CloseMainGraphView(GameObject go)//Do the same job then SwitchToMainSensorView but do not call Datafiller, so we don't refresh the data of the sensor. avoid unecessary computation.
    {
        UIStatus = UIStatus.SensorView;
        UIMainSensorView.SetActive(true);
        UIMainGraphView.SetActive(false);
        UIMainSelectorView.SetActive(false);
    }
    #endregion

    //Data Populating and Data control functions
    #region
    // fill the first main selector or Sensor view with data. The version with lodadedIndex is mainly used for scroll captors data (load a specific array of captor into the Sensor view, Selector view unchange) // MIGHT NEED REWORK FOR DUPLICATE CODE
    private  void DataFiller()
    {

        if (UIStatus == UIStatus.MainSelectorView)
        {
            int i = 0;
            GameObject[] MainSelectorViewAreaArray = MainSelectorViewArea.ToArray();
            foreach (Sensor s in Sensors)
            {
                Text txt = MainSelectorViewAreaArray[i].GetComponent<Text>();
                txt.text = s.device_SP_description;
                i++;
            }
            while (i < MainSelectorViewAreaArray.Length)
            {
                Text txt = MainSelectorViewAreaArray[i].GetComponent<Text>();
                txt.text = ABSENTVALUE;
                i++;
            }
        }
        else if (UIStatus == UIStatus.SensorView)
        {
            Sensor s = null;
            try
            {
                s = Sensors[ActualSensorSelected];
                //Fill Display with information
                Title.GetComponent<Text>().text = s.device_SP_description;
                Description.GetComponent<Text>().text = ABSENTVALUE;
                if (s.device_last_uplink_time != null)
                    Uplink.GetComponent<Text>().text = s.device_last_uplink_time;
                else
                    Uplink.GetComponent<Text>().text = ABSENTVALUE;

                ServicePoint.GetComponent<Text>().text = s.device_SP_assetTag;
                int i = 0;
                foreach (SensorMapResults smr in s.result)
                {
                    if (i < MAXLISTCAPTORSIZE)
                    {
                        CaptorsTitle[i].GetComponent<Text>().text = smr.sensor_title;
                        ImageApiCall(CaptorsLastValue[i]);
                        
                    }
                    else
                    {
                        break;
                    }
                    i++;
                }
            }
            catch
            {
                //Just continue
                Title.GetComponent<Text>().text = ABSENTVALUE;
            }

        }


    }
    private  void DataFiller(int loadedIndex)
    {

        if (UIStatus == UIStatus.MainSelectorView)
        {
            int i = 0;
            GameObject[] MainSelectorViewAreaArray = MainSelectorViewArea.ToArray();
            foreach (Sensor s in Sensors)
            {
                Text txt = MainSelectorViewAreaArray[i].GetComponent<Text>();
                txt.text = s.device_SP_description;
                i++;
            }
            while (i < MainSelectorViewAreaArray.Length)
            {
                Text txt = MainSelectorViewAreaArray[i].GetComponent<Text>();
                txt.text = ABSENTVALUE;
                i++;
            }
        }
        else if (UIStatus == UIStatus.SensorView)
        {
            Sensor s = null;
            try
            {
                s = Sensors[ActualSensorSelected];
                //Fill Display with information
                Title.GetComponent<Text>().text = s.device_SP_description;
                Description.GetComponent<Text>().text = ABSENTVALUE;
                if (s.device_last_uplink_time != null)
                    Uplink.GetComponent<Text>().text = s.device_last_uplink_time;
                else
                    Uplink.GetComponent<Text>().text = ABSENTVALUE;

                ServicePoint.GetComponent<Text>().text = s.device_SP_assetTag;

                if ((IndexOfFirstCaptorLoadedInView + 2 <= s.result.Length - 1))
                {
                    for (int i = loadedIndex; i < MAXLISTCAPTORSIZE + loadedIndex; i++)
                    {
                        if (i <= s.result.Length - 1)
                        {
                            CaptorsTitle[i % MAXLISTCAPTORSIZE].GetComponent<Text>().text = s.result[i].sensor_title;
                            ImageApiCall(CaptorsLastValue[i % MAXLISTCAPTORSIZE]);
                            
                        }
                        else
                        {
                            CaptorsLastValue[i % MAXLISTCAPTORSIZE].GetComponent<RawImage>().texture = null;
                            CaptorsTitle[i % MAXLISTCAPTORSIZE].GetComponent<Text>().text = ABSENTVALUE;
                        }
                        i++;
                    }
                    IndexOfFirstCaptorLoadedInView = loadedIndex;
                }
                else
                {
                    print("Out of bound");
                }


            }
            catch
            {
                //Just continue
                Title.GetComponent<Text>().text = ABSENTVALUE;
            }

        }


    }
    // Clean useless data and make space in the first main selector view 
    private void DataCleaner()
    {
        //todo, clean all value.
    }

    //Add a sensor from a Json object or create it
    private  void AddSensor(string s)
    {
        if (!(Sensors.Count < MAXSENSOR))
        {
            Sensors.RemoveAt(0); //TEMPORAR !! ALWAY REMOVE THE FIRST ELEMENT. NEED TO IMPLEMENT ALGO OF REPLACEMENT.
        }
        Sensors.Add(CreateSensor(s));
         DataFiller();
    }
    private  void AddSensor(JObject json)
    {
        if (!(Sensors.Count < MAXSENSOR))
        {
            Sensors.RemoveAt(0); //TEMPORAR !! ALWAY REMOVE THE FIRST ELEMENT. NEED TO IMPLEMENT ALGO OF REPLACEMENT.
        }
        Sensors.Add(CreateSensor(json));
         DataFiller();
    }
    private  void AddSensor(Sensor sensor)
    {
        if (!(Sensors.Count < MAXSENSOR))
        {
            Sensors.RemoveAt(0); //TEMPORAR !! ALWAY REMOVE THE FIRST ELEMENT. NEED TO IMPLEMENT ALGO OF REPLACEMENT.
        }
        Sensors.Add(sensor);
         DataFiller();
    }
    private Sensor CreateSensor(JObject j)
    {
        return j.ToObject<Sensor>();
    }
    private Sensor CreateSensor(string s)
    {
        JObject j = JObject.Parse(s);
        return j.ToObject<Sensor>();
    }
    private bool IsDeviceTagExist(string tag)
    {
        bool r = false;
        if (Sensors.Count != 0)
        {
            foreach (Sensor s in Sensors)
            {
                if (s.device_assetTag == tag)
                    r = true;
            }
        }
        return r;
    }
    private bool IsSPTagExist(string tag)
    {
        bool r = false;
        if (Sensors.Count != 0)
        {
            foreach (Sensor s in Sensors)
            {
                if (s.device_SP_assetTag == tag)
                    r = true;
            }
        }

        return r;
    }
    #endregion

    //Utilities Function 
    #region
    private async void SensorApiCall(string s)
    {
        //UIConsole.GetComponent<Text>().text = ">From SensorApiCall function : API CALL";
#if !UNITY_EDITOR
        if (!IsSPTagExist(s))
        {
            try
            {
                var result = await api.GetSensorResponse(s);
                if (result != null)
                {
                    print("Resultat de la requête API :" + result.device_assetTag);
                    AddSensor(result);
                }
                else
                {
                    print("Requête API null");
                }
            }
            catch (Exception e)
            {
                print(e.InnerException.Message);
            }
        }
        else
        {
            print("SP already exist in the scope : API not called");
        }



#endif



    }

    private async void ImageApiCall(GameObject go)
    {
#if !UNITY_EDITOR
        var result = await api.GetChartsFromSensorOnSpecifiedCaptor(Sensors[ActualSensorSelected], CaptorsLastValue.IndexOf(go));
        if (result != null)
        {
            print("Retour d'API image avec un result non nul");
            if (go.GetComponent<RawImage>() != null)
            {
                go.GetComponent<RawImage>().texture = result;
                if (TextureStack.ContainsKey(go))
                {
                    TextureStack[go] = result;
                }
                else
                {
                    TextureStack.Add(go, result);
                }
            }

        }
        else
        {
            print("null was return on API call : somethings goes wrong");
        }

#endif
    }

    private async void ScanSucess(string s)
    {
         SensorApiCall(s);
    }

    #endregion

    // Use this for initialization    
    void Start()
    {

        UIGlobalEventProvider m;
        GameObject temp;


        //Find all main element of the interfaces ones, and store him for avoid futur .find call that our ressources consuming.
        UIGlobalContainer = GameObject.Find("MainMenu");
        UIMainPanel = GameObject.Find("MainPanel");
        UIMainSelectorView = GameObject.Find("MainSelectorView");
        UIMainSensorView = GameObject.Find("MainSensorView");
        UIButtonsPanel = GameObject.Find("ButtonMenu");
        UINotification = GameObject.Find("Notification");
        UIMainGraphView = GameObject.Find("MainGraphView");
        Graph = GameObject.Find("Graph");
        Fold = GameObject.Find("FoldButton");
        Unfold = GameObject.Find("UnfoldButton");
        Scan = GameObject.Find("ScanButton");
        NoScan = GameObject.Find("NoScanButton");
        BackToMainSelectorView = GameObject.Find("BackToMainSelectorView");
        UpCaptorsList = GameObject.Find("UpCaptorsList");
        DownCaptorsList = GameObject.Find("DownCaptorsList");
        Title = GameObject.Find("Title");
        Status = GameObject.Find("Status");
        ServicePoint = GameObject.Find("Service Point");
        Uplink = GameObject.Find("Uplink");
        Description = GameObject.Find("Description");
        UIConsole = GameObject.Find("Console");


        //Add several element into the interatctive part or other sub-classification in order to reuse it more simply in operation
        Buttons.Add(Fold); Interactible.Add(Fold);
        Buttons.Add(Unfold); Interactible.Add(Unfold);
        Buttons.Add(Scan); Interactible.Add(Scan);
        Buttons.Add(NoScan); Interactible.Add(NoScan);
        Buttons.Add(BackToMainSelectorView); Interactible.Add(BackToMainSelectorView); MainSensorViewArea.Add(BackToMainSelectorView);
        Buttons.Add(UpCaptorsList); Interactible.Add(UpCaptorsList); MainSensorViewArea.Add(UpCaptorsList);
        Buttons.Add(DownCaptorsList); Interactible.Add(DownCaptorsList); MainSensorViewArea.Add(DownCaptorsList);
        Interactible.Add(Graph);

        for (int i = 0; i < MAXSENSOR; i++)
        {
            temp = GameObject.Find("Sensor[" + i + "]");
            Interactible.Add(temp);
            MainSelectorViewArea.Add(temp);

        }

        for (int i = 0; i < MAXLISTCAPTORSIZE; i++)
        {
            temp = GameObject.Find("CaptorTitle[" + i + "]");
            CaptorsTitle.Add(temp);
            temp = GameObject.Find("CaptorLastValue[" + i + "]");
            CaptorsLastValue.Add(temp);
            Interactible.Add(temp);
        }

        //Add different interfaces on element, in order to provide them with different services such as overing, select, etc...
        foreach (GameObject go in Interactible)
        {
            go.AddComponent<UIGlobalEventProvider>();
            if (go.GetComponent<Renderer>() != null)
            {
                go.AddComponent<SpriteOveringInterface>();
            }
            else if (go.GetComponent<Text>() != null)
            {
                go.AddComponent<TextOveringInterface>();
            }
            else if (go.GetComponent<RawImage>() && go.name != "Graph")//add exception for the Graph view : Graph in blue is not the best ! So, we don't provide a visual feedback, but you just have to click anywhere or loose focus on in order to get back on Sensor view.
            {
                go.AddComponent<ImageOveringInterface>();
            }

        }


        //Register some component-specific callback across the UI using the previous given interfaces.
        m = Fold.GetComponent<UIGlobalEventProvider>();
        m.Selected += HideUI;
        m = Unfold.GetComponent<UIGlobalEventProvider>();
        m.Selected += ShowUI;
        m = Scan.GetComponent<UIGlobalEventProvider>();
        m.Selected += StartScan;
        m = NoScan.GetComponent<UIGlobalEventProvider>();
        m.Selected += EndScan;
        m = Graph.GetComponent<UIGlobalEventProvider>();
        m.Selected += CloseMainGraphView;
        m.Unfocused += CloseMainGraphView;


        //Add interaction on all button in the main menu
        foreach (GameObject go in MainSelectorViewArea)
        {
            m = go.GetComponent<UIGlobalEventProvider>();
            m.Selected += SwitchToMainSensorView;
        }
        //add interaction in the sub menu of a sensor
        foreach (GameObject go in MainSensorViewArea)
        {
            m = go.GetComponent<UIGlobalEventProvider>();
            if (go.name == "BackToMainSelectorView")
            {
                m.Selected += SwitchToMainSelectorView;
            }
            else if (go.name == "UpCaptorsList")
            {
                m.Selected += ScrollUpCaptors;
            }
            else if (go.name == "DownCaptorsList")
            {
                m.Selected += ScrollDownCaptors;
            }

        }
        //Add the zoom on graph in the Sensor view
        foreach (GameObject go in CaptorsLastValue)
        {
            m = go.GetComponent<UIGlobalEventProvider>();
            m.Selected += SwitchToMainGraphView;
        }

        //Subscribe to some event across the system (scanner one mainly)
        ScannerManager.Instance.ScanSucessfull += ScanSucess; //Send directly the result to API for analyse.

        //Update the UI with the final state of the initialisation
        SwitchToMainSelectorView(this.gameObject);//The object given in parameter is useless here
        
        print("UIManager enable");
        
        //For test purpose
        AddSensor(jsonTest);
        print("Senseur ajouté, utilisé la case 0");
        
        DataFiller();
        UpdateUI();

    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        UpdateUI();
    }

}

