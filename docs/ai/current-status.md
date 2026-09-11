# Current Status

Last updated: 2026-09-11

## Implemented

- .NET Framework 4.8 / WPF。起動画面は`01_UI/WorkspaceWindow.xaml`。
- Workspace / LaunchItem / WorkspaceDocumentとVersion 1の`workspaces.json`。
- Workspace作成・名前変更・削除・全Workspace一覧、LaunchItem追加・編集・削除・複数選択・ドラッグ＆ドロップ登録。
- Application / Folder / File / URL / Commandの個別起動とWorkspace一括起動。Commandはcmd.exe /k。
- Target編集、Application / Commandの引数編集。WorkingDirectoryは保存・読み取り専用表示、実行への適用はApplicationのみ。
- URLのBrowser選択（Default / Chrome / Edge / Firefox）、Browser Profile指定。Chrome/Edge候補はLocal Stateの表示名とDirectoryを使用し、Directory IDを保存。FirefoxはProfile名。手入力可。
- Chrome/EdgeのBrowser Window Group。一括起動時にBrowser・Profile・Groupごとの新規ウィンドウCLIを構成。個別起動・未指定Groupは通常起動。Default / FirefoxのGroupは未対応。
- 両Registry ViewのApp Pathsと64/32-bitインストールパスによるブラウザ検出。
- URL favicon取得・LocalApplicationDataへのキャッシュ、手動IconPath、画像表示。favicon.ico取得失敗時はGoogle favicon serviceへフォールバックする既存動作。
- 今回：詳細ペイン・編集ダイアログを「何を開く？ / Target」「何で開く？ / Opener」「どこに開く？ / Destination」に整理。Browser ProfileとWindow Groupを分離し、選択内容による条件表示を実装。
- 今回：灰色Header・歯車・青ブロック・ダークな左右ペイン・Workspace選択・すべて起動を維持。編集ダイアログにも既存の配色を適用しスクロール可能にした。
- 今回：Workspace ComboBoxと同じ選択状態を使う横スクロール可能なWorkspaceタブ。相互切替時に選択表示を同期。
- 今回：Workspaceタブのドラッグ＆ドロップ並び替え。ドロップ先タブの左右で挿入位置を決め、Workspace一覧とComboBoxへ反映して即保存。
- 今回：中央ペインの「リンクを追加」からURLタイプを初期選択して直接入力可能。LaunchItemカードのドラッグ＆ドロップ並び替えと保存に対応。
- 今回：LaunchItem一覧と詳細ペインの縦横スクロールバーをAutoにし、各方向で内容が収まる場合は非表示。
- 今回：WorkspaceタブとLaunchItemカードのドラッグ中に、半透明ゴーストのポインター追従、ドラッグ元の減光と青い発光、挿入先の左右スライド、キャンセル時の復帰アニメーションを表示。
- 今回：タブとカードの挿入先アニメーション方向を修正。左へ挿入すると対象が右へ、右へ挿入すると対象が左へ退避し、挿入位置の隙間が広がる表示に変更。
- 今回：Workspaceタブ列末尾の＋ボタンから既存のWorkspace追加処理を実行。
- 今回：重複していたHeaderの青ブロック・Workspace ComboBox・一括起動・追加・編集・メニューボタンと、左ペインのWorkspace追加・名称変更ボタンを非表示／撤去。選択はタブ、追加は＋、名称変更はインライン編集へ集約。左ペインの削除は維持。
- 今回：設定画面からWindowsインストール済みフォント、全体文字サイズ（80～150%）、テーマカラー（Dark Blue / Green / Purple / Orange / Light）を変更。適用・保存・次回起動時復元に対応。
- 今回：設定内容をスクロール領域、適用・OK・キャンセルを固定下部へ分離し、既定サイズや内容超過時にも操作ボタンを表示。
- 今回：LaunchItem一覧上のCtrl＋マウスホイールでカードとアイコンを60～180%に拡大縮小。通常ホイールは一覧スクロールへ渡し、文字サイズを維持したアニメーションと次回起動時のサイズ復元に対応。
- 今回：通常表示と全Workspace表示のカード領域を表示幅に追従させ、画面幅に応じたカードの折り返しに対応。
- 今回：Headerの表示切替ボタンから選択Workspace表示と全Workspace表示を切替。全Workspace表示では左操作ペインを隠し、中央に全Workspaceを見出し・一括起動／終了ボタン・カードの組で表示。
- 今回：HeaderをLauncherMタイトル、設定、表示切替の順へ整理。機能ボタンは同じ高さに揃え、アイコンの下へ説明文を表示。
- 今回：設定画面でLauncherM起動ショートカットの有効化、修飾キー、英字／F1～F12を設定。ユーザーのスタートメニューへWindowsショートカットを作成し、アプリ終了中からの起動と無効化時の削除に対応。
- 今回：左ペインの青いWorkspace名をクリックしてインライン編集。Enter／フォーカス移動で保存、Escでキャンセルし、タブとComboBoxへ同期。
- 今回：Workspace名の編集中に編集テキストボックス以外をクリックした場合、そのクリック先がフォーカス可能かにかかわらず名称を確定して保存。
- 今回：左Workspace操作、中央LaunchItem一覧、右LaunchItem詳細の3ペイン。2本のGridSplitter、各ペインのMinWidth、左右幅の終了時保存と次回復元。
- 今回：Workspace Lifecycle（Launch / Track / Close）。Application / Commandの起動ProcessをWorkspace ID・LaunchItem ID別にメモリ追跡し、通常終了要求後に残存Processを終了。Commandは追跡PIDのProcess Treeを停止。

