﻿#define DISABLE_HOOK
#define DISABLE_CEF

#if true
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
//using CefSharp;
//using CefSharp.OffScreen;
using GTA;
using GTA.Native;
using GTANetwork.GUI.DirectXHook.Hook;
using GTANetwork.GUI.Extern;
using GTANetwork.Javascript;
using GTANetwork.Util;
using Microsoft.ClearScript.V8;
using SharpDX;
using SharpDX.Diagnostics;
using Xilium.CefGlue;
using Point = System.Drawing.Point;


namespace GTANetwork.GUI
{
    public class CefController : Script
    {
        private static bool _showCursor;

        public static bool ShowCursor
        {
            get { return _showCursor; }
            set
            {
                if (!_showCursor && value)
                {
                    _justShownCursor = true;
                    _lastShownCursor = Util.Util.TickCount;
                }
                _showCursor = value;
            }
        }

        private static bool _justShownCursor;
        private static long _lastShownCursor = 0;
        public static PointF _lastMousePoint;
        public static int GameFPS = 1;
        private Keys _lastKey;

        public static CefEventFlags GetMouseModifiers(bool leftbutton, bool rightButton)
        {
            CefEventFlags mod = CefEventFlags.None;

            if (leftbutton) mod |= CefEventFlags.LeftMouseButton;
            if (rightButton) mod |= CefEventFlags.RightMouseButton;

            return mod;
        }
        /*
        public CefController()
        {
            Tick += (sender, args) =>
            {
                GameFPS = (int)Game.FPS;
                
                if (ShowCursor)
                {
                    Game.DisableAllControlsThisFrame(0);
                    if (CEFManager.D3D11_DISABLED)
                        Function.Call(Hash._SHOW_CURSOR_THIS_FRAME);
                }
                else
                {
                    return;
                }
                
                var res = GTA.UI.Screen.Resolution;
                var mouseX = Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, (int)GTA.Control.CursorX) * res.Width;
                var mouseY = Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, (int)GTA.Control.CursorY) * res.Height;

                _lastMousePoint = new PointF(mouseX, mouseY);

                var mouseDown = Game.IsDisabledControlJustPressed(0, GTA.Control.CursorAccept);
                var mouseDownRN = Game.IsDisabledControlPressed(0, GTA.Control.CursorAccept);
                var mouseUp = Game.IsDisabledControlJustReleased(0, GTA.Control.CursorAccept);

                var rmouseDown = Game.IsDisabledControlJustPressed(0, GTA.Control.CursorCancel);
                var rmouseDownRN = Game.IsDisabledControlPressed(0, GTA.Control.CursorCancel);
                var rmouseUp = Game.IsDisabledControlJustReleased(0, GTA.Control.CursorCancel);

                var wumouseDown = Game.IsDisabledControlJustPressed(0, GTA.Control.CursorScrollUp);
                var wumouseUp = Game.IsDisabledControlJustReleased(0, GTA.Control.CursorScrollUp);

                var wdmouseDown = Game.IsDisabledControlJustPressed(0, GTA.Control.CursorScrollDown);
                var wdmouseUp = Game.IsDisabledControlJustReleased(0, GTA.Control.CursorScrollDown);

                #if !DISABLE_CEF
                foreach (var browser in CEFManager.Browsers)
                {
                    if (!browser.IsInitialized()) continue;

                    if (!browser._hasFocused)
                    {
                        browser._browser.GetBrowser().GetHost().SetFocus(true);
                        browser._browser.GetBrowser().GetHost().SendFocusEvent(true);
                        browser._hasFocused = true;
                    }

                    if (mouseX > browser.Position.X && mouseY > browser.Position.Y &&
                        mouseX < browser.Position.X + browser.Size.Width &&
                        mouseY < browser.Position.Y + browser.Size.Height)
                    {
                        browser._browser.GetBrowser()
                            .GetHost()
                            .SendMouseMoveEvent((int)(mouseX - browser.Position.X), (int)(mouseY - browser.Position.Y),
                                false, GetMouseModifiers(mouseDownRN, rmouseDownRN));

                        if (mouseDown)
                            browser._browser.GetBrowser()
                                .GetHost()
                                .SendMouseClickEvent((int)(mouseX - browser.Position.X),
                                    (int)(mouseY - browser.Position.Y), MouseButtonType.Left, false, 1, GetMouseModifiers(mouseDownRN, rmouseDownRN));

                        if (mouseUp)
                            browser._browser.GetBrowser()
                                .GetHost()
                                .SendMouseClickEvent((int)(mouseX - browser.Position.X),
                                    (int)(mouseY - browser.Position.Y), MouseButtonType.Left, true, 1, GetMouseModifiers(mouseDownRN, rmouseDownRN));

                        if (rmouseDown)
                            browser._browser.GetBrowser()
                                .GetHost()
                                .SendMouseClickEvent((int)(mouseX - browser.Position.X),
                                    (int)(mouseY - browser.Position.Y), MouseButtonType.Right, false, 1, GetMouseModifiers(mouseDownRN, rmouseDownRN));

                        if (rmouseUp)
                            browser._browser.GetBrowser()
                                .GetHost()
                                .SendMouseClickEvent((int)(mouseX - browser.Position.X),
                                    (int)(mouseY - browser.Position.Y), MouseButtonType.Right, true, 1, GetMouseModifiers(mouseDownRN, rmouseDownRN));

                        if (wdmouseDown)
                            browser._browser.GetBrowser()
                                .GetHost()
                                .SendMouseWheelEvent((int) (mouseX - browser.Position.X),
                                    (int) (mouseY - browser.Position.Y), 0, -30, GetMouseModifiers(mouseDownRN, rmouseDownRN));

                        if (wumouseDown)
                            browser._browser.GetBrowser()
                                .GetHost()
                                .SendMouseWheelEvent((int)(mouseX - browser.Position.X),
                                    (int)(mouseY - browser.Position.Y), 0, 30, GetMouseModifiers(mouseDownRN, rmouseDownRN));
                    }
                }

                #endif
            };

            KeyDown += (sender, args) =>
            {
                if (!ShowCursor) return;

                if (_justShownCursor && Util.Util.TickCount - _lastShownCursor < 500)
                {
                    _justShownCursor = false;
                    return;
                }

#if !DISABLE_CEF

                foreach (var browser in CEFManager.Browsers)
                {
                    if (!browser.IsInitialized()) continue;

                    CefEventFlags mod = CefEventFlags.None;
                    if (args.Control) mod |= CefEventFlags.ControlDown;
                    if (args.Shift) mod |= CefEventFlags.ShiftDown;
                    if (args.Alt) mod |= CefEventFlags.AltDown;
                    
                    KeyEvent kEvent = new KeyEvent();
                    kEvent.Type = KeyEventType.KeyDown;
                    kEvent.Modifiers = mod;
                    kEvent.WindowsKeyCode = (int) args.KeyCode;
                    kEvent.NativeKeyCode = (int)args.KeyValue;
                    browser._browser.GetBrowser().GetHost().SendKeyEvent(kEvent);

                    KeyEvent charEvent = new KeyEvent();
                    charEvent.Type = KeyEventType.Char;
                    
                    var key = args.KeyCode;

                    if ((key == Keys.ShiftKey && _lastKey == Keys.Menu) ||
                        (key == Keys.Menu && _lastKey == Keys.ShiftKey))
                    {
                        ClassicChat.ActivateKeyboardLayout(1, 0);
                        return;
                    }

                    _lastKey = key;

                    if (key == Keys.Escape)
                    {
                        return;
                    }

                    var keyChar = ClassicChat.GetCharFromKey(key, Game.IsKeyPressed(Keys.ShiftKey), Game.IsKeyPressed(Keys.Menu) && Game.IsKeyPressed(Keys.ControlKey));

                    if (keyChar.Length == 0 || keyChar[0] == 27) return;
                    
                    charEvent.WindowsKeyCode = keyChar[0];
                    charEvent.Modifiers = mod;
                    browser._browser.GetBrowser().GetHost().SendKeyEvent(charEvent);
                }

#endif
            };

            KeyUp += (sender, args) =>
            {
                #if !DISABLE_CEF
                if (!ShowCursor) return;
                foreach (var browser in CEFManager.Browsers)
                {
                    if (!browser.IsInitialized()) continue;

                    KeyEvent kEvent = new KeyEvent();
                    kEvent.Type = KeyEventType.KeyUp;
                    kEvent.WindowsKeyCode = (int)args.KeyCode;
                    browser._browser.GetBrowser().GetHost().SendKeyEvent(kEvent);
                }
                #endif
            };
        }
        */
    }

