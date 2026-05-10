using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    /// <summary>
    /// 聊天设置
    /// </summary>
    [SerializeField] private ChatSetting ChatSetting;
    /// <summary>
    /// 聊天UI
    /// </summary>
    [SerializeField]private GameObject ChatPanel;
    /// <summary>
    /// 输入信息
    /// </summary>
    [SerializeField] public InputField InputWord;
    /// <summary>
    /// 返回信息
    /// </summary>
    [SerializeField] private Text TextBack;
    /// <summary>
    /// 声音
    /// </summary>
    [SerializeField] private AudioSource AudioSource;
    /// <summary>
    /// 发送消息的按钮
    /// </summary>
    [SerializeField] private Button Submit;

    /// <summary>
    /// 动画
    /// </summary>
    [SerializeField] private Animator Animator;

    [Header("设置是否通过语音合成播放文本")]
    [SerializeField] private bool IsVoiceMode = true;
    [Header("勾选则不发送LLM，直接合成输入文字")]
    [SerializeField] private bool CreateVoiceMode = false;


    private void Awake()
    {
        Submit.onClick.AddListener(delegate { SendData(); });
        RegistButtonEvent();
        InputSettingWhenWebgl();
    }

    /// <summary>
    /// 支持中文输入
    /// </summary>
    private void InputSettingWhenWebgl()
    {
#if UNITY_WEBGL
        InputWord.gameObject.AddComponent<WebGLSupport.WebGLInput>();
#endif
    }

    /// <summary>
    /// 发送的信息
    /// </summary>
    private void SendData()
    {
        if (InputWord.text.Equals(""))
            return;

        if(CreateVoiceMode)
        {
            CallBack(InputWord.text);
            InputWord.text = "";
            return;
        }

        //把聊天记录放到列表
        ChatHistory.Add(InputWord.text);
        //提示词
        string msg = InputWord.text;

        ChatSetting.ChatModel.PostMsg(msg, CallBack);

        InputWord.text = "";
        TextBack.text = "正在思考中...";

        //关于动作
        SetAnimator("state",1);
    }

    //带文字发送
    public void SendData(string postWord)
    {
        if (postWord.Equals(""))
            return;

        if (CreateVoiceMode)//合成输入为语音
        {
            CallBack(postWord);
            InputWord.text = "";
            return;
        }


        //添加记录聊天
        ChatHistory.Add(postWord);
        //提示词
        string msg = postWord;

        //发送数据
        ChatSetting.ChatModel.PostMsg(msg, CallBack);

        InputWord.text = "";
        TextBack.text = "正在思考中...";

        //切换思考动作
        //SetAnimator("state", 1);
    }
    private void CallBack(string Response)
    {
        //先把传进来的文本开头结尾的空白字符去掉
        Response = Response.Trim();
        TextBack.text = "";

        //记录聊天记录
        ChatHistory.Add(Response);

        //不搞语音
        if (!IsVoiceMode || ChatSetting.TextToSpeech == null)
        {
            PrintfOutput(Response);
            return;
        }

        ChatSetting.TextToSpeech.Speak(Response, PlayVoice);
    }

    //播放语音
    private void PlayVoice(AudioClip clip, string response)
    {
        AudioSource.clip = clip;
        AudioSource.Play();

        PrintfOutput(response);
        //说话的动画
        SetAnimator("state", 2);
    }

    //逐字显示时间间隔和是否完全打印输出完
    [SerializeField] private float WordWaitTime = 0.2f;
    [SerializeField] private bool WriteState = false;
    //打印输出
    private void PrintfOutput(string msg)
    {
        if (msg == "")
            return;

        WriteState = true;
        StartCoroutine(SetTextPerWord(msg));
    }

    private IEnumerator SetTextPerWord(string msg)
    {
        int currentPos = 0;
        while (WriteState)
        {
            yield return new WaitForSeconds(WordWaitTime);
            currentPos++;
            //更新显示的内容
            TextBack.text = msg.Substring(0, currentPos);

            WriteState = currentPos < msg.Length;

        }

        //切换到等待动作
        SetAnimator("state", 0);
    }




    //聊天记录
    [SerializeField] private List<string> ChatHistory;
    //缓存已创建的聊天气泡
    [SerializeField] private List<GameObject> CrrutChatBox;
    //聊天记录显示层
    [SerializeField] private GameObject HistoryPanel;
    //聊天文本层
    [SerializeField] private RectTransform ChatTrans;
    //发送聊天气泡
    [SerializeField] private ChatPrefab InChatPrefab;
    //恢复聊天气泡
    [SerializeField] private ChatPrefab OutChatPrefab;
    //滚动条
    [SerializeField] private ScrollRect ScrollRect;
    public void OpenAndGetHistory()
    {
        ChatPanel.SetActive(false);
        HistoryPanel.SetActive(true);

        ClearChatBox();
        StartCoroutine(RetrieveChatHistory());
    }

    //清空对话框
    private void ClearChatBox()
    {
        while (CrrutChatBox.Count != 0)
        {
            if (CrrutChatBox[0])
            {
                Destroy(CrrutChatBox[0].gameObject);
                CrrutChatBox.RemoveAt(0);
            }
        }
        CrrutChatBox.Clear();
    }

    //获取历史聊天记录
    private IEnumerator RetrieveChatHistory()
    {

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < ChatHistory.Count; i++)
        {
            if (i % 2 == 0)
            {
                ChatPrefab sendChat = Instantiate(InChatPrefab, ChatTrans.transform);
                sendChat.SetText(ChatHistory[i]);
                CrrutChatBox.Add(sendChat.gameObject);
                continue;
            }

            ChatPrefab reChat = Instantiate(OutChatPrefab, ChatTrans.transform);
            reChat.SetText(ChatHistory[i]);
            CrrutChatBox.Add(reChat.gameObject);
        }

        //重新计算容器尺寸
        LayoutRebuilder.ForceRebuildLayoutImmediate(ChatTrans);
        StartCoroutine(TurnToLastLine());
    }

    //跳到最后一行
    private IEnumerator TurnToLastLine()
    {
        yield return new WaitForEndOfFrame();
        //滚动到最近的消息
        ScrollRect.verticalNormalizedPosition = 0;
    }

    public void BackChatMode()
    {
        ChatPanel.SetActive(true);
        HistoryPanel.SetActive(false);
    }

    //动画的切换
    private void SetAnimator(string ms,int value)
    {
        if (Animator == null)
            return;

        Animator.SetInteger(ms, value);
    }



    //关于语言输入的设置

    //语音识别文本是否发送到大语言模型
    [SerializeField] private bool AutoSend = true;
    //语音输入按钮
    [SerializeField] private Button VoiceInputBotton;
    //输入后的文本
    [SerializeField] private Text VoiceBottonText;
    //录音的提示信息
    [SerializeField] private Text RecordTips;
    //语音输入处理类
    [SerializeField] private VoiceInputProcessing VoiceInputProcessing;

    //注册按钮事件
    private void RegistButtonEvent()
    {
        if (VoiceInputBotton == null || VoiceInputBotton.GetComponent<EventTrigger>())
            return;

        EventTrigger trigger = VoiceInputBotton.gameObject.AddComponent<EventTrigger>();

        //添加按钮按下事件
        EventTrigger.Entry entryDown = new EventTrigger.Entry();
        entryDown.eventID = EventTriggerType.PointerDown;
        entryDown.callback = new EventTrigger.TriggerEvent();

        //添加松开事件
        EventTrigger.Entry entryUp = new EventTrigger.Entry();
        entryUp.eventID = EventTriggerType.PointerUp;
        entryUp.callback = new EventTrigger.TriggerEvent();

        //添加事件委托
        entryDown.callback.AddListener(delegate { StartRecord(); });
        entryUp.callback.AddListener(delegate { StopRecord(); });

        trigger.triggers.Add(entryDown);
        trigger.triggers.Add(entryUp);
    }

    //开始录制
    public void StartRecord()
    {
        VoiceBottonText.text = "正在录音中...";
        VoiceInputProcessing.StartRecordAudio();
    }

    //结束录制
    public void StopRecord()
    {
        VoiceBottonText.text = "按住按钮，开始录音";
        RecordTips.text = "录音结束，正在识别...";
        VoiceInputProcessing.StopRecordAudio(ProcessAudio);
    }

    //处理录制音频
    private void ProcessAudio(AudioClip audioClip)
    {
        if (ChatSetting.m_SpeechToText == null)
            return;

        ChatSetting.m_SpeechToText.SpeechToText(audioClip, ProcessText);
    }

    //处理识别文本
    private void ProcessText(string msg)
    {
        RecordTips.text = msg;
        StartCoroutine(SetTextVisible(RecordTips));

        if (AutoSend)
        {
            SendData(msg);
            return;
        }

        InputWord.text = msg;
    }

    private IEnumerator SetTextVisible(Text textbox)
    {
        yield return new WaitForSeconds(3f);
        textbox.text = "";
    }

}
