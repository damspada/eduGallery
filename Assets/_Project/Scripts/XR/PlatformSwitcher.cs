using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace EduGallery
{
    /// <summary>
    /// Turns on the desktop player or the VR player.
    /// "Auto" picks VR when a headset is running, desktop otherwise.
    /// </summary>
    public class PlatformSwitcher : MonoBehaviour
    {
        public enum Mode { Auto, Desktop, VR }

        [SerializeField] private Mode mode = Mode.Auto;
        [SerializeField] private GameObject desktopRig;
        [SerializeField] private GameObject vrRig;

        private void Awake()
        {
            bool useVR = mode == Mode.VR || (mode == Mode.Auto && HeadsetIsRunning());
            if (vrRig == null) useVR = false;
            desktopRig.SetActive(!useVR);
            if (vrRig != null) vrRig.SetActive(useVR);
        }

        private static bool HeadsetIsRunning()
        {
            var displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems(displays);
            foreach (XRDisplaySubsystem display in displays)
                if (display.running) return true;
            return false;
        }
    }
}
