using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace VRCEventUtil.Controls
{
    /// <summary>
    /// TimeSpanInputBox.xaml の相互作用ロジック
    /// </summary>
    public partial class TimeSpanInputBox : UserControl, INotifyPropertyChanged
    {
        public TimeSpanInputBox()
        {
            InitializeComponent();
        }

        public TimeSpan Time
        {
            get { return (TimeSpan)GetValue(TimeProperty); }
            set {
                SetValue(TimeProperty, value);
                RaisePropertyChanged(nameof(Seconds));
                RaisePropertyChanged(nameof(MilliSeconds));
            }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register(
                nameof(Time),
                typeof(TimeSpan),
                typeof(TimeSpanInputBox),
                new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback((d, args) =>
                    {
                        // 変更通知を出す
                        if (d is TimeSpanInputBox self) {
                            self.RaisePropertyChanged(nameof(Seconds));
                            self.RaisePropertyChanged(nameof(MilliSeconds));
                        }
                    })
                )
            );

        #region 変更通知プロパティ
        public int Seconds
        {
            get => Time.Seconds + 60 * Time.Minutes;
            set {
                if (value != Time.Seconds) {
                    Time = new TimeSpan(0, 0, 0, value, Time.Milliseconds);
                    RaisePropertyChanged(nameof(Seconds));
                }
            }
        }

        public int MilliSeconds
        {
            get => Time.Milliseconds;
            set {
                if (value != Time.Milliseconds) {
                    Time = new TimeSpan(0, 0, 0, Time.Seconds, value);
                    RaisePropertyChanged(nameof(MilliSeconds));
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 変更通知を行います．
        /// </summary>
        /// <param name="propertyName"></param>
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
