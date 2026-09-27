namespace fefek5.Toys.Runtime.Types
{
    /// <summary>The folder a relative <see cref="FolderPath"/> or <see cref="FilePath"/> starts from.</summary>
    public enum PathRoot
    {
        /// <summary>The folder holding Assets; in a build, the one holding the game's data folder.</summary>
        Project,

        /// <summary><c>Application.persistentDataPath</c>, e.g. for save files.</summary>
        PersistentData,

        /// <summary><c>Application.streamingAssetsPath</c>.</summary>
        StreamingAssets,

        /// <summary>A folder given next to the path, itself absolute or from the project folder.</summary>
        Custom,
    }
}
