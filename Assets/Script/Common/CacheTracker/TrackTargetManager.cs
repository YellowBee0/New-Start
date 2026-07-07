using System;
using System.Collections.Generic;

namespace YBFramework.Common
{
    public static class TrackTargetManager
    {
        private static readonly Dictionary<int, Actions> s_Actions = new();

        private static readonly Dictionary<(int, int), int> s_CombinedTrackIDs = new();

        private static int s_SourceID;

        private static int s_SourceTrackID;

        public static void RegisterTracker(int trackID, Action onGetControl, Action onLoseControl)
        {
            if (onLoseControl != null || onGetControl != null)
            {
                if (!s_Actions.TryGetValue(trackID, out Actions notify))
                {
                    notify = new Actions();
                    s_Actions.Add(trackID, notify);
                }
                notify.OnGetControl += onGetControl;
                notify.OnLoseControl += onLoseControl;
            }
        }

        public static void UnregisterTracker(int trackID, Action onGetControl, Action onLoseControl)
        {
            if (onLoseControl != null || onGetControl != null)
            {
                if (s_Actions.TryGetValue(trackID, out Actions notify))
                {
                    notify.OnGetControl -= onGetControl;
                    notify.OnLoseControl -= onLoseControl;
                    if (notify.OnGetControl == null && notify.OnLoseControl == null)
                    {
                        s_Actions.Remove(trackID);
                    }
                }
            }
        }

        public static void UnregisterTracker(int trackID, ActionClearModel model)
        {
            if (model == ActionClearModel.ClearAll)
            {
                s_Actions.Remove(trackID);
            }
            if (s_Actions.TryGetValue(trackID, out Actions notify))
            {
                Action otherAction;
                switch (model)
                {
                    case ActionClearModel.ClearGetControl:
                        notify.OnGetControl = null;
                        otherAction = notify.OnLoseControl;
                        break;
                    case ActionClearModel.ClearLoseControl:
                        notify.OnLoseControl = null;
                        otherAction = notify.OnGetControl;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(model), model, null);
                }
                if (otherAction == null)
                {
                    s_Actions.Remove(trackID);
                }
            }
        }

        /// <summary>
        /// 在追踪的目标受到生命周期的控制之后调用
        /// </summary>
        /// <param name="trackID">追踪id</param>
        public static void NotifyGetControl(int trackID)
        {
            if (s_Actions.TryGetValue(trackID, out Actions notify))
            {
                notify.OnGetControl?.Invoke();
            }
        }

        /// <summary>
        /// 在追踪的目标失去生命周期的控制之前调用
        /// </summary>
        /// <param name="trackID">追踪id</param>
        public static void NotifyLoseControl(int trackID)
        {
            if (s_Actions.TryGetValue(trackID, out Actions notify))
            {
                notify.OnLoseControl?.Invoke();
            }
        }

        public static int GenerateSourceID()
        {
            return s_SourceID++;
        }

        /// <summary>
        /// 组合两个id，返回一个追踪id，传入的两个id唯一且顺序不能交换
        /// </summary>
        /// <param name="trackedID">已分配的追踪id</param>
        /// <param name="sourceID">已分配的来源id</param>
        /// <returns>追踪id</returns>
        public static int CombineTrackID(int trackedID, int sourceID)
        {
            if (s_CombinedTrackIDs.TryGetValue((trackedID, sourceID), out int trackID))
            {
                return trackID;
            }
            trackID = s_SourceTrackID++;
            s_CombinedTrackIDs.Add((trackedID, sourceID), trackID);
            return trackID;
        }

        private sealed class Actions
        {
            /// <summary>
            /// 当追踪的目标受到生命周期的控制时触发
            /// </summary>
            public Action OnGetControl;

            /// <summary>
            /// 当追踪的目标失去生命周期的控制时触发
            /// </summary>
            public Action OnLoseControl;
        }
    }
}