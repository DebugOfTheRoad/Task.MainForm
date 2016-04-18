using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using TaskPlugin;

namespace Task.MainForm
{
    public partial class MainForm : Form
    {
        public MainForm()
        {

            InitializeComponent();
            _Init();
        }

        #region 初始化

        /// <summary>
        /// 定时器字典
        /// </summary>
        private Dictionary<string, System.Timers.Timer> Dic_Timers = new Dictionary<string, System.Timers.Timer>();

        #endregion

        /// <summary>
        /// 初始化程序集文件
        /// </summary>
        private void _Init()
        {

            //获取文件
            var files = PublicClass._GetPluginFile();
            if (files.Length <= 0) { _Alert("未能找到可用的程序集（*.dll）"); return; }

            //展示文件信息
            foreach (var item in files.OrderByDescending(b => b.CreationTime))
            {
                Assembly sm = Assembly.LoadFile(item.FullName);
                var ts = sm.GetTypes();
                //判断特定的dll显示
                foreach (var t in ts.Where(b => b.GetMethod("_Load") != null))
                {

                    ListViewItem lvi = new ListViewItem(item.FullName);
                    lvi.SubItems.Add(t.FullName);
                    lvi.SubItems.Add("停止");
                    lvi.SubItems.Add("");
                    DescriptionAttribute des = (TypeDescriptor.GetAttributes(t)[typeof(DescriptionAttribute)]) as DescriptionAttribute;
                    lvi.SubItems.Add(des == null ? "暂无" : des.Description);
                    listView.Items.Add(lvi);
                }
            }
        }

        /// <summary>
        /// 服务开启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            var msg = new StringBuilder(string.Empty);
            //获取选中项
            foreach (ListViewItem item in listView.Items)
            {

                try
                {

                    if (!item.Checked) { continue; }

                    //初始化
                    var fullName = item.Text;
                    var tName = item.SubItems[1].Text;
                    if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(tName)) { continue; }
                    var key = fullName + tName;
                    if (Dic_Timers.ContainsKey(key)) { continue; }  //如果定时器字典包含当前定时器，那么直接跳过

                    //默认加载了配置xml文件
                    var plugs = PublicClass._LoadPlugin<TPlugin>(fullName, tName);
                    if (plugs == null) { _ConsoleMsg(string.Format("开启服务-{0}失败", tName)); continue; }

                    //创建定时器
                    int loopTime = plugs.XmlConfig.Timer * 60 * 1000;   //分钟
                    System.Timers.Timer timer = new System.Timers.Timer(loopTime);
                    timer.Elapsed += new ElapsedEventHandler((s, ee) => timer_Elapsed(s, ee, plugs));
                    timer.Enabled = true;
                    timer.Start();
                    Dic_Timers.Add(key, timer);

                    //状态设置
                    item.UseItemStyleForSubItems = false;
                    item.SubItems[2].ForeColor = Color.Red;
                    item.SubItems[2].Text = "开启";
                    item.SubItems[3].Text = plugs.XmlConfig.Timer.ToString();
                    item.SubItems[4].Text = plugs.XmlConfig.Des;
                    msg.AppendFormat("开启服务-{0},\n", tName);
                    _ConsoleMsg(string.Format("开启服务-{0}", tName));
                }
                catch (Exception ex)
                {
                    msg.AppendFormat("异常信息-{0},\n", ex.Message);
                    _ConsoleMsg(string.Format("开启服务异常信息-{0}", ex.Message));
                }
            }
            _Alert(string.IsNullOrEmpty(msg.ToString()) ? "勾选-开启服务" : msg.ToString().TrimEnd(','));
        }

        /// <summary>
        /// 执行定时任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="obj"></param>
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e, object obj)
        {
            var plugin = obj as TPlugin;
            if (plugin == null) { return; }

            _ConsoleMsg(string.Format("执行服务-{0}", plugin.XmlConfig.Name));
            //执行任务方法
            plugin._Load();

        }

        /// <summary>
        /// 服务停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            var msg = new StringBuilder(string.Empty);
            //获取选中项
            foreach (ListViewItem item in listView.Items)
            {
                try
                {

                    if (item.Checked) { continue; }

                    var fullName = item.Text;
                    var tName = item.SubItems[1].Text;
                    var key = fullName + tName;
                    if (Dic_Timers.ContainsKey(key))
                    {

                        //释放定时器
                        var timer = Dic_Timers[key];
                        timer.Stop();
                        timer.Close();
                        timer.Dispose();

                        //移除定时器字典
                        Dic_Timers.Remove(fullName);
                        _ConsoleMsg(string.Format("停止服务-{0}", tName));
                    }
                    else { continue; }

                    item.UseItemStyleForSubItems = false;
                    item.SubItems[2].ForeColor = Color.Black;
                    item.SubItems[2].Text = "停止";
                    msg.AppendFormat("停止服务-{0},\n", tName);
                }
                catch (Exception ex)
                {
                    msg.AppendFormat("异常信息-{0},\n", ex.Message);
                    _ConsoleMsg(string.Format("停止服务异常信息-{0}", ex.Message));
                }
            }

            _Alert(string.IsNullOrEmpty(msg.ToString()) ? "去勾-停止服务" : msg.ToString().TrimEnd(','));
        }

        /// <summary>
        /// 记录操作信息
        /// </summary>
        private void _ConsoleMsg(string msg)
        {

            if (string.IsNullOrEmpty(msg)) { return; }

            this.BeginInvoke(new EventHandler(delegate {

                txtMsg.AppendText(string.Format("{0}：{1}\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg));
            }));
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        /// <param name="msg"></param>
        private void _Alert(string msg, bool isOkAlert = false, Func<bool> fun = null, Func<bool> fun01 = null)
        {
            if (!isOkAlert) { MessageBox.Show(msg, "神牛步行3-提醒"); }
            else
            {
                var result = MessageBox.Show(msg, "神牛步行3-提醒", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.OK && fun != null)
                {
                    fun();
                }
                else if (result == DialogResult.Cancel && fun01 != null)
                {

                    fun01();
                }
            }
        }

        /// <summary>
        /// 最小化到托盘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                HideForm();
            }
        }

        /// <summary>
        /// 隐藏
        /// </summary>
        private void HideForm()
        {
            this.Hide();
            this.notifyIcon.ShowBalloonTip(0x2710, "神牛步行3-提示", "双击显示窗体", ToolTipIcon.Info);
        }

        /// <summary>
        /// 关闭按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _Alert("是否需要关闭任务管理器？", true,
                () =>
                {
                    return true;
                },
                () =>
                {
                    e.Cancel = true;
                    HideForm();
                    return true;
                }
            );
        }

        /// <summary>
        /// 双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Minimized;
                HideForm();
            }
            else if (this.WindowState == FormWindowState.Minimized)
            {

                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.Activate();
            }
        }
    }
}
