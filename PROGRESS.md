# PROGRESS.md — 作業進捗

作業再開時は**このファイルを最初に読む**こと。
作業終了・中断時は必ずこのファイルを更新すること。

---

## 現在地

```
ステータス  : 実装中
現在のモジュール : #3 UVIslandDetector（次に着手）
現在のフェーズ   : RED開始前
最終更新    : 2026-02-22
```

---

## モジュール別進捗

| # | モジュール | ステータス | TESTファイル | 実装ファイル | 備考 |
|---|---|---|---|---|---|
| 1 | Constants | 🟢 REFACTOR完了 | Tests/EditMode/ConstantsTests.cs | Runtime/Constants.cs | |
| 2 | TCOLogger | 🟢 REFACTOR完了 | Tests/EditMode/TCOLoggerTests.cs | Runtime/TCOLogger.cs | |
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
1. モジュール#3（UVIslandDetector）のRED-GREEN-REFACTORを開始
2. モジュール#4（UVRectCalculator）のRED-GREEN-REFACTORを開始
```

---

## 既知の問題・懸念事項

（作業中に発見した問題をここに記録する）

---

## 中断ログ

| 日時 | 中断ポイント | 再開に必要な情報 |
|---|---|---|
| 2026-02-22 | Constants・TCOLogger完了後 | #3 UVIslandDetectorからRED開始 |

---

## メモ

（実装上の判断・設計上の迷いなどを記録する）

- Poiyomi・LiltoonのUVチャンネル設定プロパティ名は実装時に実際のシェーダーを参照して確認すること
- `TextureReadableHandler` はusingブロックで使うことを前提とした設計
- NDMFのExecuteOrder属性でAAO・TTTより後になるよう依存宣言を忘れずに追加すること
- プロジェクト構成（asmdef）を作成済み：Runtime / Editor / Tests/EditMode
