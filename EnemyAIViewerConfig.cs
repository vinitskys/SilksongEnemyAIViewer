using BepInEx.Configuration;
using InUCS.Manager;
using UnityEngine;

namespace EnemyAIViewer;

public sealed class GlobalConfig : BaseSharedConfig
{
    public ConfigEntry<KeyCode> ToggleUIKeybind;

    public GlobalConfig(ConfigFile configFile)
    {
        this.ToggleUIKeybind = configFile.Bind<KeyCode>("Input", "ToggleUIKeybind", KeyCode.BackQuote, "Hotkey to toggle UI display on/off");
    }
}
