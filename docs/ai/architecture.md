# Architecture

## Implemented：現在の構造

- `LauncherM.sln` / `LauncherM/LauncherM.csproj`：WPF、.NET Framework 4.8。
- `App.xaml`のStartupUriは`01_UI/WorkspaceWindow.xaml`。Workspace選択、中央タイル、右側詳細、編集ダイアログをcode-behindで管理。
- `03_Domain/WorkspaceModels.cs`：WorkspaceDocument → Workspace → LaunchItem。概念を表すための新しいクラス階層は導入していません。
- `02_Application/LaunchService.cs`：個別起動とWorkspace一括起動。ProcessStartInfoの生成、起動したProcessの返却、起動失敗の集約を担当。
- `02_Application/WorkspaceSession.cs`：WorkspaceごとにLauncherMが取得した安全に識別可能なProcessをメモリ上で追跡し、段階的に終了。
- `04_Infrastructure/WorkspaceRepository.cs`：DataContractJsonSerializerで作業ディレクトリの`workspaces.json`を読み書き。Version = 1。
- `BrowserProfiles.cs`：標準Local Stateのprofile.info_cacheから表示名とDirectoryのみを読み、Chrome/Edgeの候補を表示。手入力も可能。FirefoxはProfile名。
- `WebsiteIconCache.cs`：URL faviconの取得・ローカルキャッシュ。IconPathは自動取得と手動指定の双方に使用。
- 旧`0010_MainWindow`、設定画面、de.txt / se.txt関連コードは残存。MUGEN固有処理は旧UI側に留め、汎用Coreへ移しません。

## 中核概念と既存モデルの対応

設計思想の正本は[design-philosophy.md](../design-philosophy.md)です。
Workspaceは作業環境、LaunchItemはその構成要素。Target / Opener / Destinationは独立した役割で、それぞれ必要に応じたContextを持つ概念です。

| 概念 | 現在のフィールド / 処理 | 実装範囲 |
| --- | --- | --- |
| Target | Type、Target | Application / Folder / File / Url / Command。Project専用型なし |
| Opener | Browser、LaunchServiceのType分岐 | URLはDefault / Chrome / Edge / Firefox。他の型は暗黙の実行方法 |
| Opener Context | BrowserProfile | Chrome/EdgeのDirectory ID、FirefoxのProfile名。サービス側Identityではない |
| Opener Context | Arguments、WorkingDirectory | ArgumentsはApplication / Command、WorkingDirectoryはApplicationのみ使用 |
| Destination | 起動方法、BrowserWindowGroupの有無 | 明示指定なしはOS / アプリ任せ。Chrome/EdgeのWorkspace一括起動では新しいBrowser Window |
| Destination Context | BrowserWindowGroup | 同一Workspace内のBrowser・Profile・Group単位。既存Window IDやブラウザの色付きTab Groupではない |
| Target Context | 対応フィールドなし | 期待するサービスアカウント等の概念のみ |
| 表示・識別 | Id、Name、IconPath | 3要素や認証Contextには含めない |

ApplicationはTarget自身を実行、FileはOS関連付け、Folderはexplorer.exe、Commandはcmd.exe /k。
これは現在の実装制約であり、TargetとOpenerが永久に固定される設計ではありません。
一括起動は未指定グループを個別起動し、Chrome/Edgeの明示グループに`--new-window`と複数URLを渡します。
個別起動ではWindow Groupを適用しません。Default / Firefoxで保存されたGroupは起動に使いません。
ブラウザ検出は両Registry ViewのApp Pathsとインストールパスを参照します。

## Contextと実行保証

Target IdentityとOpener Identityを共通Userへ統合しません。ブラウザProfileを選んでもサービスのアカウント選択は保証されません。
Contextは実行パラメータにできるものと期待メタ情報を区別します。現在のモデルにはTarget Accountや汎用Contextの永続化はありません。
LauncherMはPassword / Cookie / Session / OAuth Tokenを扱わず、認証とログイン状態はブラウザ・サービスに委ねます。

## GUI

HeaderはLauncherMタイトルの右側に、高さを揃えたアイコン上・説明文下の設定ボタンと表示切替ボタンを並べます。

