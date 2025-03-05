﻿using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XChrome.cs.win32;

namespace XChrome.cs.xchrome
{
    public class ManagerTooler
    {
        private ManagerCache _ManagerCache;
        public ManagerTooler(ManagerCache cache) { _ManagerCache = cache; }

        /// <summary>
        /// 计算位置分布
        /// </summary>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        /// <param name="windowCount"></param>
        /// <returns></returns>
        public List<XWindowRect> ComputeAdaptiveWindowPositions(int screenWidth, int screenHeight, int windowCount)
        {
            List<XWindowRect> windows = new List<XWindowRect>();
            if (windowCount <= 0)
                return windows;

            // 根据窗口总数确定最大列数
            int maxColumns = (int)Math.Ceiling(Math.Sqrt(windowCount));
            // 根据最大列数确定总行数
            int rows = (int)Math.Ceiling((double)windowCount / maxColumns);

            // 统一窗口大小（均分屏幕）
            int windowWidth = screenWidth / maxColumns;
            int windowHeight = screenHeight / rows;

            int windowIndex = 0;
            for (int row = 0; row < rows; row++)
            {
                // 如果为最后一行，窗口数使用剩余的；否则当前行使用满列数
                int windowsInRow = (row == rows - 1 && windowCount % maxColumns != 0)
                                   ? windowCount % maxColumns
                                   : maxColumns;

                // 此处采用左对齐，所以水平偏移始终为 0
                int offsetX = 0;

                for (int col = 0; col < windowsInRow; col++)
                {
                    int left = offsetX + col * windowWidth;
                    int top = row * windowHeight;
                    windows.Add(new XWindowRect(left, top, windowWidth, windowHeight));
                    windowIndex++;
                    if (windowIndex >= windowCount)
                        break;
                }
            }



            return windows;
        }


        /// <summary>
        /// 手动调整一个浏览器的试图页面大小
        /// </summary>
        /// <param name="xchrome"></param>
        /// <returns></returns>
        public async Task AdjustmentOneView(XChromeClient xchrome)
        {
            IntPtr hwd = (IntPtr)xchrome.Hwnd;
            //总窗口
            Win32Helper.GetWindowRect(hwd, out var ret);
            //内容窗口
            var legacywindow_hwd = Win32Helper.FindWindowEx(hwd, IntPtr.Zero, "Chrome_RenderWidgetHostHWND", "Chrome Legacy Window");
            Win32Helper.RECT clientRect = new Win32Helper.RECT();
            Win32Helper.GetWindowRect(legacywindow_hwd, out clientRect);
            int ww = clientRect.Right - clientRect.Left;
            int hh = clientRect.Bottom - clientRect.Top;
            //Debug.WriteLine(ww+",,"+hh);
            xchrome.ViewportSize = new ViewportSize { Width = ww, Height = hh };
            foreach (var page in xchrome.BrowserContext.Pages)
            {
                await page.SetViewportSizeAsync(ww, hh);
            }
            //chrome的一个bug，修复
            Win32Helper.ChangeWindowPos(hwd, ret.Left, ret.Top, ret.Right - ret.Left, ret.Bottom - ret.Top);

        }


        /// <summary>
        /// 通过proxy的字符串，变成 Proxy对象
        /// </summary>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public Proxy? getProxy(string proxy = "")
        {

            if (proxy != "")
            {
                if (!proxy.StartsWith("http") && !proxy.StartsWith("socks5"))
                {
                    proxy = "http://" + proxy;
                }
                var _proxy = new Proxy();
                string[] pp = proxy.Split(":");
                _proxy.Server = pp[0] + ":" + pp[1] + ":" + pp[2];
                if (pp.Length > 3)
                {
                    _proxy.Username = pp[3];
                }
                if (pp.Length > 4)
                {
                    _proxy.Password = pp[4];
                }
                return _proxy;
            }
            else
            {
                return null;
            }
        }
    }
}
