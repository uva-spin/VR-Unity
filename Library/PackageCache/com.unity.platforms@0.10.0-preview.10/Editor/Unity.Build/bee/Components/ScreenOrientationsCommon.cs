using Unity.Properties;

namespace Unity.Build.Common
{
    /// <summary>
    /// Default screen orientation
    /// </summary>
    public enum UIOrientation
    {
        /// <summary>
        /// Portrait
        /// </summary>
        Portrait = 0,

        /// <summary>
        /// Portrait upside down
        /// </summary>
        PortraitUpsideDown = 1,

        /// <summary>
        /// Landscape: clockwise from Portrait
        /// </summary>
        LandscapeRight = 2,

        /// <summary>
        /// Landscape : counter-clockwise from Portrait
        /// </summary>
        LandscapeLeft = 3,

        /// <summary>
        /// Auto Rotation Enabled
        /// </summary>
        AutoRotation = 4
    }

    /// <summary>
    /// Build settings component for Screen orientation
    /// </summary>
    public sealed partial class ScreenOrientations
    {
        [CreateProperty]
        /// <summary>
        /// Default screen orientation
        /// </summary>
        public UIOrientation DefaultOrientation { set; get; } = UIOrientation.AutoRotation;

        [CreateProperty]
        /// <summary>
        /// Allow to rotate to portrait if DefaultOrientation is set to UIOrientation.AutoRotation
        /// </summary>
        public bool AllowAutoRotateToPortrait { set; get; } = true;

        [CreateProperty]
        /// <summary>
        /// Allow to rotate to reverse portrait (upside down) if DefaultOrientation is set to UIOrientation.AutoRotation
        /// </summary>
        public bool AllowAutoRotateToReversePortrait { set; get; } = true;

        [CreateProperty]
        /// <summary>
        /// Allow to rotate to landscape (left) if DefaultOrientation is set to UIOrientation.AutoRotation
        /// </summary>
        public bool AllowAutoRotateToLandscape { set; get; } = true;

        [CreateProperty]
        /// <summary>
        /// Allow to rotate to reverse landscape (right) if DefaultOrientation is set to UIOrientation.AutoRotation
        /// </summary>
        public bool AllowAutoRotateToReverseLandscape { set; get; } = true;
    }
}


