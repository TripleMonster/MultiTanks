using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMessageControl : MonoBehaviour {
    [SerializeField] List<Text> _textList;

    private int m_curIndex = 0;

	void Start () {
		
	}
	
	void Update () {
		
	}

    public void setContent (string content) {
        Text text = _textList[m_curIndex];
        text.text = content;
        text.gameObject.SetActive(true);
        m_curIndex = m_curIndex < 4 ? (m_curIndex + 1) : 0;
    }
}
