using Extreal.SampleApp.Holiday.App;
using Extreal.SampleApp.Holiday.App.AssetWorkflow;
using Extreal.SampleApp.Holiday.App.Config;
using UnityEngine;

namespace Extreal.SampleApp.Holiday.Controls.LandscapeControl.LandscapePlayers.Image
{
    public class LandscapeImagePlayerFactory : ILandscapePlayerFactory
    {
        public LandscapeType LandscapeType => LandscapeType.Image;

        private readonly AppState appState;
        private readonly Renderer panoramicRenderer;

        private readonly LandscapeConfig landscapeConfig;
        public LandscapeImagePlayerFactory(AppState appState, Renderer panoramicRenderer, AssetHelper assetHelper)
        {
            this.appState = appState;
            this.panoramicRenderer = panoramicRenderer;
            landscapeConfig = assetHelper.LandscapeConfig;
        }

        public ILandscapePlayer Create(StageName stageName) =>
            new LandscapeImagePlayer(appState, landscapeConfig, panoramicRenderer, $"{stageName}.jpg");
    }
}
