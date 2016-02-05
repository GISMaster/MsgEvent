# MsgEvent | 消息事件


## Summary
### Publish-Subscribe Pattern 
>In software architecture, publish–subscribe is a messaging pattern where senders of messages, 
called publishers, do not program the messages to be sent directly to specific receivers, called subscribers, 
but instead characterize published messages into classes without knowledge of which subscribers, if any, there may be. 
Similarly, subscribers express interest in one or more classes and only receive messages that are of interest, 
without knowledge of which publishers, if any, there are.

### 发布-订阅 模式
>软件架构中，发布-订阅（publish–subscribe）是一种消息传播模式，消息的发送者（发布者）不会将消息直接发送给特定的接收者（订阅者）。
而是将发布的消息按特征分类，将消息内容发送给消息广播站，由广播站统一派发给订阅者，从而发布者无需对订阅者（如果有的话）了解。
   同样的，订阅者可以表达对一个或多个类别的兴趣，只接收感兴趣的消息，无需对发布者（如果有的话）有所了解。

------------------------------------------------------------------------------------------  

## 调用说明
本模块使用C# 语言对（发布-订阅）模式进行了实现。
使用的示例详见 MsgEventUnitTest 中的 Sample-Prj：

1.首先在使用的模块处添加引用：
*  using Sym.EventsManager;
  
2.对于发布者而言，需要关注几个参数内容
*  所要发布的消息类型：  msgType：String
*  所要发布的消息内容：  msgContent：MsgEventArgs（EventArgs）
*  发布者对象        ：  msgSender ：Object

3.对于订阅者而言，需要关注几个参数内容
*  所要订阅的消息类型：  msgType：String
*  接受消息的响应方法：  callbackFunc：EventHandler<Object,MsgEventArgs>
*  订阅者对象        ：  receiver：Object
*  是否只订阅一次    ：  isOnce:bool

4.对于广播中心而言，需要关系几个参数内容
*  所要管理的消息类型：  msgType：String
*  某消息的订阅者清单：  receivers:List<MsgReceiver>

------------------------------------------------------------------------------------------ 
###DEMO：
1.定义消息对象：
   
    /// <summary>
    /// 测试实体类1
    /// </summary>
    public class D1
    {
        public string id = string.Empty;
        public double num = 0.0;
        private List<int> lyrs = new List<int>();
        public List<int> Lyrs
        {
            get { return lyrs; }
        }
    }

    /// <summary>
    /// 测试实体类2
    /// </summary>
    public class D2
    {
        public string name = string.Empty;
        public int code = 100;

    }

    /// <summary>
    /// 拓展消息体的自定义子类
    /// </summary>
    public class CustomArgs : MsgEventArgs
    {
        public CustomArgs(string _msgType):base(_msgType)
        {
            Data = new D1() {id = "d2", Lyrs = {1, 3, 4, 5, 6, 7}, num = 0.99998};
        }

        private string id = string.Empty;
        public string ID
        {
            get { return id; }
            set { id = value; }
        }
    }

2.定义几个模块类型：
   
    /// <summary>
    ///  测试模块1
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Module1<T>
    {
        public Module1()
        {
            MsgEvent.Receive("s2", saveData,this);
            MsgEvent.Receive("s3", showData, this);
        }

        private int id = 100;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public void IntroduceMe()
        {
            Console.WriteLine(string.Format("我是M1，我的ID是：{0}", id));
        }

        public void SendMsg(T data)
        {
            MsgEvent.Send("s1",data,this);
        }

        private void saveData(object sender,MsgEventArgs args)
        {
            Console.WriteLine(string.Format("我是：{1}，收到{2}的消息，内容是：{0},saveData", args.Data, "Module1",args.EventType));
        }

        private void showData(object sender, MsgEventArgs args)
        {
            Console.WriteLine(string.Format("我是：{1}，收到{2}的消息，内容是：{0},showData", args.Data, "Module1",args.EventType));

            MsgEvent.RemoveListener("s3", saveData, this);
        }

    }

    /// <summary>
    /// 测试模块2
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Module2<T>
    {
        public Module2()
        {
            MsgEvent.Receive("s1", saveData, this);
            MsgEvent.Receive("s2", saveData, this);
        }

        public void SendMsg(T data)
        {
            MsgEvent.Send("s3", data, this);
        }

        private void saveData(object sender, MsgEventArgs args)
        {
            Console.WriteLine(string.Format("我是：{1}，收到{2}的消息，内容是：{0}", args.Data, "Module2",args.EventType));
            try
            {  
                Module1<D1> module1 = sender as Module1<D1>;
                module1.Id = 900;
                module1.IntroduceMe();
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }
    }

    /// <summary>
    /// 测试模块3
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Module3<T>
    {
        public Module3()
        {
            MsgEvent.Receive("s1",saveData,this);
            MsgEvent.Receive("s2", saveData, this,true);
            MsgEvent.Receive("s3",saveData,this,true);
        }

        public void SendMsg(T data)
        {
            MsgEvent.Send("s3", data, this);
        }

        private void saveData(object sender, MsgEventArgs args)
        {
            Console.WriteLine(string.Format("我是：{1}，收到{2}的消息，内容是：{0},saveData", args.Data, "Module3",args.EventType));

            CustomArgs p1 =args as CustomArgs;
            if (p1 != null)
            {
                Console.WriteLine("子类参数对象获取成功！");
            }
        }
    }
    

3.客户端调用：
   
    /// <summary>
    /// 客户端调用测试类
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //初始化模块1，触发事件IntroduceMe
            Module1<D1> m1 = new Module1<D1>();
            m1.IntroduceMe();

            //初始化模块2,3
            Module2<D2> m2=new Module2<D2>();
            Module3<D1> m3 = new Module3<D1>();

            //每个模块分别发送消息
            m1.SendMsg(new D1(){id = "d1", num = 1000.0001});
            m2.SendMsg(new D2(){code = 900,name = "tt11"});
            m3.SendMsg(new D1(){id = "d2",Lyrs = {1,3,4,5,6,7},num = 0.99998});

            m1.SendMsg(new D1() { id = "d1", num = 1000.0001 });
            m2.SendMsg(new D2() { code = 900, name = "tt11" });
            m3.SendMsg(new D1() { id = "d2", Lyrs = { 1, 3, 4, 5, 6, 7 }, num = 0.99998 });

            m1.SendMsg(new D1() { id = "d1", num = 1000.0001 });
            m2.SendMsg(new D2() { code = 900, name = "tt11" });
            m3.SendMsg(new D1() { id = "d2", Lyrs = { 1, 3, 4, 5, 6, 7 }, num = 0.99998 });

            //临时创建一个消息体，并将此消息发送到s1信道中
            CustomArgs args1 = new CustomArgs("s1");
            args1.ID = "p1111";
            MsgEvent.Send(args1,null);

            //可根据此方法获取某一信道内的消息订阅者清单
           MsgEventProvider provider = MsgEventFactory.EventsContainer.GetAllProvider("s1");
            List<MsgReceiver> li = provider.Receivers;

            Console.ReadKey();
        }
    }
  
###输出结果：(见测试DEMO)
>我是M1,我的ID是：100

>我是：Module2，收到s1的消息，内容是：QS.Events.D1

>....
=====

