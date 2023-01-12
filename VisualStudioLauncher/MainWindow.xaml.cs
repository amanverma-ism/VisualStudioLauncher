using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisualStudioLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Dictionary<string, string> _instanceIDtoPathMap;
        private int _buttonCount;
        private bool _mainStackPanelCursorInside;
        private bool _animation_is_running = false;
        private System.Threading.SynchronizationContext _callersCtx;
        private MouseEventHandler _mouseEventHandler;
        private bool _runAAdminsChecked = true;
        Timer _timer;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool RunAsAdminChecked
        {
            get
            {
                return _runAAdminsChecked;
            }
            set
            {
                _runAAdminsChecked = value;
            }
        }

        public Dictionary<string, string> InstanceIDToPathMap
        {
            get
            {
                return _instanceIDtoPathMap;
            }
            set
            {
                _instanceIDtoPathMap = value;
            }
        }

        public double MainStackPanelWidth
        {
            get
            {
                return Math.Max(0, ActualWidth * 0.8);
            }
        }
        public double MainStackPanelHeight
        {
            get
            {
                return Math.Max(0, ActualHeight - 50);
            }
        }

        public double vsDockPanelWidth
        {
            get
            {
                return Math.Max(0, MainStackPanelWidth - 10);
            }
        }
        public double vsDockPanelHeight
        {
            get
            {
                return Math.Max(0, MainStackPanelHeight * 0.9);
            }
        }

        public double ReloadDockPanelWidth
        {
            get
            {
                return Math.Max(0, MainStackPanelWidth - 10);
            }
        }
        public double ReloadDockPanelHeight
        {
            get
            {
                return Math.Max(0, MainStackPanelHeight * 0.1);
            }
        }

        public double ReloadButtonWidth
        {
            get
            {
                return Math.Max(0, dp1.Width * 0.4);
            }
        }
        public double ReloadButtonHeight
        {
            get
            {
                return Math.Max(0, dp1.Height - 10);
            }
        }

        public double vsButtonWidth
        {
            get
            {
                return Math.Max(0, vsDockPanelWidth - 10);
            }
        }
        public double vsButtonHeight
        {
            get
            {
                return Math.Max(0, (this.vsDockPanelHeight / (_buttonCount)) - ((_buttonCount - 1) * 5));
            }
        }

        public double VSButtonCornerRadius
        {
            get
            {
                if (vsButtonHeight > 0)
                    return vsButtonHeight / 3;
                else
                    return 40;
            }
        }


        private void _VSLauncherButton_MouseMove(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("Leftbutton pressed: " + (Mouse.LeftButton == MouseButtonState.Pressed).ToString());

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (!_animation_is_running)
                {
                    var decreaseOpacityAnim = new DoubleAnimation(0.5, (Duration)TimeSpan.FromSeconds(1));
                    this.BeginAnimation(UIElement.OpacityProperty, decreaseOpacityAnim);
                    _animation_is_running = true;
                }

                this.DragMove();
                Debug.WriteLine("dragging");
            }
            else
            {
                if (_animation_is_running)
                {
                    var increaseOpacityAnim = new DoubleAnimation(1, (Duration)TimeSpan.FromSeconds(1));
                    this.BeginAnimation(UIElement.OpacityProperty, increaseOpacityAnim);
                    _animation_is_running = false;
                }
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            _callersCtx = System.Threading.SynchronizationContext.Current;
            _mainStackPanelCursorInside = false;
            this.WindowStyle = WindowStyle.None;
            this.Topmost = true;
            this.ShowInTaskbar = false;
            this.AllowsTransparency = true;
            this.Background = Brushes.Transparent;
            _mainStackPanel.Visibility = Visibility.Collapsed;
            _mainStackPanelPopup.IsOpen = false;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = System.Windows.SystemParameters.WorkArea.Right * 0.9;
            this.Width = System.Windows.SystemParameters.WorkArea.Width * 0.1;
            this.Height = System.Windows.SystemParameters.WorkArea.Height * 0.1;
            _instanceIDtoPathMap = new Dictionary<string, string>();
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.infinity.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            _VSLauncherButton_ContentControl_Image.Source = bitmapSource;
            double multiplier = bitmapSource.Width / bitmapSource.Height;
            _VSLauncherButton_ContentControl.Width = Math.Max(50, System.Windows.SystemParameters.WorkArea.Width * 0.09);
            _VSLauncherButton_ContentControl.Height = Math.Max(50 - (0.2 * 50), (_VSLauncherButton_ContentControl.Width / multiplier) - (0.2 * (_VSLauncherButton_ContentControl.Width)));

            this.Title = "Visual Studio Launcher";
            this.Loaded += Form1_Load;
            _mainStackPanel.DataContext = this;
            dp2.DataContext = this;
            dp1.DataContext = this;
            _TopStackPanel.DataContext = this;
            _reloadInsInf.DataContext = this;
            _reloadInsInf.Click += _reloadInsInf_Click;
            this.SizeChanged += MainWindow_SizeChanged;
            _mainStackPanel.Background = Brushes.Black;


            _mainStackPanel.MouseLeave += _mainStackPanel_MouseLeave;
            _mainStackPanel.MouseEnter += _mainStackPanel_MouseEnter;


            _VSLauncherButton_ContentControl.MouseEnter += _VSLauncherButton_ContentControl_MouseEnter;
            _VSLauncherButton_ContentControl.MouseLeave += _VSLauncherButton_ContentControl_MouseLeave;
            _VSLauncherButton_ContentControl.PreviewMouseLeftButtonDown += _VSLauncherButton_PreviewMouseLeftButtonDown;
            _VSLauncherButton_ContentControl.PreviewMouseLeftButtonUp += _VSLauncherButton_PreviewMouseLeftButtonUp;
            _VSLauncherButton_ContentControl.MouseLeave += _VSLauncherButton_ContentControl_MouseLeave;
            _VSLauncherButton_ContentControl.PreviewMouseRightButtonUp += _VSLauncherButton_ContentControl_PreviewMouseRightButtonUp;
            _timer = new Timer(1000);
            _timer.Elapsed += (sender1, args) =>
            {
                if (!_mainStackPanelCursorInside)
                {
                    _callersCtx.Post((_) =>
                    {
                        if (!_mainStackPanelCursorInside && !_mainStackPanel.IsMouseDirectlyOver && !_VSLauncherButton_ContentControl.IsMouseDirectlyOver && _mainStackPanel.Visibility == Visibility.Visible)
                        {
                            _mainStackPanel.Visibility = Visibility.Collapsed;
                            _mainStackPanelPopup.IsOpen = false;
                            this.Width = System.Windows.SystemParameters.WorkArea.Width * 0.1;
                            this.Height = System.Windows.SystemParameters.WorkArea.Height * 0.1;
                            Debug.WriteLine("mainstackpanel visibility collapsed: _VSLauncherButton_MouseLeave");
                            RefreshView();
                        }
                    }, null);
                }
            };
            _mouseEventHandler = new MouseEventHandler(_VSLauncherButton_MouseMove);
        }

        private void _VSLauncherButton_ContentControl_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void _mainStackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            _mainStackPanelCursorInside = true;
            Debug.WriteLine("_mainStackPanel_MouseEnter");

        }

        private void _mainStackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            _mainStackPanelCursorInside = false;
            _mainStackPanel.Visibility = Visibility.Collapsed;
            _mainStackPanelPopup.IsOpen = false;
            this.Width = System.Windows.SystemParameters.WorkArea.Width * 0.1;
            this.Height = System.Windows.SystemParameters.WorkArea.Height * 0.1;
            Debug.WriteLine("mainstackpanel visibility collapsed: _mainStackPanel_MouseLeave");
            RefreshView();
        }

        private void _VSLauncherButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (sender as ContentControl).Cursor = Cursors.Arrow;
            Debug.WriteLine("_VSLauncherButton_PreviewMouseLeftButtonUp");
            _VSLauncherButton_ContentControl.MouseMove -= _mouseEventHandler;
            Debug.WriteLine("_VSLauncherButton_ContentControl.MouseMove unsubscribed");

            this.Width = System.Windows.SystemParameters.WorkArea.Width * 0.4;
            this.Height = System.Windows.SystemParameters.WorkArea.Height * 0.4;
            _mainStackPanelPopup.IsOpen = true;
            _mainStackPanel.Visibility = Visibility.Visible;
            RefreshView();
            Debug.WriteLine("mainstackpanel visible: _VSLauncherButton_ContentControl_MouseEnter");

        }

        //private bool EnsureVisible()
        //{
        //    this.po
        //    Rectangle ctrlRect = this.bou; //The dimensions of the ctrl
        //    ctrlRect.Y = ctrl.Top; //Add in the real Top and Left Vals
        //    ctrlRect.X = ctrl.Left;
        //    Rectangle screenRect = Screen.GetWorkingArea(ctrl); //The Working Area fo the screen showing most of the Ctrl

        //    //Now tweak the ctrl's Top and Left until it's fully visible. 
        //    ctrl.Left += Math.Min(0, screenRect.Left + screenRect.Width - ctrl.Left - ctrl.Width);
        //    ctrl.Left -= Math.Min(0, ctrl.Left - screenRect.Left);
        //    ctrl.Top += Math.Min(0, screenRect.Top + screenRect.Height - ctrl.Top - ctrl.Height);
        //    ctrl.Top -= Math.Min(0, ctrl.Top - screenRect.Top);

        //}

        private void _VSLauncherButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mainStackPanel.Visibility = Visibility.Collapsed;
            _mainStackPanelPopup.IsOpen = false;
            this.Width = System.Windows.SystemParameters.WorkArea.Width * 0.1;
            this.Height = System.Windows.SystemParameters.WorkArea.Height * 0.1;
            Debug.WriteLine("mainstackpanel visibility collapsed: _VSLauncherButton_PreviewMouseLeftButtonDown");
            RefreshView();
            (sender as ContentControl).Cursor = Cursors.SizeAll;
            _VSLauncherButton_ContentControl.MouseMove += _mouseEventHandler;
            Debug.WriteLine("_VSLauncherButton_ContentControl.MouseMove subscribed");
        }


        private void _VSLauncherButton_ContentControl_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as ContentControl).Cursor = Cursors.Arrow;
            Debug.WriteLine("_VSLauncherButton_ContentControl_MouseLeave");
            _timer.AutoReset = false;
            _timer.Start();
        }

        private void _VSLauncherButton_ContentControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("_VSLauncherButton_ContentControl_MouseEnter");
            _timer.Stop();
            _VSLauncherButton_MouseMove(sender, null);
            _VSLauncherButton_PreviewMouseLeftButtonUp(sender, null);
        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void RefreshView()
        {
            OnPropertyChanged("MainStackPanelWidth");
            OnPropertyChanged("MainStackPanelHeight");
            OnPropertyChanged("ReloadDockPanelWidth");
            OnPropertyChanged("ReloadDockPanelHeight");
            OnPropertyChanged("ReloadButtonWidth");
            OnPropertyChanged("ReloadButtonHeight");
            OnPropertyChanged("vsDockPanelHeight");
            OnPropertyChanged("vsDockPanelWidth");
            OnPropertyChanged("vsButtonHeight");
            OnPropertyChanged("vsButtonWidth");
            OnPropertyChanged("VSButtonCornerRadius");
            OnPropertyChanged("RunAsChecked");
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshView();
        }

        private void _reloadInsInf_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.VsWhereResult = "";
            foreach (Button bt in _vsInstallationsContainer.Children)
                bt.Click -= VsButton_Click;
            _vsInstallationsContainer.Children.Clear();
            GetVSInstallationDetails();
        }

        /// <summary>
        /// Executes a shell command synchronously.
        /// </summary>
        /// <param name="command">string command</param>
        /// <returns>string, as output of the command.</returns>
        private List<Dictionary<string, object>> ExecuteCommandSync(string command)
        {
            string result;
            if (Properties.Settings.Default.VsWhereResult == "")
            {
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                result = proc.StandardOutput.ReadToEnd();
                Properties.Settings.Default.VsWhereResult = result;
                Properties.Settings.Default.Save();
            }
            else
            {
                result = Properties.Settings.Default.VsWhereResult;
            }
            var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return jsSerializer.Deserialize<List<Dictionary<string, object>>>(result);
        }

        private void GetVSInstallationDetails()
        {
            string cmd = "\"C:\\Program Files (x86)\\Microsoft Visual Studio\\Installer\\vswhere.exe\" -legacy -prerelease -format json";
            List<Dictionary<string, object>> visualStudioInstallations = ExecuteCommandSync(cmd);
            _buttonCount = visualStudioInstallations.Count();
            foreach (var vs in visualStudioInstallations)
            {
                System.Windows.Controls.Button vsButton = null;
                if (vs.ContainsKey("productPath"))
                {
                    vsButton = new System.Windows.Controls.Button();
                    vsButton.Content = (string)vs["displayName"];
                    vsButton.Tag = vs["instanceId"];
                    this.InstanceIDToPathMap[(string)vs["instanceId"]] = (string)vs["productPath"];
                }
                else
                {
                    switch ((string)vs["instanceId"])
                    {
                        case "VisualStudio.14.0":
                            vsButton = new System.Windows.Controls.Button();
                            vsButton.Content = "Visual Studio Professional 2015";
                            vsButton.Tag = vs["instanceId"];
                            this.InstanceIDToPathMap[(string)vs["instanceId"]] = System.IO.Path.Combine((string)vs["installationPath"], "Common7", "IDE", "devenv.exe");
                            break;
                        case "VisualStudio.11.0":
                            vsButton = new System.Windows.Controls.Button();
                            vsButton.Content = "Visual Studio Professional 2012";
                            vsButton.Tag = vs["instanceId"];
                            this.InstanceIDToPathMap[(string)vs["instanceId"]] = System.IO.Path.Combine((string)vs["installationPath"], "Common7", "IDE", "devenv.exe");
                            break;
                        case "VisualStudio.10.0":
                            vsButton = new System.Windows.Controls.Button();
                            vsButton.Content = "Visual Studio Professional 2010";
                            vsButton.Tag = vs["instanceId"];
                            this.InstanceIDToPathMap[(string)vs["instanceId"]] = System.IO.Path.Combine((string)vs["installationPath"], "Common7", "IDE", "devenv.exe");
                            break;

                    }
                }
                if (vsButton != null)
                {
                    vsButton.HorizontalAlignment = HorizontalAlignment.Center;
                    vsButton.Margin = new Thickness(0, 0, 0, 5);
                    vsButton.Click += VsButton_Click;
                    vsButton.DataContext = this;
                    Binding myBinding = new Binding("vsButtonHeight");
                    myBinding.Source = this;
                    myBinding.Mode = BindingMode.OneWay;
                    vsButton.SetBinding(Button.HeightProperty, myBinding);
                    Binding myBinding2 = new Binding("vsButtonWidth");
                    myBinding2.Source = this;
                    myBinding2.Mode = BindingMode.OneWay;
                    vsButton.SetBinding(Button.WidthProperty, myBinding2);
                    _vsInstallationsContainer.Children.Add(vsButton);
                    vsButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    vsButton.Background = Brushes.White;
                    Style style = new Style(typeof(Button));
                    Binding myBinding3 = new Binding("VSButtonCornerRadius");
                    myBinding3.Source = this;
                    myBinding3.Mode = BindingMode.OneWay;
                    Setter setter = new Setter(Border.CornerRadiusProperty, myBinding3);
                    style.Setters.Add(setter);
                    //vsButton.Style = (style);
                    vsButton.Resources.Add(typeof(Border), FindResource("ButtonBorder"));
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetVSInstallationDetails();
        }

        private void VsButton_Click(object sender, EventArgs e)
        {
            System.Windows.Controls.Button vs = sender as System.Windows.Controls.Button;
            if (InstanceIDToPathMap.ContainsKey((string)vs.Tag))
            {
                string path = InstanceIDToPathMap[(string)vs.Tag];
                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(@path);
                if (RunAsAdminChecked)
                {
                    info.UseShellExecute = true;
                    info.Verb = "runas";
                    try
                    {
                        System.Diagnostics.Process.Start(info);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                else
                { 
                    info.UseShellExecute = false;
                    info.Verb = "";
                    System.Diagnostics.Process.Start(info);
                }
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Close_Clicked(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).Name == "Close1")
            {
                this.Close();
            }
        }
    }
}
