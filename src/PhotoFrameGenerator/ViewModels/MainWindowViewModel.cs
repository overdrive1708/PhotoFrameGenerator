using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhotoFrameGenerator.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using static PhotoFrameGenerator.Models.ImageHelper;

namespace PhotoFrameGenerator.ViewModels
{
    internal partial class MainWindowViewModel : ObservableObject
    {
        //--------------------------------------------------
        // バインディングデータ
        //--------------------------------------------------
        /// <summary>
        /// タイトル
        /// </summary>
        [ObservableProperty]
        private string _title = string.Empty;

        /// <summary>
        /// 入力ファイル
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> _inputFiles = [];

        /// <summary>
        /// 表示有無(カメラ名)
        /// </summary>
        [ObservableProperty]
        private bool _isDisplayCameraModel = true;

        /// <summary>
        /// 表示有無(レンズ名)
        /// </summary>
        [ObservableProperty]
        private bool _isDisplayLensModel = true;

        /// <summary>
        /// 表示有無(焦点距離)
        /// </summary>
        [ObservableProperty]
        private bool _isDisplayFocalLength = true;

        /// <summary>
        /// 表示有無(露出モード)
        /// </summary>
        [ObservableProperty]
        private bool _isDisplayExposureMode = true;

        /// <summary>
        /// 表示有無(絞り)
        /// </summary>
        [ObservableProperty]
        private bool _isDisplayAperture = true;

        /// <summary>
        /// 表示有無(シャッタースピード)
        /// </summary>
        [ObservableProperty]
        private bool _isDisplayShutterSpeed = true;

        /// <summary>
        /// 表示有無(露出補正)
        /// </summary>
        [ObservableProperty]
        private bool _isDisplayExposureCompensation = true;

        /// <summary>
        /// 表示有無(ISO感度)
        /// </summary>
        [ObservableProperty]
        private bool _isDisplayISOSensitivity = true;

        /// <summary>
        /// 表示有無(撮影日時)
        /// </summary>
        [ObservableProperty]
        private bool _isDisplayShootingDateAndTime = true;

        /// <summary>
        /// 操作可能フラグ
        /// </summary>
        [ObservableProperty]
        private bool _isOperationEnable = false;

        /// <summary>
        /// 状態メッセージ
        /// </summary>
        [ObservableProperty]
        private string _statusMessage = Resources.Strings.MessageStatusInputFileEmpty;

        /// <summary>
        /// タイトル
        /// </summary>
        [ObservableProperty]
        private string _copyright = string.Empty;

        //--------------------------------------------------
        // バインディングコマンド
        //--------------------------------------------------
        /// <summary>
        /// ドラッグ
        /// </summary>
        [RelayCommand]
        private static void PreviewDragOver(DragEventArgs e) => ExecuteCommandPreviewDragOver(e);

        /// <summary>
        /// ドロップ
        /// </summary>
        [RelayCommand]
        private void Drop(DragEventArgs e) => ExecuteCommandDrop(e);

        /// <summary>
        /// クリア
        /// </summary>
        [RelayCommand]
        private void Clear() => ExecuteCommandClear();

        /// <summary>
        /// 生成
        /// </summary>
        [RelayCommand]
        private void Generate() => ExecuteCommandGenerate();

        /// <summary>
        /// プロジェクトのURLを開く
        /// </summary>
        [RelayCommand]
        private static void OpenProjectUrl() => ExecuteCommandOpenProjectUrl();

        //--------------------------------------------------
        // 内部変数
        //--------------------------------------------------
        /// <summary>
        /// 進捗(最大値)
        /// </summary>
        private int _progressMaximum = 1;

        /// <summary>
        /// 進捗(現在値)
        /// </summary>
        private int _progressValue = 0;

        //--------------------------------------------------
        // メソッド
        //--------------------------------------------------
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            string version = assm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;

            // バージョン情報を取得してタイトルに反映する
            Title = $"PhotoFrameGenerator Ver.{version}";

            // コピーライト情報を取得して設定
            Copyright = assm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;
        }

        /// <summary>
        /// ドラッグコマンド実行処理
        /// </summary>
        /// <param name="e">イベントデータ</param>
        private static void ExecuteCommandPreviewDragOver(DragEventArgs e)
        {
            // ドラッグしてきたデータがファイルの場合､ドロップを許可する｡
            e.Effects = DragDropEffects.Copy;
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }

        /// <summary>
        /// ドロップコマンド実行処理
        /// </summary>
        /// <param name="e">イベントデータ</param>
        private void ExecuteCommandDrop(DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] dropitems)
            {
                foreach (string dropitem in dropitems)
                {
                    if (Directory.Exists(dropitem))
                    {
                        if (Directory.GetFiles(@dropitem, "*", SearchOption.AllDirectories) is string[] files)
                        {
                            foreach (string file in files)
                            {
                                if (ImageHelper.IsJpeg(file))
                                {
                                    InputFiles.Add(file);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ImageHelper.IsJpeg(dropitem))
                        {
                            InputFiles.Add(dropitem);
                        }
                    }
                }
            }

            if(InputFiles.Count != 0)
            {
                IsOperationEnable = true;
                StatusMessage = Resources.Strings.MessageStatusAlreadyGenerate;
            }
            else
            {
                IsOperationEnable = false;
                StatusMessage = Resources.Strings.MessageStatusInputFileEmpty;
            }
        }

        /// <summary>
        /// クリアコマンド実行処理
        /// </summary>
        private void ExecuteCommandClear()
        {
            InputFiles.Clear();
            IsOperationEnable = false;
            StatusMessage = Resources.Strings.MessageStatusInputFileEmpty;
        }

        /// <summary>
        /// 生成コマンド実行処理
        /// </summary>
        private async void ExecuteCommandGenerate()
        {
            FrameDisplaySettings frameDisplaySettings = new()
            {
                IsDisplayCameraModel = IsDisplayCameraModel,
                IsDisplayLensModel = IsDisplayLensModel,
                IsDisplayFocalLength = IsDisplayFocalLength,
                IsDisplayExposureMode = IsDisplayExposureMode,
                IsDisplayAperture = IsDisplayAperture,
                IsDisplayShutterSpeed = IsDisplayShutterSpeed,
                IsDisplayExposureCompensation = IsDisplayExposureCompensation,
                IsDisplayISOSensitivity = IsDisplayISOSensitivity,
                IsDisplayShootingDateAndTime = IsDisplayShootingDateAndTime
            };
            _progressMaximum = InputFiles.Count;
            _progressValue = 0;
            StatusMessage = string.Format(Resources.Strings.MessageStatusNowGenerating, _progressValue, _progressMaximum);

            await Task.Run(() =>
            {
                foreach (string file in InputFiles)
                {
                    ImageHelper.GenerateFramedImage(file, frameDisplaySettings);
                    _progressValue++;
                    StatusMessage = string.Format(Resources.Strings.MessageStatusNowGenerating, _progressValue, _progressMaximum);
                }
            });

            StatusMessage = Resources.Strings.MessageStatusCompleteGenerate;
        }

        /// <summary>
        /// プロジェクトのURLを開くコマンド実行処理
        /// </summary>
        private static void ExecuteCommandOpenProjectUrl()
        {
            ProcessStartInfo psi = new()
            {
                FileName = @"https://github.com/overdrive1708/PhotoFrameGenerator",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
