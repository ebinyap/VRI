using System;
using System.Collections.Generic;
using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// UV回転モード。
    /// </summary>
    public enum UVRotationMode
    {
        /// <summary>通常処理。</summary>
        Normal,
        /// <summary>Emission固定（UV島の回転・反転を禁止）。</summary>
        EmissionFixed
    }

    /// <summary>
    /// マテリアルごとの設定エントリ。
    /// </summary>
    [Serializable]
    public class MaterialEntry
    {
        /// <summary>対象マテリアル。</summary>
        public Material Material;
        /// <summary>除外フラグ。trueの場合は処理をスキップ。</summary>
        public bool Excluded;
        /// <summary>UV回転モード。</summary>
        public UVRotationMode UVRotation;
    }

    /// <summary>
    /// TextureCropOptimizerの設定コンポーネント。
    /// Avatarルートに配置し、最適化対象のマテリアル設定を保持する。
    /// </summary>
    [AddComponentMenu(Constants.AddComponentMenuPath)]
    [DisallowMultipleComponent]
    public class TextureCropSettings : MonoBehaviour
    {
        /// <summary>マテリアルエントリのリスト。</summary>
        public List<MaterialEntry> Entries = new List<MaterialEntry>();
    }
}
