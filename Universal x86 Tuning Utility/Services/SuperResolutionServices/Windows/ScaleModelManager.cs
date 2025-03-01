using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows;
using System.Windows.Resources;

namespace Universal_x86_Tuning_Utility.Services.SuperResolutionServices.Windows;
//
// This is a customised version of Magpie from https://github.com/Blinue/Magpie
// I do not take credit for the full functionality of the code.
//

internal class ScaleModelManager
{
    private readonly FileSystemWatcher scaleModelsWatcher = new();
    private const string ScaleModelsPath = @".\ScaleModels.json";

    private ScaleModel[]? scaleModels = null;

    public event Action? ScaleModelsChanged;

    public ScaleModelManager()
    {
        LoadFromLocal();

        // 监视ScaleModels.json的更改
        scaleModelsWatcher.Path = AppDomain.CurrentDomain.BaseDirectory;
        scaleModelsWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
        scaleModelsWatcher.Filter = ScaleModelsPath.Substring(ScaleModelsPath.LastIndexOf('\\') + 1);
        scaleModelsWatcher.Changed += ScaleModelsWatcher_Changed;
        scaleModelsWatcher.Deleted += ScaleModelsWatcher_Changed;
        try
        {
            scaleModelsWatcher.EnableRaisingEvents = true;
        }
        catch (FileNotFoundException e)
        {

        }
    }

    public ScaleModel[]? GetScaleModels()
    {
        return scaleModels;
    }

    public bool IsValid()
    {
        return scaleModels != null && scaleModels.Length > 0;
    }

    private void LoadFromLocal()
    {
        string json = "";
        if (File.Exists(ScaleModelsPath))
        {
            try
            {
                json = File.ReadAllText(ScaleModelsPath);
            }
            catch (Exception e)
            {

            }
        }
        else
        {
            try
            {
                Uri uri = new("pack://application:,,,/Assets/BuiltInScaleModels.json", UriKind.Absolute);
                StreamResourceInfo info = Application.GetResourceStream(uri);
                using (StreamReader reader = new(info.Stream))
                {
                    json = reader.ReadToEnd();
                }
                File.WriteAllText(ScaleModelsPath, json);
            }
            catch (Exception e)
            {
            }
        }

        try
        {
            // 解析缩放配置
            scaleModels = JsonNode.Parse(
                json,
                new JsonNodeOptions { PropertyNameCaseInsensitive = false },
                new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                }
            )?.AsArray().Select(model => {
                if (model == null)
                {
                    throw new Exception("json 非法");
                }

                JsonNode name = model["name"] ?? throw new Exception("未找到 name 字段");
                JsonNode effects = model["effects"] ?? throw new Exception("未找到 effects 字段");

                return new ScaleModel
                {
                    Name = name.GetValue<string>(),
                    Effects = effects.ToJsonString()
                };
            }).ToArray();

            if (scaleModels == null || scaleModels.Length == 0)
            {
                throw new Exception("解析 json 失败");
            }
        }
        catch (Exception e)
        {
            scaleModels = null;
        }

        if (ScaleModelsChanged != null)
        {
            ScaleModelsChanged.Invoke();
        }
    }

    private void ScaleModelsWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        Thread.Sleep(10);
        Application.Current.Dispatcher.Invoke(LoadFromLocal);
    }

    public class ScaleModel
    {
        public string Name { get; set; } = "";

        public string Effects { get; set; } = "";
    }
}