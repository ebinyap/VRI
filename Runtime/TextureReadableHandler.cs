using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TextureCropOptimizer
{
    /// <summary>
    /// テクスチャのRead/Writeを一時的に有効化し、Disposeで元に戻すハンドラー。
    /// usingブロックで使用することを前提とする。
    /// </summary>
    public class TextureReadableHandler : IDisposable
    {
        private readonly Texture2D _texture;
        private readonly bool _wasReadable;
#if UNITY_EDITOR
        private readonly string _assetPath;
        private readonly TextureImporter _importer;
#endif

        /// <summary>
        /// コンストラクタでRead/Writeを一時有効化する。
        /// 元々Readableであればなにもしない。
        /// </summary>
        public TextureReadableHandler(Texture2D texture)
        {
            _texture = texture;
            _wasReadable = texture.isReadable;

            if (!_wasReadable)
            {
#if UNITY_EDITOR
                _assetPath = AssetDatabase.GetAssetPath(texture);
                if (!string.IsNullOrEmpty(_assetPath))
                {
                    _importer = AssetImporter.GetAtPath(_assetPath) as TextureImporter;
                    if (_importer != null)
                    {
                        _importer.isReadable = true;
                        _importer.SaveAndReimport();
                    }
                }
#endif
            }
        }

        /// <summary>
        /// Read/Write設定を元に戻す。
        /// </summary>
        public void Dispose()
        {
            if (!_wasReadable)
            {
#if UNITY_EDITOR
                if (_importer != null)
                {
                    _importer.isReadable = false;
                    _importer.SaveAndReimport();
                }
#endif
            }
        }
    }
}
