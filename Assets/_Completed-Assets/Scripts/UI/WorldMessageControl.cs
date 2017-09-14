using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMessageControl : MonoBehaviour {
    [SerializeField] Text _textPrefab;
    [SerializeField] float _destroyTime;

    int m_textCount;
    float m_yOffset;
    float m_deltTime;
    Queue<Text> m_textQueue = new Queue<Text>();
    Vector3 m_initPosition;
	void Start () {
        m_initPosition = _textPrefab.transform.localPosition;
	}
	
	void Update () {
        if (m_textQueue.Count > 0) {
            m_deltTime += Time.deltaTime;
            if (m_deltTime >= _destroyTime) {
                Text text = m_textQueue.Dequeue() as Text;
                Destroy(text.gameObject);
                m_deltTime = 0f;
            }
        }
	}

    public void setContent (string content) {
        m_yOffset = m_textCount * 30;

        Text text = Instantiate(_textPrefab) as Text;
        text.transform.parent = transform;
        text.transform.localPosition = m_initPosition - (new Vector3(0, m_yOffset, 0));
        text.text = content;
        text.gameObject.SetActive(true);
        m_textQueue.Enqueue(text);
        m_textCount = m_textCount > 7 ? 0 : m_textCount++;
    }
}
