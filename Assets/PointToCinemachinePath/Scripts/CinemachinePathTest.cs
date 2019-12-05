using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachinePathTest : MonoBehaviour
{
    [SerializeField] CinemachinePath path = null;
    [SerializeField] CinemachineDollyCart dollyCart = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //path.m_Waypoints;
        path.StandardizeUnit(0, CinemachinePathBase.PositionUnits.Distance);

        dollyCart.m_Speed = 0;
    }
}