詳細ペインと既存編集ダイアログを3セクションで表示。URLだけBrowser選択、明示BrowserだけProfile、Chrome/EdgeだけGroup、Application / Commandだけ引数、Applicationだけ保存済みWorkingDirectoryを表示します。
WorkingDirectoryは読み取り専用。Target Contextや未対応Destinationの入力欄は作りません。
灰色Header、歯車、ダークUI、Workspace選択・すべて起動は維持。Headerの表示切替ボタンで、選択Workspaceの3ペイン表示と全Workspace表示を切り替えます。全Workspace表示では左のWorkspace操作ペインを隠し、中央へWorkspace名、Workspace単位の一括起動・終了ボタン、そのWorkspaceのカードを順番に表示します。Workspace選択UIはタブへ集約し、既存ComboBoxは単一選択状態を保持する非表示コントロールとして使用します。タブ列末尾の＋ボタンからWorkspaceを追加でき、ドラッグ＆ドロップ時はWorkspaceDocumentのリスト順を変更して即保存します。左ペインのWorkspace名は通常はTextBlockで表示し、クリック時だけ同じ位置のTextBoxへ切り替えて編集します。Enterまたは編集欄の外側のクリックで保存し、Escでキャンセルします。中央ペインにはURLタイプを初期選択したリンク追加ボタンがあり、LaunchItemカードもドラッグ＆ドロップでリスト順を変更して即保存します。カード領域はScrollViewerの表示幅に追従し、通常表示・全Workspace表示ともカードを画面幅で折り返します。マウスホイールはカードとアイコンだけを60～180%で拡大縮小し、文字サイズは変更しません。タブとカードのドラッグ中は半透明のスナップショットをAdornerで追従表示し、挿入先の左へ置く場合は対象を右へ、右へ置く場合は対象を左へ退避させて隙間を示します。左Workspace操作・中央LaunchItem一覧・右LaunchItem詳細の3ペインを2本のGridSplitterで区切り、左右幅はユーザー設定へ保存します。一覧と詳細の縦横スクロールバーは各方向で必要な場合だけ表示します。
設定ウィンドウはWindowsのインストール済みフォント、全体文字サイズ倍率、テーマカラーを選択し、ユーザー設定へ保存します。設定内容だけをスクロール領域に置き、適用・OK・キャンセルはウィンドウ下部へ固定表示します。Workspace画面は設定変更イベントを受けてフォント・倍率・背景・前景・アクセントを再適用します。
起動ショートカットを有効にすると、設定した修飾キーと英字／FunctionキーをHotkeyに持つ`LauncherM.lnk`をユーザーのスタートメニュープログラムへ作成します。Windows Shellがショートカットキーを処理するため、LauncherMが終了している状態から起動できます。無効化時はリンクを削除し、有効時はアプリ起動時に現在のexeパスへ更新します。

## 保存互換性と既存制約

今回の整理によるDataMember・Version・起動サービスの変更はありません。Browser / Profile / Group欠落の旧JSONはOS既定起動へフォールバックします。
JSONがない場合のみde.txtを読み、launcher 0..3を4 Workspaceへ、タイトルとパスをApplicationへ移行してJSON保存します。元のde.txt / se.txtは上書きしません。
現行移行は旧icon / memo / visual / PathFileSelectをJSONへ移さず、表示順フィールドでも並べ替えません。バックアップ・atomic write・破損JSONからの復旧は未実装です。

## Workspace Lifecycle

WorkspaceはLaunchItem集合であると同時に、Launch、起動Resourceの追跡、Closeを持つ実行中セッションです。セッションは永続化せずLauncherMプロセス内だけで管理します。ApplicationとCommandで`Process.Start`が返したProcessだけを追跡し、Close時は`CloseMainWindow`、待機、必要時の強制終了の順で処理します。Commandは追跡PIDを起点に`taskkill /PID /T /F`で子プロセスも停止します。

URL、Folder、Fileは、既存ブラウザ・Explorer・OS関連付け先へ合流する可能性があるためClose対象外です。Chrome/EdgeのWindow GroupもCLIによる新規Window要求であり、安全なWindow Identityを取得できないため追跡終了しません。

## Planned / Future（未実装）

Planned：任意Opener選択、Explorer Tab destination、Monitor・座標指定は次期検討対象で、実装順や時期は未確定。
Future：独立Target Contextのメタ情報、Service-specific Target Identity Adapter、Destination Context拡張、仮想デスクトップ、WSL / VM / Remote環境。
いずれも現在の起動保証ではなく、新しいGeneric Frameworkを必要とする前提にしません。
