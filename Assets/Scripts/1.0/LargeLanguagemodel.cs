using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static LLM;

public class LargeLanguagemodel : MonoBehaviour
{
    //API地址
    [SerializeField] protected string url;
    //提示词
    [Header("发送的提示词")]
    [SerializeField] protected string Prompt=string.Empty;
    //语言
    [SerializeField] protected string language = "中文";
    //上下文保留条数
    [Header("上下文保留条数")]
    [SerializeField] protected int HistoryKeepCount = 15;
    //缓存对话
    [SerializeField] public List<SendData> DataList = new List<SendData>();
    //计算所需时间
    [SerializeField] protected Stopwatch stopwatch = new Stopwatch();
    //发送消息
    public virtual void PostMsg(string msg,Action<string> callback)
    {
        ViewHistory();
        //关于提示词的处理
        string message = "当前角色人物设定：" + Prompt + "回答的语言：" + language + "接下来的提问：" + msg;

        //缓存
        DataList.Add(new SendData("user", message));

        StartCoroutine(Request(message, callback));
    }

    public virtual IEnumerator Request(string postWord,System.Action<string> callback)
    {
        yield return new WaitForEndOfFrame();
    }
    //防止过长
    public virtual void ViewHistory()
    {
        if(DataList.Count > HistoryKeepCount)
        {
            DataList.RemoveAt(0);
        }
    }

    [Serializable]
    public class SendData
    {
        [SerializeField] public string role;
        [SerializeField] public string content;
        public SendData(string role,string content)
        {
            this.role = role;
            this.content = content;
        }
    }
}
