using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell{

    GameObject m_Instance;
    public ShellMove m_ShellMove;
    public Rigidbody m_Rigibody;
    public byte m_index;

    public void Init (GameObject instance, byte index) {
        m_Instance = instance;
        m_index = index;
        InitComponent();
    }

    public void ResetShell (Vector3 postion, Quaternion rotation) {
        m_Rigibody.position = postion;
        m_Rigibody.rotation = rotation;
    }

    void InitComponent () {
        m_Rigibody = m_Instance.GetComponent<Rigidbody>();
        m_ShellMove = m_Instance.GetComponent<ShellMove>();
    }

    public void GoFire (bool isFire) {
        m_ShellMove.SetIsFire(isFire);
    }

    public void SetFirePosition (Vector3 pos) {
        m_ShellMove.m_TargetPos = pos;
    }
}
