﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Extreal.Core.Common.System;
using Extreal.Core.Logging;
using Extreal.Integration.P2P.WebRTC;
using Extreal.SampleApp.Holiday.App.Config;
using Extreal.SampleApp.Holiday.Controls.RetryStatusControl;
using Extreal.SampleApp.Holiday.Screens.ConfirmationScreen;
using UniRx;
using Message = Extreal.SampleApp.Holiday.App.P2P.Message;

namespace Extreal.SampleApp.Holiday.App
{
    [SuppressMessage("Usage", "CC0033")]
    public class AppState : DisposableBase
    {
        private static readonly ELogger Logger = LoggingManager.GetLogger(nameof(AppState));

        private PeerRole role = PeerRole.Host;

        public string PlayerName { get; private set; } = "Guest";
        public AvatarConfig.Avatar Avatar { get; private set; }
        public SpaceConfig.Space Space { get; private set; }
        public bool IsHost => role == PeerRole.Host;
        public bool IsClient => role == PeerRole.Client;
        public string GroupName { get; private set; } // Host only
        public string GroupId { get; private set; } // Client only
        public string SpaceName { get; private set; }

        public IReadOnlyReactiveProperty<bool> PlayingReady => playingReady.AddTo(disposables);
        private readonly ReactiveProperty<bool> playingReady = new ReactiveProperty<bool>(false);

        public IReadOnlyReactiveProperty<bool> SpaceReady => spaceReady.AddTo(disposables);
        private readonly ReactiveProperty<bool> spaceReady = new ReactiveProperty<bool>(false);

        public IReadOnlyReactiveProperty<bool> P2PReady => p2PReady.AddTo(disposables);
        private readonly ReactiveProperty<bool> p2PReady = new ReactiveProperty<bool>(false);

        public IObservable<Message> OnMessageSent => onMessageSent.AddTo(disposables);
        private readonly Subject<Message> onMessageSent = new Subject<Message>();

        public IObservable<Message> OnMessageReceived => onMessageReceived.AddTo(disposables);
        private readonly Subject<Message> onMessageReceived = new Subject<Message>();

        public IObservable<string> OnNotificationReceived => onNotificationReceived.AddTo(disposables);
        private readonly Subject<string> onNotificationReceived = new Subject<string>();

        public IObservable<Confirmation> OnConfirmationReceived => onConfirmationReceived.AddTo(disposables);
        private readonly Subject<Confirmation> onConfirmationReceived = new Subject<Confirmation>();

        public IObservable<RetryStatus> OnRetryStatusReceived => onRetryStatusReceived.AddTo(disposables);
        private readonly Subject<RetryStatus> onRetryStatusReceived = new Subject<RetryStatus>();

        private readonly BoolReactiveProperty multiplayReady = new BoolReactiveProperty(false);
        private readonly BoolReactiveProperty landscapeInitialized = new BoolReactiveProperty(false);

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public StageState StageState { get; private set; }

        public AppState()
        {
            multiplayReady.AddTo(disposables);
            landscapeInitialized.AddTo(disposables);

            MonitorPlayingReadyStatus();
            RestorePlayingReadyStatus();
        }

        [SuppressMessage("Usage", "CC0033")]
        private void MonitorPlayingReadyStatus() =>
            multiplayReady.Merge(spaceReady, p2PReady, landscapeInitialized)
                .Where(_ =>
                {
                    LogWaitingStatus();
                    return multiplayReady.Value && spaceReady.Value && p2PReady.Value && landscapeInitialized.Value;
                })
                .Subscribe(_ =>
                {
                    if (Logger.IsDebug())
                    {
                        Logger.LogDebug("Ready to play");
                    }
                    playingReady.Value = true;
                })
                .AddTo(disposables);

        [SuppressMessage("Usage", "CC0033")]
        private void RestorePlayingReadyStatus() =>
            spaceReady.Merge(landscapeInitialized)
                .Where(_ =>
                {
                    LogWaitingStatus();
                    return !spaceReady.Value && !landscapeInitialized.Value;
                })
                .Subscribe(_ =>
                {
                    if (Logger.IsDebug())
                    {
                        Logger.LogDebug("Stop playing");
                    }
                    playingReady.Value = false;
                })
                .AddTo(disposables);

        private void LogWaitingStatus()
        {
            if (Logger.IsDebug())
            {
                Logger.LogDebug($"Multiplay, Space Ready, P2P Ready, Landscape Initialized: " +
                                $"{multiplayReady.Value}, {spaceReady.Value},  {p2PReady.Value}, {landscapeInitialized.Value}");
            }
        }

        public void SetPlayerName(string playerName) => PlayerName = playerName;
        public void SetAvatar(AvatarConfig.Avatar avatar) => Avatar = avatar;
        public void SetSpace(SpaceConfig.Space space) => Space = space;
        public void SetRole(PeerRole role) => this.role = role;
        public void SetGroupName(string groupName) => GroupName = groupName;
        public void SetGroupId(string groupId) => GroupId = groupId;
        public void SetSpaceName(string spaceName) => SpaceName = spaceName;
        public void SetP2PReady(bool ready) => p2PReady.Value = ready;
        public void SetMultiplayReady(bool ready) => multiplayReady.Value = ready;
        public void SetSpaceReady(bool ready) => spaceReady.Value = ready;
        public void SetLandscapeInitialized(bool initialized) => landscapeInitialized.Value = initialized;
        public void SetStage(StageName stageName) => StageState = new StageState(stageName);

        public void SendMessage(Message message) => onMessageSent.OnNext(message);

        public void ReceivedMessage(Message message) => onMessageReceived.OnNext(message);

        public void Notify(string message)
        {
            Logger.LogError(message);
            onNotificationReceived.OnNext(message);
        }

        public void Confirm(Confirmation confirmation)
        {
            if (Logger.IsDebug())
            {
                Logger.LogDebug($"Confirmation received: {confirmation.Message}");
            }
            onConfirmationReceived.OnNext(confirmation);
        }

        public void Retry(RetryStatus retryStatus)
        {
            if (Logger.IsDebug())
            {
                Logger.LogDebug($"Retry status received: {retryStatus.State} {retryStatus.Message}");
            }
            onRetryStatusReceived.OnNext(retryStatus);
        }

        protected override void ReleaseManagedResources() => disposables.Dispose();
    }
}
