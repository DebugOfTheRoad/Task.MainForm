using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TaskPlugin
{

    /// <summary>
    /// 封装方法
    /// </summary>
    public class PublicClass
    {

        #region _GetPluginFile  获取插件文件（即*.dll）   +FileInfo[]

        /// <summary>
        /// 获取插件文件（即*.dll）,默认程序根路径
        /// </summary>
        /// <param name="pluginFolderName">插件文件夹名称（默认：Plugin）</param>
        /// <returns>插件*.dll文件</returns>
        public static FileInfo[] _GetPluginFile(string pluginFolderName = "Plugin")
        {
            //组合插件文件夹路径
            string baseDirctory = AppDomain.CurrentDomain.BaseDirectory;
            string pluginPath = Path.Combine(baseDirctory, pluginFolderName);

            DirectoryInfo di = new DirectoryInfo(pluginPath);
            if (!di.Exists)
            {
                di.Create();
            }

            //获取插件文件夹下面的所有dll文件
            return di.GetFiles("*.dll");
        }

        #endregion

        #region   _LoadPlugin<T>  加载dll对象  +T

        /// <summary>
        /// 加载插件对象
        /// </summary>
        /// <typeparam name="T">dll里面的对象父级</typeparam>
        /// <param name="pluginPath">dll路径</param>
        /// <param name="tName">dll对象名称</param>
        /// <returns></returns>
        public static T _LoadPlugin<T>(string pluginPath, string tName) where T : class ,new()
        {

            if (string.IsNullOrEmpty(pluginPath) || string.IsNullOrEmpty(tName)) { return default(T); }


            try
            {
                //加载程序集
                Assembly sm = Assembly.LoadFile(pluginPath);
                //获取指定类型
                Type type = sm.GetType(tName);
                //实例化
                T t = Activator.CreateInstance(type) as T;
                if (t == null) { return default(T); }

                //初始化加载方法
                //t._Load();
                return t;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region  _WriteLog 文本日志 +void

        public static object objLog = new object();
        public static void _WriteLog(string logContent, string logFolderName ="BaseLog")
        {

            #region 文本日志

            var dateTime = DateTime.Now;
            var logFloder = AppDomain.CurrentDomain.BaseDirectory
                            + (logFolderName.IndexOf('/') > 0 ? logFolderName : logFolderName + "/")
                            + dateTime.ToString("yyyy-MM-dd");
            if (!Directory.Exists(logFloder))
            {
                Directory.CreateDirectory(logFloder);
            }

            lock (objLog)
            {

                using (StreamWriter writer = new StreamWriter(logFloder + "/" + dateTime.ToString("HH") + "-" +
                                                         (logFolderName.ToLower().IndexOf(".txt") > 0 ? logFolderName : logFolderName + ".txt"),
                                                              true))
                {
                    writer.WriteLine(string.Format("时间：{0}\r\n{1}\r\n----------------------------------------------------------------",
                                                    dateTime, logContent));
                }
            }

            #endregion
        }
        #endregion
    }
}
