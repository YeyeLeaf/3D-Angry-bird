﻿using Cinemachine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class ThrowControllor : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Animator animator;
    [SerializeField] GameObject egg;
    public GameObject ThrowingObject;
    [SerializeField] Transform ThrowingOrient;
    [SerializeField] Transform Orient;
    public GameObject clonedObject;
    public float ThrowOffset = 0.1f;
    public CinemachineVirtualCamera ThrowCamera;
    public float ThrowPowerX = 30, ThrowPowerY = 30;

    bool PowerThrow = false;
    bool ThrowCameraActive = false;

    public GameObject birdCamera;

    void Start()
    {
        
    }

    private void Update()
    {
        ThrowCameraActive = CinemachineCore.Instance.IsLive(ThrowCamera);
        if (Input.GetKeyUp(KeyCode.Q)) 
        {
            ThrowCamera.Priority = ThrowCamera.Priority == 100 ? 0 : 100;
        }

        if (clonedObject != null)
        {
            Vector3 birdCameraPosition = new Vector3(clonedObject.transform.position.x + 10, clonedObject.transform.position.y, clonedObject.transform.position.z);
            birdCamera.GetComponent<Transform>().position = birdCameraPosition;
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        //偵測滑鼠左鍵被按下，0: 左鍵 1: 右鍵 2: 中鍵
        if(Input.GetMouseButtonDown(0) && !animator.GetBool("Throw") && ThrowCameraActive && clonedObject == null)
        {
            //動畫開始
            animator.SetBool("Throw", true);
            
            //設定投擲物的旋轉，使他面向投擲方向
            Vector3 ThrowRotate = ThrowingOrient.rotation.eulerAngles;
            ThrowRotate.x = 0;
            Orient.rotation = Quaternion.Euler(ThrowRotate);

            //偵測當前執行哪個動畫，以此決定幾秒後關閉Throw代表動畫結束
            if(PowerThrow)
                StartCoroutine(CloseThrow(3f));
            else
                StartCoroutine(CloseThrow(1.8f));
        }

        //較遠的投擲為按住左alt+左鍵並在此設定投擲的X、Y力量
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            PowerThrow = true;
            ThrowPowerX = 50;
            ThrowPowerY = 50;
        }
        else if(!animator.GetBool("Throw"))
        {
            PowerThrow = false;
            ThrowPowerX = 30;
            ThrowPowerY = 30;
        }
        
        animator.SetBool("PowerThrow", PowerThrow);
    }

    void ThrowObject()
    {
        //抓取投擲出去的位置，若直接在雞蛋的位置生成，會造成小雞先與玩家碰撞，因此+上微小的Offset解決此問題
        Vector3 temp = new Vector3(egg.GetComponent<Transform>().position.x, egg.GetComponent<Transform>().position.y + ThrowOffset, egg.GetComponent<Transform>().position.z + ThrowOffset);
        //關閉手中蛋的顯示，代表投擲出去了
        egg.SetActive(false);

        //如果有被投擲出去的小雞還沒刪除，先刪除再生成新小雞，避免物件越疊越多
        if (clonedObject != null)
        {
            Destroy(clonedObject);
        }
        clonedObject = Instantiate(ThrowingObject, temp, ThrowingOrient.transform.rotation);

        birdCamera.GetComponent<CinemachineVirtualCamera>().Follow = clonedObject.transform;
        birdCamera.GetComponent<CinemachineVirtualCamera>().LookAt = clonedObject.transform;
        birdCamera.GetComponent<CinemachineVirtualCamera>().Priority = 1000;
        birdCamera.GetComponent<BirdFollowCamera>().cloneObject = clonedObject;
        birdCamera.GetComponent<BirdFollowCamera>().Throw = true;

        //抓取小雞的Rigidbody，用來施加投擲力量
        Rigidbody ThrowObjectRb = clonedObject.GetComponent<Rigidbody>();
        ThrowObjectRb.AddForce(ThrowingOrient.forward * ThrowPowerX + ThrowingOrient.up * ThrowPowerY, ForceMode.Impulse);
    }

    IEnumerator CloseThrow(float delay)
    {
        //delay秒後執行下面的程式
        yield return new WaitForSeconds(delay);
        //關閉投擲動畫，使角色位置、視角可以移動
        animator.SetBool("Throw", false);
        PowerThrow = false;
    }
}
