namespace OGE.Editor.Events
{
    /// <summary>
    /// Used to tell every texture viewer document that the global settings for
    /// texture viewers have changed, and they should update. Is an empty class
    /// since all settings data is in SettingsManager.
    /// <remarks>
    ///     Honestly seems like an obtuse way of handling this, but it's code is short and quick to implement.
    /// </remarks>
    /// </summary>
    public class TextureViewerGlobalSettingChangedEventArgs
    {
        public TextureViewerGlobalSettingChangedEventArgs()
        {

        }
    }
}
