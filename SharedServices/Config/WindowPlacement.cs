using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

// ReSharper disable All

namespace BruSoftware.SharedServices.Config
{
    // RECT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }

    // POINT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    // WINDOWPLACEMENT stores the position, size, and state of a window
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }

    /// <summary>
    /// Class for using interop GetWindowPlacement and SetWindowPlacement
    /// see http://www.pinvoke.net/default.aspx/user32.getwindowplacement
    /// and modified using https://blogs.msdn.microsoft.com/davidrickard/2010/03/08/saving-window-size-and-location-in-wpf-and-winforms/
    /// </summary>
    internal static class WindowPlacement
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        public static bool SetPlacement(IntPtr windowHandle, string placementJson)
        {
            if (string.IsNullOrEmpty(placementJson))
            {
                return false;
            }
            var placementToSerialize = JsonConvert.DeserializeObject<WindowPlacementToSerialize>(placementJson, ConfigJson.JsonSerializerSettings);
            var placement = WindowPlacementToSerialize.CreatePlacement(placementToSerialize);
            placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            placement.flags = 0;
            placement.showCmd = (placement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : placement.showCmd);
            var success = SetWindowPlacement(windowHandle, ref placement);
            return success;
        }

        public static string GetPlacement(IntPtr windowHandle)
        {
            var success = GetWindowPlacement(windowHandle, out var placement);
            if (!success)
            {
                return null;
            }
            var placementToSerialize = new WindowPlacementToSerialize(placement);
            var str = JsonConvert.SerializeObject(placementToSerialize, ConfigJson.JsonSerializerSettings);
            return str;
        }

#pragma warning disable IDE1006 // Naming Styles
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
#pragma warning restore IDE1006 // Naming Styles
    }

    public class WindowPlacementToSerialize
    {
        public WindowPlacementToSerialize(WINDOWPLACEMENT placement)
        {
            Length = placement.length;
            Flags = placement.flags;
            ShowCmd = placement.showCmd;
            MinPosition = new Point2(placement.minPosition);
            MaxPosition = new Point2(placement.maxPosition);
            NormalPosition = new Rect2(placement.normalPosition);
        }

        public int Length { get; set; }
        public int Flags { get; set; }
        public int ShowCmd { get; set; }
        public Point2 MinPosition { get; set; }
        public Point2 MaxPosition { get; set; }
        public Rect2 NormalPosition { get; set; }

        public static WINDOWPLACEMENT CreatePlacement(WindowPlacementToSerialize placementToSerialize)
        {
            var result = new WINDOWPLACEMENT
            {
                length = placementToSerialize.Length,
                flags = placementToSerialize.Flags,
                showCmd = placementToSerialize.ShowCmd,
                minPosition = new POINT(placementToSerialize.MinPosition.X, placementToSerialize.MinPosition.Y),
                maxPosition = new POINT(placementToSerialize.MaxPosition.X, placementToSerialize.MaxPosition.Y),
                normalPosition = new RECT(placementToSerialize.NormalPosition.Left, placementToSerialize.NormalPosition.Top,
                    placementToSerialize.NormalPosition.Right, placementToSerialize.NormalPosition.Bottom)
            };
            return result;
        }
    }

    public struct Rect2
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }

        public Rect2(RECT rect)
        {
            this.Left = rect.Left;
            this.Top = rect.Top;
            this.Right = rect.Right;
            this.Bottom = rect.Bottom;
        }
    }

    public struct Point2
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point2(POINT point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }
    }
}