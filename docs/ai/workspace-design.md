# Workspace / LaunchItem Design

## 作業環境と構成要素

Workspaceは「作業環境」の単位で、複数のLaunchItemを持ちます。
LaunchItemはTarget（何を）・Opener（何で）・Destination（どこに）の組合せです。
各要素は独立したContextを持てる設計思想とし、既存の平坦なモデルを維持します。
現在はWorkspace管理・LaunchItem編集・個別起動・すべて起動を実装済みです。

## WebMUGEN Workspaceの例

以下は構成例であり、プリセットの追加や端末上の配置確認を意味しません。

| 要素 | Target | Opener / Context | Destination / Context | 現在の表現 |
| --- | --- | --- | --- | --- |
| VS Code | D:\WebMUGEN\WebMUGEN.code-workspace | VS Code | アプリ任せ | FileではOS関連付け。明示VS Code起動はApplicationでCode.exeとArgumentsにworkspaceパスを指定する回避策。Fileの任意Opener選択は未実装 |
| GitHub | リポジトリURL | Chrome / 個人開発Profile | Browser Window / Main | Url + Browser + BrowserProfile + BrowserWindowGroup |
| ChatGPT | https://chatgpt.com | Chrome / 個人開発Profile | Browser Window / Main | 同上。Target Context「個人Googleアカウント」は思想のみで、保存・強制しない |
| Local Server | プロジェクト内のサーバー起動コマンド | cmd.exe /k | OS任せのコンソール | Command + Target + Arguments。WorkingDirectoryはCommandでは未適用。必要ならコマンド自身で明示的に作業先を設定 |
| Explorer | D:\WebMUGEN | Explorer | OS / Explorer任せ | Folder + Target。Explorer Group / Tab指定は未実装 |

同じWorkspaceでも各LaunchItemのOpener・Context・Destinationは独立します。
Workspaceに共通Userを持たせず、Browser/ProfileのWorkspace既定値継承も現在はありません。

## Browser Windowの現在の動作

一括起動では同一WorkspaceのBrowser・BrowserProfile・BrowserWindowGroupが一致するChrome/Edge URLを、1回の`--new-window`コマンドの複数タブとして渡します。
Group名は前後空白を除いて比較し、大文字小文字を区別。Profileも区別します。Browser名は区別しません。
未指定Group・Default・Firefoxは個別起動。個別起動ボタンではGroupを使わず、ProfileとURLを通常のCLIで渡します。
グループ名は既存ウィンドウの参照でも、ブラウザネイティブの色付きTab Groupでもありません。
実際の配置はブラウザ設定・既存プロセス・ポリシーにも依存し、CLIの送信成功だけで配置確認済みとは扱いません。
グループ単位の失敗は各項目へ報告し、後続の起動を継続します。

## Contextの境界

Browser ProfileはOpener Context。Chrome/Edgeは表示名ではなくDirectory IDを保存し、FirefoxはProfile名を保存します。
ChatGPT Account等のTarget Identityとは別概念です。認証状態はブラウザとサービスに任せ、Password、Cookie、Tokenを保存・操作しません。
Target Contextは必要に応じて期待メタ情報になり得ますが、現在はフィールドも編集UIもありません。
WorkingDirectoryはOpenerの実行Contextで、配置先ではありません。現行実装ではApplicationのみで使用、GUIでは読み取り専用です。
Destination ContextのExplorer Group、Monitor位置、Virtual EnvironmentやLinux Userは未実装の拡張概念です。

## 保存と互換性

WorkspaceDocument Version 1と既存LaunchItemのDataMemberを維持します。概念別オブジェクトへのJSON移行は行いません。
旧JSONのBrowser / BrowserProfile / BrowserWindowGroup欠落を許容し、OS既定起動を維持します。
de.txtからの移行は読み取り専用で元ファイルを保持しますが、移行対象はタイトル・Target・所属のみであり、旧メタ情報の完全移行ではありません。
詳細な現行対応表は[architecture.md](architecture.md)、検証範囲と既知の制約は[current-status.md](current-status.md)を参照してください。

## 未実装の方向

Planned：任意Opener、Explorer Tab、Monitor座標指定の検討。
Future：Target Contextの保存、サービス固有Adapter、仮想デスクトップ、WSL / VM / Remote等。
新しいPlugin SystemやWorkflow Engineを導入せず、必要になった機能から小さく実装します。
