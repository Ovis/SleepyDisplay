using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Timers;
using System.Windows.Forms;
using Newtonsoft.Json;
using SleepyDisplay.Model;
using Application = System.Windows.Application;
using Timer = System.Timers.Timer;

namespace SleepyDisplay
{
    /// <summary>
    /// タスクトレイ通知アイコン
    /// </summary>
    public partial class NotifyIconWrapper : Component
    {
        private readonly Timer _timer;

        private readonly List<DisplayTimeSetting> TimerSetting = new List<DisplayTimeSetting>();


        public NotifyIconWrapper()
        {
            InitializeComponent();

            // コンテキストメニューイベントハンドラ
            toolStripMenuItem_DisplaySleep.Click += toolStripMenuItem_DisplaySleep_Click;
            toolStripMenuItem_Exit.Click += toolStripMenuItem_Exit_Click;

            _timer = new Timer
            {
                Interval = 5000
            };

            //設定からディスプレイ電源オフ、オンのタイマー情報を取得
            var json = ConfigurationManager.AppSettings["json"];

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    TimerSetting = JsonConvert.DeserializeObject<List<DisplayTimeSetting>>(json);
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        $"設定の読み取りに失敗しました。{Environment.NewLine}{e.Message}");
                }
            }

            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
        }


        /// <summary>
        /// ディスプレイ電源オフ
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_DisplaySleep_Click(object s, EventArgs e)
        {
            DisplaySleepHelper.SleepDisplay();
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_Exit_Click(object s, EventArgs e)
        {
            if (_timer != null)
            {
                _timer.Enabled = false;
            }
            // 現在のアプリケーションを終了
            Application.Current.Shutdown();
        }

        /// <summary>
        /// タイマー処理
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void timer_Elapsed(object s, ElapsedEventArgs e)
        {
            try
            {
                _timer.Stop();

                //1分以内の設定について処理する
                var span = new TimeSpan(0, 1, 0);

                var dateNow = DateTime.Now;
                var timeOfDay = dateNow.TimeOfDay;

                foreach (var time in TimerSetting)
                {
                    //無効または未来の設定であれば飛ばす
                    if (!time.Enabled || dateNow < time.ExecTime)
                    {
                        continue;
                    }

                    var start = time.ExecTime.TimeOfDay;
                    var end = start.Add(span);

                    if (start <= timeOfDay && timeOfDay <= end)
                    {
                        switch (time.Power)
                        {
                            case PowerEnum.Off:
                                DisplaySleepHelper.SleepDisplay();
                                break;
                            case PowerEnum.On:
                                DisplaySleepHelper.WakeupDisplay();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    //次の日の処理に回す
                    time.ExecTime = time.ExecTime.AddDays(1);
                }
            }
            finally
            {
                _timer.Start();
            }
        }
    }
}
