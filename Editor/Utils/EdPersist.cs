using UnityEditor;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Namespaced persistence for editor tool state (foldout expansion, last-used paths...).
    /// All keys are prefixed so tools never collide with other EditorPrefs users.
    /// Prefs variants survive editor restarts; Session variants survive domain reloads only.
    /// </summary>
    public static class EdPersist
    {
        const string Prefix = "UEHM.";

        // EditorPrefs: survives editor restarts (per machine).
        public static bool GetBool(string key, bool defaultValue = false) => EditorPrefs.GetBool(Prefix + key, defaultValue);
        public static void SetBool(string key, bool value) => EditorPrefs.SetBool(Prefix + key, value);

        public static int GetInt(string key, int defaultValue = 0) => EditorPrefs.GetInt(Prefix + key, defaultValue);
        public static void SetInt(string key, int value) => EditorPrefs.SetInt(Prefix + key, value);

        public static float GetFloat(string key, float defaultValue = 0f) => EditorPrefs.GetFloat(Prefix + key, defaultValue);
        public static void SetFloat(string key, float value) => EditorPrefs.SetFloat(Prefix + key, value);

        public static string GetString(string key, string defaultValue = "") => EditorPrefs.GetString(Prefix + key, defaultValue);
        public static void SetString(string key, string value) => EditorPrefs.SetString(Prefix + key, value);

        // SessionState: survives domain reloads, cleared when the editor closes.
        public static bool GetSessionBool(string key, bool defaultValue = false) => SessionState.GetBool(Prefix + key, defaultValue);
        public static void SetSessionBool(string key, bool value) => SessionState.SetBool(Prefix + key, value);

        public static int GetSessionInt(string key, int defaultValue = 0) => SessionState.GetInt(Prefix + key, defaultValue);
        public static void SetSessionInt(string key, int value) => SessionState.SetInt(Prefix + key, value);

        public static string GetSessionString(string key, string defaultValue = "") => SessionState.GetString(Prefix + key, defaultValue);
        public static void SetSessionString(string key, string value) => SessionState.SetString(Prefix + key, value);
    }
}
