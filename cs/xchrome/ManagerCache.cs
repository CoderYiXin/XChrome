﻿using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XChrome.cs.tools.YTools;

namespace XChrome.cs.xchrome
{
    public class ManagerCache
    {
        public IPlaywright? playwright = null;

        private readonly object _lock_runing_xchrome = new object();
        /// <summary>
        /// 打开着的浏览器,id_xchrome
        /// </summary>
        private Dictionary<long, XChromeClient> runing_xchrome = new Dictionary<long, XChromeClient>();
        /// <summary>
        /// 句柄
        /// </summary>
        private HashSet<IntPtr> runing_hwds = new HashSet<IntPtr>();

        private Dictionary<uint, XChromeClient> runing_processId_xchrome = new Dictionary<uint, XChromeClient>();

        //是否已经启动定时调整页面
        public bool _is_jober_AdjustmentView = false;

        /// <summary>
        /// 需要关闭的弹窗
        /// </summary>
        public HashSet<string> closeUrls = new HashSet<string>() {
            "chrome-extension://mcohilncbfahbmgdjkbpemcciiolgcge/notification.html",
            "https://www.okx.com/zh-hans/web3/extension/welcome"
        };


        public List<XChromeClient> GetRuningXchromesList()
        {
            return runing_xchrome.Select(it => it.Value).ToList();
        }

        public List<long> GetRuningXchrome_idlist()
        {
            return runing_xchrome.Keys.ToList();
        }

        public XChromeClient? GetRuningXchromeById(long id)
        {
            if (runing_xchrome.ContainsKey(id))
            {
                return runing_xchrome[id];
            }
            return null;
        }

        public int GetRuningXchromeCount()
        {
            return runing_xchrome.Count;
        }

       


        /// <summary>
        /// 打开一个后设置，必须包括完整的句柄等
        /// </summary>
        /// <param name="id"></param>
        /// <param name="xchrome"></param>
        public void SetXchrome(long id, XChromeClient xchrome)
        {
            lock (_lock_runing_xchrome)
            {
                runing_xchrome.AddOrReplace(id, xchrome);
                runing_hwds.AddOrSetValue((IntPtr)xchrome.Hwnd);
                runing_processId_xchrome.AddOrReplace(xchrome.ProcessId,xchrome);
            }
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="xchrome"></param>
        public void RemoveXchrome(XChromeClient xchrome)
        {
            lock (_lock_runing_xchrome)
            {
                if (runing_xchrome.ContainsKey(xchrome.Id)) runing_xchrome.Remove(xchrome.Id);
                if (runing_hwds.Contains((IntPtr)xchrome.Hwnd)) runing_hwds.Remove((IntPtr)xchrome.Hwnd);
                if(runing_processId_xchrome.ContainsKey(xchrome.ProcessId)) runing_processId_xchrome.Remove(xchrome.ProcessId);
            }
        }

        public XChromeClient? GetRuningXchromeByProcessId(uint processId)
        {
            if (runing_processId_xchrome.ContainsKey(processId))
            {
                return runing_processId_xchrome[processId];
            }
            return null;
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public string GetHomePageHtml(XChromeClient x)
        {
            //加userid只是为了统计装机量
            string pageurl = cs.Config.chrome_start_page + "?u=" + cs.Config.userid;

            string html = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>环境：{x.Id.ToString()}</title>
  <style>
    /* 清除默认的 margin 和 padding，确保页面占满整个视口 */
    html, body {{
      margin: 0;
      padding: 0;
      height: 100%;
    }}
    /* 设置 iframe 占满整个视口，自适应大小 */
    iframe {{
      width: 100%;
      height: 100%;
      border: none;
      margin:0;
    }}
  </style>
</head>
<body>
  <!-- 此处 src 可替换为你需要加载的页面地址 -->
  <iframe id='mainiframe' src=""""></iframe>
  <script>setTimeout(()=>{{document.getElementById('mainiframe').src='{pageurl}'}},200)</script>
</body>
</html>
";
            return html;
        }



        /// <summary>
        /// 注入脚本
        /// </summary>
        /// <returns></returns>
        public string GetInitScript()
        {

            return "var ___x=0;";
        }

    }
}
