using TiltBrush;

namespace EternityEngine
{

    /// Properties for various coordinate transforms,
    /// and helper functions for converting between them.
    ///
    /// Names follow the conventions in go/vr-coords.
    ///
    /// There are 3 main coordinate systems that we are interested in:
    /// - Room Space        Real-world tracking space; room floor should be at Y=0
    /// - Scene Space       Virtual environment; origin is the center of the env
    /// - Canvas Space      Brush strokes only; origin is at the floor.
    ///
    /// The global coordinate system is also used as a convenient
    /// space to work in before projecting into some other space.
    /// Currently, Global === Room, although it's best to distinguish
    /// between the two in your code.
    ///
    public static class ArtModuleCoords
    {
        // Unless otherwise noted, transforms are in global coordinates

        /// Deprecated: use PoseChanged on a canvas instance
        public static event ArtCanvas.PoseChangedEventHandler CanvasPoseChanged
        {
            add { ArtModule.Scene.MainCanvas.PoseChanged += value; }
            remove { ArtModule.Scene.MainCanvas.PoseChanged -= value; }
        }

        /// Deprecated. Fix code to remove single-canvas assumptions,
        /// then replace with ArtModule.ActiveCanvas.Pose
        public static TrTransform CanvasPose
        {
            get { return ArtModule.ActiveCanvas.Pose; }
            set { ArtModule.ActiveCanvas.Pose = value; }
        }

        /// Deprecated. Fix code to remove single-canvas assumptions,
        /// then replace with ArtModule.ActiveCanvas.LocalPose
        public static TrTransform CanvasLocalPose
        {
            get { return ArtModule.ActiveCanvas.LocalPose; }
            set { ArtModule.ActiveCanvas.LocalPose = value; }
        }

        /// Helpers for getting and setting transforms on Transform components.
        /// Transform natively allows you to access parent-relative ("local")
        /// and root-relative ("global") views of position, rotation, and scale.
        ///
        /// These helpers:
        ///
        /// - access data as a TrTransform
        /// - provide scene-relative, canvas-relative and room-relative views
        ///
        /// The syntax is a slight abuse of C#:
        ///
        ///   TrTranform xf = Coords.AsRoom[gameobj.transform];
        ///   Coords.AsRoom[gameobj.transform] = xf;
        ///
        public static TransformExtensions.GlobalAccessor AsRoom;
        public static TransformExtensions.GlobalAccessor AsGlobal
            = new TransformExtensions.GlobalAccessor();
        public static TransformExtensions.LocalAccessor AsLocal
            = new TransformExtensions.LocalAccessor();

        /// Deprecated. Replace with canvasInstance.AsCanvas[]
        public static TransformExtensions.RelativeAccessor AsCanvas;

        // Internal

        public static void Init(ArtModule app)
        {
            AsCanvas = new TransformExtensions.RelativeAccessor(app.m_CanvasTransform);
            // Room coordinate system === Unity global coordinate system
            AsRoom = new TransformExtensions.GlobalAccessor();
        }
    }

} // namespace TiltBrush
