﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SLua;
public partial class LuaGame : MonoBehaviour
{
    LuaSvr l;
    int progress = 0;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start()
    {
        if (LuaFile.assetmode == 1)
        {
            var luabundle = AssetBundle.LoadFromFile(Application.dataPath + "/../StreamingAssets/lua.unity3d");
            LuaFile.AddLuaBundle(luabundle);
        }
        else
        {
            LuaFile.AddSearchPath(Application.dataPath + "/R/Lua",true); //lua逻辑代码目录

        }
        Application.logMessageReceived += OnLog;
        l = new LuaSvr();

        LuaSvr.mainState.loaderDelegate = OnLoad;

        l.init(OnTick, OnComplete, LuaSvrFlag.LSF_BASIC | LuaSvrFlag.LSF_EXTLIB);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnLog(string cond, string trace, LogType lt)
    {
        //Debug.Log(trace);
    }

    void OnTick(int p)
    {
        progress = p;
    }

    void OnComplete()
    {
        l.start("main");

        UnityEngine.SceneManagement.SceneManager.LoadScene("Example");
    }
#if UNITY_EDITOR
    void OnGUI()
    {
        if (progress != 100)
            GUI.Label(new Rect(0, 0, 100, 50), string.Format("Loading {0}%", progress));
    }
#endif

    byte[] OnLoad(string fn, ref string absoluteFn)
    {
        string path = LuaFile.FindFile(fn.Replace('.', '/'));

        byte[] bytes = LuaFile.ReadBytes(path);

        return bytes;
    }

    private void OnLevelWasLoaded(int level)
    {
        PlayerData data = new PlayerData();
        data.config = "Akali";
        data.animationLengths = new Dictionary<string, float> {
            { "attack1",1.250f },
            { "attack2",1.250f },
            { "dance",8.875f },
            { "die",1.750f },
            { "idle",1.250f },
            { "run",0.833f },
            { "sneak",0.583f },
            { "spell1",1.250f },
            { "spell3",1.250f }
        };
        PlayerManager.GetSingleton().CreatePlayer(data);
        
    }

}
