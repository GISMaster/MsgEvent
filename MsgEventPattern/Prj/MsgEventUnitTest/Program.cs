using Sym.EventsManager;
using System;
using System.Collections.Generic;

namespace QS.Events

{
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
            MsgEvent.Receive("s1",saveData,this);
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
            catch (Exception)
            {
                
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

    

}
