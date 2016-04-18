using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TaskPlugin
{

    /// <summary>
    /// 插件基类
    /// </summary>
    public class TPlugin
    {

        public TPlugin()
        {

            XmlConfig = _InitConfig();
        }

        #region  初始化Xml配置文件 _InitConfig +XmlConfig

        /// <summary>
        /// xml配置信息
        /// </summary>
        public XmlConfig XmlConfig;


        /// <summary>
        /// 初始化配置信息
        /// </summary>
        /// <param name="configPath">配置文件对应路径</param>
        /// <returns></returns>
        public virtual XmlConfig _InitConfig(string configPath = "")
        {
            XmlConfig config = new XmlConfig();
            config.Timer = 1;
            config.Name = this.GetType().Name;
            try
            {

                if (string.IsNullOrEmpty(configPath))
                {

                    //默认各个dllXml配置
                    var defaultConfigFolder = "PluginXml";
                    var baseAddr = AppDomain.CurrentDomain.BaseDirectory;
                    configPath = Path.Combine(baseAddr, defaultConfigFolder, config.Name + ".xml");
                }

                config.doc.Load(configPath);

                config.Timer = config.doc.SelectSingleNode("//TaskMain/Timer") == null ?
                               1 : Convert.ToInt32(config.doc.SelectSingleNode("//TaskMain/Timer").InnerXml.Trim());
                config.Name = config.doc.SelectSingleNode("//TaskMain/Name") == null ?
                               config.Name : config.doc.SelectSingleNode("//TaskMain/Name").InnerXml.Trim();
                config.Des = config.doc.SelectSingleNode("//TaskMain/Des") == null ?
                               config.Name : config.doc.SelectSingleNode("//TaskMain/Des").InnerXml.Trim();
            }
            catch (Exception ex)
            {

                throw;
            }
            return config;
        }
        #endregion

        #region 初始化-开始加载  _Load

        /// <summary>
        /// 初始化-开始起
        /// </summary>
        public virtual void _Load()
        {

            PublicClass._WriteLog("测试");
        }

        #endregion

    }

    #region 配置文件 XmlConfig

    public class XmlConfig
    {
        public XmlConfig()
        {
            doc = new XmlDocument();
        }

        /// <summary>
        /// 定制器时间（分钟）
        /// </summary>
        public int Timer { get; set; }

        /// <summary>
        /// 运行名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述(第一次获取dll描述，后面获取xml配置文件描述)
        /// </summary>
        public string Des { get; set; }


        /// <summary>
        /// xml信息
        /// </summary>
        public XmlDocument doc;
    }

    #endregion
}
