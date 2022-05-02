using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AlarmHand : MonoBehaviour
{
    //0 - hour, 1 - min
    [SerializeField][Range(0, 1)] int handType;
    bool isRotate = false;
    const float hoursDegrees = 360f / 12f,
        minutesDegrees = 360f / 60f;

    Transform parent;
    Clock clock;
    
    void Start()
    {
        parent = transform.parent;
        clock = FindObjectOfType<Clock>();
    }

    void Update()
    {
        RotationProcess();
    }

    private void RotationProcess()
    {
        if(!isRotate) { return; }
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = AngleBetweenTwoPoints(parent.position, mousePosition);

        parent.localRotation = Quaternion.Euler(new Vector3(0f, 0f, angle + 90));
    }
    float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }
    private void OnMouseDown()
    {
        isRotate = true;
        
    }
    private void OnMouseUp()
    {
        isRotate = false;
        switch (handType)
        {
            case 0:
                HoursHand();
                break;
            case 1:
                MinutesHand();
                break;
        }
        clock.GetAlarmTimeFromHands();
        //print(parent.eulerAngles.z);
    }
    public float GetAlarmAngle(int type)
    {
        switch (handType)
        {
            case 0:
                return Mathf.Round((parent.eulerAngles.z) / -hoursDegrees) + 12;
            default:
                return Mathf.Round((parent.eulerAngles.z) / -minutesDegrees) + 60;
                
        }
        
    }

    private void HoursHand()
    {
        float corPos = Mathf.Round((parent.eulerAngles.z ) / -hoursDegrees);
        //print(parent.eulerAngles.z + " - " + corPos);
        parent.localRotation = Quaternion.Euler(0f, 0f, (corPos * -hoursDegrees));
        //print(corPos + 12);
    }
    private void MinutesHand()
    {
        float corPos = Mathf.Round((parent.eulerAngles.z )  / -minutesDegrees);
        //print(time);
        parent.localRotation = Quaternion.Euler(0f, 0f, (corPos * -minutesDegrees));
        
    }
}
