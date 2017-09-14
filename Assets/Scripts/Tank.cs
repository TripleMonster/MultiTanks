using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour {

    enum TankState {
        TANK_IDLE,
        TANK_MOVE,
        TANK_RUN,
        TANK_SHOOT,
        TANK_DIE,
    }

    TankState m_CurrentState;

	void Start () {
		
	}
	
	void Update () {
        ChangeTankState(m_CurrentState);
	}

    void ChangeTankState (TankState state) {
        switch (state) {
            case TankState.TANK_IDLE:
                break;
            case TankState.TANK_MOVE:
                Move();
                break;
            case TankState.TANK_RUN:
                Run();
                break;
            case TankState.TANK_SHOOT:
                Shoot();
                break;
            case TankState.TANK_DIE:
                Die();
                break;
        }
    }

    void Move () {
        
    }

    void Run () {
        
    }

    void Shoot () {
        
    }

    void Die () {
        
    }
}
