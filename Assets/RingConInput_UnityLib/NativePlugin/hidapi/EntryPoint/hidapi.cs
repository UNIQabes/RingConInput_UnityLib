using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using System;
using System.Text;

public class HIDapi
{

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern int hid_init();

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern int hid_exit();

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern IntPtr hid_error(IntPtr device);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern IntPtr hid_enumerate(ushort vendor_id, ushort product_id);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern void hid_free_enumeration(IntPtr devs);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern int hid_get_feature_report(IntPtr device, byte[] data, UIntPtr length);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern int hid_get_indexed_string(IntPtr device, int string_index, StringBuilder str, UIntPtr maxlen);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern int hid_get_manufacturer_string(IntPtr device, StringBuilder str, UIntPtr maxlen);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern int hid_get_product_string(IntPtr device, StringBuilder str, UIntPtr maxlen);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern int hid_get_serial_number_string(IntPtr device, StringBuilder str, UIntPtr maxlen);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern IntPtr hid_open(ushort vendor_id, ushort product_id, string serial_number);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern void hid_close(IntPtr device);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern IntPtr hid_open_path(string path);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern int hid_read(IntPtr device, byte[] data, ulong length);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern int hid_read_timeout(IntPtr dev, byte[] data, ulong length, int milliseconds);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern int hid_send_feature_report(IntPtr device, byte[] data, uint length);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern int hid_set_nonblocking(IntPtr device, int nonblock);

    [DllImport("libhidapi_mac_arm.dylib")]
    public static extern int hid_write(IntPtr device, byte[] data, uint length);
}

struct hid_device_info
{
    public string path;
    public ushort vendor_id;
    public ushort product_id;
    public IntPtr serial_number;
    public ushort release_number;
    public string manufacturer_string;
    public IntPtr product_string;
    public ushort usage_page;
    public ushort usage;
    public int interface_number;
    public IntPtr next;
}
