/********************************************************************
*                      AutoTester     
*                        Wan,Yu
* AutoTester is a free software, you can use it in any commercial work. 
* But you CAN NOT redistribute it and/or modify it.
*--------------------------------------------------------------------
* Component: TestBrowser.cs
*
* Description: TestBrowser support Internet Explorer. It implent 
*              ITestBrowser interface. You can use TestBrowser to 
*              interactive with Internet Exploter, and get the information
*              of Internet Explorer. 
*
* History: 2007/09/04 wan,yu Init version
*
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

using SHDocVw;
using mshtml;
using Shrinerain.AutoTester.Win32;
using Shrinerain.AutoTester.Interface;

namespace Shrinerain.AutoTester.Function
{

    public class TestBrowser : IDisposable, ITestBrowser
    {
        #region Fileds

        //this struct is used to save the browser status.
        //Sometimes the web app will pop up a new window, it is a new browser, and we will switch to
        //the new browser, after operation finished, we need to go back to the origin browser.
        //so we need a stack to save the status, after pop window disappear, we need to pop the old status from the stack.
        protected struct TestBrowserStatus
        {
            public IntPtr _mainHandle;
            public IntPtr _ieServerHandle;
            public IntPtr _shellDocHandle;
            public InternetExplorer _ie;
            public HTMLDocument _HTMLDom;
        };

        //stack to save the browser status.
        protected static Stack<TestBrowserStatus> _statusStack = new Stack<TestBrowserStatus>(5);

        protected static TestBrowser _testBrowser;

        //Handle of IE.
        protected static IntPtr _mainHandle;

        //handle of client area.
        // client area means the area to display web pages, not include the menu, address bar, etc.
        // we can use Spy++ to get these information.
        protected static IntPtr _ieServerHandle;

        //handle of shell doc.
        protected static IntPtr _shellDocHandle;

        //InternetExplorer is under SHDocVw namespace, we use this to attach to an browser.
        protected InternetExplorer _ie = null;

        //HTML dom, we use HTML dom to get the HTML object.
        protected HTMLDocument _HTMLDom = null;

        //wait for 120 secs, for example, to wait for the browser exist.
        protected int _maxWaitSeconds = 120;

        //every time sleep for 3 secs if browser is not found.
        protected const int _interval = 3;

        //the version of browser, eg 7.0
        protected string _version;

        //the name of browser, eg Internet Explorer.
        protected string _browserName;

        /*we have 3 area here.
         * 1. rectangle of borwser, including menu bar, address bar.
         * 2. rectangle of client area, means the area to display web pages, not including meun bar, address bar.
         * 3. rectangle of web page, a web page may larger than the client area, in this situation, we will see
         *    scroll bar.
         */

        // the browser window's rect
        protected static int _ieLeft;
        protected static int _ieTop;
        protected static int _ieWidth;
        protected static int _ieHeight;

        // the client area's rect
        protected static int _clientTop;
        protected static int _clientLeft;
        protected static int _clientWidth;
        protected static int _clientHeight;

        //the web page's rect, may larger than client area because of scroll bar.
        protected static int _scrollLeft;
        protected static int _scrollTop;
        protected static int _scrollWidth;
        protected static int _scrollHeight;


        #endregion

        #region Sync Event
        //sync event is to mark the important event.

        //start download happened we the browser start to load a web page.
        protected AutoResetEvent _startDownload = new AutoResetEvent(false);

        //document load complete happened when the web page is loaded, then we can start to 
        //find the HTML object.
        protected AutoResetEvent _documentLoadComplete = new AutoResetEvent(false);

        //happened when internet explorer started.
        protected AutoResetEvent _ieStarted = new AutoResetEvent(false);
        // protected AutoResetEvent _ieExisted = new AutoResetEvent(false);

        #endregion

        #region Properties

        public int MaxWaitSeconds
        {
            get { return this._maxWaitSeconds; }
            set
            {
                if (value >= 0)
                {
                    this._maxWaitSeconds = value;
                }
            }
        }

        public static int Left
        {
            get
            {
                return _ieLeft;
            }
        }

        public static int Top
        {
            get
            {
                return _ieTop;
            }
        }

        public static int Width
        {
            get
            {
                return _ieWidth;
            }
        }

        public static int Height
        {
            get
            {
                return _ieHeight;
            }
        }

        public static int ClientTop
        {
            get
            {
                return _clientTop;
            }
        }

        public static int ClientLeft
        {
            get
            {
                return _clientLeft;
            }
        }

        public static int ClientWidth
        {
            get
            {
                return _clientWidth;
            }
        }

        public static int ClientHeight
        {
            get
            {
                return _clientHeight;
            }
        }

        public static int ScrollLeft
        {
            get
            {
                // GetScrollRect();
                return _scrollLeft;
            }
        }
        public static int ScrollTop
        {
            get
            {
                // GetScrollRect();
                return _scrollTop;
            }
        }
        public static int ScrollWidth
        {
            get
            {
                //GetScrollRect();
                return _scrollWidth;
            }
        }
        public static int ScrollHeight
        {
            get
            {
                // GetScrollRect();
                return _scrollHeight;
            }

        }

        //main handle of ie window
        public static IntPtr MainHandle
        {
            get { return _mainHandle; }
        }

        //handle of client area, Internet Explorer_Server
        public static IntPtr IEServerHandle
        {
            get { return _ieServerHandle; }
        }

        #endregion

        #region Methods

        //I keep the constructor as "public" because of reflecting
        public TestBrowser()
        {
            //currently, just support internet explorer
            // default version number is 6.0
            this._browserName = "Internet Explorer";
            this._version = "6.0";
        }

        //singleton
        public static TestBrowser GetInstance()
        {
            if (_testBrowser == null)
            {
                _testBrowser = new TestBrowser();
            }

            return _testBrowser;
        }

        ~TestBrowser()
        {
            // when GC, close AutoResetEvent.
            Dispose();
        }

        public virtual void Dispose()
        {
            if (this._ieStarted != null)
            {
                //  this._ieExisted.Close();
                this._ieStarted.Close();
                this._documentLoadComplete.Close();

                //this._ieExisted = null;
                this._ieStarted = null;
                this._documentLoadComplete = null;
            }

            GC.SuppressFinalize(this);
        }

        #region public methods

        #region operate IE

        /*  start Internet Explorer, and register the event.
         *  if failed, throw CanNotStartTestBrowserException
         */
        public virtual void Start()
        {
            try
            {
                Process p = Process.Start("iexplore.exe");

                //start a new thread to check the browser status, if OK, we will attach _ie to Internet Explorer
                Thread ieExistT = new Thread(new ThreadStart(WaitForIEExist));
                ieExistT.Start();

                //wait until the internet explorer started.
                this._ieStarted.WaitOne();

                if (_ie != null)
                {
                    //if we attached Internet Explorer successfully, register event
                    RegIEEvent();
                }
                else
                {
                    throw new CanNotStartTestBrowserException("Can not start test browser.");
                }

                //max size of browser
                MaxSize();

            }
            catch
            {
                throw new CanNotStartTestBrowserException("Can not start Internet explorer");
            }

        }

        /* void Close()
         * Close Browser.
         * 
         */
        public virtual void Close()
        {
            if (this._ie == null)
            {
                throw new TestBrowserNotFoundException();
            }
            this._ie.Quit();
        }

        /* void Load(string url)
         * Load the expected url. eg: www.sina.com.cn 
         * before we load url, we need to use Start() method to start browser first.
         */
        public virtual void Load(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                throw new CanNotLoadUrlException("Url can not be null.");
            }

            // if ie is not started, wait for 120s.
            if (this._ie == null)
            {
                _ieStarted.WaitOne(this._maxWaitSeconds * 1000, true);
            }


            object tmp = new object();
            try
            {
                // navigate to the expected url.
                this._ie.Navigate(url, ref tmp, ref tmp, ref tmp, ref tmp);
                Thread.Sleep(1 * 1000);
            }
            catch
            {
                throw new CanNotLoadUrlException();
            }

            //wait until the HTML web page is loaded successfully.
            this._documentLoadComplete.WaitOne(_maxWaitSeconds * 1000, true);

        }

        public virtual void Load(Uri url)
        {
            Load(url.ToString());
        }

        /* void Move(int top,int left)
         * move the browser to expected position. 
         * left,top means the position of left corner point
         */
        public virtual void Move(int top, int left)
        {

            if (this._ie == null)
            {
                throw new TestBrowserNotFoundException();
            }
            try
            {
                this._ie.Top = top;
                this._ie.Left = left;
            }
            catch
            {
                throw new CanNotActiveTestBrowserException("Can not move IE.");
            }
        }

        /* void Resize(int width, int height) 
         * resize the browser, set it's width and height
         */
        public virtual void Resize(int width, int height)
        {
            if (width < 1)
            {
                width = 1;
            }
            if (height < 1)
            {
                height = 1;
            }
            try
            {
                this._ie.Width = width;
                this._ie.Height = height;
            }
            catch
            {
                throw new CanNotActiveTestBrowserException("Can not resize IE.");
            }
        }

        /* void Back()
         * let the browser back to the previous url, just like you click the Back button on menu bar.
         * 
         * */
        public virtual void Back()
        {
            if (this._ie == null)
            {
                throw new TestBrowserNotFoundException();
            }
            try
            {
                this._ie.GoBack();
            }
            catch
            {
                throw new CanNotNavigateException("Can not go back.");
            }

        }

        /* void Forward()
         * let the browser navigate to the next page, just like you click the Forward button on menu bar. 
         */
        public virtual void Forward()
        {
            if (this._ie == null)
            {
                throw new TestBrowserNotFoundException();
            }
            try
            {
                this._ie.GoForward();
            }
            catch
            {
                throw new CanNotNavigateException("Can not go forward.");
            }
        }

        /* void Home()
         * let the browser navigate to it's home page.
         */
        public virtual void Home()
        {
            if (this._ie == null)
            {
                throw new TestBrowserNotFoundException();
            }
            try
            {
                this._ie.GoHome();
            }
            catch
            {
                throw new CanNotNavigateException("Can not go home.");
            }

        }

        /* void Refresh()
         * Refresh the current page. 
         */
        public virtual void Refresh()
        {
            if (this._ie == null)
            {
                throw new TestBrowserNotFoundException();
            }
            try
            {
                this._ie.Refresh();
            }
            catch
            {
                throw new CanNotNavigateException("Can not go home.");
            }
        }

        /* void Wait(int seconds)
         * make the browser wait for seconds.
         * if the parameter is smaller than 0, we will wait for int.MaxValue(2,147,483,647) seconds. 
         */
        public virtual void Wait(int seconds)
        {
            if (seconds < 0)
            {
                seconds = int.MaxValue;
            }

            if (seconds > 0)
            {
                Thread.Sleep(seconds * 1000);
            }
        }

        /* void WaitForNewWindow()
         * wait until a new Internet Explorer exist. 
         */
        public virtual void WaitForNewWindow()
        {

        }

        /* void WaitForNewTab()
         *  in tabbed browser, like Internet Explorer 7, wait until a new tab exist.
         * 
         */
        public virtual void WaitForNewTab()
        {

        }

        /* void WaitForNextPage()
         * wait until the browser load a new page.
         * eg: in google.com, you input something, then you click search button, you need to wait the browser to refresh,
         * then you can see the result page.
         */
        public virtual void WaitForNextPage()
        {
            WaitDocumentLoadComplete();
        }

        /* void WaitForPopWindow()
         * wait until the browser pop up a new HTML window.
         * It is a new Internet Explorer_Server control.
         * 
         */
        public virtual void WaitForPopWindow()
        {

            if (_mainHandle == IntPtr.Zero)
            {
                throw new TestBrowserNotFoundException("Can not find Internet explorer.");
            }

            int seconds = 0;
            while (seconds < _maxWaitSeconds)
            {
                Thread.Sleep(1 * 1000);
                seconds++;

                IntPtr popWindowHandle = GetDialogHandle(_mainHandle);

                if (popWindowHandle == IntPtr.Zero)
                {
                    continue;
                }
                else
                {

                    IntPtr popWindowIEServerHandle = GetIEServerHandle(popWindowHandle);

                    if (popWindowIEServerHandle == IntPtr.Zero)
                    {
                        continue;
                    }
                    else
                    {
                        TestBrowserStatus popStatus = new TestBrowserStatus();

                        popStatus._ieServerHandle = _ieServerHandle;
                        popStatus._mainHandle = _mainHandle;
                        popStatus._HTMLDom = _HTMLDom;

                        //save the current browser status.
                        _statusStack.Push(popStatus);

                        _mainHandle = popWindowHandle;
                        _ieServerHandle = popWindowIEServerHandle;
                        _HTMLDom = GetHTMLDomFromHandle(popWindowIEServerHandle);

                        // get the pop up window's size
                        GetSize();

                        break;

                    }
                }

            }




        }

        /* void MaxSize()
         * max the browser.
         */
        public virtual void MaxSize()
        {
            try
            {
                Win32API.PostMessage(_mainHandle, Convert.ToInt32(Win32API.WindowMessages.WM_SYSCOMMAND), Convert.ToInt32(Win32API.WindowMenuMessage.SC_MAXIMIZE), 0);
            }
            catch (Exception e)
            {
                throw new CanNotActiveTestBrowserException("Can not MAX Internet Explorer: " + e.Message);
            }
        }

        /* void Active()
         * make the browser active, set it focus and to top most window, then we can interactive with it.
         */
        public virtual void Active()
        {
            try
            {
                Win32API.SetForegroundWindow(_mainHandle);
            }
            catch (Exception e)
            {
                throw new CanNotActiveTestBrowserException("Can not Active browser: " + e.Message);
            }
        }

        #endregion

        #region Get IE Information

        /* string GetCurrentUrl()
         * return the current url in the address bar of browser
         * 
         */
        public virtual string GetCurrentUrl()
        {
            if (this._ie == null)
            {
                throw new TestBrowserNotFoundException();
            }
            try
            {
                return this._ie.LocationURL;
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not get the current url.");
            }
        }

        /* string GetStatusText()
         * return the status text of browser.
         */
        public virtual string GetStatusText()
        {
            if (this._ie == null)
            {
                throw new TestBrowserNotFoundException();
            }
            try
            {
                return this._ie.StatusText;
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not get the status text.");
            }
        }

        /* bool IsMenuVisible()
         * return true if the menu bar is visible. 
         * sometimes we will cancel the menu bar, eg: pop up window.
         */
        public virtual bool IsMenuVisiable()
        {
            if (this._ie == null)
            {
                throw new TestBrowserNotFoundException();
            }
            try
            {
                return this._ie.MenuBar;
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not get the menu status.");
            }
        }

        /* bool IsResizeable()
         * return true if the browser can be resized.
         * 
         */
        public virtual bool IsResizeable()
        {
            if (this._ie == null)
            {
                throw new TestBrowserNotFoundException();
            }
            try
            {
                return this._ie.Resizable;
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not get the resize status.");
            }
        }

        /* bool IsFullScreen()
         * return true if the  browser is full screen.
         * 
         */
        public virtual bool IsFullScreen()
        {
            if (this._ie == null)
            {
                throw new TestBrowserNotFoundException();
            }
            try
            {
                return this._ie.FullScreen;
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not the full screen status.");
            }

        }

        /* string GetBrowserName()
         * return the name of the browser. eg: Internet Explorer.
         * 
         */
        public virtual String GetBrowserName()
        {
            return this._browserName;
        }

        /* string GetBrowserVersion()
         * return the version number of the browser.
         */
        public virtual String GetBrowserVersion()
        {
            return this._version;
        }

        #endregion

        #region SYNC

        /* void WaitDocumentLoadComplete(int seconds)
         * wait until the HTML document load completely.
         * 
         */
        public virtual void WaitDocumentLoadComplete(int seconds)
        {
            if (seconds < 0)
            {
                seconds = 0;
            }

            // Console.WriteLine(DateTime.Now.ToString());

            _startDownload.Reset();
            _startDownload.WaitOne(seconds * 1000, true);

            _documentLoadComplete.Reset();
            _documentLoadComplete.WaitOne(seconds * 1000, true);

        }

        public virtual void WaitDocumentLoadComplete()
        {
            WaitDocumentLoadComplete(this._maxWaitSeconds);
        }

        #endregion


        #endregion

        #region protected virtual help methods

        /* InternetExplorer AttacchIE(IntPtr ieHandle)
         * return the instance of InternetExplorer.
         * 
         */
        protected virtual InternetExplorer AttachIE(IntPtr ieHandle)
        {
            SHDocVw.ShellWindows allBrowsers = null;

            //try 3 times to attach IE.
            int j = 0;
            while (j < 3)
            {
                //get all shell browser.
                allBrowsers = new ShellWindows();

                if (allBrowsers.Count == 0)
                {
                    throw new Exception("Error: Can not find Internet Explorer.");
                }

                for (int i = 0; i < allBrowsers.Count; i++)
                {
                    InternetExplorer tempIE = null;
                    try
                    {
                        tempIE = (InternetExplorer)allBrowsers.Item(i);
                        if (tempIE == null)
                        {
                            continue;
                        }

                        if (tempIE.HWND == 0)
                        {
                            continue;
                        }
                        else if (tempIE.HWND == (int)ieHandle) // if the browser handle equal to the browser handle we started, return it.
                        {
                            return tempIE;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                j++;

                Thread.Sleep(_interval * 1000);
            }

            return null;
        }

        /* void WaitFOrIEExist(int seconds.)
         * wait for 120 seconds max to detect if IE browser is started.
         */
        protected virtual void WaitForIEExist(int seconds)
        {
            if (seconds < 0)
            {
                seconds = 0;
            }
            int curSleepTime = 0;
            IntPtr ieHwd = IntPtr.Zero;

            while (curSleepTime <= seconds)
            {
                Thread.Sleep(_interval * 1000);
                curSleepTime += _interval;

                if (ieHwd == IntPtr.Zero)
                {
                    ieHwd = GetIEMainHandle();
                }
                if (ieHwd != IntPtr.Zero)
                {
                    _mainHandle = ieHwd;

                    _ie = AttachIE(ieHwd);

                    if (_ie == null)
                    {
                        continue;
                    }
                    else
                    {
                        Thread.Sleep(_interval * 1000);
                        this._ieStarted.Set();
                        break;
                    }
                }
            }
        }

        protected virtual void WaitForIEExist()
        {
            WaitForIEExist(this._maxWaitSeconds);
        }

        /* void GetClientRect()
         * client rect: the web page rect, not include menu, address bar ect.
         */
        protected virtual void GetClientRect()
        {
            if (_mainHandle != IntPtr.Zero)
            {
                if (_shellDocHandle == IntPtr.Zero)
                {
                    _shellDocHandle = GetShellDocHandle(_mainHandle);// Win32API.FindWindowEx(_mainHandle, IntPtr.Zero, "Shell DocObject View", null);
                    if (_shellDocHandle == IntPtr.Zero)
                    {
                        throw new TestBrowserNotFoundException("Can not get IE Client location.");
                    }
                }


                //determine the yellow warning bar, for example, if the web page contains ActiveX, we can see the yellow bar at the top of the web page.
                //if the warning bar exist, we need to add 20 height to each html control.
                IntPtr warnBar = GetWarnBarHandle(_shellDocHandle);// Win32API.FindWindowEx(_shellDocHandle, IntPtr.Zero, "#32770 (Dialog)", null);
                int addHeight = 0;
                if (warnBar != IntPtr.Zero)
                {
                    Win32API.Rect warnRect = new Win32API.Rect();
                    Win32API.GetClientRect(warnBar, ref warnRect);
                    addHeight = warnRect.Height;
                }

                //Get the actual client area rect, which shows web page to the end user.
                if (_ieServerHandle == IntPtr.Zero)
                {
                    _ieServerHandle = GetIEServerHandle(_shellDocHandle);// Win32API.FindWindowEx(_shellDocHandle, IntPtr.Zero, "Internet Explorer_Server", null);

                    if (_ieServerHandle == IntPtr.Zero)
                    {
                        throw new TestBrowserNotFoundException("Can not get IE Client location.");
                    }
                }

                Win32API.Rect tmpRect = new Win32API.Rect();
                Win32API.GetWindowRect(_ieServerHandle, ref tmpRect);

                _clientLeft = tmpRect.left;
                _clientTop = tmpRect.top + addHeight;
                _clientWidth = tmpRect.Width;
                _clientHeight = tmpRect.Height;

            }
            else
            {
                throw new TestBrowserNotFoundException("Can not get IE Client location.");
            }
        }

        /* void GetBrowserRect()
         * browser rect: the whole rect of IE, include menu, address bar etc.
         */
        protected virtual void GetBrowserRect()
        {
            if (this._ie == null)
            {
                throw new TestBrowserNotFoundException();
            }

            try
            {
                _ieLeft = this._ie.Left;
                _ieTop = this._ie.Top;
                _ieWidth = this._ie.Width;
                _ieHeight = this._ie.Height;
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not get the rect.");
            }
        }

        /* void GetScrollRect()
         * the whole web page rect, include invisible part, for example, some web pages are too long to display, we need to scroll them.
         * 
         */
        protected virtual void GetScrollRect()
        {
            try
            {
                HTMLBody bodyElement = (HTMLBody)this._HTMLDom.body;
                _scrollWidth = bodyElement.scrollWidth;
                _scrollHeight = bodyElement.scrollHeight;

                // scrollLeft means the left that Can Not been seen, scrollTop the same.
                _scrollLeft = bodyElement.scrollLeft;
                _scrollTop = bodyElement.scrollTop;
            }
            catch
            {
                _scrollWidth = 0;
                _scrollHeight = 0;
                _scrollLeft = 0;
                _scrollTop = 0;
            }

        }

        /* void GetSize()
         * Get all size information of browser.
         */
        protected virtual void GetSize()
        {
            GetBrowserRect();
            GetClientRect();
            GetScrollRect();
        }

        /* void GetPrevTestBrowserStatus()
         * Get previous browser status. eg: when pop up window disapper, we need to return to the main window.
         */
        protected virtual void GetPrevTestBrowserStatus()
        {
            if (_statusStack.Count > 0)
            {
                try
                {
                    //get the previous status from stack.
                    TestBrowserStatus tmp = _statusStack.Pop();

                    _mainHandle = tmp._mainHandle;
                    _ieServerHandle = tmp._ieServerHandle;
                    _shellDocHandle = tmp._shellDocHandle;
                    _ie = tmp._ie;
                    _HTMLDom = tmp._HTMLDom;
                }
                catch
                {
                    throw new CanNotAttachTestBrowserException("Can not get previous handle.");
                }

            }
        }

        /* IntPtr GetIEMainHandle()
         * return the handle of IEFrame, this is the parent handle of Internet Explorer. 
         * we can use Spy++ to get the the handle.
         */
        protected virtual IntPtr GetIEMainHandle()
        {
            return Win32API.FindWindow("IEFrame", null);
        }

        /* IntPtr GetShellDocHandle(IntPtr mainHandle)
         * return the handle of Shell DocObject View.
         * we can use Spy++ to get the tree structrue of Internet Explorer handles. 
         */
        protected virtual IntPtr GetShellDocHandle(IntPtr mainHandle)
        {
            if (mainHandle == IntPtr.Zero)
            {
                mainHandle = GetIEMainHandle();
            }

            //update for Internet Explorer 7
            //Internet Explorer 7 is a tab browser, we need to find "TabWindowClass" before we get the "Sheel DocObject View"

            IntPtr tabWindow = Win32API.FindWindowEx(mainHandle, IntPtr.Zero, "TabWindowClass", null);

            if (tabWindow == IntPtr.Zero) //No tab, means IE 6.0 or lower
            {
                return Win32API.FindWindowEx(mainHandle, IntPtr.Zero, "Shell DocObject View", null);
            }
            else
            {
                //tab handle found, means IE 7
                this._version = "7.0";
                return Win32API.FindWindowEx(tabWindow, IntPtr.Zero, "Shell DocObject View", null);
            }

        }

        /* IntPtr GetWarnBarHandle(IntPtr shellHandle)
         * return the warning bar handle. in Internet Explorer 6.0 with XP2 or Internet Explorer 7.0.
         * when the web page contains ActiveX (eg: FlashPlayer), we can see a yellow bar on the browser.
         */
        protected virtual IntPtr GetWarnBarHandle(IntPtr shellHandle)
        {
            if (shellHandle == IntPtr.Zero)
            {
                shellHandle = GetShellDocHandle(IntPtr.Zero);
            }

            return Win32API.FindWindowEx(shellHandle, IntPtr.Zero, "#32770 (Dialog)", null);
        }

        /* IntPtr GetIEServerHandle(IntPtr shellHandle)
         * return the handle of Internet Explorer_Server.
         * This control is used to display web page.
         */
        protected virtual IntPtr GetIEServerHandle(IntPtr shellHandle)
        {
            if (shellHandle == IntPtr.Zero)
            {
                shellHandle = GetShellDocHandle(IntPtr.Zero);
            }
            return Win32API.FindWindowEx(shellHandle, IntPtr.Zero, "Internet Explorer_Server", null);
        }

        /* IntPtr GetDialogHandle(IntPtr mainHandle)
         * return the handle of pop up page.
         * the name is Internet Explorer_TridentDlgFrame.
         */
        protected virtual IntPtr GetDialogHandle(IntPtr mainHandle)
        {
            if (mainHandle == IntPtr.Zero)
            {
                mainHandle = GetIEMainHandle();
            }
            IntPtr popHandle = Win32API.FindWindow("Internet Explorer_TridentDlgFrame", null);
            if (popHandle != IntPtr.Zero)
            {
                IntPtr parentHandle = Win32API.GetParent(popHandle);

                if (parentHandle == mainHandle)
                {
                    return popHandle;
                }
            }

            return IntPtr.Zero;
        }


        protected virtual HTMLDocument GetHTMLDomFromHandle(IntPtr ieServerHandle)
        {

            int nMsg = Win32API.RegisterWindowMessage("WM_HTML_GETOBJECT");
            UIntPtr lRes;
            if (Win32API.SendMessageTimeout(ieServerHandle, nMsg, 0, 0,
                 Win32API.SMTO_ABORTIFHUNG, 1000, out lRes) == 0)
            {
                return null;
            }
            return (HTMLDocument)Win32API.ObjectFromLresult(lRes,
                 typeof(mshtml.IHTMLDocument).GUID, IntPtr.Zero);
        }

        #endregion

        #region event

        //register IE events
        protected virtual void RegIEEvent()
        {
            RegStartDownloadEvent();
            RegDocumentLoadCompleteEvent();
            RegNavigateFailedEvent();
            RegRectChangeEvent();
            RegScrollEvent();
            RegOnNewWindowEvent();

        }

        protected virtual void RegStartDownloadEvent()
        {
            try
            {
                _ie.DownloadBegin += new DWebBrowserEvents2_DownloadBeginEventHandler(OnDownloadBegin);
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not register Downloadbegin event.");
            }
        }

        protected virtual void RegOnNewWindowEvent()
        {
            try
            {
                //  _ie.NewWindow3 += new DWebBrowserEvents2_NewWindow3EventHandler(OnNewWindow3);
                _ie.NewWindow2 += new DWebBrowserEvents2_NewWindow2EventHandler(OnNewWindow2);
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not register new window event.");
            }

        }

        protected virtual void RegScrollEvent()
        {

            // HTMLBody body = (HTMLBody)_HTMLDom.body;
            //_HTMLDom.parentWindow.
            //body.s

        }

        //document/(html page) load complete
        protected virtual void RegDocumentLoadCompleteEvent()
        {
            try
            {
                _ie.DocumentComplete += new DWebBrowserEvents2_DocumentCompleteEventHandler(OnDocumentLoadComplete);
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not register document complete event.");
            }
        }

        //resize the window
        protected virtual void RegRectChangeEvent()
        {
            try
            {
                _ie.WindowSetTop += new DWebBrowserEvents2_WindowSetTopEventHandler(OnRectChanged);
                _ie.WindowSetLeft += new DWebBrowserEvents2_WindowSetLeftEventHandler(OnRectChanged);
                _ie.WindowSetWidth += new DWebBrowserEvents2_WindowSetWidthEventHandler(OnRectChanged);
                _ie.WindowSetHeight += new DWebBrowserEvents2_WindowSetHeightEventHandler(OnRectChanged);
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not register Rect change event.");
            }
        }

        //load url failed
        protected virtual void RegNavigateFailedEvent()
        {
            try
            {
                _ie.NavigateError += new DWebBrowserEvents2_NavigateErrorEventHandler(OnNavigateError);
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not register load error event.");
            }
        }

        protected virtual void OnNavigateError(object pDisp, ref object URL, ref object Frame, ref object StatusCode, ref bool Cancel)
        {
            throw new CanNotLoadUrlException();
        }


        //fire when new web page pops up, eg. javascript: window.open
        protected void OnNewWindow2(ref object ppDisp, ref bool Cancel)
        {
            System.Windows.Forms.MessageBox.Show("new window2!");
            // throw new Exception("New window.");
            //throw new Exception("The method or operation is not implemented.");
        }

        protected void OnNewWindow3(ref object ppDisp, ref bool Cancel, uint dwFlags, string bstrUrlContext, string bstrUrl)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        protected virtual void OnDownloadBegin()
        {
            //Console.WriteLine("Download begin");
            this._startDownload.Set();
        }

        //when document load complete, we can start to operate the html controls
        protected virtual void OnDocumentLoadComplete(object pDesp, ref object pUrl)
        {
            try
            {
                //Console.WriteLine("TestBrowser");

                this._HTMLDom = (HTMLDocument)_ie.Document;

                GetSize();

                // Thread.Sleep(1000 * 1);

            }
            catch (TestBrowserNotFoundException)
            {
                throw;
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not parse html document.");
            }
            finally
            {
                // Console.WriteLine("set");
                _documentLoadComplete.Set();
            }

        }

        //when the position or rect of ie is changed, we need to re-calculate the position of html controls.
        protected virtual void OnRectChanged(int size)
        {
            try
            {
                GetSize();
            }
            catch (TestBrowserNotFoundException)
            {
                throw;
            }
            catch
            {
                throw new CanNotAttachTestBrowserException("Can not register rect change event.");
            }
        }

        #endregion

        #endregion
    }
}
