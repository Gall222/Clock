using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    [SerializeField] Transform hoursHand;
    [SerializeField] Transform minutesHand;
    [SerializeField] Transform secondsHand;
    [SerializeField] Text numericClockText;
    [SerializeField] Text alarmClockInputText;
    [SerializeField] InputField alarmClockInput;
    [SerializeField] Button alarmClockResetButton;
    [SerializeField] GameObject alarmTextPrefab;
    [SerializeField] AlarmHand alarmHandHours;
    [SerializeField] AlarmHand alarmHandMinutes;


    int hours, minutes, seconds;
    int alarmHours, alarmMinutes;

    const float hoursDegrees = 360f / 12f,
        minutesDegrees = 360f / 60f,
        secondsDegrees = 360f / 60f;

    bool alarmClockOn = false;
    bool clockOn = true;


    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        StartCoroutine(GetGlobalTime1());
        StartCoroutine(SecondsTimer());
        alarmClockInput.onValueChanged.AddListener(AutocorrectAlarmText);
        alarmClockInput.onEndEdit.AddListener(EndAlarmText);
    }
    private void Update()
    {
        HandsRotation();
        NumericClockUpdate();
        AlarmClock();
    }
    private void AlarmClock()
    {
        if (alarmClockOn) 
        {
            alarmClockResetButton.enabled = true;
            if (alarmHours == hours && alarmMinutes == minutes)
            {
                Alarm();
            }
        }else
            alarmClockResetButton.enabled = false;

    }
    private void Alarm()
    {
        StartCoroutine(CreateAlarmText());
        ResetAlarm();
    }

    IEnumerator CreateAlarmText()
    {
        var alarmText = Instantiate(alarmTextPrefab, transform.position, Quaternion.identity.normalized);
        yield return new WaitForSeconds(3f);
        Destroy(alarmText.gameObject);
    }

    private void AutocorrectAlarmText(string value)
    {
        string result = "";
        for (int i = 0; i < value.Length; i++)
        {
            var tempChar = value.Substring(i, 1);
            int res;
            if (Int32.TryParse(tempChar, out res))
            {
                result += tempChar;
            }
            if (result.Length == 2)
            {
                result += ":";
                alarmClockInput.caretPosition = value.Length + 1;
            }
        }
        alarmClockInput.text = result;
    }
    private void EndAlarmText(string value)
    {
        if (value.Length >= 1)
        {
            CorrectAlarmTime();
            AddAlarmTime();
        }
        else
            alarmClockOn = false;
    }
    private void AddAlarmTime()
    {
        alarmClockOn = true;
        alarmHours = Int32.Parse(alarmClockInput.text.Substring(0, 2));
        alarmMinutes = Int32.Parse(alarmClockInput.text.Substring(3, 2));
        if (alarmHours >= 12)
        {
            alarmHours = 12;
        }  
        if (alarmMinutes >= 60)
        {
            alarmMinutes = 0;
        }
        SetAlarmHands();
        AddAlarmTimeText();
    }

    private void SetAlarmHands()
    {
        alarmHandHours.transform.parent.localRotation = Quaternion.Euler(0f, 0f, alarmHours * -hoursDegrees);
        alarmHandMinutes.transform.parent.localRotation = Quaternion.Euler(0f, 0f, alarmMinutes * -minutesDegrees);
    }

    public void AddAlarmTimeText()
    {
        alarmClockInput.text = CorrectTime(alarmHours) + ":" + CorrectTime(alarmMinutes);
    }
    private string CorrectTime(int time)
    {
        string correctTime = time.ToString();
        if (correctTime.Length == 1)
            correctTime = "0" + time;
        return correctTime;
    }
    private void CorrectAlarmTime()
    {
        if (alarmClockInput.text.Length == 1)
        {
            alarmClockInput.text = "0" + alarmClockInput.text + ":00";
        }
        if (alarmClockInput.text.Length == 3)
        {
            alarmClockInput.text += "00";
        }
        if (alarmClockInput.text.Length == 4)
        {
            alarmClockInput.text = Regex.Replace(alarmClockInput.text, ":", ":0");
        }
    }
    public void ResetAlarm()
    {
        alarmClockInput.text = "";
        alarmClockOn = false;
    }
    public void GetAlarmTimeFromHands()
    {
        alarmHours = (int)alarmHandHours.GetAlarmAngle(0);
        alarmMinutes = (int)alarmHandMinutes.GetAlarmAngle(1);
        alarmClockOn = true;
        AddAlarmTimeText();
    }
    private void NumericClockUpdate()
    {
        numericClockText.text = CorrectTime(hours) + ":" + CorrectTime(minutes) + ":" + CorrectTime(seconds);
    }
    private void HandsRotation()
    {
        hoursHand.localRotation = Quaternion.Euler(0f,0f, hours * -hoursDegrees);
        minutesHand.localRotation = Quaternion.Euler(0f,0f, minutes * -minutesDegrees);
        secondsHand.localRotation = Quaternion.Euler(0f,0f, seconds * -secondsDegrees);
        //print(secondsHand.transform.eulerAngles.z);
    }
    IEnumerator SecondsTimer()
    {
        while (clockOn)
        {
            yield return new WaitForSeconds(1f);
            seconds++;
            if (seconds >= 60)
            {
                seconds = 0;
                minutes++;
                if (minutes >= 60)
                {
                    minutes = 0;
                    hours++;
                    StartCoroutine(GetGlobalTime1());
                }
            }
        }
    }

    IEnumerator GetGlobalTime1()
    {
        string uri = "http://www.unn.ru/time/";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            string[] pages = uri.Split('/');
            int page = pages.Length - 1;
            
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    try
                    {
                        var timeString = System.Text.RegularExpressions.Regex.Match(webRequest.downloadHandler.text, @"id=""servertime"" >\s*(.*)</div>").Groups[1].Value;
                        string[] timeArray = timeString.Split(':');
                        hours = int.Parse(timeArray[0]);
                        minutes = int.Parse(timeArray[1]);
                        seconds = int.Parse(timeArray[2]);
                        CheckHours();
                    }
                    catch
                    {
                        Debug.Log("Time 1 error!");
                        StartCoroutine(GetGlobalTime2());
                    }
                    
                    break;
            }
            
        }
    }
    IEnumerator GetGlobalTime2()
    {
        string uri = "https://www.timeserver.ru/cities/ru/moscow";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    try
                    {
                        hours = Convert.ToInt32(System.Text.RegularExpressions.Regex.Match(webRequest.downloadHandler.text,
                            @"<span class=""hours"" data-prop=""hours"">(\d{2})</span>").Groups[1].Value);
                        minutes = int.Parse(System.Text.RegularExpressions.Regex.Match(webRequest.downloadHandler.text,
                            @"<span class=""minutes"" data-prop=""minutes"">(\d{2})</span>").Groups[1].Value);
                        seconds = int.Parse(System.Text.RegularExpressions.Regex.Match(webRequest.downloadHandler.text,
                            @"<span class=""seconds"" data-prop=""seconds"">(\d{2})</span>").Groups[1].Value);
                        CheckHours();
                    }
                    catch
                    {
                        Debug.Log("Time 2 error!");
                        GetLocalTime();
                    }

                    break;
            }
        }
    }
    private void GetLocalTime()
    {
        hours = DateTime.Now.Hour;
        minutes = DateTime.Now.Minute;
        seconds = DateTime.Now.Second;
        CheckHours();
    }
    private void CheckHours()
    {
        if (hours > 12)
            hours -= 12;
    }
}
