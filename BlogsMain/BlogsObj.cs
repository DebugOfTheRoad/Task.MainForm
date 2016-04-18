using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TaskPlugin;

namespace BlogsMain
{
    public class BlogsObj : TPlugin
    {
        public async override void _Load()
        {
            //获取xml配置信息
            var des = this.XmlConfig.Des;
            PublicClass._WriteLog(string.Format("xml配置信息-Des：{0}", this.XmlConfig.Des));
            PublicClass._WriteLog(string.Format("xml配置信息：{0}", this.XmlConfig.doc.InnerXml));

            //获取第三方数据
            var client = new HttpClient();
            var response = await client.GetAsync("http://wcf.open.cnblogs.com/blog/48HoursTopViewPosts/10");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                PublicClass._WriteLog(result, "BlogsObj");
            }
        }
    }
}
