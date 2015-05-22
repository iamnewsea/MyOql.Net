//using System;
//using System.Threading;
//using MyCmn;

//namespace MyCmn
//{
//    /// <summary>
//    /// 配置MyOql作用域
//    /// </summary>
//    [Flags]
//    public enum GodErrorConfigEnum
//    {
//        //SkipRes = 0x1,
//        SkipLog = 0x2,
//    }
//    public class GodErrorScopeWrapper
//    {
//        [ThreadStatic]
//        public static GodErrorScope Current;


//        public static GodErrorConfigEnum CurrentValue
//        {
//            get
//            {
//                if (Current == null) return 0;
//                return Current.Config;
//            }
//        }
//    }
//    /// <summary>
//    /// 设置当前线程的Myoql 配置
//    /// </summary>
//    /// <remarks>
//    /// <example>
//    /// 在以下代码中两个执行的操作，会跳过权限过滤。
//    /// <code>
//    ///     using ( var config = new GodErrorScope( GodErrorConfigEnum.SkipPower ) )
//    ///     {
//    ///         var ent =  dbr.Menu.FindById(12) ;
//    ///         var usr = dbr.PLogin(ent.UserId , 'abc' ) ;
//    ///     }
//    /// </code>
//    /// </example>
//    /// </remarks>
//    [Serializable]
//    public class GodErrorScope : IDisposable
//    {
//        public GodErrorScope Parent { get; set; }

//        private GodErrorConfigEnum _Config = 0;

//        [ThreadStatic]
//        private static object _Sync_Config = new object();

//        public GodErrorConfigEnum Config
//        {
//            get
//            {
//                return _Config;
//            }
//            set
//            {
//                _Config = value;
//            }
//        }

//        public GodErrorScope(GodErrorConfigEnum config)
//        {
//            if (GodErrorScopeWrapper.Current == null)
//            {
//                GodErrorScopeWrapper.Current = this;
//            }
//            else
//            {
//                this.Parent = GodErrorScopeWrapper.Current;
//                GodErrorScopeWrapper.Current = this;

//                config |= this.Parent.Config;
//            }

//            Config = config;
//        }

//        public void Dispose()
//        {
//            GodErrorScopeWrapper.Current = this.Parent;
//        }
//    }
//}
