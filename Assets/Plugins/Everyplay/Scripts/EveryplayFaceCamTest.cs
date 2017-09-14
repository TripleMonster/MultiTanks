using UnityEngine;
using System.Collections;

public class EveryplayFaceCamTest : MonoBehaviour
{
    private bool recordingPermissionGranted = false;
    private GameObject DebugMessage = null;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Everyplay.FaceCamRecordingPermission += CheckFaceCamRecordingPermission;
    }

    void Destroy()
    {
        Everyplay.FaceCamRecordingPermission -= CheckFaceCamRecordingPermission;
    }

    private void CheckFaceCamRecordingPermission(bool granted)
    {
        recordingPermissionGranted = granted;

        if (!granted && !DebugMessage)
        {
            DebugMessage = new GameObject("FaceCamDebugMessage", typeof(GUIText));
            DebugMessage.transform.position = new Vector3(0.5f, 0.5f, 0.0f);

            if (DebugMessage != null)
            {
                GUIText DebugMessageGuiText = DebugMessage.GetComponent<GUIText>();

                if (DebugMessageGuiText)
                {
                    DebugMessageGuiText.text = "Microphone access denied. FaceCam requires access to the microphone.\nPlease enable Microphone access from Settings / Privacy / Microphone.";
                    DebugMessageGuiText.alignment = TextAlignment.Center;
                    DebugMessageGuiText.anchor = TextAnchor.MiddleCenter;
                }
            }
        }
    }

    void OnGUI()
    {
        if (recordingPermissionGranted)
        {
            if (GUI.Button(new Rect(Screen.width - 10 - 158, 10, 158, 48), Everyplay.FaceCamIsSessionRunning() ? "Stop FaceCam session" : "Start FaceCam session"))
            {
                if (Everyplay.FaceCamIsSessionRunning())
                {
                    Everyplay.FaceCamStopSession();
                }
                else
                {
                    Everyplay.FaceCamStartSession();
                }
                #if UNITY_EDITOR
                //Debug.Log("Everyplay FaceCam is not available in the Unity editor. Please compile and run on a device.");
                #endif
            }
        }
        else
        {
            if (GUI.Button(new Rect(Screen.width - 10 - 158, 10, 158, 48), "Request REC permission"))
            {
                Everyplay.FaceCamRequestRecordingPermission();
                #if UNITY_EDITOR
                //Debug.Log("Everyplay FaceCam is not available in the Unity editor. Please compile and run on a device.");
                #endif
            }
        }
    }
}