    internal class DemoCefApp : CefApp
    {
    }

    internal class DemoCefLoadHandler : CefLoadHandler
    {
        protected override void OnLoadStart(CefBrowser browser, CefFrame frame)
        {
            // A single CefBrowser instance can handle multiple requests
            //   for a single URL if there are frames (i.e. <FRAME>, <IFRAME>).
            //if (frame.IsMain)
            {
                LogManager.SimpleLog("cef", "START: " + browser.GetMainFrame().Url);
            }
        }

        protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            //if (frame.IsMain)
            {
                LogManager.SimpleLog("cef", string.Format("END: {0}, {1}", browser.GetMainFrame().Url, httpStatusCode));
            }
        }
    }

    internal class DemoLifeSpanHandler : CefLifeSpanHandler
    {
        private DemoCefClient bClient;

        internal DemoLifeSpanHandler(DemoCefClient bc)
        {
            this.bClient = bc;
        }

        protected override void OnAfterCreated(CefBrowser browser)
        {
            base.OnAfterCreated(browser);
            this.bClient.Created(browser);
        }
    }

    internal class DemoCefRenderHandler : CefRenderHandler
    {
        private int _windowHeight;
        private int _windowWidth;

        public Bitmap LastBitmap;
        public readonly object BitmapLock = new object();

        public DemoCefRenderHandler(int windowWidth, int windowHeight)
        {
            _windowWidth = windowWidth;
            _windowHeight = windowHeight;
        }

        public void SetSize(int width, int height)
        {
            _windowHeight = height;
            _windowWidth = width;
        }

        protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo)
        {
            
        }

        protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect)
        {
            return GetViewRect(browser, ref rect);
        }

        protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY)
        {
            LogManager.SimpleLog("cef", "Enter GetScreenPoint");
            screenX = viewX;
            screenY = viewY;
            return true;
        }

        protected override bool GetViewRect(CefBrowser browser, ref CefRectangle rect)
        {
            LogManager.SimpleLog("cef", "Enter GetViewRect");
            rect.X = 0;
            rect.Y = 0;
            rect.Width = _windowWidth;
            rect.Height = _windowHeight;
            return true;
        }

        protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo)
        {
            return false;
        }

        protected override void OnPopupSize(CefBrowser browser, CefRectangle rect)
        {
        }

        protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
        {
            LogManager.SimpleLog("cef", "Enter OnPaint...");
            //lock (BitmapLock)
            {
                LogManager.SimpleLog("cef", "Rendering browser...");
                //if (LastBitmap != null) LastBitmap.Dispose();
                LogManager.SimpleLog("cef", "Rendered. Saving....");
                LastBitmap = new Bitmap(width, height, width*4, PixelFormat.Format32bppRgb, buffer);
                if (CEFManager._box != null)
                    CEFManager._box.Image = LastBitmap;
                LogManager.SimpleLog("cef", "Saved!");
                //LastBitmap.Save("LastBitmap.png");
                LogManager.SimpleLog("cef", "Saved again.");
            }
        }

        protected override void OnScrollOffsetChanged(CefBrowser browser)
        {
        }
    }

    internal class DemoCefClient : CefClient
    {
        private readonly DemoCefLoadHandler _loadHandler;
        private readonly DemoCefRenderHandler _renderHandler;
        private readonly DemoLifeSpanHandler _lifeSpanHandler;

        public event EventHandler OnCreated;

        public DemoCefClient(int windowWidth, int windowHeight)
        {
            _renderHandler = new DemoCefRenderHandler(windowWidth, windowHeight);
            _loadHandler = new DemoCefLoadHandler();
            _lifeSpanHandler = new DemoLifeSpanHandler(this);
        }

        public void SetSize(int w, int h)
        {
            _renderHandler.SetSize(w, h);
        }

        public Bitmap GetLastBitmap()
        {
            lock (_renderHandler.BitmapLock)
            {
                return _renderHandler.LastBitmap;
            }
        }

        public void Created(CefBrowser bs)
        {
            if (this.OnCreated != null)
            {
                this.OnCreated(bs, EventArgs.Empty);
            }
        }

        protected override CefRenderHandler GetRenderHandler()
        {
            LogManager.SimpleLog("cef", "Renderer requested.");
            return _renderHandler;
        }

        protected override CefLoadHandler GetLoadHandler()
        {
            return _loadHandler;
        }

        protected override CefLifeSpanHandler GetLifeSpanHandler()
        {
            return _lifeSpanHandler;
        }
    }

    public static class CEFManager
    {
        #if DISABLE_HOOK
        public const bool D3D11_DISABLED = true;
        #else
        public const bool D3D11_DISABLED = false;
        #endif


        public static void InitializeCef()
        {
#if !DISABLE_CEF
            //ThreadPool.QueueUserWorkItem((WaitCallback) delegate
            //{
                CefRuntime.Load(Main.GTANInstallDir + "\\cef");

                var args = new string[0];

                var cefMainArgs = new CefMainArgs(args);
                var cefApp = new DemoCefApp();
                
                if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp, IntPtr.Zero) != -1)
                {
                    LogManager.SimpleLog("cef", "CefRuntime could not execute the secondary process.");
                }

                var cefSettings = new CefSettings()
                {
                    SingleProcess = false,
                    MultiThreadedMessageLoop = true,
                    WindowlessRenderingEnabled = true,
                    /*
                    CachePath = Main.GTANInstallDir + "\\cef",
                    ResourcesDirPath = Main.GTANInstallDir + "\\cef",
                    LocalesDirPath = Main.GTANInstallDir + "\\cef\\locales",
                    BrowserSubprocessPath = Main.GTANInstallDir + "\\cef",
                    */
                    NoSandbox = true,
                };

                CefRuntime.Initialize(cefMainArgs, cefSettings, cefApp, IntPtr.Zero);


                var form = new Form();

                _box = new PictureBox();
                _box.Dock = DockStyle.Fill;
                form.Controls.Add(_box);

                ThreadPool.QueueUserWorkItem((WaitCallback)delegate
                {
                    form.ShowDialog();
                });
            /*
            // Instruct CEF to not render to a window at all.
            CefWindowInfo cefWindowInfo = CefWindowInfo.Create();
            cefWindowInfo.SetAsWindowless(IntPtr.Zero, true);

            // Settings for the browser window itself (e.g. should JavaScript be enabled?).
            var cefBrowserSettings = new CefBrowserSettings()
            {
                JavaScriptCloseWindows = CefState.Disabled,
                JavaScriptOpenWindows = CefState.Disabled,
            };

            // Initialize some the cust interactions with the browser process.
            // The browser window will be 1280 x 720 (pixels).
            var cefClient = new DemoCefClient(1280, 720);

            // Start up the browser instance.
            string url = "http://www.reddit.com/";
            //var browser = CefBrowserHost.CreateBrowserSync(cefWindowInfo, cefClient, cefBrowserSettings, url);
            CefBrowserHost.CreateBrowser(cefWindowInfo, cefClient, cefBrowserSettings, url);

            CefBrowser browser = null;

            cefClient.OnCreated += (sender, eventArgs) =>
            {
                browser = (CefBrowser)sender;
                LogManager.SimpleLog("cef", "Ready!");
            };
            */
            //});
