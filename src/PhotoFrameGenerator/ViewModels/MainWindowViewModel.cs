using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;

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
        private string _title;

        //--------------------------------------------------
        // メソッド
        //--------------------------------------------------
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            string version = assm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

            // バージョン情報を取得してタイトルに反映する
            Title = $"PhotoFrameGenerator Ver.{version}";
        }
    }
}
