# 実装済みアクションリスト

## 基本移動

| アクション         | 説明     | 速度 |
| ------------------ | -------- | ---- |
| CREEP_RIGHT / LEFT | 忍び足   | 2    |
| WALK_RIGHT / LEFT  | 歩き     | 5    |
| RUN_RIGHT / LEFT   | 走り     | 9    |
| DASH_RIGHT / LEFT  | ダッシュ | 14   |
| STEP_RIGHT / LEFT  | 一歩移動 | 2    |
| STOP               | 停止     | -    |
| WAIT               | 待機     | -    |

## ジャンプ系

| アクション                       | 説明           |
| -------------------------------- | -------------- |
| HOP                              | 小ジャンプ     |
| JUMP                             | 通常ジャンプ   |
| HIGH_JUMP                        | 高ジャンプ     |
| FALL                             | 落下（着地）   |
| JUMP_RIGHT_SHORT / MEDIUM / LONG | 右方向ジャンプ |
| JUMP_LEFT_SHORT / MEDIUM / LONG  | 左方向ジャンプ |

## 高度な機動

| アクション            | 説明                     |
| --------------------- | ------------------------ |
| AIR_DASH_RIGHT / LEFT | 空中ダッシュ             |
| WALL_JUMP             | 壁蹴り                   |
| WALL_SLIDE            | 壁ずり                   |
| STOMP                 | 急降下（ヒップドロップ） |

## 姿勢制御

| アクション         | 説明           |
| ------------------ | -------------- |
| CROUCH             | しゃがみ       |
| CRAWL_RIGHT / LEFT | 匍匐前進       |
| SLIDE_RIGHT / LEFT | スライディング |

## 戦闘・インタラクション

| アクション   | 説明             |
| ------------ | ---------------- |
| ATTACK       | 攻撃             |
| GUARD        | 防御             |
| DODGE_ROLL   | 回避ロール       |
| INTERACT     | 調べる・拾う     |
| BREAK_OBJECT | オブジェクト破壊 |

---

_Last Updated: 2026-01-20_
