using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameManager : MonoBehaviour {

	private float AccumilatedTime = 0f;
	private float FrameLength = 0.05f;
	private int GameFrame = 0;
	private int GameFramesPerLocksetpTurn = 4;

	void Start () {
		
	}

	void Update () {
		AccumilatedTime = AccumilatedTime + Time.deltaTime;

		while (AccumilatedTime > FrameLength) {
			GameFrameTurn ();
			AccumilatedTime = AccumilatedTime - FrameLength;
		}
	}

	void GameFrameTurn () {
		//Debug.Log ("GameFrameTurn----------------");
		if (GameFrame == 0) {
			if (LockStepTurn()) {
				GameFrame++;
			}
		} else {

			GameFrame++;
            if (GameFrame == GameFramesPerLocksetpTurn) {
				GameFrame = 0;
			}
		}
	}

	bool LockStepTurn () {
		//Debug.Log ("是否可以下一回合");
		return true;
	}
}
