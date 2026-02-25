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

        protected override void Configure()
        {
            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("nadena.dev.modular-avatar")
                .AfterPlugin("com.anatawa12.avatar-optimizer")
                .AfterPlugin("net.rs64.tex-trans-tool")
                .Run("TextureCropOptimizer", ctx =>
                {
                    if (ctx.AvatarRootObject == null)
                        return;

                    var settings = ctx.AvatarRootObject.GetComponentInChildren<TextureCropSettings>();
                    if (settings == null)
                    {
                        return;
                    }

                    OptimizationPipeline.Execute(ctx.AvatarRootObject, settings);
                });
        }
    }
}
