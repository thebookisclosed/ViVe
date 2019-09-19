# ViVe
ViVe is a C# library you can use to make your own programs that interact with Windows 10's A/B feature mechanism

In case you'd like to talk to NTDLL exports directly, you can use its *NativeMethods*.

Otherwise, *RtlFeatureManager* offers the same featureset with the benefit of all unmanaged structures being delivered as standard C# classes instead.

# ViVeTool
ViVeTool is both an example of how to use ViVe, as well as a straightforward tool for power users which want to use the new APIs instantly.

# Compatibility
In order to use ViVe, you must be running Windows 10 build 18963 or newer.

![ViVeTool Helpfile](https://i.imgur.com/HrLiSxe.png)