using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Livet;
using Livet.Commands;
using System.Linq;
using System.Threading.Tasks;
using VRCEventUtil.Models.Api;

namespace VRCEventUtil.Models
{
    /// <summary>
    /// ワールドインスタンス管理
    /// </summary>
    public class WorldInstanceManager : NotificationObject
    {
        #region 変更通知プロパティ
        /// <summary>
        /// ワールドID
        /// </summary>
        public string WorldId
        {
            get => _worldId;
            set => RaisePropertyChangedIfSet(ref _worldId, value);
        }
        private string _worldId;

        /// <summary>
        /// インスタンスリスト
        /// </summary>
        public ObservableCollection<WorldInstance> Instances
        {
            get => _instances;
            set => RaisePropertyChangedIfSet(ref _instances, value);
        }
        private ObservableCollection<WorldInstance> _instances;


        /// <summary>
        /// インスタンス管理中か
        /// </summary>
        public bool IsManaging
        {
            get => _isManaging;
            set => RaisePropertyChangedIfSet(ref _isManaging, value);
        }
        private bool _isManaging;
        #endregion 変更通知プロパティ

        #region コマンド
        /// <summary>
        /// インスタンス管理を開始します．
        /// </summary>
        public void StartManagement()
        {
            var info = ApiManager.Instance.GetWorldInfo(WorldId);
            if (info is null) { return; }
            Instances = new ObservableCollection<WorldInstance>(info.Instances.Select(instance => new WorldInstance(WorldId, instance.FirstOrDefault().ToString())));
            IsManaging = true;
        }
        private ViewModelCommand _startManagementCommand;
        public ViewModelCommand StartManagementCommand => _startManagementCommand ??= new ViewModelCommand(StartManagement);

        /// <summary>
        /// インスタンス管理を停止します．
        /// </summary>
        public void StopManagement()
        {
            IsManaging = false;
        }
        private ViewModelCommand _stopManagementCommand;
        public ViewModelCommand StopManagementCommand => _stopManagementCommand ??= new ViewModelCommand(StopManagement);
        #endregion コマンド
    }

    /// <summary>
    /// インスタンス
    /// </summary>
    public class WorldInstance : NotificationObject
    {
        #region コンストラクタ
        public WorldInstance(string worldId, string instanceId)
        {
            WorldId = worldId;
            InstanceId = instanceId;
            UpdateInfo();
        }
        #endregion コンストラクタ

        #region 変更通知プロパティ
        /// <summary>
        /// World ID
        /// </summary>
        public string WorldId
        {
            get => _worldId;
            set => RaisePropertyChangedIfSet(ref _worldId, value);
        }
        private string _worldId;

        /// <summary>
        /// Instance ID
        /// </summary>
        public string InstanceId
        {
            get => _instanceId;
            set => RaisePropertyChangedIfSet(ref _instanceId, value);
        }
        private string _instanceId;

        /// <summary>
        /// ユーザー数
        /// </summary>
        public int UserNum
        {
            get => _userNum;
            set => RaisePropertyChangedIfSet(ref _userNum, value);
        }
        private int _userNum;
        #endregion 変更通知プロパティ

        #region メソッド
        public void UpdateInfo()
        {
            var info = ApiManager.Instance.GetWorldInstance(WorldId, InstanceId);
            if (info is null) { return; }
            UserNum = (int)info.NUsers;
        }
        #endregion メソッド
    }
}
