# PROGRESS.md — 作業進捗

作業再開時は**このファイルを最初に読む**こと。
作業終了・中断時は必ずこのファイルを更新すること。

---

## 現在地

```
ステータス  : 未着手
現在のモジュール : -
現在のフェーズ   : -（RED / GREEN / REFACTOR）
最終更新    : 初版作成
```

---

## モジュール別進捗

| # | モジュール | ステータス | TESTファイル | 実装ファイル | 備考 |
|---|---|---|---|---|---|
| 1 | Constants | ⬜ 未着手 | - | - | |
| 2 | TCOLogger | ⬜ 未着手 | - | - | |
| 3 | UVIslandDetector | ⬜ 未着手 | - | - | |
| 4 | UVRectCalculator | ⬜ 未着手 | - | - | |
| 5 | PowerOfTwoCalculator | ⬜ 未着手 | - | - | |
| 6 | ShaderPropertyResolver | ⬜ 未着手 | - | - | |
| 7 | TextureReadableHandler | ⬜ 未着手 | - | - | |
| 8 | TextureRebuilder | ⬜ 未着手 | - | - | |
| 9 | MeshRemapper | ⬜ 未着手 | - | - | |
| 10 | MaterialRebuilder | ⬜ 未着手 | - | - | |
| 11 | RendererCollector | ⬜ 未着手 | - | - | |
| 12 | TextureGroupBuilder | ⬜ 未着手 | - | - | |
| 13 | TextureCropSettings | ⬜ 未着手 | - | - | |
| 14 | OptimizationPipeline | ⬜ 未着手 | - | - | |
| 15 | TextureCropSettingsEditor | ⬜ 未着手 | - | - | |
| 16 | TextureCropOptimizerPlugin | ⬜ 未着手 | - | - | |

ステータス凡例：
- ⬜ 未着手
- 🔴 RED（テスト作成中）
- 🟡 GREEN（実装中）
- 🟢 REFACTOR完了（モジュール完成）
- ✅ 結合確認済み

---

## 次のタスク

```
1. プロジェクト構成・asmdefの作成
2. モジュール#1（Constants）からRED-GREEN-REFACTORを開始
```

---

## 既知の問題・懸念事項

（作業中に発見した問題をここに記録する）

---

## 中断ログ

| 日時 | 中断ポイント | 再開に必要な情報 |
|---|---|---|
| - | - | - |

---

## メモ

（実装上の判断・設計上の迷いなどを記録する）

- Poiyomi・LiltoonのUVチャンネル設定プロパティ名は実装時に実際のシェーダーを参照して確認すること
- `TextureReadableHandler` はusingブロックで使うことを前提とした設計
- NDMFのExecuteOrder属性でAAO・TTTより後になるよう依存宣言を忘れずに追加すること
