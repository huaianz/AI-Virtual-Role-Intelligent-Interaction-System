using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TextToSpeech : MonoBehaviour
{
    //语音合成的API
    [SerializeField] protected string URL = string.Empty;
    //计算所需时间
    [SerializeField] protected Stopwatch stopwatch = new Stopwatch();

    //返回音频
    public virtual void Speak(string msg,Action<AudioClip> callback) { }

    //返回音频和文本
    public virtual void Speak(string msg, Action<AudioClip, string> callback) { }
}