## 設計思想のみ（保存・実行機能ではない）

- LaunchItemは独立したTarget / Opener / Destinationで考え、Workspaceはその集合として作業環境を表す。
- 各要素に独立Contextを持てる。Target IdentityとOpener Identityを共通Userへまとめない。
- Target Contextを持てても実行を強制できるとは限らず、期待メタ情報になり得る。現時点でTarget Contextの保存・編集フィールドはない。
- Password / Cookie / Session / OAuth Token管理と自動ログインは製品の対象外。ブラウザ・サービスへ委ねる。

## Planned（未実装・次期検討対象、時期未確定）

- URL以外の任意Opener選択。
- Explorer Tab destination。
- Monitor / 座標指定。

## Future（未実装・将来候補）

- Target Contextメタ情報の保持とService-specific Target Identity Adapter。
- Destination Context拡張、Window位置・サイズ復元、仮想デスクトップ。
- WSL / VM / Remote Environment、Linux User等の実行環境Context。
- Workspace既定Browser/Profile継承、Delay、Ready待ち、Pause / Restart / Session Resume等。

Plugin System、Generic Context Framework、Workflow Engineは導入していません。

## Verified

- 2026-09-11：Workspaceタブ並び替え追加後のDebug Build成功、エラー0。XAMLイベント配線と保存処理をコンパイル確認。
- 2026-09-11：リンク直接追加、カード並び替え、スクロールバーAuto化後のDebug Build成功、エラー0。LaunchItem保存順のJSON往復チェック成功。
- 2026-09-11：タブ／カードのドラッグアニメーション追加後のDebug Build成功、エラー0。Adorner生成・破棄とWPF Animationイベント配線をコンパイル確認。実マウス操作による見た目は未確認。
- 2026-09-11：Workspace名インライン編集追加後のDebug Build成功、エラー0。クリック・Enter・Esc・LostKeyboardFocusのイベント配線をコンパイル確認。
- 2026-09-11：Workspaceタブ末尾の＋ボタン追加後のDebug Build成功、エラー0。既存AddWorkspaceイベントとの配線をコンパイル確認。
- 2026-09-11：重複操作ボタン整理後のDebug Build成功、エラー0。非表示ComboBoxによる単一選択状態とタブ同期のイベント配線をコンパイル確認。
- 2026-09-11：外観設定実装後のDebug Build成功、エラー0。ユーザー設定プロパティ、設定画面イベント、Workspace画面への再適用をコンパイル確認。実画面での全組合せ確認は未実施。
- 2026-09-11：カードのホイール拡大縮小実装後のDebug Build成功、エラー0。サイズ範囲、文字サイズ非変更、CardScale保存処理をコード確認。実ホイール操作は未確認。
- 2026-09-11：全Workspace表示切替実装後のDebug Build成功、エラー0。表示切替、Workspace単位の起動・終了、所属Workspaceへの個別起動追跡のイベント配線をコンパイル確認。実画面操作は未確認。
- 2026-09-11：起動ショートカット設定実装後のDebug Build成功、エラー0。設定保存、Windowsショートカット作成／削除、起動時リンク先更新の配線をコンパイル確認。実ショートカットキーからの起動は未確認。
- 2026-09-11：カード折り返し修正後のDebug Build成功、エラー0。通常表示・全Workspace表示のカード領域と表示幅のBindingをXAMLコンパイルで確認。実画面リサイズ操作は未確認。
- 2026-09-11：タブ／カードの挿入先アニメーション方向修正後のDebug Build成功、エラー0。共通退避処理への適用をコード確認。実ドラッグ操作は未確認。
- 2026-09-11：Workspace名の編集欄外クリック確定実装後のDebug Build成功、エラー0。Window PreviewMouseDownと既存保存処理の配線をコンパイル確認。実クリック操作は未確認。
- 2026-09-11：Headerレイアウト整理後のDebug Build成功、エラー0。タイトル右側の同高ボタン、アイコン・説明文、表示切替ラベル更新をXAMLコンパイルで確認。実画面操作は未確認。
- 2026-09-11：設定画面の下部ボタン固定修正後のDebug Build成功、エラー0。可変スクロール領域と固定ボタン行のGrid配置をXAMLコンパイルで確認。実画面サイズ別の確認は未実施。
- 2026-09-11：カード拡大縮小をCtrl＋ホイールへ変更後のDebug Build成功、エラー0。通常ホイールを未処理のままScrollViewerへ渡す条件分岐をコード確認。実ホイール操作は未確認。

