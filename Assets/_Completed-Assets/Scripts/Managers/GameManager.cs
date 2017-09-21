using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Complete;
using Nanolink;

namespace Manager {
	// 管理整个游戏的流程
    [RequireComponent(typeof(TanksManager))]
	public class GameManager : MonoBehaviour   
	{   
		[SerializeField] Text m_MessageText; 

        private TanksManager m_TanksManager;
        private NetworkingManager m_NetworkingManager = null;

		private void Start()
		{
			m_MessageText.text = string.Empty;
            m_TanksManager = GetComponent<TanksManager>();
            m_NetworkingManager = NetworkingManager.getInstance();
            if (m_NetworkingManager != null) {
                m_NetworkingManager.m_ConnectedEvent.AddListener(initScene);
                m_NetworkingManager.init();
            }
		}

		private void FixedUpdate()
		{
			if (m_NetworkingManager != null)
			{
				m_NetworkingManager.doUpdate();
			}
		}

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)){
                m_NetworkingManager.Disconnected();
                Application.Quit();
            }
                
        }

        private void initScene(byte index) {
			m_TanksManager.m_OverEvent.AddListener (GameOver);
            m_TanksManager.m_RelifeEvent.AddListener(GameRelife);
			m_TanksManager.createSelfTank (index);
		}

        void GameOver (string content) {
            m_MessageText.gameObject.SetActive(true);
          
            m_MessageText.text = content;
		}

        void GameRelife () {
            m_MessageText.gameObject.SetActive(false);
        }

        private void OnGUI()
        {
            if (m_NetworkingManager != null)
                m_NetworkingManager.drawGUI();
        }
    }	    
}
