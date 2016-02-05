using System;
using System.Collections.Generic;

/////////////////////////////////////////////////////////////////////////////////////////////////
/// 本文件是对观察者模式的拓展实现，提供一套便于使用的数据及消息内容的订阅-发布机制
/// 使用者只需关注所要发布的数据内容和通信信道，即可利用此API来实现模块间的消息传递
/// 目的：降低类间的相互依赖关系，为模块化而实现
/// -------------------------------------------------------------------------------------------------------------------------
/// Editor ：Chencl        
/// Email  ：superman.ccl@hotmail.com
/// //////////////////////////////////////////////////////////////////////////////////////////////
namespace Sym.EventsManager
{
    /// <summary>
    /// 消息订阅-发布的操作类
    /// </summary>
    public class MsgEvent
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="args">消息体对象</param>
        /// <param name="sender">发送者对象</param>
        public static void Send(MsgEventArgs args,object sender =null)
        {
            MsgEventFactory.EventsContainer.Run(sender, args);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msgType">信道ID</param>
        /// <param name="data">消息数据</param>
        /// <param name="sender">发送者对象</param>
        public static void Send(string msgType,object data,object sender=null)
        {
            MsgEventArgs args = new MsgEventArgs(msgType);
            args.Data = data;
            MsgEventFactory.EventsContainer.Run(sender, args);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="msgType">信道ID</param>
        /// <param name="func">用于处理接收到消息的方法</param>
        /// <param name="receiver">订阅者对象，默认为null</param>
        /// <param name="isOnce">是否接收方法只处理一次，默认为false</param>
        public static void Receive(string msgType,EventHandler<MsgEventArgs> func,object receiver=null,bool isOnce=false)
        {
            MsgEventFactory.EventsContainer.AddListener(msgType,func,receiver,isOnce);
        }

        /// <summary>
        /// 移除消息的订阅
        /// </summary>
        /// <param name="msgType">信道ID</param>
        /// <param name="func">需要移除的接收消息处理方法</param>
        /// <param name="receiver">订阅者对象，默认为null</param>
        public static void RemoveListener(string msgType, EventHandler<MsgEventArgs> func,object receiver=null)
        {
            MsgEventFactory.EventsContainer.RemoveListener(msgType,func,receiver);
        }

    }

    /// <summary>
    /// 消息体
    /// </summary>
    public class MsgEventArgs : EventArgs
    {
        public MsgEventArgs(string eventType)
        {
            this.EventType = eventType;
        }

        /// <summary>
        /// 消息类型
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public object Data { get; set; }
    }

    /// <summary>
    /// 消息订阅者封装类
    /// </summary>
    public class MsgReceiver
    {
        private object _receiver = null;
        private bool isOnce = false;
        private string key = string.Empty;
        public EventHandler<MsgEventArgs> appEventHandler;

        /// <summary>
        /// 接收者对象
        /// </summary>
        public object ReceiverObj
        {
            get { return _receiver; }
            set { _receiver = value; }
        }

        /// <summary>
        /// 是否只接收一次操作
        /// </summary>
        public bool IsOnce
        {
            get { return isOnce; }
            set { isOnce = value; }
        }

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

    }

    /// <summary>
    /// 消息事件提供者
    /// </summary>
    public class MsgEventProvider
    {
        public MsgEventProvider(string _eventType)
        {
            eventType = _eventType;
            Receivers = new List<MsgReceiver>();
        }

        ~MsgEventProvider()
        {
            //Console.WriteLine(string.Format("{0}的MsgEventProvider析构函数执行！", eventType));
        }

        private string eventType = string.Empty;

        public string EventType
        {
            get { return eventType; }
        }

        private event EventHandler<MsgEventArgs> appEventHandler;

        public event EventHandler<MsgEventArgs> AppEventHandler
        {
            add
            {
                appEventHandler += value;
            }
            remove
            {
                appEventHandler -= value;
            }
        }

        public List<MsgReceiver> Receivers;
        
        #region 外部调用事件
        public void Run(Object sender,MsgEventArgs args)
        {
            if (appEventHandler!=null)
            {
                appEventHandler(sender, args);
            }

            #region 去除那些只绑定一次处理事件的对象

            for (int i=0;i<Receivers.Count;i++)
            {
                try
                {
                    MsgReceiver item = Receivers[i];
                    
                    if(item==null)continue;

                    if (item.IsOnce)
                    {
                        AppEventHandler -= item.appEventHandler;
                        Receivers.Remove(item);
                    }
                }
                catch
                {
                   Console.WriteLine("去除接受者对象列表中内容错误！");
                }
            }


            #endregion

        }

        public void AddListener(MsgReceiver receiver)
        {
            Receivers.Add(receiver);
            AppEventHandler += receiver.appEventHandler;
        }

        public void RemoveListener(MsgReceiver receiver)
        {
            for (int i = 0; i < Receivers.Count; i++)
            {
                MsgReceiver item = Receivers[i];
                if (item.ReceiverObj == receiver.ReceiverObj)
                {
                    AppEventHandler -= item.appEventHandler;
                    Receivers.Remove(item);
                    return;
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// 消息事件管理器生成工厂
    /// </summary>
    public class MsgEventFactory
    {
        private static MsgEventsContainer _msgEventsContainer = null;
        public static MsgEventsContainer EventsContainer
        {
            get
            {
                if (_msgEventsContainer==null)
                {
                    _msgEventsContainer = new MsgEventsContainer();
                }
                return _msgEventsContainer;
            }
        }
    }

    /// <summary>
    /// 消息事件管理器
    /// </summary>
    public class MsgEventsContainer
    {
        public MsgEventsContainer()
        {
            eventsList = new Dictionary<string, MsgEventProvider>();
        }

        #region 属性|字段
        private Dictionary<string, MsgEventProvider> eventsList;
        #endregion

        #region 方法

        public void AddListener(string msgType,EventHandler<MsgEventArgs> func, object _receiver,  bool isOnce)
        {
            if (eventsList.ContainsKey(msgType)==false)
            {
                MsgEventProvider _msgEventProvider = new MsgEventProvider(msgType);
                eventsList.Add(msgType,_msgEventProvider);
            }

            MsgEventProvider msgEventProvider = eventsList[msgType];

            #region 构建消息接收对象
            MsgReceiver receiver = new MsgReceiver();
            receiver.IsOnce = isOnce;
            receiver.ReceiverObj = _receiver;
            receiver.appEventHandler = func;
            #endregion
           
            msgEventProvider.AddListener(receiver);
        }

        public void RemoveListener(string msgType, EventHandler<MsgEventArgs> func,object _receiver)
        {
            if (eventsList.ContainsKey(msgType) == false)return;

            MsgEventProvider msgEventProvider = eventsList[msgType];

            #region 构建消息接受者对象
            MsgReceiver receiver = new MsgReceiver();
            receiver.IsOnce = true;
            receiver.ReceiverObj = _receiver;
            receiver.appEventHandler = func;
            #endregion
           
            msgEventProvider.RemoveListener(receiver);
        }

        public void Run(Object sender,MsgEventArgs args)
        {
            string msgType = args.EventType;

            //未存在注册的消息事件，则直接返回
            if(eventsList.ContainsKey(msgType)==false)return;

            MsgEventProvider msgEventProvider = eventsList[msgType];

            msgEventProvider.Run(sender, args);
        }

        public MsgEventProvider GetAllProvider(string msgType)
        {
            if (eventsList.ContainsKey(msgType)==false) return null;

            return eventsList[msgType];
        }

        #endregion

    }

}
