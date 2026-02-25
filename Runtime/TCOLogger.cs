using System;
using System.Text;
using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// TextureCropOptimizer専用のログ出力クラス。
    /// 全ログにプレフィックスを付与し、エラー時はビルドを中断する。
    /// </summary>
    public static class TCOLogger
    {
        /// <summary>
        /// 情報レベルのログを出力する。
        /// </summary>
        /// <param name="category">ログカテゴリ。</param>
        /// <param name="message">メッセージ。</param>
        /// <param name="target">対象アセット名（省略可）。</param>
        public static void Info(string category, string message, string target = null)
        {
            var sb = new StringBuilder();
            sb.Append($"{Constants.LogPrefix} INFO | {category} | {message}");
            if (target != null)
                sb.Append($"\n  Target  : {target}");

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// 警告レベルのログを出力する。
        /// </summary>
        /// <param name="category">ログカテゴリ。</param>
        /// <param name="message">メッセージ。</param>
        /// <param name="target">対象アセット名（省略可）。</param>
        /// <param name="detail">詳細情報（省略可）。</param>
        public static void Warning(string category, string message, string target = null, string detail = null)
        {
            var sb = new StringBuilder();
            sb.Append($"{Constants.LogPrefix} WARNING | {category} | {message}");
            if (detail != null)
                sb.Append($"\n  Detail  : {detail}");
            if (target != null)
                sb.Append($"\n  Target  : {target}");

            Debug.LogWarning(sb.ToString());
        }

        /// <summary>
        /// エラーレベルのログを出力し、例外をスローしてビルドを中断する。
        /// </summary>
        /// <param name="category">ログカテゴリ。</param>
        /// <param name="message">メッセージ。</param>
        /// <param name="target">対象アセット名（省略可）。</param>
        /// <param name="detail">詳細情報（省略可）。</param>
        /// <param name="fix">修正方法のヒント（省略可）。</param>
        public static void Error(string category, string message, string target = null, string detail = null, string fix = null)
        {
            var sb = new StringBuilder();
            sb.Append($"{Constants.LogPrefix} ERROR | {category} | {message}");
            if (detail != null)
                sb.Append($"\n  Detail  : {detail}");
            if (target != null)
                sb.Append($"\n  Target  : {target}");
            sb.Append("\n  Action  : Build Aborted");
            if (fix != null)
                sb.Append($"\n  Fix     : {fix}");

            var errorMessage = sb.ToString();
            Debug.LogError(errorMessage);
            throw new Exception(errorMessage);
        }

        /// <summary>
        /// バイト数を人間が読みやすい形式に変換する。
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            if (bytes >= 1024 * 1024)
                return $"{bytes / (1024f * 1024f):F1} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024f:F1} KB";
            return $"{bytes} B";
        }
    }
}