- Visual Studio 2022 Community MSBuild 17.14：変更後のDebug Build成功、エラー0。既存コード由来の警告14件。
- `tests/WorkspaceLifecycleChecks.cs`：7チェック成功。Commandは追跡対象、Browserは除外、追跡Command Treeの停止、同時に起動した無関係Processの生存、終了後のセッション再利用を実Processで確認。
- 既存`LaunchModelChecks.cs` 12チェック、`BrowserWorkspaceChecks.cs` 12チェック成功。LaunchServiceがProcessを返す変更後も既存5種起動、Browser/Profile/Group、保存互換性を維持。
- XAMLのコンパイルにより、Workspaceタブ、3ペイン、2本のGridSplitter、イベント配線、設定プロパティを確認。

- Visual Studio 2022 Community MSBuild 17.14：Debug / Release Build成功、エラー0。既存の未使用フィールドとJSONデシリアライズ用フィールドの警告あり。
- x86 `tests/BrowserWorkspaceChecks.cs`：12チェック成功。Browser / Profile / Group分離、複数URL CLI、新Window CLI、個別Profile、無効メンバー・グループ失敗後の継続、JSON往復、旧JSON省略項目、実環境Chrome/EdgeのProfileメタ情報検出。
- x86 `tests/LaunchModelChecks.cs`：12チェック成功。5種類の起動指示、Applicationの引数・WorkingDirectory、Folder引用符、File / URLのOS関連付け、Commandの既存/kとWorkingDirectory未適用、5種類の新Repositoryでの再読み込み、de.txt移行時の原本保持。
- 起動テストはProcessStartInfoの捕捉であり、外部アプリの実ウィンドウ配置検証ではない。
- Computer Useで専用fixtureを使用してWPFを実起動。灰色Header・ダークな左右ペインを視認。Chrome URLの3セクションとProfile / Main Group表示を確認。
- 編集ダイアログの3セクション・配色を視認。ChromeからDefaultに変更するとProfile / Group欄が非表示になることを確認。
- 編集ダイアログから保存成功。JSONでVersion・Workspace / Item ID・Target・WorkingDirectory・非表示Groupの保持を確認。
- Folder選択時はExplorerと配置指定なしを表示し、Browser Profile / Group / 引数 / WorkingDirectoryを表示しないことを確認。
- 検証用GUIを終了。実ユーザーのworkspaces.jsonは変更していない。

## 過去の検証と今回未確認の範囲

2026-09-09にはDebug / Release BuildとBrowser関連12チェック、Chrome/Edgeへの実起動コマンド送信・追加Window出現、ChatGPT favicon取得・デコード・同一originキャッシュ再利用を確認済み。
ただし正確なA/Bタブ配置・C分離や異なるProfileの実Window Identityは未確認のまま。
今回、外部アプリ全種類の実起動、GUI終了後の再起動による読込、全Browser / TypeのGUI組合せ、cold start、実ユーザーde.txtの完全な移行は検証していません。
今回の専用fixtureでWPFプロセスは起動しましたが、実行環境のComputer Useにネイティブアプリ面が公開されず、タブのクリック同期、Splitterドラッグ、画面サイズ変更、幅保存後の再起動は実画面では未確認です。

## Known Issues / 実装制約

- WorkingDirectoryはApplicationだけに適用。Commandその他の種類で有効と解釈しない。GUIからの編集も未対応。
- Fileの任意エディタ選択、Folderの任意Opener、既存Browser WindowへのTab配置は未実装。
- Window Groupは起動時CLIで、ブラウザの設定やポリシーによる結果まで保証しない。
- Workspace CloseはApplication / Commandだけが対象。Browser / Explorer / Fileは共有プロセスや既存Windowを巻き込む危険があるため閉じない。LauncherM再起動後はセッション追跡を復元しない。
- Applicationが単一インスタンスへ処理を引き渡して起動Processが直ちに終了した場合、その既存インスタンスは閉じない。強制終了時の未保存データ保護は各アプリの通常終了応答に依存する。
- 非標準user-data root、portable版、削除済みProfile等の自動管理は未対応。
- Repositoryはatomic write、バックアップ、破損JSON復旧に未対応。
- de.txt移行は所属・タイトル・パスのみ。旧icon / memo / visual / PathFileSelectは元ファイルに残り、JSONへ完全移行しない。旧Orderによる並べ替えもない。
- 旧コードには環境固有の絶対パスが残存。

## 変更範囲

今回の設計整理は既存平坦モデルを維持し、DataMember・JSON Version・LaunchServiceの挙動を変更していません。
着手時点の未コミットBrowser / Profile / Group / URLアイコン実装を保全し、その上に文書・GUI整理と回帰チェックを追加しています。
