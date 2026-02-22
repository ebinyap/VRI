namespace TextureCropOptimizer
{
    /// <summary>
    /// TextureCropOptimizer全体で使用する定数を定義するクラス。
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// GameObjectメニューに表示するパス。
        /// </summary>
        public const string MenuPath = "GameObject/TextureCropOptimizer/TextureCropSettings";

        /// <summary>
        /// ログ出力時のプレフィックス。
        /// </summary>
        public const string LogPrefix = "[TextureCropOptimizer]";

        /// <summary>
        /// tiling のデフォルト値。
        /// </summary>
        public const float DefaultTiling = 1.0f;

        /// <summary>
        /// offset のデフォルト値。
        /// </summary>
        public const float DefaultOffset = 0.0f;

        /// <summary>
        /// テクスチャの最小サイズ（ピクセル）。
        /// </summary>
        public const int MinTextureSize = 4;
    }
}
