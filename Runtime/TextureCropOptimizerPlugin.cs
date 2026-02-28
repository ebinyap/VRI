using System;
using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(TextureCropOptimizer.TextureCropOptimizerPlugin))]

namespace TextureCropOptimizer
{
    /// <summary>
    /// NDMFへの登録・フェーズ制御を行うプラグインクラス。
    /// OptimizingPhaseの末尾、AAO・TTTより後に実行される。
    /// </summary>
    public class TextureCropOptimizerPlugin : Plugin<TextureCropOptimizerPlugin>
    {
        /// <summary>
        /// プラグインの表示名。
        /// </summary>
        public override string DisplayName => "TextureCropOptimizer";

        /// <summary>
        /// プラグインのQualifiedName。AfterPluginでの参照に使用される。
        /// </summary>
        public override string QualifiedName => "com.ebinyap.texture-crop-optimizer";

        protected override void Configure()
        {
            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("com.anatawa12.avatar-optimizer")
                .AfterPlugin("net.rs64.tex-trans-tool")
                .AfterPlugin("nadena.dev.modular-avatar")
                .Run("TextureCropOptimizer", ctx =>
                {
                    if (ctx.AvatarRootObject == null)
                    {
                        TCOLogger.Info("Plugin", "AvatarRootObjectがnullです。スキップします");
                        return;
                    }

                    var settings = ctx.AvatarRootObject.GetComponentInChildren<TextureCropSettings>();
                    if (settings == null)
                    {
                        return;
                    }

                    try
                    {
                        OptimizationPipeline.Execute(ctx.AvatarRootObject, settings);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[TextureCropOptimizer] ビルド中にエラーが発生しました: {e.Message}\n{e.StackTrace}");
                    }
                });
        }
    }
}
