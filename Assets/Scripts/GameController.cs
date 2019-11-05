using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public int targetFrameRate;
    private void Awake()
    {
#if UNITY_ANDROID
        Application.targetFrameRate = targetFrameRate;
        Screen.orientation = ScreenOrientation.Portrait;
#endif

#if UNITY_IOS
        Application.targetFrameRate = targetFrameRate;
        Screen.orientation = ScreenOrientation.Portrait;
#endif
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