#endif
        }

        public static PictureBox _box;

        public static void DisposeCef()
        {
#if !DISABLE_CEF
            CefRuntime.Shutdown();
#endif
        }

        public static void Initialize(Size screenSize)
        {
            ScreenSize = screenSize;
#if !DISABLE_HOOK
            SharpDX.Configuration.EnableObjectTracking = true;
            Configuration.EnableReleaseOnFinalizer = true;
            Configuration.EnableTrackingReleaseOnFinalizer = true;
            StopRender = false;
            Disposed = false;

            try
            {
                DirectXHook = new DXHookD3D11(screenSize.Width, screenSize.Height);
                DirectXHook.Hook();
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "DIRECTX START");
            }
#endif

            RenderThread = new Thread(RenderLoop);
            RenderThread.IsBackground = true;
            RenderThread.Start();
        }

        public static List<Browser> Browsers = new List<Browser>();
        public static int FPS = 30;
        public static Thread RenderThread;
        public static bool StopRender;
        public static Size ScreenSize;
        public static bool Disposed = true;

        internal static DXHookD3D11 DirectXHook;

        private static long _lastCefRender = 0;
        private static Bitmap _lastCefBitmap = null;
        
        public static void RenderLoop()
        {
            Application.ThreadException += ApplicationOnThreadException;
            AppDomain.CurrentDomain.UnhandledException += AppDomainException;
            
            LogManager.DebugLog("STARTING MAIN LOOP");


            var cursor = new Bitmap(Main.GTANInstallDir + "\\images\\cef\\cursor.png");

#if !DISABLE_HOOK
            SharpDX.Configuration.EnableObjectTracking = true;

            while (!StopRender)
            {
                try
                {
                    using (
                        Bitmap doubleBuffer = new Bitmap(ScreenSize.Width, ScreenSize.Height,
                            PixelFormat.Format32bppArgb))
                    {

                        if (!Main.MainMenu.Visible)
                            using (var graphics = Graphics.FromImage(doubleBuffer))
                            {
#if !DISABLE_CEF
                                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                                lock (Browsers)
                                    foreach (var browser in Browsers)
                                    {
                                        if (browser.Headless) continue;
                                        var bitmap = browser.GetRawBitmap();

                                        if (bitmap == null) continue;

                                        if (browser.Pinned == null || browser.Pinned.Length != 4)
                                            graphics.DrawImageUnscaled(bitmap, browser.Position);
                                        else
                                        {
                                            var bmOut = new FastBitmap(doubleBuffer);
                                            var ourText = new FastBitmap(bitmap);

                                            QuadDistort.DrawBitmap(ourText,
                                                browser.Pinned[0].Floor(),
                                                browser.Pinned[1].Floor(),
                                                browser.Pinned[2].Floor(),
                                                browser.Pinned[3].Floor(),
                                                bmOut);

                                            graphics.DrawImageUnscaled(bmOut, 0, 0);
                                        }
                                        bitmap.Dispose();
                                    }
#endif
                                if (CefController.ShowCursor)
                                    graphics.DrawImage(cursor, CefController._lastMousePoint);
                            }

                        DirectXHook.SetBitmap(doubleBuffer);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.LogException(ex, "DIRECTX HOOK");
                }
                finally
                {
                    Thread.Sleep(1000 / FPS);
                }
            }

            cursor.Dispose();

            lock (Browsers)
            {
                foreach (var browser in CEFManager.Browsers)
                {
                    browser.Dispose();
                }

                Browsers.Clear();
            }

            if (_lastCefBitmap != null)
            {
                _lastCefBitmap.Dispose();
                _lastCefBitmap = null;
            }

            try
            {
                DirectXHook.Dispose();
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "DIRECTX DISPOSAL");
            }
#endif

            Application.ThreadException -= ApplicationOnThreadException;
            AppDomain.CurrentDomain.UnhandledException -= AppDomainException;

            Disposed = true;
        }

        private static void ApplicationOnThreadException(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
        {
            LogManager.LogException(threadExceptionEventArgs.Exception, "APPTHREAD");
        }

        private static void AppDomainException(object sender, UnhandledExceptionEventArgs threadExceptionEventArgs)
        {
            LogManager.LogException(threadExceptionEventArgs.ExceptionObject as Exception, "APPTHREAD");
        }
    }
    

    public class BrowserJavascriptCallback
    {
        private V8ScriptEngine _parent;
#if !DISABLE_CEF
        private Browser _wrapper;
#endif
        public BrowserJavascriptCallback(V8ScriptEngine parent, Browser wrapper)
        {
            _parent = parent;
#if !DISABLE_CEF
            _wrapper = wrapper;
#endif
        }

        public BrowserJavascriptCallback() { }

        public object call(string functionName, params object[] arguments)
        {
#if !DISABLE_CEF
            if (!_wrapper._localMode) return null;

            object objToReturn = null;
            bool hasValue = false;

            lock (JavascriptHook.ThreadJumper)
            JavascriptHook.ThreadJumper.Add(() =>
            {
                try
                {
                    string callString = functionName + "(";

                    if (arguments != null)
                        for (int i = 0; i < arguments.Length; i++)
                        {
                            string comma = ", ";

                            if (i == arguments.Length - 1)
                                comma = "";

                            if (arguments[i] is string)
                            {
                                callString += System.Web.HttpUtility.JavaScriptStringEncode(arguments[i].ToString(), true) + comma;
                            }
                            else
                            {
                                callString += arguments[i] + comma;
                            }
                        }

                    callString += ");";

                    objToReturn = _parent.Evaluate(callString);
                }
                finally
                {
                    hasValue = true;
                }
            });

            while (!hasValue) Thread.Sleep(10);

            return objToReturn;
#else
            return null;
#endif
        }

        public object eval(string code)
        {
#if !DISABLE_CEF
            if (!_wrapper._localMode) return null;

            object objToReturn = null;
            bool hasValue = false;

            lock (JavascriptHook.ThreadJumper)
            JavascriptHook.ThreadJumper.Add(() =>
            {
                try
                {
                    objToReturn = _parent.Evaluate(code);
                }
                finally
                {
                    hasValue = true;
                }
            });

            while (!hasValue) Thread.Sleep(10);

            return objToReturn;
#else
            return null;
#endif
        }

        public void addEventHandler(string eventName, Action<object[]> action)
        {
#if !DISABLE_CEF
            if (!_wrapper._localMode) return;
            _eventHandlers.Add(new Tuple<string, Action<object[]>>(eventName, action));
#endif
        }

        internal void TriggerEvent(string eventName, params object[] arguments)
        {
            foreach (var handler in _eventHandlers)
            {
                if (handler.Item1 == eventName)
                    handler.Item2.Invoke(arguments);
            }
        }

        private List<Tuple<string, Action<object[]>>> _eventHandlers = new List<Tuple<string, Action<object[]>>>();
    }

    public class Browser : IDisposable
    {
#if !DISABLE_CEF
        internal DemoCefClient _client;
        internal CefBrowser _browser;
#endif
        internal readonly bool _localMode;
        internal bool _hasFocused;
        
        public bool Headless = false;

        public Point Position { get; set; }

        public PointF[] Pinned { get; set; }
        
        private Size _size;
        public Size Size
        {
            get { return _size; }
            set
            {
                //_browser.Size = value;
                #if !DISABLE_CEF
                _client.SetSize(value.Width, value.Height);
                #endif
                _size = value;
            }
        }

        private V8ScriptEngine Father;
        /*
        public object eval(string code)
        {
            if (!_localMode) return null;
#if !DISABLE_CEF

            var task = _browser.EvaluateScriptAsync(code);

            task.Wait();

            if (task.Result.Success)
                return task.Result.Result;
            LogManager.LogException(new Exception(task.Result.Message), "CLIENTSIDESCRIPT -> CEF COMMUNICATION");
#endif
            return null;
        }

        public object call(string method, params object[] arguments)
        {
            if (!_localMode) return null;
#if !DISABLE_CEF
            string callString = method + "(";

            for (int i = 0; i < arguments.Length; i++)
            {
                string comma = ", ";

                if (i == arguments.Length - 1)
                    comma = "";

                if (arguments[i] is string)
                {
                    var escaped = System.Web.HttpUtility.JavaScriptStringEncode(arguments[i].ToString(), true);
                    callString += escaped + comma;
                }
                else if (arguments[i] is bool)
                {
                    callString += arguments[i].ToString().ToLower() + comma;
                }
                else
                {
                    callString += arguments[i] + comma;
                }
            }

            callString += ");";

            var task = _browser.EvaluateScriptAsync(callString);

            task.Wait();

            if (task.Result.Success)
                return task.Result.Result;

            LogManager.LogException(new Exception(task.Result.Message), "CLIENTSIDESCRIPT -> CEF COMMUNICATION");
#endif
            return null;
        }
        */
        internal Browser(V8ScriptEngine father, Size browserSize, bool localMode)
        {
            Father = father;
#if !DISABLE_CEF
            CefWindowInfo cefWindowinfo = CefWindowInfo.Create();
            cefWindowinfo.SetAsWindowless(IntPtr.Zero, true);

            var browserSettings = new CefBrowserSettings()
            {
                JavaScriptCloseWindows = CefState.Disabled,
                JavaScriptOpenWindows = CefState.Disabled,
            };

            _client = new DemoCefClient(browserSize.Width, browserSize.Height);
            
            _client.OnCreated += (sender, args) =>
            {
                _browser = (CefBrowser) sender;
                LogManager.SimpleLog("cef", "Browser ready!");
            };

            Size = browserSize;
            _localMode = localMode;
            //ThreadPool.QueueUserWorkItem((WaitCallback) delegate
            //{
                CefBrowserHost.CreateBrowser(cefWindowinfo, _client, browserSettings);
            //});
#endif
        }
        
        internal void GoToPage(string page)
        {
#if !DISABLE_CEF
            if (_browser != null)
            {
                LogManager.SimpleLog("cef", "Trying to load page " + page + "...");
                _browser.GetMainFrame().LoadUrl(page);
            }
#endif
        }

        internal void Close()
        {
#if !DISABLE_CEF
            if (_browser == null) return;
            _browser.GetHost().CloseBrowser(true);
            _browser.Dispose();
#endif
        }

        internal void LoadHtml(string html)
        {
#if !DISABLE_CEF
            if (_browser == null) return;
            _browser.GetMainFrame().LoadString(html, "localhost");
#endif            
        }

        internal string GetAddress()
        {
#if !DISABLE_CEF
            if (_browser == null) return null;
            return _browser.GetMainFrame().Url;
#else
            return null;
#endif
        }

        internal bool IsLoading()
        {
#if !DISABLE_CEF
            return _browser.IsLoading;
#else
            return false;
#endif
        }

        internal bool IsInitialized()
        {
#if !DISABLE_CEF
            return _browser != null;
#else
            return true;
#endif
        }

        internal Bitmap GetRawBitmap()
        {
#if !DISABLE_CEF
            //if (!_browser.IsBrowserInitialized) return null;

            //if (_browser.Size.Width != Size.Width && _browser.Size.Height != Size.Height)
                //_browser.Size = Size;

            //Bitmap output = _browser.ScreenshotOrNull();
            //_browser.InvokeRenderAsync(_browser.BitmapFactory.CreateBitmap(false, 1));
            //return output;
            Bitmap lbmp = _client.GetLastBitmap();

            LogManager.SimpleLog("cef", "Requesting bitmap. Null? " + (lbmp == null));
            return lbmp;
#else
            return null;
#endif
        }

        internal Bitmap GetBitmap()
        {
            var bmp = GetRawBitmap();

            if (bmp == null) return null;
            
            Bitmap doubleBuffer = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(doubleBuffer))
            {
                graphics.DrawImage(bmp, new Point(0, 0));
            }
#if !DISABLE_CEF
            //_browser.InvokeRenderAsync(_browser.BitmapFactory.CreateBitmap(false, 1));
#endif

            return doubleBuffer;
        }

        public void Dispose()
        {
#if !DISABLE_CEF
            _browser = null;
#endif
        }
    }
    //*/
}
#endif